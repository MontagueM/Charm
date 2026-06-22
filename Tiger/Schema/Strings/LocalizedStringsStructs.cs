
namespace Tiger.Schema.Strings;

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080035A, 0x48)] //5A038080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809A88, 0x50)] //889A8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808099EF, 0x50)] //EF998080
public struct SLocalizedStrings
{
    public ulong ThisSize;
    public SortedDynamicArray<SStringHash> StringHashes;
    // [DestinyField(FieldType.FileHash), MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
    // public StringData[] StringData;
    // [SchemaField(FieldType.FileHash)]  // only working with english rn for speed
    public LocalizedStringsData EnglishStringsData;    // actually StringData class
}

[SchemaStruct(0x80800070, 0x4)]//70008080
public struct SStringHash
{
    public StringHash StringHash;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808008BE, 0x58)] //BE088080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809A8A, 0x58)] //8A9A8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808099F1, 0x58)] //F1998080
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, 0x808099F1, 0x48)] //F1998080
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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80809A90, 0x20)] //909A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809A90, 0x20)] //909A8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x808099F7, 0x20)] //F7998080
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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080033E, 0x10)] //3E038080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809A8E, 0x10)] //8E9A8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x808099F5, 0x10)] //F5998080
public struct SStringPartDefinition
{
    public RelativePointer StartStringPartPointer;
    public long PartCount;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800550, 0x10)] //50058080
public struct S80800550
{
    [SchemaField(0x68)]
    public LocalizedStrings ActivityGlobalStrings; // content\activities\strings\activity_global_strings.localized_strings.tft
    [SchemaField(0xE8)]
    public LocalizedStrings CharacterNames; // content\sandbox\strings\character_names.localized_strings.tft
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808047B7, 0x58)] //B7478080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808021F7, 0x68)] //808021F7
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80802102, 0x68)] //02218080
public struct S80802102
{
    [SchemaField(0x20, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<S80803C0E> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808047C6, 0x8)] //C6478080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80803C0E, 0x8)] //0E3C8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80803C0E, 0x28)] //0E3C8080
public struct S80803C0E
{
    [SchemaField(0x0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Tag Unk00;

    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag Unk10; // Can be string container or something else
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080760A, 0x10)] //8080760A
public struct S8080760A
{
    [SchemaField(0x8)]
    public Tag Container;
}
