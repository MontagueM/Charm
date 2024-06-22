using System.Runtime.InteropServices;

namespace Tiger.DESTINY2_BEYONDLIGHT_3402;

[StructLayout(LayoutKind.Explicit)]
public struct PackageHeader : IPackageHeader
{
    [FieldOffset(0x8)]
    public ulong PackageGroup;
    [FieldOffset(0x10)]
    public ushort PackageId;
    [FieldOffset(0x20)]
    public uint Timestamp;
    [FieldOffset(0x30)]
    public ushort PatchId;
    [FieldOffset(0x60)]
    public uint FileEntryTableCount;
    [FieldOffset(0x44)]
    public uint FileEntryTableOffset;
    [FieldOffset(0x68)]
    public uint BlockEntryTableCount;
    [FieldOffset(0x6C)]
    public uint BlockEntryTableOffset;
    [FieldOffset(0x78)]
    public uint ActivityTableCount;
    [FieldOffset(0x7C)]
    public uint ActivityTableOffset;
    [FieldOffset(0xB8)]
    public uint Hash64TableSize;
    [FieldOffset(0xBC)]
    public uint Hash64TableOffset;

    public ulong GetPackageGroup()
    {
        return PackageGroup;
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
            // D2FileEntryBitpacked fileEntryBitpacked = reader.ReadBytes(d2FileEntrySize).ToType<D2FileEntryBitpacked>();
            D2FileEntryBitpacked fileEntryBitpacked = new(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32());
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
        List<SHash64Definition> hash64List = new();

        reader.Seek(Hash64TableOffset + 0x50, SeekOrigin.Begin);
        for (int i = 0; i < Hash64TableSize; i++)
        {
            var entry = reader.ReadBytes(0x10).ToType<SHash64Definition>();
            hash64List.Add(entry);
        }

        if (PackageId != 0x013e)
        {
            return hash64List;
        }

        // test
        reader.Seek(Hash64TableOffset + 0x28, SeekOrigin.Begin);
        long count = reader.ReadInt64();
        RelativePointer pointer = SchemaDeserializer.Get().DeserializeTigerType<RelativePointer>(reader);
        reader.Seek(pointer.AbsoluteOffset + 0x10 + count * 4, SeekOrigin.Begin);
        return hash64List;
        while (true)
        {
            ushort count0 = reader.ReadUInt16();
            List<ushort> unk0s = new();
            for (ushort i = 0; i < count0; i++)
            {
                ushort unk0 = reader.ReadUInt16();
                unk0s.Add(unk0);
            }
            ushort count1 = reader.ReadUInt16();
            List<ushort> unk1s = new();
            for (ushort i = 0; i < count1; i++)
            {
                ushort unk1 = reader.ReadUInt16();
                unk1s.Add(unk1);
            }
            ushort count2 = reader.ReadUInt16();
            List<ushort> unk2s = new();
            for (ushort i = 0; i < count2; i++)
            {
                ushort unk2 = reader.ReadUInt16();
                unk2s.Add(unk2);
            }
            ushort count3 = reader.ReadUInt16();
            // padded to 8 bytes
            if (reader.Position % 8 != 0)
            {
                reader.Seek(8 - reader.Position % 8, SeekOrigin.Current);
            }
            List<ulong> hash64s = new();
            for (ushort i = 0; i < count3; i++)
            {
                ulong hash64 = reader.ReadUInt64();
                hash64s.Add(hash64);
            }
            if (reader.Position >= reader.BaseStream.Length)
            {
                break;
            }
        }

        return hash64List;
    }

    public List<PackageActivityEntry> GetAllActivities(TigerReader reader)
    {
        List<PackageActivityEntry> activityEntries = new();

        // todo this can be better if we had the package using schema deserialization properly
        // 0x30 is due to the indirection table which we skip
        for (int i = 0; i < ActivityTableCount; i++)
        {
            reader.Seek(ActivityTableOffset + 0x30 + 0x10 * i, SeekOrigin.Begin);
            SD2PackageActivityEntry entry = SchemaDeserializer.Get().DeserializeSchema<SD2PackageActivityEntry>(reader);
            activityEntries.Add(new PackageActivityEntry
            {
                TagHash = entry.TagHash,
                TagClassHash = entry.TagClassHash,
                Name = entry.Name.Value
            });
        }

        return activityEntries;
    }
};

[StrategyClass(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
public class Package : Tiger.Package
{
    public Package(string packagePath) : base(packagePath, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)
    {
    }

    protected override void ReadHeader(TigerReader reader)
    {
        reader.Seek(0, SeekOrigin.Begin);
        Header = reader.ReadType<PackageHeader>();
    }

    [DllImport("ThirdParty/oo2core_9_win64.dll", EntryPoint = "OodleLZ_Decompress")]
    public static extern bool OodleLZ_Decompress(byte[] buffer, int bufferSize, byte[] outputBuffer, int outputBufferSize, int a, int b,
        int c, IntPtr d, IntPtr e, IntPtr f, IntPtr g, IntPtr h, IntPtr i, int threadModule);

    protected override byte[] OodleDecompress(byte[] buffer, int blockSize)
    {
        byte[] decompressedBuffer = new byte[BlockSize];
        OodleLZ_Decompress(buffer, blockSize, decompressedBuffer, BlockSize, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 3);
        return decompressedBuffer;
    }

    protected override byte[] GenerateNonce()
    {
        byte[] nonce = { 0x84, 0xEA, 0x11, 0xC0, 0xAC, 0xAB, 0xFA, 0x20, 0x33, 0x11, 0x26, 0x99 };
        nonce[0] ^= (byte)((Header.GetPackageId() >> 8) & 0xFF);
        nonce[11] ^= (byte)(Header.GetPackageId() & 0xFF);
        return nonce;
    }
}
