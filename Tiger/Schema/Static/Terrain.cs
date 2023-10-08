using Tiger.Exporters;
using Tiger.Schema.Model;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Static;


public class Terrain : Tag<STerrain>
{
    public Terrain(FileHash hash) : base(hash)
    {

    }

    // To test use edz.strike_hmyn and alleys_a adf6ae80
    public void LoadIntoExporter(ExporterScene scene, string saveDirectory, bool bSaveShaders, D2Class_7D6C8080 parentResource, bool exportStatic = false)
    {
        // todo fix terrain
        if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999)
        {
            return;
        }
        // Directory.CreateDirectory(saveDirectory + "/Textures/Terrain/");
        // Directory.CreateDirectory(saveDirectory + "/Shaders/Terrain/");

        // if (Hash != "CAC7D380")
        // {
        //     return;
        // }
        // Uses triangle strip + only using first set of vertices and indices
        Dictionary<StaticPart, IMaterial> parts = new Dictionary<StaticPart, IMaterial>();
        List<string> dyeMaps = new List<string>();
        var x = new List<float>();
        var y = new List<float>();
        var z = new List<float>();
        foreach (var partEntry in _tag.StaticParts)
        {
            if (partEntry.DetailLevel == 0)
            {
                var part = MakePart(partEntry);
                parts.TryAdd(part, partEntry.Material);
                x.AddRange(part.VertexPositions.Select(a => a.X));
                y.AddRange(part.VertexPositions.Select(a => a.Y));
                z.AddRange(part.VertexPositions.Select(a => a.Z));

                if (partEntry.Material == null) continue;

                scene.Materials.Add(new ExportMaterial(partEntry.Material, true));
                part.Material = partEntry.Material;
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
                scene.Textures.Add(partEntry.Dyemap);
                dyeMaps.Add(partEntry.Dyemap.Hash.ToString());
            }
        }
        localOffset = new Vector3((x.Max() + x.Min()) / 2, (y.Max() + y.Min()) / 2, (z.Max() + z.Min()) / 2);
        foreach (var part in parts)
        {
            // scale by 1.99 ish, -1 for all sides, multiply by 512?
            TransformPositions(part.Key, localOffset);
            TransformTexcoords(part.Key);
        }

        scene.AddStatic(Hash, parts.Keys.ToList());
        // For now we pre-transform it
        if (!exportStatic)
        {
            scene.AddStaticInstance(Hash, 1, Vector4.Zero, globalOffset);
        }

        // We need to add these textures after the static is initialised
        foreach (var part in parts)
        {
            Texture dyemap = _tag.MeshGroups[part.Key.GroupIndex].Dyemap;
            if (dyemap != null)
            {
                if (!exportStatic)
                {
                    scene.AddTextureToMaterial(part.Key.Material.FileHash, terrainTextureIndex, dyemap);
                }

                if (CharmInstance.GetSubsystem<ConfigSubsystem>().GetS2ShaderExportEnabled())
                {
                    if (File.Exists($"{saveDirectory}/Shaders/Source2/materials/Terrain/{part.Value.FileHash}.vmat"))
                    {
                        string[] vmat = File.ReadAllLines($"{saveDirectory}/Shaders/Source2/materials/Terrain/{part.Value.FileHash}.vmat");
                        int lastBraceIndex = Array.FindLastIndex(vmat, line => line.Trim().Equals("}")); //Searches for the last brace (})
                        bool textureFound = Array.Exists(vmat, line => line.Trim().StartsWith("TextureT14"));
                        if (!textureFound && lastBraceIndex != -1)
                        {
                            var newVmat = vmat.Take(lastBraceIndex).ToList();

                            for (int i = 0; i < dyeMaps.Count; i++) //Add all the dyemaps to the vmat
                            {
                                //Console.WriteLine($"{dyeMaps[i]}");
                                newVmat.Add($"  TextureT{terrainTextureIndex}_{i} \"materials/Textures/{dyeMaps[i]}.png\"");
                            }

                            newVmat.AddRange(vmat.Skip(lastBraceIndex));
                            File.WriteAllLines($"{saveDirectory}/Shaders/Source2/materials/Terrain/{Hash}_{part.Value.FileHash}.vmat", newVmat);
                            File.Delete($"{saveDirectory}/Shaders/Source2/materials/Terrain/{part.Value.FileHash}.vmat"); //Delete the old vmat, dont need it anymore
                        }
                    }
                }
            }
        }
    }

    public StaticPart MakePart(SStaticPart entry)
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

        _tag.Vertices1.ReadVertexData(part, uniqueVertexIndices, 0, -1, true);
        _tag.Vertices2.ReadVertexData(part, uniqueVertexIndices, 0, -1, true);

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

        Vector4 vec = _tag.MeshGroups[part.GroupIndex].Unk20;

        if (vec.Z == 0.0078125)
        {
            scaleX = 1 / 0.4375 * 2.28571428571 * 2;
            translateX = 0.333333; // 0 if no 2 * 2.285
        }
        else if (vec.Z == -0.9765625)
        {
            scaleX = 32;
            translateX = -14;
        }
        else if (vec.Z == -1.9609375)
        {
            // todo shadowkeep idk
            scaleX = 1;
            translateX = 0;
        }
        else if (vec.Z == -2.9453125)
        {
            // todo shadowkeep idk
            scaleX = 1;
            translateX = 0;
        }
        else
        {
            throw new Exception("Unknown terrain uv scale x");
        }
        if (vec.W == 0.0078125)
        {
            scaleY = -1 / 0.4375 * 2.28571428571 * 2;
            translateY = 0.333333;
        }
        else if (vec.W == -0.9765625)
        {
            scaleY = -32;
            translateY = -14;
        }
        else if (vec.W == -1.9609375)
        {
            // todo shadowkeep idk
            scaleY = 1;
            translateY = 0;
        }
        else if (vec.W == -2.9453125)
        {
            // todo shadowkeep idk
            scaleY = 1;
            translateY = 0;
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
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "4B718080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "7D6C8080", 0x20)]
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
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "4F718080", 0xB0)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "816C8080", 0xB0)]
public struct STerrain
{
    public long FileSize;
    [SchemaField(0x10)]
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    [SchemaField(0x58, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x50, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<SMeshGroup> MeshGroups;

    public VertexBuffer Vertices1;
    public VertexBuffer Vertices2;
    public IndexBuffer Indices1;
    public IMaterial Unk6C;
    public IMaterial Unk70;
    [SchemaField(0x80, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<SStaticPart> StaticParts;
    public VertexBuffer Vertices3;
    public VertexBuffer Vertices4;
    public IndexBuffer Indices2;
    [SchemaField(0xA0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x98, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public int Unk98;
    public int Unk9C;
    public int UnkA0;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "54718080", 0x60)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "866C8080", 0x60)]
public struct SMeshGroup
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

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "52718080", 0x0C)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "846C8080", 0x0C)]
public struct SStaticPart
{
    public IMaterial Material;
    public uint IndexOffset;
    public ushort IndexCount;
    public byte GroupIndex;
    public byte DetailLevel;
}
