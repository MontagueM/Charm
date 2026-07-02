using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Entity;

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
