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
    [SchemaField(0xEC)]
    public uint NamedTagTableCount;
    [SchemaField(0xF0)]
    public uint NamedTagTableOffset;

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
        int d1FileEntrySize = Marshal.SizeOf<D1FileEntryBitpacked>();
        for (int i = 0; i < FileEntryTableCount; i++)
        {
            D1FileEntry fileEntryBitpacked = new D1FileEntry(reader.ReadBytes(d1FileEntrySize).ToType<D1FileEntryBitpacked>());
            fileEntries.Add(new D2FileEntry()
            {
                Reference = fileEntryBitpacked.Reference,
                NumType = fileEntryBitpacked.NumType,
                NumSubType = fileEntryBitpacked.NumSubType,
                StartingBlockIndex = fileEntryBitpacked.StartingBlockIndex,
                StartingBlockOffset = fileEntryBitpacked.StartingBlockOffset,
                FileSize = fileEntryBitpacked.FileSize,
            });
        }

        return fileEntries;
    }

    public List<D2BlockEntry> GetBlockEntries(TigerReader reader)
    {
        reader.Seek(BlockEntryTableOffset, SeekOrigin.Begin);

        List<D2BlockEntry> blockEntries = new();
        int d1BlockEntrySize = Marshal.SizeOf<D1BlockEntry>();
        for (int i = 0; i < BlockEntryTableCount; i++)
        {
            D1BlockEntry blockEntry = reader.ReadBytes(d1BlockEntrySize).ToType<D1BlockEntry>();
            blockEntries.Add(new D2BlockEntry()
            {
                Offset = blockEntry.Offset,
                Size = blockEntry.Size,
                PatchId = blockEntry.PatchId,
                BitFlag = blockEntry.BitFlag,
                //SHA1 = blockEntry.SHA1,
            });
        }

        return blockEntries;
    }

    public List<SHash64Definition> GetHash64Definitions(TigerReader reader)
    {
        return new List<SHash64Definition>();
    }

    public List<PackageActivityEntry> GetAllActivities(TigerReader reader)
    {
        reader.Seek(NamedTagTableOffset, SeekOrigin.Begin);

        List<PackageActivityEntry> activityEntries = new();
        int d1ActivityEntrySize = Marshal.SizeOf<SPackageActivityEntry_D1>();
        for (int i = 0; i < NamedTagTableCount; i++)
        {
            SPackageActivityEntry_D1 activityEntry = reader.ReadBytes(d1ActivityEntrySize).ToType<SPackageActivityEntry_D1>();
            string Name = reader.ReadNullTerminatedString();

            // 16068080 is SUnkActivity_ROI
            // 2E058080 is SActivity_ROI
            activityEntries.Add(new PackageActivityEntry()
            {
                TagHash = new FileHash(activityEntry.TagHash),
                TagClassHash = new TagClassHash(activityEntry.TagClassHash),
                Name = Name,
            });

            reader.Seek(NamedTagTableOffset + (0x44 * i), SeekOrigin.Begin);
        }

        return activityEntries;
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x44)]
public struct SPackageActivityEntry_D1
{
    public uint TagHash; // Reading these as FileHash and TagClassHash causes a crash for some reason...?
    public uint TagClassHash;
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

public struct D1FileEntry
{
    public TigerHash Reference;
    public sbyte NumType;
    public sbyte NumSubType;
    public int StartingBlockIndex;
    public int StartingBlockOffset;
    public int FileSize;

    public D1FileEntry(D1FileEntryBitpacked entryBitpacked)
    {
        // EntryA
        Reference = new TigerHash(entryBitpacked.Reference);

        // EntryB
        NumType = (sbyte)(entryBitpacked.EntryB & 0xffff);
        NumSubType = (sbyte)(entryBitpacked.EntryB >> 24);

        // EntryC
        StartingBlockIndex = (int)(entryBitpacked.BlockInfo & 0x3FFF);
        StartingBlockOffset = (int)(((entryBitpacked.BlockInfo >> 14) & 0x3FFF) << 4);

        // EntryD
        FileSize = (int)((entryBitpacked.BlockInfo >> 28) & 0x3FFFFFFF);
    }
};

[StructLayout(LayoutKind.Sequential)]
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "F39E8080", 0x10)]
public struct D1FileEntryBitpacked
{
    public uint Reference;
    public uint EntryB;
    public ulong BlockInfo;

    public D1FileEntryBitpacked(uint reference, uint entryB, ulong blockInfo)
    {
        Reference = reference;
        EntryB = entryB;
        BlockInfo = blockInfo;
    }
}

[StructLayout(LayoutKind.Sequential)]
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "EE9E8080", 0x20)]
public unsafe struct D1BlockEntry
{
    public uint Offset;
    public uint Size;
    public ushort PatchId;
    public ushort BitFlag;
    public fixed byte SHA1[0x14];
};


