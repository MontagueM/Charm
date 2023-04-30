using System.Runtime.InteropServices;

namespace Tiger;

public interface IPackage {
    private static string PackageExtension = ".pkg";
    
    public string PackagePath { get; }

    PackageMetadata GetPackageMetadata();

    FileMetadata GetFileMetadata(ushort fileId);
    FileMetadata GetFileMetadata(FileHash fileId);
    List<FileMetadata> GetAllFileMetadata();

    byte[] GetFileBytes(FileHash fileId);
    HashSet<int> GetRequiredPatches();
    
    public static void CheckValidPackagePath(string packagePath)
    {
        CheckPackagePathExists(packagePath);
        CheckPackagePathValidPrefix(packagePath);
        CheckPackagePathValidExtension(packagePath);
    }

    private static void CheckPackagePathExists(string packagePath)
    {
        if (!File.Exists(packagePath))
        {
            throw new FileNotFoundException($"The package path '{packagePath}' does not exist");
        }
    }

    private static void CheckPackagePathValidPrefix(string packagePath)
    {
        string prefix = Strategy.GetStrategyPackagePrefix(Strategy.CurrentStrategy);
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
        NumType = (sbyte) ((entryBitpacked.EntryB >> 9) & 0x7F);
        NumSubType = (sbyte) ((entryBitpacked.EntryB >> 6) & 0x7);

        // EntryC
        StartingBlockIndex = (int) (entryBitpacked.EntryC & 0x3FFF);
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
    public ushort PatchId;
    public ushort BitFlag;
    public fixed byte SHA1[0x14];
    public fixed byte GCMTag[0x10];
};
