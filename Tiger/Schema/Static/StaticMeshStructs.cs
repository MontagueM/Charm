using Tiger.Schema.Shaders;
using Tiger.Schema.Model;

namespace Tiger.Schema.Static;

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "A7718080", 0x90)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "446D8080", 0x70)]
public struct SStaticMesh
{
    public long FileSize;
    public IStaticMeshData StaticData;
    [SchemaField(0x10)]
    public DynamicArray<SMaterialHash> Materials;
    public DynamicArray<SStaticMeshDecal> Decals;
    [SchemaField(0x3C)]  // revise this, not correct. maybe correct for decals?
    public Vector3 Scale;
    [SchemaField(0x50)]
    public Vector4 Offset;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Vector4 ModelTransform;
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Vector2 TexcoordScale;
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Vector2 TexcoordTranslation;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "14008080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "14008080", 0x4)]
public struct SMaterialHash
{
    public IMaterial Material;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "93718080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "2F6D8080", 0x24)]
public struct SStaticMeshDecal
{
    public sbyte Unk00;
    public sbyte Unk01;
    public sbyte LODLevel;
    public sbyte Unk03;
    public int PrimitiveType;
    public IndexBuffer Indices;
    public VertexBuffer Vertices0;
    public VertexBuffer Vertices1;
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public VertexBuffer? Vertices2;
    public uint IndexOffset;
    public uint IndexCount;
    public IMaterial Material;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "94718080", 0x40)]
public struct SStaticMeshData_SK
{
    public long FileSize;
    public DynamicArray<SStaticMeshMaterialAssignment_SK> MaterialAssignments;
    public DynamicArray<SStaticMeshPart> Parts;
    public DynamicArray<SStaticMeshBuffers> Buffers;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "306D8080", 0x60)]
public struct SStaticMeshData_WQ
{
    public long FileSize;
    public DynamicArray<SStaticMeshMaterialAssignment_WQ> MaterialAssignments;
    public DynamicArray<SStaticMeshPart> Parts;
    public DynamicArray<SStaticMeshBuffers> Meshes;
    [SchemaField(0x40)]
    public Vector4 ModelTransform;
    public float TexcoordScale;
    public Vector2 TexcoordTranslation;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "9B718080", 0x8)]
public struct SStaticMeshMaterialAssignment_SK
{
    public ushort PartIndex;
    public ushort DetailLevel;  // not exactly, but definitely related to it (maybe distance-based)
    public ushort Unk04;
    public ushort Unk06;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "386D8080", 0x6)]
public struct SStaticMeshMaterialAssignment_WQ
{
    public ushort PartIndex;
    public byte DetailLevel;  // not exactly, but definitely related to it (maybe distance-based)
    public byte Unk03;
    public ushort Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "9A718080", 0xC)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "376D8080", 0xC)]
public struct SStaticMeshPart
{
    public uint IndexOffset;
    public uint IndexCount;
    public ushort BufferIndex;
    public sbyte DetailLevel;
    public sbyte PrimitiveType;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "99718080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "366D8080", 0x14)]
public struct SStaticMeshBuffers
{
    public IndexBuffer Indices;
    public VertexBuffer Vertices0;
    public VertexBuffer? Vertices1;
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public VertexBuffer Vertices2;
    public uint UnkOffset;
}
