using Microsoft.VisualBasic.FileIO;

namespace Tiger.Schema;

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "E8718080", 0x400)]
public struct SMaterial_SK
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [SchemaField(0x48)]
    public ShaderBytecode VertexShader;
    [SchemaField(0x50)]
    public DynamicArray<STextureTag> VSTextures;
    [SchemaField(0x68)]
    public DynamicArray<D2Class_09008080> Unk68;
    public DynamicArray<Vec4> Unk78;
    public DynamicArray<SDirectXSamplerTag> VS_Samplers;
    public DynamicArray<Vec4> Unk98;

    [SchemaField(0x2C8)]
    public ShaderBytecode? PixelShader;
    [SchemaField(0x2D0)]
    public DynamicArray<STextureTag> PSTextures;
    [SchemaField(0x2E8)]
    public DynamicArray<D2Class_09008080> Unk2E8;
    public DynamicArray<Vec4> Unk2F8;
    public DynamicArray<SDirectXSamplerTag> PS_Samplers;
    public DynamicArray<Vec4> Unk310;
    [SchemaField(0x34C)]
    public FileHash PSVector4Container;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "AA6D8080", 0x3B0)]
public struct SMaterial_BL
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [SchemaField(0x58)]
    public ShaderBytecode VertexShader;
    [SchemaField(0x60)]
    public DynamicArray<STextureTag64> VSTextures;
    //TODO: change these names
    [SchemaField(0x78)]
    public DynamicArray<D2Class_09008080> Unk90;
    public DynamicArray<Vec4> UnkA0;
    public DynamicArray<SDirectXSamplerTag> VS_Samplers;
    public DynamicArray<Vec4> UnkC0;

    [SchemaField(0x298)]
    public ShaderBytecode? PixelShader;
    [SchemaField(0x2A0)]
    public DynamicArray<STextureTag64> PSTextures;
    [SchemaField(0x2B8)]
    public DynamicArray<D2Class_09008080> Unk2D0;
    public DynamicArray<Vec4> Unk2E0;
    public DynamicArray<SDirectXSamplerTag> PS_Samplers;
    public DynamicArray<Vec4> Unk300;
    [SchemaField(0x30C)]
    public FileHash PSVector4Container;

    [SchemaField(0x328)]
    public ShaderBytecode? ComputeShader;
    [SchemaField(0x330)]
    public DynamicArray<STextureTag64> CSTextures;
    [SchemaField(0x348)]
    public DynamicArray<D2Class_09008080> Unk360;
    public DynamicArray<Vec4> CSCbuffers0;
    public DynamicArray<SDirectXSamplerTag> CS_Samplers;
    public DynamicArray<Vec4> CSCbuffers1;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "AA6D8080", 0x3D0)]
public struct SMaterial_WQ
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [SchemaField(0x70)]
    public ShaderBytecode VertexShader;
    [SchemaField(0x78)]
    public DynamicArray<STextureTag64> VSTextures;
    [SchemaField(0x90)]
    public DynamicArray<D2Class_09008080> Unk90;
    public DynamicArray<Vec4> UnkA0;
    public DynamicArray<SDirectXSamplerTag> VS_Samplers;
    public DynamicArray<Vec4> UnkC0;

    [SchemaField(0x2B0)]
    public ShaderBytecode? PixelShader;
    [SchemaField(0x2B8)]
    public DynamicArray<STextureTag64> PSTextures;
    [SchemaField(0x2D0)]
    public DynamicArray<D2Class_09008080> Unk2D0;
    public DynamicArray<Vec4> Unk2E0;
    public DynamicArray<SDirectXSamplerTag> PS_Samplers;
    public DynamicArray<Vec4> Unk300;
    [SchemaField(0x324)]
    public FileHash PSVector4Container;

    [SchemaField(0x340)]
    public ShaderBytecode? ComputeShader;
    [SchemaField(0x348)]
    public DynamicArray<STextureTag64> CSTextures;
    [SchemaField(0x360)]
    public DynamicArray<D2Class_09008080> Unk360;
    public DynamicArray<Vec4> CSCbuffers0;
    public DynamicArray<SDirectXSamplerTag> CS_Samplers;
    public DynamicArray<Vec4> CSCbuffers1;
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
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "3F018080", 0x10)]
public struct SDirectXSamplerTag
{
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
