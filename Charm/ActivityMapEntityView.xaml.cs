using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using NVorbis.Contracts;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Activity;
using Tiger.Schema.Activity.DESTINY2_WITCHQUEEN_6307;
using Tiger.Schema.Entity;

namespace Charm;

public partial class ActivityMapEntityView : UserControl
{
    private FbxHandler _globalFbxHandler = null;
    private static ConfigSubsystem _config = CharmInstance.GetSubsystem<ConfigSubsystem>();

    public ActivityMapEntityView()
    {
        InitializeComponent();
        _globalFbxHandler = new FbxHandler(false);
    }

    public void LoadUI(IActivity activity)
    {
        MapList.ItemsSource = GetMapList(activity);
        ExportControl.SetExportFunction(Export, (int)ExportTypeFlag.Full, true);
        ExportControl.SetExportInfo(activity.FileHash);
    }

    private ObservableCollection<DisplayEntBubble> GetMapList(IActivity activity)
    {
        var maps = new ObservableCollection<DisplayEntBubble>();
        foreach (var bubble in activity.EnumerateBubbles())
        {
            DisplayEntBubble displayMap = new();
            displayMap.Name = $"{bubble.Name}";
            displayMap.Hash = bubble.MapReference.TagData.ChildMapReference.Hash;
            displayMap.LoadType = DisplayEntBubble.Type.Bubble;
            displayMap.Data = displayMap;
            maps.Add(displayMap);
        }

        DisplayEntBubble displayActivity = new();
        displayActivity.Name = $"{PackageResourcer.Get().GetActivityName(activity.FileHash)}";
        displayActivity.Hash = $"{activity.FileHash}";
        displayActivity.LoadType = DisplayEntBubble.Type.Activity;
        displayActivity.Data = displayActivity;
        maps.Add(displayActivity);

        return maps;
    }

    private void GetBubbleContentsButton_OnClick(object sender, RoutedEventArgs e)
    {
        DisplayEntBubble tagData = (sender as Button).Tag as DisplayEntBubble; //apparently this works..?
        if(tagData.LoadType == DisplayEntBubble.Type.Bubble)
        {
            FileHash hash = new FileHash(tagData.Hash);
            Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(hash);
            PopulateEntityContainerList(bubbleMaps);
        }
        else
        {
            FileHash hash = new FileHash(tagData.Hash);
            IActivity activity = FileResourcer.Get().GetFileInterface<IActivity>(hash);
            PopulateActivityEntityContainerList(activity);
        }
    }

    private void EntityMapPart_OnCheck(object sender, RoutedEventArgs e)
    {
        if ((sender as CheckBox).Tag is null)
            return;

        DisplayEntityMap tagData = (sender as CheckBox).Tag as DisplayEntityMap; //apparently this works..?

        foreach (DisplayEntityMap item in EntityContainerList.Items)
        {
            if (item.Name == "Select all")
                continue;

            if (item.Selected)
            {
                if (tagData.EntityType == DisplayEntityMap.Type.Map)
                {
                    Tag<SMapContainer> map = FileResourcer.Get().GetSchemaTag<SMapContainer>(tagData.Hash);
                    if (map == null)
                        continue;

                    ConcurrentBag<FileHash> sMapDataTables = new();
                    foreach(var entry in map.TagData.MapDataTables)
                    {
                        sMapDataTables.Add(entry.MapDataTable.Hash);
                    }

                    PopulateEntityList(sMapDataTables.ToList());
                }  
                else
                {
                    PopulateEntityList(tagData.DataTables);
                }
            }
        }
    }

    private void PopulateEntityContainerList(Tag<SBubbleDefinition> bubbleMaps)
    {
        ConcurrentBag<DisplayEntityMap> items = new ConcurrentBag<DisplayEntityMap>();
        Parallel.ForEach(bubbleMaps.TagData.MapResources, m =>
        {
            if (m.MapContainer.TagData.MapDataTables.Count > 0)
            {
                DisplayEntityMap entityMap = new();
                entityMap.Name = $"{m.MapContainer.Hash}";
                entityMap.Hash = m.MapContainer.Hash;
                entityMap.Count = m.MapContainer.TagData.MapDataTables.Count;
                entityMap.EntityType = DisplayEntityMap.Type.Map;
                entityMap.Data = entityMap;

                items.Add(entityMap);
            }
        });
        var sortedItems = new List<DisplayEntityMap>(items);
        sortedItems.Sort((a, b) => b.Count.CompareTo(a.Count));
        sortedItems.Insert(0, new DisplayEntityMap
        {
            Name = "Select all"
        });
        EntityContainerList.ItemsSource = sortedItems;
    }

    private void PopulateActivityEntityContainerList(IActivity activity)
    {
        ConcurrentBag<DisplayEntityMap> items = new ConcurrentBag<DisplayEntityMap>();

        foreach (var entry in activity.EnumerateActivityEntities())
        {
            if(entry.DataTables.Count > 0)
            {
                DisplayEntityMap entityMap = new();
                entityMap.Name = $"{entry.BubbleName} {entry.ActivityPhaseName2}: {entry.DataTables.Count} Entries";
                entityMap.Hash = entry.Hash;
                entityMap.Count = entry.DataTables.Count;
                entityMap.EntityType = DisplayEntityMap.Type.Activity;
                entityMap.DataTables = entry.DataTables;
                entityMap.Data = entityMap;

                items.Add(entityMap);
            }
        }

        var sortedItems = new List<DisplayEntityMap>(items);
        sortedItems.Sort((a, b) => a.Name.CompareTo(b.Name));
        sortedItems.Insert(0, new DisplayEntityMap
        {
            Name = "Select all"
        });
        EntityContainerList.ItemsSource = sortedItems;
    }

    private void PopulateEntityList(List<FileHash> dataTables)
    {
        ConcurrentBag<DisplayEntityList> items = new ConcurrentBag<DisplayEntityList>();
        ConcurrentDictionary<FileHash, int> entityCountDictionary = new ConcurrentDictionary<FileHash, int>();

        Parallel.ForEach(dataTables, data =>
        {
            Tag<SMapDataTable> entry = FileResourcer.Get().GetSchemaTag<SMapDataTable>(data);
            entry.TagData.DataEntries.ForEach(entry => //Need(?) to do this to get number of instances for Entities
            {
                if (entry is SMapDataEntry dynamicResource)
                {
                    entityCountDictionary.AddOrUpdate(dynamicResource.GetEntityHash(), 1, (_, count) => count + 1);
                }
            });

            entityCountDictionary.Keys.AsParallel().ForAll(entityHash =>
            {
                Entity entity = FileResourcer.Get().GetFile(typeof(Entity), entityHash);
                if (entity.HasGeometry())
                {
                    if (!items.Any(item => item.Hash == entity.Hash)) //Check if the entity is already in the EntityList
                    {
                        items.Add(new DisplayEntityList
                        {
                            Name = $"Entity {entity.Hash}: {entityCountDictionary[entityHash]} Instances",
                            Hash = entity.Hash,
                            Instances = entityCountDictionary[entityHash]
                        });
                    }
                }
            });
        });

        var sortedItems = new List<DisplayEntityList>(items);
        sortedItems.Sort((a, b) => b.Instances.CompareTo(a.Instances));
        sortedItems.Insert(0, new DisplayEntityList
        {
            Name = "Select all",
            Parent = dataTables
        });
        EntitiesList.ItemsSource = sortedItems;
    }

    public async void Export(ExportInfo info)
    {
        IActivity activity = FileResourcer.Get().GetFileInterface<IActivity>(info.Hash);
        Log.Info($"Exporting activity data name: {PackageResourcer.Get().GetActivityName(activity.FileHash)}, hash: {activity.FileHash}, export type {info.ExportType.ToString()}");
        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Hidden;
        });
        var maps = new ConcurrentDictionary<List<FileHash>, string>();
        bool bSelectAll = false;
        foreach (DisplayEntityMap item in EntityContainerList.Items)
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
                    maps.TryAdd(item.DataTables, item.Hash);
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

        Parallel.ForEach(maps, map =>
        {
            ExportFull(map.Key, map.Value);
            MainWindow.Progress.CompleteStage();
        });

        Tiger.Exporters.Exporter.Get().Export();
        
        MainWindow.Progress.CompleteStage();

        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Visible;
        });
        Log.Info($"Exported activity data name: {PackageResourcer.Get().GetActivityName(activity.FileHash)}, hash: {activity.FileHash}");
        MessageBox.Show("Activity map data exported completed.");
    }

    public static void ExportFull(List<FileHash> dataTables, string hash)
    {
        string savePath = _config.GetExportSavePath() + $"/{hash}";
        if (_config.GetSingleFolderMapsEnabled())
        {
            savePath = _config.GetExportSavePath() + "/Maps";
        }

        Directory.CreateDirectory(savePath);
        if (_config.GetIndvidualStaticsEnabled())
        {
            Directory.CreateDirectory(savePath + "/Entities");
            ExportIndividual(dataTables, hash, savePath);
        }

        ExtractDataTables(dataTables, hash, savePath);

        if (_config.GetUnrealInteropEnabled())
        {
            AutomatedExporter.SaveInteropUnrealPythonFile(savePath, hash, AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat(), _config.GetSingleFolderMapsEnabled());
        }
    }

    private static void ExtractDataTables(List<FileHash> dataTables, string hash, string savePath)
    {
        // todo these scenes can be combined
        ExporterScene dynamicPointScene = Exporter.Get().CreateScene($"{hash}_EntityPoints", ExportType.EntityPoints);
        ExporterScene dynamicScene = Exporter.Get().CreateScene($"{hash}_Entities", ExportType.Map);

        Parallel.ForEach(dataTables, data =>
        {
            var dataTable = FileResourcer.Get().GetSchemaTag<SMapDataTable>(data);
            dataTable.TagData.DataEntries.ForEach(entry =>
            {
                if (entry is SMapDataEntry dynamicResource)
                {
                    Entity entity = FileResourcer.Get().GetFile<Entity>(entry.GetEntityHash());
                    if (entity.HasGeometry())
                    {
                        dynamicScene.AddMapEntity(dynamicResource, entity);
                        entity.SaveMaterialsFromParts(savePath, entity.Load(ExportDetailLevel.MostDetailed), true);
                    }
                    else
                        dynamicPointScene.AddEntityPoints(dynamicResource);
                }
            });
        });
    }

    private static void ExportIndividual(List<FileHash> dataTables, string hash, string savePath)
    {
        Parallel.ForEach(dataTables, data =>
        {
            var dataTable = FileResourcer.Get().GetSchemaTag<SMapDataTable>(data);
            dataTable.TagData.DataEntries.ForEach(entry =>
            {
                if (entry is SMapDataEntry dynamicResource)
                {
                    Entity entity = FileResourcer.Get().GetFile<Entity>(entry.GetEntityHash());
                    if (entity.HasGeometry())
                    {
                        ExporterScene dynamicScene = Exporter.Get().CreateScene(entity.Hash, ExportType.EntityInMap);
                        dynamicScene.AddEntity(dynamicResource.GetEntityHash(), entity.Load(ExportDetailLevel.MostDetailed), entity.Skeleton?.GetBoneNodes());
                        entity.SaveMaterialsFromParts(savePath, entity.Load(ExportDetailLevel.MostDetailed), true);

                        if (_config.GetS2VMDLExportEnabled())
                        {
                            Source2Handler.SaveEntityVMDL($"{savePath}/Entities", entity);
                        }
                    }
                }
            });
        });
    }

    private async void EntityMapView_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as DisplayEntityMap;
        Log.Info($"Loading UI for static map hash: {dc.Name}");
        MapControl.Clear();
        MapControl.Visibility = Visibility.Hidden;
        var lod = MapControl.ModelView.GetSelectedLod();
        if (dc.Name == "Select all")
        {
            var items = EntityContainerList.Items.Cast<DisplayEntityMap>().Where(x => x.Name != "Select all");
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
                foreach (DisplayEntityMap item in items)
                {
                    if(item.EntityType == DisplayEntityMap.Type.Map)
                    {
                        Tag<SMapContainer> tag = FileResourcer.Get().GetSchemaTag<SMapContainer>(new FileHash(item.Hash));
                        foreach (var datatable in tag.TagData.MapDataTables)
                        {
                            MapControl.LoadMap(datatable.MapDataTable.Hash, lod, true);
                        }
                        MainWindow.Progress.CompleteStage();
                    }
                    else
                    {
                        foreach (var datatable in item.DataTables)
                        {
                            MapControl.LoadMap(datatable, lod, true);
                        }
                        MainWindow.Progress.CompleteStage();
                    }
                }
            });
        }
        else
        {
            MainWindow.Progress.SetProgressStages(new List<string> { dc.Hash });
            await Task.Run(() =>
            {
                if (dc.EntityType == DisplayEntityMap.Type.Map)
                {
                    Tag<SMapContainer> tag = FileResourcer.Get().GetSchemaTag<SMapContainer>(new FileHash(dc.Hash));
                    foreach (var datatable in tag.TagData.MapDataTables)
                    {
                        MapControl.LoadMap(datatable.MapDataTable.Hash, lod, true);
                    }
                    MainWindow.Progress.CompleteStage();
                }
                else
                {
                    foreach (var datatable in dc.DataTables)
                    {
                        MapControl.LoadMap(datatable, lod, true);
                    }
                    MainWindow.Progress.CompleteStage();
                }
            });
        }
        MapControl.Visibility = Visibility.Visible;
    }

    private async void EntityView_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as DisplayEntityList;
        MapControl.Clear();
        Log.Info($"Loading UI for entity: {dc.Name}");
        MapControl.Visibility = Visibility.Hidden;
        var lod = MapControl.ModelView.GetSelectedLod();
        if (dc.Name == "Select all")
        {
            var items = dc.Parent;
            List<string> mapStages = items.Select(x => $"Loading to UI: {x}").ToList();
            if (mapStages.Count == 0)
            {
                Log.Error("No entities available for view.");
                MessageBox.Show("No entities available for view.");
                return;
            }

            MainWindow.Progress.SetProgressStages(mapStages);
            await Task.Run(() =>
            {
                foreach (var datatable in items)
                {
                    var mapDataTable = FileResourcer.Get().GetSchemaTag<SMapDataTable>(datatable);
                    MapControl.LoadMap(mapDataTable.Hash, lod, true);
                    MainWindow.Progress.CompleteStage();
                }
            });
        }
        else
        {
            FileHash tagHash = new FileHash(dc.Hash);
            MainWindow.Progress.SetProgressStages(new List<string> { $"Loading Entity to UI: {tagHash}" });
            await Task.Run(() =>
            {
                MapControl.LoadEntity(tagHash, _globalFbxHandler);
                MainWindow.Progress.CompleteStage();
            });
        }
        MapControl.Visibility = Visibility.Visible;
    }

    private async void EntityExport_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as DisplayEntityList;
        Log.Info($"Exporting entity: {dc.Name}");
        MapControl.Visibility = Visibility.Hidden;

        if (dc.Name == "Select all")
        {
            var items = dc.Parent;
            List<string> mapStages = items.Select(x => $"Exporting Entities: {x}").ToList();
            if (mapStages.Count == 0)
            {
                Log.Error("No entities available for export.");
                MessageBox.Show("No entities available for export.");
                return;
            }

            MainWindow.Progress.SetProgressStages(mapStages);
            await Task.Run(() =>
            {
                foreach (var datatable in items)
                {
                    var mapDataTable = FileResourcer.Get().GetSchemaTag<SMapDataTable>(datatable);
                    foreach (var entry in mapDataTable.TagData.DataEntries)
                    {
                        Entity entity = FileResourcer.Get().GetFile<Entity>(entry.GetEntityHash());
                        EntityView.Export(new List<Entity> { entity }, entity.Hash, ExportTypeFlag.Full);
                    }
                    MainWindow.Progress.CompleteStage();
                }
            });
        }
        else
        {
            FileHash tagHash = new FileHash(dc.Hash);
            MainWindow.Progress.SetProgressStages(new List<string> { $"Exporting Entity: {tagHash}" });
            await Task.Run(() =>
            {
                Entity entity = FileResourcer.Get().GetFile<Entity>(tagHash);
                EntityView.Export(new List<Entity> { entity }, entity.Hash, ExportTypeFlag.Full);
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

public class DisplayEntBubble
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public Type LoadType { get; set; } //this kinda sucks but dont want to have 2 seperate tabs for map and activity entities
    public DisplayEntBubble Data { get; set; }

    public enum Type
    {
        Bubble,
        Activity
    }
}

public class DisplayEntityMap
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public int Count { get; set; }

    public bool Selected { get; set; }
    public Type EntityType { get; set; }
    public List<FileHash> DataTables { get; set; }
    public DisplayEntityMap Data { get; set; }

    public enum Type
    {
        Map,
        Activity
    }
}

public class DisplayEntityList
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public List<FileHash> Parent { get; set; }
    public int Instances { get; set; }

    public bool Selected { get; set; }

    public override bool Equals(object obj)
    {
        var other = obj as DisplayEntityList;
        return other != null && Hash == other.Hash;
    }

    public override int GetHashCode()
    {
        return Hash?.GetHashCode() ?? 0;
    }
}
