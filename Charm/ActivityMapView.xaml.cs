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
using Field;
using Field.General;
using Serilog;

namespace Charm;

public partial class ActivityMapView : UserControl
{
    private readonly ILogger _activityLog = Log.ForContext<ActivityMapView>();

    public ActivityMapView()
    {
        InitializeComponent();
    } 
    
    public void LoadUI(Activity activity)
    {
        MapList.ItemsSource = GetMapList(activity);
        ExportControl.SetExportFunction(ExportFull, true);
        ExportControl.SetExportInfo(activity.Hash);
    }

    private ObservableCollection<DisplayBubble> GetMapList(Activity activity)
    {
        var maps = new ObservableCollection<DisplayBubble>();
        foreach (var mapEntry in activity.Header.Unk50)
        {
            // idk why this can happen but it can, some weird stuff with h64
            // for the child map reference, ive only seen it once so far but the hash for it was just FFFFFFFF in the map reference file
            if (mapEntry.MapReference is null || mapEntry.MapReference.Header.ChildMapReference == null)
                continue;
            DisplayBubble displayMap = new DisplayBubble();
            displayMap.Name = $"{mapEntry.BubbleName} ({mapEntry.Unk10.BubbleName})";  // assuming Unk10 is 0F978080 or 0B978080
            displayMap.Hash = mapEntry.MapReference.Header.ChildMapReference.Hash;
            maps.Add(displayMap);
        }
        return maps;
    }

    private void GetBubbleContentsButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagHash hash = new TagHash((sender as Button).Tag as string);
        Tag<D2Class_01878080> bubbleMaps = PackageHandler.GetTag<D2Class_01878080>(hash);
        PopulateStaticList(bubbleMaps);
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
                        Hash = tag.Hash,
                        Name = $"{tag.Hash}: {tag.Header.Instances.Count} instances, {tag.Header.Statics.Count} uniques",
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

    public async void ExportFull(ExportInfo info)
    {
        Activity activity = PackageHandler.GetTag(typeof(Activity), new TagHash(info.Hash));
        _activityLog.Debug($"Exporting activity data name: {PackageHandler.GetActivityName(activity.Hash)}, hash: {activity.Hash}");
        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Hidden;
        });
        var maps = new List<StaticMapData>();
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
                    maps.Add(PackageHandler.GetTag(typeof(StaticMapData), new TagHash(item.Hash)));
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
        Parallel.ForEach(maps, staticMapData =>
        {
            MapView.ExportMinimalMap(staticMapData);
            // MapView.ExportFullMap(staticMapData);
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