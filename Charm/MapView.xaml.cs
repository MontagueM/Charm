using System;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Static;

namespace Charm;

public partial class MapView : UserControl
{
    // public StaticMapData StaticMap;
    // public FileHash Hash;

    private static MainWindow _mainWindow = null;

    private static ConfigSubsystem _config = CharmInstance.GetSubsystem<ConfigSubsystem>();

    private static bool source2Models = _config.GetS2VMDLExportEnabled();
    private static bool source2Mats = _config.GetS2VMATExportEnabled();
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
        FbxHandler fbxHandler = new FbxHandler();

        string meshName = map.Hash.ToString();
        string savePath = _config.GetExportSavePath() + $"/{meshName}";
        if (_config.GetSingleFolderMapsEnabled())
        {
            savePath = _config.GetExportSavePath() + "/Maps";
        }
        fbxHandler.InfoHandler.SetMeshName(meshName);
        Directory.CreateDirectory(savePath);

        if(exportStatics)
        {
            Directory.CreateDirectory(savePath + "/Statics");
            ExportStatics(exportStatics, savePath, map);
        }

        ExtractDataTables(map, savePath, fbxHandler, ExportTypeFlag.Full);

        if (_config.GetUnrealInteropEnabled())
        {
            fbxHandler.InfoHandler.SetUnrealInteropPath(_config.GetUnrealInteropPath());
            AutomatedExporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat(), _config.GetSingleFolderMapsEnabled());
        }
        if (_config.GetBlenderInteropEnabled())
        {
            AutomatedExporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat());
        }

        fbxHandler.InfoHandler.AddType("Map");
        fbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        fbxHandler.Dispose();
    }

    public static void ExportMinimalMap(Tag<SMapContainer> map, ExportTypeFlag exportTypeFlag)
    {
        FbxHandler fbxHandler = new FbxHandler();

        string meshName = map.Hash.ToString();
        string savePath = _config.GetExportSavePath() + $"/{meshName}";
        if (_config.GetSingleFolderMapsEnabled())
        {
            savePath = _config.GetExportSavePath() + "/Maps";
        }
        fbxHandler.InfoHandler.SetMeshName(meshName);
        Directory.CreateDirectory(savePath);
        Directory.CreateDirectory(savePath + "/Dynamics");

        if(exportStatics)
        {
            Directory.CreateDirectory(savePath + "/Statics");
            Directory.CreateDirectory(savePath + "/Statics/LOD");
            ExportStatics(exportStatics, savePath, map);
        }

        ExtractDataTables(map, savePath, fbxHandler, exportTypeFlag);

        if (_config.GetUnrealInteropEnabled())
        {
            fbxHandler.InfoHandler.SetUnrealInteropPath(_config.GetUnrealInteropPath());
            AutomatedExporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat(), _config.GetSingleFolderMapsEnabled());
        }
        if (_config.GetBlenderInteropEnabled())
        {
            AutomatedExporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat());
        }

        fbxHandler.InfoHandler.AddType("Map");
        fbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        fbxHandler.Dispose();
    }

    public static void ExportTerrainMap(Tag<SMapContainer> map)
    {
        FbxHandler fbxHandler = new FbxHandler();
        string meshName = map.Hash.ToString();
        string savePath = _config.GetExportSavePath() + $"/{meshName}";
        if (_config.GetSingleFolderMapsEnabled())
        {
            savePath = _config.GetExportSavePath() + "/Maps";
        }
        if (_config.GetUnrealInteropEnabled())
        {
            fbxHandler.InfoHandler.SetUnrealInteropPath(_config.GetUnrealInteropPath());
        }

        fbxHandler.InfoHandler.SetMeshName(meshName+"_Terrain");
        Directory.CreateDirectory(savePath);

        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            data.MapDataTable.TagData.DataEntries.ForEach(entry =>
            {
                if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is D2Class_7D6C8080 terrainArrangement)  // Terrain
                {
                    //entry.Rotation.SetW(1);
                    terrainArrangement.Terrain.LoadIntoFbxScene(fbxHandler, savePath, _config.GetUnrealInteropEnabled(), terrainArrangement);
                }
            });
        });

        if (_config.GetBlenderInteropEnabled())
        {
            AutomatedExporter.SaveInteropBlenderPythonFile(savePath, meshName + "_Terrain", AutomatedExporter.ImportType.Terrain, _config.GetOutputTextureFormat());
        }

        fbxHandler.InfoHandler.AddType("Terrain");
        fbxHandler.ExportScene($"{savePath}/{meshName}_Terrain.fbx");
        fbxHandler.Dispose();
    }

    private static void ExtractDataTables(Tag<SMapContainer> map, string savePath, FbxHandler fbxHandler, ExportTypeFlag exportTypeFlag)
    {
        FbxHandler dynamicHandler = new FbxHandler(false);
        //dynamicHandler.InfoHandler.SetMeshName(map.Hash.GetHashString()+"_DynamicPoints");
        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            data.MapDataTable.TagData.DataEntries.ForEach(entry =>
            {
                //Console.WriteLine($"{entry.DataResource}");
                if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is SMapDataResource staticMapResource)  // Static map
                {
                    if (exportTypeFlag == ExportTypeFlag.ArrangedMap)
                    {
                        staticMapResource.StaticMapParent.TagData.StaticMap.LoadArrangedIntoFbxScene(fbxHandler); //Arranged because...arranged
                    }
                    else if (exportTypeFlag == ExportTypeFlag.Full || exportTypeFlag == ExportTypeFlag.Minimal) //No terrain on a minimal rip makes sense right?
                    {
                        staticMapResource.StaticMapParent.TagData.StaticMap.LoadIntoFbxScene(fbxHandler, savePath, _config.GetUnrealInteropEnabled() || _config.GetS2ShaderExportEnabled());
                    }
                }
                else if(entry is SMapDataEntry dynamicResource)
                {
                    dynamicHandler.AddDynamicPointsToScene(dynamicResource, dynamicResource.Entity.Hash, dynamicHandler);
                }
                // else if (entry.DataResource is D2Class_7D6C8080 terrainArrangement && exportTypeFlag == ExportTypeFlag.Full)  // Terrain should only export with a Full export or terrain only
                // {
                //     //entry.Rotation.SetW(1);
                //     terrainArrangement.Terrain.LoadIntoFbxScene(fbxHandler, savePath, _config.GetUnrealInteropEnabled() || _config.GetS2ShaderExportEnabled(), terrainArrangement);
                // }
            });
        });
        dynamicHandler.ExportScene($"{savePath}/{map.Hash}_DynamicPoints.fbx");
        dynamicHandler.Dispose();
    }

    private static void ExportStatics(bool exportStatics, string savePath, Tag<SMapContainer> map)
    {
        if (exportStatics) //Export individual statics
        {
            Parallel.ForEach(map.TagData.MapDataTables, data =>
            {
                data.MapDataTable.TagData.DataEntries.ForEach(entry =>
                {
                    if (entry.DataResource.GetValue(data.MapDataTable.GetReader()) is SMapDataResource staticMapResource)  // Static map
                    {
                        var parts = staticMapResource.StaticMapParent.TagData.StaticMap.TagData.Statics;
                        //staticMapResource.StaticMapParent.TagData.StaticMap.LoadIntoFbxScene(staticHandler, savePath, _config.GetUnrealInteropEnabled());
                        //Parallel.ForEach(parts, part =>
                        foreach(var part in parts)
                        {
                            if(File.Exists($"{savePath}/Statics/{part.Static.Hash}.fbx")) continue;

                            string staticMeshName = part.Static.Hash.ToString();
                            FbxHandler staticHandler = new FbxHandler(false);

                            //staticHandler.InfoHandler.SetMeshName(staticMeshName);
                            var staticmesh = part.Static.Load(ExportDetailLevel.MostDetailed);

                            staticHandler.AddStaticToScene(staticmesh, part.Static.Hash);

                            if(source2Models)
                            {
                                //Source 2 shit
                                File.Copy("Exporters/template.vmdl", $"{savePath}/Statics/{staticMeshName}.vmdl", true);
                                string text = File.ReadAllText($"{savePath}/Statics/{staticMeshName}.vmdl");

                                StringBuilder mats = new StringBuilder();

                                int i = 0;
                                foreach (StaticPart staticpart in staticmesh)
                                {
                                    mats.AppendLine("{");
                                    //mats.AppendLine($"    from = \"{staticMeshName}_Group{staticpart.GroupIndex}_index{staticpart.Index}_{i}_{staticpart.LodCategory}_{i}.vmat\"");
                                    mats.AppendLine($"    from = \"{staticpart.Material.Hash}.vmat\"");
                                    mats.AppendLine($"    to = \"materials/{staticpart.Material.Hash}.vmat\"");
                                    mats.AppendLine("},\n");
                                    i++;
                                }

                                text = text.Replace("%MATERIALS%", mats.ToString());
                                text = text.Replace("%FILENAME%", $"models/{staticMeshName}.fbx");
                                text = text.Replace("%MESHNAME%", staticMeshName);

                                File.WriteAllText($"{savePath}/Statics/{staticMeshName}.vmdl", text);
                                //
                            }

                            staticHandler.ExportScene($"{savePath}/Statics/{staticMeshName}.fbx");
                            staticHandler.Dispose();
                    }//);
                    }
                    // Dont see a reason to export terrain itself as its own fbx
                    // else if (entry.DataResource is D2Class_7D6C8080 terrainArrangement)  // Terrain
                    // {
                    //     var parts = terrainArrangement.Terrain.TagData.MeshParts;
                    //     terrainArrangement.Terrain.LoadIntoFbxScene(staticHandler, savePath, _config.GetUnrealInteropEnabled(), terrainArrangement);

                    //     int i = 0;
                    //     foreach (var part in parts)
                    //     {
                    //         mats.AppendLine("{");
                    //         mats.AppendLine($"    from = \"{staticMeshName}_Group{part.GroupIndex}_{i}_{i}.vmat\"");
                    //         mats.AppendLine($"    to = \"materials/{part.Material.Hash}.vmat\"");
                    //         mats.AppendLine("},\n");
                    //         i++;
                    //     }

                    // }

                    //
                });
            });
        }
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
