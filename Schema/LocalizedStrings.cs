using System.Text;
using Tiger;

namespace Schema;

public struct TigerString
{
    public StringHash Hash { get; }
    public string RawValue { get; }
    
    public static TigerString Empty => new(new StringHash(), "");

    public TigerString(StringHash hash, string value)
    {
        RawValue = value;
    }
    
    public override string ToString()
    {
        return RawValue;
    }
}

public class LocalizedStrings : Tag<SLocalizedStrings>
{
    public LocalizedStrings(FileHash hash) : base(hash)
    {
    }

    public TigerString GetStringFromHash(StringHash hash)
    {
        int index = FindIndexOfStringHash(hash);
        if (index == -1)
        {
            throw new Exception($"Could not find string with hash {hash}");
        }
        return new TigerString(hash, _tag.EnglishStringsData.GetStringFromIndex(index));
    }
    
    private int FindIndexOfStringHash(StringHash hash)
    {
        using (TigerReader reader = GetReader())
        {
            return _tag.StringHashes.InterpolationSearchIndex(reader, hash);
        }
    }
    
    
}

public class LocalizedStringsData : Tag<SLocalizedStringsData>
{
    // Don't parse as we do it via index-access
    public LocalizedStringsData(FileHash hash) : base(hash)
    {
    }
    
    /// <summary>
    /// Given the index of a string, returns the string.
    /// </summary>
    /// <param name="stringIndex">The index of the string to retrieve, where the index can be found from the hash table of the string bank.</param>
    /// <returns>The string of the index given.</returns>
    public string GetStringFromIndex(int stringIndex)
    {
        List<string> strings;
        using (TigerReader reader = GetReader())
        {
            SStringPartDefinition combination = _tag.StringCombinations.ElementAt(reader, stringIndex);
            strings = ParseStringParts(reader, combination);
        }

        return string.Join("", strings.ToArray());
    }

    private string GetStringFromPart(TigerReader reader, SStringPart part)
    {
        reader.Seek(part.StringPartDefinitionPointer.AbsoluteOffset, SeekOrigin.Begin);
        // int dataOffset = (int) (part.StringDataPointer - Header.StringParts[0].StringDataPointer);
        StringBuilder builder = new StringBuilder();
        int c = 0;
        while (c < part.ByteLength)
        {
            // byte[] sectionData = ReadChars(dataOffset+c, 3);
            byte[] sectionData = reader.ReadBytes(3);
            int val = sectionData[0];
            if (val >= 0xC0 && val <= 0xDF)  // 2 byte unicode
            {
                var rawBytes = BitConverter.ToUInt32(Encoding.Convert(Encoding.UTF8, Encoding.UTF32, sectionData));
                builder.Append(Convert.ToChar(rawBytes));
                c += 2;
                reader.Seek(-1, SeekOrigin.Current);
            }
            else if (val >= 0xE0 && val <= 0xEF)  // 3 byte unicode
            {
                var rawBytes = BitConverter.ToUInt32(Encoding.Convert(Encoding.UTF8, Encoding.UTF32, sectionData));
                builder.Append(Convert.ToChar(rawBytes));
                c += 3;
            }
            else
            {
                builder.Append(Encoding.UTF8.GetString(new [] { CipherShift(sectionData[0], part) }));
                c += 1;
                reader.Seek(-2, SeekOrigin.Current);
            }
        }
        
        return builder.ToString();
    }
    
    private byte CipherShift(byte c, SStringPart part)
    {
        int val = c + part.CipherShift;
        return (byte)val;
    }
    
    private List<string> ParseStringParts(TigerReader reader, SStringPartDefinition stringPart)
    {
        int stringPartSize = SchemaDeserializer.Get().GetSchemaStructSize<SStringPart>();
        int partStartIndex = (int)(stringPart.StartStringPartPointer.AbsoluteOffset - _tag.StringParts.Offset.AbsoluteOffset) / stringPartSize; // this is bad as magic numbers but means we dont parse multiple times

        List<string> strings = new List<string>();
        for (int i = 0; i < stringPart.PartCount; i++)
        {
            strings.Add(GetStringFromPart(reader, _tag.StringParts.ElementAt(reader, partStartIndex+i)));
        }   


        return strings;
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "319A8080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307,"EF998080", 0x50)]
public struct SLocalizedStrings
{
    public ulong ThisSize;
    public SortedDynamicArray<SStringHash> StringHashes;
    // [DestinyField(FieldType.TagHash), MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
    // public StringData[] StringData;
    // [SchemaField(FieldType.TagHash)]  // only working with english rn for speed
    public LocalizedStringsData EnglishStringsData;  // actually StringData class
}

[SchemaStruct("70008080", 0x4)]
public struct SStringHash
{
    public TigerHash StringHash;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "8A9A8080", 0x58)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "F1998080", 0x58)]
public struct SLocalizedStringsData
{
    public long ThisSize;
    public DynamicArray<SStringPart> StringParts;
    // might be a colour table here
    [SchemaField(0x38, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<SStringCharacter> StringCharacters;
    public DynamicArray<SStringPartDefinition> StringCombinations;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "909A8080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "F7998080", 0x20)]
public struct SStringPart
{
    [SchemaField(0x8)]
    public RelativePointer StringPartDefinitionPointer; // this doesn't get accessed so no need to make this easy to access
    // public DestinyHash Unk10;
    [SchemaField(0x14)]
    public ushort ByteLength;  // these can differ if multibyte unicode
    public ushort StringLength;
    public ushort CipherShift;  // now always zero
}
    
[SchemaStruct("05008080", 0x01)]
public struct SStringCharacter
{
    public byte Character;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "8E9A8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "F5998080", 0x10)]
public struct SStringPartDefinition
{
    public RelativePointer StartStringPartPointer;
    public long PartCount;
}
//     
//     
// // no-container string caching
//
// [StructLayout(LayoutKind.Sequential, Size = 0x68)]
// public struct D2Class_02218080
// {
//     public long FileSize;
//     // public long Unk08;
//     // [DestinyField(FieldType.TagHash)] 
//     // public Tag Unk10; // 0D3C8080
//     // [DestinyField(FieldType.TagHash)] 
//     // public Tag Unk14;
//     // [DestinyField(FieldType.TablePointer)]
//     // public List<D2Class_133C8080> Unk18;
//     [DestinyOffset(0x28), DestinyField(FieldType.TablePointer)]
//     public List<D2Class_0E3C8080> Unk28;
//     // [DestinyField(FieldType.TagHash)] 
//     // public Tag Unk38;
//     // [DestinyField(FieldType.TagHash)] 
//     // public Tag Unk3C;
//     // [DestinyField(FieldType.TagHash)] 
//     // public Tag Unk40;
//     // [DestinyField(FieldType.TagHash)] 
//     // public Tag Unk44;
//     // [DestinyField(FieldType.TagHash)] 
//     // public Tag Unk48;
//     // [DestinyField(FieldType.TagHash)] 
//     // public Tag Unk4C;
//     // [DestinyField(FieldType.TagHash)] 
//     // public Tag Unk50;
//     // [DestinyOffset(0x58), DestinyField(FieldType.TablePointer)]
//     // public List<D2Class_04218080> Unk58;
// }
//     
// [StructLayout(LayoutKind.Sequential, Size = 0x18)]
// public struct D2Class_133C8080
// {
//     public DestinyHash Unk00;
//     [DestinyOffset(0x8), DestinyField(FieldType.TablePointer)]
//     private List<D2Class_153C8080> Unk08;
// }
//     
// [StructLayout(LayoutKind.Sequential, Size = 8)]
// public struct D2Class_153C8080
// {
//     public DestinyHash Unk00;
//     public Tag Unk08;
// }
//     
// [StructLayout(LayoutKind.Sequential, Size = 0x28)]
// public struct D2Class_0E3C8080
// {
//     [DestinyField(FieldType.TagHash64)]
//     public Tag Unk00;
//     [DestinyField(FieldType.TagHash64)]
//     public StringContainer Unk10;  // this can be a string container
// }
//     
// [StructLayout(LayoutKind.Sequential, Size = 0x50)]
// public struct D2Class_04218080
// {
//     // this causes a duplication of the entire table, dunno why it does this but i wont bother
// }