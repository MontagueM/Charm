using Arithmic;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Entity;

public class EntityModel : Tag<SEntityModel>
{
    public EntityModel(FileHash hash) : base(hash)
    {
    }

    public Vector4 RotationOffset = Vector4.Quaternion;
    public Vector4 TranslationOffset = Vector4.Zero;

    public Vector4 Scale => _tag.ModelScale;
    public Vector4 Translation => _tag.ModelTranslation;

    /*
     * We need the parent resource to get access to the external materials
     */
    public List<DynamicMeshPart> Load(ExportDetailLevel detailLevel, EntityComponent parentResource, bool transparentsOnly = false, bool hasSkeleton = false, LoadLevel loadLevel = LoadLevel.Full)
    {
        Dictionary<int, Dictionary<int, SCB6E8080>> dynamicParts = GetPartsOfDetailLevel(detailLevel);
        List<DynamicMeshPart> parts = GenerateParts(dynamicParts, parentResource, hasSkeleton, loadLevel);

        if (transparentsOnly) // ROI decal/transparent mesh purposes. I hate this and its not the right way to do this
            return parts.Where(x => x.Material.RenderStates.BlendState() != -1).ToList();
        else
            return parts;
    }

    /// <summary>
    /// There are two flags that we use as selection criteria.
    /// First is LodCategory, second is DetailGroup.
    /// DetailGroup groups together objects that belong to the same geometry representation.
    /// LodCategory is a scale 0-A (usually 0,4,7,9) that determines how detailed (0 is highest).
    /// The criteria for selection for highest detail is:
    /// - detail level closest to 0 within the whole group.
    /// - OR parts that have a material right there as I'm still unsure about external material table stuff AND same detail level.
    /// </summary>
    /// <param name="detailLevel">The desired level of detail to get parts for.</param>
    /// <returns></returns>
    private Dictionary<int, Dictionary<int, SCB6E8080>> GetPartsOfDetailLevel(ExportDetailLevel eDetailLevel)
    {
        Dictionary<int, Dictionary<int, SCB6E8080>> parts = new();

        using TigerReader reader = GetReader();

        int meshIndex = 0;
        foreach (SEntityModelMesh mesh in _tag.Meshes.Enumerate(reader))
        {
            int partIndex = 0;
            parts.Add(meshIndex, new Dictionary<int, SCB6E8080>());
            for (int i = 0; i < mesh.Parts.Count; i++)
            {
                SCB6E8080 part = mesh.Parts[reader, i];

                if (eDetailLevel == ExportDetailLevel.AllLevels)
                {
                    parts[meshIndex].Add(partIndex, part);
                }
                else
                {
                    if (eDetailLevel == ExportDetailLevel.MostDetailed && part.LodCategory is ELodCategory.MainGeom0 or ELodCategory.GripStock0 or ELodCategory.Stickers0 or ELodCategory.InternalGeom0 or ELodCategory.Detail0)
                    {
                        parts[meshIndex].Add(partIndex, part);
                    }
                    else if (eDetailLevel == ExportDetailLevel.LeastDetailed && part.LodCategory == ELodCategory.LowPolyGeom3)
                    {
                        parts[meshIndex].Add(partIndex, part);
                    }
                }
                partIndex++;
            }

            meshIndex++;
        }

        return parts;
    }

    private List<DynamicMeshPart> GenerateParts(Dictionary<int, Dictionary<int, SCB6E8080>> dynamicParts, EntityComponent parentResource, bool hasSkeleton = false, LoadLevel loadLevel = LoadLevel.Full)
    {
        List<DynamicMeshPart> parts = new();
        List<int> exportPartRange = new();
        if (_tag.Meshes.Count == 0) return parts;

        int meshIndex = 0;
        foreach (SEntityModelMesh mesh in _tag.Meshes.Enumerate(GetReader()))
        {
            exportPartRange = GetExportRanges(mesh);
            foreach ((int i, SCB6E8080 part) in dynamicParts[meshIndex])
            {
                if (!exportPartRange.Contains(i))
                    continue;

                var renderStage = GetStageForPart(mesh, i);
                DynamicMeshPart dynamicMeshPart = new(part, parentResource)
                {
                    Index = i,
                    MeshIndex = meshIndex,
                    GroupIndex = part.ExternalIdentifier,
                    LodCategory = part.LodCategory,
                    bAlphaClip = (part.GetFlags() & 0x8) != 0
                    || ((part.GetFlags() & 0x20) != 0 && (TfxRenderStage)renderStage == TfxRenderStage.Decals),
                    GearDyeChangeColorIndex = part.GearDyeChangeColorIndex,
                    HasSkeleton = hasSkeleton,
                    RotationOffset = RotationOffset,
                    TranslationOffset = TranslationOffset,
                    VertexLayoutIndex = mesh.GetInputLayoutForStage(0),
                    // -1 shouldnt be possible..right?
                    RenderStage = renderStage ?? TfxRenderStage.GenerateGbuffer
                };

                //Console.WriteLine($"{i}--------------");
                //Console.WriteLine($"Material {part.Material?.Hash}");
                //Console.WriteLine($"VariantShaderIndex {part.VariantShaderIndex}");
                //Console.WriteLine($"PrimitiveType {part.PrimitiveType}");
                //Console.WriteLine($"IndexOffset {part.IndexOffset}");
                //Console.WriteLine($"IndexCount {part.IndexCount}");
                //Console.WriteLine($"Unk10 {part.Unk10}");
                //Console.WriteLine($"ExternalIdentifier {part.ExternalIdentifier}");
                //Console.WriteLine($"Unk16 {part.Unk16}");
                //Console.WriteLine($"FlagsD1 {part.FlagsD1}");
                //Console.WriteLine($"FlagsD2 {part.FlagsD2}");
                //Console.WriteLine($"GearDyeChangeColorIndex {part.GearDyeChangeColorIndex}");
                //Console.WriteLine($"LodCategory {part.LodCategory}");
                //Console.WriteLine($"RenderStage {dynamicMeshPart.RenderStage}");

                //We only care about the vertex shader for now for mesh data
                //But if theres also no pixel shader then theres no point in adding it
                if (dynamicMeshPart.Material is null ||
                dynamicMeshPart.Material.Vertex.Shader is null ||
                dynamicMeshPart.Material.Pixel.Shader is null) // || dynamicMeshPart.Material.Unk08 != 1)
                    continue;

                dynamicMeshPart.Material.RenderStage = dynamicMeshPart.RenderStage;
                if (loadLevel == LoadLevel.Full)
                    dynamicMeshPart.GetAllData(mesh, _tag);

                parts.Add(dynamicMeshPart);
            }

            meshIndex++;
        }

        return parts;
    }

    public static List<int> GetExportRanges(SEntityModelMesh mesh)
    {
        List<int> exportPartRange = new();

        foreach (TfxRenderStage stage in Globals.Get().GetExportStages())
        {
            Range range = mesh.GetRangeForStage((int)stage);
            if (!(range.Start.Value < range.End.Value))
                continue;

            for (int i = range.Start.Value; i < range.End.Value; i++)
                exportPartRange.Add(i);
        }

        return exportPartRange;
    }

    public static TfxRenderStage? GetStageForPart(SEntityModelMesh mesh, int partIndex)
    {
        for (int stageIndex = 0; stageIndex < 24; stageIndex++)
        {
            ushort start = mesh.PartRangePerRenderStage[stageIndex];
            ushort end = mesh.PartRangePerRenderStage[stageIndex + 1];

            if (partIndex >= start && partIndex < end)
            {
                return (TfxRenderStage)stageIndex;
            }
        }

        return null;
    }
}

public class DynamicMeshPart : MeshPart
{
    public List<VertexWeight> VertexWeights = new();

    // used for single-pass skin buffer, where we want to find the position vec from a global index
    public Dictionary<uint, int> VertexIndexMap = new();

    public List<Vector4> VertexColourSlots = new();
    public bool bAlphaClip;
    public bool HasSkeleton;
    public byte GearDyeChangeColorIndex = 0xFF;

    public DynamicMeshPart(SCB6E8080 part, EntityComponent parentResource) : base()
    {
        IndexOffset = part.IndexOffset;
        IndexCount = part.IndexCount;
        PrimitiveType = (PrimitiveType)part.PrimitiveType;
        VariantShaderIndex = part.VariantShaderIndex;

        if (VariantShaderIndex == -1)
            Material = part.Material;
        else
            Material = GetMaterialFromExternalMaterial(VariantShaderIndex, parentResource);
    }

    public DynamicMeshPart() : base()
    {
    }

    public void GetAllData(SEntityModelMesh mesh, SEntityModel model)
    {
        IndexBuffer = mesh.Indices;
        VertexBuffer0 = mesh.Vertices1;
        VertexBuffer1 = mesh.Vertices2;
        VertexBuffer2 = mesh.VertexColour;
        VertexBuffer3 = mesh.SinglePassSkinningBuffer;

        Indices = mesh.Indices.GetIndexData(PrimitiveType, IndexOffset, IndexCount);

        // Get unique vertex indices we need to get data for
        HashSet<uint> uniqueVertexIndices = new();
        foreach (UIntVector3 index in Indices)
        {
            uniqueVertexIndices.Add(index.X);
            uniqueVertexIndices.Add(index.Y);
            uniqueVertexIndices.Add(index.Z);
        }
        VertexIndices = uniqueVertexIndices.ToList();

        for (int i = 0; i < VertexIndices.Count; i++)
        {
            VertexIndexMap.Add(VertexIndices[i], i);
        }

        //Dictionary<uint, int> lookup = new Dictionary<uint, int>();
        //for (int i = 0; i < VertexIndices.Count; i++)
        //{
        //    lookup[VertexIndices[i]] = i;
        //}

        //Log.Debug($"Reading vertex buffers {mesh.Vertices1.Hash}/{mesh.Vertices1.TagData.Stride} and {mesh.Vertices2?.Hash}/{mesh.Vertices2?.TagData.Stride}");
        mesh.Vertices1.ReadVertexDataFromLayout(this, uniqueVertexIndices, 0);
        mesh.Vertices2?.ReadVertexDataFromLayout(this, uniqueVertexIndices, 1);

        if (mesh.OldWeights != null)
            mesh.OldWeights.ReadVertexData(this, uniqueVertexIndices, 2); // bufferIndex 2 is used for D1, shouldnt affect D2 I hope

        if (mesh.VertexColour != null)
            mesh.VertexColour.ReadVertexData(this, uniqueVertexIndices);

        if (mesh.SinglePassSkinningBuffer != null)
            mesh.SinglePassSkinningBuffer.ReadVertexData(this, uniqueVertexIndices);

        //Debug.Assert(VertexPositions.Count == VertexTexcoords0.Count && VertexPositions.Count == VertexNormals.Count);

        if (Material.EnumerateScopes().Any(x => x == TfxScope.SPEEDTREE))
            return;

        TransformPositions(mesh, model);
        TransformTexcoords(mesh, model);
    }

    private void TransformTexcoords(SEntityModelMesh mesh, SEntityModel header)
    {
        Vector2 texcoordScale = !Strategy.IsD1() ? header.TexcoordScale : mesh.TexcoordScale;
        Vector2 texcoordTranslation = !Strategy.IsD1() ? header.TexcoordTranslation : mesh.TexcoordTranslation;

        UVTransform = new Vector4(texcoordScale, texcoordTranslation);

        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            Vector2 tx = VertexTexcoords0[i];
            VertexTexcoords0[i] = new Vector2(
                tx.X * texcoordScale.X + texcoordTranslation.X,
                tx.Y * texcoordScale.Y + texcoordTranslation.Y
            );
        }

        // Detail UVs
        //TODO: Make outputs match what renderdoc says they actually are
        if (mesh.SinglePassSkinningBuffer != null)
        {
            try
            {
                short stride = mesh.SinglePassSkinningBuffer.TagData.Stride;
                using TigerReader handle = mesh.SinglePassSkinningBuffer.GetReferenceReader();

                for (int i = 0; i < VertexPositions.Count; i++)
                {
                    int normW = (int)(32767.0996f * VertexNormals[i].W);
                    uint index = (uint)normW >> 3;
                    index = index & 4095;

                    handle.Seek(index * stride, SeekOrigin.Begin);
                    float UVX = (float)handle.ReadHalf();
                    float UVY = (float)handle.ReadHalf();

                    Vector2 tx = VertexTexcoords0[i];
                    var tx1 = new Vector2(tx.X * UVX, ((tx.Y * UVY) * -1) - 0.65); // idfk whats going wrong here
                    VertexTexcoords1.Add(tx1);
                    //Console.WriteLine($"({i}) {mesh.SinglePassSkinningBuffer.Hash} {index} ({(index * 0x4):X}): XY ({tx.X}, {tx.Y}) ZW ({tx1.X}, {tx1.Y})");
                }
            }
            catch (Exception e)
            {
                Log.Error($"{mesh.SinglePassSkinningBuffer.Hash}: {e.Message}");
            }
        }
        else
        {
            VertexTexcoords1 = VertexTexcoords0.Select(tx1 => new Vector2(tx1.X * 5, (1 - tx1.Y) * 5)).ToList();
        }

        // Flip Y axis, fix detail UV offset
        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            Vector2 tx = VertexTexcoords0[i];
            Vector2 tx1 = VertexTexcoords1[i];
            VertexTexcoords0[i] = new Vector2(tx.X, 1f - tx.Y);
        }
    }

    private void TransformPositions(SEntityModelMesh mesh, SEntityModel header)
    {
        Vector4 modelScale = !Strategy.IsD1() ? header.ModelScale : mesh.ModelScale;
        Vector4 modelTranslation = !Strategy.IsD1() ? header.ModelTranslation : mesh.ModelTranslation;

        MeshScale = modelScale;
        MeshTransform = modelTranslation;

        for (int i = 0; i < VertexPositions.Count; i++)
        {
            VertexPositions[i] = new Vector4(
                VertexPositions[i].X * modelScale.X + modelTranslation.X,
                VertexPositions[i].Y * modelScale.Y + modelTranslation.Y,
                VertexPositions[i].Z * modelScale.Z + modelTranslation.Z,
                VertexPositions[i].W
            );
        }
    }

    public static Material? GetMaterialFromExternalMaterial(int variantShaderIndex, EntityComponent parentResource)
    {
        using TigerReader reader = parentResource.GetReader();

        DynamicArrayUnloaded<SExternalMaterialMapEntry> map = parentResource is EntityPhysicsModelParent ?
            ((S6C6D8080)parentResource.TagData.Unk18.GetValue(reader)).ExternalMaterialsMap :
            ((S8F6D8080)parentResource.TagData.Unk18.GetValue(reader)).ExternalMaterialsMap;

        DynamicArrayUnloaded<S14008080> mats = parentResource is EntityPhysicsModelParent ?
            ((S6C6D8080)parentResource.TagData.Unk18.GetValue(reader)).ExternalMaterials :
            ((S8F6D8080)parentResource.TagData.Unk18.GetValue(reader)).ExternalMaterials;

        if (map.Count == 0 || mats.Count == 0)
            return null;

        if (variantShaderIndex >= map.Count)
            return null; // todo this is actually wrong ig...

        SExternalMaterialMapEntry mapEntry = map[reader, variantShaderIndex];
        int permutationIndex = 0;
        if (parentResource is EntityModelParent parent && parent.MaterialPermutations is not null)
        {
            permutationIndex = parent.MaterialPermutations.OverrideIndex != -1
                ? parent.MaterialPermutations.OverrideIndex : parent.MaterialPermutations.CalculatePermutationIndex() ?? 0;
        }

        return mats[reader, mapEntry.MaterialStartIndex + (permutationIndex % mapEntry.MaterialCount)].Material;
    }

    public static void AddVertexColourSlotInfo(DynamicMeshPart dynamicPart, short w)
    {
        Vector4 vc = Vector4.Zero;
        switch (w & 0x7)
        {
            case 0:
                vc.X = 0.333f;
                break;
            case 1:
                vc.X = 0.666f;
                break;
            case 2:
                vc.X = 0.999f;
                break;
            case 3:
                vc.Y = 0.333f;
                break;
            case 4:
                vc.Y = 0.666f;
                break;
            case 5:
                vc.Y = 0.999f;
                break;
        }

        if (dynamicPart.bAlphaClip)
        {
            vc.Z = 0.25f;
        }

        dynamicPart.VertexColourSlots.Add(vc);
    }
}

