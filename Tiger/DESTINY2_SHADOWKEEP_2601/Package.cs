using System.Runtime.InteropServices;
using System.Security.Cryptography;

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

    public List<Hash64Definition> GetHash64Definitions(TigerReader reader) => throw new NotSupportedException();
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
    public GlobalPointer<ActivityTableData> ActivityTableData;
    [SchemaField(0xF8)]
    public uint ActivityTableDataSize;
    [SchemaField(0x110)]
    public GlobalPointer<PackageTablesData> PackageTablesData;
    [SchemaField(0x118)]
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
        return (uint)PackageTablesData.Value.FileEntries.Count;
    }

    public List<D2FileEntry> GetFileEntries(TigerReader reader)
    {
        List<D2FileEntry> fileEntries = new();
        // todo this goes via the full deserialization route, which is slow
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

    public List<Hash64Definition> GetHash64Definitions(TigerReader reader) => throw new NotSupportedException();
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x20)]
public struct ActivityTableData
{
    [SchemaField(0x00)]
    public long ThisSize;
    [SchemaField(0x10)]
    public uint ActivityTableCount;
    [SchemaField(0x18)]
    public uint ActivityTableOffset;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x30)]
public struct PackageTablesData
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
    [DllImport("oo2core_3_win64.dll", EntryPoint = "OodleLZ_Decompress")]
    public static extern bool OodleLZ_Decompress(byte[] buffer, int bufferSize, byte[] outputBuffer, int outputBufferSize, int a, int b,
        int c, IntPtr d, IntPtr e, IntPtr f, IntPtr g, IntPtr h, IntPtr i, int threadModule);

    public Package(string packagePath) : base(packagePath, TigerStrategy.DESTINY2_SHADOWKEEP_2601)
    {
    }

    protected override void ReadHeader(TigerReader reader)
    {
        reader.Seek(0x8, SeekOrigin.Begin);
        ulong buildId = reader.ReadUInt64();
        bool isNewHeader = buildId >= 17011569960331205102;
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
        nonce[0] ^= (byte)((_tag.GetPackageId() >> 8) & 0xFF);
        nonce[1] ^= 0x26;
        nonce[11] ^= (byte)(_tag.GetPackageId() & 0xFF);
        return nonce;
    }
}
