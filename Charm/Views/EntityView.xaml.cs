using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Investment;
using Tiger.Schema.Shaders;

namespace Charm;

public partial class EntityView : UserControl
{
    public FileHash Hash;
    private bool _isEntityModel = false;
    private HelixModelView HelixMV;

    public EntityView()
    {
        InitializeComponent();
    }

    public bool LoadEntity(FileHash entityHash)
    {
        if (_isEntityModel)
            LoadEntityModel(entityHash);

        Log.Info($"Loading Entity {entityHash}");

        Hash = entityHash;
        SetupCheckboxHandlers();

        Entity entity = FileResourcer.Get().GetFile<Entity>(entityHash);

        List<Entity> entities = new() { entity };
        entities.AddRange(entity.GetEntityChildren());

        HelixMV ??= (HelixModelView)ModelView.UCModelView.Resources["HelixMV"];

        HelixMV.Clear();
        List<HelixModelView.DisplayPart> displayParts = ModelView.MakeEntityDisplayParts(entities, ModelView.GetSelectedLod());
        HelixMV.SetChildren(displayParts);
        HelixMV.Title = entityHash;
        HelixMV.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";

        return true;
    }

    public bool LoadEntityModel(FileHash entityModelHash)
    {
        Hash = entityModelHash;
        _isEntityModel = true;
        SetupCheckboxHandlers();

        EntityModel entityModel = FileResourcer.Get().GetFile<EntityModel>(entityModelHash);

        if (HelixMV is null)
            HelixMV = (HelixModelView)ModelView.UCModelView.Resources["HelixMV"];

        HelixMV.Clear();
        List<HelixModelView.DisplayPart> displayParts = ModelView.MakeEntityModelDisplayParts(entityModel, ModelView.GetSelectedLod());
        HelixMV.SetChildren(displayParts);
        HelixMV.Title = entityModelHash;
        HelixMV.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";

        return true;
    }

    public static void Export(List<Entity> entities, string name, string overridePath = null, ExportTypeFlag exportType = ExportTypeFlag.Full, EntitySkeleton overrideSkeleton = null)
    {
        ConfigSubsystem config = ConfigSubsystem.Get();
        name = Helpers.SanitizeString(name);
        string savePath = (overridePath is null ? config.GetExportSavePath() : overridePath) + $"/{name}";

        Log.Verbose($"Exporting entity model name: {name}");

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
            if (exportType == ExportTypeFlag.Full)
            {
                entity.SaveMaterialsFromParts(scene, dynamicParts);
                entity.SaveTexturePlates(savePath);
            }

            Tiger.Exporters.Exporter.Get().Export(savePath ?? null); // 'temp' fix for file in-use crash
        }

        if (exportType == ExportTypeFlag.Full)
        {
            if (config.GetUnrealInteropEnabled())
            {
                AutomatedExporter.SaveInteropUnrealPythonFile(savePath, name, AutomatedExporter.ImportType.Entity, config.GetOutputTextureFormat());
            }
        }

        //Tiger.Exporters.Exporter.Get().Export(savePath ?? null);
        Log.Info($"Exported entity model {name} to {savePath.Replace('\\', '/')}/");
    }

    public static void ExportInventoryItem(InventoryItem item, string savePath, bool aggregateOutput = false)
    {
        // just to be safe, hopefully this doesn't cause issues
        if (item.IsOrnament && item.Parent is null)
            item.Parent = Investment.Get().GetOrnamentParent(item).Result;

        ConfigSubsystem config = ConfigSubsystem.Get();
        string name = item.Name != string.Empty ? Helpers.SanitizeString(item.Name) : $"{item.ApiHash}";
        if (!aggregateOutput)
            savePath = config.GetExportSavePath() + $"/{name}";

        Directory.CreateDirectory(savePath);
        Directory.CreateDirectory($"{savePath}/Textures");
        ExporterScene scene = Tiger.Exporters.Exporter.Get().CreateScene(name, Strategy.IsD1() ? ExportType.D1API : ExportType.API);

        // Dont export gear shader for ghost projections since they dont use it
        if (!item.ItemTraits.Any(x => x == DestinyTraitID.item_ghost_hologram))
            ExportGearShader(item, name, savePath);

        // Export the model
        // todo bad, should be replaced
        EntitySkeleton overrideSkeleton = null;
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            var skeleHash = item.IsGhost ? 0x681CD34630600000 : 0x95952E340F670000;
            Entity skele = FileResourcer.Get().GetFile<Entity>(new FileHash(Hash64Map.Get().GetHash32Checked(skeleHash))); // 64 bit more permanent
            overrideSkeleton = new EntitySkeleton(skele.Skeleton.Hash);
        }
        else if (Strategy.IsD1())
        {
            Entity playerBase = FileResourcer.Get().GetFile<Entity>(new FileHash(0x8184E10A));
            overrideSkeleton = new EntitySkeleton(playerBase.Skeleton.Hash);
        }

        Entity? val = Investment.Get().GetPatternEntityFromHash(item.Parent != null ? item.Parent.TagData.InventoryItemHash : item.TagData.InventoryItemHash);

        Log.Debug($"Pattern Entity {val?.Hash}");

        if (val != null && val.Skeleton != null)
        {
            overrideSkeleton = val.Skeleton;
        }

        List<Entity> entities = Investment.Get().GetEntitiesFromHash(item.TagData.InventoryItemHash);

        Log.Info($"Exporting entity model name: {name}");

        foreach (Entity entity in entities)
        {
            if (entity.Hash.CheckRedacted())
            {
                Log.Warning($"Entity {entity.Hash} is redacted, can not export.");
                continue;
            }
            Log.Debug($"Entity {entity?.Hash}: HasGeometry {entity?.HasGeometry()}");

            // ghost projections have just a rectangle mesh, we want just the actual projection mesh 
            if (item.ItemTraits.Any(x => x == DestinyTraitID.item_ghost_hologram))
            {
                ExportGhostProjection(entity, scene);
                continue;
            }

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
        }

        if (!aggregateOutput)
            Tiger.Exporters.Exporter.Get().Export();
        else
            Tiger.Exporters.Exporter.Get().Export(savePath);

        Log.Info($"Exported entity model {name} to {savePath.Replace('\\', '/')}/");
    }

    // todo, make more generic for entities
    public static void ExportGhostProjection(Entity entity, ExporterScene scene)
    {
        foreach (FileHash hash in entity.Components)
        {
            if (Strategy.IsD1() && hash.GetReferenceHash() != 0x80800861)
                continue;

            EntityComponent resource = FileResourcer.Get().GetFile<EntityComponent>(hash);
            if (resource.TagData.Unk18.GetValue(resource.GetReader()) is S80808179 sequencer)
            {
                // only in Array2 afaik
                foreach (S808091F1 element in sequencer.Array1)
                {
                    Debug.Assert(element.Unk10.GetValue(resource.GetReader()) is not SSequenceParticleSystem);
                }

                List<Tag<SParticleSystem>> particles = new();
                foreach (S808091F1 element in sequencer.Array2)
                {
                    if (element.Unk10.GetValue(resource.GetReader()) is SSequenceParticleSystem particle)
                    {
                        foreach (var entry in particle.Unk28.Select(x => x.ParticleSystem).Where(x => x is not null))
                        {
                            if (entry.TagData.ModelContainer is null)
                                continue;

                            particles.Add(entry);
                        }
                    }
                }

                if (!particles.Any())
                    return;

                // I *think* the last entry is the one used in the inspection screen? All the others have slightly different pixel shaders
                var last = particles.Where(x => x.TagData.ModelContainer is not null).Last();
                var container = last.TagData.ModelContainer;

                Material overrideMat = null;
                if (last.TagData.UnkMat is not null)
                {
                    overrideMat = last.TagData.UnkMat;
                    scene.Materials.Add(new ExportMaterial(overrideMat));
                }

                // Unsure if theres only ever 1 model here
                foreach (var model in container.TagData.Models.Enumerate(container.GetReader()).Where(x => x.Model is not null))
                {
                    if (scene.Entities.Any(x => x.Mesh.Hash == model.Model.Hash))
                        continue;

                    scene.AddModel(model.Model, overrideMat);
                }
            }
        }
    }

    // I don't like this
    public static void ExportGearShader(InventoryItem item, string itemName, string savePath)
    {
        var config = ConfigSubsystem.Get();

        Log.Info($"Exporting Gear Shader for: {item.Name}");
        // Export the dye info
        if (Strategy.IsD1())
        {
            Dictionary<TigerHash, DyeD1> dyes = new();
            if (item.TagData.Unk90.GetValue(item.GetReader()) is S80807377 translationBlock)
            {
                foreach (S8080737B dyeEntry in translationBlock.DefaultDyes)
                {
                    DyeD1 dye = Investment.Get().GetD1DyeFromIndex(dyeEntry.GetDyeIndex());
                    if (dye != null)
                    {
                        dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()), dye);
                        dye.ExportTextures($"{savePath}/Textures", config.GetOutputTextureFormat());
                    }
                }
                foreach (S8080737B dyeEntry in translationBlock.LockedDyes)
                {
                    DyeD1 dye = Investment.Get().GetD1DyeFromIndex(dyeEntry.GetDyeIndex());
                    if (dye != null)
                    {
                        dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()), dye);
                        dye.ExportTextures($"{savePath}/Textures", config.GetOutputTextureFormat());
                    }
                }
            }
            AutomatedExporter.SaveD1ShaderInfo(savePath, itemName, config.GetOutputTextureFormat(), dyes.Values.ToList());
        }
        else
        {
            Dictionary<TigerHash, Dye> dyes = new();
            if (item.TagData.Unk90.GetValue(item.GetReader()) is S80807377 translationBlock)
            {
                foreach (S8080737B dyeEntry in translationBlock.DefaultDyes)
                {
                    Dye dye = Investment.Get().GetDyeFromIndex(dyeEntry.GetDyeIndex());
                    if (dye is null)
                        continue;
                    dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()), dye);
#if DEBUG
                    System.Console.WriteLine($"{item.Name}: DefaultDye {dye.Hash} - {Dye.GetChannelName(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()))}");
#endif
                }
                foreach (S8080737B dyeEntry in translationBlock.LockedDyes)
                {
                    Dye dye = Investment.Get().GetDyeFromIndex(dyeEntry.GetDyeIndex());
                    if (dye is null)
                        continue;
                    dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()), dye);
#if DEBUG
                    System.Console.WriteLine($"{item.Name}: LockedDye {dye.Hash} - {Dye.GetChannelName(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()))}");
#endif
                }
            }

            AutomatedExporter.SaveBlenderApiFile(savePath, itemName,
                config.GetOutputTextureFormat(), dyes.Values.ToList());

            Texture iridesceneLookup = Globals.Get().RenderGlobals.TagData.Textures.TagData.IridescenceLookup;
            TextureExporter.SaveTextureToFile($"{savePath}/Textures/Iridescence_Lookup", iridesceneLookup.GetScratchImage());
        }
        Log.Info($"Exported Gear Shader for: {item.Name}");
    }



    private void SetupCheckboxHandlers()
    {
        ModelView.TextureCheckBox.Visibility = Visibility.Visible;
        ModelView.SkeletonCheckBox.Visibility = Visibility.Visible;

        // Detach first to prevent multiple subscriptions
        ModelView.TextureCheckBox.Checked -= ReloadEntity;
        ModelView.TextureCheckBox.Unchecked -= ReloadEntity;

        ModelView.SkeletonCheckBox.Checked -= ReloadEntity;
        ModelView.SkeletonCheckBox.Unchecked -= ReloadEntity;

        ModelView.TextureCheckBox.Checked += ReloadEntity;
        ModelView.TextureCheckBox.Unchecked += ReloadEntity;

        ModelView.SkeletonCheckBox.Checked += ReloadEntity;
        ModelView.SkeletonCheckBox.Unchecked += ReloadEntity;
    }

    private void ReloadEntity(object sender, RoutedEventArgs e) =>
        LoadEntity(Hash);
}
