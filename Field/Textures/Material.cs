using System.Runtime.InteropServices;
using Field.Entities;
using Field.General;

namespace Field.Textures;

public class Material : Tag
{
    public D2Class_AA6D8080 Header;

    
    public Material(TagHash hash) : base(hash)
    {
    }
    public Material(string hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_AA6D8080>();
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x3D0)]
public struct D2Class_AA6D8080
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [DestinyOffset(0x70), DestinyField(FieldType.TagHash)]
    public ShaderHeader VertexShader;
    [DestinyOffset(0x78), DestinyField(FieldType.TablePointer)]
    public List<D2Class_CF6D8080> VSTextures;
    [DestinyOffset(0x90), DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk90;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> UnkA0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_3F018080> UnkB0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> UnkC0;
    
    [DestinyOffset(0x2B0), DestinyField(FieldType.TagHash)]
    public ShaderHeader PixelShader;
    [DestinyOffset(0x2B8), DestinyField(FieldType.TablePointer)]
    public List<D2Class_CF6D8080> PSTextures;
    [DestinyOffset(0x2D0), DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk2D0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk2E0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_3F018080> Unk2F0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk300;
    
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_CF6D8080
{
    public long TextureIndex;
    [DestinyField(FieldType.TagHash64)]
    public TextureHeader Texture;
}

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct D2Class_09008080
{
    public byte Value;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_3F018080
{
    [DestinyField(FieldType.TagHash64)]
    public Tag Unk00;
}
