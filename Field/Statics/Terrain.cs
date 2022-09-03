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
        // Directory.CreateDirectory(saveDirectory + "/Textures/Terrain/");
        // Directory.CreateDirectory(saveDirectory + "/Shaders/Terrain/");

        // if (Hash != "CAC7D380")
        // {
        //     return;
        // }
        // Uses triangle strip + only using first set of vertices and indices
        List<Part> parts = new List<Part>();
        var x = new List<float>();
        var y = new List<float>();
        var z = new List<float>();
        foreach (var partEntry in Header.MeshParts)
        {
            if (partEntry.DetailLevel == 0)
            {
                var part = MakePart(partEntry);
                parts.Add(part);
                x.AddRange(part.VertexPositions.Select(a => a.X));
                y.AddRange(part.VertexPositions.Select(a => a.Y));
                z.AddRange(part.VertexPositions.Select(a => a.Z));
                // Material
                if (partEntry.Material == null) continue;
                if(!Directory.Exists($"{saveDirectory}/Textures/"))
                    Directory.CreateDirectory($"{saveDirectory}/Textures/");

                partEntry.Material.SaveAllTextures($"{saveDirectory}/Textures/");
                part.Material = partEntry.Material;
                // dynamicPart.Material.SaveVertexShader(saveDirectory);
                if (bSaveShaders)
                {
                    partEntry.Material.SavePixelShader($"{saveDirectory}/Shaders/");
                    partEntry.Material.SaveVertexShader($"{saveDirectory}/Shaders/Vertex/");
                    partEntry.Material.SaveComputeShader($"{saveDirectory}/Shaders/");
                }
            }
        }
        var globalOffset = new Vector3(
            (Header.Unk10.X + Header.Unk20.X) / 2,
            (Header.Unk10.Y + Header.Unk20.Y) / 2,
            (Header.Unk10.Z + Header.Unk20.Z) / 2);

        Vector3 localOffset;
        int terrainTextureIndex = 14;
        for (int i = 0; i < Header.MeshGroups.Count; i++)
        {
            // Part part = MakePart(partEntry);
            // parts.Add(part);
            var partEntry = Header.MeshGroups[i];
            if (partEntry.Dyemap != null)
            {
                partEntry.Dyemap.SavetoFile($"{saveDirectory}/Textures/PS_{terrainTextureIndex}_{partEntry.Dyemap.Hash}");
            }
        }
        localOffset = new Vector3((x.Max() + x.Min())/2, (y.Max() + y.Min())/2, (z.Max() + z.Min())/2);
        foreach (var part in parts)
        {
            // scale by 1.99 ish, -1 for all sides, multiply by 512?
            TransformPositions(part, localOffset);
            TransformTexcoords(part);
        }
        
        fbxHandler.AddStaticToScene(parts, Hash);
        // For now we pre-transform it
        fbxHandler.InfoHandler.AddInstance(Hash, 1, Vector4.Zero, globalOffset);
        
        // We need to add these textures after the static is initialised
        foreach (var part in parts)
        {
            if (Header.MeshGroups[part.GroupIndex].Dyemap != null)
            {
                fbxHandler.InfoHandler.AddCustomTexture(part.Material.Hash, terrainTextureIndex, Header.MeshGroups[part.GroupIndex].Dyemap);
            }
        }
    }

    public Part MakePart(D2Class_846C8080 entry)
    {
        Part part = new Part();
        part.GroupIndex = entry.GroupIndex;
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
    
    private void TransformPositions(Part part, Vector3 localOffset)
    {
        for (int i = 0; i < part.VertexPositions.Count; i++)
        {
            // based on middle points
            part.VertexPositions[i] = new Vector4(  // technically actually 1008 1008 4 not 1024 1024 4?
                (part.VertexPositions[i].X - localOffset.X) * 1024,
                (part.VertexPositions[i].Y - localOffset.Y) * 1024,
                (part.VertexPositions[i].Z - localOffset.Z) * 4,
                part.VertexPositions[i].W
            );
            var b = 0;
        }
    }

    private void TransformTexcoords(Part part)
    {
        double scaleX, scaleY, translateX, translateY;
        if (Header.MeshGroups[part.GroupIndex].Unk20.Z == 0.0078125)
        {
            scaleX = 1 / 0.4375 * 2.28571428571 * 2;
            translateX = 0.333333; // 0 if no 2 * 2.285
        }
        else if (Header.MeshGroups[part.GroupIndex].Unk20.Z == -0.9765625)
        {
            scaleX = 32;
            translateX = -14;
        }
        else
        {
            throw new Exception("Unknown terrain uv scale x");
        }
        if (Header.MeshGroups[part.GroupIndex].Unk20.W == 0.0078125)
        {
            scaleY = -1 / 0.4375 * 2.28571428571 * 2;
            translateY = 0.333333;
        }
        else if (Header.MeshGroups[part.GroupIndex].Unk20.W == -0.9765625)
        {
            scaleY = -32;
            translateY = -14;
        }
        else
        {
            throw new Exception("Unknown terrain uv scale y");
        }
        for (int i = 0; i < part.VertexTexcoords.Count; i++)
        {
            part.VertexTexcoords[i] = new Vector2(
            part.VertexTexcoords[i].X * scaleX + translateX,
            part.VertexTexcoords[i].Y * scaleY + (1 - translateY)
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
    public List<D2Class_866C8080> MeshGroups;

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
    public List<D2Class_846C8080> MeshParts;
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
    public byte GroupIndex;
    public byte DetailLevel;
}