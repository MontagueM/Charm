
using Tiger.Exporters;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;

namespace Tiger.Schema;

public enum ExportDetailLevel
{
    MostDetailed,
    LeastDetailed,
    AllLevels
}

/// <summary>
/// Mesh data represented in the raw form the game will use.
/// Used to have the most control over the mesh data.
/// </summary>
public class RawMeshPart
{
    // public List<VertexBuffer> VertexBuffers;
    // public List<IndexBuffer> IndexBuffers;
    // public RawMaterial Material;
}

/// <summary>
/// A processed form of RawMeshPart that is ready to be exported.
/// </summary>
public class MeshPart
{
    public int Index;
    public uint IndexOffset;
    public uint IndexCount;
    public PrimitiveType PrimitiveType;
    public ELodCategory LodCategory;
    public List<UIntVector3> Indices = new List<UIntVector3>();
    public List<uint> VertexIndices = new List<uint>();
    public List<Vector4> VertexPositions = new List<Vector4>();
    public List<Vector2> VertexTexcoords0 = new List<Vector2>();
    public List<Vector2> VertexTexcoords1 = new List<Vector2>();
    public List<Vector4> VertexNormals = new List<Vector4>();
    public List<Vector4> VertexTangents = new List<Vector4>();
    public List<Vector4> VertexColours = new List<Vector4>();
    public Dictionary<int, List<Vector4>> VertexExtraData = new(); //TEXCOORD#, extra data
    public IMaterial? Material;
    public int GroupIndex = 0;
    public int VertexLayoutIndex = -1;
    public int MaxVertexColorIndex = -1;
}

public struct VertexWeight
{
    public IntVector4 WeightValues;
    public IntVector4 WeightIndices;
}

public class StaticMesh : Tag<SStaticMesh>
{
    public StaticMesh(FileHash hash) : base(hash) { }

    // We cache here so we don't have to recompute this every time we want to export.
    // private List<RawMeshPart>? _rawMeshParts;
    // private List<MeshPart>? _meshParts;
    // public static event EventHandler<

    public void SaveMaterialsFromParts(ExporterScene scene, List<StaticPart> parts)
    {
        foreach (var part in parts)
        {
            if (part.Material == null)
            {
                continue;
            }
            scene.Materials.Add(new ExportMaterial(part.Material));
        }
    }

    public List<StaticPart> Load(ExportDetailLevel detailLevel)
    {
        List<StaticPart> decalParts = LoadDecals(detailLevel);
        var mainParts = _tag.StaticData.Load(detailLevel, _tag);
        mainParts.AddRange(decalParts);
        return mainParts;
    }

    public Task<List<StaticPart>> LoadAsync(ExportDetailLevel detailLevel)
    {
        return Task.Run(() => Load(detailLevel));
    }

    private List<StaticPart> LoadDecals(ExportDetailLevel detailLevel)
    {
        List<StaticPart> parts = new List<StaticPart>();
        foreach (var decalPartEntry in _tag.Decals)
        {
            if (detailLevel == ExportDetailLevel.MostDetailed)
            {
                if (decalPartEntry.LODLevel != 1 && decalPartEntry.LODLevel != 2 && decalPartEntry.LODLevel != 10)
                {
                    continue;
                }
            }
            else if (detailLevel == ExportDetailLevel.LeastDetailed)
            {
                if (decalPartEntry.LODLevel == 1 || decalPartEntry.LODLevel == 2 || decalPartEntry.LODLevel == 10)
                {
                    continue;
                }
            }
            StaticPart part = new StaticPart(decalPartEntry);
            part.GetDecalData(decalPartEntry, _tag);
            part.Material = decalPartEntry.Material;
            parts.Add(part);
        }

        return parts;
    }
}
