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
using Field;
using Field.General;
using Field.Models;
using Field.Entities;
using Field.Statics;
using Serilog;

namespace Charm;

public partial class MapView : UserControl
{
    // public StaticMapData StaticMap;
    // public TagHash Hash;

    private static MainWindow _mainWindow = null;

    private static bool source2Models = ConfigHandler.GetS2VMDLExportEnabled();
    private static bool source2Mats = ConfigHandler.GetS2VMATExportEnabled();
    private static bool exportStatics = ConfigHandler.GetIndvidualStaticsEnabled();


    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        ModelView.LodCombobox.SelectedIndex = 1; // default to least detail
    }
    
    public MapView()
    {
        InitializeComponent();
    }

    public void LoadMap(TagHash tagHash, ELOD detailLevel)
    {
        GetStaticMapData(tagHash, detailLevel);
        // _mainWindow.SetNewestTabSelected();
    }

    private void GetStaticMapData(TagHash tagHash, ELOD detailLevel)
    {
        Tag<D2Class_07878080> tag = PackageHandler.GetTag<D2Class_07878080>(tagHash);
        StaticMapData staticMapData = ((D2Class_C96C8080)tag.Header.DataTables[1].DataTable.Header.DataEntries[0].DataResource).StaticMapParent.Header.StaticMap;
        SetMapUI(staticMapData, detailLevel);
    }

    private void SetMapUI(StaticMapData staticMapData, ELOD detailLevel)
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
    
    public static void ExportFullMap(Tag<D2Class_07878080> map)
    {
        FbxHandler fbxHandler = new FbxHandler();
       
        string meshName = map.Hash.GetHashString();
        string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
        if (ConfigHandler.GetSingleFolderMapsEnabled())
        {
            savePath = ConfigHandler.GetExportSavePath() + "/Maps";
        }
        fbxHandler.InfoHandler.SetMeshName(meshName);
        Directory.CreateDirectory(savePath);
    
        if(exportStatics)
        {
            Directory.CreateDirectory(savePath + "/Statics");
            ExportStatics(exportStatics, savePath, map);
        }

        ExtractDataTables(map, savePath, fbxHandler, EExportTypeFlag.Full);

        if (ConfigHandler.GetUnrealInteropEnabled())
        {
            fbxHandler.InfoHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
            AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Map, ConfigHandler.GetOutputTextureFormat(), ConfigHandler.GetSingleFolderMapsEnabled());
        }
        if (ConfigHandler.GetBlenderInteropEnabled())
        {
            //Only gonna export a blender py for maps (for now)
            AutomatedImporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedImporter.EImportType.Map, ConfigHandler.GetOutputTextureFormat(), ConfigHandler.GetSingleFolderMapsEnabled());
        }

        fbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        fbxHandler.Dispose();
    }

    public static void ExportMinimalMap(Tag<D2Class_07878080> map, EExportTypeFlag exportTypeFlag)
    {
        FbxHandler fbxHandler = new FbxHandler();
       
        string meshName = map.Hash.GetHashString();
        string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
        if (ConfigHandler.GetSingleFolderMapsEnabled())
        {
            savePath = ConfigHandler.GetExportSavePath() + "/Maps";
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

        ExtractDataTables(map, savePath, fbxHandler, EExportTypeFlag.Minimal);

        if (ConfigHandler.GetUnrealInteropEnabled())
        {
            fbxHandler.InfoHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
            AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Map, ConfigHandler.GetOutputTextureFormat(), ConfigHandler.GetSingleFolderMapsEnabled());
        }
        if (ConfigHandler.GetBlenderInteropEnabled())
        {
            //Only gonna export a blender py for maps (for now)
            AutomatedImporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedImporter.EImportType.Map, ConfigHandler.GetOutputTextureFormat(), ConfigHandler.GetSingleFolderMapsEnabled());
        }

        fbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        fbxHandler.Dispose();
    }

    public static void ExportTerrainMap(Tag<D2Class_07878080> map)
    {
        FbxHandler fbxHandler = new FbxHandler();
        string meshName = map.Hash.GetHashString();
        string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
        if (ConfigHandler.GetSingleFolderMapsEnabled())
        {
            savePath = ConfigHandler.GetExportSavePath() + "/Maps";
        }
        if (ConfigHandler.GetUnrealInteropEnabled())
        {
            fbxHandler.InfoHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
        }
        
        fbxHandler.InfoHandler.SetMeshName(meshName+"_Terrain");
        Directory.CreateDirectory(savePath);

        Parallel.ForEach(map.Header.DataTables, data =>
        {
            data.DataTable.Header.DataEntries.ForEach(entry =>
            {
                if (entry.DataResource is D2Class_7D6C8080 terrainArrangement)  // Terrain
                {
                    //entry.Rotation.SetW(1);
                    terrainArrangement.Terrain.LoadIntoFbxScene(fbxHandler, savePath, ConfigHandler.GetUnrealInteropEnabled(), terrainArrangement);
                }
            });
        });

        if (ConfigHandler.GetBlenderInteropEnabled())
        {
            AutomatedImporter.SaveInteropBlenderPythonFile(savePath, meshName + "_Terrain", AutomatedImporter.EImportType.Map, ConfigHandler.GetOutputTextureFormat(), ConfigHandler.GetSingleFolderMapsEnabled());
        }

        fbxHandler.ExportScene($"{savePath}/{meshName}_Terrain.fbx");
        fbxHandler.Dispose();
    }
    
    private static void ExtractDataTables(Tag<D2Class_07878080> map, string savePath, FbxHandler fbxHandler, EExportTypeFlag exportTypeFlag)
    {
        FbxHandler dynamicHandler = new FbxHandler(false);
        //dynamicHandler.InfoHandler.SetMeshName(map.Hash.GetHashString()+"_DynamicPoints");
        Parallel.ForEach(map.Header.DataTables, data =>
        {
            data.DataTable.Header.DataEntries.ForEach(entry =>
            {
                //Console.WriteLine($"{entry.DataResource}");
                if (entry.DataResource is D2Class_C96C8080 staticMapResource)  // Static map
                {
                    if (exportTypeFlag == EExportTypeFlag.ArrangedMap)
                    {
                        staticMapResource.StaticMapParent.Header.StaticMap.LoadArrangedIntoFbxScene(fbxHandler); //Arranged because...arranged
                    }
                    else if (exportTypeFlag == EExportTypeFlag.Full || exportTypeFlag == EExportTypeFlag.Minimal) //No terrain on a minimal rip makes sense right?
                    {
                        staticMapResource.StaticMapParent.Header.StaticMap.LoadIntoFbxScene(fbxHandler, savePath, ConfigHandler.GetUnrealInteropEnabled() || ConfigHandler.GetS2ShaderExportEnabled());
                    }
                }
                else if(entry is D2Class_85988080 dynamicResource)
                {    
                    dynamicHandler.AddDynamicPointsToScene(dynamicResource, dynamicResource.Entity.Hash, dynamicHandler);
                }
                else if (entry.DataResource is D2Class_7D6C8080 terrainArrangement && exportTypeFlag == EExportTypeFlag.Full)  // Terrain should only export with a Full export or terrain only
                {
                    //entry.Rotation.SetW(1);
                    terrainArrangement.Terrain.LoadIntoFbxScene(fbxHandler, savePath, ConfigHandler.GetUnrealInteropEnabled() || ConfigHandler.GetS2ShaderExportEnabled(), terrainArrangement);
                }
            });
        });
        dynamicHandler.ExportScene($"{savePath}/{map.Hash.GetHashString()}_DynamicPoints.fbx");
        dynamicHandler.Dispose();
    }

    private static void ExportStatics(bool exportStatics, string savePath, Tag<D2Class_07878080> map)
    {
        if (exportStatics) //Export individual statics
        {
            Parallel.ForEach(map.Header.DataTables, data =>
            {    
                data.DataTable.Header.DataEntries.ForEach(entry =>
                {
                    if (entry.DataResource is D2Class_C96C8080 staticMapResource)  // Static map
                    {
                        var parts = staticMapResource.StaticMapParent.Header.StaticMap.Header.Statics;
                        //staticMapResource.StaticMapParent.Header.StaticMap.LoadIntoFbxScene(staticHandler, savePath, ConfigHandler.GetUnrealInteropEnabled());
                        //Parallel.ForEach(parts, part =>
                        foreach(var part in parts)
                        {
                            if(File.Exists($"{savePath}/Statics/{part.Static.Hash.GetHashString()}.fbx")) continue;
                            
                            string staticMeshName = part.Static.Hash.GetHashString();
                            FbxHandler staticHandler = new FbxHandler(false);
                            
                            staticHandler.InfoHandler.SetMeshName(staticMeshName);
                            var staticmesh = part.Static.Load(ELOD.MostDetail);

                            staticHandler.AddStaticToScene(staticmesh, part.Static.Hash);

                            if(source2Models)
                            {
                                //Source 2 shit
                                File.Copy("template.vmdl", $"{savePath}/Statics/{staticMeshName}.vmdl", true);
                                string text = File.ReadAllText($"{savePath}/Statics/{staticMeshName}.vmdl");
                            
                                StringBuilder mats = new StringBuilder();

                                int i = 0;
                                foreach (Part staticpart in staticmesh)
                                {
                                    mats.AppendLine("{");
                                    mats.AppendLine($"    from = \"{staticMeshName}_Group{staticpart.GroupIndex}_{i}_{i}.vmat\"");
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
                    //     var parts = terrainArrangement.Terrain.Header.MeshParts;
                    //     terrainArrangement.Terrain.LoadIntoFbxScene(staticHandler, savePath, ConfigHandler.GetUnrealInteropEnabled(), terrainArrangement);
                    
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

    private List<MainViewModel.DisplayPart> MakeDisplayParts(StaticMapData staticMap, ELOD detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();
        Parallel.ForEach(staticMap.Header.InstanceCounts, c =>
        {
            // inefficiency as sometimes there are two instance count entries with same hash. why? idk
            var model = staticMap.Header.Statics[c.StaticIndex].Static;
            var parts = model.Load(ELOD.MostDetail);
            for (int i = c.InstanceOffset; i < c.InstanceOffset + c.InstanceCount; i++)
            {
                foreach (var part in parts)
                {
                    MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
                    displayPart.BasePart = part;
                    displayPart.Translations.Add(staticMap.Header.Instances[i].Position);
                    displayPart.Rotations.Add(staticMap.Header.Instances[i].Rotation);
                    displayPart.Scales.Add(staticMap.Header.Instances[i].Scale);
                    displayParts.Add(displayPart);
                }

            }
        });
        return displayParts.ToList();
    }
}