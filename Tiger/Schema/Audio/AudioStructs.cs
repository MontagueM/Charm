using Tiger.Schema.Strings;

namespace Tiger.Schema.Audio;

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808D54, 0x28)] //80808D54
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808097B8, 0x28)] //B8978080
public struct SDialogueTable
{
    public long FileSize;
    public DynamicArray<S80809728> Unk08;
    public DynamicArray<S80809729> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808D18, 8)] //188D8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809728, 8)] //28978080
public struct S80809728
{
    public TigerHash Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808D19, 0x10)] //198D8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809729, 0x10)] //29978080
public struct S80809729
{
    public TigerHash Unk00;
    [SchemaField(0x8)]
    public ResourcePointer Unk08;
}

/// <summary>
/// Group of S80809733, used for accessing random sounds to play out of a bundle.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808D1F, 0x38)] //1F8D8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080972F, 0x48)] //2F978080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080972F, 0x40)] //2F978080
public struct S8080972F
{
    [SchemaField(0x30, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x38, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk40; // 2A978080, 2D978080
}

// rest is wrong for latest but the array is correct
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808D1A, 0x30)] //1A8D8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080972A, 0x30)] //2A978080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x8080972A, 0x38)] //2A978080
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, 0x8080972A, 0x40)] //2A978080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080972A, 0x40)] //2A978080
public struct S8080972A
{
    [SchemaField(0x20, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    public DynamicArray<S8080972F> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808D23, 0x44)] //238D8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809733, 0x68)] //33978080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80809733, 0x8C)] //33978080
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, 0x80809733, 0x94)] //33978080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80809733, 0xA4)] //33978080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80809733, 0xA8)] //33978080
public struct S80809733
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

    [SchemaField(0x3C, TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(0x88, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    [SchemaField(0x98, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringHash NarratorString2;

    public string GetVoiceline()
    {
        if (Strategy.IsBL() || Strategy.IsPreBL())
            return VoicelineM_BL.Value.ToString();
        else
            return VoicelineM.Value.ToString();
    }

    public string GetNarratorString()
    {
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_LIGHTFALL_7366)
        {
            if (NarratorString2.IsInvalid() || !GlobalStrings.Get().CheckString(NarratorString2))
                return GlobalStrings.Get().GetString(NarratorString);
            else
                return GlobalStrings.Get().GetString(NarratorString2);
        }

        return GlobalStrings.Get().GetString(NarratorString);
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080972D, 0x28)] //2D978080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x8080972D, 0x30)] //2D978080
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, 0x8080972D, 0x38)] //2D978080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080972D, 0x38)] //2D978080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080972D, 0x40)] //2D978080
public struct S8080972D
{
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S80809730> Unk20;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809730, 0x8)] //30978080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80809730, 0x28)] //30978080
public struct S80809730
{
    [SchemaField(0x0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public ResourcePointer Unk20; //33978080 or 2A978080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080080A, 0x58)] //0A088080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809802, 0x38)] //02988080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809738, 0x38)] //38978080
public struct S80809738
{
    public long FileSize;
    public StringHash SoundbankName;

    [SchemaField(0x34, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x14, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_LATEST)]
    public BKHD SoundbankBL; // D1 - BL, EoF

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(TigerStrategy.DESTINY2_LATEST, Obsolete = true)]
    public Tag<S80808363> SoundbankWQ; // WQ - TFS

    [SchemaField(0x38, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<Wem> Wems;

    public BKHD GetSoundbank()
    {
        if (Strategy.IsLatest() || Strategy.IsBL() || Strategy.IsPreBL() || Strategy.IsD1())
            return SoundbankBL;
        else
            return SoundbankWQ.TagData.SoundBank;
    }
}

[SchemaStruct(0x80808363, 4)] //63838080
public struct S80808363
{
    public BKHD SoundBank;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808007AA, 0x3C)] //AA078080
public struct S808007AA
{
    [SchemaField(0x20)]
    public StringHash Narrator;

    // Male
    public Tag<S80809738> Dialogue;
    public LocalizedStrings Strings;
    public StringHash VoiceLine;

    // Female
    public Tag<S80809738> DialogueF;
    public LocalizedStrings StringsF;
    public StringHash VoiceLineF;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808D1D, 0x2C)] //1D8D8080
public struct S80808D1D
{
    [SchemaField(0x18)]
    public DynamicArray<S80808D20> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808D20, 0x8)] //208D8080
public struct S80808D20
{
    public ResourcePointer Pointer; // 238D8080, 1A8D8080
}

// I think this is used for the interactive text popups introduced in EoF
// Idk why they are in dialogue tables though
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080B6CE, 0x64)] //CEB68080
public struct S8080B6CE
{

}




