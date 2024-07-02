using System.Text;

namespace Tiger.Schema.Strings;

public struct TigerString
{
    public StringHash Hash { get; }
    public string RawValue { get; }

    public static TigerString Empty => new(new StringHash(), "");

    public TigerString(string value)
    {
        Hash = StringHash.Invalid;
        RawValue = value;
    }

    public TigerString(StringHash hash, string value)
    {
        Hash = hash;
        RawValue = value;
    }

    public override string ToString()
    {
        return RawValue;
    }

    public static implicit operator string(TigerString tigerString)
    {
        return tigerString.RawValue;
    }
}

public struct LocalizedStringView
{
    public FileHash ParentFileHash;
    public FileHash DataFileHash;
    public int StringIndex;
    public StringHash StringHash;
    public string RawString;
}

public class LocalizedStrings : Tag<SLocalizedStrings>
{
    public LocalizedStrings(FileHash hash) : base(hash) { }

    public LocalizedStrings(FileHash hash, bool shouldLoad) : base(hash, shouldLoad) { }

    public TigerString GetStringFromHash(StringHash hash)
    {
        int index = FindIndexOfStringHash(hash);
        if (index == -1)
        {
            // Log.Error($"Could not find string with hash {hash}");
            return new TigerString($"NotFound-{hash}");
        }
        return new TigerString(hash, _tag.EnglishStringsData.GetStringFromIndex(index));
    }

    private int FindIndexOfStringHash(StringHash hash)
    {
        using TigerReader reader = GetReader();
        if (_tag.StringHashes is null) // idk why this happens but its so annoying
            Deserialize(true);

        return _tag.StringHashes.InterpolationSearchIndex(reader, hash);
    }

    public List<LocalizedStringView> GetAllStringViews()
    {
        using (TigerReader reader = GetReader())
        {
            if (_tag.StringHashes is null)
                Deserialize(true);

            return _tag.StringHashes
                .Select(reader, (hash, index) => new TigerString(hash.StringHash, _tag.EnglishStringsData.GetStringFromIndex(index)))
                .Select((hash, index) => new LocalizedStringView
                {
                    ParentFileHash = Hash,
                    DataFileHash = _tag.EnglishStringsData.Hash,
                    StringIndex = index,
                    StringHash = hash.Hash,
                    RawString = hash.RawValue
                }).ToList();
        }
    }
}

public class LocalizedStringsData : Tag<SLocalizedStringsData>
{
    // Don't parse as we do it via index-access
    public LocalizedStringsData(FileHash hash) : base(hash) { }

    /// <summary>
    /// Given the index of a string, returns the string.
    /// </summary>
    /// <param name="stringIndex">The index of the string to retrieve, where the index can be found from the hash table of the string
    /// bank.</param> <returns>The string of the index given.</returns>
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
        StringBuilder builder = new();
        int c = 0;
        while (c < part.ByteLength)
        {
            byte[] sectionData = reader.ReadBytes(3);
            int val = sectionData[0];
            if (val >= 0xC0 && val <= 0xDF)    // 2 byte unicode
            {
                var rawBytes = BitConverter.ToUInt32(Encoding.Convert(Encoding.UTF8, Encoding.UTF32, sectionData));
                builder.Append(Convert.ToChar(rawBytes));
                c += 2;
                reader.Seek(-1, SeekOrigin.Current);
            }
            else if (val >= 0xE0 && val <= 0xEF)    // 3 byte unicode
            {
                uint rawBytes = BitConverter.ToUInt32(Encoding.Convert(Encoding.UTF8, Encoding.UTF32, sectionData));
                builder.Append(Convert.ToChar(CipherShift(rawBytes, part)));
                c += 3;
            }
            else
            {
                builder.Append(Encoding.UTF8.GetString(new[] { (byte)CipherShift(sectionData[0], part) }));
                c += 1;
                reader.Seek(-2, SeekOrigin.Current);
            }
        }

        return builder.ToString();
    }

    private uint CipherShift(uint c, SStringPart part)
    {
        uint val = c + part.CipherShift;
        return val;
    }

    private List<string> ParseStringParts(TigerReader reader, SStringPartDefinition stringPart)
    {
        int stringPartSize = SchemaDeserializer.Get().GetSchemaStructSize<SStringPart>();
        int partStartIndex = (int)(stringPart.StartStringPartPointer.AbsoluteOffset - _tag.StringParts.Offset.AbsoluteOffset) /
                             stringPartSize;    // this is bad as magic numbers but means we dont parse multiple times

        List<string> strings = new();
        for (int i = 0; i < stringPart.PartCount; i++)
        {
            strings.Add(GetStringFromPart(reader, _tag.StringParts.ElementAt(reader, partStartIndex + i)));
        }

        return strings;
    }
}
