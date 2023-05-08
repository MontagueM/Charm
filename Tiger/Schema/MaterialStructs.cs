namespace Tiger.Schema;

[SchemaStruct("AA6D8080", 0x3D0)]
public struct SMaterial
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [SchemaField(0x70)]
    public ShaderBytecode VertexShader;
    [SchemaField(0x78)]
    public DynamicArray<TextureTag64> VSTextures;
    [SchemaField(0x90)]
    public DynamicArray<D2Class_09008080> Unk90;
    public DynamicArray<Vec4> UnkA0;
    public DynamicArray<D2Class_3F018080> UnkB0;
    public DynamicArray<Vec4> UnkC0;

    [SchemaField(0x2B0)]
    public ShaderBytecode PixelShader;
    [SchemaField(0x2B8)]
    public DynamicArray<TextureTag64> PSTextures;
    [SchemaField(0x2D0)]
    public DynamicArray<D2Class_09008080> Unk2D0;
    public DynamicArray<Vec4> Unk2E0;
    public DynamicArray<D2Class_3F018080> Unk2F0;
    public DynamicArray<Vec4> Unk300;
    [SchemaField(0x324)]
    public FileHash PSVector4Container;

    [SchemaField(0x340)]
    public ShaderBytecode ComputeShader;
    [SchemaField(0x348)]
    public DynamicArray<TextureTag64> CSTextures;
    [SchemaField(0x360)]
    public DynamicArray<D2Class_09008080> Unk360;
    public DynamicArray<Vec4> CSCbuffers0;
    public DynamicArray<D2Class_3F018080> Unk380;
    public DynamicArray<Vec4> CSCbuffers1;

}

[SchemaStruct("CF6D8080", 0x18)]
public struct TextureTag64
{
    public long TextureIndex;
    public Texture Texture;
}

[SchemaStruct("09008080", 1)]
public struct D2Class_09008080
{
    public byte Value;
}

[SchemaStruct("3F018080", 0x10)]
public struct D2Class_3F018080
{
    // [DestinyField(FieldType.TagHash64)]
    // public Tag Unk00;
}

[SchemaStruct("90008080", 0x10)]
public struct Vec4
{
    public Vector4 Vec;
}
