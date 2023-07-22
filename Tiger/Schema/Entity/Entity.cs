using System.Collections.Generic;

namespace Tiger.Schema.Entity;

public class Entity : Tag<D2Class_D89A8080>
{
    public List<EntityResource> Resources = new();

    // Entity features
    public EntitySkeleton Skeleton = null;
    public EntityModel Model = null;
    public EntityResource ModelParentResource = null;
    public EntityModel PhysicsModel = null;
    public EntityResource PatternAudio = null;
    public EntityResource PatternAudioUnnamed = null;
    public EntityControlRig ControlRig = null;

    protected Entity(FileHash hash) : base(hash)
    {
    }

    protected Entity(FileHash hash, bool shouldParse = true) : base(hash, shouldParse)
    {
    }

    public void Load()
    {
        foreach (var resource in _tag.EntityResources)
        {
            switch (resource.ResourceHash.TagData.Unk10.Value)
            {
                case D2Class_8A6D8080:  // Entity model
                    Model = ((D2Class_8F6D8080)resource.ResourceHash.TagData.Unk18.Value).Model;
                    ModelParentResource = resource.ResourceHash;
                    break;
                case D2Class_5B6D8080:  // Entity physics model
                    PhysicsModel = ((D2Class_6C6D8080)resource.ResourceHash.TagData.Unk18.Value).PhysicsModel;
                    break;
                case D2Class_DD818080:  // Entity skeleton FK
                    Skeleton = FileResourcer.Get().GetFile<EntitySkeleton>(resource.ResourceHash.Hash);
                    break;
                case D2Class_668B8080:  // Entity skeleton IK
                    ControlRig = FileResourcer.Get().GetFile<EntityControlRig>(resource.ResourceHash.Hash);
                    break;
                case D2Class_97318080:
                    PatternAudio = resource.ResourceHash;
                    break;
                case D2Class_F62C8080:
                    PatternAudioUnnamed = resource.ResourceHash;
                    break;
                default:
                    // throw new NotImplementedException($"Implement parsing for {resource.ResourceHash._tag.Unk08}");
                    break;
            }
        }
    }

    public List<DynamicPart> Load(ExportDetailLevel detailLevel)
    {
        var dynamicParts = new List<DynamicPart>();
        if (Model != null)
        {
            dynamicParts = dynamicParts.Concat(Model.Load(detailLevel, ModelParentResource)).ToList();
        }
        if (PhysicsModel != null)
        {
            dynamicParts = dynamicParts.Concat(PhysicsModel.Load(detailLevel, ModelParentResource)).ToList();
        }
        return dynamicParts;
    }

    public void SaveMaterialsFromParts(string saveDirectory, List<DynamicPart> dynamicParts, bool bSaveShaders)
    {
        Directory.CreateDirectory($"{saveDirectory}/Textures");
        Directory.CreateDirectory($"{saveDirectory}/Shaders");
        foreach (var dynamicPart in dynamicParts)
        {
            if (dynamicPart.Material == null) continue;
            dynamicPart.Material.SaveAllTextures($"{saveDirectory}/Textures");
            // dynamicPart.Material.SaveVertexShader(saveDirectory);
            if (bSaveShaders)
            {
                dynamicPart.Material.SavePixelShader($"{saveDirectory}/Shaders");
                dynamicPart.Material.SaveVertexShader($"{saveDirectory}/Shaders");
            }
            // Environment.Exit(5);
        }
    }

    public void SaveTexturePlates(string saveDirectory)
    {
        Directory.CreateDirectory($"{saveDirectory}/Textures/");
        if (((D2Class_8F6D8080) ModelParentResource.TagData.Unk18.Value).TexturePlates is null)
        {
            return;
        }
        var rsrc = ((D2Class_8F6D8080) ModelParentResource.TagData.Unk18.Value).TexturePlates.TagData;
        rsrc.AlbedoPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_albedo");
        rsrc.NormalPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_normal");
        rsrc.GStackPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_gstack");
        rsrc.DyemapPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_dyemap");
    }
}
