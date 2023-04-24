using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Field;
using Field.General;
using Field.Entities;
using Serilog;
using Assimp.Unmanaged;
using static Internal.Fbx.FbxAccumulatorEntry;
using Field.Models;
using System.IO;
using System.Xml.Linq;
using Internal.Fbx;

namespace Charm;

public partial class ActivityMapView : UserControl
{
    private readonly ILogger _activityLog = Log.ForContext<ActivityMapView>();
    private string _activeBubble = "";
	private FbxHandler _globalFbxHandler = null;

	public ActivityMapView()
    {
        InitializeComponent();
		_globalFbxHandler = new FbxHandler(false);
	} 
    
    public void LoadUI(Activity activity)
    {
        MapList.ItemsSource = GetMapList(activity);
        ExportControl.SetExportFunction(ExportFull, (int)EExportTypeFlag.Full | (int)EExportTypeFlag.Minimal | (int)EExportTypeFlag.ArrangedMap | (int)EExportTypeFlag.TerrainOnly, true);
        ExportControl.SetExportInfo(activity.Hash);
    }

    private ObservableCollection<DisplayBubble> GetMapList(Activity activity)
    {
        var maps = new ObservableCollection<DisplayBubble>();
        foreach (var mapEntry in activity.Header.Unk50)
        {
            foreach (var mapReferences in mapEntry.MapReferences)
            {
                // idk why this can happen but it can, some weird stuff with h64
                // for the child map reference, ive only seen it once so far but the hash for it was just FFFFFFFF in the map reference file
                if (mapReferences.MapReference is null || mapReferences.MapReference.Header.ChildMapReference == null)
                    continue;
                DisplayBubble displayMap = new DisplayBubble();
                displayMap.Name = $"{mapEntry.BubbleName} ({mapEntry.LocationName})";  // assuming Unk10 is 0F978080 or 0B978080
                displayMap.Hash = $"{mapReferences.MapReference.Header.ChildMapReference.Hash}|{mapEntry.BubbleName}";
                maps.Add(displayMap);   
            }
        }
        return maps;
    }

    private void GetBubbleContentsButton_OnClick(object sender, RoutedEventArgs e)
    {
		string tag = (sender as Button).Tag as string;
		string[] values = tag.Split('|'); //very hacky way of getting both the hash and bubble name from the button tag, tried doing some Tag<> stuff but couldnt figure it out
		string hash = values[0];
		string name = values[1];

        _activeBubble = name;
        ActiveBubble.Text = $"{_activeBubble}";

		Tag<D2Class_01878080> bubbleMaps = PackageHandler.GetTag<D2Class_01878080>(new TagHash(hash));
		PopulateStaticList(bubbleMaps);
	}

    private void StaticMapPart_OnCheck(object sender, RoutedEventArgs e)
    {
        TagHash hash = new TagHash((sender as CheckBox).Tag as string);
        Tag<D2Class_07878080> map = PackageHandler.GetTag<D2Class_07878080>(hash);
        
        foreach (DisplayStaticMap item in StaticList.Items)
        {
            if(item.Name == "Select all")
                continue;

            if (item.Selected)
            {
				if (map == null)
					continue;

				PopulateDynamicsList(map);
                ActiveEnts.Text = $"{map.Hash} Entities";
            }
        }
    }

    private void PopulateStaticList(Tag<D2Class_01878080> bubbleMaps)
    {
        ConcurrentBag<DisplayStaticMap> items = new ConcurrentBag<DisplayStaticMap>();
        Parallel.ForEach(bubbleMaps.Header.MapResources, m =>
        {
            if (m.MapResource.Header.DataTables.Count > 1)
            {
                if (m.MapResource.Header.DataTables[1].DataTable.Header.DataEntries.Count > 0)
                {
                    StaticMapData tag = m.MapResource.Header.DataTables[1].DataTable.Header.DataEntries[0].DataResource.StaticMapParent.Header.StaticMap;
                    items.Add(new DisplayStaticMap
                    {
                        Hash = m.MapResource.Hash,
                        Name = $"{m.MapResource.Hash}: {tag.Header.Instances.Count} instances, {tag.Header.Statics.Count} uniques",
                        Instances = tag.Header.Instances.Count
                    });     
                }
            }
        });
        var sortedItems = new List<DisplayStaticMap>(items);
        sortedItems.Sort((a, b) => b.Instances.CompareTo(a.Instances));
        sortedItems.Insert(0, new DisplayStaticMap
        {
            Name = "Select all"
        });
        StaticList.ItemsSource = sortedItems;
    }

    private void PopulateDynamicsList(Tag<D2Class_07878080> map)//(Tag<D2Class_01878080> bubbleMaps)
    {
        ConcurrentBag<DisplayDynamicMap> items = new ConcurrentBag<DisplayDynamicMap>();   
        Parallel.ForEach(map.Header.DataTables, data =>
        {
            data.DataTable.Header.DataEntries.ForEach(entry =>
            {
                if(entry is D2Class_85988080 dynamicResource)
                {    
                    if (!items.Contains(new DisplayDynamicMap { Hash = dynamicResource.Entity.Hash }))
                    {
                        if (dynamicResource.Entity.HasGeometry())
                        {
                            items.Add(new DisplayDynamicMap
                            {
                                Name = $"Entity {dynamicResource.Entity.Hash}",
                                Hash = dynamicResource.Entity.Hash
                            });
                        }
                        //else
                        //{
                        //    items.Add(new DisplayDynamicMap
                        //    {
                        //        Hash = dynamicResource.Entity.Hash,
                        //        Name = $"{dynamicResource.Entity.Hash}: 0 meshes",
                        //        Models = 0
                        //    });
                        //}
                    }
                }
            });
        });
        var sortedItems = new List<DisplayDynamicMap>(items);
        sortedItems.Sort((a, b) => b.Models.CompareTo(a.Models));
        sortedItems.Insert(0, new DisplayDynamicMap
        {
            Name = "Select all"
        });
        DynamicsList.ItemsSource = sortedItems;
    }

    public async void ExportFull(ExportInfo info)
    {
        Activity activity = PackageHandler.GetTag(typeof(Activity), new TagHash(info.Hash));
        _activityLog.Debug($"Exporting activity data name: {PackageHandler.GetActivityName(activity.Hash)}, hash: {activity.Hash}");
        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Hidden;
        });
        var maps = new List<Tag<D2Class_07878080>>();
        bool bSelectAll = false;
        foreach (DisplayStaticMap item in StaticList.Items)
        {
            if (item.Selected && item.Name == "Select all")
            {
                bSelectAll = true;
            }
            else
            {
                if (item.Selected || bSelectAll)
                {
                    maps.Add(PackageHandler.GetTag<D2Class_07878080>(new TagHash(item.Hash)));
                }
            }
        }

        if (maps.Count == 0)
        {
            _activityLog.Error("No maps selected for export.");
            MessageBox.Show("No maps selected for export.");
            return;
        }

        List<string> mapStages = maps.Select((x, i) => $"exporting {i+1}/{maps.Count}").ToList();
        MainWindow.Progress.SetProgressStages(mapStages);
        // MainWindow.Progress.SetProgressStages(new List<string> { "exporting activity map data parallel" });
        Parallel.ForEach(maps, map =>
        {
            if (info.ExportType == EExportTypeFlag.Full)
            {
                MapView.ExportFullMap(map, activity, _activeBubble);
                MapView.ExportTerrainMap(map, activity, _activeBubble);
            }
            else if (info.ExportType == EExportTypeFlag.TerrainOnly)
            {
                MapView.ExportTerrainMap(map, activity, _activeBubble);
            }
            else if (info.ExportType == EExportTypeFlag.Minimal)
            {
                MapView.ExportMinimalMap(map, info.ExportType, activity, _activeBubble);
            }
            else
            {
                MapView.ExportMinimalMap(map, info.ExportType, activity, _activeBubble);
            }
            
            MainWindow.Progress.CompleteStage();
        });
        // MapView.ExportFullMap(staticMapData);
            // MainWindow.Progress.CompleteStage();

        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Visible;
        });
        _activityLog.Information($"Exported activity data name: {PackageHandler.GetActivityName(activity.Hash)}, hash: {activity.Hash}");
        MessageBox.Show("Activity map data exported completed.");
    }

    private async void StaticMap_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as DisplayStaticMap;
        MapControl.Clear();
        _activityLog.Debug($"Loading UI for static map hash: {dc.Name}");
        MapControl.Visibility = Visibility.Hidden;
        var lod = MapControl.ModelView.GetSelectedLod();
        if (dc.Name == "Select all")
        {
            var items = StaticList.Items.Cast<DisplayStaticMap>().Where(x => x.Name != "Select all");
            List<string> mapStages = items.Select(x => $"loading to ui: {x.Hash}").ToList();
            if (mapStages.Count == 0)
            {
                _activityLog.Error("No maps selected for export.");
                MessageBox.Show("No maps selected for export.");
                return;
            }
            MainWindow.Progress.SetProgressStages(mapStages);
            await Task.Run(() =>
            {
                foreach (DisplayStaticMap item in items)
                {
                    MapControl.LoadMap(new TagHash(item.Hash), lod);
                    MainWindow.Progress.CompleteStage();
                }
            });
        }
        else
        {
            var tagHash = new TagHash(dc.Hash);
            MainWindow.Progress.SetProgressStages(new List<string> {tagHash });
            // cant do this rn bc of lod problems with dupes
            // MapControl.ModelView.SetModelFunction(() => MapControl.LoadMap(tagHash, MapControl.ModelView.GetSelectedLod()));
            await Task.Run(() =>
            {
                MapControl.LoadMap(tagHash, lod); 
                MainWindow.Progress.CompleteStage();
            });
        }
        MapControl.Visibility = Visibility.Visible;
    }

    private async void Entity_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as DisplayDynamicMap;
        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Hidden;
        });

        if (dc.Name == "Select all")
        {
            var items = DynamicsList.Items.Cast<DisplayDynamicMap>().Where(x => x.Name != "Select all");
            List<string> entStages = items.Select(x => $"exporting entity: {x.Hash}").ToList();
            if (entStages.Count == 0)
            {
                _activityLog.Error("No entities selected for export.");
                MessageBox.Show("No entities selected for export.");
                return;
            }
            MainWindow.Progress.SetProgressStages(entStages);
            await Task.Run(() =>
            {
                foreach (DisplayDynamicMap item in items)
                {
                    Entity ent = new Entity(new TagHash(item.Hash));
                    FbxHandler entHandler = new FbxHandler();
                    entHandler.InfoHandler.SetMeshName(ent.Hash.GetHashString());

                    List<FbxNode> skeletonNodes = new List<FbxNode>();
                    if (ent.Skeleton != null)
                    {
                        skeletonNodes = entHandler.AddSkeleton(ent.Skeleton.GetBoneNodes());
                    }

                    Log.Debug($"Exporting entity model name: {ent.Hash}");
                    string savePath = ConfigHandler.GetExportSavePath();
                    string meshName = string.Join("_", ent.Hash.ToString().Split(Path.GetInvalidFileNameChars()));
                    
                    savePath += $"/{meshName}";
                    
                    Directory.CreateDirectory(savePath);

                   
                    var dynamicParts = ent.Load(ELOD.MostDetail, true);
                    entHandler.AddEntityToScene(ent, dynamicParts, ELOD.MostDetail, skeletonNodes);

                    ent.SaveMaterialsFromParts(savePath, dynamicParts, ConfigHandler.GetUnrealInteropEnabled() || ConfigHandler.GetS2ShaderExportEnabled(), ConfigHandler.GetSaveCBuffersEnabled());
                    ent.SaveTexturePlates(savePath);

                    entHandler.InfoHandler.AddType("Entity");
                    entHandler.ExportScene($"{savePath}/{meshName}.fbx");
                    entHandler.Dispose();
                    Log.Information($"Exported entity model {ent.Hash} to {savePath.Replace('\\', '/')}/");

                    MainWindow.Progress.CompleteStage();
                }
            });
        }
        else
        {
            var tagHash = new TagHash(dc.Hash);
            MainWindow.Progress.SetProgressStages(new List<string> { $"exporting entity: {tagHash}" });
            // cant do this rn bc of lod problems with dupes
            // MapControl.ModelView.SetModelFunction(() => MapControl.LoadMap(tagHash, MapControl.ModelView.GetSelectedLod()));
            await Task.Run(() =>
            {
                Entity ent = new Entity(new TagHash(dc.Hash));
                FbxHandler entHandler = new FbxHandler();
                entHandler.InfoHandler.SetMeshName(ent.Hash.GetHashString());

                List<FbxNode> skeletonNodes = new List<FbxNode>();
                if (ent.Skeleton != null)
                {
                    skeletonNodes = entHandler.AddSkeleton(ent.Skeleton.GetBoneNodes());
                }

                Log.Debug($"Exporting entity model name: {ent.Hash}");
                string savePath = ConfigHandler.GetExportSavePath();
                string meshName = string.Join("_", ent.Hash.ToString().Split(Path.GetInvalidFileNameChars()));

                savePath += $"/{meshName}";

                Directory.CreateDirectory(savePath);

                var dynamicParts = ent.Load(ELOD.MostDetail, true);
                entHandler.AddEntityToScene(ent, dynamicParts, ELOD.MostDetail, skeletonNodes);

                ent.SaveMaterialsFromParts(savePath, dynamicParts, ConfigHandler.GetUnrealInteropEnabled() || ConfigHandler.GetS2ShaderExportEnabled(), ConfigHandler.GetSaveCBuffersEnabled());
                ent.SaveTexturePlates(savePath);

                entHandler.InfoHandler.AddType("Entity");
                entHandler.ExportScene($"{savePath}/{meshName}.fbx");
                entHandler.Dispose();
                Log.Information($"Exported entity model {ent.Hash} to {savePath.Replace('\\', '/')}/");

                MainWindow.Progress.CompleteStage();
            });
        }
        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Visible;
        });
    }

	private async void EntityView_OnClick(object sender, RoutedEventArgs e)
	{
		var s = sender as Button;
		var dc = s.DataContext as DisplayDynamicMap;
		MapControl.Clear();
		_activityLog.Debug($"Loading UI for entity: {dc.Name}");
		MapControl.Visibility = Visibility.Hidden;
		var lod = MapControl.ModelView.GetSelectedLod();
		if (dc.Name == "Select all")
		{
			//var items = StaticList.Items.Cast<DisplayStaticMap>().Where(x => x.Name != "Select all");
			//List<string> mapStages = items.Select(x => $"loading to ui: {x.Hash}").ToList();
			//if (mapStages.Count == 0)
			//{
			//	_activityLog.Error("No maps selected for export.");
			//	MessageBox.Show("No maps selected for export.");
			//	return;
			//}
			//MainWindow.Progress.SetProgressStages(mapStages);
			//await Task.Run(() =>
			//{
			//	foreach (DisplayStaticMap item in items)
			//	{
			//		MapControl.LoadMap(new TagHash(item.Hash), lod);
			//		MainWindow.Progress.CompleteStage();
			//	}
			//});
		}
		else
		{
			var tagHash = new TagHash(dc.Hash);
			MainWindow.Progress.SetProgressStages(new List<string> { tagHash });
			// cant do this rn bc of lod problems with dupes
			// MapControl.ModelView.SetModelFunction(() => MapControl.LoadMap(tagHash, MapControl.ModelView.GetSelectedLod()));
			await Task.Run(() =>
			{
				Entity ent = new Entity(new TagHash(dc.Hash));
				MapControl.LoadEntity(ent, _globalFbxHandler);
				MainWindow.Progress.CompleteStage();
			});
		}
		MapControl.Visibility = Visibility.Visible;
	}

	public void Dispose()
    {
        MapControl.Dispose();
    }
}

public class DisplayBubble
{
    public string Name { get; set; }
    public string Hash { get; set; }
}

public class DisplayStaticMap
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public int Instances { get; set; }
    
    public bool Selected { get; set; }
}

public class DisplayDynamicMap
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public int Models { get; set; }
    
    public bool Selected { get; set; }

    public override bool Equals(object obj)
    {
        var other = obj as DisplayDynamicMap;
        return other != null && Hash == other.Hash;
    }

    public override int GetHashCode()
    {
        return Hash?.GetHashCode() ?? 0;
    }
}