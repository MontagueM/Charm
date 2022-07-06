using System.Runtime.InteropServices;
using System.Text;
using Field.General;

namespace Field.Strings;

public class StringData : Tag
{
    public D2Class_F1998080 Header;
    public StringData(TagHash hash) : base(hash)
    {
    }

    // private byte[] ReadChars(int offset, int num)
    // {
    //     byte[] ret = new byte[num];
    //     for (int i = 0; i < num; i++)
    //     {
    //         if (offset + i == Header.StringData.Count) 
    //             return ret;
    //         ret[i] = Header.StringData[offset+i].StringCharacter;
    //     }
    //
    //     return ret;
    // }

    private string GetStringFromPart(D2Class_F7998080 part, BinaryReader handle)
    {
        handle.BaseStream.Seek(part.StringDataPointer, SeekOrigin.Begin);
        // int dataOffset = (int) (part.StringDataPointer - Header.StringParts[0].StringDataPointer);
        StringBuilder builder = new StringBuilder();
        int c = 0;
        while (c < part.ByteLength)
        {
            // byte[] sectionData = ReadChars(dataOffset+c, 3);
            byte[] sectionData = handle.ReadBytes(3);
            int val = sectionData[0];
            if (val >= 0xC0 && val <= 0xDF)  // 2 byte unicode
            {
                var rawBytes = BitConverter.ToUInt32(Encoding.Convert(Encoding.UTF8, Encoding.UTF32, sectionData));
                builder.Append(Convert.ToChar(rawBytes));
                c += 2;
                handle.BaseStream.Seek(-1, SeekOrigin.Current);
            }
            else if (val >= 0xE0 && val <= 0xEF)  // 3 byte unicode
            {
                var rawBytes = BitConverter.ToUInt32(Encoding.Convert(Encoding.UTF8, Encoding.UTF32, sectionData));
                builder.Append(Convert.ToChar(rawBytes));
                c += 3;
            }
            else
            {
                builder.Append(Encoding.UTF8.GetString(new [] { sectionData[0] }));
                c += 1;
                handle.BaseStream.Seek(-2, SeekOrigin.Current);
            }
        }
        
        return builder.ToString();
    }
    
    private List<string> ParseStringParts(D2Class_F5998080 combination, BinaryReader handle)
    {
        // Handle.BaseStream.Seek(combination.StartStringPartPointer, SeekOrigin.Begin);
        int partStartIndex = (int)(combination.StartStringPartPointer - 0x60) / 0x20; // this is bad as magic numbers but means we dont parse multiple times
        // List<D2Class_F7998080> stringParts = new List<D2Class_F7998080>();
        // for (int i = 0; i < combination.PartCount; i++)
        // {
        //     // stringParts.Add(ReadStruct(typeof(D2Class_F7998080), Handle));
        // }

        List<string> strings = new List<string>();
        // foreach (var part in stringParts)
        for (int i = 0; i < combination.PartCount; i++)
        {
            strings.Add(GetStringFromPart(Header.StringParts.ElementAt(partStartIndex+i, handle), handle));
        }   


        return strings;
    }
    
    /// <summary>
    /// Given the index of a string, returns the string.
    /// </summary>
    /// <param name="stringIndex">The index of the string to retrieve, where the index can be found from the hash table of the string bank.</param>
    /// <returns>The string of the index given.</returns>
    public string ParseStringIndex(int stringIndex)
    {
        List<string> strings;
        using (var handle = GetHandle())
        {
            D2Class_F5998080 combination = Header.StringCombinations.ElementAt(stringIndex, handle);
            strings = ParseStringParts(combination, handle);
        }

        return string.Join("", strings.ToArray());
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_F1998080>();
    }
}