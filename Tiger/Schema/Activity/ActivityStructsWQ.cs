using Tiger.Schema.Audio;
using Tiger.Schema.Entity;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "8E8E8080", 0xB4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "8E8E8080", 0x78)]
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, "8E8E8080", 0x88)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "8E8E8080", 0x88)]
public struct SActivity_WQ
{
    public long FileSize;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer ActivityName;

    [SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash LocationName;  // these all have actual string hashes but have no string container given directly
    public TigerHash Unk0C;
    public TigerHash Unk10;
    public TigerHash Unk14;
    public ResourcePointer Unk18;  // 6A988080 + 20978080 (+ 19978080, beyondlight)
    public FileHash64 Destination;  // S8B8E8080

    //[SchemaField(0x30, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)] // Doesnt look useful?
    //public DynamicArray<S00978080> Unk30;

    [SchemaField(0x70, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<S26898080> Unk40;
    public DynamicArray<S24898080> Unk50;

    [SchemaField(0x90, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x60, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    public TigerHash Unk60;
    public FileHash Unk64;  // an entity thing

    [SchemaField(0xA0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_LIGHTFALL_7366, Tag64 = true)]
    public Tag AmbientActivity;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "8B8E8080", 0xD0)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "8B8E8080", 0x78)]
public struct S8B8E8080
{
    public long FileSize;
    public StringHash LocationName;

    [SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer LocalizedStringsContentPath;

    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public LocalizedStrings StringContainer;
    public FileHash Events;
    public FileHash Patrols;
    public uint Unk28;
    public FileHash Unk2C;

    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<SDE448080> TagBags;

    [SchemaField(0xB8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<S2E898080> Activities;
    public StringPointer DestinationName;
}

[SchemaStruct("DE448080", 4)]
public struct SDE448080
{
    public Tag Unk00;
}

[SchemaStruct("2E898080", 0x18)]
public struct S2E898080
{
    public TigerHash ShortActivityName;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    public TigerHash Unk10;
    public StringPointer ActivityName;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "26898080", 0x90)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "26898080", 0x58)]
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, "26898080", 0x68)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "26898080", 0x70)]
public struct S26898080
{
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer DevBubbleName;

    public TigerHash LocationName;
    public TigerHash ActivityName;
    public TigerHash BubbleName;
    public TigerHash Unk0C;
    public TigerHash Unk10;

    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash BubbleName2;

    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk20;
    public TigerHash Unk24;
    public TigerHash Unk28;

    [SchemaField(0x38, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public int Unk30;

    [SchemaField(0x70, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x38, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    [SchemaField(0x50, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S48898080> Unk38;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "48898080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "48898080", 0x18)]
public struct S48898080
{
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer DevBubbleName;

    public TigerHash LocationName;
    public TigerHash ActivityName;
    public StringHash BubbleName;
    public TigerHash ActivityPhaseName;
    public TigerHash ActivityPhaseName2;
    public Tag<S898E8080> UnkEntityReference;
}

[SchemaStruct("898E8080", 0x30)]
public struct S898E8080
{
    public long FileSize;
    public long Unk08;
    public ResourcePointer Unk10;  // 46938080 has dialogue table, 45938080 unk, 19978080 unk
    [SchemaField(0x18)]
    public Tag Unk18;  // S898E8080 entity script stuff
}

[SchemaStruct("46938080", 0x58)]
public struct S46938080
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(0x3C)]
    public int Unk3C;
    public float Unk40;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "19978080", 0x108)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "19978080", 0x20)]
public struct S19978080
{
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk10;

    [SchemaField(0x38, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public DynamicArray<SC39F8080> DirectiveTables;

    [SchemaField(0x48, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer DialogueTableContentString;

    [SchemaField(0x50, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag<SDialogueTable> DialogueTableBL;

    [SchemaField(0x60, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer MusicContentString;

    [SchemaField(0x68, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag<SMusicTemplate> Music;

    [SchemaField(0x90, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer CinematicContentString;

    [SchemaField(0xA8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer CinematicContentString2;

    [SchemaField(0xF0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer DescentContentString2;

    // TODO: re-implement this when Entity is downgraded for beyond light

    //[SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    //[SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    //public Entity.Entity UnkEntityF8;
}

[SchemaStruct("C39F8080", 0x18)]
public struct SC39F8080
{
    // TODO: are these actually obsolete in wq+?
    //[SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer DirectiveTableContentString;
    //[SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag<SC78E8080> DirectiveTable;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "18978080", 0x80)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "18978080", 0x20)]
public struct S18978080 //uhh
{
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk10;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk18;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    [SchemaField(0x1C, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    public Tag<SMusicTemplate> Unk1C;

    //Beyond Light only below

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public int Unk00BL;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public TigerHash Unk04BL;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public TigerHash Unk08BL;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public TigerHash Unk0CBL;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public TigerHash Unk10BL;

    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag Unk28BL;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public DynamicArray<SC39F8080> Unk38BL;

    [SchemaField(0x50, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag Unk50BL;

    [SchemaField(0x68, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag Unk68BL;
}

[SchemaStruct("17978080", 0x20)]
public struct S17978080
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;

    public TigerHash Unk10;

    [SchemaField(0x18)]
    public TigerHash Unk18;

    public int Unk1C;
}

[SchemaStruct("45938080", 0x58)]
public struct S45938080
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(0x18)]
    public DynamicArray<S28998080> Unk18;

    [SchemaField(0x3C)]
    public int Unk3C;

    public float Unk40;
}

[SchemaStruct("44938080", 0x58)]
public struct S44938080
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(0x18)]
    public DynamicArray<S28998080> Unk18;

    [SchemaField(0x3C)]
    public int Unk3C;

    public float Unk40;
    public TigerHash Unk44;

    [SchemaField(0x50)]
    public int Unk50;
}

/// <summary>
/// Generally used in ambients to provide dialogue and music together.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "D5908080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, "D5908080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "D5908080", 0x58)]
public struct SD5908080
{
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_LATEST, Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(0x38, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x14, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    [SchemaField(0x1C, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<SMusicTemplate> Music;

    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S28998080> Unk20;
}

[SchemaStruct("28998080", 0x10)]
public struct S28998080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public TigerHash Unk08;
}

[SchemaStruct("1A978080", 0x18)]
public struct S1A978080
{
    [SchemaField(Tag64 = true)]
    public Tag Unk00;
}

[SchemaStruct("478F8080", 0x18)]
public struct S478F8080
{
    [SchemaField(Tag64 = true)]
    public Tag Unk00;
}

/// <summary>
/// Stores static map data for activities
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "24898080", 0x40)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "24898080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "24898080", 0x48)]
public struct S24898080
{
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer UnkBubbleName;

    public TigerHash LocationName;
    public TigerHash ActivityName;
    public StringHash BubbleName;

    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public ResourcePointer Unk10;  // 0F978080, 53418080
    public DynamicArray<S48898080> Unk18;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag<SBubbleParent> Unk30;

    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<S1D898080> MapReferences;
}

[SchemaStruct("1D898080", 0x10)]
public struct S1D898080
{
    [SchemaField(Tag64 = true)]
    public Tag<SBubbleParent> MapReference;
}

[SchemaStruct("53418080", 0x20)]
public struct S53418080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0xC)]
    public int Unk0C;
}

[SchemaStruct("54418080", 0x40)]
public struct S54418080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0xC)]
    public int Unk0C;
}

[SchemaStruct("0F978080", 0x40)]
public struct S0F978080
{
    public StringPointer BubbleName;
    public TigerHash Unk08;
    public TigerHash Unk0C;
    public TigerHash Unk10;

    [SchemaField(0x28)]
    public long Unk28;
    public DynamicArray<SDD978080> Unk30;
}

[SchemaStruct("DD978080", 0x10)]
public struct SDD978080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public TigerHash Unk08;
}

/// <summary>
/// Directive table + audio links for activity directives.
/// </summary>
/// 
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "6A988080", 0x84)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "6A988080", 0xA4)]
public struct S6A988080
{
    // Idk why these got swapped in EoF
    [SchemaField(0x0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S28898080> DirectiveTables;

    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<SB7978080> DialogueTables;

    public TigerHash StartingBubbleName;
    public TigerHash Unk24;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)] // idk why these are needed when only WQ SchemaStruct is used
    [SchemaField(0x2C, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LATEST)] // EoF x34 -> RNG x30
    public Tag<SMusicTemplate> Music;

    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264)] // Tag64 in Renegades?
    public Tag<SA4BC8080> Music2;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x60, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringPointer DescentMusicPath;

    [SchemaField(Tag64 = true)]
    public Entity.Entity DescentMusic;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x7C, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x84, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag DescentMisc; // C7978080, contains anim clips and models used when loading into destination
}

[SchemaStruct("A4BC8080", 0x18)]
public struct SA4BC8080
{
    [SchemaField(0x8)]
    public DynamicArray<SA6BC8080> Unk08;
}

[SchemaStruct("A6BC8080", 0x18)]
public struct SA6BC8080
{
    [SchemaField(Tag64 = true)]
    public WwiseSound Sound;
}

/// <summary>
/// Directive table for public events so no audio linked.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "20978080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "20978080", 0x40)]
public struct S20978080
{
    // Idk why these got swapped in EoF
    [SchemaField(0x0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S28898080> DirectiveTables;

    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<SB7978080> DialogueTables;

    [SchemaField(0x20)]
    public TigerHash StartingBubbleName;

    [SchemaField(0x2C)]
    public Tag<SMusicTemplate> Music;
}

[SchemaStruct("28898080", 4)]
public struct S28898080
{
    public Tag<SC78E8080> DirectiveTable;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "B7978080", 0x14)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "B7978080", 0x10)]
public struct SB7978080
{
    [SchemaField(Tag64 = true)]
    public Tag<SDialogueTable> DialogueTable;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "80804F72", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "C78E8080", 0x18)]
public struct SC78E8080
{
    public long FileSize;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<SC98E8080> DirectiveTable;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    public DynamicArray<S744F8080> DirectiveTableSK;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "764F8080", 0x24)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "C98E8080", 0x3C)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "C98E8080", 0x80)]
public struct SC98E8080
{
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public TigerHash Hash;

    [SchemaField(0x0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringReference NameStringBL;

    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public StringReference64 NameString;

    [SchemaField(0x8, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringReference DescriptionStringBL;

    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public StringReference64 DescriptionString;

    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringReference ObjectiveStringBL;

    [SchemaField(0x40, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public StringReference64 ObjectiveString;

    [SchemaField(0x18, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringReference Unk58BL;

    [SchemaField(0x58, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public StringReference64 Unk58;

    [SchemaField(0x20, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x38, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public int ObjectiveTargetCount;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "744F8080", 0x28)]
public struct S744F8080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0x10)]
    public DynamicArray<SC98E8080> Directives;
}

#region Audio

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "EB458080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "EB458080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "90BC8080", 0x28)]
public struct SMusicTemplate
{
    public long FileSize;
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Obsolete = true)]
    public StringPointer MusicTemplateName;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag MusicTemplateTag; // F0458080

    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<SED458080> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "ED458080", 8)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "93BC8080", 8)]
public struct SED458080
{
    public ResourcePointer Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "F5458080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "94BC8080", 0x40)]
public struct SF5458080
{
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Obsolete = true)]
    public StringPointer WwiseMusicLoopName;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public WwiseSound MusicLoopSound;

    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    [SchemaField(0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<SFB458080> Unk18;
}

[SchemaStruct("F7458080", 0x28)]
public struct SF7458080
{
    public StringPointer AmbientMusicSetName;
    [SchemaField(0x8, Tag64 = true)]
    public Tag<S50968080> AmbientMusicSet;
    public DynamicArray<SFA458080> Unk18;
}

[SchemaStruct("50968080", 0x20)]
public struct S50968080
{
    public long FileSize;
    public DynamicArray<S318A8080> Unk08;
    public TigerHash Unk18;
}

[SchemaStruct("318A8080", 0x30)]
public struct S318A8080
{
    [SchemaField(Tag64 = true)]
    public WwiseSound MusicLoopSound;
    public float Unk10;
    public TigerHash Unk14;
    public float Unk18;
    public TigerHash Unk1C;
    public float Unk20;
    public TigerHash Unk24;
    public int Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "FA458080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "FA458080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "9CBC8080", 0x30)]
public struct SFA458080
{
    public TigerHash Unk00;
    [SchemaField(8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Obsolete = true)]
    public StringPointer EventName;

    [SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash EventHash;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "FB458080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "FB458080", 0x30)]
public struct SFB458080
{
    public TigerHash Unk00;
    [SchemaField(0x8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Obsolete = true)]
    public StringPointer EventName;

    [SchemaField(0x14, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash EventHash;
}

[SchemaStruct("F0458080", 0x28)]
public struct SF0458080
{
    public long FileSize;
    public int Unk08;
    public int Unk0C;
    public int WwiseSwitchKey;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "E6BF8080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "97BC8080", 0x38)] // I think this is right?
public struct SUnkMusicE6BF8080
{
    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag Unk18;
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<SUnkMusicE8BF8080> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "E8BF8080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, "E8BF8080", 0x40)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "E8BF8080", 0x40)]
public struct SUnkMusicE8BF8080
{
    [SchemaField(0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash EventHash;
    [SchemaField(0x08, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Obsolete = true)]
    public StringPointer EventDescription;
}

[SchemaStruct("BE8E8080", 0x20)]
public struct SBE8E8080
{
    public long FileSize;
    public DynamicArray<S42898080> EntityComponents;
}

[SchemaStruct("42898080", 0x4)]
public struct S42898080
{
    public Tag<S43898080> EntityComponentParent;
}

[SchemaStruct("43898080", 0x28)]
public struct S43898080
{
    public long FileSize;
    public TigerHash Unk08;
    [SchemaField(0x20)]
    public EntityComponent EntityComponent;
}

#endregion
