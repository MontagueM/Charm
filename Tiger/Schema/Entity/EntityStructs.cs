using Tiger.Schema.Investment;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Entity;

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800734, 0xA8)] //34078080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809C0F, 0xA0)] //0F9C8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809AD8, 0x98)] //D89A8080
public struct SEntity
{
    public long FileSize;

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x08, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArrayUnloaded<S80809ACD> EntityComponents;

    [SchemaField(0x68, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x58, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x50, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArrayUnloaded<S80809AED> UnkResources; // Basically EntityComponents but contains the Resource's Unk10 ClassHash
}


[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800715, 0xC)] //15078080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809C04, 0xC)] //049C8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809ACD, 0xC)] //CD9A8080
public struct S80809ACD  // entity resource entry
{
    public FileHash Resource; // Can sometimes be a non-entity resource in D1, for whatever reason
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800370, 0x28)] //70038080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809C22, 0x28)] //229C8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809AED, 0x28)] //ED9A8080
public struct S80809AED
{
    [SchemaField(0xC)]
    public TigerHash Unk10ClassHash;
    public FileHash Resource;
}


[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800861, 0xA0)] //61088080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809C36, 0xA0)] //369C8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809B06, 0xA0)] //069B8080
public struct S80809B06  // Entity resource
{
    public long FileSize;

    [SchemaField(0x10)]
    public ResourcePointer Unk10;
    public ResourcePointer Unk18;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x80, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Tag UnkHash80;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Tag UnkHash84;  // 819A8080
    // Rest is unknown
}


/*
 * The external material map provides the mapping of external material index -> material tag
 * could be these external materials are dynamic themselves - we'll extract them all but select the first
 */
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A9C, 0x290)] //9C1A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808072BD, 0x340)] //BD728080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D8F, 0x3E0)] //8F6D8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80806D8F, 0x450)] //8F6D8080
public struct S80806D8F
{
    [SchemaField(0x38, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S80809AF7> Unk38;

    [SchemaField(0xF0, TigerStrategy.DESTINY2_LATEST)]
    public AABB BoundingBox;

    [SchemaField(0x15C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x1DC, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x224, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x224, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x264, TigerStrategy.DESTINY2_LATEST)]
    public EntityModel Model;

    [SchemaField(0x1A8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public DynamicArray<S80801B12> TexturePlatesROI;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x310, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x310, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x350, TigerStrategy.DESTINY2_LATEST)]
    public Tag<S80806E1C> TexturePlates;

    //[SchemaField(0x3B8, TigerStrategy.DESTINY2_LATEST)]
    //public DynamicArray<S80809591> UnkChannels;

    [SchemaField(0x230, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x2D0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x360, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x3C0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x3C0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x400, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArrayUnloaded<SExternalMaterialMapEntry> ExternalMaterialsMap;

    [SchemaField(0x420, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArrayUnloaded<SInt16> Unk400;

    [SchemaField(0x260, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x300, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x398, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x3F0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x3F0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x430, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArrayUnloaded<S80806D98> Unk410;

    [SchemaField(0x270, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x310, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x3A0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x400, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x400, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x440, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArrayUnloaded<SMaterialHash> ExternalMaterials;
}

// Physics model resource, same layout as normal model resource?
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801BF6, 0x840)] //F61B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80807286, 0x360)] //86728080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D6C, 0x480)] //6C6D8080
public struct S80806D6C
{
    [SchemaField(0x38, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S80809AF7> Unk38;

    [SchemaField(0xF0, TigerStrategy.DESTINY2_LATEST)]
    public AABB BoundingBox;

    [SchemaField(0x15C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x1DC, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x224, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x224, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x264, TigerStrategy.DESTINY2_LATEST)]
    public EntityModel PhysicsModel;

    [SchemaField(0x230, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x2D0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x360, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x3C0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x3C0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x400, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArrayUnloaded<SExternalMaterialMapEntry> ExternalMaterialsMap;

    [SchemaField(0x420, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArrayUnloaded<SInt16> Unk400;

    [SchemaField(0x260, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x300, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x398, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x3F0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x3F0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x430, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArrayUnloaded<S80806D98> Unk410;

    [SchemaField(0x270, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x310, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x3A0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x400, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x400, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x440, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArrayUnloaded<SMaterialHash> ExternalMaterials;
}

//[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80809591, 0x28)] //91958080
//public struct S80809591
//{
//    [SchemaField(0x20)]
//    public TigerHash ChannelHash;
//}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801B12, 0x30)] //121B8080
public struct S80801B12
{
    [SchemaField(0x28)]
    public Tag<S80806E1C> TexturePlates;
}

#region Texture Plates

/// <summary>
/// Texture plate header that stores all the texture plates used for the EntityModel.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801C3C, 0x30)] //3C1C8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80806E1C, 0x38)] //1C6E8080
public struct S80806E1C
{
    public long FileSize;

    [SchemaField(0x24, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TexturePlate AlbedoPlate;
    public TexturePlate NormalPlate;
    public TexturePlate GStackPlate;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TexturePlate DyemapPlate;
}

/// <summary>
/// Texture plate that stores the data for placing textures on a canvas.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800147, 0x20)] //47018080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80809E91, 0x20)] //919E8080
public struct S80809E91
{
    public long FileSize;
    [SchemaField(0x10)]
    public DynamicArrayUnloaded<S80809E93> PlateTransforms;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800163, 0x14)] //63018080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80809E93, 0x14)] //939E8080
public struct S80809E93
{
    public Texture Texture;
    public IntVector2 Translation;
    public IntVector2 Scale;
}

#endregion

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801AFE, 0x8)] //FE1A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808072C5, 0x8)] //C5728080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D98, 0x8)] //986D8080
public struct S80806D98
{
    public ushort Unk00;
    public short Unk02;
    public ushort Unk04;
    public short Unk06;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A84, 0xC)] //841A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808072C4, 0xC)] //C4728080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D97, 0xC)] //976D8080
public struct SExternalMaterialMapEntry
{
    public int MaterialCount;
    public int MaterialStartIndex;
    public int Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A80, 0x1D0)] //801A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808072B8, 0x200)] //B8728080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D8A, 0x2E0)] //8A6D8080
public struct S80806D8A
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808006BD, 0x100)] //BD068080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808545, 0x100)] //45858080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808081DD, 0x100)] //DD818080
public struct S808081DD
{
    //[SchemaField(0x30)]
    //public DynamicArray<S808081DC> Unk30;
    //public DynamicArray<S80808640> Unk40;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080052B, 0xA0)] //2B058080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080853D, 0xA0)] //3D858080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808081D5, 0xA0)] //D5818080
public struct S808081D5
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800598, 0x10)] //98058080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808081DC, 0x40)] //DC818080
public struct S808081DC
{
    [SchemaField(0x0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicArray<S80809F4F> Unk20;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808001B0, 0x20)] //B0018080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809F75, 0x20)] //759F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809F4F, 0x20)] //4F9F8080
public struct S80809F4F
{
    public Tiger.Schema.Vector4 Rotation;
    public Tiger.Schema.Vector4 Translation;
}

[SchemaStruct(0x80808640, 8)] //40868080
public struct S80808640
{
    public ushort Unk00;
    public ushort Unk02;
    public uint Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800467, 0xA8)] //67048080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080853E, 0xB0)] //3E858080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808081D6, 0xC0)] //D6818080
public struct S808081D6
{
    [SchemaField(0x88, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x80, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x90, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArrayUnloaded<S80808642> NodeHierarchy;
    public DynamicArrayUnloaded<S80809F4F> DefaultInverseObjectSpaceTransforms;
    //public DynamicArrayUnloaded<SInt16> RangeIndexMap;
    //public DynamicArrayUnloaded<SInt16> InnerIndexMap;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080049A, 0xE0)] //9A048080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808546, 0xF0)] //46858080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808081DE, 0x108)] //DE818080
public struct S808081DE
{
    [SchemaField(0x88, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x80, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x90, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArrayUnloaded<S80808642> NodeHierarchy;
    public DynamicArrayUnloaded<S80809F4F> DefaultObjectSpaceTransforms;
    public DynamicArrayUnloaded<S80809F4F> DefaultInverseObjectSpaceTransforms;
    public DynamicArrayUnloaded<SInt16> RangeIndexMap;
    public DynamicArrayUnloaded<SInt16> InnerIndexMap;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808004F4, 0x10)] //F4048080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808A08, 0x10)] //088A8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808642, 0x10)] //42868080
public struct S80808642
{
    public TigerHash NodeHash;
    public int ParentNodeIndex;
    public int FirstChildNodeIndex;
    public int NextSiblingNodeIndex;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801AB5, 0x44)] //B51A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808073A5, 0xA0)] //A5738080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80806F07, 0xA0)] //076F8080
public struct SEntityModel  // Entity model
{
    public long FileSize;
    [SchemaField(0x10)]
    public DynamicArrayUnloaded<SEntityModelMesh> Meshes;
    [SchemaField(0x20)]
    public Vector4 Unk20;
    public long Unk30;
    [SchemaField(0x38)]
    public long UnkFlags38;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)] // Model transforms are stored in SEntityModelMesh for D1
    [SchemaField(0x50, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Vector4 ModelScale;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Vector4 ModelTranslation;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Vector2 TexcoordScale;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Vector2 TexcoordTranslation;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Vector4 Unk80;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public TigerHash Unk90;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public TigerHash Unk94;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801BBF, 0xA0)] //BF1B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80807378, 0x88)] //78738080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806EC5, 0x80)] //C56E8080
public struct SEntityModelMesh
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public Vector4 ModelScale;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public Vector4 ModelTranslation;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public Vector2 TexcoordScale;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public Vector2 TexcoordTranslation;

    [SchemaField(0x30, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public VertexBuffer Vertices1;  // vert file 1 (positions)
    public VertexBuffer Vertices2;  // vert file 2 (texcoords/normals)
    public VertexBuffer OldWeights;  // old weights
    public TigerHash Unk0C;  // nothing ever
    public IndexBuffer Indices;  // indices

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public VertexBuffer VertexColour;  // vertex colour

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public VertexBuffer SinglePassSkinningBuffer;  // single pass skinning buffer
    public int Zeros1C;
    public DynamicArrayUnloaded<S80806ECB> Parts;

    /// Range of parts to render per render stage
    /// Can be obtained as follows:
    ///
    ///     - Start = part_range_per_render_stage[stage]
    ///     - End = part_range_per_render_stage[stage + 1]
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, ArraySizeConst = 20)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, ArraySizeConst = 24)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, ArraySizeConst = 25)] // ArraySizeConst being the number of elements
    public ushort[] PartRangePerRenderStage;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, ArraySizeConst = 19)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, ArraySizeConst = 24)]
    public byte[] InputLayoutPerRenderStageBL;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, ArraySizeConst = 23)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    public short[] InputLayoutPerRenderStageSK;

    public Range GetRangeForStage(int stage)
    {
        int start = PartRangePerRenderStage[stage];
        int end = PartRangePerRenderStage[stage + 1];
        return new Range(start, end);
    }

    public int GetInputLayoutForStage(int stage)
    {
        return Strategy.IsPreBL() ? InputLayoutPerRenderStageSK[stage] : InputLayoutPerRenderStageBL[stage];
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801AEF, 0x24)] //EF1A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080737E, 0x20)] //7E738080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806ECB, 0x24)] //CB6E8080
public struct S80806ECB  // TODO use DCG to figure out what this is
{
    public Material Material;  // AA6D8080
    public short VariantShaderIndex;  // variant_shader_index
    public short PrimitiveType;
    public uint IndexOffset;
    public uint IndexCount;
    public uint Unk10;  // might be number of strips?

    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x14, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public short ExternalIdentifier;  // external_identifier
    public byte Unk16;
    public byte Unk17;

    // need to check this on WQ, theres no way its an int
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public int FlagsD2;

    [SchemaField(0x1C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public short FlagsD1; //??

    [SchemaField(0x1E, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x1A, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x1C, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public byte GearDyeChangeColorIndex;   // sbyte gear_dye_change_color_index
    public ELodCategory LodCategory;
    public byte Unk1E;
    public byte LodRun;  // lod_run
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public int Unk20; // variant_shader_index?

    public int GetFlags()
    {
        if (Strategy.IsD1())
            return FlagsD1;
        else
            return FlagsD2;
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A79, 0x210)] //791A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80807273, 0x240)] //73728080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D5B, 0x320)] //5B6D8080
public struct S80806D5B
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080209B, 0x330)] //9B208080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80805EDA, 0x150)] //DA5E8080
public struct S80805EDA
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802089, 0x270)] //89208080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80805EDB, 0x240)] //DB5E8080
public struct S80805EDB
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x108, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag<S80809723> Unk108;

    [SchemaField(0x114, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringHash EntityName;
}

[SchemaStruct(0x80809723, 0x48)] //23978080
public struct S80809723
{
    public long FileSize;
    public StringHash EntityName;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080881C, 0x50)] //1C888080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808412, 0x50)] //12848080
public struct S80808412
{
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808818, 0x90)] //18888080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080840E, 0xA0)] //0E848080
public struct S8080840E
{
    [SchemaField(0x78, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x88, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<S8080841B> Unk88;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808820, 0x18)] //20888080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080841B, 0x18)] //1B848080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080841B, 0x38)] //1B848080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080841B, 0x40)] //1B848080
public struct S8080841B
{
    [SchemaField(0x8, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S8080841D> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808822, 0x8)] //22888080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080841D, 0x18)] //1D848080
public struct S8080841D
{
    public TigerHash Unk00;
    public int Unk04;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Tag Entity;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808881, 0xEC)] //81888080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808485, 0xE0)] //85848080
public struct S80808881
{
    [SchemaField(0x74, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Tag Entity;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809A3B, 0x50)] //3B9A8080
public struct S80809A3B
{
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080948F, 0xC8)] //8F948080
public struct S8080948F
{
    [SchemaField(0xA8)]
    public DynamicArray<S80808356> UnkA8;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808356, 0x68)] //56838080
public struct S80808356
{
    [SchemaField(0x8)]
    public DynamicArray<S80808358> Table1; // Why...Are these all the same...?
    public DynamicArray<S80808358> Table2;
    public DynamicArray<S80808358> Table3;
    public DynamicArray<S80808358> Table4;
    public DynamicArray<S80808358> Table5;
    public DynamicArray<S80808358> Table6;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808358, 0x18)] //58838080
public struct S80808358
{
    public ResourceInTablePointer<SMapDataEntry>? Datatable;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80807EB6, 0x34)] //B67E8080
public struct S80807EB6
{
    [SchemaField(0x20)]
    public StringHash EntityName;
}

#region Named Bags

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809720, 0x18)] //20978080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x8080471D, 0x18)] //1D478080
public struct S8080471D
{
    public long FileSize;

    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<S808059D3> DestinationGlobalTagBags;

    // Below only for Beyond Light
    [SchemaField(0x8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer DestinationGlobalTagBagNameBL;

    [SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag<S80808930> DestinationGlobalTagBagBL;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808059D3, 0x10)] //D3598080
public struct S808059D3
{
    [SchemaField(0x0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public FileHash DestinationGlobalTagBag;

    [SchemaField(0x8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public StringPointer DestinationGlobalTagBagName;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809478, 0x18)] //80809478
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808930, 0x28)] //30898080
public struct S80808930
{
    public long FileSize;

    [SchemaField(0x8, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<S80808933> Entries;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080947A, 0x10)] //7A948080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808933, 0x20)] //33898080
public struct S80808933
{
    public StringPointer TagPath;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Tag Tag;  // if .pattern.tft, then Entity - if .budget_set.tft, then parent of itself

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public StringPointer TagNote;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808099D1, 0x8)] //808099D1
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080987E, 0x8)] //7E988080
public struct S8080987E
{
    public Tag Bag;
    public Tag UnkMapDatatable;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809F10, 0x58)] //80809F10
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809EED, 0x58)] //ED9E8080
public struct S80809EED
{
    public long FileSize;

    [SchemaField(0x18)]
    public Tag Unk18;

    [SchemaField(0x20, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<S80809EF1> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809F14, 0x10)] //149F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809EF1, 0x18)] //F19E8080
public struct S80809EF1
{
    public StringPointer TagPath;
    [SchemaField(0x8, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Tag Tag;  // if .pattern.tft, then Entity
}

#endregion

#region Audio

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802580, 0x598)] //80258080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x8080356E, 0x6b8)] //6E358080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080356E, 0x6d8)] //6E358080
public struct S8080356E
{
    [SchemaField(0x538, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x648, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x668, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x680, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S8080319B> PatternAudioGroups;

    [SchemaField(0x4E8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    [SchemaField(0x610, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)] // unsure if actually tag64
    public Tag<S80806FA3> FallbackAudioGroup;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808031DE, 0xD8)] //DE318080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x8080319B, 0x128)] //9B318080
public struct S8080319B
{
    public TigerHash WeaponContentGroup1Hash;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    //[SchemaField(0x18, Tag64 = true)]
    //public FileHash StringContainer;  // idk why but i presume debug strings, not important

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash WeaponContentGroup2Hash;  // "weaponContentGroupHash" from API
    // theres other stringcontainer stuff but skipping it

    [SchemaField(0x40, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Entity? WeaponSkeletonEntityD1;

    [SchemaField(0xA0, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Entity? WeaponSkeletonEntityD2;

    [SchemaField(0x88, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xD0, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag<S80806FA3> AudioGroup;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800C96, 0x18)] //960C8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80808C0D, 0x18)] //0D8C8080
public struct S80808C0D
{
    public long FileSize;
    public DynamicArray<S80808C0F> Audio;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808003FD, 0x18)] //FD038080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80808C0F, 0x18)] //0F8C8080
public struct S80808C0F
{
    public TigerHash WwiseEventHash;
    [SchemaField(0x8)]
    public DynamicArray<S80808C13> Sounds;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080060E, 0x24)] //0E068080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80808C13, 0x28)] //138C8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80808C13, 0x40)] //138C8080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80808C13, 0x38)] //138C8080
public struct S80808C13
{
    public short Unk00;
    public short Unk02;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    [SchemaField(0x10)]
    public StringPointer WwiseEventName;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public FileHash Data; // Can be WwiseSound or pattern entity
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802719, 0x530)] //19278080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80803197, 0x540)] //97318080
public struct S80803197
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802598, 0x90)] //98258080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80802CF6, 0xB0)] //F62C8080
public struct S80802CF6
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080248D, 0x208)] //8D248080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80802CF4, 0x338)] //F42C8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80802CF4, 0x358)] //F42C8080
public struct S80802CF4
{
    [SchemaField(0x188, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public ResourcePointer Unk188;

    [SchemaField(0x1D8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x2C8, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x318, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S80802CFA> PatternAudioGroups;

    [SchemaField(0xD0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    [SchemaField(0xD0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Entity? FallbackAudio1;

    [SchemaField(0xF0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    [SchemaField(0x100, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Entity? FallbackAudio2;

    [SchemaField(0x118, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Entity? FallbackAudio3;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802312, 0x140)] //12238080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80802CFA, 0x258)] //FA2C8080
[SchemaStruct(TigerStrategy.DESTINY2_LIGHTFALL_7366, 0x8080BCEE, 0x278)] //EEBC8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080B773, 0x2A8)] //73B78080 // Why does this keep changing???
public struct S80802CFA
{
    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash WeaponContentGroupHash; // "weaponContentGroupHash" from API
    public TigerHash Unk14;
    public TigerHash Unk18;

    [SchemaField(0x30, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash WeaponTypeHash1; // "weaponTypeHash" from API

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)] // These aren't obsolete but not needed, I don't think
    [SchemaField(0x60, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag Unk60;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag Unk78;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x90, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag Unk90;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xA8, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag UnkA8;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xC0, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag UnkC0;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xD8, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    public Tag UnkD8;

    [SchemaField(0x78, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xF0, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    [SchemaField(0x120, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag<S80806FA3> AudioEntityParent;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x120, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x150, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)] // ??
    public TigerHash WeaponTypeHash2; // "weaponTypeHash" from API

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x130, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    [SchemaField(0x160, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag Unk130;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x148, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    [SchemaField(0x178, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag Unk148;

    [SchemaField(0x80, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x118, TigerStrategy.DESTINY2_LIGHTFALL_7366)]
    [SchemaField(0x148, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public ResourcePointer Unk118;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x1C0, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    [SchemaField(0x1D0, TigerStrategy.DESTINY2_LIGHTFALL_7366, Tag64 = true)]
    [SchemaField(0x200, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag Unk1C0;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x1D8, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    [SchemaField(0x1E8, TigerStrategy.DESTINY2_LIGHTFALL_7366, Tag64 = true)]
    [SchemaField(0x218, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag Unk1D8;

    // public DynamicArray<S80809787> Unk1E8;
    // public DynamicArray<S80809784> Unk1F8;
    // public DynamicArray<S80802D06> Unk208;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x248, TigerStrategy.DESTINY2_WITCHQUEEN_6307, Tag64 = true)]
    [SchemaField(0x268, TigerStrategy.DESTINY2_LIGHTFALL_7366, Tag64 = true)]
    [SchemaField(0x298, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag Unk248;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80803165, 0x48)] //65318080
public struct S80803165
{
    public long FileSize;
    public StringPointer TagPath1;
    public Entity? Entity1;
    [SchemaField(0x18)]
    public StringPointer TagPath2;
    public Entity? Entity2;
    [SchemaField(0x28)]
    public StringPointer TagPath3;
    public Entity? Entity3;
    [SchemaField(0x38)]
    public StringPointer TagPath4;
    public Entity? Entity4;
}

[SchemaStruct(0x80802D09, 0xA0)] //092D8080
public struct S80802D09
{
    public long FileSize;
    public TigerHash Unk08;
    [SchemaField(0x18, Tag64 = true)]
    public Entity? Unk18;
    [SchemaField(0x30, Tag64 = true)]
    public Entity? Unk30;
    [SchemaField(0x48, Tag64 = true)]
    public Entity? Unk48;
    [SchemaField(0x60, Tag64 = true)]
    public Entity? Unk60;
    [SchemaField(0x78, Tag64 = true)]
    public Entity? Unk78;
    [SchemaField(0x90, Tag64 = true)]
    public Entity? Unk90;
}


// Turns out this can be used for more than just sounds, recent findings have seen it used for map global channels?
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800610, 0x270)] //10068080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808084E9, 0x2E8)] //E9848080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808179, 0x390)] //79818080
public struct S80808179
{
    [SchemaField(0x110, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x158, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x1A8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x1C8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)] // 100% gonna get changed with the next expansion, calling it now
    public DynamicArray<S808091F1> Array1;

    [SchemaField(0x120, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x168, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x1B8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x1D8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S808091F1> Array2;

    [SchemaField(0x130, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)] // idk if this is in D2, theres no space for it in SK but theres a 0x10 gap in BL
    public DynamicArray<S808091F1> D1Array3;

    [SchemaField(0x140, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x178, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x1D8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x1F8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S8080816F> Array3;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800629, 0x8)] //29068080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808093E6, 0x18)] //E6938080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808091F1, 0x18)] //F1918080
public struct S808091F1
{
    [SchemaField(0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public ResourcePointer Unk10; // B9678080, 40668080
}


[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080079A, 0x250)] //9A078080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808084D7, 0x250)] //D7848080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809479, 0x300)] //79948080
public struct S80809479
{
}

[SchemaStruct(0x80802D0A, 0x4C)] //0A2D8080
public struct S80802D0A
{
    [SchemaField(0x8, Tag64 = true)]
    public Entity? Unk08;
    [SchemaField(0x20, Tag64 = true)]
    public Entity? Unk20;
    [SchemaField(0x38, Tag64 = true)]
    public Entity? Unk38;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802340, 0x34)] //40238080
public struct S80802340
{
    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public Entity? Unk08;
    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public Entity? Unk20;
    [SchemaField(0x28, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public Entity? Unk38;
}

#endregion


[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809BD9, 0x190)] //D99B8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808092D8, 0x190)] //D8928080
public struct S808092D8
{
    [SchemaField(0x84)]
    public Tag<SMapDataTable> Unk84;
    [SchemaField(0x90)]
    public Vector4 Rotation;
    public Vector4 Translation;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808CEF, 0x60)] //EF8C8080
public struct S80808CEF
{
    [SchemaField(0x58)]
    public Tag<SMapDataTable> Unk58;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808098FA, 0x80)] //FA988080
public struct S808098FA
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S80809905> Unk58;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808CF8, 0x80)] //F88C8080
public struct S80808CF8
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S80809905> Unk58;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808098EF, 0x80)] //EF988080
public struct S808098EF
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S80809905> Unk58;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080416F, 0xE0)] //6F418080
public struct S8080416F
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S80809905> Unk58;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809826, 0x98)] //26988080
public struct S80809826
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S80809905> Unk58;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80804695, 0x90)] //95468080
public struct S80804695
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S80809905> Unk58;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809905, 0x10)] //05998080
public struct S80809905
{
    public TigerHash FNVHash;
    [SchemaField(0x8)]
    public ulong WorldID;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080906B, 0x28)] //6B908080
public struct S8080906B
{
    [SchemaField(0x8)]
    public DynamicArray<S80809D02> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809D02, 0x10)] //029D8080
public struct S80809D02
{
    public ResourceInTablePointer<S8080894D> Unk00;
    public RelativePointer Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080894D, 0xC)] //4D898080
public struct S8080894D
{
    public StringPointer Name;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808013E3, 0x1190)] //E3138080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80807C35, 0x1BD0)] //357C8080
public struct S80807C35
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801308, 0x2C0)] //08138080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808018, 0x448)] //18808080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80808018, 0x478)] //18808080
public struct S80808018
{
    [SchemaField(0x278, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x398, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x3C0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    [SchemaField(0x398, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)] // Got moved on Heresy Act 2 (3/11/25) update
    public Tag<S80807E4D> Unk3C0;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808013F3, 0x90)] //F3138080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80807E4D, 0x30)] //4D7E8080
public struct S80807E4D
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x2C, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public StringHash EntityName;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808046B5, 0x150)] //B5468080
public struct S808046B5
{
    [SchemaField(0x80)]
    public DynamicArray<S80804696> Unk80;

    [SchemaField(0xC0)]
    public Vector4 Rotation;
    public Vector4 Translation;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80804696, 0x78)] //96468080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80804696, 0x80)] //96468080
public struct S80804696
{
    [SchemaField(0x28, Tag64 = true)]
    public Tag<SMapDataTable> DataTable;
    public StringHash Name;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808026B9, 0x1D8)] //B9268080
public struct S808026B9
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808028DA, 0x98)] //DA288080
public struct S808028DA
{
    [SchemaField(0x68)]
    public Entity? Unk68;
}

// TODO: Other versions
// Entity carried weapon resource?
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808024B4, 0x480)] //B4248080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803EBB, 0x260)] //BB3E8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808031E9, 0x260)] //E9318080 // Entity Resource 0x10
public struct S808031E9
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802408, 0x248)] //08248080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803EBC, 0x278)] //BC3E8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808031EA, 0x2F8)] //EA318080 // Entity Resource 0x18
public struct S808031EA
{
    //[SchemaField(0x38)]
    //public DynamicArray<S80809AF7> Unk38;

    [SchemaField(0xE8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)] // ??
    public DynamicArray<S808031F8> UnkE8;

    [SchemaField(0x108, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x120, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x180, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x1C0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S808031F2> Unk1C0;

    [SchemaField(0x170, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x180, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x1E0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x220, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S80809AC9> Unk220;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080269D, 0x38)] //9D268080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803EC4, 0x40)] //C43E8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808031F2, 0x40)] //F2318080
public struct S808031F2
{
    //[SchemaField(0x8)]
    //public int Unk08;
    //[SchemaField(0x10)]
    //public int Unk10;
    //[SchemaField(0x28)]
    //public float Unk28;

    [SchemaField(0x28, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicArray<S808031F8> Unk30;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802BAE, 0x10)] //AE2B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808031F8, 0x10)] //F8318080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808031F8, 0x18)] //F8318080
public struct S808031F8
{
    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true), NoLoad]
    public Entity Entity;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80809AF7, 0x18)] //F79A8080
public struct S80809AF7
{
    public int Unk00;
    [SchemaField(0x8)]
    public DynamicArray<S80809AFB> Unk8;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80809AFB, 0x8)] //FB9A8080
public struct S80809AFB
{
    public TigerHash SwitchKey; // weapon_type "switch_key"
    public TigerHash Value; // weapon name "value"
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800608, 0x40)] //08068080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809C00, 0x40)] //009C8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809AC9, 0x30)] //C99A8080
public struct S80809AC9
{
    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Vector4 Rotation;
    public Vector4 Translation; // ZXY???

    [SchemaField(0x4, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x24, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public int ParentBoneIndex;

    [SchemaField(0x30, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public StringHash UnkName;
}

// Some other weapon entity resource, spider tank cannon 80C3EA34
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802663, 0xC60)] //63268080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803C23, 0xDE0)] //233C8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80802E74, 0xDE0)] //742E8080 // Entity Resource 0x10
public struct S80802E74
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802708, 0x598)] //08278080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803A00, 0x6E8)] //003A8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80802C28, 0x828)] //282C8080 // Resource 0x18
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80802C28, 0x858)] //282C8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80802C28, 0x880)] //282C8080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80802C28, 0x8C8)] //282C8080
public struct S80802C28
{
    [SchemaField(0x100, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x148, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x160, TigerStrategy.DESTINY2_SHADOWKEEP_2999)]
    [SchemaField(0x1B8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x1D8, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S80802C20> Unk1D8;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802B71, 0xA0)] //712B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808039F7, 0x90)] //F7398080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80802C20, 0x98)] //202C8080
public struct S80802C20
{
    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x18, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true), NoLoad]
    public Entity Entity;

    [SchemaField(0x88, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x80, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<S808036FD> Transform;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802793, 0x40)] //93278080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803E91, 0x40)] //913E8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808036FD, 0x30)] //FD368080
public struct S808036FD
{
    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Vector4 Rotation;
    public Vector4 Translation; // ZXY???

    [SchemaField(0x4, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x24, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public int ParentBoneIndex;

    [SchemaField(0x30, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public StringHash UnkName;
}

