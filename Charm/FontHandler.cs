using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Other;
using FontFamily = System.Windows.Media.FontFamily;

namespace Charm;

[InitializeAfter(typeof(Hash64Map))]
public class FontHandler : Strategy.StrategistSingleton<FontHandler>
{
    public ConcurrentDictionary<FontInfo, FontFamily> Fonts = new();

    public FontHandler(TigerStrategy strategy) : base(strategy)
    {
    }

    protected override void Initialise()
    {
        //return true;
        SaveAllFonts();
        LoadAllFonts();
        RegisterFonts();
    }

    protected override void Reset()
    {

    }

    private static void SaveAllFonts()
    {
        //0x80a00000 represents 0100 package
        //var vals = PackageHandler.GetAllEntriesOfReference(0x100, 0x80803c0f);
        var vals = PackageResourcer.Get().GetAllHashes<D2Class_0F3C8080>();
        Tag<D2Class_0F3C8080> fontsContainer = FileResourcer.Get().GetSchemaTag<D2Class_0F3C8080>(vals.First());
        // Check if the font exists in the Fonts/ folder, if not extract it
        if (!Directory.Exists("fonts/"))
        {
            Directory.CreateDirectory("fonts/");
        }
        Parallel.ForEach(fontsContainer.TagData.FontParents, f =>
        {
            var ff = f.FontParent.TagData.FontFile;
            var fontName = f.FontParent.TagData.FontName.Value;
            if (!File.Exists($"fonts/{fontName}"))
            {
                using (TigerReader reader = ff.GetReader())
                {
                    var bytes = reader.ReadBytes((int)f.FontParent.TagData.FontFileSize);
                    File.WriteAllBytes($"fonts/{fontName}", bytes);
                }
            }
        });
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

    private void RegisterFonts()
    {
        foreach (var (key, value) in Fonts)
        {
            Application.Current.Resources.Add($"{key.Family} {key.Subfamily}", value);
        }

        // Debug font list
        //List<string> fontList = fonts.Select(pair => (pair.Key.Family + " " + pair.Key.Subfamily).Trim()).ToList();
        //foreach (var s in fontList)
        //{
        //    Console.WriteLine(s);
        //}
        /*
        Haas Grot Disp 75 Bold
        Noto Serif KR Medium
        Destiny Keys Regular
        Pragmatica Bold
        Pragmatica Book
        Pragmatica Medium Oblique
        Pragmatica Medium
        Pragmatica Book Oblique
        Noto Sans TC
        Noto Sans TC Medium
        Noto Serif TC Medium
        Aldine 401 BT
        Noto Serif JP Medium
        Noto Sans JP
        Noto Sans JP Medium
        Noto Serif SC Medium
        Noto Sans KR
        Noto Sans KR Medium
        Cromwell NF
        Destiny Symbols
        Cromwell HPLHS
        Haas Grot Text 55 Roman
        Haas Grot Text 56 Italic
        Haas Grot Text 65 Medium
        Haas Grot Text 66 Medium Italic
        Noto Sans SC
        Noto Sans SC Medium
        */
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

        var nameTableRecord = br.ReadType<OtfNameTableRecord>(true);
        br.BaseStream.Seek(nameTableRecord.Offset, SeekOrigin.Begin);

        var namingTableVer0 = br.ReadType<OtfNamingTableVersion0>(true);

        List<OtfNameRecord> nameRecords = new(namingTableVer0.Count);
        for (int i = 0; i < namingTableVer0.Count; i++)
        {
            var nameRecord = br.ReadType<OtfNameRecord>(true);
            nameRecords.Add(nameRecord);
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
    public uint Length;
    public uint Offset;
    public uint Checksum;
}

[StructLayout(LayoutKind.Sequential)]
struct OtfNamingTableVersion0
{
    public ushort StorageOffset;
    public ushort Count;
    public ushort Version;
}

[StructLayout(LayoutKind.Sequential)]
struct OtfNameRecord
{
    public ushort StringOffset;
    public ushort Length;
    public ushort NameId;
    public ushort LanguageId;
    public ushort EncodingId;
    public ushort PlatformId;
}

public class BinaryReaderBE : BinaryReader
{
    public BinaryReaderBE(Stream stream) : base(stream) { }

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

    public dynamic ToType(byte[] bytes, Type type)
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type); }
        finally { handle.Free(); }
    }

    public T ReadType<T>(bool BE = true)
    {
        return (T)ReadType(typeof(T), BE);
    }

    public dynamic ReadType(Type type, bool BE)
    {
        var buffer = new byte[Marshal.SizeOf(type)];
        Read(buffer, 0, buffer.Length);
        if (BE)
            Array.Reverse(buffer);
        return ToType(buffer, type);
    }

}
