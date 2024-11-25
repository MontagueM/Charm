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

    private static ConfigSubsystem _config = CharmInstance.GetSubsystem<ConfigSubsystem>();

    private static bool source2Models = _config.GetS2VMDLExportEnabled();
    private static bool exportStatics = _config.GetIndvidualStaticsEnabled();

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
        foreach (var tables in tag.TagData.MapDataTables)
        {
            foreach (var entry in tables.MapDataTable.TagData.DataEntries)
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
        var displayParts = MakeDisplayParts(staticMapData, detailLevel);
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
        var displayParts = MakeEntityDisplayParts(dataentry, detailLevel);
        Dispatcher.Invoke(() =>
        {
            MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
            MVM.SetChildren(displayParts);
        });
        displayParts.Clear();
    }

    private void SetTerrainMapUI(Terrain terrain, ExportDetailLevel detailLevel)
    {
        var displayParts = MakeTerrainDisplayParts(terrain, detailLevel);
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
        foreach (var entity in entities)
            AddEntity(entity, ExportDetailLevel.MostDetailed, fbxHandler);
        return LoadUI(fbxHandler);
    }

    private void AddEntity(Entity entity, ExportDetailLevel detailLevel, FbxHandler fbxHandler)
    {
        var dynamicParts = entity.Load(detailLevel);
        //ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        //dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();
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

    public static void ExportFullMap(Tag<SMapContainer> map, ExportTypeFlag exportTypeFlag = ExportTypeFlag.Full)
    {
        ExporterScene scene = Exporter.Get().CreateScene(map.Hash.ToString(), ExportType.Map);

        string meshName = map.Hash.ToString();
        string savePath = _config.GetExportSavePath() + $"/{meshName}";
        if (_config.GetSingleFolderMapsEnabled())
        {
            savePath = _config.GetExportSavePath() + "/Maps";
        }

        Directory.CreateDirectory(savePath);

        ExtractDataTables(map, savePath, scene, ExportTypeFlag.Full);

        if (_config.GetUnrealInteropEnabled())
        {
            AutomatedExporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat(), _config.GetSingleFolderMapsEnabled());
        }
    }

    private static void ExtractDataTables(Tag<SMapContainer> map, string savePath, ExporterScene scene, ExportTypeFlag exportTypeFlag)
    {
        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
            {
                if (data.MapDataTable.TagData.DataEntries[0].DataResource.GetValue(data.MapDataTable.GetReader()) is SMapDataResource staticMapResource)
                {
                    staticMapResource.StaticMapParent.Load();
                    staticMapResource.StaticMapParent.TagData.StaticMap.LoadDecalsIntoExporterScene(scene);
                }
            }

            data.MapDataTable.TagData.DataEntries.ForEach(entry =>
            {
                if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is SMapDataResource staticMapResource)  // Static map
                {
                    if (exportTypeFlag == ExportTypeFlag.Full)
                    {
                        staticMapResource.StaticMapParent.Load();
                        staticMapResource.StaticMapParent.TagData.StaticMap.LoadIntoExporterScene(scene);
                    }
                }
            });
        });
    }

    // TODO: Merge all this into one, or simplify it?
    private List<MainViewModel.DisplayPart> MakeDisplayParts(StaticMapData staticMap, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            if (staticMap.TagData.D1StaticMapData is not null)
            {
                var d1MapData = staticMap.TagData.D1StaticMapData;
                var statics = d1MapData.GetStatics();
                var instances = d1MapData.ParseTransforms();
                Parallel.ForEach(statics, mesh =>
                {
                    var parts = d1MapData.Load(mesh.Value, instances);
                    foreach (var info in mesh.Value)
                    {
                        for (int i = info.TransformIndex; i < info.TransformIndex + info.InstanceCount; i++)
                        {
                            foreach (var part in parts)
                            {
                                MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
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
                var model = staticMap.TagData.Statics[c.StaticIndex].Static;
                var parts = model.Load(ExportDetailLevel.MostDetailed);
                for (int i = c.InstanceOffset; i < c.InstanceOffset + c.InstanceCount; i++)
                {
                    foreach (var part in parts)
                    {
                        MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
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
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();
        List<StaticPart> parts = new();
        foreach (var partEntry in terrain.TagData.StaticParts)
        {
            if (partEntry.DetailLevel == 0)
            {
                var part = terrain.MakePart(partEntry);
                terrain.TransformPositions(part);
                terrain.TransformTexcoords(part);
                parts.Add(part);
            }
        }

        foreach (var part in parts)
        {
            MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
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
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();

        List<SMapDataEntry> dataEntries = new();
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON && hash.GetReferenceHash().Hash32 == 0x808003F6) //F6038080
            dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SF6038080>(hash).TagData.EntityResource.CollapseIntoDataEntry());
        else
            dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SMapDataTable>(hash).TagData.DataEntries);

        Parallel.ForEach(dataEntries, entry =>
        {
            Entity entity = FileResourcer.Get().GetFile(typeof(Entity), entry.GetEntityHash());
            List<Entity> entities = new List<Entity> { entity };
            entities.AddRange(entity.GetEntityChildren());
            foreach (var ent in entities)
            {
                if (ent.HasGeometry())
                {
                    var parts = ent.Load(ExportDetailLevel.MostDetailed);

                    foreach (var part in parts)
                    {
                        MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
                        displayPart.BasePart = part;
                        displayPart.Translations.Add(entry.Translation.ToVec3());
                        displayPart.Rotations.Add(entry.Rotation);
                        displayPart.Scales.Add(new Tiger.Schema.Vector3(entry.Translation.W, entry.Translation.W, entry.Translation.W));
                        displayParts.Add(displayPart);
                    }
                }
            }
        });
        return displayParts.ToList();
    }
}
