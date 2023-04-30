using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Tiger.DESTINY2_SHADOWKEEP_2601;

// todo change PackageHeader into a class and inherit from IPackageHeader, and instead add GetFileEntries() and GetBlockEntries() to IPackage

public interface IPackageHeader
{
    public ushort GetPackageId();
    public uint GetTimestamp();
    public ushort GetPatchId();
    public uint GetFileCount();
    public List<D2FileEntry> GetFileEntries(TigerReader reader);
    public List<D2BlockEntry> GetBlockEntries(TigerReader reader);
}

[StructLayout(LayoutKind.Explicit)]
public struct PackageHeaderOld : IPackageHeader
{
    [FieldOffset(0x04)]
    public ushort PackageId;
    [FieldOffset(0x10)]
    public uint Timestamp;
    [FieldOffset(0x20)]
    public ushort PatchId;
    [FieldOffset(0xB4)]
    public uint FileEntryTableCount;
    [FieldOffset(0xB8)]
    public uint FileEntryTableOffset;
    [FieldOffset(0xD0)]
    public uint BlockEntryTableCount;
    [FieldOffset(0xD4)]
    public uint BlockEntryTableOffset;

    public ushort GetPackageId()
    {
        return PackageId;
    }

    public uint GetTimestamp()
    {
        return Timestamp;
    }

    public ushort GetPatchId()
    {
        return PatchId;
    }

    public uint GetFileCount()
    {
        return FileEntryTableCount;
    }

    public List<D2FileEntry> GetFileEntries(TigerReader reader)
    {
        reader.Seek(FileEntryTableOffset, SeekOrigin.Begin);

        List<D2FileEntry> fileEntries = new();
        int d2FileEntrySize = Marshal.SizeOf<D2FileEntryBitpacked>();
        for (int i = 0; i < FileEntryTableCount; i++)
        {
            D2FileEntryBitpacked fileEntryBitpacked = reader.ReadBytes(d2FileEntrySize).ToType<D2FileEntryBitpacked>();
            fileEntries.Add(new D2FileEntry(fileEntryBitpacked));
        }

        return fileEntries;
    }

    public List<D2BlockEntry> GetBlockEntries(TigerReader reader)
    {
        reader.Seek(BlockEntryTableOffset, SeekOrigin.Begin);

        List<D2BlockEntry> blockEntries = new();
        int d2BlockEntrySize = Marshal.SizeOf<D2BlockEntry>();
        for (int i = 0; i < BlockEntryTableCount; i++)
        {
            D2BlockEntry blockEntry = reader.ReadBytes(d2BlockEntrySize).ToType<D2BlockEntry>();
            blockEntries.Add(blockEntry);
        }

        return blockEntries;
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct PackageHeaderNew : IPackageHeader
{
    [FieldOffset(0x04)]
    public ushort PackageId;
    [FieldOffset(0x10)]
    public uint Timestamp;
    [FieldOffset(0x20)]
    public ushort PatchId;
    [FieldOffset(0xF0)] 
    public GlobalPointer<ActivityTableData> ActivityTableData;
    [FieldOffset(0xF8)]
    public uint ActivityTableDataSize;
    [FieldOffset(0x110)]
    public GlobalPointer<PackageTablesData> PackageTablesData;
    [FieldOffset(0x118)]
    public uint PackageTablesDataSize;
    
    public ushort GetPackageId()
    {
        return PackageId;
    }

    public uint GetTimestamp()
    {
        return Timestamp;
    }

    public ushort GetPatchId()
    {
        return PatchId;
    }

    public uint GetFileCount()
    {
        return (uint) PackageTablesData.Value.FileEntries.Count;
    }

    public List<D2FileEntry> GetFileEntries(TigerReader reader)
    {
        List<D2FileEntry> fileEntries = new();
        foreach (D2FileEntryBitpacked fileEntryBitpacked in PackageTablesData.Value.FileEntries.Enumerate(reader))
        {
            fileEntries.Add(new D2FileEntry(fileEntryBitpacked));
        }

        return fileEntries;
    }

    public List<D2BlockEntry> GetBlockEntries(TigerReader reader)
    {
        List<D2BlockEntry> blockEntries = new();
        foreach (D2BlockEntry blockEntry in PackageTablesData.Value.BlockEntries.Enumerate(reader))
        {
            blockEntries.Add(blockEntry);
        }

        return blockEntries;
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct ActivityTableData
{
    [FieldOffset(0x00)]
    public long ThisSize;
    [FieldOffset(0x10)]
    public uint ActivityTableCount;
    [FieldOffset(0x18)]
    public uint ActivityTableOffset;
}

[StructLayout(LayoutKind.Explicit)]
public struct PackageTablesData
{
    [FieldOffset(0x00)]
    public long ThisSize;
    [FieldOffset(0x10)]
    public DynamicArray<D2FileEntryBitpacked> FileEntries;
    [FieldOffset(0x20)]
    public DynamicArray<D2BlockEntry> BlockEntries;
}


[StrategyClass(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
public class Package : IPackage
{
    private const int BlockSize = 0x40_000;
    public string PackagePath { get; }
    private TigerReader _reader;
    private IPackageHeader Header;
    private List<D2FileEntry> FileEntries;
    private List<D2BlockEntry> BlockEntries;
    private Dictionary<int, TigerReader> _packageHandles = new();

    private static readonly byte[] AesKey0 = { 0xD6, 0x2A, 0xB2, 0xC1, 0x0C, 0xC0, 0x1B, 0xC5, 0x35, 0xDB, 0x7B, 0x86, 0x55, 0xC7, 0xDC,
        0x3B };

    private static readonly byte[] AesKey1 = { 0x3A, 0x4A, 0x5D, 0x36, 0x73, 0xA6, 0x60, 0x58, 0x7E, 0x63, 0xE6, 0x76, 0xE4, 0x08, 0x92,
        0xB5 };

    [DllImport("oo2core_3_win64.dll", EntryPoint = "OodleLZ_Decompress")]
    public static extern bool OodleLZ_Decompress(byte[] buffer, int bufferSize, byte[] outputBuffer, int outputBufferSize, int a, int b,
        int c, IntPtr d, IntPtr e, IntPtr f, IntPtr g, IntPtr h, IntPtr i, int threadModule);

    public Package(string packagePath)
    {
        IPackage.CheckValidPackagePath(packagePath);
        PackagePath = SanitisePackagePath(packagePath);
        GetReader();
        ReadHeader();
        FileEntries = Header.GetFileEntries(_reader);
        BlockEntries = Header.GetBlockEntries(_reader);
        CloseReader();
    }

    private static string SanitisePackagePath(string packagePath) { return packagePath.Replace("\\", "/"); }

    private void GetReader() { _reader = new TigerReader(File.Open(PackagePath, FileMode.Open, FileAccess.Read, FileShare.Read)); }

    private void CloseReader() { _reader.Close(); }

    private void ReadHeader()
    {
        _reader.Seek(0x8, SeekOrigin.Begin);
        ulong buildId = _reader.ReadUInt64();
        bool isNewHeader = buildId >= 17011569960331205102;
        _reader.Seek(0, SeekOrigin.Begin);
        // Header = _reader.ReadBytes(0x120).ToType<PackageHeader>();
        if (isNewHeader)
        {
            Header = SchemaDeserializer.Get().DeserializeSchema<PackageHeaderNew>(_reader);
        }
        else
        {
            Header = SchemaDeserializer.Get().DeserializeSchema<PackageHeaderOld>(_reader);
        }
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

    private static readonly string FileMetadataInvalidPackageIdMessage = "The provided file hash has an invalid package id: ";
    public FileMetadata GetFileMetadata(FileHash fileHash)
    {
        if (fileHash.PackageId != Header.GetPackageId())
        {
            throw new ArgumentException(FileMetadataInvalidPackageIdMessage + fileHash.PackageId);
        }
        return GetFileMetadata(fileHash.FileIndex);
    }

    private static readonly string FileMetadataFileIndexOutOfRangeMessage = "The provided file hash has an out-of-range file index: ";
    public FileMetadata GetFileMetadata(ushort fileIndex)
    {
        if (fileIndex >= FileEntries.Count)
        {
            throw new ArgumentOutOfRangeException(FileMetadataFileIndexOutOfRangeMessage + $"{fileIndex} >= {FileEntries.Count}");
        }
        return new FileMetadata(new FileHash(Header.GetPackageId(), fileIndex), FileEntries[fileIndex]);
    }
    
    public List<FileMetadata> GetAllFileMetadata()
    {
        List<FileMetadata> fileMetadataList = new List<FileMetadata>();
        for (ushort fileIndex = 0; fileIndex < FileEntries.Count; fileIndex++)
        {
            fileMetadataList.Add(GetFileMetadata(fileIndex));
        }
        return fileMetadataList;
    }

    private List<D2BlockEntry> GetBlockEntries(int blockIndex, int blockCount) { return BlockEntries.GetRange(blockIndex, blockCount); }

    private D2BlockEntry GetBlockEntry(int blockIndex) { return BlockEntries[blockIndex]; }

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

    private int GetBlockCount(D2FileEntry fileEntry)
    {
        return 1 + (int) Math.Floor((double) (fileEntry.StartingBlockOffset + fileEntry.FileSize - 1) / BlockSize);
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
        byte[] blockBuffer = packageHandle.ReadBytes((int) blockEntry.Size);
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

    private byte[] GenerateNonce()
    {
        byte[] nonce = { 0x84, 0xDF, 0x11, 0xC0, 0xAC, 0xAB, 0xFA, 0x20, 0x33, 0x11, 0x26, 0x99, };
        nonce[0] ^= (byte) ((Header.GetPackageId() >> 8) & 0xFF);
        nonce[1] ^= 0x26;
        nonce[11] ^= (byte) (Header.GetPackageId() & 0xFF);
        return nonce;
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
        Marshal.Copy((IntPtr) block.GCMTag, tag, 0, 0x10);
        aes.Decrypt(iv, buffer, tag, decryptedBuffer);
        return decryptedBuffer;
    }

    private byte[] DecompressBuffer(byte[] buffer, D2BlockEntry block)
    {
        byte[] decompressedBuffer = new byte[BlockSize];
        OodleLZ_Decompress(buffer, (int) block.Size, decompressedBuffer, BlockSize, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 3);

        return decompressedBuffer;
    }

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
}