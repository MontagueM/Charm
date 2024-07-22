using System.Diagnostics;

namespace Tiger.Schema.Static;

public class StaticPart : MeshPart
{
    public StaticPart(SStaticPart terrainPartEntry) : base()
    {
        IndexOffset = terrainPartEntry.IndexOffset;
        IndexCount = terrainPartEntry.IndexCount;
        LodCategory = (ELodCategory)terrainPartEntry.DetailLevel;
        PrimitiveType = PrimitiveType.TriangleStrip;
    }

    public StaticPart(SStaticMeshPart staticPartEntry) : base()
    {
        IndexOffset = staticPartEntry.IndexOffset;
        IndexCount = staticPartEntry.IndexCount;
        LodCategory = (ELodCategory)staticPartEntry.DetailLevel;
        PrimitiveType = (PrimitiveType)staticPartEntry.PrimitiveType;
    }

    public StaticPart(SStaticMeshDecal decalPartEntry) : base()
    {
        VertexLayoutIndex = decalPartEntry.GetVertexLayoutIndex();
        IndexOffset = decalPartEntry.IndexOffset;
        IndexCount = decalPartEntry.IndexCount;
        LodCategory = (ELodCategory)decalPartEntry.LODLevel;
        PrimitiveType = (PrimitiveType)decalPartEntry.PrimitiveType;
    }

    public StaticPart(SStaticMeshData_D1 staticPartEntry) : base()
    {
        IndexOffset = staticPartEntry.IndexOffset;
        IndexCount = staticPartEntry.IndexCount;
        LodCategory = (ELodCategory)staticPartEntry.DetailLevel;
        PrimitiveType = (PrimitiveType)staticPartEntry.PrimitiveType;
    }

    public void GetAllData(SStaticMeshData_D1 mesh) // D1
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

        mesh.Vertices0.ReadVertexDataFromLayout(this, uniqueVertexIndices, 0);
        mesh.Vertices1?.ReadVertexDataFromLayout(this, uniqueVertexIndices, 1);
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

        buffers.Vertices0.ReadVertexDataFromLayout(this, uniqueVertexIndices, 0);
        buffers.Vertices1?.ReadVertexDataFromLayout(this, uniqueVertexIndices, 1);
        buffers.VertexColor?.ReadVertexData(this, uniqueVertexIndices);

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

        mesh.Vertices0.ReadVertexDataFromLayout(this, uniqueVertexIndices, 0);
        mesh.Vertices1?.ReadVertexDataFromLayout(this, uniqueVertexIndices, 1);
        mesh.VertexColor?.ReadVertexData(this, uniqueVertexIndices);

        TransformData(container);
    }

    private void TransformData(SStaticMesh container)
    {
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402)
        {
            var t = (container.StaticData as DESTINY2_BEYONDLIGHT_3402.StaticMeshData).TagData;
            TransformPositions(t.ModelTransform);
            TransformUVs(new Vector2(t.TexcoordScale, t.TexcoordScale), t.TexcoordTranslation);

            if (VertexNormals.Count == 0 && VertexTangents.Count != 0)
            {
                // Don't question it, idk why or how this works either
                VertexNormals = VertexTangents;
            }
            // Fallback vertex color
            if (VertexColours.Count == 0)
            {
                VertexColours = new List<Vector4>(Enumerable.Repeat(new Vector4(0f, 0f, 0f, 1f), VertexPositions.Count));
            }
        }
        else
        {
            TransformPositions(container.ModelTransform);
            TransformUVs(container.TexcoordScale, container.TexcoordTranslation);
        }

        Debug.Assert(VertexPositions.Count == VertexTexcoords0.Count && VertexPositions.Count == VertexNormals.Count);
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
