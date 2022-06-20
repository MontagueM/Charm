using System.IO;
using Field.General;

namespace Field.Strings;

public class StringContainer : Tag
{
    public D2Class_EF998080 Header;
        
    public StringContainer(string hash) : base(hash)
    {
    }
        
    public StringContainer(TagHash hash) : base(hash)
    {
    }


    public string GetStringFromHash(ELanguage language, DestinyHash hash)
    {
        // return Header.StringData[(int)language].ParseStringIndex(Header.StringHashTable.BinarySearch(hash));
        return Header.StringData.ParseStringIndex(Header.StringHashTable.BinarySearch(hash));

    }
        
    public Dictionary<DestinyHash, string> GetAllStrings(ELanguage language)
    {
        Dictionary<DestinyHash, string> strings = new Dictionary<DestinyHash, string>();
        for (int i = 0; i < Header.StringHashTable.Count; i++)
        {
            strings.Add(Header.StringHashTable[i], Header.StringData.ParseStringIndex(i));
        }
        return strings;
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_EF998080>();
    }
}