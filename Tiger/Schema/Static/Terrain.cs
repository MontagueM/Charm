using System.Runtime.InteropServices;

namespace Tiger.Schema.Static;


public class Terrain : Tag<D2Class_816C8080>
{
    public Terrain(FileHash hash) : base(hash)
    {

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
        List<StaticPart> parts = new();
        var x = new List<float>();
        var y = new List<float>();
        var z = new List<float>();
        foreach (var partEntry in _tag.StaticParts)
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
                    partEntry.Material.SavePixelShader($"{saveDirectory}/Shaders/", true);
                    partEntry.Material.SaveVertexShader($"{saveDirectory}/Shaders/Vertex/");
                    partEntry.Material.SaveComputeShader($"{saveDirectory}/Shaders/");
                }
            }
        }
        var globalOffset = new Vector3(
            (_tag.Unk10.X + _tag.Unk20.X) / 2,
            (_tag.Unk10.Y + _tag.Unk20.Y) / 2,
            (_tag.Unk10.Z + _tag.Unk20.Z) / 2);

        Vector3 localOffset;
        int terrainTextureIndex = 14;
        for (int i = 0; i < _tag.MeshGroups.Count; i++)
        {
            // Part part = MakePart(partEntry);
            // parts.Add(part);
            var partEntry = _tag.MeshGroups[i];
            if (partEntry.Dyemap != null)
            {
                partEntry.Dyemap.SavetoFile($"{saveDirectory}/Textures/{partEntry.Dyemap.Hash}");
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
            if (_tag.MeshGroups[part.GroupIndex].Dyemap != null)
            {
                fbxHandler.InfoHandler.AddCustomTexture(part.Material.Hash, terrainTextureIndex, _tag.MeshGroups[part.GroupIndex].Dyemap);

                if (File.Exists($"{saveDirectory}/Shaders/Source2/materials/{part.Material.Hash}.vmat"))
                {
                    //Get the last line of the vmat file
                    var vmat = File.ReadAllLines($"{saveDirectory}/Shaders/Source2/materials/{part.Material.Hash}.vmat");
                    var lastLine = vmat[vmat.Length - 1];

                    //Insert a new line before the last line
                    var newVmat = vmat.Take(vmat.Length - 1).ToList();
                    newVmat.Add($"  TextureT14 " + $"\"{_tag.MeshGroups[part.GroupIndex].Dyemap.Hash}.png\"");
                    newVmat.Add(lastLine);
                    File.WriteAllLines($"{saveDirectory}/Shaders/Source2/materials/{part.Material.Hash}.vmat", newVmat);
                }
            }
        }
    }

    public StaticPart MakePart(D2Class_846C8080 entry)
    {
        StaticPart part = new(entry);
        part.GroupIndex = entry.GroupIndex;
        part.Indices = _tag.Indices1.GetIndexData(PrimitiveType.TriangleStrip, entry.IndexOffset, entry.IndexCount);
        // Get unique vertex indices we need to get data for
        HashSet<uint> uniqueVertexIndices = new HashSet<uint>();
        foreach (UIntVector3 index in part.Indices)
        {
            uniqueVertexIndices.Add(index.X);
            uniqueVertexIndices.Add(index.Y);
            uniqueVertexIndices.Add(index.Z);
        }
        part.VertexIndices = uniqueVertexIndices.ToList();

        _tag.Vertices1.ReadVertexData(part, uniqueVertexIndices);
        _tag.Vertices2.ReadVertexData(part, uniqueVertexIndices);

        return part;
    }

    private void TransformPositions(StaticPart part, Vector3 localOffset)
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
        }
    }

    private void TransformTexcoords(StaticPart part)
    {
        double scaleX, scaleY, translateX, translateY;
        if (_tag.MeshGroups[part.GroupIndex].Unk20.Z == 0.0078125)
        {
            scaleX = 1 / 0.4375 * 2.28571428571 * 2;
            translateX = 0.333333; // 0 if no 2 * 2.285
        }
        else if (_tag.MeshGroups[part.GroupIndex].Unk20.Z == -0.9765625)
        {
            scaleX = 32;
            translateX = -14;
        }
        else
        {
            throw new Exception("Unknown terrain uv scale x");
        }
        if (_tag.MeshGroups[part.GroupIndex].Unk20.W == 0.0078125)
        {
            scaleY = -1 / 0.4375 * 2.28571428571 * 2;
            translateY = 0.333333;
        }
        else if (_tag.MeshGroups[part.GroupIndex].Unk20.W == -0.9765625)
        {
            scaleY = -32;
            translateY = -14;
        }
        else
        {
            throw new Exception("Unknown terrain uv scale y");
        }
        for (int i = 0; i < part.VertexTexcoords0.Count; i++)
        {
            part.VertexTexcoords0[i] = new Vector2(
            part.VertexTexcoords0[i].X * scaleX + translateX,
            part.VertexTexcoords0[i].Y * scaleY + (1 - translateY)
            );
        }
    }
}

/// <summary>
/// Terrain data resource.
/// </summary>
[SchemaStruct("7D6C8080", 0x20)]
public struct D2Class_7D6C8080
{
    [SchemaField(0x10)]
    public short Unk10;  // tile x-y coords?
    public short Unk12;
    public TigerHash Unk14;
    public Terrain Terrain;
    public Tag<SOcclusionBounds> TerrainBounds;
}

/// <summary>
/// Terrain _tag.
/// </summary>
[SchemaStruct("816C8080", 0xB0)]
public struct D2Class_816C8080
{
    public long FileSize;
    [SchemaField(0x10)]
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    [SchemaField(0x50)]
    public DynamicArray<D2Class_866C8080> MeshGroups;

    public VertexBuffer Vertices1;
    public VertexBuffer Vertices2;
    public IndexBuffer Indices1;
    public Material Unk6C;
    public Material Unk70;
    [SchemaField(0x78)]
    public DynamicArray<D2Class_846C8080> StaticParts;
    public VertexBuffer Vertices3;
    public VertexBuffer Vertices4;
    public IndexBuffer Indices2;
    [SchemaField(0x98)]
    public int Unk98;
    public int Unk9C;
    public int UnkA0;
}

[SchemaStruct("866C8080", 0x60)]
public struct D2Class_866C8080
{
    public float Unk00;
    public float Unk04;
    public float Unk08;
    public float Unk0C;
    public float Unk10;
    public float Unk14;
    public float Unk18;
    [SchemaField(0x20)]
    public Vector4 Unk20;
    public uint Unk30;
    public uint Unk34;
    public uint Unk38;
    public uint Unk3C;
    public uint Unk40;
    public uint Unk44;
    public uint Unk48;
    public uint Unk4C;
    public Texture Dyemap;
}

[SchemaStruct("846C8080", 0x0C)]
public struct D2Class_846C8080
{
    public Material Material;
    public uint IndexOffset;
    public ushort IndexCount;
    public byte GroupIndex;
    public byte DetailLevel;
}
