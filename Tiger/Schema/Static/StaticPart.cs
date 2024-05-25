﻿using System.Diagnostics;
using Arithmic;

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
        VertexLayoutIndex = decalPartEntry.VertexLayoutIndex;
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

        // Have to call it like this b/c we don't know the format of the vertex data here
        mesh.Vertices0.ReadVertexData(this, uniqueVertexIndices, 0, mesh.Vertices1 != null ? mesh.Vertices1.TagData.Stride : -1, false);
        mesh.Vertices1?.ReadVertexData(this, uniqueVertexIndices, 1, mesh.Vertices0.TagData.Stride, false);
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

        if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999)
        {
            InputSignature[] inputSignatures = Material.VertexShader.InputSignatures.ToArray();
            int b0Stride = buffers.Vertices0.TagData.Stride;
            int b1Stride = buffers.Vertices1?.TagData.Stride ?? 0;
            List<InputSignature> inputSignatures0 = new();
            List<InputSignature> inputSignatures1 = new();
            int stride = 0;
            foreach (InputSignature inputSignature in inputSignatures)
            {
                if (stride < b0Stride)
                    inputSignatures0.Add(inputSignature);
                else
                    inputSignatures1.Add(inputSignature);

                if (inputSignature.Semantic == InputSemantic.Colour || inputSignature.Semantic == InputSemantic.BlendIndices || inputSignature.Semantic == InputSemantic.BlendWeight)
                    stride += inputSignature.GetNumberOfComponents() * 1;  // 1 byte per component
                else
                    stride += inputSignature.GetNumberOfComponents() * 2;  // 2 bytes per component
            }

            List<int> strides = new();
            if (buffers.Vertices0 != null) strides.Add(buffers.Vertices0.TagData.Stride);
            if (buffers.Vertices1 != null) strides.Add(buffers.Vertices1.TagData.Stride);
            if (buffers.VertexColor != null) strides.Add(buffers.VertexColor.TagData.Stride);
            Helpers.DecorateSignaturesWithBufferIndex(ref inputSignatures, strides); // absorb into the getter probs

            Log.Debug($"Reading vertex buffers {buffers.Vertices0.Hash}/{buffers.Vertices0.TagData.Stride}/{inputSignatures.Where(s => s.BufferIndex == 0).DebugString()} and {buffers.Vertices1?.Hash}/{buffers.Vertices1?.TagData.Stride}/{inputSignatures.Where(s => s.BufferIndex == 1).DebugString()}");
            buffers.Vertices0.ReadVertexDataSignatures(this, uniqueVertexIndices, inputSignatures0, false);
            buffers.Vertices1?.ReadVertexDataSignatures(this, uniqueVertexIndices, inputSignatures1, false);
        }
        else
        {
            buffers.Vertices0.ReadVertexDataFromLayout(this, uniqueVertexIndices, 0);
            buffers.Vertices1?.ReadVertexDataFromLayout(this, uniqueVertexIndices, 1);
            buffers.VertexColor?.ReadVertexData(this, uniqueVertexIndices);
        }

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
        if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999)
        {
            List<InputSignature> inputSignatures = mesh.Material.VertexShader.InputSignatures;
            int b0Stride = mesh.Vertices0.TagData.Stride;
            int b1Stride = mesh.Vertices1?.TagData.Stride ?? 0;
            List<InputSignature> inputSignatures0 = new();
            List<InputSignature> inputSignatures1 = new();
            int stride = 0;
            foreach (InputSignature inputSignature in inputSignatures)
            {
                if (stride < b0Stride)
                {
                    inputSignatures0.Add(inputSignature);
                }
                else
                {
                    inputSignatures1.Add(inputSignature);
                }

                if (inputSignature.Semantic == InputSemantic.Colour)
                {
                    stride += inputSignature.GetNumberOfComponents() * 1;  // 1 byte per component
                }
                else
                {
                    stride += inputSignature.GetNumberOfComponents() * 2;  // 2 bytes per component
                }
                // todo entities can have 4 bytes per component, although its isolated for cloth so we can probably account for it
            }
            Debug.Assert(b0Stride + b1Stride == stride);

            Log.Debug($"Reading vertex buffers {mesh.Vertices0.Hash}/{b0Stride}/{inputSignatures0.DebugString()} and {mesh.Vertices1?.Hash}/{b1Stride}/{inputSignatures1.DebugString()}");
            mesh.Vertices0.ReadVertexDataSignatures(this, uniqueVertexIndices, inputSignatures0);
            mesh.Vertices1?.ReadVertexDataSignatures(this, uniqueVertexIndices, inputSignatures1);
        }
        else
        {
            mesh.Vertices0.ReadVertexDataFromLayout(this, uniqueVertexIndices, 0);
            mesh.Vertices1?.ReadVertexDataFromLayout(this, uniqueVertexIndices, 1);
            mesh.VertexColor?.ReadVertexData(this, uniqueVertexIndices);
        }

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
