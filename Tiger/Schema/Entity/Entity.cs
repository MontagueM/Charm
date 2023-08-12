using System.Collections.Generic;

namespace Tiger.Schema.Entity;

public class Entity : Tag<SEntity>
{
    public List<EntityResource> Resources = new();

    // Entity features
    public EntitySkeleton? Skeleton { get; private set; }
    public EntityModel? Model { get; private set; }
    public EntityResource? ModelParentResource { get; private set; }
    public EntityModel? PhysicsModel { get; private set; }
    public EntityResource? PatternAudio { get; private set; }
    public EntityResource? PatternAudioUnnamed { get; private set; }
    public EntityControlRig? ControlRig { get; private set; }

    private bool _loaded = false;

    public Entity(FileHash hash) : base(hash)
    {
        Load();
    }

    public Entity(FileHash hash, bool shouldParse = true) : base(hash, shouldParse)
    {
        if (shouldParse)
        {
            Load();
        }
    }

    public void Load()
    {
        _loaded = true;
        foreach (var resource in _tag.EntityResources.Select(GetReader(), r => r.Resource))
        {
            switch (resource.TagData.Unk10.GetValue(resource.GetReader()))
            {
                case D2Class_8A6D8080:  // Entity model
                    Model = ((D2Class_8F6D8080)resource.TagData.Unk18.GetValue(resource.GetReader())).Model;
                    ModelParentResource = resource;
                    break;
                case D2Class_5B6D8080:  // Entity physics model  todo shadowkeep
                    PhysicsModel = ((D2Class_6C6D8080)resource.TagData.Unk18.GetValue(resource.GetReader())).PhysicsModel;
                    break;
                case D2Class_DD818080:  // Entity skeleton FK
                    Skeleton = FileResourcer.Get().GetFile<EntitySkeleton>(resource.Hash);
                    break;
                case D2Class_668B8080:  // Entity skeleton IK  todo shadowkeep
                    ControlRig = FileResourcer.Get().GetFile<EntityControlRig>(resource.Hash);
                    break;
                case D2Class_97318080: // todo shadowkeep
                    PatternAudio = resource;
                    break;
                case D2Class_F62C8080: // todo shadowkeep
                    PatternAudioUnnamed = resource;
                    break;
                default:
                    // throw new NotImplementedException($"Implement parsing for {resource.Resource._tag.Unk08}");
                    break;
            }
        }
    }

    public List<DynamicMeshPart> Load(ExportDetailLevel detailLevel)
    {
        if (!_loaded)
        {
            Load();
        }
        var dynamicParts = new List<DynamicMeshPart>();
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

    public void SaveMaterialsFromParts(string saveDirectory, List<DynamicMeshPart> dynamicParts, bool bSaveShaders)
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
        if (((D2Class_8F6D8080) ModelParentResource.TagData.Unk18.GetValue(ModelParentResource.GetReader())).TexturePlates is null)
        {
            return;
        }
        var rsrc = ((D2Class_8F6D8080) ModelParentResource.TagData.Unk18.GetValue(ModelParentResource.GetReader())).TexturePlates.TagData;
        rsrc.AlbedoPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_albedo");
        rsrc.NormalPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_normal");
        rsrc.GStackPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_gstack");
        rsrc.DyemapPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_dyemap");
    }

    public bool HasGeometry()
    {
        if (!_loaded)
        {
            Load();
        }
        return Model != null;
    }
}
