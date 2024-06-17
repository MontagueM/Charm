namespace Tiger.Schema;

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D71A8080", 0x488)]
public struct SMaterial_ROI // PS4 / ORBIS shaders aren't able to be decompiled :(
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [SchemaField(0x18)]
    public ScopeBitsD1 UsedScopes;
    public ScopeBitsD1 CompatibleScopes;

    [SchemaField(0x20)]
    public uint Unk20;

    [SchemaField(0x28)]
    public ShaderBytecode? VertexShader;
    [SchemaField(0x38)]
    public DynamicArray<STextureTag> VSTextures;
    [SchemaField(0x50)]
    public DynamicArray<D2Class_09008080> VS_TFX_Bytecode;
    [SchemaField(0x70)]
    public DynamicArray<SDirectXSamplerTagSK> VS_Samplers;
    public DynamicArray<Vec4> VS_TFX_Bytecode_Constants;
    public DynamicArray<Vec4> VS_CBuffers;
    [SchemaField(0xAC)]
    public FileHash VSVector4Container;

    [SchemaField(0x2A8)]
    public ShaderBytecode? PixelShader;
    [SchemaField(0x2B8)]
    public DynamicArray<STextureTag> PSTextures;
    [SchemaField(0x2D0)]
    public DynamicArray<D2Class_09008080> PS_TFX_Bytecode;
    [SchemaField(0x2F0)]
    public DynamicArray<SDirectXSamplerTagSK> PS_Samplers;
    public DynamicArray<Vec4> PS_TFX_Bytecode_Constants;
    public DynamicArray<Vec4> PS_CBuffers;
    [SchemaField(0x32C)]
    public FileHash PSVector4Container;

    //[SchemaField(0x368)]
    //public ShaderBytecode? ComputeShader;
    //[SchemaField(0x370)]
    //public DynamicArray<STextureTag> CSTextures;
    //[SchemaField(0x388)]
    //public DynamicArray<D2Class_09008080> CS_TFX_Bytecode;
    //public DynamicArray<Vec4> CS_TFX_Bytecode_Constants;
    //public DynamicArray<SDirectXSamplerTagSK> CS_Samplers;
    //public DynamicArray<Vec4> CS_CBuffers;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "E8718080", 0x400)]
public struct SMaterial_SK
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [SchemaField(0x18)]
    public ScopeBitsSK UsedScopes;
    public ScopeBitsSK CompatibleScopes;

    [SchemaField(0x48)]
    public ShaderBytecode VertexShader;
    [SchemaField(0x50)]
    public DynamicArray<STextureTag> VSTextures;
    [SchemaField(0x68)]
    public DynamicArray<D2Class_09008080> VS_TFX_Bytecode;
    public DynamicArray<Vec4> VS_TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTagSK> VS_Samplers;
    public DynamicArray<Vec4> VS_CBuffers;
    [SchemaField(0xCC)]
    public FileHash VSVector4Container;

    [SchemaField(0x2C8)]
    public ShaderBytecode? PixelShader;
    [SchemaField(0x2D0)]
    public DynamicArray<STextureTag> PSTextures;
    [SchemaField(0x2E8)]
    public DynamicArray<D2Class_09008080> PS_TFX_Bytecode;
    public DynamicArray<Vec4> PS_TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTagSK> PS_Samplers;
    public DynamicArray<Vec4> PS_CBuffers;
    [SchemaField(0x34C)]
    public FileHash PSVector4Container;

    [SchemaField(0x368)]
    public ShaderBytecode? ComputeShader;
    [SchemaField(0x370)]
    public DynamicArray<STextureTag> CSTextures;
    [SchemaField(0x388)]
    public DynamicArray<D2Class_09008080> CS_TFX_Bytecode;
    public DynamicArray<Vec4> CS_TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTagSK> CS_Samplers;
    public DynamicArray<Vec4> CS_CBuffers;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "AA6D8080", 0x3B0)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "AA6D8080", 0x3D0)]
public struct SMaterial_BL
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [SchemaField(0x20)]
    public ScopeBits UsedScopes;
    public ScopeBits CompatibleScopes;

    [SchemaField(0x58, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public ShaderBytecode VertexShader;

    [SchemaField(0x60, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<STextureTag64> VSTextures;

    [SchemaField(0x78, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x90, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<D2Class_09008080> VS_TFX_Bytecode;
    public DynamicArray<Vec4> VS_TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTagBL> VS_Samplers;
    public DynamicArray<Vec4> VS_CBuffers;

    [SchemaField(0xCC, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0xE4, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public FileHash VSVector4Container;

    [SchemaField(0x298, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x2B0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public ShaderBytecode? PixelShader;

    [SchemaField(0x2A0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x2B8, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<STextureTag64> PSTextures;

    [SchemaField(0x2B8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x2D0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<D2Class_09008080> PS_TFX_Bytecode;
    public DynamicArray<Vec4> PS_TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTagBL> PS_Samplers;
    public DynamicArray<Vec4> PS_CBuffers;

    [SchemaField(0x30C, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x324, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public FileHash PSVector4Container;

    [SchemaField(0x328, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x340, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public ShaderBytecode? ComputeShader;

    [SchemaField(0x330, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x348, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<STextureTag64> CSTextures;

    [SchemaField(0x348, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x360, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<D2Class_09008080> CS_TFX_Bytecode;
    public DynamicArray<Vec4> CS_TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTagBL> CS_Samplers;
    public DynamicArray<Vec4> CS_CBuffers;
}

[SchemaStruct("11728080", 0x8)]
public struct STextureTag
{
    public uint TextureIndex;
    public Texture Texture;

    public static implicit operator STextureTag(STextureTag64 tag) => new() { TextureIndex = (uint)tag.TextureIndex, Texture = tag.Texture };
}

[SchemaStruct("CF6D8080", 0x18)]
public struct STextureTag64
{
    public long TextureIndex;
    [Tag64]
    public Texture Texture;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "F3738080", 0x10)]
public struct SDirectXSamplerTagSK
{
    public DirectXSampler Samplers;
}

// todo make tag64 bound to strategy
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "3F018080", 0x10)]
public struct SDirectXSamplerTagBL
{
    [Tag64]
    public DirectXSampler Samplers;
}


[SchemaStruct("09008080", 1)]
public struct D2Class_09008080
{
    public byte Value;
}

[SchemaStruct("90008080", 0x10)]
public struct Vec4
{
    public Vector4 Vec;
}

[Flags]
public enum ScopeBits : ulong
{
    FRAME = 1UL << 0,
    VIEW = 1UL << 1,
    RIGID_MODEL = 1UL << 2,
    EDITOR_MESH = 1UL << 3,
    EDITOR_TERRAIN = 1UL << 4,
    CUI_VIEW = 1UL << 5,
    CUI_OBJECT = 1UL << 6,
    SKINNING = 1UL << 7,
    SPEEDTREE = 1UL << 8,
    CHUNK_MODEL = 1UL << 9,
    DECAL = 1UL << 10,
    INSTANCES = 1UL << 11,
    SPEEDTREE_LOD_DRAWCALL_DATA = 1UL << 12,
    TRANSPARENT = 1UL << 13,
    TRANSPARENT_ADVANCED = 1UL << 14,
    SDSM_BIAS_AND_SCALE_TEXTURES = 1UL << 15,
    TERRAIN = 1UL << 16,
    POSTPROCESS = 1UL << 17,
    CUI_BITMAP = 1UL << 18,
    CUI_STANDARD = 1UL << 19,
    UI_FONT = 1UL << 20,
    CUI_HUD = 1UL << 21,
    PARTICLE_TRANSFORMS = 1UL << 22,
    PARTICLE_LOCATION_METADATA = 1UL << 23,
    CUBEMAP_VOLUME = 1UL << 24,
    GEAR_PLATED_TEXTURES = 1UL << 25,
    GEAR_DYE_0 = 1UL << 26,
    GEAR_DYE_1 = 1UL << 27,
    GEAR_DYE_2 = 1UL << 28,
    GEAR_DYE_DECAL = 1UL << 29,
    GENERIC_ARRAY = 1UL << 30,
    GEAR_DYE_SKIN = 1UL << 31,
    GEAR_DYE_LIPS = 1UL << 32,
    GEAR_DYE_HAIR = 1UL << 33,
    GEAR_DYE_FACIAL_LAYER_0_MASK = 1UL << 34,
    GEAR_DYE_FACIAL_LAYER_0_MATERIAL = 1UL << 35,
    GEAR_DYE_FACIAL_LAYER_1_MASK = 1UL << 36,
    GEAR_DYE_FACIAL_LAYER_1_MATERIAL = 1UL << 37,
    PLAYER_CENTERED_CASCADED_GRID = 1UL << 38,
    GEAR_DYE_012 = 1UL << 39,
    COLOR_GRADING_UBERSHADER = 1UL << 40,
}

[Flags]
public enum ScopeBitsSK : uint
{
    FRAME = 1U << 0,
    VIEW = 1U << 1,
    RIGID_MODEL = 1U << 2,
    EDITOR_MESH = 1U << 3,
    EDITOR_TERRAIN = 1U << 4,
    CUI_VIEW = 1U << 5,
    CUI_OBJECT = 1U << 6,
    SKINNING = 1U << 7,
    SPEEDTREE = 1U << 8,
    CHUNK_MODEL = 1U << 9,
    DECAL = 1U << 10,
    INSTANCES = 1U << 11,
    SPEEDTREE_LOD_DRAWCALL_DATA = 1U << 12,
    TRANSPARENT = 1U << 13,
    TRANSPARENT_ADVANCED = 1U << 14,
    SDSM_BIAS_AND_SCALE_TEXTURES = 1U << 15,
    TERRAIN = 1U << 16,
    POSTPROCESS = 1U << 17,
    CUI_BITMAP = 1U << 18,
    CUI_STANDARD = 1U << 19,
    UI_FONT = 1U << 20,
    CUI_HUD = 1U << 21,
    PARTICLE_TRANSFORMS = 1U << 22,
    PARTICLE_LOCATION_METADATA = 1U << 23,
    CUBEMAP_VOLUME = 1U << 24,
    GEAR_PLATED_TEXTURES = 1U << 25,
    GEAR_DYE_0 = 1U << 26,
    GEAR_DYE_1 = 1U << 27,
    GEAR_DYE_2 = 1U << 28,
    GEAR_DYE_DECAL = 1U << 29,
    GENERIC_ARRAY = 1U << 30,
    WEATHER = 1U << 31
}

[Flags]
public enum ScopeBitsD1 : uint
{
    FRAME = 1U << 0,
    VIEW = 1U << 1,
    RIGID_MODEL = 1U << 2,
    EDITOR_MESH = 1U << 3,
    EDITOR_TERRAIN = 1U << 4,
    CUI_VIEW = 1U << 5,
    CUI_OBJECT = 1U << 6,
    SKINNING = 1U << 7,
    SPEEDTREE = 1U << 8,
    CHUNK_MODEL = 1U << 9,
    DECAL = 1U << 10,
    INSTANCES = 1U << 11,
    SPEEDTREE_INSTANCE_DATA = 1U << 12,
    SPEEDTREE_LOD_DRAWCALL_DATA = 1U << 13,
    TRANSPARENT = 1U << 14,
    TRANSPARENT_ADVANCED = 1U << 15,
    SDSM_BIAS_AND_SCALE_TEXTURES = 1U << 16,
    TERRAIN = 1U << 17,
    POSTPROCESS = 1U << 18,
    CUI_BITMAP = 1U << 19,
    CUI_STANDARD = 1U << 20,
    UI_FONT = 1U << 21,
    CUI_HUD = 1U << 22,
}
