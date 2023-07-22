namespace Tiger.Schema;

public class StaticPart : MeshPart
{
    public StaticPart(D2Class_376D8080 staticPartEntry) : base()
    {
        IndexOffset = staticPartEntry.IndexOffset;
        IndexCount = staticPartEntry.IndexCount;
        PrimitiveType = (EPrimitiveType)staticPartEntry.PrimitiveType;
    }

    public StaticPart(D2Class_2F6D8080 decalPartEntry) : base()
    {
        IndexOffset = decalPartEntry.IndexOffset;
        IndexCount = decalPartEntry.IndexCount;
        PrimitiveType = (EPrimitiveType)decalPartEntry.PrimitiveType;
    }

    public void GetAllData(D2Class_366D8080 mesh, SStaticMesh container)
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
        mesh.Vertices0.ReadVertexData(this, uniqueVertexIndices);
        mesh.Vertices1.ReadVertexData(this, uniqueVertexIndices);

        TransformPositions(container.StaticData.TagData);
        TransformUVs(container.StaticData.TagData);
    }

    public void GetDecalData(D2Class_2F6D8080 mesh, SStaticMesh container)
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
        mesh.Vertices0.ReadVertexData(this, uniqueVertexIndices);
        mesh.Vertices1.ReadVertexData(this, uniqueVertexIndices);
        mesh.Vertices2?.ReadVertexData(this, uniqueVertexIndices);

        TransformPositions(container.StaticData.TagData);
        TransformUVs(container.StaticData.TagData);
    }

    private void TransformUVs(SStaticMeshData header)
    {
        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            VertexTexcoords0[i] = new Vector2(
                VertexTexcoords0[i].X * header.TexcoordScale + header.TexcoordXTranslation,
                VertexTexcoords0[i].Y * -header.TexcoordScale + 1 - header.TexcoordYTranslation
            );
        }
    }

    private void TransformPositions(SStaticMeshData header)
    {
        for (int i = 0; i < VertexPositions.Count; i++)
        {
            // i think theres a different scale and offset for model data vs decals... like 99% sure
            VertexPositions[i] = new Vector4(
                VertexPositions[i].X * header.ModelTransform.W + header.ModelTransform.X,
                VertexPositions[i].Y * header.ModelTransform.W + header.ModelTransform.Y,
                VertexPositions[i].Z * header.ModelTransform.W + header.ModelTransform.Z,
                VertexPositions[i].W
            );
        }
    }
}
