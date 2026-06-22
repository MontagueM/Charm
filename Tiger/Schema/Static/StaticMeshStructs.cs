
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Static;

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808071A7, 0x90)] //A7718080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D44, 0x70)] //446D8080
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

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80807193, 0x20)] //93718080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D2F, 0x24)] //2F6D8080
public struct SStaticMeshDecal
{
    // ugh this is ugly
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    public short RenderStageSK;
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    public short VertexLayoutIndexSK;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public byte RenderStageBL;
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public byte VertexLayoutIndexBL;

    [SchemaField(4, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(2, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public sbyte LODLevel;
    public sbyte Unk03;
    public short PrimitiveType;
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public short Unk06;
    public IndexBuffer Indices;
    public VertexBuffer Vertices0;
    public VertexBuffer Vertices1;
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public VertexBuffer? VertexColor;
    public uint IndexOffset;
    public uint IndexCount;
    public Material Material;

    public int GetVertexLayoutIndex()
    {
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402)
            return VertexLayoutIndexBL;
        else
            return VertexLayoutIndexSK;
    }

    public int GetRenderStage()
    {
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402)
            return RenderStageBL;
        else
            return RenderStageSK;
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80807194, 0x40)] //94718080
public struct SStaticMeshData_SK
{
    public long FileSize;
    public DynamicArray<SStaticMeshMaterialAssignment_SK> MaterialAssignments;
    public DynamicArray<SStaticMeshPart> Parts;
    public DynamicArray<SStaticMeshBuffers> Buffers;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D30, 0x70)] //306D8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80806D30, 0x60)] //306D8080
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

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080719B, 0x8)] //9B718080
public struct SStaticMeshMaterialAssignment_SK
{
    public ushort PartIndex;
    public ushort RenderStage;  // TFX render stage
    public short VertexLayoutIndex;
    public ushort Unk06;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80806D38, 0x6)] //386D8080
public struct SStaticMeshMaterialAssignment_WQ
{
    public ushort PartIndex;
    public byte RenderStage;  // TFX render stage
    public byte VertexLayoutIndex;
    public ushort Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080719A, 0xC)] //9A718080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D37, 0xC)] //376D8080
public struct SStaticMeshPart
{
    public uint IndexOffset;
    public uint IndexCount;
    public ushort BufferIndex;
    public sbyte DetailLevel;
    public sbyte PrimitiveType;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80807199, 0x10)] //99718080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D36, 0x14)] //366D8080
public struct SStaticMeshBuffers
{
    public IndexBuffer Indices;
    public VertexBuffer Vertices0;
    public VertexBuffer? Vertices1;
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public VertexBuffer VertexColor;
    public uint UnkOffset;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801BD6, 0x18)] //D61B8080
public struct SStaticMeshData_D1
{
    public VertexBuffer Vertices0;
    public VertexBuffer Vertices1;
    public IndexBuffer Indices;
    public sbyte UnkC;
    public sbyte UnkD;
    public sbyte DetailLevel;
    public sbyte PrimitiveType;
    public uint IndexOffset;
    public uint IndexCount;
}
