using System.Runtime.InteropServices;

namespace Tiger.DESTINY2_SHADOWKEEP_2601;

// todo change PackageHeader into a class and inherit from IPackageHeader, and instead add GetFileEntries() and GetBlockEntries() to IPackage

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0xD8)]
public struct PackageHeaderOld : IPackageHeader
{
    [SchemaField(0x04)]
    public ushort PackageId;
    [SchemaField(0x10)]
    public uint Timestamp;
    [SchemaField(0x20)]
    public ushort PatchId;
    [SchemaField(0xB4)]
    public uint FileEntryTableCount;
    [SchemaField(0xB8)]
    public uint FileEntryTableOffset;
    [SchemaField(0xD0)]
    public uint BlockEntryTableCount;
    [SchemaField(0xD4)]
    public uint BlockEntryTableOffset;
    [SchemaField(0xF0)]
    public GlobalPointer<SMiscTableDataOld> MiscTableData;
    [SchemaField(0xF4)]
    public uint MiscTableDataSize;

    public ulong GetPackageGroup()
    {
        return 0;
    }

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

    public List<SHash64Definition> GetHash64Definitions(TigerReader reader)
    {
        return new List<SHash64Definition>();
    }

    public List<PackageActivityEntry> GetAllActivities(TigerReader reader)
    {
        if (MiscTableData.Value.Activities == null)
        {
            return new List<PackageActivityEntry>();
        }

        List<PackageActivityEntry> activities = new List<PackageActivityEntry>();

        foreach (var activity in MiscTableData.Value.Activities)
        {
            activities.Add(new PackageActivityEntry
            {
                TagHash = activity.TagHash,
                TagClassHash = activity.TagClassHash,
                Name = activity.Name.Value,
            });
        }

        return activities;
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x120)]
public struct PackageHeaderNew : IPackageHeader
{
    [SchemaField(0x04)]
    public ushort PackageId;
    [SchemaField(0x10)]
    public uint Timestamp;
    [SchemaField(0x20)]
    public ushort PatchId;
    [SchemaField(0xF0)]
    public GlobalPointer<SMiscTableDataNew> MiscTableData;
    [SchemaField(0xF4)]
    public uint MiscTableDataSize;
    [SchemaField(0x110)]
    public GlobalPointer<SPackageTablesData> PackageTablesData;
    [SchemaField(0x114)]
    public uint PackageTablesDataSize;

    public ulong GetPackageGroup()
    {
        return 0;
    }

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
        return (uint)PackageTablesData.Value.FileEntries.Count;
    }

    public List<D2FileEntry> GetFileEntries(TigerReader reader)
    {
        List<D2FileEntry> fileEntries = new();
        foreach (D2FileEntryBitpacked fileEntryBitpacked in PackageTablesData.Value.FileEntries)
        {
            fileEntries.Add(new D2FileEntry(fileEntryBitpacked));
        }

        return fileEntries;
    }

    public List<D2BlockEntry> GetBlockEntries(TigerReader reader)
    {
        List<D2BlockEntry> blockEntries = new();
        foreach (D2BlockEntry blockEntry in PackageTablesData.Value.BlockEntries)
        {
            blockEntries.Add(blockEntry);
        }

        return blockEntries;
    }

    public List<SHash64Definition> GetHash64Definitions(TigerReader reader)
    {
        if (MiscTableData.Value.Hash64s == null)
        {
            return new List<SHash64Definition>();
        }

        return MiscTableData.Value.Hash64s;
    }

    public List<PackageActivityEntry> GetAllActivities(TigerReader reader)
    {
        if (MiscTableData.Value.Activities == null)
        {
            return new List<PackageActivityEntry>();
        }

        List<PackageActivityEntry> activities = new List<PackageActivityEntry>();

        foreach (var activity in MiscTableData.Value.Activities)
        {
            activities.Add(new PackageActivityEntry
            {
                TagHash = activity.TagHash,
                TagClassHash = activity.TagClassHash,
                Name = activity.Name.Value,
            });
        }

        return activities;
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x18)]
public struct SMiscTableDataOld
{
    [SchemaField(0x00)]
    public long ThisSize;
    public DynamicArray<SD2PackageActivityEntry> Activities;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x60)]
public struct SMiscTableDataNew
{
    [SchemaField(0x00)]
    public long ThisSize;
    [SchemaField(0x10)]
    public DynamicArray<SD2PackageActivityEntry> Activities;
    [SchemaField(0x30)]
    public DynamicArray<SHash64Definition> Hash64s;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x30)]
public struct SPackageTablesData
{
    [SchemaField(0x00)]
    public long ThisSize;
    [SchemaField(0x10)]
    public DynamicArray<D2FileEntryBitpacked> FileEntries;
    [SchemaField(0x20)]
    public DynamicArray<D2BlockEntry> BlockEntries;
}


[StrategyClass(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
public class Package : Tiger.Package
{
    [DllImport("ThirdParty/oo2core_3_win64.dll", EntryPoint = "OodleLZ_Decompress")]
    public static extern bool OodleLZ_Decompress(byte[] buffer, int bufferSize, byte[] outputBuffer, int outputBufferSize, int a, int b,
        int c, IntPtr d, IntPtr e, IntPtr f, IntPtr g, IntPtr h, IntPtr i, int threadModule);

    public Package(string packagePath) : base(packagePath, TigerStrategy.DESTINY2_SHADOWKEEP_2601)
    {
    }

    protected override void ReadHeader(TigerReader reader)
    {
        reader.Seek(0x10, SeekOrigin.Begin);
        ulong timestamp = reader.ReadUInt32();
        bool isNewHeader = timestamp >= 1533900000;
        reader.Seek(0, SeekOrigin.Begin);
        if (isNewHeader)
        {
            Header = SchemaDeserializer.Get().DeserializeSchema<PackageHeaderNew>(reader);
        }
        else
        {
            Header = SchemaDeserializer.Get().DeserializeSchema<PackageHeaderOld>(reader);
        }
    }

    protected override byte[] OodleDecompress(byte[] buffer, int blockSize)
    {
        byte[] decompressedBuffer = new byte[BlockSize];
        OodleLZ_Decompress(buffer, blockSize, decompressedBuffer, BlockSize, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 3);
        return decompressedBuffer;
    }

    protected override byte[] GenerateNonce()
    {
        byte[] nonce = { 0x84, 0xDF, 0x11, 0xC0, 0xAC, 0xAB, 0xFA, 0x20, 0x33, 0x11, 0x26, 0x99, };
        nonce[0] ^= (byte)((Header.GetPackageId() >> 8) & 0xFF);
        nonce[1] ^= 0x26;
        nonce[11] ^= (byte)(Header.GetPackageId() & 0xFF);
        return nonce;
    }
}
