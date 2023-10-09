using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
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
        Tag<SMapDataTable> dataentry = FileResourcer.Get().GetSchemaTag<SMapDataTable>(tagHash);
        SetEntityMapUI(dataentry, detailLevel);
    }

    private void GetStaticMapData(FileHash fileHash, ExportDetailLevel detailLevel)
    {
        Tag<SMapContainer> tag = FileResourcer.Get().GetSchemaTag<SMapContainer>(fileHash);
        Tag<SMapDataTable> mapDataTable = tag.TagData.MapDataTables[1].MapDataTable;
        StaticMapData staticMapData = ((SMapDataResource)mapDataTable.TagData.DataEntries[0].DataResource.GetValue(mapDataTable.GetReader())).StaticMapParent.TagData.StaticMap;
        SetMapUI(staticMapData, detailLevel);
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

    private void SetEntityMapUI(Tag<SMapDataTable> dataentry, ExportDetailLevel detailLevel)
    {
        var displayParts = MakeEntityDisplayParts(dataentry, detailLevel);
        Dispatcher.Invoke(() =>
        {
            MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
            MVM.SetChildren(displayParts);
        });
        displayParts.Clear();
    }

    public bool LoadEntity(FileHash entityHash, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        AddEntity(entityHash, ExportDetailLevel.MostDetailed, fbxHandler);
        return LoadUI(fbxHandler);
    }

    private void AddEntity(FileHash entityHash, ExportDetailLevel detailLevel, FbxHandler fbxHandler)
	{
        Entity entity = FileResourcer.Get().GetFile(typeof(Entity), entityHash);
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

    public static void ExportFullMap(Tag<SMapContainer> map)
    {
        ExporterScene scene = Exporter.Get().CreateScene(map.Hash.ToString(), ExportType.Map);

        string meshName = map.Hash.ToString();
        string savePath = _config.GetExportSavePath() + $"/{meshName}";
        if (_config.GetSingleFolderMapsEnabled())
        {
            savePath = _config.GetExportSavePath() + "/Maps";
        }

        Directory.CreateDirectory(savePath);
        if(exportStatics)
        {
            Directory.CreateDirectory(savePath + "/Statics");
            ExportStatics(savePath, map);
        }

        ExtractDataTables(map, savePath, scene, ExportTypeFlag.Full);

        if (_config.GetUnrealInteropEnabled())
        {
            AutomatedExporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat(), _config.GetSingleFolderMapsEnabled());
        }
    }

    public static void ExportMinimalMap(Tag<SMapContainer> map, ExportTypeFlag exportTypeFlag)
    {
        ExporterScene scene = Exporter.Get().CreateScene(map.Hash.ToString(), ExportType.Map);
        string meshName = map.Hash.ToString();
        string savePath = _config.GetExportSavePath() + $"/{meshName}";
        if (_config.GetSingleFolderMapsEnabled())
        {
            savePath = _config.GetExportSavePath() + "/Maps";
        }
        Directory.CreateDirectory(savePath);
        if (exportStatics)
        {
            Directory.CreateDirectory(savePath + "/Statics");
            ExportStatics(savePath, map);
        }

        ExtractDataTables(map, savePath, scene, exportTypeFlag);

        if (_config.GetUnrealInteropEnabled())
        {
            AutomatedExporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat(), _config.GetSingleFolderMapsEnabled());
        }
    }

    public static void ExportTerrainMap(Tag<SMapContainer> map)
    {
        ExporterScene scene = Exporter.Get().CreateScene($"{map.Hash}_Terrain", ExportType.Terrain);
        bool export = false;
        string meshName = map.Hash.ToString();
        string savePath = _config.GetExportSavePath() + $"/{meshName}";
        if (_config.GetSingleFolderMapsEnabled())
        {
            savePath = _config.GetExportSavePath() + "/Maps";
        }

        Directory.CreateDirectory(savePath);

        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            data.MapDataTable.TagData.DataEntries.ForEach(entry =>
            {
                if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is D2Class_7D6C8080 terrainArrangement)  // Terrain
                {
                    export = true;
                    terrainArrangement.Terrain.LoadIntoExporter(scene, savePath, _config.GetUnrealInteropEnabled(), terrainArrangement);
                    if (exportStatics)
                    {
                        if (source2Models)
                        {
                            File.Copy("Exporters/template.vmdl", $"{savePath}/Statics/{terrainArrangement.Terrain.Hash}_Terrain.vmdl", true);
                        }
                        ExporterScene staticScene = Exporter.Get().CreateScene($"{terrainArrangement.Terrain.Hash}_Terrain", ExportType.StaticInMap);
                        terrainArrangement.Terrain.LoadIntoExporter(staticScene, savePath, _config.GetUnrealInteropEnabled() || _config.GetS2ShaderExportEnabled(), terrainArrangement, true);
                    }
                }
            });
        });

        if (export)
        {
            if (_config.GetUnrealInteropEnabled())
            {
                AutomatedExporter.SaveInteropUnrealPythonFile(savePath, meshName + "_Terrain", AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat(), _config.GetSingleFolderMapsEnabled());
            }
        }
    }

    private static void ExtractDataTables(Tag<SMapContainer> map, string savePath, ExporterScene scene, ExportTypeFlag exportTypeFlag)
    {
        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            data.MapDataTable.TagData.DataEntries.ForEach(entry =>
            {
                if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is SMapDataResource staticMapResource)  // Static map
                {
                    if (exportTypeFlag == ExportTypeFlag.ArrangedMap)
                    {
                        staticMapResource.StaticMapParent.TagData.StaticMap.LoadArrangedIntoExporterScene(); //Arranged because...arranged
                    }
                    else if (exportTypeFlag == ExportTypeFlag.Full || exportTypeFlag == ExportTypeFlag.Minimal) //No terrain on a minimal rip makes sense right?
                    {
                        staticMapResource.StaticMapParent.TagData.StaticMap.LoadIntoExporterScene(scene, savePath, _config.GetUnrealInteropEnabled() || _config.GetS2ShaderExportEnabled());
                    }
                }
                if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is CubemapResource cubemap)
                {
                    scene.AddCubemap(cubemap);
                }
                if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is SMapLightResource mapLight)
                {
                    scene.AddPointLight(pointLight, entry);
                }
                if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is SMapDecalsResource decals)
                {
                    if (decals.MapDecals is null || decals.MapDecals.TagData.DecalResources is null)
                        return;
                    scene.AddDecals(decals);
                    foreach (var item in decals.MapDecals.TagData.DecalResources)
                    {
                        if (item.StartIndex >= 0 && item.StartIndex < decals.MapDecals.TagData.Locations.Count)
                        { 
                            for (int i = item.StartIndex; i < item.StartIndex + item.Count && i < decals.MapDecals.TagData.Locations.Count; i++)
                            {
                                scene.Materials.Add(new ExportMaterial(item.Material));
                            }
                        }
                    }
                }
            });
        });
    }

    private static void ExportStatics(string savePath, Tag<SMapContainer> map)
    {
        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            data.MapDataTable.TagData.DataEntries.ForEach(entry =>
            {
                if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is SMapDataResource staticMapResource)  // Static map
                {
                    var parts = staticMapResource.StaticMapParent.TagData.StaticMap.TagData.Statics;
                    foreach (var part in parts)
                    {
                        if (File.Exists($"{savePath}/Statics/{part.Static.Hash}.fbx")) continue;

                        string staticMeshName = part.Static.Hash.ToString();
                        ExporterScene staticScene = Exporter.Get().CreateScene(staticMeshName, ExportType.StaticInMap);
                        var staticmesh = part.Static.Load(ExportDetailLevel.MostDetailed);
                        staticScene.AddStatic(part.Static.Hash, staticmesh);

                        if (source2Models)
                        {
                            Source2Handler.SaveStaticVMDL($"{savePath}/Statics", staticMeshName, staticmesh);
                        }
                    }
                }
            });
        });
    }

    private List<MainViewModel.DisplayPart> MakeDisplayParts(StaticMapData staticMap, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();
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
                    displayPart.Scales.Add(staticMap.TagData.Instances[i].Scale);
                    displayParts.Add(displayPart);
                }

            }
        });
        return displayParts.ToList();
    }

    private List<MainViewModel.DisplayPart> MakeEntityDisplayParts(Tag<SMapDataTable> dataentry, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();
        Parallel.ForEach(dataentry.TagData.DataEntries, entry =>
        {
            Entity entity = FileResourcer.Get().GetFile(typeof(Entity), entry.GetEntityHash());
            if (entity.HasGeometry())
            {
                var parts = entity.Load(ExportDetailLevel.MostDetailed);

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
        });
        return displayParts.ToList();
    }
}
