
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
    public EPrimitiveType PrimitiveType;
    public ELodCategory LodCategory;
    public List<UIntVector3> Indices = new List<UIntVector3>();
    public List<uint> VertexIndices = new List<uint>();
    public List<Vector4> VertexPositions = new List<Vector4>();
    public List<Vector2> VertexTexcoords0 = new List<Vector2>();
    public List<Vector2> VertexTexcoords1 = new List<Vector2>();
    public List<Vector4> VertexNormals = new List<Vector4>();
    public List<Vector4> VertexTangents = new List<Vector4>();
    public List<Vector4> VertexColours = new List<Vector4>();
    public Material Material;
    public int GroupIndex = 0;
}

public class DynamicMeshPart : MeshPart
{
    public List<VertexWeight>? VertexWeights;
    public List<Vector4> VertexColourSlots = new List<Vector4>();
    public bool bAlphaClip;
    public byte GearDyeChangeColorIndex = 0xFF;
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

    public void SaveMaterialsFromParts(string saveDirectory, List<StaticPart> parts, bool bSaveShaders)
    {
        Directory.CreateDirectory($"{saveDirectory}/Textures");
        Directory.CreateDirectory($"{saveDirectory}/Shaders");
        foreach (var part in parts)
        {
            if (part.Material.Hash.IsInvalid()) continue;
            part.Material.SaveAllTextures($"{saveDirectory}/Textures");
            if (bSaveShaders)
            {
                part.Material.SavePixelShader($"{saveDirectory}/Shaders");
                part.Material.SaveVertexShader($"{saveDirectory}/Shaders");
                part.Material.SaveComputeShader($"{saveDirectory}/Shaders");
            }
        }
    }

    public List<StaticPart> Load(ExportDetailLevel detailLevel)
    {
        List<StaticPart> decalParts = LoadDecals(detailLevel);
        var mainParts = _tag.StaticData.Load(detailLevel, _tag);
        mainParts.AddRange(decalParts);
        return mainParts;
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
            part.Material = decalPartEntry.MaterialHash;
            parts.Add(part);
        }

        return parts;
    }
}
