
using System.Diagnostics;
using Arithmic;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Entity;

public class EntityModel : Tag<SEntityModel>
{
    public EntityModel(FileHash hash) : base(hash)
    {
    }

    /*
     * We need the parent resource to get access to the external materials
     */
    public List<DynamicMeshPart> Load(ExportDetailLevel detailLevel, EntityResource parentResource)
    {
        Dictionary<int, Dictionary<int, D2Class_CB6E8080>> dynamicParts = GetPartsOfDetailLevel(detailLevel);
        List<DynamicMeshPart> parts = GenerateParts(dynamicParts, parentResource);
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
                if (eDetailLevel == ExportDetailLevel.AllLevels)
                {
                    parts[meshIndex].Add(partIndex++, part);
                }
                else
                {
                    if (eDetailLevel == ExportDetailLevel.MostDetailed && (part.LodCategory == ELodCategory.MainGeom0 ||
                                                                           part.LodCategory == ELodCategory.GripStock0 ||
                                                                           part.LodCategory == ELodCategory.Stickers0 ||
                                                                           part.LodCategory == ELodCategory.InternalGeom0 ||
                                                                           part.LodCategory == ELodCategory.Detail0))
                    {
                        parts[meshIndex].Add(partIndex++, part);
                    }
                    else if (eDetailLevel == ExportDetailLevel.LeastDetailed && part.LodCategory == ELodCategory.LowPolyGeom3)
                    {
                        parts[meshIndex].Add(partIndex++, part);
                    }
                }
            }

            meshIndex++;
        }

        return parts;
    }

    private List<DynamicMeshPart> GenerateParts(Dictionary<int, Dictionary<int, D2Class_CB6E8080>> dynamicParts, EntityResource parentResource)
    {
        List<DynamicMeshPart> parts = new();
        if (_tag.Meshes.Count == 0) return parts;

        int meshIndex = 0;
        foreach (SEntityModelMesh mesh in _tag.Meshes.Enumerate(GetReader()))
        {
            // Make part group map
            Dictionary<int, int> partGroups = new();
            HashSet<short> groups = new(mesh.StagePartOffsets.AsEnumerable());
            var groupList = groups.ToList();
            groupList.Remove(0x707);
            groupList.Sort();
            for (int i = 0; i < groupList.Count-1; i++)
            {
                for (int j = groupList[i]; j < groupList[i + 1]; j++)
                {
                    partGroups[j] = i;
                }
            }

            foreach ((int i, D2Class_CB6E8080 part) in dynamicParts[meshIndex])
            {
                DynamicMeshPart dynamicMeshPart = new(part, parentResource)
                {
                    Index = i, GroupIndex = partGroups[i], LodCategory = part.LodCategory,
                    bAlphaClip = (part.Flags & 0x8) != 0,
                    GearDyeChangeColorIndex = part.GearDyeChangeColorIndex
                };
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
    public byte GearDyeChangeColorIndex = 0xFF;

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

        // Have to call it like this b/c we don't know the format of the vertex data here
        Log.Debug($"Reading vertex buffers {mesh.Vertices1.Hash}/{mesh.Vertices1.TagData.Stride} and {mesh.Vertices2?.Hash}/{mesh.Vertices2?.TagData.Stride}");
        mesh.Vertices1.ReadVertexData(this, uniqueVertexIndices, 0, mesh.Vertices2 != null ? mesh.Vertices2.TagData.Stride : -1, false);
        mesh.Vertices2?.ReadVertexData(this, uniqueVertexIndices, 1, mesh.Vertices1.TagData.Stride,  false);
        if (mesh.OldWeights != null)
        {
            mesh.OldWeights.ReadVertexData(this, uniqueVertexIndices);
        }
        if (mesh.VertexColour != null)
        {
            mesh.VertexColour.ReadVertexData(this, uniqueVertexIndices);
        }
        if (mesh.SinglePassSkinningBuffer != null)
        {
            mesh.SinglePassSkinningBuffer.ReadVertexData(this, uniqueVertexIndices);
        }

        Debug.Assert(VertexPositions.Count == VertexTexcoords0.Count && VertexPositions.Count == VertexNormals.Count);

        TransformPositions(model);
        TransformTexcoords(model);
    }

    private void TransformTexcoords(SEntityModel header)
    {
        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            var tx = VertexTexcoords0[i];
            VertexTexcoords0[i] = new Vector2(
                tx.X * header.TexcoordScale.X + header.TexcoordTranslation.X,
                tx.Y * -header.TexcoordScale.Y + 1 - header.TexcoordTranslation.Y
            );
            VertexTexcoords1.Add(new Vector2(
                tx.X * header.TexcoordScale.X * 5 + header.TexcoordTranslation.X * 5,
                tx.Y * -header.TexcoordScale.Y * 5 + 1 - header.TexcoordTranslation.Y * 5
            ));
        }
    }

    private void TransformPositions(SEntityModel header)
    {
        for (int i = 0; i < VertexPositions.Count; i++)
        {
            VertexPositions[i] = new Vector4(
                VertexPositions[i].X * header.ModelScale.X + header.ModelTranslation.X,
                VertexPositions[i].Y * header.ModelScale.Y + header.ModelTranslation.Y,
                VertexPositions[i].Z * header.ModelScale.Z + header.ModelTranslation.Z,
                VertexPositions[i].W
            );
        }
    }

    private IMaterial? GetMaterialFromExternalMaterial(short externalMaterialIndex, EntityResource parentResource)
    {
        using TigerReader reader = parentResource.GetReader();

        List<IMaterial> materials = new();
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            var map = ((D2Class_8F6D8080) parentResource.TagData.Unk18.GetValue(reader)).ExternalMaterialsMapWQ;
            var mats = ((D2Class_8F6D8080) parentResource.TagData.Unk18.GetValue(reader)).ExternalMaterials;
            if (map.Count == 0 || mats.Count == 0)
            {
                return null;
            }
            if (externalMaterialIndex >= map.Count)
                return null; // todo this is actually wrong ig...

            var mapEntry = map[reader, externalMaterialIndex];
            // For now we'll just set as the first material in the array
            for (int i = mapEntry.MaterialStartIndex; i < mapEntry.MaterialStartIndex + mapEntry.MaterialCount; i++)
            {
                materials.Add(mats[reader, i].Material);
            }
        }
        else
        {
            return null; // todo shadowkeep
        }

        return materials[0];
    }
}
