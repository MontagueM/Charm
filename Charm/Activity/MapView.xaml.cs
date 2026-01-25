using System.Collections.Concurrent;
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
using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;
using Tiger.Schema.Entity;
using Tiger.Schema.Static;

namespace Charm;

public partial class MapView : UserControl
{
    // public StaticMapData StaticMap;
    // public FileHash Hash;

    private static MainWindow _mainWindow = null;

    private static ConfigSubsystem _config = TigerInstance.GetSubsystem<ConfigSubsystem>();

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        ModelView.LodCombobox.SelectedIndex = 1; // default to least detail
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
                if (entry.DataResource.GetValue(tables.MapDataTable.GetReader()) is SMapDataResource resource)
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
        List<MainViewModel.DisplayPart> displayParts = MakeDisplayParts(staticMapData, detailLevel);
        Dispatcher.Invoke(() =>
        {
            MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
            MVM.SetChildren(displayParts);
            MVM.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";
        });
        displayParts.Clear();
    }

    private void SetEntityMapUI(FileHash dataentry, ExportDetailLevel detailLevel)
    {
        List<MainViewModel.DisplayPart> displayParts = MakeEntityDisplayParts(dataentry, detailLevel);
        Dispatcher.Invoke(() =>
        {
            MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
            MVM.SetChildren(displayParts);
        });
        displayParts.Clear();
    }

    private void SetTerrainMapUI(Terrain terrain, ExportDetailLevel detailLevel)
    {
        List<MainViewModel.DisplayPart> displayParts = MakeTerrainDisplayParts(terrain, detailLevel);
        Dispatcher.Invoke(() =>
        {
            MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
            MVM.SetChildren(displayParts);
        });
        displayParts.Clear();
    }

    public bool LoadEntity(List<Entity> entities, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        foreach (Entity entity in entities)
            AddEntity(entity, ExportDetailLevel.MostDetailed, fbxHandler);
        return LoadUI(fbxHandler);
    }

    private void AddEntity(Entity entity, ExportDetailLevel detailLevel, FbxHandler fbxHandler)
    {
        List<DynamicMeshPart> dynamicParts = entity.Load(detailLevel);
        //ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        //dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();
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

    public void Clear()
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        MVM.Clear();
    }

    public void Dispose()
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        MVM.Dispose();
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
                if (data.MapDataTable.TagData.DataEntries[0].DataResource.GetValue(data.MapDataTable.GetReader()) is SMapDataResource staticMapResource)
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
                    case SMapDataResource staticMapResource:
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

    // TODO: Merge all this into one, or simplify it?
    private List<MainViewModel.DisplayPart> MakeDisplayParts(StaticMapData staticMap, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new();
        if (Strategy.IsD1())
        {
            if (staticMap.TagData.D1StaticMapData is not null)
            {
                StaticMapData_D1 d1MapData = staticMap.TagData.D1StaticMapData;
                Dictionary<FileHash, List<StaticMapData_D1.MeshInfo>> statics = d1MapData.GetStatics();
                List<StaticMapData_D1.InstanceTransform> instances = d1MapData.ParseTransforms();
                Parallel.ForEach(statics, mesh =>
                {
                    List<StaticPart> parts = d1MapData.Load(mesh.Value, instances);
                    foreach (StaticMapData_D1.MeshInfo info in mesh.Value)
                    {
                        for (int i = info.TransformIndex; i < info.TransformIndex + info.InstanceCount; i++)
                        {
                            foreach (StaticPart part in parts)
                            {
                                MainViewModel.DisplayPart displayPart = new();
                                displayPart.BasePart = part;
                                displayPart.Translations.Add(instances[i].Translation.ToVec3());
                                displayPart.Rotations.Add(instances[i].Rotation);
                                displayPart.Scales.Add(instances[i].Scale.ToVec3());
                                displayParts.Add(displayPart);
                            }
                        }
                    }
                });
            }
        }
        else
        {
            Parallel.ForEach(staticMap.TagData.InstanceCounts, c =>
            {
                // inefficiency as sometimes there are two instance count entries with same hash. why? idk
                StaticMesh model = staticMap.TagData.Statics[c.StaticIndex].Static;
                List<StaticPart> parts = model.Load(ExportDetailLevel.MostDetailed);
                for (int i = c.InstanceOffset; i < c.InstanceOffset + c.InstanceCount; i++)
                {
                    foreach (StaticPart part in parts)
                    {
                        MainViewModel.DisplayPart displayPart = new();
                        displayPart.BasePart = part;
                        displayPart.Translations.Add(staticMap.TagData.Instances[i].Position);
                        displayPart.Rotations.Add(staticMap.TagData.Instances[i].Rotation);
                        displayPart.Scales.Add(new Vector3(staticMap.TagData.Instances[i].Scale.X));
                        displayParts.Add(displayPart);
                    }

                }
            });
        }

        return displayParts.ToList();
    }

    private List<MainViewModel.DisplayPart> MakeTerrainDisplayParts(Terrain terrain, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new();
        List<StaticPart> parts = new();
        foreach (STerrainPart partEntry in terrain.TagData.StaticParts)
        {
            if (partEntry.DetailLevel == 0)
            {
                StaticPart part = terrain.MakePart(partEntry);
                terrain.TransformPositions(part);
                terrain.TransformTexcoords(part);
                parts.Add(part);
            }
        }

        foreach (StaticPart part in parts)
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

    private List<MainViewModel.DisplayPart> MakeEntityDisplayParts(FileHash hash, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new();

        List<SMapDataEntry> dataEntries = new();
        if (Strategy.IsD1() && hash.GetReferenceHash().Hash32 == 0x808003F6) //F6038080
            dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SF6038080>(hash).TagData.EntityComponent.CollapseIntoDataEntry());
        else
            dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SMapDataTable>(hash).TagData.DataEntries);

        Parallel.ForEach(dataEntries, entry =>
        {
            Entity entity = FileResourcer.Get().GetFile(typeof(Entity), entry.Entity.Hash);
            List<Entity> entities = new() { entity };
            entities.AddRange(entity.GetEntityChildren());
            foreach (Entity ent in entities)
            {
                if (ent.HasGeometry())
                {
                    List<DynamicMeshPart> parts = ent.Load(ExportDetailLevel.MostDetailed);

                    foreach (DynamicMeshPart part in parts)
                    {
                        MainViewModel.DisplayPart displayPart = new();
                        displayPart.BasePart = part;
                        displayPart.Translations.Add(entry.Transfrom.Translation.ToVec3());
                        displayPart.Rotations.Add(entry.Transfrom.Rotation);
                        displayPart.Scales.Add(new Tiger.Schema.Vector3(entry.Transfrom.Translation.W, entry.Transfrom.Translation.W, entry.Transfrom.Translation.W));
                        displayParts.Add(displayPart);
                    }
                }
            }
        });
        return displayParts.ToList();
    }
}
