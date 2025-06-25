
namespace Tiger.Schema.Strings;

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "5A038080", 0x48)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "889A8080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "EF998080", 0x50)]
public struct SLocalizedStrings
{
    public ulong ThisSize;
    public SortedDynamicArray<SStringHash> StringHashes;
    // [DestinyField(FieldType.FileHash), MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
    // public StringData[] StringData;
    // [SchemaField(FieldType.FileHash)]  // only working with english rn for speed
    public LocalizedStringsData EnglishStringsData;    // actually StringData class
}

[SchemaStruct("70008080", 0x4)]
public struct SStringHash
{
    public StringHash StringHash;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "BE088080", 0x58)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "8A9A8080", 0x58)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "F1998080", 0x58)]
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, "F1998080", 0x48)]
public struct SLocalizedStringsData
{
    public long ThisSize;
    public DynamicArrayUnloaded<SStringPart> StringParts;
    // might be a colour table here
    [SchemaField(0x38, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArrayUnloaded<SInt8> StringCharacters;
    public DynamicArrayUnloaded<SStringPartDefinition> StringCombinations;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "909A8080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "909A8080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "F7998080", 0x20)]
public struct SStringPart
{
    [SchemaField(0x8)]
    public RelativePointer StringPartDefinitionPointer;    // this doesn't get accessed so no need to make this easy to access
    // public TigerHash Unk10;
    [SchemaField(0x14)]
    public ushort ByteLength;    // these can differ if multibyte unicode
    public ushort StringLength;
    public ushort CipherShift;    // now always zero
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "3E038080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "8E9A8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "F5998080", 0x10)]
public struct SStringPartDefinition
{
    public RelativePointer StartStringPartPointer;
    public long PartCount;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "50058080", 0x10)]
public struct S50058080
{
    [SchemaField(0x68)]
    public LocalizedStrings ActivityGlobalStrings; // content\activities\strings\activity_global_strings.localized_strings.tft
    [SchemaField(0xE8)]
    public LocalizedStrings CharacterNames; // content\sandbox\strings\character_names.localized_strings.tft
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "B7478080", 0x58)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "808021F7", 0x68)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "02218080", 0x68)]
public struct S02218080
{
    [SchemaField(0x20, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<S0E3C8080> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "C6478080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "0E3C8080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "0E3C8080", 0x28)]
public struct S0E3C8080
{
    [SchemaField(0x0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Tag Unk00;

    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag Unk10; // Can be string container or something else
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "8080760A", 0x10)]
public struct S8080760A
{
    [SchemaField(0x8)]
    public Tag Container;
}
