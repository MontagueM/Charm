using Tiger.Schema.Strings;

namespace Tiger.Schema.Activity.DESTINY2_SHADOWKEEP_2601;

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "DE918080", 0x88)]
public struct SActivity_SK
{
    public long FileSize;
    public Tag<S62998080> LocationNames;
    public Tag<S80978080> Unk0C;
    public DynamicArray<S537D8080> Bubbles;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "537D8080", 0x10)]
public struct S537D8080
{
    [Tag64]
    public Tag<SBubbleParent> MapReference;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "62998080", 0x88)]
public struct S62998080
{
    public long FileSize;
    public StringPointer ActivityDevName;
    public DynamicArray<SC4988080> BubbleNames;
    public DynamicArray<SC2988080> Unk30;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "C4988080", 0x18)]
public struct SC4988080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public StringHash BubbleName;
    public short Unk0C;
    public short ThisIndex;
    public int BubbleIndex;  // index to S537D8080 in SActivity_SK
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "C2988080", 0x50)]
public struct SC2988080
{
    public StringHash BubbleName;
    public int CumulativeOffset;
    public DynamicArray<S9C9B8080> Unk08;
    public DynamicArray<S9C9B8080> Unk18;
    public int Unk28;
    [SchemaField(0x48)]
    public int ThisIndex;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "9C9B8080", 2)]
public struct S9C9B8080
{
    public short Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "80978080", 0x88)]
public struct S80978080
{
    public long FileSize;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "94998080", 0x88)]
public struct SUnkActivity_SK
{
    public long FileSize;
    public TigerHash LocationName;  // these all have actual string hashes but have no string container given directly
    [SchemaField(0x18)]
    public uint Unk18;
    public TigerHash Unk1C;
    public TigerHash Unk20;
    public TigerHash Unk24;
    public LocalizedStrings LocalizedStrings;
    [SchemaField(0x30)]
    public StringPointer ActivityDevName;
    // public Tag Unk38;  // 80978080
    //[SchemaField(0x40)]
    //public DynamicArray<S4A928080> Unk40;
    [SchemaField(0x50)]
    public DynamicArray<S4D928080> Unk50;
    // [SchemaField(0x78)]
    // public Tag Unk78;
    // [SchemaField(0x80)]
    // public Tag Unk80;  // 40948080
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "4A928080", 0x10)]
public struct S4A928080
{
    public StringHash UnkLocationName;
    public StringPointer UnkLocationDevName;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "4D928080", 0x18)]
public struct S4D928080
{
    public StringHash LocationName;
    [SchemaField(0x08)]
    public DynamicArray<S4F928080> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "4F928080", 0x4C)]
public struct S4F928080
{
    public uint Unk00;
    public StringHash UnkName0;
    public uint Unk0C;
    [SchemaField(0x1C)]
    public uint Unk1C;
    public uint Unk20;
    [SchemaField(0x40)]
    public StringHash UnkName1;
    public Tag Unk44;
    public uint Unk48;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "5B928080", 0x18)]
public struct S5B928080
{
    public long FileSize;
    public StringPointer ActivityDevName;
    public uint Unk10;
    public Tag<S5E928080> Unk14;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "5E928080", 0x38)]
public struct S5E928080
{
    public long FileSize;
    public DynamicArray<S60928080> Unk08;
    public DynamicArray<S60928080> Unk18;
    public DynamicArray<S60928080> Unk28; //This sometimes crashes for some reason...?
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "60928080", 0x4)]
public struct S60928080
{
    public Tag<S62948080> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "62948080", 0x58)]
public struct S62948080
{
    public long FileSize;
    public TigerHash Unk08; //BubbleName?
    public TigerHash Unk0C; //ActivityPhaseName?

    [SchemaField(0x38)]
    public DynamicArray<S64948080> Unk38;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "64948080", 0x1C)]
public struct S64948080
{
    [SchemaField(0x8)]
    public DynamicArray<S66948080> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "66948080", 0x4)]
public struct S66948080
{
    public Tag<S68948080> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "68948080", 0x20)]
public struct S68948080
{
    public long FileSize;
    public Tag<SMapDataTable> DataTable;
}
