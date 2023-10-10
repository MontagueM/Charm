using Arithmic;
using System;
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
    public void LoadIntoExporter(ExporterScene scene, string saveDirectory, bool bSaveShaders, bool exportStatic = false)
    {
        // todo fix terrain
        //if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999)
        //{
        //    return;
        //}

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
                if (partEntry.Material is null || partEntry.Material.VertexShader is null)
                    continue;

                var part = MakePart(partEntry);
                parts.TryAdd(part, partEntry.Material);
                x.AddRange(part.VertexPositions.Select(a => a.X));
                y.AddRange(part.VertexPositions.Select(a => a.Y));
                z.AddRange(part.VertexPositions.Select(a => a.Z));

                scene.Materials.Add(new ExportMaterial(partEntry.Material, true));
                part.Material = partEntry.Material;
            }
        }

        int terrainTextureIndex = 14;
        for (int i = 0; i < _tag.MeshGroups.Count; i++)
        {
            var partEntry = _tag.MeshGroups[i];
            if (partEntry.Dyemap != null)
            {
                scene.Textures.Add(partEntry.Dyemap);
                dyeMaps.Add(partEntry.Dyemap.Hash.ToString());
            }
        }
        Vector3 localOffset = new Vector3((x.Max() + x.Min()) / 2, (y.Max() + y.Min()) / 2, (z.Max() + z.Min()) / 2);
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
            scene.AddStaticInstance(Hash, 1, Vector4.Zero, Vector3.Zero);
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

        List<InputSignature> inputSignatures = entry.Material.VertexShader.InputSignatures;
        int b0Stride = _tag.Vertices1.TagData.Stride;
        int b1Stride = _tag.Vertices2?.TagData.Stride ?? 0;
        List<InputSignature> inputSignatures0 = new();
        List<InputSignature> inputSignatures1 = new();
        int stride = 0;
        foreach (InputSignature inputSignature in inputSignatures)
        {
            if (stride < b0Stride)
                inputSignatures0.Add(inputSignature);
            else
                inputSignatures1.Add(inputSignature);

            if (inputSignature.Semantic == InputSemantic.Colour)
                stride += inputSignature.GetNumberOfComponents() * 1;  // 1 byte per component
            else
                stride += inputSignature.GetNumberOfComponents() * 2;  // 2 bytes per component
        }

        Log.Debug($"Reading vertex buffers {_tag.Vertices1.Hash}/{_tag.Vertices1.TagData.Stride}/{inputSignatures.Where(s => s.BufferIndex == 0).DebugString()} and {_tag.Vertices2?.Hash}/{_tag.Vertices2?.TagData.Stride}/{inputSignatures.Where(s => s.BufferIndex == 1).DebugString()}");
        _tag.Vertices1.ReadVertexDataSignatures(part, uniqueVertexIndices, inputSignatures0, true);
        _tag.Vertices2.ReadVertexDataSignatures(part, uniqueVertexIndices, inputSignatures1, true);

        //_tag.Vertices1.ReadVertexData(part, uniqueVertexIndices, 0, -1, true);
        //_tag.Vertices2.ReadVertexData(part, uniqueVertexIndices, 0, -1, true);

        return part;
    }

    private void TransformPositions(StaticPart part, Vector3 localOffset)
    {
        for (int i = 0; i < part.VertexPositions.Count; i++)
        {
            //The "standard" terrain vertex shader from hlsl
            System.Numerics.Vector4 r0, r1, r2 = new();
            System.Numerics.Vector4 v0 = new(part.VertexPositions[i].X, part.VertexPositions[i].Y, part.VertexPositions[i].Z, part.VertexPositions[i].W);
            System.Numerics.Vector4 v1 = new(part.VertexNormals[i].X, part.VertexNormals[i].Y, part.VertexNormals[i].Z, part.VertexNormals[i].W);

            //r0 = cb11.transform + v0;
            r0.X = _tag.Unk30.X + v0.X;
            r0.Y = _tag.Unk30.Y + v0.Y;
            r0.Z = _tag.Unk30.Z + v0.Z;
            r0.W = _tag.Unk30.W + v0.W;

            r0.Z = r0.W * 65536 + r0.Z;

            //r0.xyw = float3(0.015625, 0.015625, 0.000122070313) * r0.xyz;
            r0.X = 0.015625f * r0.X;
            r0.Y = 0.015625f * r0.Y;
            r0.W = 0.000122070313f * r0.Z;

            //r1.xyz = float3(0,1,0) * v1.yzx;
            r1.X = 0 * v1.Y;
            r1.Y = 1 * v1.Z;
            r1.Z = 0 * v1.X;

            //r1.xyz = v1.zxy * float3(0,0,1) + -r1.xyz;
            r1.X = v1.Z * 0 + -r1.X;
            r1.Y = v1.X * 0 + -r1.Y;
            r1.Z = v1.Y * 1 + -r1.Z;

            //r0.z = dot(r1.yz, r1.yz);
            r0.Z = System.Numerics.Vector2.Dot(new(r1.Y, r1.Z), new(r1.Y, r1.Z));

            //r0.z = rsqrt(r0.z);
            r0.Z = MathF.ReciprocalSqrtEstimate(r0.Z);

            //r1.xyz = r1.xyz * r0.zzz;
            r1.X = r1.X * r0.Z;
            r1.Y = r1.Y * r0.Z;
            r1.Z = r1.Z * r0.Z;

            //r2.xyz = v1.zxy * r1.yzx;
            r2.X = v1.Z * r1.Y;
            r2.Y = v1.X * r1.Z;
            r2.Z = v1.Y * r1.X;

            //r2.xyz = v1.yzx * r1.zxy + -r2.xyz;
            r2.X = v1.Y * r1.Z + -r2.X;
            r2.Y = v1.Z * r1.X + -r2.Y;
            r2.Z = v1.X * r1.Y + -r2.Z;

            //r0.z = dot(r2.xyz, r2.xyz);
            //r0.z = rsqrt(r0.z);
            r0.Z = System.Numerics.Vector3.Dot(new(r2.X, r2.Y, r2.Z), new(r2.X, r2.Y, r2.Z));
            r0.Z = MathF.ReciprocalSqrtEstimate(r0.Z);
                
            part.VertexPositions[i] = new Vector4(r0.X, r0.Y, r0.Z * r0.W, r0.W); 
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
