using System.Runtime.InteropServices;

namespace Tiger.DESTINY1_RISE_OF_IRON;

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x110)]
public struct PackageHeader : IPackageHeader
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

    //pub named_tag_table_size: u32,
    //pub named_tag_table_offset: u32,
    //pub named_tag_table_hash: [u8; 20],

    [SchemaField(0xEC)]
    public uint NamedTagTableSize;
    [SchemaField(0xF0)]
    public GlobalPointer<SD1PackageActivityEntry> NamedTagTable;

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

    public List<SPackageActivityEntry> GetAllActivities(TigerReader reader)
    {
        return new List<SPackageActivityEntry>();
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x44)]
public struct SD1PackageActivityEntry
{
    public FileHash TagHash;
    public TagClassHash TagClassHash;
    //public StringNullTerminated Name;
}


[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x30)]
public struct SPackageTablesData
{
    [SchemaField(0x00)]
    public long ThisSize;
    [SchemaField(0x10)]
    public DynamicArray<D2FileEntryBitpacked> FileEntries;
    [SchemaField(0x20)]
    public DynamicArray<D2BlockEntry> BlockEntries;
}


[StrategyClass(TigerStrategy.DESTINY1_RISE_OF_IRON)]
public class Package : Tiger.Package
{
    [DllImport("ThirdParty/oo2core_3_win64.dll", EntryPoint = "OodleLZ_Decompress")]
    public static extern bool OodleLZ_Decompress(byte[] buffer, int bufferSize, byte[] outputBuffer, int outputBufferSize, int a, int b,
        int c, IntPtr d, IntPtr e, IntPtr f, IntPtr g, IntPtr h, IntPtr i, int threadModule);

    public Package(string packagePath) : base(packagePath, TigerStrategy.DESTINY1_RISE_OF_IRON)
    {
    }

    protected override void ReadHeader(TigerReader reader)
    {
        Header = SchemaDeserializer.Get().DeserializeSchema<PackageHeader>(reader);
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
