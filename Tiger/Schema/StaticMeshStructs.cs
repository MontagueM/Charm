using System.Runtime.InteropServices;

namespace Tiger.Schema;

    // [StructLayout(LayoutKind.Sequential, Size = 0x70)]
    // public struct D2Class_446D8080
    // {
    //     public long FileSize;
    //     [DestinyField(FieldType.TagHash)]
    //     public StaticData StaticData;
    //     [DestinyOffset(0x10), DestinyField(FieldType.TablePointer)]
    //     public List<D2Class_14008080> Materials;
    //     [DestinyField(FieldType.TablePointer)]
    //     public List<D2Class_2F6D8080> Decals;
    //     [DestinyOffset(0x3C)]  // revise this, not correct. maybe correct for decals?
    //     public Vector3 Scale;
    //     [DestinyOffset(0x50)]
    //     public Vector4 Offset;
    // }
    //
    // [StructLayout(LayoutKind.Sequential, Size = 4)]
    // public struct D2Class_14008080
    // {
    //     public DestinyHash MaterialHash;
    // }
    //
    // [StructLayout(LayoutKind.Sequential, Size = 0x24)]
    // public struct D2Class_2F6D8080
    // {
    //     public sbyte Unk00;
    //     public sbyte Unk01;
    //     public sbyte LODLevel;
    //     public sbyte Unk03;
    //     public int PrimitiveType;
    //     [DestinyField(FieldType.TagHash)]
    //     public IndexHeader Indices;
    //     [DestinyField(FieldType.TagHash)]
    //     public VertexHeader Vertices1;
    //     [DestinyField(FieldType.TagHash)]
    //     public VertexHeader Vertices2;
    //     [DestinyField(FieldType.TagHash)]
    //     public VertexHeader Vertices3;
    //     public uint IndexOffset;
    //     public uint IndexCount;
    //     public DestinyHash MaterialHash;
    // }
    //
    // [StructLayout(LayoutKind.Sequential, Size = 0x60)]
    // public struct D2Class_306D8080
    // {
    //     public long FileSize;
    //     [DestinyField(FieldType.TablePointer)]
    //     public List<D2Class_386D8080> MaterialAssignments;
    //     [DestinyField(FieldType.TablePointer)]
    //     public List<D2Class_376D8080> Parts;
    //     [DestinyField(FieldType.TablePointer)]
    //     public List<D2Class_366D8080> Meshes;
    //     [DestinyOffset(0x40)]
    //     public Vector4 ModelTransform;
    //     [DestinyOffset(0x50)]
    //     public float TexcoordScale;
    //     public float TexcoordXTranslation;
    //     public float TexcoordYTranslation;
    // }
    //
    // [StructLayout(LayoutKind.Sequential, Size = 0x8)]
    // public struct TexcoordTransform
    // {
    //     public float Scale;
    //     public float Translation;
    // }
    //
    // [StructLayout(LayoutKind.Sequential, Size = 0x6)]
    // public struct D2Class_386D8080
    // {
    //     public ushort PartIndex;
    //     public byte DetailLevel;  // not exactly, but definitely related to it (maybe distance-based)
    //     public byte Unk03;
    //     public ushort Unk04;
    // }
    //
    // [StructLayout(LayoutKind.Sequential, Size = 0xC)]
    // public struct D2Class_376D8080
    // {
    //     public uint IndexOffset;
    //     public uint IndexCount;
    //     public ushort Unk08;
    //     public sbyte DetailLevel;
    //     public sbyte PrimitiveType;
    // }
    //
    // [StructLayout(LayoutKind.Sequential, Size = 0x14)]
    // public struct D2Class_366D8080
    // {
    //     [DestinyField(FieldType.TagHash)]
    //     public IndexHeader Indices;
    //     [DestinyField(FieldType.TagHash)]
    //     public VertexHeader Vertices1;
    //     [DestinyField(FieldType.TagHash)]
    //     public VertexHeader Vertices2;
    //     [DestinyField(FieldType.TagHash)]
    //     public VertexHeader Vertices3;
    // }
