using System.Runtime.InteropServices;
using Field.General;

namespace Field;

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