using Tiger.Schema.Strings;

namespace Tiger.Schema.Audio;

[SchemaStruct("B8978080", 0x28)]
public struct SDialogueTable
{
    public long FileSize;
    public DynamicArray<D2Class_28978080> Unk08;
    public DynamicArray<D2Class_29978080> Unk18;
}

[SchemaStruct("28978080", 8)]
public struct D2Class_28978080
{
    public TigerHash Unk00;
}

[SchemaStruct("29978080", 0x10)]
public struct D2Class_29978080
{
    public TigerHash Unk00;
    [SchemaField(0x8)]
    public ResourcePointer Unk08;
}

/// <summary>
/// Group of D2Class_33978080, used for accessing random sounds to play out of a bundle.
/// </summary>
[SchemaStruct("2F978080", 0x48)]
public struct D2Class_2F978080
{
    [SchemaField(0x10), Tag64]
    public Tag Unk10;
    //[SchemaField(0x1C)]
    public TigerHash Unk20;
    public TigerHash Unk24;
    public TigerHash Unk28;
    public TigerHash Unk2C;
    [SchemaField(0x38)]
    public float Unk38;
    [SchemaField(0x40)]
    public ResourcePointer Unk40; // 2A978080, 2D978080
}

// rest is wrong for latest but the array is correct
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "2A978080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "2A978080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, "2A978080", 0x40)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "2A978080", 0x40)]
public struct D2Class_2A978080
{
    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    public DynamicArray<D2Class_2F978080> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "33978080", 0x68)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "33978080", 0x8C)]
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, "33978080", 0x94)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "33978080", 0xA4)]
public struct D2Class_33978080
{
    // Male
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402), Tag64]
    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307), Tag64]
    [SchemaField(0x20, TigerStrategy.DESTINY2_LIGHTFALL_7366), Tag64]
    public WwiseSound SoundM;

    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringReference VoicelineM_BL;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    public StringReference64 VoicelineM;

    // Female
    [SchemaField(0x38, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_WITCHQUEEN_6307), Tag64]
    [SchemaField(0x50, TigerStrategy.DESTINY2_LIGHTFALL_7366), Tag64]
    [SchemaField(0x58, TigerStrategy.DESTINY2_LATEST), Tag64]
    public WwiseSound SoundF;

    [SchemaField(0x48, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringReference VoicelineF_BL;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    [SchemaField(0x58, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x60, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_LATEST)]
    public StringReference64 VoicelineF;

    [SchemaField(0x5C, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x7C, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x84, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    [SchemaField(0x94, TigerStrategy.DESTINY2_LATEST)]
    public StringHash NarratorString;


    public string GetVoiceline()
    {
        if (Strategy.IsBL())
            return VoicelineM_BL.Value.ToString();
        else
            return VoicelineM.Value.ToString();
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "2D978080", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "2D978080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, "2D978080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "2D978080", 0x38)]
public struct D2Class_2D978080
{
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    public DynamicArray<D2Class_30978080> Unk20;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "30978080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "30978080", 0x28)]
public struct D2Class_30978080
{
    [Tag64, SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag Unk00;
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk10;
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk14;
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk18;
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk1C;
    public ResourcePointer Unk20; //33978080 or 2A978080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "0A088080", 0x58)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "02988080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "38978080", 0x38)]
public struct D2Class_38978080
{
    public long FileSize;
    public StringHash SoundbankName;

    [SchemaField(0x34, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public BKHD SoundbankBL;

    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag<D2Class_63838080> SoundbankWQ;

    [SchemaField(0x38, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicArray<Wem> Wems;
}

[SchemaStruct("418A8080", 0x38)]
public struct D2Class_418A8080
{
    public long Unk00;
    public float Unk08;
}

[SchemaStruct("63838080", 4)]
public struct D2Class_63838080
{
    public BKHD SoundBank;
}

[SchemaStruct("438A8080", 0x28)]
public struct D2Class_438A8080
{
    public long FileSize;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "AA078080", 0x3C)]
public struct SAA078080
{
    [SchemaField(0x20)]
    public StringHash Narrator;

    // Male
    public Tag<D2Class_38978080> Dialogue;
    public LocalizedStrings Strings;
    public StringHash VoiceLine;

    // Female
    public Tag<D2Class_38978080> DialogueF;
    public LocalizedStrings StringsF;
    public StringHash VoiceLineF;
}




