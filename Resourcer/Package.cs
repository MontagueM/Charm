using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Resourcer;

public struct PackageMetadata
{
    public string PackagePath;
    public string PackageName;
    public int PackageId;
    public int PatchId;
    public int Timestamp;
}

public struct FileMetadata
{
    public FileHash Hash;
    public TigerHash Reference;
    public int Size;
    
    public FileMetadata(FileHash fileHash, D2FileEntry fileEntry)
    {
        Hash = fileHash;
        Reference = fileEntry.Reference;
        Size = fileEntry.FileSize;
    }
}


public interface IPackage
{
    public string PackagePath { get; }
    
    PackageMetadata GetPackageMetadata();
    
    FileMetadata GetFileMetadata(FileHash fileId);

    byte[] GetFileBytes(FileHash fileId);
}

[StructLayout(LayoutKind.Explicit)]
public struct D2PackageHeader
{
    [FieldOffset(0x10)] public ushort PackageId;
    [FieldOffset(0x20)] public uint Timestamp;
    [FieldOffset(0x30)] public ushort PatchId;
    [FieldOffset(0x60)] public uint FileEntryTableCount;
    [FieldOffset(0x44)] public uint FileEntryTableOffset;
    [FieldOffset(0x68)] public uint BlockEntryTableCount;
    [FieldOffset(0x6C)] public uint BlockEntryTableOffset;
    [FieldOffset(0x78)] public uint ActivityTableCount;
    [FieldOffset(0x7C)] public uint ActivityTableOffset;
    [FieldOffset(0xB8)] public uint Hash64TableSize;
    [FieldOffset(0xBC)] public uint Hash64TableOffset;
};

public struct D2FileEntry
{
    public TigerHash Reference;
    public sbyte NumType;
    public sbyte NumSubType;
    public int StartingBlock;
    public int StartingBlockOffset;
    public int FileSize;

    public D2FileEntry(D2FileEntryBitpacked entryBitpacked)
    {
        // EntryA
        Reference = new TigerHash(entryBitpacked.Reference);

        // EntryB
        NumType = (sbyte) ((entryBitpacked.EntryB >> 9) & 0x7F);
        NumSubType = (sbyte) ((entryBitpacked.EntryB >> 6) & 0x7);

        // EntryC
        StartingBlock = (int) (entryBitpacked.EntryC & 0x3FFF);
        StartingBlockOffset = (int) (((entryBitpacked.EntryC >> 14) & 0x3FFF) << 4);

        // EntryD
        FileSize = (int) ((entryBitpacked.EntryD & 0x3FFFFFF) << 4 | (entryBitpacked.EntryC >> 28) & 0xF);
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
    public ushort PatchID;
    public ushort BitFlag;
    public fixed byte SHA1[0x14];
    public fixed byte GCMTag[0x10];
};

public class D2Package : IPackage
{
    private const int BlockSize = 0x40_000;
    public string PackagePath { get; }
    private BinaryReader _reader;
    private D2PackageHeader Header;
    public List<D2FileEntry> FileEntries;
    public List<D2BlockEntry> BlockEntries;
    
    private static readonly byte[] AesKey0 =
        {0xD6, 0x2A, 0xB2, 0xC1, 0x0C, 0xC0, 0x1B, 0xC5, 0x35, 0xDB, 0x7B, 0x86, 0x55, 0xC7, 0xDC, 0x3B};

    private static readonly byte[] AesKey1 =
        {0x3A, 0x4A, 0x5D, 0x36, 0x73, 0xA6, 0x60, 0x58, 0x7E, 0x63, 0xE6, 0x76, 0xE4, 0x08, 0x92, 0xB5};

    [DllImport("oo2core_9_win64.dll", EntryPoint = "OodleLZ_Decompress")]
    public static extern bool OodleLZ_Decompress(byte[] buffer, int bufferSize, byte[] outputBuffer,
        int outputBufferSize, int a, int b, int c, IntPtr d, IntPtr e, IntPtr f, IntPtr g, IntPtr h, IntPtr i,
        int threadModule);
    
    public D2Package(string packagePath)
    {
        CheckValidPackagePath(packagePath);
        PackagePath = SanitisePackagePath(packagePath);
        GetReader();
        ReadHeader();
        ReadFileEntries();
        ReadBlockEntries();
        CloseReader();
    }
    
    private static void CheckValidPackagePath(string packagePath)
    {
        CheckPackagePathExists(packagePath);
        CheckPackagePathValidPrefix(packagePath);
        CheckPackagePathValidExtension(packagePath);
    }
    
    private static readonly string PackagePathDoesNotExistMessage = "The package path does not exist: ";
    private static void CheckPackagePathExists(string packagePath)
    {
        if (!File.Exists(packagePath))
        {
            throw new FileNotFoundException(PackagePathDoesNotExistMessage + packagePath);
        }
    }

    private static readonly string PackagePathInvalidPrefixMessage = "The package path does not have the correct prefix: ";
    private static void CheckPackagePathValidPrefix(string packagePath)
    {
        string prefix = Strategy.GetStrategyPackagePrefix(Strategy.CurrentStrategy);
        if (!packagePath.Contains(prefix + "_"))
        {
            throw new ArgumentException(PackagePathInvalidPrefixMessage + packagePath + ", " + prefix);
        }
    }

    private static readonly string PackagePathInvalidExtensionMessage = "The packages directory does not have the extension .pkg.: ";
    private static void CheckPackagePathValidExtension(string packagePath)
    {
        if (!packagePath.EndsWith(".pkg"))
        {
            throw new ArgumentException(PackagePathInvalidExtensionMessage + packagePath);
        }
    }
    
    private static string SanitisePackagePath(string packagePath)
    {
        return packagePath.Replace("\\", "/");
    }
    
    private void GetReader()
    {
        _reader = new BinaryReader(File.Open(PackagePath, FileMode.Open));
    }
    
    private void CloseReader()
    {
        _reader.Close();
    }

    private void ReadHeader()
    {
        _reader.BaseStream.Seek(0, SeekOrigin.Begin);
        Header = _reader.ReadBytes(0x100).ToStructure<D2PackageHeader>();
    }
    
    private void ReadFileEntries()
    {
        _reader.BaseStream.Seek(Header.FileEntryTableOffset, SeekOrigin.Begin);

        FileEntries = new List<D2FileEntry>();
        int d2FileEntrySize = Marshal.SizeOf<D2FileEntryBitpacked>();
        for (int i = 0; i < Header.FileEntryTableCount; i++)
        {
            D2FileEntryBitpacked fileEntryBitpacked = _reader.ReadBytes(d2FileEntrySize).ToStructure<D2FileEntryBitpacked>();
            FileEntries.Add(new D2FileEntry(fileEntryBitpacked));
        }
    }
    
    private void ReadBlockEntries()
    {
        _reader.BaseStream.Seek(Header.BlockEntryTableOffset, SeekOrigin.Begin);

        BlockEntries = new List<D2BlockEntry>();
        int d2BlockEntrySize = Marshal.SizeOf<D2BlockEntry>();
        for (int i = 0; i < Header.BlockEntryTableCount; i++)
        {
            D2BlockEntry blockEntry = _reader.ReadBytes(d2BlockEntrySize).ToStructure<D2BlockEntry>();
            BlockEntries.Add(blockEntry);
        }    
    }
    
    public PackageMetadata GetPackageMetadata()
    {
        PackageMetadata packageMetadata = new PackageMetadata();
        packageMetadata.PackagePath = PackagePath;
        packageMetadata.PackageName = PackagePath.Split('/').Last();
        packageMetadata.PackageId = Header.PackageId;
        packageMetadata.PatchId = Header.PatchId;
        packageMetadata.Timestamp = (int) Header.Timestamp;
        return packageMetadata;
    }

    private static readonly string FileMetadataInvalidPackageIdMessage = "The provided file hash has an invalid package id: ";
    public FileMetadata GetFileMetadata(FileHash fileHash)
    {
        if (fileHash.GetPackageId() != Header.PackageId)
        {
            throw new ArgumentException(FileMetadataInvalidPackageIdMessage  + fileHash.GetPackageId());
        }
        return GetFileMetadata(fileHash.GetFileIndex());
    }
    
    private static readonly string FileMetadataFileIndexOutOfRangeMessage = "The provided file hash has an out-of-range file index: ";
    public FileMetadata GetFileMetadata(ushort fileIndex)
    {
        if (fileIndex >= FileEntries.Count)
        {
            throw new ArgumentOutOfRangeException(FileMetadataFileIndexOutOfRangeMessage + $"{fileIndex} >= {FileEntries.Count}");
        }
        return new FileMetadata(new FileHash(Header.PackageId, fileIndex), FileEntries[fileIndex]);
    }

    public byte[] GetFileBytes(FileHash fileId)
    {
        D2FileEntry fileEntry = FileEntries[fileId.GetFileIndex()];
                    int blockCount =
                (int) Math.Floor((double) (fileEntry.StartingBlockOffset + fileEntry.FileSize - 1) / (double) BlockSize);
            byte[] buffer = new byte[fileEntry.FileSize];
            int currentBufferOffset = 0;
            int currentBlockID = 0;
            Dictionary<int, BinaryReader> pkgHandles = new Dictionary<int, BinaryReader>();
            List<D2BlockEntry> blocks = BlockEntries.GetRange(fileEntry.StartingBlockOffset, blockCount);
            foreach (D2BlockEntry block in blocks)
            {
                BinaryReader pkgHandle = null;
                if (pkgHandles.ContainsKey(block.PatchID))
                {
                    pkgHandle = pkgHandles[block.PatchID];
                }
                else
                {
                    // pkgHandle = new BinaryReader(new FileStream(
                        // Path.Combine(PkgPath.Substring(0, PkgPath.Length - 5) + block.PatchID.ToString("D") + ".pkg"),
                        // FileMode.Open, FileAccess.Read, FileShare.Read));
                    // pkgHandles.Add(block.PatchID, pkgHandle);
                }

                // Read block data
                pkgHandle.BaseStream.Seek(block.Offset, SeekOrigin.Begin);
                byte[] blockBuffer = pkgHandle.ReadBytes((int) block.Size);

                // Deobfuscate
                byte[] decryptedBuffer;
                if ((block.BitFlag & 0x2) == 2)
                {
                    decryptedBuffer = DecryptBuffer(blockBuffer, block);
                }
                else
                {
                    decryptedBuffer = blockBuffer;
                }

                byte[] decompressedBuffer;
                if ((block.BitFlag & 0x1) == 1)
                {
                    decompressedBuffer = DecompressBuffer(decryptedBuffer, block);
                }
                else
                {
                    decompressedBuffer = decryptedBuffer;
                }

                if (currentBlockID == 0)
                {
                    int copySize;
                    if (currentBlockID == blockCount)
                    {
                        copySize = fileEntry.FileSize;
                    }
                    else
                    {
                        copySize = 0x40_000 - fileEntry.StartingBlockOffset;
                    }

                    Array.Copy(decompressedBuffer, fileEntry.StartingBlockOffset, buffer, 0, copySize);
                    currentBufferOffset += copySize;
                }
                else if (currentBlockID == blockCount)
                {
                    Array.Copy(decompressedBuffer, 0, buffer, currentBufferOffset,
                        fileEntry.FileSize - currentBufferOffset);
                }
                else
                {
                    Array.Copy(decompressedBuffer, 0, buffer, currentBufferOffset, BlockSize);
                    currentBufferOffset += BlockSize;
                }

                currentBlockID++;

            }

            return buffer;
    }
    
    public byte[] GenerateIV()
    {
        byte[] nonce = {0x84, 0xEA, 0x11, 0xC0, 0xAC, 0xAB, 0xFA, 0x20, 0x33, 0x11, 0x26, 0x99};
        nonce[0] ^= (byte) ((Header.PackageId >> 8) & 0xFF);
        nonce[11] ^= (byte) (Header.PackageId & 0xFF);
        nonce[5] ^= Header.Timestamp < 1675098707 ? (byte) 0 : (byte) 1;
        return nonce;
    }

    public unsafe byte[] DecryptBuffer(byte[] buffer, D2BlockEntry block)
    {
        byte[] decryptedBuffer = new byte[buffer.Length];
        byte[] key = new byte[0x10];
        if ((block.BitFlag & 0x4) == 4)
        {
            key = AesKey1;
        }
        else
        {
            key = AesKey0;
        }

        byte[] iv = GenerateIV();
        using var aes = new AesGcm(key);
        byte[] tag = new byte[0x10];
        Marshal.Copy((IntPtr) block.GCMTag, tag, Header.Timestamp < 1675098903 ? (byte) 0 : (byte) 1, 0x10);
        aes.Decrypt(iv, buffer, tag, decryptedBuffer);
        return decryptedBuffer;
    }

    public byte[] DecompressBuffer(byte[] buffer, D2BlockEntry block)
    {
        byte[] decompressedBuffer = new byte[0x40_000];
        OodleLZ_Decompress(buffer, (int) block.Size, decompressedBuffer, BlockSize,
            0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 3);

        return decompressedBuffer;
    }
}