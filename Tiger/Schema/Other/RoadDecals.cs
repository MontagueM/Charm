using Tiger.Exporters;
using Tiger.Schema.Entity;

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
        foreach (S808068E3 a in _tag.Entries)
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

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806DF1, 0x18)] //F16D8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808068E8, 0x18)] //E8688080
public struct SMapRoadDecalsResource
{
    [SchemaField(0x10), NoLoad]
    public RoadDecals RoadDecals; // Contrary to the name, it is more than just decals on roads
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806DF3, 0x58)] //F36D8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808068EA, 0x58)] //EA688080
public struct SMapRoadDecals
{
    public ulong FileSize;
    public DynamicArray<S808068E3> Entries;
    public FileHash OcclusionBounds;
    [SchemaField(0x20)]
    public AABB UnkBounds;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806DEC, 0x60)] //EC6D8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808068E3, 0x60)] //E3688080
public struct S808068E3
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
