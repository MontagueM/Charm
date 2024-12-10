using System.Diagnostics;
using Arithmic;
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
        var _strategy = Strategy.CurrentStrategy;

        List<DynamicMeshPart> parts = new();
        List<int> exportPartRange = new();
        if (_tag.Meshes.Count == 0) return parts;
        int meshIndex = 0;
        foreach (SEntityModelMesh mesh in _tag.Meshes.Enumerate(GetReader()))
        {
            //Console.WriteLine($"{Hash}: Input Layout {mesh.GetInputLayoutForStage(0)}");
            exportPartRange = GetExportRanges(mesh);

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
                    VertexLayoutIndex = mesh.GetInputLayoutForStage(0)
                };

                //We only care about the vertex shader for now for mesh data
                //But if theres also no pixel shader then theres no point in adding it
                if (dynamicMeshPart.Material is null ||
                dynamicMeshPart.Material.Vertex.Shader is null ||
                dynamicMeshPart.Material.Pixel.Shader is null) // || dynamicMeshPart.Material.Unk08 != 1)
                    continue;

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

        //foreach (TfxRenderStage stage in ExportRenderStages)
        //{
        //    var range = mesh.GetRangeForStage((int)stage);
        //    if (!(range.Start.Value < range.End.Value))
        //        continue;
        //    Console.WriteLine($"Part Range: {mesh.GetRangeForStage((int)stage).Start.Value}-{mesh.GetRangeForStage((int)stage).End.Value - 1} : {stage}");
        //}

        foreach (TfxRenderStage stage in Globals.Get().ExportRenderStages)
        {
            var range = mesh.GetRangeForStage((int)stage);
            if (!(range.Start.Value < range.End.Value))
                continue;

            for (int i = range.Start.Value; i < range.End.Value; i++)
                exportPartRange.Add(i);
        }

        return exportPartRange;
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
            Material = part.Material;
        else
            Material = GetMaterialFromExternalMaterial(part.VariantShaderIndex, parentResource);
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

        Log.Debug($"Reading vertex buffers {mesh.Vertices1.Hash}/{mesh.Vertices1.TagData.Stride} and {mesh.Vertices2?.Hash}/{mesh.Vertices2?.TagData.Stride}");
        mesh.Vertices1.ReadVertexDataFromLayout(this, uniqueVertexIndices, 0);
        mesh.Vertices2?.ReadVertexDataFromLayout(this, uniqueVertexIndices, 1);

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
        Vector2 texcoordScale = !Strategy.IsD1() ? header.TexcoordScale : mesh.TexcoordScale;
        Vector2 texcoordTranslation = !Strategy.IsD1() ? header.TexcoordTranslation : mesh.TexcoordTranslation;
        float yOffset = 0f;//5f / 3f; // idfk

        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            var tx = VertexTexcoords0[i];
            VertexTexcoords0[i] = new Vector2(
                tx.X * texcoordScale.X + texcoordTranslation.X,
                tx.Y * texcoordScale.Y + texcoordTranslation.Y
            );
        }

        // Detail UVs
        if (mesh.SinglePassSkinningBuffer != null)
        {
            try
            {
                var stride = mesh.SinglePassSkinningBuffer.TagData.Stride;
                using TigerReader handle = mesh.SinglePassSkinningBuffer.GetReferenceReader();
                //var data = mesh.SinglePassSkinningBuffer.GetReferenceData();
                for (int i = 0; i < VertexPositions.Count; i++)
                {
                    int normW = (int)(32767.0996f * VertexNormals[i].W);
                    uint index = (uint)normW >> 3;
                    index = index & 4095;

                    handle.Seek(index * stride, SeekOrigin.Begin);
                    float UVX = (float)handle.ReadHalf();
                    float UVY = (float)handle.ReadHalf();

                    var tx = VertexTexcoords0[i];
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
            yOffset = 0f;
            VertexTexcoords1 = VertexTexcoords0.Select(tx1 => new Vector2(tx1.X * 5, (1 - tx1.Y) * 5)).ToList();
        }

        // Flip Y axis, fix detail UV offset
        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            var tx = VertexTexcoords0[i];
            var tx1 = VertexTexcoords1[i];
            VertexTexcoords0[i] = new Vector2(tx.X, 1f - tx.Y);
            // VertexTexcoords1[i] = new Vector2(tx1.X, tx1.Y);
        }
    }

    private void TransformPositions(SEntityModelMesh mesh, SEntityModel header)
    {
        Vector4 modelScale = !Strategy.IsD1() ? header.ModelScale : mesh.ModelScale;
        Vector4 modelTranslation = !Strategy.IsD1() ? header.ModelTranslation : mesh.ModelTranslation;

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

    private Material? GetMaterialFromExternalMaterial(short variantShaderIndex, EntityResource parentResource)
    {
        using TigerReader reader = parentResource.GetReader();

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
