using System.Runtime.InteropServices;
using Field.General;
using Field.Models;
using Field.Textures;

namespace Field;


public class Terrain : Tag
{
    public D2Class_816C8080 Header;
    
    public Terrain(TagHash hash) : base(hash)
    {
        
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_816C8080>();
    }
    
    // To test use edz.strike_hmyn and alleys_a adf6ae80
    public void LoadIntoFbxScene(FbxHandler fbxHandler, string savePath, bool bSaveShaders, D2Class_7D6C8080 parentResource)
    {
        // Uses triangle strip + only using first set of vertices and indices
        List<Part> parts = new List<Part>();
        Header.Unk78.ForEach(part => parts.Add(MakePart(part)));
        fbxHandler.AddStaticToScene(parts, Hash);
    }

    public Part MakePart(D2Class_846C8080 entry)
    {
        Part part = new Part();
        part.Indices = Header.Indices1.Buffer.ParseBuffer(EPrimitiveType.TriangleStrip, entry.IndexOffset, entry.IndexCount);
        // Get unique vertex indices we need to get data for
        HashSet<uint> uniqueVertexIndices = new HashSet<uint>();
        foreach (UIntVector3 index in part.Indices)
        {
            uniqueVertexIndices.Add(index.X);
            uniqueVertexIndices.Add(index.Y);
            uniqueVertexIndices.Add(index.Z);
        }
        part.VertexIndices = uniqueVertexIndices.ToList();

        Header.Vertices1.Buffer.ParseBuffer(part, uniqueVertexIndices);
        Header.Vertices2.Buffer.ParseBuffer(part, uniqueVertexIndices);

        return part;
    }
}

/// <summary>
/// Terrain data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_7D6C8080
{
    [DestinyOffset(0x10)]
    public short Unk10;  // tile x-y coords?
    public short Unk12;
    public DestinyHash Unk14;
    [DestinyField(FieldType.TagHash)]
    public Terrain Terrain;
    [DestinyField(FieldType.TagHash)]
    public Tag<D2Class_B1938080> TerrainBounds;
}

/// <summary>
/// Terrain header.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0xB0)]
public struct D2Class_816C8080
{
    public long FileSize;
    [DestinyOffset(0x10)]
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    [DestinyOffset(0x50), DestinyField(FieldType.TablePointer)]
    public List<D2Class_866C8080> Unk50;

    [DestinyField(FieldType.TagHash)]
    public VertexHeader Vertices1;
    [DestinyField(FieldType.TagHash)]
    public VertexHeader Vertices2;
    [DestinyField(FieldType.TagHash)]
    public IndexHeader Indices1;
    [DestinyField(FieldType.TagHash)]
    public Material Unk6C;
    [DestinyField(FieldType.TagHash)]
    public Material Unk70;
    [DestinyOffset(0x78), DestinyField(FieldType.TablePointer)]
    public List<D2Class_846C8080> Unk78;
    [DestinyField(FieldType.TagHash)]
    public VertexHeader Vertices3;
    [DestinyField(FieldType.TagHash)]
    public VertexHeader Vertices4;
    [DestinyField(FieldType.TagHash)]
    public IndexHeader Indices2;
    [DestinyOffset(0x98)]
    public int Unk98;
    public int Unk9C;
    public int UnkA0;
}

[StructLayout(LayoutKind.Sequential, Size = 0x60)]
public struct D2Class_866C8080
{
    public float Unk00;
    public float Unk04;
    public float Unk08;
    public float Unk0C;
    public float Unk10;
    public float Unk14;
    public float Unk18;
    [DestinyOffset(0x20)]
    public Vector4 Unk20;
    public int Unk30;
    public int Unk34;
    public int Unk38;
    public int Unk3C;
    public int Unk40;
    public int Unk44;
    public int Unk48;
    public int Unk4C;
    [DestinyField(FieldType.TagHash)]
    public TextureHeader Dyemap;
}

[StructLayout(LayoutKind.Sequential, Size = 0x0C)]
public struct D2Class_846C8080
{
    [DestinyField(FieldType.TagHash)]
    public Material Unk00;
    public uint IndexOffset;
    public ushort IndexCount;
    public byte Unk0A;
    public byte Unk0B;
}