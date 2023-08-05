
using System.Diagnostics;

namespace Tiger.Schema.Static;

public class StaticPart : MeshPart
{
    public StaticPart(SStaticPart terrainPartEntry) : base()
    {
        IndexOffset = terrainPartEntry.IndexOffset;
        IndexCount = terrainPartEntry.IndexCount;
        PrimitiveType = PrimitiveType.TriangleStrip;
    }

    public StaticPart(SStaticMeshPart staticPartEntry) : base()
    {
        IndexOffset = staticPartEntry.IndexOffset;
        IndexCount = staticPartEntry.IndexCount;
        PrimitiveType = (PrimitiveType)staticPartEntry.PrimitiveType;
    }

    public StaticPart(SStaticMeshDecal decalPartEntry) : base()
    {
        IndexOffset = decalPartEntry.IndexOffset;
        IndexCount = decalPartEntry.IndexCount;
        PrimitiveType = (PrimitiveType)decalPartEntry.PrimitiveType;
    }

    public void GetAllData(SStaticMeshBuffers buffers, SStaticMesh container)
    {
        Indices = buffers.Indices.GetIndexData(PrimitiveType, IndexOffset, IndexCount);
        // Get unique vertex indices we need to get data for
        HashSet<uint> uniqueVertexIndices = new HashSet<uint>();
        foreach (UIntVector3 index in Indices)
        {
            uniqueVertexIndices.Add(index.X);
            uniqueVertexIndices.Add(index.Y);
            uniqueVertexIndices.Add(index.Z);
        }
        VertexIndices = uniqueVertexIndices.ToList();
        // Have to call it like this b/c we don't know the format of the vertex data here
        buffers.Vertices0.ReadVertexData(this, uniqueVertexIndices, 0);
        buffers.Vertices1?.ReadVertexData(this, uniqueVertexIndices, 1, buffers.Vertices0.TagData.Stride);

        Debug.Assert(VertexPositions.Count == VertexTexcoords0.Count && VertexPositions.Count == VertexNormals.Count);

        TransformData(container);
    }

    public void GetDecalData(SStaticMeshDecal mesh, SStaticMesh container)
    {
        Indices = mesh.Indices.GetIndexData(PrimitiveType, IndexOffset, IndexCount);
        // Get unique vertex indices we need to get data for
        HashSet<uint> uniqueVertexIndices = new HashSet<uint>();
        foreach (UIntVector3 index in Indices)
        {
            uniqueVertexIndices.Add(index.X);
            uniqueVertexIndices.Add(index.Y);
            uniqueVertexIndices.Add(index.Z);
        }
        VertexIndices = uniqueVertexIndices.ToList();
        // Have to call it like this b/c we don't know the format of the vertex data here
        mesh.Vertices0.ReadVertexData(this, uniqueVertexIndices, 0);
        mesh.Vertices1.ReadVertexData(this, uniqueVertexIndices, 1);
        mesh.Vertices2?.ReadVertexData(this, uniqueVertexIndices);

        TransformData(container);
    }

    private void TransformData(SStaticMesh container)
    {
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            var t = (container.StaticData as Tiger.Schema.Static.DESTINY2_WITCHQUEEN_6307.StaticMeshData).TagData;
            TransformPositions(t.ModelTransform);
            TransformUVs(new Vector2(t.TexcoordScale, t.TexcoordScale), t.TexcoordTranslation);
        }
        else
        {
            TransformPositions(container.ModelTransform);
            TransformUVs(container.TexcoordScale, container.TexcoordTranslation);
        }
    }

    private void TransformUVs(Vector2 texcoordScale, Vector2 texcoordTranslation)
    {
        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            VertexTexcoords0[i] = new Vector2(
                VertexTexcoords0[i].X * texcoordScale.X + texcoordTranslation.X,
                VertexTexcoords0[i].Y * -texcoordScale.Y + 1 - texcoordTranslation.Y
            );
        }
    }

    private void TransformPositions(Vector4 modelTransform)
    {
        for (int i = 0; i < VertexPositions.Count; i++)
        {
            // i think theres a different scale and offset for model data vs decals... like 99% sure
            VertexPositions[i] = new Vector4(
                VertexPositions[i].X * modelTransform.W + modelTransform.X,
                VertexPositions[i].Y * modelTransform.W + modelTransform.Y,
                VertexPositions[i].Z * modelTransform.W + modelTransform.Z,
                VertexPositions[i].W
            );
        }
    }
}
