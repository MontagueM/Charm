using Tiger.Schema.Audio;
using Tiger.Schema.Entity;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808E8E, 0xB4)] //8E8E8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80808E8E, 0x78)] //8E8E8080
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, 0x80808E8E, 0x88)] //8E8E8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80808E8E, 0x88)] //8E8E8080
public struct SActivity_WQ
{
    public long FileSize;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer ActivityName;

    [SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public StringHash LocationName;  // these all have actual string hashes but have no string container given directly
    public TigerHash Unk0C;
    public TigerHash Unk10;
    public TigerHash Unk14;
    public ResourcePointer Unk18;  // 6A988080 + 20978080 (+ 19978080, beyondlight)
    public FileHash64 Destination;  // S80808E8B

    //[SchemaField(0x30, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)] // Doesnt look useful?
    //public DynamicArray<S80809700> Unk30;

    [SchemaField(0x70, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<S80808926> Unk40;
    public DynamicArray<S80808924> Unk50;

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

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808E8B, 0xD0)] //8B8E8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80808E8B, 0x78)] //8B8E8080
public struct S80808E8B
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
    public DynamicArrayUnloaded<S808044DE> TagBags;

    [SchemaField(0xB8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArrayUnloaded<S8080892E> Activities;
    public StringPointer DestinationName;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x808044DE, 4)] //DE448080
public struct S808044DE
{
    public Tag Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080892E, 0x18)] //2E898080
public struct S8080892E
{
    public TigerHash ShortActivityName;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    public TigerHash Unk10;
    public StringPointer ActivityName;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808926, 0x90)] //26898080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80808926, 0x58)] //26898080
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, 0x80808926, 0x68)] //26898080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80808926, 0x70)] //26898080
public struct S80808926
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
    public DynamicArray<S80808948> Unk38;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808948, 0x20)] //48898080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80808948, 0x18)] //48898080
public struct S80808948
{
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer DevBubbleName;

    public TigerHash LocationName;
    public TigerHash ActivityName;
    public StringHash BubbleName;
    public TigerHash ActivityPhaseName;
    public TigerHash ActivityPhaseName2;
    public Tag<S80808E89> UnkEntityReference;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808E89, 0x30)] //898E8080
public struct S80808E89
{
    public long FileSize;
    public long Unk08;
    public ResourcePointer Unk10;  // 46938080 has dialogue table, 45938080 unk, 19978080 unk
    [SchemaField(0x18)]
    public Tag Unk18;  // S80808E89 entity script stuff
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809346, 0x58)]//46938080
public struct S80809346
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(0x3C)]
    public int Unk3C;
    public float Unk40;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809719, 0x108)] //19978080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80809719, 0x20)] //19978080
public struct S80809719
{
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk10;

    [SchemaField(0x38, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public DynamicArray<S80809FC3> DirectiveTables;

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

[SchemaStruct(0x80809FC3, 0x18)] //C39F8080
public struct S80809FC3
{
    // TODO: are these actually obsolete in wq+?
    //[SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer DirectiveTableContentString;
    //[SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag<S80808EC7> DirectiveTable;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809718, 0x80)] //18978080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80809718, 0x20)] //18978080
public struct S80809718 //uhh
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
    public DynamicArray<S80809FC3> Unk38BL;

    [SchemaField(0x50, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag Unk50BL;

    [SchemaField(0x68, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag Unk68BL;
}

[SchemaStruct(0x80809717, 0x20)] //17978080
public struct S80809717
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;

    public TigerHash Unk10;

    [SchemaField(0x18)]
    public TigerHash Unk18;

    public int Unk1C;
}

[SchemaStruct(0x80809345, 0x58)] //45938080
public struct S80809345
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(0x18)]
    public DynamicArray<S80809928> Unk18;

    [SchemaField(0x3C)]
    public int Unk3C;

    public float Unk40;
}

[SchemaStruct(0x80809344, 0x58)] //44938080
public struct S80809344
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(0x18)]
    public DynamicArray<S80809928> Unk18;

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
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x808090D5, 0x50)] //D5908080
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, 0x808090D5, 0x50)] //D5908080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808090D5, 0x58)] //D5908080
public struct S808090D5
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
    public DynamicArray<S80809928> Unk20;
}

[SchemaStruct(0x80809928, 0x10)] //28998080
public struct S80809928
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public TigerHash Unk08;
}

[SchemaStruct(0x8080971A, 0x18)] //1A978080
public struct S8080971A
{
    [SchemaField(Tag64 = true)]
    public Tag Unk00;
}

[SchemaStruct(0x80808F47, 0x18)] //478F8080
public struct S80808F47
{
    [SchemaField(Tag64 = true)]
    public Tag Unk00;
}

/// <summary>
/// Stores static map data for activities
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808924, 0x40)] //24898080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80808924, 0x38)] //24898080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80808924, 0x48)] //24898080
public struct S80808924
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
    public DynamicArray<S80808948> Unk18;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag<SBubbleParent> Unk30;

    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<S8080891D> MapReferences;
}

[SchemaStruct(0x8080891D, 0x10)] //1D898080
public struct S8080891D
{
    [SchemaField(Tag64 = true)]
    public Tag<SBubbleParent> MapReference;
}

[SchemaStruct(0x80804153, 0x20)] //53418080
public struct S80804153
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0xC)]
    public int Unk0C;
}

[SchemaStruct(0x80804154, 0x40)] //54418080
public struct S80804154
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0xC)]
    public int Unk0C;
}

[SchemaStruct(0x8080970F, 0x40)] //0F978080
public struct S8080970F
{
    public StringPointer BubbleName;
    public TigerHash Unk08;
    public TigerHash Unk0C;
    public TigerHash Unk10;

    [SchemaField(0x28)]
    public long Unk28;
    public DynamicArray<S808097DD> Unk30;
}

[SchemaStruct(0x808097DD, 0x10)] //DD978080
public struct S808097DD
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public TigerHash Unk08;
}

/// <summary>
/// Directive table + audio links for activity directives.
/// </summary>
/// 
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x8080986A, 0x84)] //6A988080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080986A, 0xA4)] //6A988080
public struct S8080986A
{
    // Idk why these got swapped in EoF
    [SchemaField(0x0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S80808928> DirectiveTables;

    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S808097B7> DialogueTables;

    public TigerHash StartingBubbleName;
    public TigerHash Unk24;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)] // idk why these are needed when only WQ SchemaStruct is used
    [SchemaField(0x2C, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LATEST)] // EoF x34 -> RNG x30
    public Tag<SMusicTemplate> Music;

    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264)] // Tag64 in Renegades?
    public Tag<S8080BCA4> Music2;

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

[SchemaStruct(0x8080BCA4, 0x18)] //A4BC8080
public struct S8080BCA4
{
    [SchemaField(0x8)]
    public DynamicArray<S8080BCA6> Unk08;
}

[SchemaStruct(0x8080BCA6, 0x18)] //A6BC8080
public struct S8080BCA6
{
    [SchemaField(Tag64 = true)]
    public WwiseSound Sound;
}

/// <summary>
/// Directive table for public events so no audio linked.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80809720, 0x38)] //20978080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80809720, 0x40)] //20978080
public struct S80809720
{
    // Idk why these got swapped in EoF
    [SchemaField(0x0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S80808928> DirectiveTables;

    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S808097B7> DialogueTables;

    [SchemaField(0x20)]
    public TigerHash StartingBubbleName;

    [SchemaField(0x2C)]
    public Tag<SMusicTemplate> Music;
}

[SchemaStruct(0x80808928, 4)] //28898080
public struct S80808928
{
    public Tag<S80808EC7> DirectiveTable;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808097B7, 0x14)] //B7978080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808097B7, 0x10)] //B7978080
public struct S808097B7
{
    [SchemaField(Tag64 = true)]
    public Tag<SDialogueTable> DialogueTable;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80804F72, 0x18)] //80804F72
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808EC7, 0x18)] //C78E8080
public struct S80808EC7
{
    public long FileSize;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<S80808EC9> DirectiveTable;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    public DynamicArray<S80804F74> DirectiveTableSK;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80804F76, 0x24)] //764F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808EC9, 0x3C)] //C98E8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80808EC9, 0x80)] //C98E8080
public struct S80808EC9
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

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80804F74, 0x28)] //744F8080
public struct S80804F74
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0x10)]
    public DynamicArray<S80808EC9> Directives;
}

#region Audio

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808045EB, 0x38)] //EB458080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x808045EB, 0x38)] //EB458080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080BC90, 0x28)] //90BC8080
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
    public DynamicArray<S808045ED> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808045ED, 8)] //ED458080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080BC93, 8)] //93BC8080
public struct S808045ED
{
    public ResourcePointer Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808045F5, 0x30)] //F5458080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080BC94, 0x40)] //94BC8080
public struct S808045F5
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
    public DynamicArray<S808045FB> Unk18;
}

[SchemaStruct(0x808045F7, 0x28)] //F7458080
public struct S808045F7
{
    public StringPointer AmbientMusicSetName;
    [SchemaField(0x8, Tag64 = true)]
    public Tag<S80809650> AmbientMusicSet;
    public DynamicArray<S808045FA> Unk18;
}

[SchemaStruct(0x80809650, 0x20)] //50968080
public struct S80809650
{
    public long FileSize;
    public DynamicArray<S80808A31> Unk08;
    public TigerHash Unk18;
}

[SchemaStruct(0x80808A31, 0x30)] //318A8080
public struct S80808A31
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

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808045FA, 0x18)] //FA458080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x808045FA, 0x20)] //FA458080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080BC9C, 0x30)] //9CBC8080
public struct S808045FA
{
    public TigerHash Unk00;
    [SchemaField(8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Obsolete = true)]
    public StringPointer EventName;

    [SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash EventHash;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808045FB, 0x20)] //FB458080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808045FB, 0x30)] //FB458080
public struct S808045FB
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

[SchemaStruct(0x808045F0, 0x28)] //F0458080
public struct S808045F0
{
    public long FileSize;
    public int Unk08;
    public int Unk0C;
    public int WwiseSwitchKey;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x8080BFE6, 0x38)] //E6BF8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080BC97, 0x38)] //97BC8080 // I think this is right?
public struct SUnkMusicE6BF8080
{
    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag Unk18;
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<SUnkMusicE8BF8080> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x8080BFE8, 0x30)] //E8BF8080
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, 0x8080BFE8, 0x40)] //E8BF8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080BFE8, 0x40)] //E8BF8080
public struct SUnkMusicE8BF8080
{
    [SchemaField(0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash EventHash;
    [SchemaField(0x08, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Obsolete = true)]
    public StringPointer EventDescription;
}

[SchemaStruct(0x80808EBE, 0x20)] //BE8E8080
public struct S80808EBE
{
    public long FileSize;
    public DynamicArray<S80808942> EntityComponents;
}

[SchemaStruct(0x80808942, 0x4)] //42898080
public struct S80808942
{
    public Tag<S80808943> EntityComponentParent;
}

[SchemaStruct(0x80808943, 0x28)] //43898080
public struct S80808943
{
    public long FileSize;
    public TigerHash Unk08;
    [SchemaField(0x20)]
    public EntityComponent EntityComponent;
}

#endregion
