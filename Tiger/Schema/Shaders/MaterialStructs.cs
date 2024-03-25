namespace Tiger.Schema;

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D71A8080", 0x488)]
public struct SMaterial_ROI // PS4 / ORBIS shaders aren't able to be decompiled :(
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;
    [SchemaField(0x24)]
    public ushort Unk24; // ??

    //[SchemaField(0x28)] // Vertex shader can be here also so idk what this is
    //public ShaderBytecode? UnkShader;
    //[SchemaField(0x120)]
    //public DynamicArray<Vec4> UnkShaderBuffer;
    //[SchemaField(0x14C)]
    //public FileHash UnkShaderVector4Container;

    [SchemaField(0x168)]
    public ShaderBytecode? VertexShader;
    [SchemaField(0x178)]
    public DynamicArray<STextureTag> VSTextures;
    [SchemaField(0x190)]
    public DynamicArray<D2Class_09008080> VS_TFX_Bytecode;
    [SchemaField(0x1B0)]
    public DynamicArray<SDirectXSamplerTagSK> VS_Samplers;
    public DynamicArray<Vec4> VS_TFX_Bytecode_Constants;
    public DynamicArray<Vec4> VS_CBuffers;
    [SchemaField(0x1EC)]
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
    public ushort Unk18; // ??
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
public struct SMaterial_BL
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;
    [SchemaField(0x20)]
    public ushort Unk20; // ??
    [SchemaField(0x58)]
    public ShaderBytecode VertexShader;
    [SchemaField(0x60)]
    public DynamicArray<STextureTag64> VSTextures;
    [SchemaField(0x78)]
    public DynamicArray<D2Class_09008080> VS_TFX_Bytecode;
    public DynamicArray<Vec4> VS_TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTagBL> VS_Samplers;
    public DynamicArray<Vec4> VS_CBuffers;
    [SchemaField(0xCC)]
    public FileHash VSVector4Container;

    [SchemaField(0x298)]
    public ShaderBytecode? PixelShader;
    [SchemaField(0x2A0)]
    public DynamicArray<STextureTag64> PSTextures;
    [SchemaField(0x2B8)]
    public DynamicArray<D2Class_09008080> PS_TFX_Bytecode;
    public DynamicArray<Vec4> PS_TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTagBL> PS_Samplers;
    public DynamicArray<Vec4> PS_CBuffers;
    [SchemaField(0x30C)]
    public FileHash PSVector4Container;

    [SchemaField(0x328)]
    public ShaderBytecode? ComputeShader;
    [SchemaField(0x330)]
    public DynamicArray<STextureTag64> CSTextures;
    [SchemaField(0x348)]
    public DynamicArray<D2Class_09008080> CS_TFX_Bytecode;
    public DynamicArray<Vec4> CS_TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTagBL> CS_Samplers;
    public DynamicArray<Vec4> CS_CBuffers;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "AA6D8080", 0x3D0)]
public struct SMaterial_WQ
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;
    [SchemaField(0x20)]
    public ushort Unk20; // ??
    [SchemaField(0x70)]
    public ShaderBytecode VertexShader;
    [SchemaField(0x78)]
    public DynamicArray<STextureTag64> VSTextures;
    [SchemaField(0x90)]
    public DynamicArray<D2Class_09008080> VS_TFX_Bytecode;
    public DynamicArray<Vec4> VS_TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTagBL> VS_Samplers;
    public DynamicArray<Vec4> VS_CBuffers;
    [SchemaField(0xE4)]
    public FileHash VSVector4Container;

    [SchemaField(0x2B0)]
    public ShaderBytecode? PixelShader;
    [SchemaField(0x2B8)]
    public DynamicArray<STextureTag64> PSTextures;
    [SchemaField(0x2D0)]
    public DynamicArray<D2Class_09008080> PS_TFX_Bytecode;
    public DynamicArray<Vec4> PS_TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTagBL> PS_Samplers;
    public DynamicArray<Vec4> PS_CBuffers;
    [SchemaField(0x324)]
    public FileHash PSVector4Container;

    [SchemaField(0x340)]
    public ShaderBytecode? ComputeShader;
    [SchemaField(0x348)]
    public DynamicArray<STextureTag64> CSTextures;
    [SchemaField(0x360)]
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

[SchemaStruct("F3738080", 0x10)]
public struct D2Class_F3738080
{
    // [DestinyField(FieldType.TagHash64)]
    // public Tag Unk00;
}

[SchemaStruct("90008080", 0x10)]
public struct Vec4
{
    public Vector4 Vec;
}
