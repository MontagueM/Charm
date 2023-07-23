
namespace Tiger.Schema.Entity;

public class EntityModel : Tag<D2Class_076F8080>
{
    protected EntityModel(FileHash hash) : base(hash)
    {
    }

    /*
     * We need the parent resource to get access to the external materials
     */
    public List<DynamicPart> Load(ExportDetailLevel detailLevel, EntityResource parentResource)
    {
        Dictionary<int, D2Class_CB6E8080> dynamicParts = GetPartsOfDetailLevel(detailLevel);
        List<DynamicPart> parts = GenerateParts(dynamicParts, parentResource);
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
    private Dictionary<int, D2Class_CB6E8080> GetPartsOfDetailLevel(ExportDetailLevel eDetailLevel)
    {
        Dictionary<int, D2Class_CB6E8080> parts = new Dictionary<int, D2Class_CB6E8080>();

        for (var i = 0; i < _tag.Meshes[0].Parts.Count; i++)
        {
            D2Class_CB6E8080 part = _tag.Meshes[0].Parts[i];
            if (eDetailLevel == ExportDetailLevel.AllLevels)
            {
                parts.Add(i, part);
            }
            else
            {
                if (eDetailLevel == ExportDetailLevel.MostDetailed && (part.LodCategory == ELodCategory.MainGeom0 ||
                                                        part.LodCategory == ELodCategory.GripStock0 ||
                                                        part.LodCategory == ELodCategory.Stickers0 ||
                                                        part.LodCategory == ELodCategory.InternalGeom0 ||
                                                        part.LodCategory == ELodCategory.Detail0))
                {
                    parts.Add(i, part);
                }
                else if (eDetailLevel == ExportDetailLevel.LeastDetailed && part.LodCategory == ELodCategory.LowPolyGeom3)
                {
                    parts.Add(i, part);
                }
            }
        }

        return parts;
    }

    private List<DynamicPart> GenerateParts(Dictionary<int, D2Class_CB6E8080> dynamicParts, EntityResource parentResource)
    {
        List<DynamicPart> parts = new List<DynamicPart>();
        if (_tag.Meshes.Count > 1) throw new Exception("Multiple meshes not supported");
        if (_tag.Meshes.Count == 0) return new List<DynamicPart>();
        D2Class_C56E8080 mesh = _tag.Meshes[0];

        // Make part group map
        Dictionary<int, int> partGroups = new Dictionary<int, int>();
        HashSet<short> groups = new HashSet<short>(mesh.StagePartOffsets.AsEnumerable());
        var groupList = groups.ToList();
        groupList.Remove(0x707);
        groupList.Sort();
        for (var i = 0; i < groupList.Count-1; i++)
        {
            for (int j = groupList[i]; j < groupList[i + 1]; j++)
            {
                partGroups[j] = i;
            }
        }

        foreach (var (i, part) in dynamicParts)
        {
            DynamicPart dynamicPart = new(part, parentResource);
            dynamicPart.Index = i;
            dynamicPart.GroupIndex = partGroups[i];
            dynamicPart.LodCategory = part.LodCategory;
            dynamicPart.bAlphaClip = (part.Flags & 0x8) != 0;
            dynamicPart.GearDyeChangeColorIndex = part.GearDyeChangeColorIndex;
            dynamicPart.GetAllData(mesh, _tag);
            parts.Add(dynamicPart);
        }
        return parts;
    }

}

public class DynamicPart : MeshPart
{
    public List<VertexWeight> VertexWeights = new List<VertexWeight>();
    public bool bAlphaClip;
    public byte GearDyeChangeColorIndex = 0xFF;

    public DynamicPart(D2Class_CB6E8080 part, EntityResource parentResource) : base()
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

    public DynamicPart() : base()
    {
    }

    public void GetAllData(D2Class_C56E8080 mesh, D2Class_076F8080 model)
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
        mesh.Vertices1.ReadVertexData(this, uniqueVertexIndices);
        mesh.Vertices2.ReadVertexData(this, uniqueVertexIndices);
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

        TransformPositions(model);
        TransformTexcoords(model);
    }

    private void TransformTexcoords(D2Class_076F8080 header)
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

    private void TransformPositions(D2Class_076F8080 header)
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

    private Material GetMaterialFromExternalMaterial(short externalMaterialIndex, EntityResource parentResource)
    {
        var map = ((D2Class_8F6D8080) parentResource.TagData.Unk18.Value).ExternalMaterialsMap;
        var mats = ((D2Class_8F6D8080) parentResource.TagData.Unk18.Value).ExternalMaterials;
        if (map.Count == 0 || mats.Count == 0)
        {
            return null;
        }
        if (externalMaterialIndex >= map.Count)
            return null; // todo this is actually wrong ig...
        var mapEntry = map[externalMaterialIndex];
        // For now we'll just set as the first material in the array
        List<Material> materials = new List<Material>();
        for (int i = mapEntry.MaterialStartIndex; i < mapEntry.MaterialStartIndex + mapEntry.MaterialCount; i++)
        {
            materials.Add(mats[i].Material);
        }

        return materials[0];
    }
}
