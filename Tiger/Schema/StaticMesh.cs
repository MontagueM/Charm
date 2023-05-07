
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
    public List<Vector4> VertexColourSlots = new List<Vector4>();
    // public Material Material;

}

public class DynamicPart : MeshPart
{
    // public List<VertexWeight> VertexWeights = new List<VertexWeight>();
    public int GroupIndex = 0;
    public bool bAlphaClip;
    public byte GearDyeChangeColorIndex = 0xFF;
}

public class StaticMesh : Tag<SLocalizedStrings>
{
    public StaticMesh(FileHash hash) : base(hash) { }

    // We cache here so we don't have to recompute this every time we want to export.
    private List<RawMeshPart>? _rawMeshParts;
    private List<MeshPart>? _meshParts;

    public void ExportRaw(ExportDetailLevel detailLevel)
    {

    }

    public void Export(ExportDetailLevel detailLevel)
    {

    }
}
