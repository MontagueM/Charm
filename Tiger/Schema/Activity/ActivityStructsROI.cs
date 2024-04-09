using Tiger.Schema.Entity;
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
    public StringHash LocationName;  // these all have actual string hashes but have no string container given directly
    [SchemaField(0x18)]
    public uint Unk18;
    public StringHash Unk1C;
    public StringHash DestinationName;
    public StringHash Unk24;
    public LocalizedStrings LocalizedStrings;
    [SchemaField(0x30)]
    public StringPointer ActivityDevName;
    [SchemaField(0x48)]
    public DynamicArray<S0C068080> Unk48;
    [SchemaField(0x58)]
    public DynamicArray<S3F078080> Unk58; // Phases
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
    [SchemaField(0x28)]
    public LocalizedStrings Strings;
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
    [SchemaField(0x18)]
    public DynamicArray<S22428080> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "22428080", 0x4)]
public struct S22428080
{
    public Tag<SF6038080> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "F6038080", 0x10)]
public struct SF6038080
{
    [SchemaField(0xC)]
    public EntityResource? EntityResource; // Check Unk10 for 2E098080, Unk18 -> DD078080 0x80
}

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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "BC078080", 0x464)]
public struct SBC078080
{

}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "A7058080", 0x80)]
public struct SA7058080
{
    [SchemaField(0x68)]
    public Tag<SD9128080> Unk68;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "14008080", 0x4)]
public struct S14008080
{
    public FileHash Unk00; // Can be SUnkActivity_ROI or something else (Based on tag type?)
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "48018080", 0x28)]
public struct S48018080 // Named tag 'parent'
{
    public long FileSize;
    [SchemaField(0xC)]
    public TagClassHash Reference; // The reference hash of the tag next to it
    public FileHash Tag;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D9128080", 0x58)]
public struct SD9128080 // Scripted entity stuff? Using Vosik to fill this one out
{
    public long FileSize;
    public StringHash Unk09; // sq_machine
    public TigerHash Unk0C;
    public FileHash Unk10;
    [SchemaField(0x20)]
    public DynamicArray<SD6148080> Unk20;
    public DynamicArray<S2B138080> Locations;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D6148080", 0x38)]
public struct SD6148080
{
    public StringHash Type; // boss
    [SchemaField(0x8)]
    public DynamicArray<S48138080> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "48138080", 0x10)]
public struct S48138080
{
    public ResourcePointer Pointer; // 06048080 (SMapDataEntry), data resource pointing to 33138080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "33138080", 0x34)] // D2 works the same
public struct S33138080
{
    public ResourcePointer Pointer; // 152B8080
    [SchemaField(0x20)]
    public StringHash EntityName; // Vosik, The Archpriest
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "152B8080", 0x28)]
public struct S152B8080
{
    [SchemaField(0x10)]
    public DynamicArray<S4E2A8080> Unk10;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "4E2A8080", 0x8)]
public struct S4E2A8080
{
    public TigerHash Unk00;
    public StringHash Type; // Faction? Type? (Devil Splicer's, Scorch Cannon)
}


[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "2B138080", 0x30)]
public struct S2B138080
{
    public Vector4 Location;
    public Vector4 Rotation;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "DD078080", 0xB0)]
public struct SDD078080
{
    [SchemaField(0x60)]
    public StringPointer DevName;
    [SchemaField(0x68)]
    public DynamicArray<SMapDataEntry> DataEntries;
    [SchemaField(0x80), NoLoad]
    public Entity.Entity? UnkEntity;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "90258080", 0x570)]
public struct S90258080
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "93298080", 0xA8)]
public struct S93298080
{
    [SchemaField(0x60)]
    public StringPointer DevName;
    public DynamicArray<SD7318080> Directives;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D7318080", 0x18)]
public struct SD7318080
{
    public StringHash Description;
    public StringHash Objective;
    public StringHash Objective2;
}


