using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Field.General;
using Field.Models;

namespace Field.Statics;

public class StaticMapData : Tag
{
    public D2Class_AD938080 Header;

    public StaticMapData(TagHash hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_AD938080>();
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0xC0)]
public struct D2Class_AD938080
{
    public long FileSize;
    [DestinyOffset(0x18), DestinyField(FieldType.TagHash)]
    public Tag<D2Class_B1938080> ModelOcclusionBounds;
    [DestinyOffset(0x40), DestinyField(FieldType.TablePointer)]
    public List<D2Class_406D8080> Instances;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0B008080> Unk50;
    [DestinyOffset(0x78), DestinyField(FieldType.TablePointer)]
    public List<D2Class_BD938080> Statics;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_286D8080> InstanceCounts;
    [DestinyOffset(0x98)]
    public DestinyHash Unk98;
    [DestinyOffset(0xA0)]
    public Vector4 UnkA0;
    public Vector4 UnkB0;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_B1938080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_B3938080> InstanceBounds;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_B3938080
{
    public Vector4 Corner1;
    public Vector4 Corner2;
    public DestinyHash Unk20;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_406D8080
{
    public Vector4 Rotation;
    public Vector3 Position;
    public Vector3 Scale;  // Only X is used as a global scale
}

[StructLayout(LayoutKind.Sequential, Size = 0x4)]
public struct D2Class_BD938080
{
    [DestinyField(FieldType.TagHash)]
    public StaticContainer Static;
}

[StructLayout(LayoutKind.Sequential, Size = 0x8)]
public struct D2Class_286D8080
{
    public short InstanceCount;
    public short InstanceOffset;
    public short StaticIndex;
    public short Unk06;
}