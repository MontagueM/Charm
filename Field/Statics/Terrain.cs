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
    public void LoadIntoFbxScene(FbxHandler fbxHandler, string saveDirectory, bool bSaveShaders, D2Class_7D6C8080 parentResource)
    {
        Directory.CreateDirectory(saveDirectory + "/Textures/Terrain/");
        Directory.CreateDirectory(saveDirectory + "/Shaders/Terrain/");

        if (Hash != "CAC7D380")
        {
            return;
        }
        // Uses triangle strip + only using first set of vertices and indices
        List<Part> parts = new List<Part>();
        var x = new List<float>();
        var y = new List<float>();
        var z = new List<float>();
        foreach (var partEntry in Header.Unk78)
        {
            if (partEntry.Unk0B == 0)
            {
                var part = MakePart(partEntry);
                parts.Add(part);
                x.AddRange(part.VertexPositions.Select(a => a.X));
                y.AddRange(part.VertexPositions.Select(a => a.Y));
                z.AddRange(part.VertexPositions.Select(a => a.Z));
                // Material
                if (partEntry.Material == null) continue;
                partEntry.Material.SaveAllTextures($"{saveDirectory}/Textures/Terrain/");
                part.Material = partEntry.Material;
                // dynamicPart.Material.SaveVertexShader(saveDirectory);
                if (bSaveShaders)
                {
                    partEntry.Material.SavePixelShader($"{saveDirectory}/Shaders/Terrain/");
                }
            }
        }
        var globalOffset = new Vector3(
            (Header.Unk10.X + Header.Unk20.X) / 2,
            (Header.Unk10.Y + Header.Unk20.Y) / 2,
            (Header.Unk10.Z + Header.Unk20.Z) / 2);


        Vector3 localOffset;
        for (int i = 0; i < Header.Unk50.Count; i++)
        {
            // Part part = MakePart(partEntry);
            // parts.Add(part);
            var partEntry = Header.Unk50[i];
            if (partEntry.Dyemap != null)
                partEntry.Dyemap.SavetoFile($"{saveDirectory}/Textures/Terrain/{Hash}_Dyemap_{i}_{partEntry.Dyemap.Hash}");
            // x.AddRange(part.VertexPositions.Select(a => a.X));
            // y.AddRange(part.VertexPositions.Select(a => a.Y));
            // z.AddRange(part.VertexPositions.Select(a => a.Z));
        }
        localOffset = new Vector3((x.Max() + x.Min())/2, (y.Max() + y.Min())/2, (z.Max() + z.Min())/2);
        var min = new Vector3(x.Min(), y.Min(), z.Min());
        var max = new Vector3(x.Max(), y.Max(), z.Max());
        foreach (var part in parts)
        {
            // scale by 1.99 ish, -1 for all sides, multiply by 512?
            TransformPositions(part, globalOffset, localOffset, min, max);
            TransformTexcoords(part);
            // TransformPositions2(part, globalOffset, localOffset, min, max);
        }
        
        fbxHandler.AddStaticToScene(parts, Hash);
    }

    public Part MakePart(D2Class_846C8080 entry)
    {
        Part part = new Part();
        part.GroupIndex = entry.Unk0A;
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
        
        // debug todo remove
        // foreach (var partVertexPosition in part.VertexPositions)
        // {
        //     if (partVertexPosition.W == 0)
        //     {
        //         part.VertexColours.Add(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        //     }
        //     else if (partVertexPosition.W == 1)
        //     {
        //         part.VertexColours.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
        //     }
        //     else
        //     {
        //         part.VertexColours.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
        //     }
        // }

        return part;
    }
    
    uint prevoffset = 0;
    
    public Part MakePart(D2Class_866C8080 entry)
    {
        Part part = new Part();
        part.Indices = Header.Indices1.Buffer.ParseBuffer(EPrimitiveType.TriangleStrip, prevoffset, entry.Unk40-prevoffset);
        prevoffset = entry.Unk40;
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
    
    private void TransformPositions(Part part, Vector3 globalOffset, Vector3 localOffset, Vector3 min, Vector3 max)
    {
        for (int i = 0; i < part.VertexPositions.Count; i++)
        {
            // based on middle points
            part.VertexPositions[i] = new Vector4(  // technically actually 1008 1008 4 not 1024 1024 4?
                (part.VertexPositions[i].X - localOffset.X) * 1.96875 * 512 + globalOffset.X,
                (part.VertexPositions[i].Y - localOffset.Y) * 1.96875 * 512 + globalOffset.Y,
                (part.VertexPositions[i].Z - localOffset.Z) * 0.0078125 * 512 + globalOffset.Z,
                part.VertexPositions[i].W
            );
            // interpolate
            var a = 0;
            // part.VertexPositions[i] = new Vector4(
            //         Header.Unk10.X + Math.Abs((Header.Unk20.X - Header.Unk10.X) * (part.VertexPositions[i].X - min.X) / (max.X - min.X)),
            //         Header.Unk10.Y + Math.Abs((Header.Unk20.Y - Header.Unk10.Y) * (part.VertexPositions[i].Y - min.Y) / (max.Y - min.Y)),
            //         Header.Unk10.Z + Math.Abs((Header.Unk20.Z - Header.Unk10.Z) * (part.VertexPositions[i].Z - min.Z) / (max.Z - min.Z)),
            //     part.VertexPositions[i].W
            // );
            var b = 0;
        }
    }
    
    private void TransformTexcoords(Part part)
    {
        for (int i = 0; i < part.VertexTexcoords.Count; i++)
        {
            part.VertexTexcoords[i] = new Vector2(
                part.VertexTexcoords[i].X * 10 + 8.8,
                part.VertexTexcoords[i].Y * -20 + 1 - 0
            );
        }
    }
    
    private void TransformPositions2(Part part, Vector3 globalOffset, Vector3 localOffset, Vector3 min, Vector3 max)
    {
        for (int i = 0; i < part.VertexPositions.Count; i++)
        {
            part.VertexPositions[i] = new Vector4(
                Header.Unk10.X + ((float)1.96875 * part.VertexPositions[i].X - 1) * 512,
                Header.Unk10.Y + ((float)1.96875 * part.VertexPositions[i].Y - 1) * 512,
                Header.Unk10.Z + ((float)1.96875 * part.VertexPositions[i].Z - 1),
                part.VertexPositions[i].W
            );
        }
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
    public uint Unk30;
    public uint Unk34;
    public uint Unk38;
    public uint Unk3C;
    public uint Unk40;
    public uint Unk44;
    public uint Unk48;
    public uint Unk4C;
    [DestinyField(FieldType.TagHash)]
    public TextureHeader Dyemap;
}

[StructLayout(LayoutKind.Sequential, Size = 0x0C)]
public struct D2Class_846C8080
{
    [DestinyField(FieldType.TagHash)]
    public Material Material;
    public uint IndexOffset;
    public ushort IndexCount;
    public byte Unk0A;
    public byte Unk0B;
}