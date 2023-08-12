using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Activity;
using Tiger.Schema.Entity;

namespace Charm;

public partial class ActivityMapView : UserControl
{
    public ActivityMapView()
    {
        InitializeComponent();
    }

    public void LoadUI(IActivity activity)
    {
        MapList.ItemsSource = GetMapList(activity);
        ExportControl.SetExportFunction(ExportFull, (int)ExportTypeFlag.Full | (int)ExportTypeFlag.Minimal | (int)ExportTypeFlag.ArrangedMap | (int)ExportTypeFlag.TerrainOnly, true);
        ExportControl.SetExportInfo(activity.FileHash);
    }

    private ObservableCollection<DisplayBubble> GetMapList(IActivity activity)
    {
        var maps = new ObservableCollection<DisplayBubble>();
        foreach (var bubble in activity.EnumerateBubbles())
        {
            DisplayBubble displayMap = new();
            displayMap.Name = bubble.Name;
            displayMap.Hash = bubble.MapReference.TagData.ChildMapReference.Hash;
            maps.Add(displayMap);
        }
        // foreach (var mapEntry in activity.TagData.Unk50)
        // {
        //     foreach (var mapReferences in mapEntry.MapReferences)
        //     {
        //         // idk why this can happen but it can, some weird stuff with h64
        //         // for the child map reference, ive only seen it once so far but the hash for it was just FFFFFFFF in the map reference file
        //         if (mapReferences.MapReference is null || mapReferences.MapReference.TagData.ChildMapReference == null)
        //             continue;
        //         DisplayBubble displayMap = new DisplayBubble();
        //         displayMap.Name = $"{mapEntry.BubbleName} ({mapEntry.LocationName})";  // assuming Unk10 is 0F978080 or 0B978080
        //         displayMap.Hash = mapReferences.MapReference.TagData.ChildMapReference.Hash;
        //         maps.Add(displayMap);
        //     }
        // }
        return maps;
    }

    private void GetBubbleContentsButton_OnClick(object sender, RoutedEventArgs e)
    {
        FileHash hash = new FileHash((sender as Button).Tag as string);
        Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(hash);
        PopulateStaticList(bubbleMaps);
    }

    private void StaticMapPart_OnCheck(object sender, RoutedEventArgs e)
    {
        FileHash hash = new FileHash((sender as CheckBox).Tag as string);
        Tag<SMapContainer> map = FileResourcer.Get().GetSchemaTag<SMapContainer>(hash);

        foreach (DisplayStaticMap item in StaticList.Items)
        {
            if (item.Name == "Select all")
                continue;

            // if (item.Selected)
            // {
            //     PopulateDynamicsList(map);
            // }
        }
    }

    private void PopulateStaticList(Tag<SBubbleDefinition> bubbleMaps)
    {
        ConcurrentBag<DisplayStaticMap> items = new ConcurrentBag<DisplayStaticMap>();
        Parallel.ForEach(bubbleMaps.TagData.MapResources, m =>
        {
            if (m.MapContainer.TagData.MapDataTables.Count > 1)
            {
                Tag<SMapDataTable> mapDataTable = m.MapContainer.TagData.MapDataTables[1].MapDataTable;
                if (mapDataTable.TagData.DataEntries.Count > 0)
                {
                    StaticMapData? tag = mapDataTable.TagData.DataEntries[0].DataResource.GetValue(mapDataTable.GetReader())?.StaticMapParent.TagData.StaticMap;
                    if (tag == null)
                    {
                        return; // todo sk broke this
                    }
                    items.Add(new DisplayStaticMap
                    {
                        Hash = m.MapContainer.Hash,
                        Name = $"{m.MapContainer.Hash}: {tag.TagData.Instances.Count} instances, {tag.TagData.Statics.Count} uniques",
                        Instances = tag.TagData.Instances.Count
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

    private void PopulateDynamicsList(Tag<SMapContainer> map)//(Tag<SBubbleDefinition> bubbleMaps)
    {

        ConcurrentBag<DisplayDynamicMap> items = new ConcurrentBag<DisplayDynamicMap>();
        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            data.MapDataTable.TagData.DataEntries.ForEach(entry =>
            {
                if (entry is SMapDataEntry dynamicResource)
                {
                    Entity entity = FileResourcer.Get().GetFile(typeof(Entity), dynamicResource.GetEntityHash());

                    if (entity.Model != null)
                    {
                        items.Add(new DisplayDynamicMap
                        {
                            Hash = dynamicResource.GetEntityHash(),
                            Name = $"{dynamicResource.GetEntityHash()}: {entity.Model.TagData.Meshes.Count} meshes",
                            Models = entity.Model.TagData.Meshes.Count
                        });
                    }
                    else
                    {
                        items.Add(new DisplayDynamicMap
                        {
                            Hash = dynamicResource.GetEntityHash(),
                            Name = $"{dynamicResource.GetEntityHash()}: 0 meshes",
                            Models = 0
                        });
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
        // todo figure out how to make this work
        IActivity activity = FileResourcer.Get().GetFileInterface<IActivity>(info.Hash);
        Log.Info($"Exporting activity data name: {PackageResourcer.Get().GetActivityName(activity.FileHash)}, hash: {activity.FileHash}, export type {info.ExportType.ToString()}");
        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Hidden;
        });
        var maps = new List<Tag<SMapContainer>>();
        bool bSelectAll = false;
        foreach (DisplayStaticMap item in StaticList.Items)
        {
            if (item.Selected && item.Name == "Select all")
            {
                bSelectAll = true;
                Log.Info($"Selected all maps");
            }
            else
            {
                if (item.Selected || bSelectAll)
                {
                    maps.Add(FileResourcer.Get().GetSchemaTag<SMapContainer>(item.Hash));
                    Log.Info($"Selected map: {item.Hash}");
                }
            }
        }

        if (maps.Count == 0)
        {
            Log.Error("No maps selected for export.");
            MessageBox.Show("No maps selected for export.");
            return;
        }

        List<string> mapStages = maps.Select((x, i) => $"exporting {i + 1}/{maps.Count}").ToList();
        MainWindow.Progress.SetProgressStages(mapStages);
        // MainWindow.Progress.SetProgressStages(new List<string> { "exporting activity map data parallel" });
        Parallel.ForEach(maps, map =>
        {
            if (info.ExportType == ExportTypeFlag.Full)
            {
                MapView.ExportFullMap(map);
                MapView.ExportTerrainMap(map);
            }
            else if (info.ExportType == ExportTypeFlag.TerrainOnly)
            {
                MapView.ExportTerrainMap(map);
            }
            else if (info.ExportType == ExportTypeFlag.Minimal)
            {
                MapView.ExportMinimalMap(map, info.ExportType);
            }
            else
            {
                MapView.ExportMinimalMap(map, info.ExportType);
            }

            MainWindow.Progress.CompleteStage();
        });
        // MapView.ExportFullMap(staticMapData);
        // MainWindow.Progress.CompleteStage();

        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Visible;
        });
        Log.Info($"Exported activity data name: {PackageResourcer.Get().GetActivityName(activity.FileHash)}, hash: {activity.FileHash}");
        MessageBox.Show("Activity map data exported completed.");
    }

    private async void StaticMap_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as DisplayStaticMap;
        MapControl.Clear();
        Log.Info($"Loading UI for static map hash: {dc.Name}");
        MapControl.Visibility = Visibility.Hidden;
        var lod = MapControl.ModelView.GetSelectedLod();
        if (dc.Name == "Select all")
        {
            var items = StaticList.Items.Cast<DisplayStaticMap>().Where(x => x.Name != "Select all");
            List<string> mapStages = items.Select(x => $"loading to ui: {x.Hash}").ToList();
            if (mapStages.Count == 0)
            {
                Log.Error("No maps selected for export.");
                MessageBox.Show("No maps selected for export.");
                return;
            }
            MainWindow.Progress.SetProgressStages(mapStages);
            await Task.Run(() =>
            {
                foreach (DisplayStaticMap item in items)
                {
                    MapControl.LoadMap(new FileHash(item.Hash), lod);
                    MainWindow.Progress.CompleteStage();
                }
            });
        }
        else
        {
            var fileHash = new FileHash(dc.Hash);
            MainWindow.Progress.SetProgressStages(new List<string> { fileHash });
            // cant do this rn bc of lod problems with dupes
            // MapControl.ModelView.SetModelFunction(() => MapControl.LoadMap(fileHash, MapControl.ModelView.GetSelectedLod()));
            await Task.Run(() =>
            {
                MapControl.LoadMap(fileHash, lod);
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
}
