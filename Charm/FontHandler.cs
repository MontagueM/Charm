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
using Tiger;
using Tiger.Schema.Other;
using FontFamily = System.Windows.Media.FontFamily;

namespace Charm;

public class FontHandler : Subsystem
{
    public ConcurrentDictionary<FontInfo, FontFamily> Fonts = new();

    protected override bool Initialise()
    {
        return true;
        SaveAllFonts();
        return LoadAllFonts();
    }

    private static void SaveAllFonts()
    {
        // 0x80a00000 represents 0100 package
        // var vals = PackageHandler.GetAllEntriesOfReference(0x100, 0x80803c0f);
        // var vals = PackageResourcer.Get().GetAllHashes<D2Class_0F3C8080>();
        // Tag<D2Class_0F3C8080> fontsContainer = FileResourcer.Get().GetSchemaTag<D2Class_0F3C8080>(vals.First());
        // // Check if the font exists in the Fonts/ folder, if not extract it
        // if (!Directory.Exists("fonts/"))
        // {
        //     Directory.CreateDirectory("fonts/");
        // }
        // Parallel.ForEach(fontsContainer.TagData.FontParents, f =>
        // {
        //     var ff = f.FontParent.TagData.FontFile;
        //     var fontName = f.FontParent.TagData.FontName;
        //     if (!File.Exists($"fonts/{fontName}"))
        //     {
        //         using (TigerReader reader = ff.GetReader())
        //         {
        //             var bytes = reader.ReadBytes((int)f.FontParent.TagData.FontFileSize);
        //             File.WriteAllBytes($"fonts/{fontName}", bytes);
        //         }
        //     }
        // });
    }

    private bool LoadAllFonts()
    {

        // Parallel.ForEach(Directory.GetFiles(@"fonts/"), s =>
        foreach (var s in Directory.GetFiles(@"fonts/"))
        {
            var otfPath = Environment.CurrentDirectory + "/" + s;
            FontInfo fontInfo = GetFontInfo(otfPath);
            FontFamily font = new FontFamily(otfPath + $"#{fontInfo.Family}");
            Fonts.TryAdd(fontInfo, font);
        }//);

        return Fonts.Count > 0;
    }

    public struct FontInfo
    {
        public string Family;
        public string Subfamily;
    }

    private FontInfo GetFontInfo(string fontPath)
    {
        FontInfo fontInfo;
        using var br = new BinaryReaderBE(new MemoryStream(File.ReadAllBytes(fontPath)));
        byte[] val = br.ReadBytes(4);
        while (Encoding.ASCII.GetString(val) != "name")
        {
            val = br.ReadBytes(4);
        }

        var nameTableRecord = br.ReadType<OtfNameTableRecord>();

        br.BaseStream.Seek(nameTableRecord.Offset, SeekOrigin.Begin);

        var namingTableVer0 = br.ReadType<OtfNamingTableVersion0>();

        List<OtfNameRecord> nameRecords = new(namingTableVer0.Count);
        for (int i = 0; i < namingTableVer0.Count; i++)
        {
            nameRecords.Add(br.ReadType<OtfNameRecord>());
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
        fontInfo.Family = ReadString(br, familyRecord.Length).Trim();

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
        fontInfo.Subfamily = ReadString(br, subfamilyRecord.Length).Trim();

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

public class BinaryReaderBE : BinaryReader {
    public BinaryReaderBE(Stream stream)  : base(stream) { }

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
