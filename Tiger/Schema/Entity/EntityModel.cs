using System.Diagnostics;
using Arithmic;
using Tiger.Schema.Model;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Entity;

public class EntityModel : Tag<SEntityModel>
{
    public EntityModel(FileHash hash) : base(hash)
    {
    }

    public Vector4 RotationOffset = new();
    public Vector4 TranslationOffset = new();

    /*
     * We need the parent resource to get access to the external materials
     */
    public List<DynamicMeshPart> Load(ExportDetailLevel detailLevel, EntityResource parentResource, bool transparentsOnly = false, bool hasSkeleton = false)
    {
        Dictionary<int, Dictionary<int, D2Class_CB6E8080>> dynamicParts = GetPartsOfDetailLevel(detailLevel);
        List<DynamicMeshPart> parts = GenerateParts(dynamicParts, parentResource, hasSkeleton);
        if (transparentsOnly) // ROI decal/transparent mesh purposes. I hate this and its not the right way to do this
            return parts.Where(x => x.Material.Unk20 != 0).ToList();
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
    private Dictionary<int, Dictionary<int, D2Class_CB6E8080>> GetPartsOfDetailLevel(ExportDetailLevel eDetailLevel)
    {
        Dictionary<int, Dictionary<int, D2Class_CB6E8080>> parts = new();

        using TigerReader reader = GetReader();

        int meshIndex = 0;
        foreach (SEntityModelMesh mesh in _tag.Meshes.Enumerate(GetReader()))
        {
            int partIndex = 0;
            parts.Add(meshIndex, new Dictionary<int, D2Class_CB6E8080>());
            for (int i = 0; i < mesh.Parts.Count; i++)
            {
                D2Class_CB6E8080 part = mesh.Parts[reader, i];
                //Console.WriteLine($"{i}--------------");
                //Console.WriteLine($"Material {part.Material?.FileHash}");
                //Console.WriteLine($"VariantShaderIndex {part.VariantShaderIndex}");
                //Console.WriteLine($"PrimitiveType {part.PrimitiveType}");
                //Console.WriteLine($"IndexOffset {part.IndexOffset}");
                //Console.WriteLine($"IndexCount {part.IndexCount}");
                //Console.WriteLine($"Unk10 {part.Unk10}");
                //Console.WriteLine($"ExternalIdentifier {part.ExternalIdentifier}");
                //Console.WriteLine($"Unk16 {part.Unk16}");
                //Console.WriteLine($"FlagsD1 {part.FlagsD1}");
                //Console.WriteLine($"GearDyeChangeColorIndex {part.GearDyeChangeColorIndex}");
                //Console.WriteLine($"LodCategory {part.LodCategory}");

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

    private List<DynamicMeshPart> GenerateParts(Dictionary<int, Dictionary<int, D2Class_CB6E8080>> dynamicParts, EntityResource parentResource, bool hasSkeleton = false)
    {
        List<DynamicMeshPart> parts = new();
        List<int> exportPartRange = new();
        if (_tag.Meshes.Count == 0) return parts;
        int meshIndex = 0;
        foreach (SEntityModelMesh mesh in _tag.Meshes.Enumerate(GetReader()))
        {
            Console.WriteLine($"Input Layout {mesh.GetInputLayoutForStage(TfxRenderStage.GenerateGbuffer)}");
            foreach (TfxRenderStage stage in (TfxRenderStage[])Enum.GetValues(typeof(TfxRenderStage)))
            {
                if (stage == TfxRenderStage.ComputeSkinning && Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999)
                    continue;

                var range = mesh.GetRangeForStage(stage);
                if (!(range.Start.Value < range.End.Value))
                    continue;
                Console.WriteLine($"Part Range: {mesh.GetRangeForStage(stage).Start.Value}-{mesh.GetRangeForStage(stage).End.Value - 1} : {stage}");
            }

            foreach (TfxRenderStage stage in VertexLayouts.ExportRenderStages)
            {
                var range = mesh.GetRangeForStage(stage);
                if (!(range.Start.Value < range.End.Value))
                    continue;

                for (int i = range.Start.Value; i < range.End.Value; i++)
                {
                    exportPartRange.Add(i);
                }

                Console.WriteLine($"Export Part Range: {mesh.GetRangeForStage(stage).Start.Value}-{mesh.GetRangeForStage(stage).End.Value - 1} : {stage}");
            }

            foreach ((int i, D2Class_CB6E8080 part) in dynamicParts[meshIndex])
            {
                if (!exportPartRange.Contains(i))
                    continue;

                DynamicMeshPart dynamicMeshPart = new(part, parentResource)
                {
                    Index = i,
                    GroupIndex = part.ExternalIdentifier,
                    LodCategory = part.LodCategory,
                    bAlphaClip = (part.GetFlags() & 0x8) != 0,
                    GearDyeChangeColorIndex = part.GearDyeChangeColorIndex,
                    HasSkeleton = hasSkeleton,
                    RotationOffset = RotationOffset,
                    TranslationOffset = TranslationOffset,
                    VertexLayoutIndex = mesh.GetInputLayoutForStage(TfxRenderStage.GenerateGbuffer)
                };
                //We only care about the vertex shader for now for mesh data
                //But if theres also no pixel shader then theres no point in adding it
                if (Strategy.CurrentStrategy > TigerStrategy.DESTINY1_RISE_OF_IRON)
                {
                    if (dynamicMeshPart.Material is null ||
                    dynamicMeshPart.Material.VertexShader is null ||
                    dynamicMeshPart.Material.PixelShader is null)
                        //dynamicMeshPart.Material.Unk08 != 1 ||
                        //(dynamicMeshPart.Material.Unk20 & 0x8000) != 0)
                        continue;
                }
                else
                {
                    if (dynamicMeshPart.Material is null ||
                    dynamicMeshPart.Material.VertexShader is null ||
                    dynamicMeshPart.Material.PixelShader is null) // || dynamicMeshPart.Material.Unk08 != 1)
                        continue;

                    //if (dynamicMeshPart.Material.Unk08 != 1)
                    //    Console.WriteLine($"{dynamicMeshPart.Material.FileHash}");
                }

                dynamicMeshPart.GetAllData(mesh, _tag);
                parts.Add(dynamicMeshPart);
            }

            meshIndex++;
        }

        return parts;
    }
}

public class DynamicMeshPart : MeshPart
{
    public List<VertexWeight> VertexWeights = new List<VertexWeight>();

    // used for single-pass skin buffer, where we want to find the position vec from a global index
    public Dictionary<uint, int> VertexIndexMap = new Dictionary<uint, int>();

    public List<Vector4> VertexColourSlots = new List<Vector4>();
    public bool bAlphaClip;
    public bool HasSkeleton;
    public byte GearDyeChangeColorIndex = 0xFF;

    public Vector4 RotationOffset = new();
    public Vector4 TranslationOffset = new();

    public DynamicMeshPart(D2Class_CB6E8080 part, EntityResource parentResource) : base()
    {
        IndexOffset = part.IndexOffset;
        IndexCount = part.IndexCount;
        PrimitiveType = (PrimitiveType)part.PrimitiveType;
        if (part.VariantShaderIndex == -1)
        {
            Material = part.Material;
        }
        else
        {
            Material = GetMaterialFromExternalMaterial(part.VariantShaderIndex, parentResource);
        }
    }

    public DynamicMeshPart() : base()
    {
    }

    public void GetAllData(SEntityModelMesh mesh, SEntityModel model)
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

        for (int i = 0; i < VertexIndices.Count; i++)
        {
            VertexIndexMap.Add(VertexIndices[i], i);
        }

        if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999 && Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            DXBCIOSignature[] inputSignatures = Material.VertexShader.InputSignatures.ToArray();
            int b0Stride = mesh.Vertices1.TagData.Stride;
            int b1Stride = mesh.Vertices2?.TagData.Stride ?? 0;
            List<DXBCIOSignature> inputSignatures0 = new();
            List<DXBCIOSignature> inputSignatures1 = new();
            int stride = 0;
            foreach (DXBCIOSignature inputSignature in inputSignatures)
            {
                if (stride < b0Stride)
                    inputSignatures0.Add(inputSignature);
                else
                    inputSignatures1.Add(inputSignature);

                if (inputSignature.Semantic == DXBCSemantic.Colour || inputSignature.Semantic == DXBCSemantic.BlendIndices || inputSignature.Semantic == DXBCSemantic.BlendWeight)
                    stride += inputSignature.GetNumberOfComponents() * 1;  // 1 byte per component
                else
                    stride += inputSignature.GetNumberOfComponents() * 2;  // 2 bytes per component
            }

            Log.Debug($"Reading vertex buffers {mesh.Vertices1.Hash}/{mesh.Vertices1.TagData.Stride}/{inputSignatures.Where(s => s.BufferIndex == 0).DebugString()} and {mesh.Vertices2?.Hash}/{mesh.Vertices2?.TagData.Stride}/{inputSignatures.Where(s => s.BufferIndex == 1).DebugString()}");
            mesh.Vertices1.ReadVertexDataSignatures(this, uniqueVertexIndices, inputSignatures0, false);
            mesh.Vertices2?.ReadVertexDataSignatures(this, uniqueVertexIndices, inputSignatures1, false);
        }
        else
        {
            mesh.Vertices1.ReadVertexDataFromLayout(this, uniqueVertexIndices, 0);
            mesh.Vertices2?.ReadVertexDataFromLayout(this, uniqueVertexIndices, 1);

            // Have to call it like this b/c we don't know the format of the vertex data here
            //Log.Debug($"Reading vertex buffers {mesh.Vertices1.Hash}/{mesh.Vertices1.TagData.Stride} and {mesh.Vertices2?.Hash}/{mesh.Vertices2?.TagData.Stride}");
            //mesh.Vertices1.ReadVertexData(this, uniqueVertexIndices, 0, mesh.Vertices2 != null ? mesh.Vertices2.TagData.Stride : -1, false);
            //mesh.Vertices2?.ReadVertexData(this, uniqueVertexIndices, 1, mesh.Vertices1.TagData.Stride, false);
        }


        if (mesh.OldWeights != null)
            mesh.OldWeights.ReadVertexData(this, uniqueVertexIndices, 2); // bufferIndex 2 is used for D1, shouldnt affect D2 I hope

        if (mesh.VertexColour != null)
            mesh.VertexColour.ReadVertexData(this, uniqueVertexIndices);

        if (mesh.SinglePassSkinningBuffer != null)
            mesh.SinglePassSkinningBuffer.ReadVertexData(this, uniqueVertexIndices);

        Debug.Assert(VertexPositions.Count == VertexTexcoords0.Count && VertexPositions.Count == VertexNormals.Count);

        TransformPositions(mesh, model);
        TransformTexcoords(mesh, model);
    }

    private void TransformTexcoords(SEntityModelMesh mesh, SEntityModel header)
    {
        Vector2 texcoordScale = Strategy.CurrentStrategy > TigerStrategy.DESTINY1_RISE_OF_IRON ? header.TexcoordScale : mesh.TexcoordScale;
        Vector2 texcoordTranslation = Strategy.CurrentStrategy > TigerStrategy.DESTINY1_RISE_OF_IRON ? header.TexcoordTranslation : mesh.TexcoordTranslation;

        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            var tx = VertexTexcoords0[i];
            VertexTexcoords0[i] = new Vector2(
                tx.X * texcoordScale.X + texcoordTranslation.X,
                tx.Y * -texcoordScale.Y + 1 - texcoordTranslation.Y
            );
            VertexTexcoords1.Add(new Vector2(
                tx.X * texcoordScale.X * 5 + texcoordTranslation.X * 5,
                tx.Y * -texcoordScale.Y * 5 + 1 - texcoordTranslation.Y * 5
            ));
        }
    }

    private void TransformPositions(SEntityModelMesh mesh, SEntityModel header)
    {
        Vector4 modelScale = Strategy.CurrentStrategy > TigerStrategy.DESTINY1_RISE_OF_IRON ? header.ModelScale : mesh.ModelScale;
        Vector4 modelTranslation = Strategy.CurrentStrategy > TigerStrategy.DESTINY1_RISE_OF_IRON ? header.ModelTranslation : mesh.ModelTranslation;

        for (int i = 0; i < VertexPositions.Count; i++)
        {
            VertexPositions[i] = new Vector4(
                VertexPositions[i].X * modelScale.X + modelTranslation.X + TranslationOffset.X,
                VertexPositions[i].Y * modelScale.Y + modelTranslation.Y + TranslationOffset.Y,
                VertexPositions[i].Z * modelScale.Z + modelTranslation.Z + TranslationOffset.Z,
                VertexPositions[i].W
            );
        }
    }

    private IMaterial? GetMaterialFromExternalMaterial(short variantShaderIndex, EntityResource parentResource)
    {
        using TigerReader reader = parentResource.GetReader();

        List<IMaterial> materials = new();

        var map = parentResource is EntityPhysicsModelParent ?
            ((D2Class_6C6D8080)parentResource.TagData.Unk18.GetValue(reader)).ExternalMaterialsMap :
            ((D2Class_8F6D8080)parentResource.TagData.Unk18.GetValue(reader)).ExternalMaterialsMap;

        var mats = parentResource is EntityPhysicsModelParent ?
            ((D2Class_6C6D8080)parentResource.TagData.Unk18.GetValue(reader)).ExternalMaterials :
            ((D2Class_8F6D8080)parentResource.TagData.Unk18.GetValue(reader)).ExternalMaterials;

        if (map.Count == 0 || mats.Count == 0)
        {
            return null;
        }
        if (variantShaderIndex >= map.Count)
            return null; // todo this is actually wrong ig...

        var mapEntry = map[reader, variantShaderIndex];

        return mats[reader, mapEntry.MaterialStartIndex + (0 % mapEntry.MaterialCount)].Material;
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
