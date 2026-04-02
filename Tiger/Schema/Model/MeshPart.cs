using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public enum ExportDetailLevel
{
    MostDetailed,
    LeastDetailed,
    AllLevels
}

public enum LoadLevel
{
    Full,
    Minimal
}

/// <summary>
/// A processed form of RawMeshPart that is ready to be exported.
/// </summary>
public class MeshPart
{
    public int Index;
    public int MeshIndex;
    public int GroupIndex = 0;
    public uint IndexOffset;
    public uint IndexCount;
    public PrimitiveType PrimitiveType;
    public ELodCategory LodCategory;
    public Material? Material;
    public int VariantShaderIndex = -1;
    public int VertexLayoutIndex = -1;
    public int MaxVertexColorIndex = -1;

    public Vector3 VertexScale = Vector3.One; // Currently used as misc CFG info, this is the scale applied to vertex positions when exporting.
    public Vector3 VertexOffset = Vector3.Zero;
    public Vector4 RotationOffset = Vector4.Quaternion;
    public Vector4 TranslationOffset = Vector4.Zero;

    public List<UIntVector3> Indices = new();
    public List<uint> VertexIndices = new();
    public List<Vector4> VertexPositions = new();
    public List<Vector2> VertexTexcoords0 = new();
    public List<Vector2> VertexTexcoords1 = new();
    public List<Vector4> VertexNormals = new();
    public List<Vector4> VertexTangents = new();
    public List<Vector4> VertexColours = new();
    public List<Vector4> VertexAO = new();
    public Dictionary<int, List<Vector4>> VertexExtraData = new(); //TEXCOORD#, extra data


    // For Custom Renderer
    public TfxRenderStage RenderStage;
    public IndexBuffer IndexBuffer;
    public VertexBuffer VertexBuffer0;
    public VertexBuffer? VertexBuffer1;
    public VertexBuffer? VertexBuffer2;
    public VertexBuffer? VertexBuffer3; // Skinning buffer for entities

    public Vector4 MeshScale = Vector4.One;
    public Vector4 MeshTransform = new(1f, 1f, 0f, 0f);
    public Vector4 UVTransform = new(1f, 1f, 0f, 0f);

    /// <summary>
    /// Creates an instance of a specified type, derived from MeshPart, using data from the provided index and vertex buffers and other data.
    /// </summary>
    /// <typeparam name="T">
    /// The type of MeshPart to create. Must be derived from MeshPart.
    /// </typeparam>
    /// <param name="ib">The mesh index buffer.</param>
    /// <param name="vb">The mesh vertex buffer.</param>
    /// <param name="mat">The material to assign to the created mesh part.</param>
    /// <param name="primitiveType">The type of primitives used by the mesh.</param>
    /// <param name="layoutIndex">The layout index to assign for vertex data lookup.</param>
    /// <param name="indexCount">The number of indices to read from the index buffer.</param>
    /// <param name="indexOffset">The offset in the index buffer where the indices start.</param>
    /// <returns>
    /// A new instance of type <typeparamref name="T"/> initialized with the provided data.
    /// </returns>
    public static T CreateFromBuffers<T>(
    IndexBuffer ib,
    VertexBuffer vb,
    Material mat,
    PrimitiveType primitiveType,
    int layoutIndex,
    uint indexCount,
    uint indexOffset,
    TfxRenderStage renderStage = TfxRenderStage.GenerateGbuffer) where T : MeshPart, new()
    {
        T part = new();

        if (mat is not null)
        {
            part.Material = mat;
            part.Material.RenderStage = renderStage;
        }

        part.Indices = ib.GetIndexData(primitiveType, indexOffset, indexCount);
        part.VertexLayoutIndex = layoutIndex;
        part.IndexCount = indexCount;
        part.IndexOffset = indexOffset;
        if (part is DynamicMeshPart)
            (part as DynamicMeshPart).RenderStage = renderStage;

        // Get unique vertex indices we need to get data for
        HashSet<uint> uniqueVertexIndices = new();
        foreach (UIntVector3 index in part.Indices)
        {
            uniqueVertexIndices.Add(index.X);
            uniqueVertexIndices.Add(index.Y);
            uniqueVertexIndices.Add(index.Z);
        }
        part.VertexIndices = uniqueVertexIndices.ToList();

        //Log.Debug($"Reading vertex buffers {vb.Hash}/{vb.TagData.Stride}");
        vb.ReadVertexDataFromLayout(part, uniqueVertexIndices, 0);

        return part;
    }

    public void TransformPosition(Vector4 offset, Vector4 scale)
    {
        for (int i = 0; i < VertexPositions.Count; i++)
        {
            VertexPositions[i] = new Vector4(
                VertexPositions[i].X * scale.X + offset.X,
                VertexPositions[i].Y * scale.Y + offset.Y,
                VertexPositions[i].Z * scale.Z + offset.Z,
                VertexPositions[i].W
            );
        }
    }

    public void TransformTexcoord(Vector2 offset, Vector2 scale)
    {
        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            Vector2 tx = VertexTexcoords0[i];
            VertexTexcoords0[i] = new Vector2(
                tx.X * scale.X + offset.X,
                1 - (tx.Y * scale.Y + offset.Y)
            );
        }
    }
}
