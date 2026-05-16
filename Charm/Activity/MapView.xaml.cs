using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Entity;

namespace Charm;

public partial class MapView : UserControl
{
    private HelixModelView HelixMV;
    private static ConfigSubsystem _config = TigerInstance.GetSubsystem<ConfigSubsystem>();

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        ModelView.LodCombobox.SelectedIndex = 1; // default to least detail for static maps
    }

    public MapView()
    {
        InitializeComponent();
    }

    public void LoadMap(FileHash fileHash, ExportDetailLevel detailLevel, bool isEntities = false)
    {
        if (isEntities)
            GetEntityMapData(fileHash, detailLevel);
        else
            GetStaticMapData(fileHash, detailLevel);
    }

    private void GetEntityMapData(FileHash tagHash, ExportDetailLevel detailLevel)
    {
        SetEntityMapUI(tagHash, detailLevel);
    }

    private void GetStaticMapData(FileHash fileHash, ExportDetailLevel detailLevel)
    {
        Tag<SMapContainer> tag = FileResourcer.Get().GetSchemaTag<SMapContainer>(fileHash);
        foreach (SMapDataTableEntry tables in tag.TagData.MapDataTables)
        {
            foreach (SMapDataEntry entry in tables.MapDataTable.TagData.DataEntries)
            {
                if (entry.DataResource.GetValue(tables.MapDataTable.GetReader()) is SStaticMapDataResource resource)
                {
                    resource.StaticMapParent?.Load();
                    if (resource.StaticMapParent is null || resource.StaticMapParent.TagData.StaticMap is null)
                        continue;

                    StaticMapData staticMapData = resource.StaticMapParent.TagData.StaticMap;
                    SetMapUI(staticMapData, detailLevel);
                }
                if (entry.DataResource.GetValue(tables.MapDataTable.GetReader()) is SMapTerrainResource terrain)
                {
                    terrain.Terrain?.Load();
                    if (terrain.Terrain is null)
                        continue;

                    SetTerrainMapUI(terrain.Terrain, detailLevel);
                }
            }
        }
    }

    private void SetMapUI(StaticMapData staticMapData, ExportDetailLevel detailLevel)
    {
        List<HelixModelView.DisplayPart> displayParts = ModelView.MakeStaticMapDisplayParts(staticMapData, detailLevel);
        Dispatcher.Invoke(() =>
        {
            HelixModelView HelixMV = (HelixModelView)ModelView.UCModelView.Resources["HelixMV"];
            HelixMV.SetChildren(displayParts);
            HelixMV.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";
        });
        displayParts.Clear();
    }

    private void SetEntityMapUI(FileHash dataentry, ExportDetailLevel detailLevel)
    {
        List<HelixModelView.DisplayPart> displayParts = ModelView.MakeMapDataTableDisplayParts(dataentry, detailLevel);
        Dispatcher.Invoke(() =>
        {
            HelixModelView HelixMV = (HelixModelView)ModelView.UCModelView.Resources["HelixMV"];
            HelixMV.SetChildren(displayParts);
        });
        displayParts.Clear();
    }

    private void SetTerrainMapUI(Terrain terrain, ExportDetailLevel detailLevel)
    {
        List<HelixModelView.DisplayPart> displayParts = ModelView.MakeTerrainDisplayParts(terrain, detailLevel);
        Dispatcher.Invoke(() =>
        {
            HelixModelView HelixMV = (HelixModelView)ModelView.UCModelView.Resources["HelixMV"];
            HelixMV.SetChildren(displayParts);
        });
        displayParts.Clear();
    }

    public void LoadEntity(FileHash entityHash)
    {
        Log.Info($"Loading Entity {entityHash}");

        //Hash = entityHash;
        //SetupCheckboxHandlers();

        Entity entity = FileResourcer.Get().GetFile<Entity>(entityHash);

        List<Entity> entities = new() { entity };
        entities.AddRange(entity.GetEntityChildren());

        Dispatcher.Invoke(() =>
        {
            HelixMV ??= (HelixModelView)ModelView.UCModelView.Resources["HelixMV"];

            HelixMV.Clear();
            List<HelixModelView.DisplayPart> displayParts = ModelView.MakeEntityDisplayParts(entities, ModelView.GetSelectedLod());
            HelixMV.SetChildren(displayParts);
            HelixMV.Title = entityHash;
            HelixMV.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";
        });
    }

    public void Clear()
    {
        HelixModelView HelixMV = (HelixModelView)ModelView.UCModelView.Resources["HelixMV"];
        HelixMV.Clear();
    }

    public void Dispose()
    {
        HelixModelView HelixMV = (HelixModelView)ModelView.UCModelView.Resources["HelixMV"];
        HelixMV.Dispose();
    }

    public static void ExportFullMap(Tag<SMapContainer> map, string savePath)
    {
        Directory.CreateDirectory(savePath);

        ExtractDataTables(map, savePath);

        if (_config.GetUnrealInteropEnabled())
        {
            AutomatedExporter.SaveInteropUnrealPythonFile(savePath, map.Hash.ToString(), AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat(), _config.GetSingleFolderMapAssetsEnabled());
        }
    }

    private static void ExtractDataTables(Tag<SMapContainer> map, string savePath)
    {
        ExporterScene staticsScene = Exporter.Get().CreateScene(map.Hash.ToString(), ExportType.Statics, DataExportType.Map);
        ExporterScene terrainScene = Exporter.Get().CreateScene($"{map.Hash}_Terrain", ExportType.Terrain, DataExportType.Map);

        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            if (Strategy.IsD1())
            {
                if (data.MapDataTable.TagData.DataEntries[0].DataResource.GetValue(data.MapDataTable.GetReader()) is SStaticMapDataResource staticMapResource)
                {
                    staticMapResource.StaticMapParent?.Load();
                    if (staticMapResource.StaticMapParent is null)
                        return;

                    staticMapResource.StaticMapParent.TagData.StaticMap.LoadDecalsIntoExporterScene(staticsScene);
                }
            }

            data.MapDataTable.TagData.DataEntries.ForEach(entry =>
            {
                switch (entry.DataResource.GetValue(data.MapDataTable.GetReader()))
                {
                    case SStaticMapDataResource staticMapResource:
                        staticMapResource.StaticMapParent?.Load();
                        if (staticMapResource.StaticMapParent is null)
                            return;

                        staticMapResource.StaticMapParent.TagData.StaticMap.LoadIntoExporterScene(staticsScene);
                        break;

                    case SStaticAOResource AO:
                        Exporter.Get().GetOrCreateGlobalScene().AddToGlobalScene(AO, true);
                        break;

                    case SMapTerrainResource terrain:
                        terrain.Terrain?.Load();
                        if (terrain.Terrain is null)
                            return;

                        terrain.Terrain.LoadIntoExporter(terrainScene, savePath, terrain.Identifier);
                        break;
                    default:
                        break;
                }
            });
        });
    }

    //private void SetupCheckboxHandlers()
    //{
    //    ModelView.TextureCheckBox.Visibility = Visibility.Visible;
    //    ModelView.SkeletonCheckBox.Visibility = Visibility.Visible;

    //    // Detach first to prevent multiple subscriptions
    //    ModelView.TextureCheckBox.Checked -= ReloadEntity;
    //    ModelView.TextureCheckBox.Unchecked -= ReloadEntity;

    //    ModelView.SkeletonCheckBox.Checked -= ReloadEntity;
    //    ModelView.SkeletonCheckBox.Unchecked -= ReloadEntity;

    //    ModelView.TextureCheckBox.Checked += ReloadEntity;
    //    ModelView.TextureCheckBox.Unchecked += ReloadEntity;

    //    ModelView.SkeletonCheckBox.Checked += ReloadEntity;
    //    ModelView.SkeletonCheckBox.Unchecked += ReloadEntity;
    //}

    //private void ReloadEntity(object sender, RoutedEventArgs e) =>
    //    LoadEntity(Hash);
}
