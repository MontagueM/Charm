using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Field;
using Field.General;
using Serilog;
using FontFamily = System.Windows.Media.FontFamily;

namespace Charm;

public class FontHandler
{
    public static ConcurrentDictionary<FontInfo, FontFamily> Initialise()
    {
        SaveAllFonts();
        return LoadAllFonts();
    }

    private static void SaveAllFonts()
    {
        Log.Information("1");
        // 0x80a00000 represents 0100 package
        var vals = PackageHandler.GetAllEntriesOfReference(0x100, 0x80803c0f);
        Tag<D2Class_0F3C8080> fontsContainer = PackageHandler.GetTag<D2Class_0F3C8080>(vals[0]);
        // Check if the font exists in the Fonts/ folder, if not extract it
        Log.Information("2");
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
        Log.Information("3");
    }

    private static ConcurrentDictionary<FontInfo, FontFamily> LoadAllFonts()
    {
        ConcurrentDictionary<FontInfo, FontFamily> fontFamilies = new ConcurrentDictionary<FontInfo, FontFamily>();
        Log.Information("4");

        Parallel.ForEach(Directory.GetFiles(@"fonts/"), s =>
        {
            var otfPath = Environment.CurrentDirectory + "/" + s;
            FontInfo fontInfo = GetFontInfo(otfPath);
            FontFamily font = new FontFamily(otfPath + $"#{fontInfo.Family}");
            fontFamilies[fontInfo] = font;
        });
        Log.Information("5");

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
        Log.Information($"f1 {fontPath}");
        using (var br = new BinaryReaderBE(new MemoryStream(File.ReadAllBytes(fontPath))))
        {
            Log.Information($"fa {fontPath}");

            byte[] val = br.ReadBytes(4);
            while (Encoding.ASCII.GetString(val) != "name")
            {
                val = br.ReadBytes(4);
            }
            Log.Information($"fb {fontPath}");


            var nameTableRecord = StructConverter.ReadStructure<OtfNameTableRecord>(br);

            br.BaseStream.Seek(nameTableRecord.Offset, SeekOrigin.Begin);

            var namingTableVer0 = StructConverter.ReadStructure<OtfNamingTableVersion0>(br);

            List<OtfNameRecord> nameRecords = new List<OtfNameRecord>(namingTableVer0.Count);
            for (int i = 0; i < namingTableVer0.Count; i++)
            {
                nameRecords.Add(StructConverter.ReadStructure<OtfNameRecord>(br));
            }
            Log.Information($"fc {fontPath}");

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
            fontInfo.Family = ReadString(br, familyRecord.Length).Trim();
            Log.Information($"fd {fontPath}");

            OtfNameRecord subfamilyRecord;
            try
            {
                subfamilyRecord = nameRecords.FirstOrDefault(x => x.NameId == 17);
            }
            catch (InvalidOperationException e)
            {
                subfamilyRecord = nameRecords.FirstOrDefault(x => x.NameId == 2);
            }
            Log.Information($"fe {fontPath}");

            br.BaseStream.Seek(nameTableRecord.Offset + namingTableVer0.StorageOffset + subfamilyRecord.StringOffset, SeekOrigin.Begin);
            fontInfo.Subfamily = ReadString(br, subfamilyRecord.Length).Trim();
        }
        Log.Information($"f2 {fontPath}");

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

        return ConvertWideChar(sb.ToString());
    }
    
    private static string ConvertWideChar(string s)
    {
        if (s.Contains("\x00")) // wchar_t
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            bytes = bytes.Where(x => x != '\x00').ToArray();
            return Encoding.UTF8.GetString(bytes);
        }
        return s;
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