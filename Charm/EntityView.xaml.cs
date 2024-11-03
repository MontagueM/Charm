using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Investment;

namespace Charm;

public partial class EntityView : UserControl
{
    public FileHash Hash;
    private string Name;

    public EntityView()
    {
        InitializeComponent();
    }

    public bool LoadEntity(FileHash entityHash, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        Entity entity = FileResourcer.Get().GetFile<Entity>(entityHash);

        // HelixToolkit is throwing a huge fit about a lot of D1 entities and just crashes when trying to load from fbx :))))))
        // Only option is to load them as display parts instead. Won't show skeleton and won't look as nice but no other choice.
        // On the plus side, things load slightly faster.
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            var parts = MakeEntityDisplayParts(entity, ExportDetailLevel.MostDetailed);
            Dispatcher.Invoke(() =>
            {
                MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
                MVM.Clear();
                MVM.SetChildren(parts);
            });
            parts.Clear();
            return true;
        }
        else
        {
            List<Entity> entities = new List<Entity> { entity };
            entities.AddRange(entity.GetEntityChildren());
            entities.ForEach(entity =>
            {
                AddEntity(entity, ModelView.GetSelectedLod(), fbxHandler);
            });
            return LoadUI(fbxHandler);
        }

    }

    public bool LoadEntityModel(FileHash entityModelHash, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        EntityModel entityModel = FileResourcer.Get().GetFile<EntityModel>(entityModelHash);

        var dynamicParts = entityModel.Load(ModelView.GetSelectedLod(), null);
        ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        if (ModelView.GetSelectedGroupIndex() != -1)
            dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();

        for (int i = 0; i < dynamicParts.Count; i++)
        {
            var dynamicPart = dynamicParts[i];
            fbxHandler.AddMeshPartToScene(dynamicPart, i, entityModelHash);
        }

        return LoadUI(fbxHandler);
    }

    public async void LoadEntityFromApi(TigerHash apiHash, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        List<Entity> entities = Investment.Get().GetEntitiesFromHash(apiHash);
        foreach (var entity in entities)
        {
            // todo find out why sometimes this is null
            if (entity == null)
            {
                continue;
            }
            AddEntity(entity, ModelView.GetSelectedLod(), fbxHandler);
        }
        LoadUI(fbxHandler);
    }

    private void AddEntity(Entity entity, ExportDetailLevel detailLevel, FbxHandler fbxHandler)
    {
        var dynamicParts = entity.Load(detailLevel);
        ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        if (ModelView.GetSelectedGroupIndex() != -1)
            dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();
        fbxHandler.AddEntityToScene(entity, dynamicParts, detailLevel);
        Log.Verbose($"Adding entity {entity.Hash}/{entity.Model?.Hash} with {dynamicParts.Sum(p => p.Indices.Count)} vertices to fbx");
    }

    private bool LoadUI(FbxHandler fbxHandler)
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        string filePath = $"{config.GetExportSavePath()}/temp.fbx";
        fbxHandler.ExportScene(filePath);
        bool loaded = MVM.LoadEntityFromFbx(filePath);
        fbxHandler.Clear();
        return loaded;
    }

    public static void Export(List<Entity> entities, string name, ExportTypeFlag exportType, EntitySkeleton overrideSkeleton = null, ExporterScene scene = null)
    {
        ConfigSubsystem config = ConfigSubsystem.Get();
        name = Helpers.SanitizeString(name);
        string savePath = config.GetExportSavePath() + $"/{name}";

        if (scene == null)
            scene = Tiger.Exporters.Exporter.Get().CreateScene(name, ExportType.Entity);

        Log.Verbose($"Exporting entity model name: {name}");

        foreach (var entity in entities)
        {
            if (entity.Skeleton == null && overrideSkeleton != null)
                entity.Skeleton = overrideSkeleton;

            var dynamicParts = entity.Load(ExportDetailLevel.MostDetailed);
            List<BoneNode> boneNodes = overrideSkeleton != null ? overrideSkeleton.GetBoneNodes() : new List<BoneNode>();
            if (entity.Skeleton != null && overrideSkeleton == null)
            {
                boneNodes = entity.Skeleton.GetBoneNodes();
            }
            scene.AddEntity(entity.Hash, dynamicParts, boneNodes, entity.Gender);
            if (exportType == ExportTypeFlag.Full)
            {
                entity.SaveMaterialsFromParts(scene, dynamicParts);
                entity.SaveTexturePlates(savePath);
            }
        }

        if (exportType == ExportTypeFlag.Full)
        {
            if (config.GetUnrealInteropEnabled())
            {
                AutomatedExporter.SaveInteropUnrealPythonFile(savePath, name, AutomatedExporter.ImportType.Entity, config.GetOutputTextureFormat());
            }
        }

        // Scale and rotate
        // fbxHandler.ScaleAndRotateForBlender(boneNodes[0]);
        Tiger.Exporters.Exporter.Get().Export();
        Log.Info($"Exported entity model {name} to {savePath.Replace('\\', '/')}/");
    }

    public static void ExportInventoryItem(ApiItem item, string savePath, bool aggregateOutput = false)
    {
        ConfigSubsystem config = ConfigSubsystem.Get();
        string name = Helpers.SanitizeString(item.ItemName);
        if (!aggregateOutput)
            savePath = config.GetExportSavePath() + $"/{name}";

        var scene = Tiger.Exporters.Exporter.Get().CreateScene(name, Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON ? ExportType.D1API : ExportType.API);

        // Export the model
        // todo bad, should be replaced
        EntitySkeleton overrideSkeleton = null;
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            string skeleHash = item.ItemType == "Ghost Shell" ? "0000603046D31C68" : "0000670F342E9595";
            Entity skele = FileResourcer.Get().GetFile<Entity>(new FileHash(Hash64Map.Get().GetHash32Checked(skeleHash))); // 64 bit more permanent
            overrideSkeleton = new EntitySkeleton(skele.Skeleton.Hash);
        }
        else if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            Entity playerBase = FileResourcer.Get().GetFile<Entity>(new FileHash("0AE18481"));
            overrideSkeleton = new EntitySkeleton(playerBase.Skeleton.Hash);
        }



        var val = Investment.Get().GetPatternEntityFromHash(item.Parent != null ? item.Parent.TagData.InventoryItemHash : item.Item.TagData.InventoryItemHash);
        if (val != null && val.Skeleton != null)
        {
            overrideSkeleton = val.Skeleton;
        }

        var entities = Investment.Get().GetEntitiesFromHash(item.Item.TagData.InventoryItemHash);

        Log.Verbose($"Exporting entity model name: {name}");

        foreach (var entity in entities)
        {
            if (entity.Skeleton == null && overrideSkeleton != null)
                entity.Skeleton = overrideSkeleton;

            var dynamicParts = entity.Load(ExportDetailLevel.MostDetailed);
            List<BoneNode> boneNodes = overrideSkeleton != null ? overrideSkeleton.GetBoneNodes() : new List<BoneNode>();
            if (entity.Skeleton != null && overrideSkeleton == null)
            {
                boneNodes = entity.Skeleton.GetBoneNodes();
            }
            scene.AddEntity(entity.Hash, dynamicParts, boneNodes, entity.Gender);
            entity.SaveMaterialsFromParts(scene, dynamicParts);
            entity.SaveTexturePlates(savePath);
        }

        //if (exportType == ExportTypeFlag.Full)
        //{
        //    if (config.GetUnrealInteropEnabled())
        //    {
        //        AutomatedExporter.SaveInteropUnrealPythonFile(savePath, name, AutomatedExporter.ImportType.Entity, config.GetOutputTextureFormat());
        //    }
        //}

        // Scale and rotate
        // fbxHandler.ScaleAndRotateForBlender(boneNodes[0]);
        if (!aggregateOutput)
            Tiger.Exporters.Exporter.Get().Export();
        else
            Tiger.Exporters.Exporter.Get().Export(savePath);

        Log.Info($"Exported entity model {name} to {savePath.Replace('\\', '/')}/");

        Directory.CreateDirectory(savePath);
        ExportGearShader(item, name, savePath);
    }

    // I don't like this
    public static void ExportGearShader(ApiItem item, string itemName, string savePath)
    {
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();

        // Export the dye info
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            Dictionary<TigerHash, DyeD1> dyes = new Dictionary<TigerHash, DyeD1>();
            if (item.Item.TagData.Unk90.GetValue(item.Item.GetReader()) is D2Class_77738080 translationBlock)
            {
                foreach (var dyeEntry in translationBlock.DefaultDyes)
                {
                    DyeD1 dye = Investment.Get().GetD1DyeFromIndex(dyeEntry.DyeIndex);
                    if (dye != null)
                    {
                        dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.ChannelIndex), dye);
                        dye.ExportTextures($"{savePath}/Textures", config.GetOutputTextureFormat());
                    }
                }
                foreach (var dyeEntry in translationBlock.LockedDyes)
                {
                    DyeD1 dye = Investment.Get().GetD1DyeFromIndex(dyeEntry.DyeIndex);
                    if (dye != null)
                    {
                        dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.ChannelIndex), dye);
                        dye.ExportTextures($"{savePath}/Textures", config.GetOutputTextureFormat());
                    }
                }
            }
            AutomatedExporter.SaveD1ShaderInfo(savePath, itemName, config.GetOutputTextureFormat(), dyes.Values.ToList());
        }
        else
        {
            Dictionary<TigerHash, Dye> dyes = new Dictionary<TigerHash, Dye>();
            if (item.Item.TagData.Unk90.GetValue(item.Item.GetReader()) is D2Class_77738080 translationBlock)
            {
                foreach (var dyeEntry in translationBlock.DefaultDyes)
                {
                    Dye dye = Investment.Get().GetDyeFromIndex(dyeEntry.DyeIndex);
                    dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.ChannelIndex), dye);
#if DEBUG
                    System.Console.WriteLine($"{item.ItemName}: DefaultDye {dye.Hash}");
#endif
                }
                foreach (var dyeEntry in translationBlock.LockedDyes)
                {
                    Dye dye = Investment.Get().GetDyeFromIndex(dyeEntry.DyeIndex);
                    dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.ChannelIndex), dye);
#if DEBUG
                    System.Console.WriteLine($"{item.ItemName}: LockedDye {dye.Hash}");
#endif
                }
            }

            AutomatedExporter.SaveBlenderApiFile(savePath, itemName,
                config.GetOutputTextureFormat(), dyes.Values.ToList());

            var iridesceneLookup = Globals.Get().RenderGlobals.TagData.Textures.TagData.IridescenceLookup;
            TextureExtractor.SaveTextureToFile($"{savePath}/Textures/Iridescence_Lookup", iridesceneLookup.GetScratchImage());
        }
    }

    private List<MainViewModel.DisplayPart> MakeEntityDisplayParts(Entity entity, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();
        List<Entity> entities = new List<Entity> { entity };
        entities.AddRange(entity.GetEntityChildren());

        foreach (var ent in entities)
        {
            if (ent.HasGeometry())
            {
                var dynamicParts = ent.Load(detailLevel);
                ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
                if (ModelView.GetSelectedGroupIndex() != -1)
                    dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();

                foreach (var part in dynamicParts)
                {
                    MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
                    displayPart.BasePart = part;
                    displayPart.Translations.Add(Vector3.Zero);
                    displayPart.Rotations.Add(Vector4.Zero);
                    displayPart.Scales.Add(Vector3.One);
                    displayParts.Add(displayPart);
                }
            }
        }

        return displayParts.ToList();
    }
}
