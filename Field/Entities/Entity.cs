using System.Collections.Generic;
using Field;
using Field.General;
using Field.Models;

namespace Field.Entities;

public class Entity : Tag
{
    public D2Class_D89A8080 Header;
    public List<EntityResource> Resources;
    
    // Entity features
    public EntitySkeleton Skeleton = null;
    public EntityModel Model = null;
    public EntityResource ModelParentResource = null;
    public EntityModel PhysicsModel = null;
    public EntityControlRig ControlRig = null;
    
    public Entity(TagHash hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_D89A8080>();
    }

    protected override void ParseData()
    {
        foreach (var resource in Header.EntityResources)
        {
            switch (resource.ResourceHash.Header.Unk10)
            {
                case D2Class_8A6D8080:  // Entity model
                    Model = ((D2Class_8F6D8080)resource.ResourceHash.Header.Unk18).Model;
                    ModelParentResource = resource.ResourceHash;
                    break;
                case D2Class_5B6D8080:  // Entity physics model
                    PhysicsModel = ((D2Class_6C6D8080)resource.ResourceHash.Header.Unk18).PhysicsModel;
                    break;
                case D2Class_DD818080:  // Entity skeleton
                    Skeleton = new EntitySkeleton(resource.ResourceHash);
                    break;
                case D2Class_668B8080:  // Entity skeleton
                    ControlRig = new EntityControlRig(resource.ResourceHash);
                    break;
                default:
                    // throw new NotImplementedException($"Implement parsing for {resource.ResourceHash.Header.Unk08}");
                    break;
            }
        }
    }

    public List<DynamicPart> Load(ELOD detailLevel)
    {
        var dynamicParts = new List<DynamicPart>();
        if (PhysicsModel != null)
        {
            dynamicParts = dynamicParts.Concat(PhysicsModel.Load(detailLevel, ModelParentResource)).ToList();
        }
        if (Model != null)
        {
            dynamicParts = dynamicParts.Concat(Model.Load(detailLevel, ModelParentResource)).ToList();
        }
        return dynamicParts;
    }

    public void SaveMaterialsFromParts(string saveDirectory, List<DynamicPart> dynamicParts)
    {
        Directory.CreateDirectory($"{saveDirectory}/Textures");
        Directory.CreateDirectory($"{saveDirectory}/Shaders");
        foreach (var dynamicPart in dynamicParts)
        {
            if ($"{dynamicPart.IndexOffset}_{dynamicPart.IndexCount}_{dynamicPart.DetailLevel}" == "132170_2027_0")
            {
                var a = 0;
            }
            if (dynamicPart.Material == null) continue;
            dynamicPart.Material.SaveAllTextures($"{saveDirectory}/Textures");
            // dynamicPart.Material.SaveVertexShader(saveDirectory);
            dynamicPart.Material.SavePixelShader($"{saveDirectory}/Shaders");
            // Environment.Exit(5);
        }
    }

    public void SaveTexturePlates(string saveDirectory)
    {
        Directory.CreateDirectory($"{saveDirectory}/Textures/");
        if (((D2Class_8F6D8080) ModelParentResource.Header.Unk18).TexturePlates is null)
        {
            return;
        }
        var rsrc = ((D2Class_8F6D8080) ModelParentResource.Header.Unk18).TexturePlates.Header;
        rsrc.AlbedoPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_albedo.dds");
        rsrc.NormalPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_normal.dds");
        rsrc.GStackPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_gstack.dds");
        rsrc.DyemapPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_dyemap.dds");
    }
}