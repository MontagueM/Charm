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
using HelixToolkit.SharpDX.Core.Model.Scene;
using SharpDX.Direct3D9;

namespace Charm;

public partial class MapView : UserControl
{
    // public StaticMapData StaticMap;
    // public TagHash Hash;

    private static MainWindow _mainWindow = null;

    private static bool source2Models = ConfigHandler.GetS2VMDLExportEnabled();
    private static bool source2Mats = ConfigHandler.GetS2VMATExportEnabled();
    private static bool exportStatics = ConfigHandler.GetIndvidualStaticsEnabled();
    private static bool exportEntities = ConfigHandler.GetIndvidualEntitiesEnabled();

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

	public void LoadEntity(Entity entity, FbxHandler fbxHandler)
	{
		fbxHandler.Clear();
		AddEntity(entity, ELOD.MostDetail, fbxHandler);
	}

	private void AddEntity(Entity entity, ELOD detailLevel, FbxHandler fbxHandler)
	{
		var dynamicParts = entity.Load(detailLevel);
		//ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
		//dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();
		fbxHandler.AddEntityToScene(entity, dynamicParts, detailLevel);
		LoadUI(fbxHandler);
	}

	private bool LoadUI(FbxHandler fbxHandler)
	{
		MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
		string filePath = $"{ConfigHandler.GetExportSavePath()}/temp.fbx";
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
    
    public static void ExportFullMap(Tag<D2Class_07878080> map, Activity activity, string bubbleName)
    {
        //Console.WriteLine($"{map.Hash} : {PackageHandler.GetActivityName(activity.Hash)} : {activity.Header.LocationName} : {activity.Hash}");
		//_activityLog.Debug($"Exporting activity data name: {PackageHandler.GetActivityName(activity.Hash)}, hash: {activity.Hash}");
		FbxHandler fbxHandler = new FbxHandler();
       
        string meshName = map.Hash.GetHashString();
        string savePath = ConfigHandler.GetExportSavePath() + $"/Maps/{activity.Header.LocationName}/";
        if (ConfigHandler.GetSingleFolderMapsEnabled())
        {
            savePath = ConfigHandler.GetExportSavePath() + $"/Maps/{activity.Header.LocationName}/{bubbleName}/";
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
            AutomatedImporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedImporter.EImportType.Map, ConfigHandler.GetOutputTextureFormat());
        }

        if(exportEntities)
        {
            Directory.CreateDirectory(savePath + "/Dynamics");
            ExportDynamics(savePath, map);
        }
        
        fbxHandler.InfoHandler.AddType("Map");
        fbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        fbxHandler.Dispose();

    }


	public static void ExportMinimalMap(Tag<D2Class_07878080> map, EExportTypeFlag exportTypeFlag, Activity activity, string bubbleName)
    {
        FbxHandler fbxHandler = new FbxHandler();

		string meshName = map.Hash.GetHashString();
		string savePath = ConfigHandler.GetExportSavePath() + $"/Maps/{activity.Header.LocationName}/";
		if (ConfigHandler.GetSingleFolderMapsEnabled())
		{
			savePath = ConfigHandler.GetExportSavePath() + $"/Maps/{activity.Header.LocationName}/{bubbleName}/";
		}
		fbxHandler.InfoHandler.SetMeshName(meshName);
        Directory.CreateDirectory(savePath);
        Directory.CreateDirectory(savePath + "/Dynamics");
    
        if(exportStatics)
        {
            Directory.CreateDirectory(savePath + "/Statics");
            ExportStatics(exportStatics, savePath, map);
        }

        ExtractDataTables(map, savePath, fbxHandler, exportTypeFlag);

        if (ConfigHandler.GetUnrealInteropEnabled())
        {
            fbxHandler.InfoHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
            AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Map, ConfigHandler.GetOutputTextureFormat(), ConfigHandler.GetSingleFolderMapsEnabled());
        }
        if (ConfigHandler.GetBlenderInteropEnabled())
        {
            AutomatedImporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedImporter.EImportType.Map, ConfigHandler.GetOutputTextureFormat());
        }

        fbxHandler.InfoHandler.AddType("Map");
        fbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        fbxHandler.Dispose();
    }

    public static void ExportTerrainMap(Tag<D2Class_07878080> map, Activity activity, string bubbleName)
    {
        FbxHandler fbxHandler = new FbxHandler();
        string meshName = map.Hash.GetHashString();
        string savePath = ConfigHandler.GetExportSavePath() + $"/Maps/{activity.Header.LocationName}/";
        if (ConfigHandler.GetSingleFolderMapsEnabled())
        {
            savePath = ConfigHandler.GetExportSavePath() + $"/Maps/{activity.Header.LocationName}/{bubbleName}/";
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
                    terrainArrangement.Terrain.LoadIntoFbxScene(fbxHandler, savePath, ConfigHandler.GetUnrealInteropEnabled() || ConfigHandler.GetS2ShaderExportEnabled(), terrainArrangement, ConfigHandler.GetSaveCBuffersEnabled());
                    if(exportStatics)
                    {
						Directory.CreateDirectory($"{savePath}/Statics/");
						if (source2Models)
                        {
							File.Copy("template.vmdl", $"{savePath}/Statics/{terrainArrangement.Terrain.Hash}_Terrain.vmdl", true);
                        }
                        FbxHandler staticHandler = new FbxHandler(false);
                        terrainArrangement.Terrain.LoadIntoFbxScene(staticHandler, savePath, ConfigHandler.GetUnrealInteropEnabled() || ConfigHandler.GetS2ShaderExportEnabled(), terrainArrangement, ConfigHandler.GetSaveCBuffersEnabled(), true);
                        staticHandler.ExportScene($"{savePath}/Statics/{terrainArrangement.Terrain.Hash}_Terrain.fbx");
                        staticHandler.Dispose();
                    }
                }
            });
        });

        if (ConfigHandler.GetBlenderInteropEnabled())
        {
            AutomatedImporter.SaveInteropBlenderPythonFile(savePath, meshName + "_Terrain", AutomatedImporter.EImportType.Terrain, ConfigHandler.GetOutputTextureFormat());
        }

        fbxHandler.InfoHandler.AddType("Terrain");
        fbxHandler.ExportScene($"{savePath}/{meshName}_Terrain.fbx");
        fbxHandler.Dispose();
    }
    
    private static void ExtractDataTables(Tag<D2Class_07878080> map, string savePath, FbxHandler fbxHandler, EExportTypeFlag exportTypeFlag)
    {
        FbxHandler dynamicHandler = new FbxHandler();
        dynamicHandler.InfoHandler.SetMeshName($"{map.Hash.GetHashString()}_Dynamics");
        dynamicHandler.InfoHandler.AddType("Dynamics");
        //FbxHandler dynamicPoints = new FbxHandler(false);
       
        Parallel.ForEach(map.Header.DataTables, data =>
        {
            data.DataTable.Header.DataEntries.ForEach(entry =>
            {
                if (entry.DataResource is D2Class_C96C8080 staticMapResource)  // Static map
                {
                    if (exportTypeFlag == EExportTypeFlag.ArrangedMap)
                    {
                        staticMapResource.StaticMapParent.Header.StaticMap.LoadArrangedIntoFbxScene(fbxHandler); //Arranged because...arranged
                    }
                    else if (exportTypeFlag == EExportTypeFlag.Full || exportTypeFlag == EExportTypeFlag.Minimal) //No terrain on a minimal rip makes sense right?
                    {
                        staticMapResource.StaticMapParent.Header.StaticMap.LoadIntoFbxScene(fbxHandler, savePath, ConfigHandler.GetUnrealInteropEnabled() || ConfigHandler.GetS2ShaderExportEnabled(), ConfigHandler.GetSaveCBuffersEnabled());
                    }
                }
                else if(entry is D2Class_85988080 dynamicResource)
                {
                    dynamicHandler.AddDynamicToScene(dynamicResource, dynamicResource.Entity.Hash, savePath, ConfigHandler.GetUnrealInteropEnabled() || ConfigHandler.GetS2ShaderExportEnabled(), ConfigHandler.GetSaveCBuffersEnabled());
                    //dynamicPoints.AddDynamicPointsToScene(dynamicResource, dynamicResource.Entity.Hash, dynamicPoints);
                }
            });
        });
        dynamicHandler.ExportScene($"{savePath}/{map.Hash.GetHashString()}_Dynamics.fbx");
        dynamicHandler.Dispose();

        //dynamicPoints.ExportScene($"{savePath}/{map.Hash.GetHashString()}_DynamicPoints.fbx");
        //dynamicPoints.Dispose();
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
                            
                            //staticHandler.InfoHandler.SetMeshName(staticMeshName);
                            var staticmesh = part.Static.Load(ELOD.MostDetail);

                            staticHandler.AddStaticToScene(staticmesh, part.Static.Hash);

                            if(source2Models)
                            {
                                Source2Handler.SaveStaticVMDL($"{savePath}/Statics", staticMeshName, staticmesh);
                            }

                            staticHandler.ExportScene($"{savePath}/Statics/{staticMeshName}.fbx");
                            staticHandler.Dispose();
                        }//);
                    } 
                });
            });
        }
    }

    private static void ExportDynamics(string savePath, Tag<D2Class_07878080> map)
    {
        Parallel.ForEach(map.Header.DataTables, data =>
        {
            data.DataTable.Header.DataEntries.ForEach(entry =>
            {
                if (entry is D2Class_85988080 dynamicResource)
                {
                    if (dynamicResource.Entity.HasGeometry())
                    {
                        FbxHandler singleDynamicHandler = new FbxHandler(false);
                        singleDynamicHandler.AddDynamicToScene(dynamicResource, dynamicResource.Entity.Hash, savePath, ConfigHandler.GetUnrealInteropEnabled() || ConfigHandler.GetS2ShaderExportEnabled(), ConfigHandler.GetSaveCBuffersEnabled());

                        if (source2Models)
                        {
                            Source2Handler.SaveEntityVMDL($"{savePath}/Dynamics", dynamicResource.Entity);
						}

                        singleDynamicHandler.ExportScene($"{savePath}/Dynamics/{dynamicResource.Entity.Hash}.fbx");
                        singleDynamicHandler.Dispose();
                    }
                }
            });
        });
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