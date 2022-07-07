using System.Runtime.InteropServices;

namespace Field.General;

public class FontHandler
{
    public static void Initialise()
    {
        // 0x80a00000 represents 0100 package
        var pAllEntries = PackageHandler.GetAllEntriesOfReference(0x80a00000, 0x80803c0f);
        uint[] vals = new uint[pAllEntries.dataSize];
        PackageHandler.Copy(pAllEntries.dataPtr, vals, 0, pAllEntries.dataSize);
        Tag<D2Class_0F3C8080> fontsContainer = PackageHandler.GetTag(typeof(Tag<D2Class_0F3C8080>),0x80800000 + (0x100 << 0xD) + vals[0]);
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