using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Activity;

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
        ExportControl.SetExportFunction(ExportFull, (int)ExportTypeFlag.Full | (int)ExportTypeFlag.ArrangedMap, true);
        ExportControl.SetExportInfo(activity.FileHash);
    }

    private ObservableCollection<DisplayBubble> GetMapList(IActivity activity)
    {
        var maps = new ObservableCollection<DisplayBubble>();
        foreach (var bubble in activity.EnumerateBubbles())
        {
            DisplayBubble displayMap = new();
            displayMap.Name = bubble.Name;
            displayMap.Hash = bubble.ChildMapReference.Hash;
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

    private void PopulateStaticList(Tag<SBubbleDefinition> bubbleMaps)
    {
        ConcurrentBag<DisplayStaticMap> items = new ConcurrentBag<DisplayStaticMap>();

        if (Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            Parallel.ForEach(bubbleMaps.TagData.MapResources, m =>
            {
                if (m.GetMapContainer().TagData.MapDataTables.Count > 1)
                {
                    Tag<SMapDataTable> mapDataTable = m.GetMapContainer().TagData.MapDataTables[1].MapDataTable;
                    if (mapDataTable.TagData.DataEntries.Count > 0)
                    {
                        StaticMapData? tag = mapDataTable.TagData.DataEntries[0].DataResource.GetValue(mapDataTable.GetReader())?.StaticMapParent.TagData.StaticMap;
                        if (tag == null)
                            return; // todo sk broke this

                        items.Add(new DisplayStaticMap
                        {
                            Hash = m.GetMapContainer().Hash,
                            Name = $"{m.GetMapContainer().Hash}: {tag.TagData.Instances.Count} instances, {tag.TagData.Statics.Count} uniques",
                            Instances = tag.TagData.Instances.Count
                        });
                    }
                }
            });
        }
        else
        {
            Parallel.ForEach(bubbleMaps.TagData.MapResources, m =>
            {
                if (m.GetMapContainer().TagData.MapDataTables.Count > 1)
                {
                    foreach (var dataTable in m.GetMapContainer().TagData.MapDataTables)
                    {
                        foreach (var entry in dataTable.MapDataTable.TagData.DataEntries)
                        {
                            if (entry.DataResource.GetValue(dataTable.MapDataTable.GetReader()) is SMapDataResource resource)
                            {
                                if (resource.StaticMapParent.TagData.StaticMap.TagData.D1StaticMapData is not null)
                                {
                                    var d1StaticMapData = resource.StaticMapParent.TagData.StaticMap.TagData.D1StaticMapData;
                                    items.Add(new DisplayStaticMap
                                    {
                                        Hash = d1StaticMapData.Hash,
                                        Name = $"{d1StaticMapData.Hash}"
                                        //Name = $"{m.GetMapContainer().Hash}: {tag.TagData.Instances.Count} instances, {tag.TagData.Statics.Count} uniques",
                                        //Instances = tag.TagData.Instances.Count
                                    });
                                }
                            }
                        }
                    }
                }
            });
        }

        var sortedItems = new List<DisplayStaticMap>(items);
        sortedItems.Sort((a, b) => b.Instances.CompareTo(a.Instances));
        sortedItems.Insert(0, new DisplayStaticMap
        {
            Name = "Select all"
        });
        StaticList.ItemsSource = sortedItems;
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

        List<string> mapStages = maps.Select((x, i) => $"Exporting {i + 1}/{maps.Count}").ToList();
        mapStages.Add("Finishing Export");
        MainWindow.Progress.SetProgressStages(mapStages);
        // MainWindow.Progress.SetProgressStages(new List<string> { "exporting activity map data parallel" });
        Parallel.ForEach(maps, map =>
        {
            MapView.ExportFullMap(map, info.ExportType);
            MainWindow.Progress.CompleteStage();
        });
        // MapView.ExportFullMap(staticMapData);
        // MainWindow.Progress.CompleteStage();

        Tiger.Exporters.Exporter.Get().Export();

        MainWindow.Progress.CompleteStage();

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
                Log.Error("No maps available for viewing.");
                MessageBox.Show("No maps available for viewing.");
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
