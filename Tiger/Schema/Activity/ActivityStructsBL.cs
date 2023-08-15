using Tiger.Schema.Audio;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "8E8E8080", 0xB4)]
public struct SActivity_BL
{
    public long FileSize;
    public StringPointer ActivityName;
    public TigerHash LocationName;  // these all have actual string hashes but have no string container given directly
    public TigerHash Unk14;
    public TigerHash Unk18;
    public TigerHash Unk1C;
    public ResourcePointer Unk20;  // 6A988080 + 20978080
    public FileHash64 Unk28;  // some weird kind of parent thing with names, contains the string container for this tag
    [SchemaField(0x70)]
    public DynamicArray<D2Class_26898080> Unk70;
    public DynamicArray<D2Class_24898080> Unk80;
    public TigerHash Unk90;
    public TigerHash Unk94;
    public FileHash Unk98;  // an entity thing
    // public FileHash64 UnkActivity68;  // todo this uses an unknown hash64 system in the package
}

[SchemaStruct("26898080", 0x90)]
public struct D2Class_26898080
{
    public StringPointer DevBubbleName;
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public TigerHash BubbleName;
    public TigerHash Unk14;
    public TigerHash Unk18;
    [SchemaField(0x20)]
    public TigerHash BubbleName2;
    [SchemaField(0x28)]
    public TigerHash Unk20;
    public TigerHash Unk24;
    public TigerHash Unk28;
    [SchemaField(0x38)]
    public int Unk38;
    [SchemaField(0x70)]
    public DynamicArray<D2Class_48898080> Unk70;
}

[SchemaStruct("48898080", 0x20)]
public struct D2Class_48898080
{
    public StringPointer DevBubbleName;
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public TigerHash BubbleName;
    public TigerHash ActivityPhaseName;
    public TigerHash ActivityPhaseName2;
    public Tag<D2Class_898E8080> UnkEntityReference;
}

[SchemaStruct("898E8080", 0x30)]
public struct D2Class_898E8080
{
    public long FileSize;
    public long Unk08;
    public ResourcePointer Unk10;  // ef968080 unk
    public Tag Unk14;  // D2Class_898E8080 entity script stuff
}

[SchemaStruct("EF968080", 0x30)]
public struct D2Class_EF968080
{

}

/// <summary>
/// Stores static map data for activities
/// </summary>
[SchemaStruct("24898080", 0x40)]
public struct D2Class_24898080
{
    public StringPointer UnkBubbleName;
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public StringHash BubbleName;
    [SchemaField(0x18)]
    public ResourcePointer Unk18;  // 0F978080, 53418080
    public DynamicArray<D2Class_48898080> Unk20;
    //public DynamicArray<D2Class_1D898080> MapReferences;
    [Tag64]
    public Tag<SBubbleParent> Unk30;
}
