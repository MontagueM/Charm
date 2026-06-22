using Tiger.Schema.Entity;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080052E, 0x28)] //2E058080
public struct SActivity_ROI
{
    public long FileSize;
    public Tag<S80800636> LocationNames;
    [SchemaField(0x10)]
    public DynamicArray<S8080410A> Bubbles;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080410A, 0x4)] //0A418080
public struct S8080410A
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public Tag<SBubbleDefinition> ChildMapReference;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800636, 0x38)] //36068080
public struct S80800636
{
    public long FileSize;
    public StringPointer ActivityDevName;
    public StringHash Unk10; // unsure if string hash
    [SchemaField(0x18)]
    public DynamicArray<S808006DB> BubbleNames;
    public DynamicArray<S8080087D> Unk30;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808006DB, 0x18)] //DB068080
public struct S808006DB
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public StringHash BubbleName;
    public short Unk0C;
    public short ThisIndex;
    public int BubbleIndex;  // index to S8080410A in SActivity_ROI
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080087D, 0xAC)] //7D088080
public struct S8080087D
{
    public StringHash BubbleName;
    //[SchemaField(0x18)]
    //public DynamicArray<S80800442> Unk18;

    // Bunch of Vec4s for some reason
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800616, 0x68)] //16068080
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
    public DynamicArray<S8080060C> Unk48;
    [SchemaField(0x58)]
    public DynamicArray<S8080073F> Unk58; // Phases
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080073F, 0x8)] //3F078080
public struct S8080073F
{
    public StringHash Unk00;
    public int Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080060C, 0x18)] //0C068080
public struct S8080060C
{
    public StringHash LocationName;
    [SchemaField(0x08)]
    public DynamicArray<S808006A8> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808006A8, 0x3C)] //A8068080
public struct S808006A8
{
    public uint Unk00;
    public StringHash UnkName0;
    [SchemaField(0x30)]
    public StringHash UnkName1;
    public Tag Unk34; // F0088080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808008F0, 0x20)] //F0088080
public struct S808008F0
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
    public DynamicArray<S808040D3> Unk08;
    public DynamicArray<S808040D3> Unk18;
    public DynamicArray<S808040D3> Unk28; //This sometimes crashes for some reason...?
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808040D3, 0x4)] //D3408080
public struct S808040D3
{
    public FileHash Unk00; // 6E078080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080076E, 0x48)] //6E078080
public struct S8080076E
{
    public long FileSize;
    public TigerHash Unk08;
    public TigerHash Unk1C;
    //public DynamicArray<S808007D3> Unk18;
    [SchemaField(0x28)]
    public LocalizedStrings Strings;
    [SchemaField(0x30)]
    public DynamicArray<S808005E9> Unk30;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808007D3, 0x10)] //D3078080
public struct S808007D3
{
    public int Unk00;
    [SchemaField(0x08)]
    public StringPointer Name;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808005E9, 0x28)] //E9058080
public struct S808005E9
{
    [SchemaField(0x10)]
    public Tag<SMapDataTable> Unk10;
    [SchemaField(0x18)]
    public DynamicArray<S80804222> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80804222, 0x4)] //22428080
public struct S80804222
{
    public Tag<S808003F6> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808003F6, 0x10)] //F6038080
public struct S808003F6
{
    [SchemaField(0xC)]
    public EntityComponent? EntityComponent; // Check Unk10 for 2E098080, Unk18 -> DD078080 0x80
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080092E, 0x3A0)] //2E098080
public struct S8080092E
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808005A7, 0x80)] //A7058080
public struct S808005A7
{
    [SchemaField(0x68)]
    public Tag<S808012D9> Unk68;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800148, 0x28)] //48018080
public struct S80800148 // Named tag 'parent'
{
    public long FileSize;
    [SchemaField(0xC)]
    public TagClassHash Reference; // The reference hash of the tag next to it
    public FileHash Tag;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808012D9, 0x58)] //D9128080
public struct S808012D9 // Scripted entity stuff? Using Vosik to fill this one out
{
    public long FileSize;
    public StringHash Unk09; // sq_machine
    public TigerHash Unk0C;
    public FileHash Unk10;
    [SchemaField(0x20)]
    public DynamicArray<S808014D6> Unk20;
    public DynamicArray<S8080132B> Locations;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808014D6, 0x38)] //D6148080
public struct S808014D6
{
    public StringHash Type; // boss
    [SchemaField(0x8)]
    public DynamicArray<S80801348> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801348, 0x10)] //48138080
public struct S80801348
{
    public ResourcePointer Pointer; // 06048080 (SMapDataEntry), data resource pointing to 33138080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801333, 0x34)] //33138080 // D2 works the same
public struct S80801333
{
    public ResourcePointer Pointer; // 152B8080
    [SchemaField(0x20)]
    public StringHash EntityName; // Vosik, The Archpriest
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802B15, 0x28)] //152B8080
public struct S80802B15
{
    [SchemaField(0x10)]
    public DynamicArray<S80802A4E> Unk10;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802A4E, 0x8)] //4E2A8080
public struct S80802A4E
{
    public TigerHash Unk00;
    public StringHash Type; // Faction? Type? (Devil Splicer's, Scorch Cannon)
}


[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080132B, 0x30)] //2B138080
public struct S8080132B
{
    public Vector4 Location;
    public Vector4 Rotation;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808007DD, 0xB0)] //DD078080
public struct S808007DD
{
    [SchemaField(0x60)]
    public StringPointer DevName;
    [SchemaField(0x68)]
    public DynamicArray<SMapDataEntry> DataEntries;
    [SchemaField(0x80), NoLoad]
    public Entity.Entity? UnkEntity;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802590, 0x570)] //90258080
public struct S80802590
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802993, 0xA8)] //93298080
public struct S80802993
{
    [SchemaField(0x60)]
    public StringPointer DevName;
    public DynamicArray<S808031D7> Directives;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808031D7, 0x18)] //D7318080
public struct S808031D7
{
    public StringHash Description;
    public StringHash Objective;
    public StringHash Objective2;
}


