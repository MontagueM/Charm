using System.Collections.Concurrent;
using System.Collections.Generic;
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

namespace Charm;

public partial class EntityView : UserControl
{
    public FileHash Hash;
    private MainViewModel MVM;

    public EntityView()
    {
        InitializeComponent();
    }

    public bool LoadEntity(FileHash entityHash)
    {
        Hash = entityHash;
        SetupCheckboxHandlers();

        Entity entity = FileResourcer.Get().GetFile<Entity>(entityHash);

        List<Entity> entities = new() { entity };
        entities.AddRange(entity.GetEntityChildren());

        MVM ??= (MainViewModel)ModelView.UCModelView.Resources["MVM"];

        MVM.Clear();
        List<MainViewModel.DisplayPart> displayParts = MakeEntityDisplayParts(entities, ModelView.GetSelectedLod());
        MVM.SetChildren(displayParts);
        MVM.Title = entityHash;
        MVM.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";

        return true;
    }

    public bool LoadEntityModel(FileHash entityModelHash)
    {
        Hash = entityModelHash;
        SetupCheckboxHandlers();

        EntityModel entityModel = FileResourcer.Get().GetFile<EntityModel>(entityModelHash);

        if (MVM is null)
            MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];

        MVM.Clear();
        List<MainViewModel.DisplayPart> displayParts = MakeEntityModelDisplayParts(entityModel, ModelView.GetSelectedLod());
        MVM.SetChildren(displayParts);
        MVM.Title = entityModelHash;
        MVM.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";

        return true;
    }

    public async void LoadEntityFromApi(TigerHash apiHash, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        List<Entity> entities = Investment.Get().GetEntitiesFromHash(apiHash);
        foreach (Entity entity in entities)
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
        List<DynamicMeshPart> dynamicParts = entity.Load(detailLevel);
        ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        if (ModelView.GetSelectedGroupIndex() != -1)
            dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();
        fbxHandler.AddEntityToScene(entity, dynamicParts, detailLevel);
        Log.Verbose($"Adding entity {entity.Hash}/{entity.Model?.Hash} with {dynamicParts.Sum(p => p.Indices.Count)} vertices to fbx");
    }

    private bool LoadUI(FbxHandler fbxHandler)
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
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
            scene = Tiger.Exporters.Exporter.Get().CreateScene(name, ExportType.Entities);

        Log.Verbose($"Exporting entity model name: {name}");

        foreach (Entity entity in entities)
        {
            if (entity.Skeleton == null && overrideSkeleton != null)
                entity.Skeleton = overrideSkeleton;

            List<DynamicMeshPart> dynamicParts = entity.Load(ExportDetailLevel.MostDetailed);
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

    public static void ExportInventoryItem(DareItem item, string savePath, bool aggregateOutput = false)
    {
        ConfigSubsystem config = ConfigSubsystem.Get();
        string name = Helpers.SanitizeString(item.ItemName);
        if (!aggregateOutput)
            savePath = config.GetExportSavePath() + $"/{name}";

        Directory.CreateDirectory(savePath);
        Directory.CreateDirectory($"{savePath}/Textures");
        ExporterScene scene = Tiger.Exporters.Exporter.Get().CreateScene(name, Strategy.IsD1() ? ExportType.D1API : ExportType.API);

        ExportGearShader(item, name, savePath);

        // Export the model
        // todo bad, should be replaced
        EntitySkeleton overrideSkeleton = null;
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            string skeleHash = item.ItemType == "Ghost Shell" ? "0000603046D31C68" : "0000670F342E9595";
            Entity skele = FileResourcer.Get().GetFile<Entity>(new FileHash(Hash64Map.Get().GetHash32Checked(skeleHash))); // 64 bit more permanent
            overrideSkeleton = new EntitySkeleton(skele.Skeleton.Hash);
        }
        else if (Strategy.IsD1())
        {
            Entity playerBase = FileResourcer.Get().GetFile<Entity>(new FileHash("0AE18481"));
            overrideSkeleton = new EntitySkeleton(playerBase.Skeleton.Hash);
        }

        Entity? val = Investment.Get().GetPatternEntityFromHash(item.Parent != null ? item.Parent.TagData.InventoryItemHash : item.Item.TagData.InventoryItemHash);
        if (val != null && val.Skeleton != null)
        {
            overrideSkeleton = val.Skeleton;
        }

        List<Entity> entities = Investment.Get().GetEntitiesFromHash(item.Item.TagData.InventoryItemHash);

        Log.Info($"Exporting entity model name: {name}");

        foreach (Entity entity in entities)
        {
            if (entity.Skeleton == null && overrideSkeleton != null)
                entity.Skeleton = overrideSkeleton;

            List<DynamicMeshPart> dynamicParts = entity.Load(ExportDetailLevel.MostDetailed);
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
    }

    // I don't like this
    public static void ExportGearShader(DareItem item, string itemName, string savePath)
    {
        var config = ConfigSubsystem.Get();

        Log.Info($"Exporting Gear Shader for: {item.ItemName}");
        // Export the dye info
        if (Strategy.IsD1())
        {
            Dictionary<TigerHash, DyeD1> dyes = new();
            if (item.Item.TagData.Unk90.GetValue(item.Item.GetReader()) is S77738080 translationBlock)
            {
                foreach (S7B738080 dyeEntry in translationBlock.DefaultDyes)
                {
                    DyeD1 dye = Investment.Get().GetD1DyeFromIndex(dyeEntry.DyeIndex);
                    if (dye != null)
                    {
                        dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.ChannelIndex), dye);
                        dye.ExportTextures($"{savePath}/Textures", config.GetOutputTextureFormat());
                    }
                }
                foreach (S7B738080 dyeEntry in translationBlock.LockedDyes)
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
            Dictionary<TigerHash, Dye> dyes = new();
            if (item.Item.TagData.Unk90.GetValue(item.Item.GetReader()) is S77738080 translationBlock)
            {
                foreach (S7B738080 dyeEntry in translationBlock.DefaultDyes)
                {
                    Dye dye = Investment.Get().GetDyeFromIndex(dyeEntry.DyeIndex);
                    dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.ChannelIndex), dye);
#if DEBUG
                    System.Console.WriteLine($"{item.ItemName}: DefaultDye {dye.Hash}");
#endif
                }
                foreach (S7B738080 dyeEntry in translationBlock.LockedDyes)
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

            Texture iridesceneLookup = Globals.Get().RenderGlobals.TagData.Textures.TagData.IridescenceLookup;
            TextureExtractor.SaveTextureToFile($"{savePath}/Textures/Iridescence_Lookup", iridesceneLookup.GetScratchImage());
        }
        Log.Info($"Exported Gear Shader for: {item.ItemName}");
    }

    private List<MainViewModel.DisplayPart> MakeEntityDisplayParts(List<Entity> entities, ExportDetailLevel detailLevel)
    {
        bool useTextures = ModelView.TextureCheckBox.IsChecked == true;

        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new();
        foreach (Entity ent in entities)
        {
            if (ent.HasGeometry())
            {
                List<DynamicMeshPart> dynamicParts = ent.Load(detailLevel);
                ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
                if (ModelView.GetSelectedGroupIndex() != -1)
                    dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();

                foreach (DynamicMeshPart part in dynamicParts)
                {
                    MainViewModel.DisplayPart displayPart = new();
                    displayPart.BasePart = part;
                    displayPart.Translations.Add(Vector3.Zero);
                    displayPart.Rotations.Add(Vector4.Zero);
                    displayPart.Scales.Add(Vector3.One);

                    if (useTextures && part.Material?.Pixel.Textures.Any() == true)
                    {
                        Stream texture = TextureView.RemoveAlpha(part.Material.Pixel.Textures[0].Texture.GetTexture());
                        displayPart.DiffuseMaterial = new()
                        {
                            DiffuseMap = new HelixToolkit.SharpDX.Core.TextureModel(texture, true),
                        };
                    }

                    displayParts.Add(displayPart);
                }
            }

            if (ent.Skeleton != null)
            {
                MainViewModel.DisplayPart displayPart = new();
                displayPart.BoneNodes = ent.Skeleton.GetBoneNodes();
                displayPart.Translations.Add(Vector3.Zero);
                displayPart.Rotations.Add(Vector4.Zero);
                displayPart.Scales.Add(Vector3.One);

                displayParts.Add(displayPart);
            }
        }

        return displayParts.ToList();
    }

    // TODO combine with above, I don't like this
    private List<MainViewModel.DisplayPart> MakeEntityModelDisplayParts(EntityModel entModel, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new();

        List<DynamicMeshPart> dynamicParts = entModel.Load(detailLevel, null);
        ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        if (ModelView.GetSelectedGroupIndex() != -1)
            dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();

        foreach (DynamicMeshPart part in dynamicParts)
        {
            MainViewModel.DisplayPart displayPart = new();
            displayPart.BasePart = part;
            displayPart.Translations.Add(Vector3.Zero);
            displayPart.Rotations.Add(Vector4.Zero);
            displayPart.Scales.Add(Vector3.One);

            displayParts.Add(displayPart);
        }

        return displayParts.ToList();
    }

    private void SetupCheckboxHandlers()
    {
        ModelView.TextureCheckBox.Visibility = Visibility.Visible;

        // Detach first to prevent multiple subscriptions
        ModelView.TextureCheckBox.Checked -= TextureCheckBox_Checked;
        ModelView.TextureCheckBox.Unchecked -= TextureCheckBox_Unchecked;

        ModelView.TextureCheckBox.Checked += TextureCheckBox_Checked;
        ModelView.TextureCheckBox.Unchecked += TextureCheckBox_Unchecked;
    }

    private void TextureCheckBox_Checked(object sender, RoutedEventArgs e) =>
        LoadEntity(Hash);

    private void TextureCheckBox_Unchecked(object sender, RoutedEventArgs e) =>
        LoadEntity(Hash);
}
