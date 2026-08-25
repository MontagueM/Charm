using System.Diagnostics;
using System.Numerics;
using Arithmic;
using Tiger.Exporters;

namespace Tiger.Schema.Entity;

public class Entity : Tag<SEntity>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.DynamicObjects;
    // Entity features, todo clean this up
    public EntitySkeleton? Skeleton { get; set; }
    public EntityControlRig? ControlRig { get; private set; }
    public EntityModelParent? ModelParent { get; private set; }
    public EntityPhysicsModelParent? PhysicsModelParent { get; private set; }

    public EntityComponent? PatternAudio { get; private set; }
    public EntityComponent? PatternAudioUnnamed { get; private set; }
    public EntityComponent? CarriedWeapon { get; private set; }
    public EntityComponent? Attachments { get; private set; }
    public EntityComponent? EntityChildren { get; private set; }

    public List<EntitySequencer>? Sequences { get; private set; } // The Sequencer (tm) ?

    public EntityModel? Model => ModelParent?.GetModel();
    public EntityModel? PhysicsModel => PhysicsModelParent?.GetModel();

    public string? EntityName { get; set; } // Usually just the generic name (Ogre, Vandal, etc)
    public DestinyGenderDefinition Gender { get; set; } = DestinyGenderDefinition.None; // Only used for player armor

    public IEnumerable<FileHash> Components => _tag.EntityComponents.Select(GetReader(), r => r.Resource);

    private bool _loaded = false;
    public Entity(FileHash hash, bool shouldParse = true) : base(hash, shouldParse)
    {
        if (shouldParse)
            Load();
    }


    // TODO: Try to speed up a little more.
    // TagListView LoadEntityList currently down to ~33 seconds with roughly 4.5 - 5gb of ram usage
    public void Load()
    {
        Deserialize();
        if (_tag.FileSize == 0)
            return;

        _loaded = true;
        foreach (FileHash? resourceHash in Components)
        {
            if (Strategy.IsD1() && resourceHash.GetReferenceHash() != 0x80800861)
                continue;

            EntityComponent resource = FileResourcer.Get().GetFile<EntityComponent>(resourceHash);
            switch (resource.TagData.Unk10.GetValue(resource.GetReader(), false))
            {
                case S80806D8A:  // Entity model
                    ModelParent = FileResourcer.Get().GetFile<EntityModelParent>(resource.Hash);
                    break;

                case S80806D5B:  // Entity physics model
                    PhysicsModelParent = FileResourcer.Get().GetFile<EntityPhysicsModelParent>(resource.Hash);
                    break;

                case S808081D5:
                case S808081DD:  // Entity skeleton FK
                    Skeleton = FileResourcer.Get().GetFile<EntitySkeleton>(resource.Hash);
                    break;

                //case S80808B66:  // Entity skeleton IK 
                //    ControlRig = FileResourcer.Get().GetFile<EntityControlRig>(resource.Hash);
                //    break;

                case S80803197: // todo? shadowkeep
                    PatternAudio = resource;
                    break;

                case S80802CF6: // todo? shadowkeep
                    PatternAudioUnnamed = resource;
                    break;

                case S80807C35: // Generic name
                    // we care more about the specific name so if the entity name is already assigned, dont assign this one
                    if (EntityName == null)
                    {
                        StringHash genericName = ((S80808018)resource.TagData.Unk18.GetValue(resource.GetReader())).Unk3C0.TagData.EntityName;
                        if (GlobalStrings.Get().GetString(genericName) != genericName)
                            EntityName = GlobalStrings.Get().GetString(genericName);
                    }
                    break;

                case S80805EDA: // Specific name
                    StringHash specificName = Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON ?
                        ((S80805EDB)resource.TagData.Unk18.GetValue(resource.GetReader())).Unk108.TagData.EntityName :
                        ((S80805EDB)resource.TagData.Unk18.GetValue(resource.GetReader())).EntityName;

                    // Don't assign a name if the name hash doesnt return an actual string (returns the name hash instead)
                    if (GlobalStrings.Get().GetString(specificName) != specificName)
                        EntityName = GlobalStrings.Get().GetString(specificName);
                    break;

                case S80809479:
                    if (Sequences is null)
                        Sequences = new();

                    Sequences.Add(new(resource.Hash));
                    break;

                case S80808412:
                    EntityChildren = resource;
                    break;

                case S808031E9:
                    CarriedWeapon = resource;
                    break;

                case S80802E74:
                    Attachments = resource;
                    break;

                default:
                    //Console.WriteLine($"{resource.TagData.Unk18.GetValue(resource.GetReader())}");
                    // throw new NotImplementedException($"Implement parsing for {resource.Resource._tag.Unk08}");
                    break;
            }
        }
    }

    /// <summary>
    /// Loads both the normal model and physics model into dynamic mesh parts
    /// </summary>
    /// <param name="detailLevel"></param>
    /// <param name="loadLevel"></param>
    /// <returns></returns>
    public List<DynamicMeshPart> Load(ExportDetailLevel detailLevel, LoadLevel loadLevel = LoadLevel.Default)
    {
        if (!_loaded)
            Load();

        var dynamicParts = new List<DynamicMeshPart>();
        if (Model != null)
            dynamicParts = dynamicParts.Concat(Model.Load(detailLevel, ModelParent, hasSkeleton: Skeleton != null, loadLevel: loadLevel)).ToList();

        if (PhysicsModel != null)
            dynamicParts = dynamicParts.Concat(PhysicsModel.Load(detailLevel, PhysicsModelParent, hasSkeleton: Skeleton != null, loadLevel: loadLevel)).ToList();

        return dynamicParts;
    }

    /// <summary>
    /// Loads only the normal model into dynamic mesh parts
    /// </summary>
    /// <param name="detailLevel"></param>
    /// <param name="loadLevel"></param>
    /// <returns></returns>
    public List<DynamicMeshPart> LoadModel(ExportDetailLevel detailLevel, LoadLevel loadLevel = LoadLevel.Default)
    {
        if (!_loaded)
            Load();

        var dynamicParts = new List<DynamicMeshPart>();
        if (Model != null)
            dynamicParts = dynamicParts.Concat(Model.Load(detailLevel, ModelParent, hasSkeleton: Skeleton != null, loadLevel: loadLevel)).ToList();

        return dynamicParts;
    }

    /// <summary>
    /// Loads only the physics model into dynamic mesh parts
    /// </summary>
    /// <param name="detailLevel"></param>
    /// <param name="loadLevel"></param>
    /// <returns></returns>
    public List<DynamicMeshPart> LoadPhysicsModel(ExportDetailLevel detailLevel, LoadLevel loadLevel = LoadLevel.Default)
    {
        if (!_loaded)
            Load();

        var dynamicParts = new List<DynamicMeshPart>();
        if (PhysicsModel != null)
            dynamicParts = dynamicParts.Concat(PhysicsModel.Load(detailLevel, PhysicsModelParent, hasSkeleton: Skeleton != null, loadLevel: loadLevel)).ToList();

        return dynamicParts;
    }

    public void SaveMaterialsFromParts(ExporterScene scene, List<DynamicMeshPart> dynamicParts)
    {
        foreach (DynamicMeshPart dynamicPart in dynamicParts)
        {
            if (dynamicPart.Material == null) continue;
            scene.Materials.Add(new ExportMaterial(dynamicPart.Material));
        }
    }

    public void SaveTexturePlates(string saveDirectory)
    {
        if (ModelParent is null)
            return;

        Directory.CreateDirectory($"{saveDirectory}/Textures/");
        var parentResource = (S80806D8F)ModelParent.TagData.Unk18.GetValue(ModelParent.GetReader());

        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_SHADOWKEEP_2601 && parentResource.TexturePlates is null)
            return;

        if (Strategy.IsD1() && (parentResource.TexturePlatesROI.Count == 0 || parentResource.TexturePlatesROI[0].TexturePlates is null))
            return;

        S80806E1C rsrc = Strategy.IsD1() ? parentResource.TexturePlatesROI[0].TexturePlates.TagData : parentResource.TexturePlates.TagData;

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
                Load();
        }

        return ModelParent != null;
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

        List<Entity> entities = new();
        if (CarriedWeapon is not null)
        {
            var weaponEntry = (S808031EA)CarriedWeapon.GetUnk18();
            Log.Debug($"CarriedWeapon {CarriedWeapon.Hash} ({weaponEntry.Unk1C0.Count} entries)");

            var offsetTrans = Vector4.Zero;
            var offsetRot = Vector4.Quaternion;

            if (weaponEntry.Unk220.Any())
            {
                Debug.Assert(weaponEntry.Unk220.Count == 1);

                var offset = weaponEntry.Unk220[0];
                var parentBone = offset.ParentBoneIndex;
                if (parentBone is not -1 && Skeleton is not null)
                {
                    var nodes = Skeleton.GetBoneNodes();
                    if (nodes.Count >= parentBone)
                    {
                        offsetTrans = new(nodes[parentBone].DefaultObjectSpaceTransform.Translation);
                        offsetRot = nodes[parentBone].DefaultObjectSpaceTransform.QuaternionRotation;

                        Quaternion newRot = offsetRot.ToQuat() * offset.Rotation.ToQuat();

                        offsetTrans += new Vector4(offset.Translation.X, offset.Translation.Y, offset.Translation.Z);
                        offsetRot = new Vector4(newRot.X, newRot.Y, newRot.Z, newRot.W);

                        //Log.Debug($"{GlobalStrings.Get().GetString(nodes[parentBone].Hash)}: {offsetTrans} : {offsetRot}");
                    }
                }
            }

            if (weaponEntry.Unk1C0.Any())
            {
                foreach (var entry in weaponEntry.Unk1C0.Select(x => x.Unk30))
                {
                    foreach (var entity in entry.Select(x => x.Entity))
                    {
                        if (entity is null || entities.Contains(entity))
                            continue;

                        entities.Add(entity);

                        entity.Load();
                        if (entity.ModelParent is not null && entity.Model is not null)
                        {
                            entity.Model.TranslationOffset = offsetTrans;
                            entity.Model.RotationOffset = offsetRot;
                        }
                    }
                }
            }

            if (Strategy.IsD1()) // ??
            {
                foreach (var entry in weaponEntry.UnkE8)
                {
                    if (entry.Entity is null || entities.Contains(entry.Entity))
                        continue;

                    entities.Add(entry.Entity);

                    entry.Entity.Load();
                    if (entry.Entity.ModelParent is not null && entry.Entity.Model is not null)
                    {
                        entry.Entity.Model.TranslationOffset = offsetTrans;
                        entry.Entity.Model.RotationOffset = offsetRot;
                    }
                }
            }
        }

        if (Attachments is not null)
        {
            var attachmentEntry = (S80802C28)Attachments.GetUnk18();
            Log.Debug($"Attachments {Attachments.Hash} ({attachmentEntry.Unk1D8.Count} entries)");

            foreach (var entry in attachmentEntry.Unk1D8)
            {
                if (entry.Entity is null || entities.Contains(entry.Entity))
                    continue;

                if (!entry.Transform.Any())
                    continue;

                Debug.Assert(entry.Transform.Count == 1);

                var offsetTrans = Vector4.Zero;
                var offsetRot = Vector4.Quaternion;

                var offset = entry.Transform[0];
                var parentBone = offset.ParentBoneIndex;
                if (parentBone is not -1 && Skeleton is not null)
                {
                    var nodes = Skeleton.GetBoneNodes();
                    if (nodes.Count >= parentBone)
                    {
                        offsetTrans = new(nodes[parentBone].DefaultObjectSpaceTransform.Translation);
                        offsetRot = nodes[parentBone].DefaultObjectSpaceTransform.QuaternionRotation;

                        Quaternion newRot = offsetRot.ToQuat() * offset.Rotation.ToQuat();

                        offsetTrans += new Vector4(offset.Translation.Z, offset.Translation.X, offset.Translation.Y);
                        offsetRot = new Vector4(newRot.X, newRot.Y, newRot.Z, newRot.W);

                        //Log.Debug($"{entry.Entity.Hash}, {GlobalStrings.Get().GetString(nodes[parentBone].Hash)}: {offsetTrans} : {Vector4.QuaternionToEulerAngles(offsetRot)}");
                    }
                }

                entry.Entity.Load();
                if (entry.Entity.ModelParent is not null && entry.Entity.Model is not null)
                {
                    entry.Entity.Model.TranslationOffset = offsetTrans;
                    entry.Entity.Model.RotationOffset = offsetRot;
                }

                entities.Add(entry.Entity);
            }
        }

        if (Sequences is not null)
        {
            foreach (var sequence in Sequences)
            {
                entities.AddRange(sequence.GetSequencerEntities());
            }
        }

        if (EntityChildren is null)
            return entities;

        if (EntityChildren.GetUnk18() is S8080840E children)
        {
            foreach (S8080841B entry in children.Unk88)
            {
                foreach (S8080841D entry2 in entry.Unk08)
                {
                    if (entry2.Entity is null)
                        continue;

                    Entity entity = FileResourcer.Get().GetFile<Entity>(entry2.Entity.Hash);
                    if (entity.HasGeometry())
                    {
                        entities.Add(entity);
                        //Just in case
                        foreach (Entity child in entity.GetEntityChildren())
                            entities.Add(child);
                    }
                }
            }
        }

        return entities;
    }

    public static void Export(List<Entity> entities, string name, string? overridePath = null, EntitySkeleton overrideSkeleton = null)
    {
        ConfigSubsystem config = ConfigSubsystem.Get();
        string sanitizedName = Helpers.SanitizeString(name);
        string savePath = (overridePath is null ? config.GetExportSavePath() : overridePath) + $"/{sanitizedName}";

        Log.Verbose($"Exporting entity model name: {sanitizedName}");

        foreach (Entity entity in entities)
        {
            var scene = Tiger.Exporters.Exporter.Get().CreateScene(entity.Hash, ExportType.Entities);

            if (entity.Skeleton == null && overrideSkeleton != null)
                entity.Skeleton = overrideSkeleton;

            List<DynamicMeshPart> dynamicParts = entity.Load(ExportDetailLevel.MostDetailed);
            List<BoneNode> boneNodes = overrideSkeleton != null ? overrideSkeleton.GetBoneNodes() : new List<BoneNode>();
            if (entity.Skeleton != null && overrideSkeleton == null)
            {
                boneNodes = entity.Skeleton.GetBoneNodes();
            }
            scene.AddEntity(entity, dynamicParts, boneNodes);
            entity.SaveMaterialsFromParts(scene, dynamicParts);
            entity.SaveTexturePlates(savePath);

            Tiger.Exporters.Exporter.Get().Export(savePath ?? null); // 'temp' fix for file in-use crash
        }

        //if (exportType == ExportTypeFlag.Full)
        //{
        //    if (config.GetUnrealInteropEnabled())
        //    {
        //        AutomatedExporter.SaveInteropUnrealPythonFile(savePath, name, AutomatedExporter.ImportType.Entity, config.GetOutputTextureFormat());
        //    }
        //}

        //Tiger.Exporters.Exporter.Get().Export(savePath ?? null);
        Log.Info($"Exported entity model {sanitizedName} to {savePath.Replace('\\', '/')}/");
    }
}
