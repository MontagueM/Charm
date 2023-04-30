using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Tiger;

public interface IPackageHeader
{
    public ushort GetPackageId();
    public uint GetTimestamp();
    public ushort GetPatchId();
    public uint GetFileCount();
    public List<D2FileEntry> GetFileEntries(TigerReader reader);
    public List<D2BlockEntry> GetBlockEntries(TigerReader reader);
}

public interface IPackage
{
    PackageMetadata GetPackageMetadata();

    FileMetadata GetFileMetadata(ushort fileId);
    FileMetadata GetFileMetadata(FileHash fileId);
    List<FileMetadata> GetAllFileMetadata();

    byte[] GetFileBytes(FileHash fileId);
    HashSet<int> GetRequiredPatches();
}

public abstract class Package : IPackage
{
    private static string PackageExtension = ".pkg";
    private List<D2FileEntry> FileEntries;
    private List<D2BlockEntry> BlockEntries;
    protected const int BlockSize = 0x40_000;
    public string PackagePath { get; }
    protected TigerReader _reader;
    protected IPackageHeader Header;
    private readonly Dictionary<int, TigerReader> _packageHandles = new();
    protected TigerStrategy PackageStrategy;

    private static readonly byte[] AesKey0 = { 0xD6, 0x2A, 0xB2, 0xC1, 0x0C, 0xC0, 0x1B, 0xC5, 0x35, 0xDB, 0x7B, 0x86, 0x55, 0xC7, 0xDC,
        0x3B };

    private static readonly byte[] AesKey1 = { 0x3A, 0x4A, 0x5D, 0x36, 0x73, 0xA6, 0x60, 0x58, 0x7E, 0x63, 0xE6, 0x76, 0xE4, 0x08, 0x92,
        0xB5 };

    public HashSet<int> GetRequiredPatches()
    {
        HashSet<int> requiredPatches = new HashSet<int>();
        foreach (D2FileEntry fileEntry in FileEntries)
        {
            int blockCount = GetBlockCount(fileEntry);
            List<D2BlockEntry> blocks = GetBlockEntries(fileEntry.StartingBlockIndex, blockCount);
            foreach (D2BlockEntry blockEntry in blocks)
            {
                requiredPatches.Add(blockEntry.PatchId);
            }
        }

        return requiredPatches;
    }

    protected Package(string packagePath, TigerStrategy strategy)
    {
        PackageStrategy = strategy;
        CheckValidPackagePath(packagePath, PackageStrategy);
        PackagePath = SanitisePackagePath(packagePath);
        Initialise();
    }

    private void Initialise()
    {
        GetReader();
        ReadHeader();
        FileEntries = Header.GetFileEntries(_reader);
        BlockEntries = Header.GetBlockEntries(_reader);
        CloseReader();
    }

    private static string SanitisePackagePath(string packagePath) { return packagePath.Replace("\\", "/"); }

    protected abstract void ReadHeader();

    public List<T> GetAllTags<T>() where T : TigerFile
    {
        List<T> tags = new();

        SchemaStructAttribute attribute = GetAttribute<SchemaStructAttribute>(typeof(T).BaseType.GenericTypeArguments[0]);
        TigerHash referenceHash = new(attribute.ClassHash);
        // T tag = FileResourcer.Get().GetTag<T>(fileHash);

        return tags;
    }

    private T? GetAttribute<T>(ICustomAttributeProvider var) where T : StrategyAttribute
    {
        T[] attributes = var.GetCustomAttributes(typeof(T), false).Cast<T>().ToArray();
        if (!attributes.Any())
        {
            // we still want to be able to get size of non-schema types
            return null;
        }
        if (attributes.Length == 1)
        {
            return attributes.First();
        }
        T? attribute = (T?)var.GetCustomAttributes(typeof(T), false).FirstOrDefault(a => ((T)a).Strategy == PackageStrategy);
        if (attribute == null)
        {
            throw new Exception($"Failed to get schema struct size for type {var} as it has multiple schema struct attributes but none match the current strategy {PackageStrategy}");
        }
        return attribute;
    }

    public static void CheckValidPackagePath(string packagePath, TigerStrategy strategy)
    {
        CheckPackagePathExists(packagePath);
        CheckPackagePathValidPrefix(packagePath, strategy);
        CheckPackagePathValidExtension(packagePath);
    }

    private static void CheckPackagePathExists(string packagePath)
    {
        if (!File.Exists(packagePath))
        {
            throw new FileNotFoundException($"The package path '{packagePath}' does not exist");
        }
    }

    private static void CheckPackagePathValidPrefix(string packagePath, TigerStrategy strategy)
    {
        string prefix = Strategy.GetStrategyPackagePrefix(strategy);
        if (!packagePath.Contains(prefix + "_"))
        {
            throw new ArgumentException($"The package path '{packagePath}' does not have the correct prefix '{prefix}'");
        }
    }

    private static void CheckPackagePathValidExtension(string packagePath)
    {
        if (!packagePath.EndsWith(PackageExtension))
        {
            throw new ArgumentException($"The package path '{packagePath}' does not have the extension {PackageExtension}.");
        }
    }

    private void GetReader() { _reader = new TigerReader(File.Open(PackagePath, FileMode.Open, FileAccess.Read, FileShare.Read)); }

    private void CloseReader() { _reader.Close(); }

    /// <summary>
    /// Find what blocks the file is made out of. For most small files this is a single block since blocks are
    /// 262144 bytes long, but larger files will span multiple blocks.
    /// ( = block start, [ = file start, ] = file end, ) = block end, - = data
    /// If a file is made of a single block, we just need to identify which patch file the block is located in and copy from the given block
    /// offset -> offset + fileSize:
    /// (----[--]--)
    ///
    /// If a file is made of multiple blocks, it often looks like this:
    /// (----[-----) (----------) (---]------)
    /// The first block is be copied from offset -> end of block.
    /// The middling blocks are copied entirely.
    /// The final block is copied from start -> fileSize - bytes already copied
    /// </summary>
    public byte[] GetFileBytes(FileHash fileId)
    {
        D2FileEntry fileEntry = FileEntries[fileId.FileIndex];
        byte[] finalFileBuffer = new byte[fileEntry.FileSize];
        int blockCount = GetBlockCount(fileEntry);
        int currentBufferOffset = 0;
        int currentBlockId = 0;

        List<D2BlockEntry> blocks = GetBlockEntries(fileEntry.StartingBlockIndex, blockCount);
        foreach (D2BlockEntry blockEntry in blocks)
        {
            TigerReader packageHandle = GetPackageHandle(blockEntry.PatchId);

            byte[] blockBuffer = ReadBlockBuffer(packageHandle, blockEntry);
            blockBuffer = DecryptAndDecompressBlockBufferIfRequired(blockBuffer, blockEntry);

            bool isFirstBlock = currentBlockId == 0;
            bool isLastBlock = currentBlockId == blockCount - 1;
            bool isOnlyOneBlock = blockCount == 1;
            if (isOnlyOneBlock)
            {
                int copySize = fileEntry.FileSize;
                Array.Copy(blockBuffer, fileEntry.StartingBlockOffset, finalFileBuffer, 0, copySize);
            }
            else if (isFirstBlock)
            {
                int copySize = BlockSize - fileEntry.StartingBlockOffset;
                Array.Copy(blockBuffer, fileEntry.StartingBlockOffset, finalFileBuffer, 0, copySize);
                currentBufferOffset += copySize;
            }
            else if (isLastBlock)
            {
                int copySize = fileEntry.FileSize - currentBufferOffset;
                Array.Copy(blockBuffer, 0, finalFileBuffer, currentBufferOffset, copySize);
            }
            else
            {
                const int copySize = BlockSize;
                Array.Copy(blockBuffer, 0, finalFileBuffer, currentBufferOffset, copySize);
                currentBufferOffset += BlockSize;
            }

            currentBlockId++;
        }

        return finalFileBuffer;
    }

    public PackageMetadata GetPackageMetadata()
    {
        PackageMetadata packageMetadata = new PackageMetadata();
        packageMetadata.Path = PackagePath;
        packageMetadata.Name = PackagePath.Split('/').Last();
        packageMetadata.Id = Header.GetPackageId();
        packageMetadata.PatchId = Header.GetPatchId();
        packageMetadata.Timestamp = Header.GetTimestamp();
        packageMetadata.FileCount = Header.GetFileCount();
        return packageMetadata;
    }

    public FileMetadata GetFileMetadata(FileHash fileHash)
    {
        if (fileHash.PackageId != Header.GetPackageId())
        {
            throw new ArgumentException($"The provided file hash has an invalid package id: {fileHash.PackageId}");
        }
        return GetFileMetadata(fileHash.FileIndex);
    }

    public FileMetadata GetFileMetadata(ushort fileIndex)
    {
        if (fileIndex >= FileEntries.Count)
        {
            throw new ArgumentOutOfRangeException($"The provided file hash has an out-of-range file index {fileIndex} >= {FileEntries.Count}");
        }
        return new FileMetadata(new FileHash(Header.GetPackageId(), fileIndex), FileEntries[fileIndex]);
    }



    private List<D2BlockEntry> GetBlockEntries(int blockIndex, int blockCount) { return BlockEntries.GetRange(blockIndex, blockCount); }

    private int GetBlockCount(D2FileEntry fileEntry)
    {
        return 1 + (int)Math.Floor((double)(fileEntry.StartingBlockOffset + fileEntry.FileSize - 1) / BlockSize);
    }

    private TigerReader GetPackageHandle(ushort patchId)
    {
        if (!_packageHandles.TryGetValue(patchId, out TigerReader packageHandle))
        {
            packageHandle =
                new TigerReader(new FileStream(GetSpecificPackagePatchPath(patchId), FileMode.Open, FileAccess.Read, FileShare.Read));
            _packageHandles.Add(patchId, packageHandle);
        }
        return packageHandle;
    }

    // This only supports patchIds that are from 0-9.
    private string GetSpecificPackagePatchPath(ushort patchId)
    {
        string packagePatchAndExtension = "0.pkg";
        string pathWithNoPatchAndExtension = PackagePath.Substring(0, PackagePath.Length - packagePatchAndExtension.Length);

        return Path.Combine(pathWithNoPatchAndExtension + patchId.ToString("D") + ".pkg");
    }

    private byte[] ReadBlockBuffer(TigerReader packageHandle, D2BlockEntry blockEntry)
    {
        packageHandle.Seek(blockEntry.Offset, SeekOrigin.Begin);
        byte[] blockBuffer = packageHandle.ReadBytes((int)blockEntry.Size);
        return blockBuffer;
    }

    private byte[] DecryptAndDecompressBlockBufferIfRequired(byte[] blockBuffer, D2BlockEntry blockEntry)
    {
        byte[] decryptedBuffer;
        if ((blockEntry.BitFlag & 0x2) == 2)
        {
            decryptedBuffer = DecryptBuffer(blockBuffer, blockEntry);
        }
        else
        {
            decryptedBuffer = blockBuffer;
        }

        byte[] decompressedBuffer;
        if ((blockEntry.BitFlag & 0x1) == 1)
        {
            decompressedBuffer = DecompressBuffer(decryptedBuffer, blockEntry);
        }
        else
        {
            decompressedBuffer = decryptedBuffer;
        }

        return decompressedBuffer;
    }

    private unsafe byte[] DecryptBuffer(byte[] buffer, D2BlockEntry block)
    {
        byte[] decryptedBuffer = new byte[buffer.Length];
        byte[] key;
        if ((block.BitFlag & 0x4) == 4)
        {
            key = AesKey1;
        }
        else
        {
            key = AesKey0;
        }

        byte[] iv = GenerateNonce();
        using var aes = new AesGcm(key);
        byte[] tag = new byte[0x10];
        Marshal.Copy((IntPtr)block.GCMTag, tag, 0, 0x10);
        aes.Decrypt(iv, buffer, tag, decryptedBuffer);
        return decryptedBuffer;
    }

    protected abstract byte[] GenerateNonce();

    private byte[] DecompressBuffer(byte[] buffer, D2BlockEntry block)
    {
        return OodleDecompress(buffer, (int)block.Size);
    }

    protected abstract byte[] OodleDecompress(byte[] buffer, int blockSize);

    public List<FileMetadata> GetAllFileMetadata()
    {
        List<FileMetadata> fileMetadataList = new List<FileMetadata>();
        for (ushort fileIndex = 0; fileIndex < FileEntries.Count; fileIndex++)
        {
            fileMetadataList.Add(GetFileMetadata(fileIndex));
        }
        return fileMetadataList;
    }
}

public struct PackageMetadata
{
    public string Path;
    public string Name;
    public ushort Id;
    public ushort PatchId;
    public uint Timestamp;
    public uint FileCount;
}

public struct FileMetadata
{
    public ushort FileIndex;
    public FileHash Hash;
    public TigerHash Reference;
    public int Size;
    public sbyte Type;
    public sbyte SubType;

    public FileMetadata(FileHash fileHash, D2FileEntry fileEntry)
    {
        FileIndex = fileHash.FileIndex;
        Hash = fileHash;
        Reference = fileEntry.Reference;
        Size = fileEntry.FileSize;
        Type = fileEntry.NumType;
        SubType = fileEntry.NumSubType;
    }
}

public struct D2FileEntry
{
    public TigerHash Reference;
    public sbyte NumType;
    public sbyte NumSubType;
    public int StartingBlockIndex;
    public int StartingBlockOffset;
    public int FileSize;

    public D2FileEntry(D2FileEntryBitpacked entryBitpacked)
    {
        // EntryA
        Reference = new TigerHash(entryBitpacked.Reference);

        // EntryB
        NumType = (sbyte)((entryBitpacked.EntryB >> 9) & 0x7F);
        NumSubType = (sbyte)((entryBitpacked.EntryB >> 6) & 0x7);

        // EntryC
        StartingBlockIndex = (int)(entryBitpacked.EntryC & 0x3FFF);
        StartingBlockOffset = (int)(((entryBitpacked.EntryC >> 14) & 0x3FFF) << 4);

        // EntryD
        FileSize = (int)((entryBitpacked.EntryD & 0x3FFFFFF) << 4 | (entryBitpacked.EntryC >> 28) & 0xF);
    }
};

[StructLayout(LayoutKind.Sequential)]
public struct D2FileEntryBitpacked
{
    public uint Reference;
    public uint EntryB;
    public uint EntryC;
    public uint EntryD;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct D2BlockEntry
{
    public uint Offset;
    public uint Size;
    public ushort PatchId;
    public ushort BitFlag;
    public fixed byte SHA1[0x14];
    public fixed byte GCMTag[0x10];
};
