using Tiger.Schema.Strings;

namespace Tiger.Schema.Audio;

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "80808D54", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "B8978080", 0x28)]
public struct SDialogueTable
{
    public long FileSize;
    public DynamicArray<S28978080> Unk08;
    public DynamicArray<S29978080> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "188D8080", 8)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "28978080", 8)]
public struct S28978080
{
    public TigerHash Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "198D8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "29978080", 0x10)]
public struct S29978080
{
    public TigerHash Unk00;
    [SchemaField(0x8)]
    public ResourcePointer Unk08;
}

/// <summary>
/// Group of S33978080, used for accessing random sounds to play out of a bundle.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "1F8D8080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "2F978080", 0x48)]
public struct S2F978080
{
    [SchemaField(0x30, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public ResourcePointer Unk40; // 2A978080, 2D978080
}

// rest is wrong for latest but the array is correct
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "1A8D8080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "2A978080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "2A978080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, "2A978080", 0x40)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "2A978080", 0x40)]
public struct S2A978080
{
    [SchemaField(0x20, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    public DynamicArray<S2F978080> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "238D8080", 0x44)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "33978080", 0x68)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "33978080", 0x8C)]
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, "33978080", 0x94)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "33978080", 0xA4)]
public struct S33978080
{
    // Male
    [SchemaField(0x18, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_LIGHTFALL_7366, Tag64 = true)]
    public WwiseSound SoundM;

    [SchemaField(0x1C, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringReference VoicelineM_BL;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    public StringReference64 VoicelineM;

    // Female
    //[SchemaField(0x38, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    //[SchemaField(0x48, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    //[SchemaField(0x50, TigerStrategy.DESTINY2_LIGHTFALL_7366, Tag64 = true)]
    //[SchemaField(0x58, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    //public WwiseSound SoundF;

    //[SchemaField(0x48, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    //[SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    //public StringReference VoicelineF_BL;

    //[SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    //[SchemaField(0x58, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    //[SchemaField(0x60, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    //[SchemaField(0x68, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    //public StringReference64 VoicelineF;

    [SchemaField(0x3C, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x5C, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x7C, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x84, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    [SchemaField(0x94, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringHash NarratorString;


    public string GetVoiceline()
    {
        if (Strategy.IsBL() || Strategy.IsPreBL())
            return VoicelineM_BL.Value.ToString();
        else
            return VoicelineM.Value.ToString();
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "2D978080", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "2D978080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, "2D978080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "2D978080", 0x38)]
public struct S2D978080
{
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    public DynamicArray<S30978080> Unk20;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "30978080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "30978080", 0x28)]
public struct S30978080
{
    [SchemaField(0x0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public ResourcePointer Unk20; //33978080 or 2A978080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "0A088080", 0x58)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "02988080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "38978080", 0x38)]
public struct S38978080
{
    public long FileSize;
    public StringHash SoundbankName;

    [SchemaField(0x34, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x14, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public BKHD SoundbankBL;

    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag<S63838080> SoundbankWQ;

    [SchemaField(0x38, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<Wem> Wems;
}

[SchemaStruct("418A8080", 0x38)]
public struct S418A8080
{
    public long Unk00;
    public float Unk08;
}

[SchemaStruct("63838080", 4)]
public struct S63838080
{
    public BKHD SoundBank;
}

[SchemaStruct("438A8080", 0x28)]
public struct S438A8080
{
    public long FileSize;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "AA078080", 0x3C)]
public struct SAA078080
{
    [SchemaField(0x20)]
    public StringHash Narrator;

    // Male
    public Tag<S38978080> Dialogue;
    public LocalizedStrings Strings;
    public StringHash VoiceLine;

    // Female
    public Tag<S38978080> DialogueF;
    public LocalizedStrings StringsF;
    public StringHash VoiceLineF;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "1D8D8080", 0x2C)]
public struct S1D8D8080
{
    [SchemaField(0x18)]
    public DynamicArray<S208D8080> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "208D8080", 0x8)]
public struct S208D8080
{
    public ResourcePointer Pointer; // 238D8080, 1A8D8080
}





