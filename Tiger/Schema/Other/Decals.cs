using Tiger.Exporters;

using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class Decals : Tag<SMapDecals>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.Decals;
    public Decals(FileHash hash) : base(hash)
    {
    }

    public void LoadIntoExporter(ExporterScene scene)
    {
        Exporter.Get().GetGlobalScene().AddToGlobalScene(this);

        foreach (S80806963 instance in _tag.DecalResources.Enumerate(GetReader()))
        {
            for (int i = instance.StartIndex; i < instance.StartIndex + instance.Count; i++)
            {
                if (instance.Material is null)
                    continue;

                instance.Material.RenderStage = TfxRenderStage.Decals;
                scene.Materials.Add(new(instance.Material));
            }
        }
    }

    public void DebugExport(string savePath)
    {
        List<Vector4> cube = GetCube();
        List<Transform> transforms = GetTransforms();
        int j = 0;
        foreach (Transform transform in transforms)
        {
            List<Vector4> transformedCubes = ApplyTransformsToCube(cube, transform);
            ExportCube($"{savePath}\\cube_{j}.obj", transformedCubes);
            j++;
        }
    }

    public List<Transform> GetTransforms()
    {
        using TigerReader reader = _tag.Transforms.GetReferenceReader();
        short stride = _tag.Transforms.TagData.Stride;
        List<Transform> transforms = new();

        for (int i = 0; i < reader.BaseStream.Length / stride; i++)
        {
            reader.BaseStream.Seek(i * stride, SeekOrigin.Begin);
            var pos = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 1); // format R32g32b32Float, stride 0xC
            var rot = new Vector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()); // format R16g16b16a16Snorm, stride 0x8
            var scale = new Vector4(reader.ReadHalf(), reader.ReadHalf(), reader.ReadHalf(), reader.ReadHalf()); // format R16g16b16a16Float, stride 0x8

            transforms.Add(new()
            {
                Position = pos.ToVec3(),
                Quaternion = rot,
                Scale = scale.ToVec3()
            });
        }

        return transforms;
    }

    public List<Vector4> GetCube()
    {
        using TigerReader reader = _tag.Cube.GetReferenceReader();

        int vertexCount = (int)(reader.BaseStream.Length / 4); // triangle list
        Vector4[] cubePoints = new Vector4[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            cubePoints[i] = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        return cubePoints.ToList();
    }

    public List<Vector4> ApplyTransformsToCube(List<Vector4> cubeVertices, Transform transform)
    {
        List<Vector4> transformedVertices = new();

        Vector3 position = transform.Position;
        Vector4 rotation = transform.Quaternion;
        Vector3 scale = transform.Scale;

        foreach (Vector4 vertex in cubeVertices)
        {
            // Scale
            Vector3 scaledVertex = new(vertex.X * scale.X, vertex.Y * scale.Y, vertex.Z * scale.Z);

            // Rotate
            Vector3 rotatedVertex = Vector3.Transform(scaledVertex, rotation);

            // Translate
            Vector4 finalVertex = new(rotatedVertex.X + position.X, rotatedVertex.Y + position.Y, rotatedVertex.Z + position.Z, 1);

            transformedVertices.Add(finalVertex);
        }


        return transformedVertices;
    }

    public void ExportCube(string filePath, List<Vector4> cubePoints)
    {
        using (StreamWriter writer = new(filePath))
        {
            // Write vertices
            foreach (Vector4 point in cubePoints)
            {
                writer.WriteLine($"v {point.X} {point.Y} {point.Z}");
            }

            // Write faces (each 3 vertices form a triangle)
            for (int i = 0; i < 36; i += 3)
            {
                writer.WriteLine($"f {i + 1} {i + 2} {i + 3}");
            }
        }

        Console.WriteLine($"Cube exported to {filePath}");
    }
}

/// </summary>
/// Map Decals Resource
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A70, 0x10)] //701A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806E62, 0x18)] //626E8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806955, 0x18)] //55698080
public struct SMapDecalsResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public Decals MapDecals;
}

/// <summary>
/// Map Decals
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801B40, 0x68)] //401B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806E68, 0x78)] //686E8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080695B, 0x78)] //5B698080
public struct SMapDecals
{
    public ulong FileSize;
    public DynamicArrayUnloaded<S80806963> DecalResources;

    [SchemaField(0x18)]
    public DynamicArrayUnloaded<S80806964> UnkLocations;

    [SchemaField(0x28)]
    public VertexBuffer Transforms;
    public VertexBuffer Cube; // The same for every single decal it seems?

    [SchemaField(0x38), NoLoad]
    public Tag<SOcclusionBounds> Bounds;

    [SchemaField(0x40)]
    public Vector4 Unk40; //some type of bounds
    public Vector4 Unk50;
    public TigerHash Unk60;
}

/// <summary>
/// Decal resources
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A83, 0x8)] //831A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806E6C, 0x8)] //6C6E8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806963, 0x8)] //63698080
public struct S80806963
{
    public Material Material;
    public short StartIndex;
    public short Count; //Number of entries to read
}

/// <summary>
/// Decal Location
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A53, 0x10)] //531A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806E6D, 0x10)] //6D6E8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806964, 0x10)] //64698080
public struct S80806964
{
    public Vector4 Location;
}
