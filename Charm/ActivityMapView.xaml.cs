using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Arithmic;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Activity;
using static Charm.APIItemView;

namespace Charm;

public partial class ActivityMapView : UserControl
{
    private IActivity _currentActivity;
    private DisplayBubble _currentBubble;
    private string _destinationName;

    private APITooltip ToolTip;

    public ActivityMapView()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        if (ConfigSubsystem.Get().GetAnimatedBackground())
        {
            SpinnerShader _spinner = new SpinnerShader();
            Spinner.Effect = _spinner;
            SizeChanged += _spinner.OnSizeChanged;
            _spinner.ScreenWidth = (float)ActualWidth;
            _spinner.ScreenHeight = (float)ActualHeight;
            _spinner.Scale = new(0, 0);
            _spinner.Offset = new(-1, -1);
            SpinnerContainer.Visibility = Visibility.Visible;
        }

        if (ToolTip is null)
        {
            ToolTip = new();
            Panel.SetZIndex(ToolTip, 50);
            MainContainer.Children.Add(ToolTip);
        }
    }

    private void ExportButton_MouseEnter(object sender, MouseEventArgs e)
    {
        ToolTip.ActiveItem = (sender as FrameworkElement);
        string[] text = (sender as FrameworkElement).Tag.ToString().Split(":");

        PlugItem plugItem = new()
        {
            Name = $"{text[0]}",
            Description = $"{text[1]}",
            PlugStyle = DestinySocketCategoryStyle.Reusable
        };

        ToolTip.MakeTooltip(plugItem);
    }

    public void ExportButton_MouseLeave(object sender, MouseEventArgs e)
    {
        ToolTip.ClearTooltip();
        ToolTip.ActiveItem = null;
    }

    public void LoadUI(IActivity activity)
    {
        _destinationName = activity.DestinationName;
        _currentActivity = activity;

        MapList.ItemsSource = GetMapList(activity);
        ExportControl.SetExportFunction(ExportFull, (int)ExportTypeFlag.Full, true); //| (int)ExportTypeFlag.ArrangedMap, true);
        ExportControl.SetExportInfo(activity.FileHash);

        QuickControls.Visibility = Visibility.Hidden;
        ExportControl.Visibility = Visibility.Hidden;

        if (Strategy.IsD1() || Strategy.IsPreBL())
            ActivityEntsButton.Visibility = Visibility.Hidden;
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
        return maps;
    }

    private async void GetBubbleContentsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var bubble = (sender as ToggleButton).DataContext as DisplayBubble;
        _currentBubble = bubble;
        FileHash hash = new FileHash(bubble.Hash);

        Dispatcher.Invoke(() => MapControl.Visibility = Visibility.Hidden);
        MainWindow.Progress.SetProgressStages(new() { $"Loading Map Parts for {bubble.Name}" });

        Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(hash);
        await Task.Run(() => PopulateStaticList(bubbleMaps));

        MainWindow.Progress.CompleteStage();
        Dispatcher.Invoke(() => MapControl.Visibility = Visibility.Visible);
        QuickControls.Visibility = Visibility.Visible;
        ExportControl.Visibility = Visibility.Visible;
    }

    private void PopulateStaticList(Tag<SBubbleDefinition> bubbleMaps)
    {
        ConcurrentBag<DisplayStaticMap> items = new ConcurrentBag<DisplayStaticMap>();
        Parallel.ForEach(bubbleMaps.TagData.MapResources, m =>
        {
            foreach (var dataTable in m.GetMapContainer().TagData.MapDataTables)
            {
                foreach (var entry in dataTable.MapDataTable.TagData.DataEntries)
                {
                    if (entry.DataResource.GetValue(dataTable.MapDataTable.GetReader()) is SMapDataResource resource)
                    {
                        resource.StaticMapParent?.Load();
                        if (resource.StaticMapParent is null || resource.StaticMapParent.TagData.StaticMap is null)
                            continue;

                        var tag = resource.StaticMapParent.TagData.StaticMap;
                        if (Strategy.IsD1())
                        {
                            int instanceCount = tag.TagData.D1StaticMapData != null ? tag.TagData.D1StaticMapData.TagData.InstanceCounts : tag.TagData.Decals.Count;
                            items.Add(new DisplayStaticMap
                            {
                                Hash = m.GetMapContainer().Hash,
                                Name = $"{m.GetMapContainer().Hash}: {instanceCount} instances",
                                Instances = instanceCount
                            });
                        }
                        else
                        {
                            items.Add(new DisplayStaticMap
                            {
                                Hash = m.GetMapContainer().Hash,
                                Name = $"{m.GetMapContainer().Hash}: {tag.TagData.Instances.Count} instances, {tag.TagData.Statics.Count} uniques",
                                Instances = tag.TagData.Instances.Count
                            });
                        }
                    }
                }
            }
        });

        var sortedItems = new List<DisplayStaticMap>(items);
        sortedItems.Sort((a, b) => b.Instances.CompareTo(a.Instances));
        sortedItems.Insert(0, new DisplayStaticMap
        {
            Name = "Select all"
        });

        // Shouldnt be a problem in D2, but in D1 a map container can have multiple static map parents
        // it still exports fine (I think) but having multiple of the same map container entries can cause info.cfg read/write crashes
        Dispatcher.Invoke(() =>
        {
            StaticList.ItemsSource = sortedItems.DistinctBy(x => x.Hash);
        });
    }

    private async void QuickControl_OnClick(object sender, RoutedEventArgs e)
    {
        int exportType = Int32.Parse(((sender as Button).DataContext as string));
        switch (exportType)
        {
            case 0: // All Map
                await Task.Run(() => ExportStaticMap());
                goto case 2;

            case 1: // Static Map
                await Task.Run(() => ExportStaticMap());
                break;

            case 2: // Map Resources
                await Task.Run(() => ExportResources());
                break;

            case 3: // Activity Entities
                var maps = new ConcurrentDictionary<FileHash, List<FileHash>>();
                var entries = _currentActivity.EnumerateActivityEntities().Where(x => x.BubbleName == _currentBubble.Name).ToList();

                if (Strategy.IsPostBL() || Strategy.IsBL())
                {
                    var tag = (_currentActivity as Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402.Activity).TagData.AmbientActivity;
                    if (tag is not null)
                    {
                        var ambient = FileResourcer.Get().GetFileInterface<IActivity>(tag.Hash);
                        entries.AddRange(ambient.EnumerateActivityEntities().Where(x => x.BubbleName == _currentBubble.Name).ToList());
                    }
                }

                foreach (var entry in entries)
                {
                    if (entry.DataTables.Count > 0)
                    {
                        var containerHash = entry.Hash;
                        if (!maps.ContainsKey(containerHash))
                            maps.TryAdd(containerHash, new());

                        foreach (var hash in entry.DataTables)
                        {
                            if (!maps[containerHash].Contains(hash))
                                maps[containerHash].Add(hash);
                        }
                    }
                }
                await Task.Run(() => ExportResources(maps));
                break;
        }

        MessageBox.Show("Export Complete.");
    }

    public async void ExportStaticMap()
    {
        Log.Info($"Exporting Static Map: {_currentBubble.Name}, {_currentBubble.Hash}");
        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Hidden;
        });

        Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(_currentBubble.Hash);
        var maps = new List<FileHash>();
        bubbleMaps.TagData.MapResources.ForEach(m =>
        {
            var containerHash = m.GetMapContainer().Hash;
            if (!maps.Contains(containerHash))
                maps.Add(containerHash);
        });

        List<string> mapStages = maps.Select((x, i) => $"Preparing {x} ({i + 1}/{maps.Count()})").ToList();
        mapStages.Add("Exporting Static Map");
        MainWindow.Progress.SetProgressStages(mapStages);

        Tiger.Exporters.Exporter.Get().GetOrCreateGlobalScene();
        string savePath = $"{ConfigSubsystem.Get().GetExportSavePath()}/Maps/{_currentActivity.DestinationName}/";
        maps.ForEach(map =>
        {
            MapView.ExportFullMap(FileResourcer.Get().GetSchemaTag<SMapContainer>(map), savePath);
            MainWindow.Progress.CompleteStage();
        });

        Tiger.Exporters.Exporter.Get().Export(savePath);
        MainWindow.Progress.CompleteStage();

        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Visible;
        });
        Log.Info($"Exported Static Map: {_currentBubble.Name}, {_currentBubble.Hash}");
    }

    public async void ExportResources(ConcurrentDictionary<FileHash, List<FileHash>> maps = null)
    {
        // this is dumb but whatever lol
        string type = maps == null ? "Map Resources" : "Activity Entities";
        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Hidden;
        });

        if (maps is null)
        {
            Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(_currentBubble.Hash);
            maps = new ConcurrentDictionary<FileHash, List<FileHash>>();
            bubbleMaps.TagData.MapResources.ForEach(m =>
            {
                var containerHash = m.GetMapContainer().Hash;
                if (!maps.ContainsKey(containerHash))
                    maps.TryAdd(m.GetMapContainer().Hash, new());

                foreach (var dataTable in m.GetMapContainer().TagData.MapDataTables)
                {
                    var hash = dataTable.MapDataTable;
                    if (dataTable.MapDataTable is not null && !maps[containerHash].Contains(hash.Hash))
                        maps[containerHash].Add(hash.Hash);
                }
            });
        }

        Log.Info($"Exporting {type}: {_currentBubble.Name}, {_currentBubble.Hash}");
        List<string> mapStages = maps.Select((x, i) => $"Preparing {_currentBubble.Name} ({i + 1}/{maps.Count()})").ToList();
        mapStages.Add($"Exporting {type}");
        MainWindow.Progress.SetProgressStages(mapStages);

        string savePath = $"{ConfigSubsystem.Get().GetExportSavePath()}/Maps/{_currentActivity.DestinationName}/";
        foreach ((FileHash container, List<FileHash> hashes) in maps)
        {
            ActivityMapEntityView.ExportFull(hashes, container, savePath);
            MainWindow.Progress.CompleteStage();
        };

        Tiger.Exporters.Exporter.Get().Export(savePath);
        MainWindow.Progress.CompleteStage();

        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Visible;
        });
        Log.Info($"Exported {type}: {_currentBubble.Name}, {_currentBubble.Hash}");
    }

    // Kept for individual / old method of exporting
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

        List<string> mapStages = maps.Select((x, i) => $"Preparing {i + 1}/{maps.Count}").ToList();
        mapStages.Add("Exporting");
        MainWindow.Progress.SetProgressStages(mapStages);

        Tiger.Exporters.Exporter.Get().GetOrCreateGlobalScene();
        string savePath = $"{ConfigSubsystem.Get().GetExportSavePath()}/Maps/{_currentActivity.DestinationName}/";
        maps.ForEach(map =>
        {
            MapView.ExportFullMap(map, savePath);
            MainWindow.Progress.CompleteStage();
        });

        Tiger.Exporters.Exporter.Get().Export(savePath);

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
        MapControl.Visibility = Visibility.Hidden;
        Log.Info($"Loading UI for static map hash: {dc.Name}");

        var lod = MapControl.ModelView.GetSelectedLod();
        if (dc.Name == "Select all")
        {
            var items = StaticList.Items.Cast<DisplayStaticMap>().Where(x => x.Name != "Select all");
            List<string> mapStages = items.Select(x => $"Loading to UI: {x.Hash}").ToList();
            if (mapStages.Count == 0)
            {
                Log.Error("No maps available for viewing.");
                MessageBox.Show("No maps available for viewing.");
                return;
            }
            MainWindow.Progress.SetProgressStages(mapStages);
            await Task.Run(() =>
            {
                Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(_currentBubble.Hash);
                bubbleMaps.TagData.MapResources.ForEach(m =>
                {
                    MapControl.LoadMap(m.GetMapContainer().Hash, lod);
                    MainWindow.Progress.CompleteStage();
                });
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
