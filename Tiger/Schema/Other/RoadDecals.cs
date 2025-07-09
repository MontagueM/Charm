using Tiger.Exporters;
using Tiger.Schema.Entity;
using Tiger.Schema.Model;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class RoadDecals : Tag<SMapRoadDecals>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.RoadDecals;
    public RoadDecals(FileHash hash) : base(hash)
    {

    }

    public void LoadIntoExporter(ExporterScene scene)
    {
        foreach (SE3688080 a in _tag.Entries)
        {
            Transform transform = new()
            {
                Position = a.Position.ToVec3(),
                Quaternion = a.Rotation,
                Rotation = Vector4.QuaternionToEulerAngles(a.Rotation),
                Scale = new(a.Position.W)
            };

            DynamicMeshPart part = MeshPart.CreateFromBuffers<DynamicMeshPart>(
                a.IndexBuffer,
                a.VertexBuffer,
                a.Material,
                PrimitiveType.Triangles,
                Strategy.IsPreBL() ? 8 : 9,
                (uint)a.FaceCount * 3,
                a.IndexOffset);

            part.TransformPosition(a.Offset, a.Scale);
            part.TransformTexcoord(a.TexcoordOffset, a.TexcoordScale);

            scene.AddMapModelParts($"{a.VertexBuffer.Hash}", new List<MeshPart> { part }, transform);
            scene.Materials.Add(new ExportMaterial(part.Material));
        }
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "F16D8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "E8688080", 0x18)]
public struct SMapRoadDecalsResource
{
    [SchemaField(0x10), NoLoad]
    public RoadDecals RoadDecals; // Contrary to the name, it is more than just decals on roads
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "F36D8080", 0x58)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "EA688080", 0x58)]
public struct SMapRoadDecals
{
    public ulong FileSize;
    public DynamicArray<SE3688080> Entries;
    public FileHash OcclusionBounds;
    [SchemaField(0x20)]
    public AABB UnkBounds;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "EC6D8080", 0x60)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "E3688080", 0x60)]
public struct SE3688080
{
    public Material Material;
    public IndexBuffer IndexBuffer;
    public VertexBuffer VertexBuffer;
    public ushort FaceCount; // Needs multiplied by 3
    public ushort IndexOffset; // Always 0, so idk if IndexCount is an int then
    public Vector4 Rotation;
    public Vector4 Position;
    public Vector4 Scale;
    public Vector4 Offset;
    public Vector2 TexcoordScale;
    public Vector2 TexcoordOffset;
}
