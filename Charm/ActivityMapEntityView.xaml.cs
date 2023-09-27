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
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Activity;
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
        return maps;
    }

    private void GetBubbleContentsButton_OnClick(object sender, RoutedEventArgs e)
    {
        FileHash hash = new FileHash((sender as Button).Tag as string);
        Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(hash);
        PopulateEntityContainerList(bubbleMaps);
    }

    private void EntityMapPart_OnCheck(object sender, RoutedEventArgs e)
    {
        if ((sender as CheckBox).Tag is null)
            return;

        FileHash hash = new FileHash((sender as CheckBox).Tag as string);
        Tag<SMapContainer> map = FileResourcer.Get().GetSchemaTag<SMapContainer>(hash);
        foreach (DisplayEntityMap item in EntityContainerList.Items)
        {
            if (item.Name == "Select all")
                continue;

            if (item.Selected)
            {
                if (map == null)
                    continue;
                PopulateEntityList(map);
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
                items.Add(new DisplayEntityMap
                {
                    Hash = m.MapContainer.Hash,
                    Name = $"{m.MapContainer.Hash}", //{m.MapContainer.TagData.MapDataTables.Count} MapDataTables",
                    Count = m.MapContainer.TagData.MapDataTables.Count
                });
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

    private void PopulateEntityList(Tag<SMapContainer> map)
    {
        ConcurrentBag<DisplayEntityList> items = new ConcurrentBag<DisplayEntityList>();
        ConcurrentDictionary<FileHash, int> entityCountDictionary = new ConcurrentDictionary<FileHash, int>();

        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            data.MapDataTable.TagData.DataEntries.ForEach(entry => //Need(?) to do this to get number of instances for Entities
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
            Parent = map
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
        var maps = new List<Tag<SMapContainer>>();
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

        Parallel.ForEach(maps, map =>
        {
            ExportFull(map);
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

    public static void ExportFull(Tag<SMapContainer> map)
    {
        string meshName = map.Hash.ToString();
        string savePath = _config.GetExportSavePath() + $"/{meshName}";
        if (_config.GetSingleFolderMapsEnabled())
        {
            savePath = _config.GetExportSavePath() + "/Maps";
        }

        Directory.CreateDirectory(savePath);
        if (_config.GetIndvidualStaticsEnabled())
        {
            Directory.CreateDirectory(savePath + "/Entities");
            ExportIndividual(savePath, map);
        }

        ExtractDataTables(map, savePath);

        if (_config.GetUnrealInteropEnabled())
        {
            AutomatedExporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat(), _config.GetSingleFolderMapsEnabled());
        }
    }

    private static void ExtractDataTables(Tag<SMapContainer> map, string savePath)
    {
        // todo these scenes can be combined
        ExporterScene dynamicPointScene = Exporter.Get().CreateScene($"{map.Hash}_EntityPoints", ExportType.EntityPoints);
        ExporterScene dynamicScene = Exporter.Get().CreateScene($"{map.Hash}_Entities", ExportType.Map);

        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            data.MapDataTable.TagData.DataEntries.ForEach(entry =>
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

    private static void ExportIndividual(string savePath, Tag<SMapContainer> map)
    {
        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            data.MapDataTable.TagData.DataEntries.ForEach(entry =>
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
                    Tag<SMapContainer> tag = FileResourcer.Get().GetSchemaTag<SMapContainer>(new FileHash(item.Hash));
                    foreach (var datatable in tag.TagData.MapDataTables)
                    {
                        MapControl.LoadMap(datatable.MapDataTable.Hash, lod, true);
                    }
                    MainWindow.Progress.CompleteStage();
                }
            });
        }
        else
        {
            var fileHash = new FileHash(dc.Hash);
            MainWindow.Progress.SetProgressStages(new List<string> { fileHash });
            await Task.Run(() =>
            {
                Tag<SMapContainer> tag = FileResourcer.Get().GetSchemaTag<SMapContainer>(fileHash);
                foreach (var datatable in tag.TagData.MapDataTables)
                {
                    MapControl.LoadMap(datatable.MapDataTable.Hash, lod, true);
                    MainWindow.Progress.CompleteStage();
                }
                MainWindow.Progress.CompleteStage();
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
            var items = dc.Parent.TagData.MapDataTables;
            List<string> mapStages = items.Select(x => $"Loading to UI: {x.MapDataTable.Hash}").ToList();
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
                    MapControl.LoadMap(datatable.MapDataTable.Hash, lod, true);
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
            var items = dc.Parent.TagData.MapDataTables;
            List<string> mapStages = items.Select(x => $"Exporting Entities: {x.MapDataTable.Hash}").ToList();
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
                    foreach(var entry in datatable.MapDataTable.TagData.DataEntries)
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

public class DisplayEntityMap
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public int Count { get; set; }

    public bool Selected { get; set; }
}

public class DisplayEntityList
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public Tag<SMapContainer> Parent { get; set; }
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
