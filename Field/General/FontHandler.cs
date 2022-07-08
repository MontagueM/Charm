using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Text;
using FontFamily = System.Windows.Media.FontFamily;

namespace Field.General;

public class FontHandler
{
    public static ConcurrentDictionary<FontInfo, FontFamily> Initialise()
    {
        SaveAllFonts();
        return LoadAllFonts();
    }

    private static void SaveAllFonts()
    {
        // 0x80a00000 represents 0100 package
        var vals = PackageHandler.GetAllEntriesOfReference(0x100, 0x80803c0f);
        Tag<D2Class_0F3C8080> fontsContainer = PackageHandler.GetTag(typeof(Tag<D2Class_0F3C8080>),vals[0]);
        // Check if the font exists in the Fonts/ folder, if not extract it
        if (!Directory.Exists("fonts/"))
        {
            Directory.CreateDirectory("fonts/");
        }
        Parallel.ForEach(fontsContainer.Header.FontParents, f =>
        {
            var ff = f.FontParent.Header.FontFile;
            var fontName = f.FontParent.Header.FontName;
            if (!File.Exists($"fonts/{fontName}"))
            {
                using (var handle = ff.GetHandle())
                {
                    var bytes = handle.ReadBytes((int)f.FontParent.Header.FontFileSize);
                    File.WriteAllBytes($"fonts/{fontName}", bytes);

                } 
            }
        });
    }

    private static ConcurrentDictionary<FontInfo, FontFamily> LoadAllFonts()
    {
        ConcurrentDictionary<FontInfo, FontFamily> fontFamilies = new ConcurrentDictionary<FontInfo, FontFamily>();
        
        Parallel.ForEach(Directory.GetFiles(@"fonts/"), s =>
        {
            var otfPath = Environment.CurrentDirectory + "/" + s;
            FontInfo fontInfo = GetFontInfo(otfPath);
            FontFamily font = new FontFamily(otfPath + $"#{fontInfo.Family}");
            fontFamilies[fontInfo] = font;
        });
        return fontFamilies;
    }

    public struct FontInfo
    {
        public string Family;
        public string Subfamily;
    }

    private static FontInfo GetFontInfo(string fontPath)
    {
        FontInfo fontInfo;
        using (var br = new BinaryReaderBE(new MemoryStream(File.ReadAllBytes(fontPath))))
        {
            byte[] val = br.ReadBytes(4);
            while (Encoding.ASCII.GetString(val) != "name")
            {
                val = br.ReadBytes(4);
            }

            var nameTableRecord = StructConverter.ReadStructure<OtfNameTableRecord>(br);

            br.BaseStream.Seek(nameTableRecord.Offset, SeekOrigin.Begin);

            var namingTableVer0 = StructConverter.ReadStructure<OtfNamingTableVersion0>(br);

            List<OtfNameRecord> nameRecords = new List<OtfNameRecord>(namingTableVer0.Count);
            for (int i = 0; i < namingTableVer0.Count; i++)
            {
                nameRecords.Add(StructConverter.ReadStructure<OtfNameRecord>(br));
            }

            OtfNameRecord familyRecord;
            try
            {
                familyRecord = nameRecords.First(x => x.NameId == 16);
            }
            catch (InvalidOperationException e)
            {
                familyRecord = nameRecords.First(x => x.NameId == 1);
            }
            br.BaseStream.Seek(nameTableRecord.Offset + namingTableVer0.StorageOffset + familyRecord.StringOffset, SeekOrigin.Begin);
            fontInfo.Family = ReadString(br, familyRecord.Length);
            
            OtfNameRecord subfamilyRecord;
            try
            {
                subfamilyRecord = nameRecords.FirstOrDefault(x => x.NameId == 17);
            }
            catch (InvalidOperationException e)
            {
                subfamilyRecord = nameRecords.FirstOrDefault(x => x.NameId == 2);
            }
            br.BaseStream.Seek(nameTableRecord.Offset + namingTableVer0.StorageOffset + subfamilyRecord.StringOffset, SeekOrigin.Begin);
            fontInfo.Subfamily = ReadString(br, subfamilyRecord.Length);
        }

        return fontInfo;
    }
    
    /// <summary>
    /// Glyph names are kinda interesting, could get them in the future. CCF table?
    /// </summary>
    private static List<string> GetGlyphNames(string fontPath)
    {
        throw new NotImplementedException();
    }

    private static string ReadString(BinaryReaderBE br, int length)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            char c = br.ReadChar();
            sb.Append(c);
        }

        return sb.ToString();
    }
}

[StructLayout(LayoutKind.Sequential)]
struct OtfNameTableRecord
{
    public uint Checksum;
    public uint Offset;
    public uint Length;
}

[StructLayout(LayoutKind.Sequential)]
struct OtfNamingTableVersion0
{
    public ushort Version;
    public ushort Count;
    public ushort StorageOffset;
}

[StructLayout(LayoutKind.Sequential)]
struct OtfNameRecord
{
    public ushort PlatformId;
    public ushort EncodingId;
    public ushort LanguageId;
    public ushort NameId;
    public ushort Length;
    public ushort StringOffset;
}

public class BinaryReaderBE : BinaryReader { 
    public BinaryReaderBE(System.IO.Stream stream)  : base(stream) { }

    public override int ReadInt32()
    {
        var data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToInt32(data, 0);
    }

    public override Int16 ReadInt16()
    {
        var data = base.ReadBytes(2);
        Array.Reverse(data);
        return BitConverter.ToInt16(data, 0);
    }

    public override Int64 ReadInt64()
    {
        var data = base.ReadBytes(8);
        Array.Reverse(data);
        return BitConverter.ToInt64(data, 0);
    }

    public override UInt32 ReadUInt32()
    {
        var data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToUInt32(data, 0);
    }

}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_0F3C8080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_113C8080> FontParents;
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_113C8080
{
    [DestinyField(FieldType.TagHash)]
    public Tag<D2Class_123C8080> FontParent;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_123C8080
{
    public long FileSize;
    [DestinyField(FieldType.TagHash)]
    public DestinyFile FontFile;
    [DestinyOffset(0x10), DestinyField(FieldType.RelativePointer)]
    public string FontName;
    public long FontFileSize;
}