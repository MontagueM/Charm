using System.Diagnostics;
using Tiger.Exporters;

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
        Deserialize();
        _loaded = true;
        Debug.Assert(_tag.FileSize != 0);
        foreach (var resourceHash in _tag.EntityResources.Select(GetReader(), r => r.Resource))
        {
            if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON && resourceHash.GetReferenceHash() != 0x80800861)
                continue;
            EntityResource resource = FileResourcer.Get().GetFile<EntityResource>(resourceHash);
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
                    //Console.WriteLine($"{resource.TagData.Unk18.GetValue(resource.GetReader())}");
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

    public void SaveMaterialsFromParts(ExporterScene scene, List<DynamicMeshPart> dynamicParts)
    {
        foreach (var dynamicPart in dynamicParts)
        {
            if (dynamicPart.Material == null) continue;
            scene.Materials.Add(new ExportMaterial(dynamicPart.Material));
        }
    }

    public void SaveTexturePlates(string saveDirectory)
    {
        Directory.CreateDirectory($"{saveDirectory}/Textures/");
        var parentResource = (D2Class_8F6D8080)ModelParentResource.TagData.Unk18.GetValue(ModelParentResource.GetReader());

        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_SHADOWKEEP_2601 && parentResource.TexturePlates is null)
            return;

        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON && (parentResource.TexturePlatesROI.Count == 0 || parentResource.TexturePlatesROI[0].TexturePlates is null))
            return;

        var rsrc = Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON
            ? parentResource.TexturePlates.TagData : parentResource.TexturePlatesROI[0].TexturePlates.TagData;


        rsrc.AlbedoPlate?.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_albedo");
        rsrc.NormalPlate?.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_normal");
        rsrc.GStackPlate?.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_gstack");
        rsrc.DyemapPlate?.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_dyemap");
    }

    private readonly object _lock = new();
    public bool HasGeometry()
    {
        lock (_lock)
        {
            if (!_loaded)
            {
                Load();
            }
        }
        return Model != null;
    }

    public List<Entity> GetEntityChildren()
    {
        lock (_lock)
        {
            if (!_loaded)
            {
                Load();
            }
        }

        List<Entity> entities = new List<Entity>();
        foreach (var resourceHash in _tag.EntityResources.Select(GetReader(), r => r.Resource))
        {
            if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON && resourceHash.GetReferenceHash() != 0x80800861)
                continue;

            EntityResource resource = FileResourcer.Get().GetFile<EntityResource>(resourceHash);
            //Weird to have to get Unk10 first to be able to get Unk18
            //Trying to get Unk18 directly just crashes
            switch (resource.TagData.Unk10.GetValue(resource.GetReader()))
            {
                case D2Class_12848080:
                    foreach (var entry in ((D2Class_0E848080)resource.TagData.Unk18.GetValue(resource.GetReader())).Unk88)
                    {
                        foreach (var entry2 in entry.Unk08)
                        {
                            if (entry2.Unk08 is null)
                                continue;

                            Entity entity = FileResourcer.Get().GetFile<Entity>(entry2.Unk08.Hash);
                            if (entity.HasGeometry())
                            {
                                entities.Add(entity);
                                //Just in case
                                foreach (var child in entity.GetEntityChildren())
                                {
                                    entities.Add(child);
                                }
                            }
                        }
                    }
                    break;
            }
        }
        return entities;
    }
}
