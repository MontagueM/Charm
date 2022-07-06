using System.IO;
using Field.General;

namespace Field.Strings;

public class StringContainer : Tag
{
    public D2Class_EF998080 Header;
    
    public StringContainer(TagHash hash) : base(hash)
    {
    }


    public string GetStringFromHash(ELanguage language, DestinyHash hash)
    {
        int index = Header.StringHashTable.BinarySearch(hash);
        if (index < 0) return String.Empty;
        return Header.StringData.ParseStringIndex(index);
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_EF998080>();
    }
}