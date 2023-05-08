using System.Runtime.InteropServices;

namespace Tiger.Schema;

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "446D8080", 0x70)]
public struct SStaticMesh
{
    public long FileSize;
    public StaticMeshData StaticData;
    [SchemaField(0x10)]
    public DynamicArray<D2Class_14008080> Materials;
    public DynamicArray<D2Class_2F6D8080> Decals;
    [SchemaField(0x3C)]  // revise this, not correct. maybe correct for decals?
    public Vector3 Scale;
    [SchemaField(0x50)]
    public Vector4 Offset;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "14008080", 0x4)]
public struct D2Class_14008080
{
    public Material MaterialHash;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "2F6D8080", 0x24)]
public struct D2Class_2F6D8080
{
    public sbyte Unk00;
    public sbyte Unk01;
    public sbyte LODLevel;
    public sbyte Unk03;
    public int PrimitiveType;
    public IndexBuffer Indices;
    public VertexBuffer Vertices0;
    public VertexBuffer Vertices1;
    public VertexBuffer Vertices2;
    public uint IndexOffset;
    public uint IndexCount;
    public Material MaterialHash;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "306D8080", 0x60)]
public struct SStaticMeshData
{
    public long FileSize;
    public DynamicArray<D2Class_386D8080> MaterialAssignments;
    public DynamicArray<D2Class_376D8080> Parts;
    public DynamicArray<D2Class_366D8080> Meshes;
    [SchemaField(0x40)]
    public Vector4 ModelTransform;
    [SchemaField(0x50)]
    public float TexcoordScale;
    public float TexcoordXTranslation;
    public float TexcoordYTranslation;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "386D8080", 0x6)]
public struct D2Class_386D8080
{
    public ushort PartIndex;
    public byte DetailLevel;  // not exactly, but definitely related to it (maybe distance-based)
    public byte Unk03;
    public ushort Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "376D8080", 0xC)]
public struct D2Class_376D8080
{
    public uint IndexOffset;
    public uint IndexCount;
    public ushort Unk08;
    public sbyte DetailLevel;
    public sbyte PrimitiveType;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "366D8080", 0x14)]
public struct D2Class_366D8080
{
    public IndexBuffer Indices;
    public VertexBuffer Vertices0;
    public VertexBuffer Vertices1;
    public VertexBuffer Vertices2;
}
