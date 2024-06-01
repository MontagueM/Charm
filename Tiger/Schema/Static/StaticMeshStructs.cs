using Tiger.Schema.Entity;
using Tiger.Schema.Model;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Static;

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "A7718080", 0x90)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "446D8080", 0x70)]
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
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "14008080", 0x4)]
public struct SMaterialHash
{
    public IMaterial Material;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "93718080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "2F6D8080", 0x24)]
public struct SStaticMeshDecal
{
    public sbyte RenderStage;
    public sbyte VertexLayoutIndex;
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public short Unk02;
    public sbyte LODLevel;
    public sbyte Unk03;
    public short PrimitiveType;
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public short Unk06;
    public IndexBuffer Indices;
    public VertexBuffer Vertices0;
    public VertexBuffer Vertices1;
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public VertexBuffer? VertexColor;
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

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "306D8080", 0x70)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "306D8080", 0x60)]
public struct SStaticMeshData_BL
{
    public long FileSize;
    public DynamicArray<SStaticMeshMaterialAssignment_WQ> MaterialAssignments;
    public DynamicArray<SStaticMeshPart> Parts;
    public DynamicArray<SStaticMeshBuffers> Meshes;
    [SchemaField(0x50, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Vector4 ModelTransform;
    public float TexcoordScale;
    public Vector2 TexcoordTranslation;
    public uint MaxVertexColorIndex;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "9B718080", 0x8)]
public struct SStaticMeshMaterialAssignment_SK
{
    public ushort PartIndex;
    public ushort RenderStage;  // TFX render stage
    public ushort Unk04; // VertexLayoutIndex?
    public ushort Unk06;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "386D8080", 0x6)]
public struct SStaticMeshMaterialAssignment_WQ
{
    public ushort PartIndex;
    public byte RenderStage;  // TFX render stage
    public byte VertexLayoutIndex;
    public ushort Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "9A718080", 0xC)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "376D8080", 0xC)]
public struct SStaticMeshPart
{
    public uint IndexOffset;
    public uint IndexCount;
    public ushort BufferIndex;
    public sbyte DetailLevel;
    public sbyte PrimitiveType;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "99718080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "366D8080", 0x14)]
public struct SStaticMeshBuffers
{
    public IndexBuffer Indices;
    public VertexBuffer Vertices0;
    public VertexBuffer? Vertices1;
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public VertexBuffer VertexColor;
    public uint UnkOffset;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D61B8080", 0x18)]
public struct SStaticMeshData_D1
{
    public VertexBuffer Vertices0;
    public VertexBuffer Vertices1;
    public IndexBuffer Indices;
    public sbyte UnkC; // render stage?
    public sbyte UnkD; // vertex layout index?
    public sbyte DetailLevel;
    public sbyte PrimitiveType;
    public uint IndexOffset;
    public uint IndexCount;
}

#region Decorators
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "361C8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "AD718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "C36C8080", 0x18)]
public struct SDecoratorMapResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public Decorator Decorator;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CE1A8080", 0xA8)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "64718080", 0xA8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "986C8080", 0xA8)]
public struct SDecorator
{
    public ulong Size;
    public DynamicArray<D2Class_B16C8080> DecoratorModels;
    public DynamicArray<D2Class_07008080> InstanceRanges;
    public DynamicArray<D2Class_07008080> Unk28;
    public DynamicArray<D2Class_07008080> Unk38;
    public Tag<D2Class_A46C8080> BufferData;
    public Tag<SOcclusionBounds> OcculusionBounds;
    public DynamicArray<D2Class_07008080> Unk50;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "17488080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "7D718080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B16C8080", 0x4)]
public struct D2Class_B16C8080
{
    public Tag<D2Class_B26C8080> DecoratorModel;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "221C8080", 0xD8)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "7E718080", 0xD8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B26C8080", 0x100)]
public struct D2Class_B26C8080
{
    public long FileSize;
    public EntityModel Model;
    public int UnkC;
    //public AABB BoundingBox; not in pre-bl, dont really care about it tho
    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag Unk30;  // D2Class_B46C8080
    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x14, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x34, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag<D2Class_B86C8080> SpeedTreeData; // Used for actual trees
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D81B8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "84718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B86C8080", 0x18)]
public struct D2Class_B86C8080
{
    [SchemaField(0x8)]
    public DynamicArray<D2Class_BA6C8080> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "9A1A8080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "86718080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "BA6C8080", 0x50)]
public struct D2Class_BA6C8080
{
    public Vector4 Unk00;
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    public Vector4 Unk40;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CB1A8080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "70718080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "A46C8080", 0x20)]
public struct D2Class_A46C8080
{
    public ulong Size;
    public TigerHash Unk08;
    public TigerHash UnkC;
    public int Unk10;
    public Tag<D2Class_9F6C8080> Unk14;
    public VertexBuffer InstanceBuffer;
    public Tag<SDecoratorInstanceData> InstanceData;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "321B8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "73718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "A76C8080", 0x18)]
public struct SDecoratorInstanceData
{
    [SchemaField(0x8)]
    public DynamicArray<D2Class_A96C8080> InstanceElement;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "291B8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "75718080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "A96C8080", 0x10)]
public struct D2Class_A96C8080
{
    // Normalized position
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, ArraySizeConst = 4)]
    public ushort[] Position;
    // Rotation represented as an 8-bit quaternion
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, ArraySizeConst = 4)]
    public byte[] Rotation;
    // RGBA color
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, ArraySizeConst = 4)]
    public byte[] Color;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "721A8080", 0x60)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "6B718080", 0x60)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "9F6C8080", 0x60)]
public struct D2Class_9F6C8080
{
    // Speedtree[0-5] cbuffer
    public Vector4 Unk00;
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    public Vector4 Unk40;
    public Vector4 Unk50;
}
#endregion
