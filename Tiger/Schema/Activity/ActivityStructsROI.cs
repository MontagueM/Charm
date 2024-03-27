using Tiger.Schema.Strings;

namespace Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "2E058080", 0x28)]
public struct SActivity_ROI
{
    public long FileSize;
    public Tag<S36068080> LocationNames;
    [SchemaField(0x10)]
    public DynamicArray<S0A418080> Bubbles;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "0A418080", 0x4)]
public struct S0A418080
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public Tag<SBubbleDefinition> ChildMapReference;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "36068080", 0x38)]
public struct S36068080
{
    public long FileSize;
    public StringPointer ActivityDevName;
    public StringHash Unk10; // unsure if string hash
    [SchemaField(0x18)]
    public DynamicArray<SDB068080> BubbleNames;
    public DynamicArray<S7D088080> Unk30;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "DB068080", 0x18)]
public struct SDB068080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public StringHash BubbleName;
    public short Unk0C;
    public short ThisIndex;
    public int BubbleIndex;  // index to S0A418080 in SActivity_ROI
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "7D088080", 0xAC)]
public struct S7D088080
{
    public StringHash BubbleName;
    //[SchemaField(0x18)]
    //public DynamicArray<S42048080> Unk18;

    // Bunch of Vec4s for some reason
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "42048080", 2)]
public struct S42048080
{
    public short Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "16068080", 0x68)]
public struct SUnkActivity_ROI
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
    [SchemaField(0x48)]
    public DynamicArray<S0C068080> Unk48;
    //[SchemaField(0x58)]
    //public DynamicArray<S3F078080> Unk58;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "3F078080", 0x8)]
public struct S3F078080
{
    public StringHash Unk00;
    public int Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "0C068080", 0x18)]
public struct S0C068080
{
    public StringHash LocationName;
    [SchemaField(0x08)]
    public DynamicArray<SA8068080> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "A8068080", 0x3C)]
public struct SA8068080
{
    public uint Unk00;
    public StringHash UnkName0;
    [SchemaField(0x30)]
    public StringHash UnkName1;
    public Tag Unk34; // F0088080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "F0088080", 0x20)]
public struct SF0088080
{
    public long FileSize;
    public StringPointer ActivityDevName;
    public uint Unk10;
    [SchemaField(0x1C)]
    public FileHash Unk1C; // SF0088080_Child
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x38)] // Doesn't have an 8080 reference hash
public struct SF0088080_Child
{
    public long FileSize;
    public DynamicArray<SD3408080> Unk08;
    public DynamicArray<SD3408080> Unk18;
    public DynamicArray<SD3408080> Unk28; //This sometimes crashes for some reason...?
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D3408080", 0x4)]
public struct SD3408080
{
    public FileHash Unk00; // 6E078080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "6E078080", 0x48)]
public struct S6E078080
{
    public long FileSize;
    public TigerHash Unk08;
    public TigerHash Unk1C;
    //public DynamicArray<SD3078080> Unk18;
    //public FileHash Unk28;
    [SchemaField(0x30)]
    public DynamicArray<SE9058080> Unk30;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D3078080", 0x10)]
public struct SD3078080
{
    public int Unk00;
    [SchemaField(0x08)]
    public StringPointer Name;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "E9058080", 0x28)]
public struct SE9058080
{
    [SchemaField(0x10)]
    public Tag<SMapDataTable> Unk10;
    //[SchemaField(0x18)]
    //public DynamicArray<S22428080> Unk18;
}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "22428080", 0x4)]
//public struct S22428080
//{
//    public Tag<SF6038080> Unk00;
//}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "F6038080", 0x10)]
//public struct SF6038080
//{
//    public EntityResource Unk0C; // Check Unk10 for 2E098080, Unk18 -> DD078080 0x80
//}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "DD078080", 0xB0)]
//public struct SDD078080
//{
//    [SchemaField(0x60)]
//    public StringPointer EntityName;
//    [SchemaField(0x80)]
//    public Entity.Entity Entity; // Why doesn't just Entity work here, am I stupid?
//}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "2E098080", 0x3A0)]
public struct S2E098080
{

}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "14008080", 0x4)]
public struct S14008080
{
    public FileHash Unk00; // Can be SUnkActivity_ROI or something else (Based on tag type?)
}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "60928080", 0x4)]
//public struct S60928080
//{
//    public Tag<S62948080> Unk00;
//}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "62948080", 0x58)]
//public struct S62948080
//{
//    public long FileSize;
//    public TigerHash Unk08; //BubbleName?
//    public TigerHash Unk0C; //ActivityPhaseName?

//    [SchemaField(0x38)]
//    public DynamicArray<S64948080> Unk38;
//}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "64948080", 0x1C)]
//public struct S64948080
//{
//    [SchemaField(0x8)]
//    public DynamicArray<S66948080> Unk08;
//}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "66948080", 0x4)]
//public struct S66948080
//{
//    public Tag<S68948080> Unk00;
//}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "68948080", 0x20)]
//public struct S68948080
//{
//    public long FileSize;
//    public Tag<SMapDataTable> DataTable;
//}


