using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

    public void LoadMap(FileHash fileHash, ExportDetailLevel detailLevel)
    {
        GetStaticMapData(fileHash, detailLevel);
        // _mainWindow.SetNewestTabSelected();
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
            Directory.CreateDirectory(savePath + "/Entities");
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
            Directory.CreateDirectory(savePath + "/Entities");
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
        // todo these scenes can be combined
        ExporterScene dynamicPointScene = Exporter.Get().CreateScene($"{map.Hash}_EntityPoints", ExportType.EntityPoints);
        ExporterScene dynamicScene = Exporter.Get().CreateScene($"{map.Hash}_Entities", ExportType.Map);

        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            //Console.WriteLine($"{data.MapDataTable.Hash}");
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
                if (entry is SMapDataEntry dynamicResource)
                {
                    Entity entity = FileResourcer.Get().GetFile<Entity>(entry.GetEntityHash());
                    if(entity.HasGeometry())
                    {
                        dynamicScene.AddMapEntity(dynamicResource, entity);
                    }
                    dynamicPointScene.AddEntityPoints(dynamicResource);
                }
                if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is CubemapResource cubemap)
                {
                    scene.AddCubemap(cubemap);
                }
                if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is SMapDecalsResource decals)
                {
                    if (decals.MapDecals is not null)
                    {
                        Directory.CreateDirectory($"{savePath}/textures/decals/");
                        foreach (var item in decals.MapDecals.TagData.DecalResources)
                        {
                            // Check if the index is within the bounds of the second list
                            if (item.StartIndex >= 0 && item.StartIndex < decals.MapDecals.TagData.Locations.Count)
                            {
                                // Loop through the second list based on the given parameters
                                for (int i = item.StartIndex; i < item.StartIndex + item.Count && i < decals.MapDecals.TagData.Locations.Count; i++)
                                {
                                    var secondListEntry = decals.MapDecals.TagData.Locations[i];
                                    var boxCorners = decals.MapDecals.TagData.DecalProjectionBounds.TagData.InstanceBounds[i];

                                    // Access the desired data from the second list entry
                                    Vector4 location = secondListEntry.Location;

                                    //item.Material.SavePixelShader($"{ConfigHandler.GetExportSavePath()}/test/");
                                    item.Material.SaveAllTextures($"{savePath}/textures/decals/");
                                    //Source2Handler.SaveDecalVMAT($"{ConfigHandler.GetExportSavePath()}/test/", item.Material.Hash, item.Material);

                                    //fbxHandler.AddEmptyToScene($"{item.Material.Hash} {boxCorners.Unk24}", location, Vector4.Zero);
                                    //fbxHandler.InfoHandler.AddDecal(boxCorners.Unk24.GetHashString(), item.Material.Hash, location, boxCorners.Corner1, boxCorners.Corner2);
                                    //fbxHandler.InfoHandler.AddMaterial(item.Material);
                                }
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
                if (entry is SMapDataEntry dynamicResource)
                {
                    Entity entity = FileResourcer.Get().GetFile<Entity>(entry.GetEntityHash());
                    if (entity.HasGeometry())
                    {
                        ExporterScene dynamicScene = Exporter.Get().CreateScene(entity.Hash, ExportType.EntityInMap);
                        dynamicScene.AddEntity(dynamicResource.GetEntityHash(), entity.Model.Load(ExportDetailLevel.MostDetailed, entity.ModelParentResource), entity.Skeleton?.GetBoneNodes());
                        entity.SaveMaterialsFromParts(savePath, entity.Model.Load(ExportDetailLevel.MostDetailed, entity.ModelParentResource), true);
                        if (source2Models)
                        {
                            Source2Handler.SaveEntityVMDL($"{savePath}/Entities", entity);
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
}
