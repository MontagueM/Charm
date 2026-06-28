using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Activity;
using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;
using Tiger.Schema.Activity.DESTINY2_SHADOWKEEP_2601;
using Tiger.Schema.Entity;

namespace Charm;

public partial class ActivityMapEntityView : UserControl
{
    private static ConfigSubsystem _config = TigerInstance.GetSubsystem<ConfigSubsystem>();

    private IActivity _currentActivity;
    private DisplayEntBubble _currentBubble;
    private string _destinationName;

    public ActivityMapEntityView()
    {
        InitializeComponent();
    }

    public void LoadUI(IActivity activity)
    {
        _destinationName = activity.DestinationName;
        _currentActivity = activity;

        MapList.ItemsSource = GetMapList(activity);
        ExportControl.SetExportFunction(Export, (int)ExportTypeFlag.Full, true);
        ExportControl.SetExportInfo(activity.FileHash);
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
    }

    private ObservableCollection<DisplayEntBubble> GetMapList(IActivity activity)
    {
        var maps = new ObservableCollection<DisplayEntBubble>();
        foreach (Bubble bubble in activity.EnumerateBubbles())
        {
            DisplayEntBubble displayMap = new();
            displayMap.Name = $"{bubble.Name}";
            displayMap.Hash = bubble.ChildMapReference.Hash;
            displayMap.LoadType = DisplayEntBubble.Type.Bubble;
            displayMap.Data = displayMap;
            maps.Add(displayMap);
        }

        switch (Strategy.CurrentStrategy)
        {
            case >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402:
                DisplayEntBubble displayActivity = new();
                displayActivity.Name = $"{PackageResourcer.Get().GetActivityName(activity.FileHash)}";
                displayActivity.Hash = activity.FileHash;
                displayActivity.LoadType = DisplayEntBubble.Type.Activity;
                displayActivity.Data = displayActivity;
                maps.Add(displayActivity);

                Tag? ambient = (activity as Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402.Activity).TagData.AmbientActivity;
                if (ambient is not null)
                {
                    DisplayEntBubble ambientActivity = new();
                    ambientActivity.Name = $"{PackageResourcer.Get().GetActivityName(ambient.Hash)}";
                    ambientActivity.Hash = ambient.Hash;
                    ambientActivity.LoadType = DisplayEntBubble.Type.Activity;
                    ambientActivity.Data = ambientActivity;
                    maps.Add(ambientActivity);
                }
                break;

            case TigerStrategy.DESTINY2_SHADOWKEEP_2999 or TigerStrategy.DESTINY2_SHADOWKEEP_2601:
                // This sucks. A lot.
                ConcurrentCollections.ConcurrentHashSet<FileHash> valsSK = PackageResourcer.Get().GetAllHashes<SUnkActivity_SK>();
                foreach (FileHash val in valsSK)
                {
                    Tag<SUnkActivity_SK> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_SK>(val);
                    string activityName = PackageResourcer.Get().GetActivityName(activity.FileHash).Split(':')[1];

                    if (tag.TagData.ActivityDevName.Value.Contains(activityName)) //This is probably really bad...
                    {
                        DisplayEntBubble displayActivitySK = new();
                        displayActivitySK.Name = $"{PackageResourcer.Get().GetActivityName(val)}";
                        displayActivitySK.Hash = tag.Hash;
                        displayActivitySK.ParentHash = activity.FileHash;
                        displayActivitySK.LoadType = DisplayEntBubble.Type.Activity;
                        displayActivitySK.Data = displayActivitySK;
                        maps.Add(displayActivitySK);
                    }
                }
                break;

            case TigerStrategy.DESTINY1_RISE_OF_IRON:
                // This also sucks. A lot.
                Dictionary<FileHash, TagClassHash> valsROI = PackageResourcer.Get().GetD1Activities();
                foreach (KeyValuePair<FileHash, TagClassHash> val in valsROI)
                {
                    if (val.Value == 0x80800616)
                    {
                        Tag<SUnkActivity_ROI> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_ROI>(val.Key);

                        string activityName = PackageResourcer.Get().GetActivityName(activity.FileHash).Split(':')[1];
                        if (tag.TagData.ActivityDevName.Value.Contains(activityName))
                        {
                            DisplayEntBubble displayActivityROI = new();
                            displayActivityROI.Name = $"{PackageResourcer.Get().GetActivityName(val.Key).Split(":").First()}";
                            displayActivityROI.Hash = tag.Hash;
                            displayActivityROI.ParentHash = activity.FileHash;
                            displayActivityROI.LoadType = DisplayEntBubble.Type.Activity;
                            displayActivityROI.Data = displayActivityROI;
                            maps.Add(displayActivityROI);
                        }
                    }
                }
                break;
            default:
                break;
        }

        return maps;
    }

    private async void GetBubbleContentsButton_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.Invoke(() => MapControl.Visibility = Visibility.Hidden);

        DisplayEntBubble tagData = (sender as RadioButton).Tag as DisplayEntBubble; //apparently this works..?
        if (tagData.LoadType == DisplayEntBubble.Type.Bubble)
        {
            MainWindow.Progress.SetProgressStages(new() { $"Loading Resources for {tagData.Name}" });
            FileHash hash = tagData.Hash;
            _currentBubble = tagData;

            Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(hash);
            await Task.Run(() => PopulateEntityContainerList(bubbleMaps));
        }
        else
        {
            MainWindow.Progress.SetProgressStages(new() { $"Loading Activity Entities for {tagData.Name}" });
            FileHash hash = tagData.Hash;
            if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999)
            {
                FileHash parentHash = tagData.ParentHash;
                IActivity activity = FileResourcer.Get().GetFileInterface<IActivity>(parentHash);
                await Task.Run(() => PopulateActivityEntityContainerList(activity, hash));
            }
            else
            {
                IActivity activity = FileResourcer.Get().GetFileInterface<IActivity>(hash);
                await Task.Run(() => PopulateActivityEntityContainerList(activity));
            }

        }

        MainWindow.Progress.CompleteStage();
        Dispatcher.Invoke(() => MapControl.Visibility = Visibility.Visible);
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

                    ConcurrentBag<FileHash> sMapDataTables = new(map.TagData.MapDataTables.Select(entry => entry.MapDataTable.Hash));
                    PopulateEntityList(sMapDataTables.ToList(), null);
                }
                else
                {
                    PopulateEntityList(tagData.DataTables, tagData.WorldIDs);
                }
            }
        }
    }

    private void PopulateEntityContainerList(Tag<SBubbleDefinition> bubbleMaps)
    {
        ConcurrentBag<DisplayEntityMap> items = new();
        Parallel.ForEach(bubbleMaps.TagData.MapResources, m =>
        {
            if (m.MapContainer.TagData.MapDataTables.Count > 0)
            {
                DisplayEntityMap entityMap = new();
                entityMap.Name = $"{m.MapContainer.Hash}";
                entityMap.Hash = m.MapContainer.Hash;
                entityMap.Count = m.MapContainer.TagData.MapDataTables.Count;
                entityMap.EntityType = DisplayEntityMap.Type.Map;
                entityMap.DataTables = m.MapContainer.TagData.MapDataTables.Select(entry => entry.MapDataTable.Hash).ToList();
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
        Dispatcher.Invoke(() => EntityContainerList.ItemsSource = sortedItems);
    }

    private void PopulateActivityEntityContainerList(IActivity activity, FileHash UnkActivity = null)
    {
        ConcurrentBag<DisplayEntityMap> items = new();

        foreach (ActivityEntities entry in activity.EnumerateActivityEntities(UnkActivity))
        {
            if (entry.DataTables.Count > 0)
            {
                DisplayEntityMap entityMap = new();
                entityMap.Name = $"{entry.BubbleName}: {entry.ActivityPhaseName2}: {entry.DataTables.Count} Entries";
                entityMap.Hash = entry.Hash;
                entityMap.Count = entry.DataTables.Count;
                entityMap.EntityType = DisplayEntityMap.Type.Activity;
                entityMap.DataTables = entry.DataTables;
                entityMap.WorldIDs = entry.WorldIDs;
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
        Dispatcher.Invoke(() => EntityContainerList.ItemsSource = sortedItems);
    }

    private void PopulateEntityList(List<FileHash> dataTables, Dictionary<ulong, ActivityEntity>? worldIDs)
    {
        ConcurrentBag<DisplayEntityList> items = new();
        ConcurrentDictionary<FileHash, ConcurrentBag<ulong>> entities = new();

        Parallel.ForEach(dataTables, data =>
        {
            List<SMapDataEntry> dataEntries = new();
            if (Strategy.IsD1() && data.GetReferenceHash().Hash32 == 0x808003F6) //F6038080
                dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<S808003F6>(data).TagData.EntityComponent.CollapseIntoDataEntry());
            else
                dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SMapDataTable>(data).TagData.DataEntries);

            dataEntries.ForEach(entry =>
            {
                if (!entities.ContainsKey(entry.Entity.Hash))
                {
                    entities[entry.Entity.Hash] = new ConcurrentBag<ulong>();
                }
                entities[entry.Entity.Hash].Add(entry.WorldID);
            });
        });

        entities.AsParallel().ForAll(entityHash =>
        {
            Entity entity = FileResourcer.Get().GetFile(typeof(Entity), entityHash.Key);
            if (entity.HasGeometry())
            {
                foreach (ulong namedEnt in entityHash.Value)
                {
                    if (worldIDs is not null && worldIDs.ContainsKey(namedEnt))
                    {
                        string Name = worldIDs[namedEnt].Name;
                        string SubName = worldIDs[namedEnt].SubName;
                        //This is gross
                        if (!items.Any(item => item.CompareByName(new DisplayEntityList { Name = $"{Name}:{SubName}" })))
                        {
                            int Count = worldIDs.Count(kvp => kvp.Value.Name == worldIDs[namedEnt].Name);
                            items.Add(new DisplayEntityList
                            {
                                DisplayName = $"{Name}: {Count} Instances",
                                SubName = $"{SubName}",
                                Name = $"{Name}:{SubName}",
                                Hash = entity.Hash,
                                Instances = Count
                            });
                        }
                    }
                }
                if (!items.Any(item => item.CompareByHash(new DisplayEntityList { Hash = entity.Hash }))) //Dont want duplicate entries if a named entity was already added
                {
                    items.Add(new DisplayEntityList
                    {
                        DisplayName = $"{entity.Hash}: {entityHash.Value.Count} Instances",
                        Name = entity.Hash,
                        Hash = entity.Hash,
                        Instances = entityHash.Value.Count
                    });
                }
            }
        });

        var sortedItems = new List<DisplayEntityList>(items);
        sortedItems.Sort((a, b) => b.Instances.CompareTo(a.Instances));
        sortedItems.Insert(0, new DisplayEntityList
        {
            DisplayName = "All Entities",
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
                    if (item.DataTables is null)
                        continue;
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

        List<string> mapStages = maps.Select((x, i) => $"Preparing {i + 1}/{maps.Count}").ToList();
        mapStages.Add("Exporting");
        MainWindow.Progress.SetProgressStages(mapStages);

        string savePath = $"{ConfigSubsystem.Get().GetExportSavePath()}/Maps/{_currentActivity.DestinationName}/";
        maps.ToList().ForEach(map =>
        {
            ExportFull(map.Key, map.Value, savePath);
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

    public static void ExportFull(List<FileHash> dataTables, string hash, string savePath)
    {
        Directory.CreateDirectory(savePath);
        ExtractDataTables(dataTables, hash, savePath);

        if (_config.GetUnrealInteropEnabled())
        {
            AutomatedExporter.SaveInteropUnrealPythonFile(savePath, hash, AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat(), _config.GetSingleFolderMapAssetsEnabled());
        }
    }

    private static void ExtractDataTables(List<FileHash> dataTables, string hash, string savePath)
    {
        GlobalExporterScene globalScene = Tiger.Exporters.Exporter.Get().GetOrCreateGlobalScene();

        // todo these scenes can be combined
        ExporterScene entitiesScene = Exporter.Get().CreateScene($"{hash}_Entities", ExportType.Entities, DataExportType.Map);
        ExporterScene skyScene = Exporter.Get().CreateScene($"{hash}_SkyObjects", ExportType.SkyObjects, DataExportType.Map);
        ExporterScene decoratorsScene = Exporter.Get().CreateScene($"{hash}_Decorators", ExportType.Decorators, DataExportType.Map);
        ExporterScene treesScene = Exporter.Get().CreateScene($"{hash}_SpeedTrees", ExportType.SpeedTrees, DataExportType.Map);
        ExporterScene roadDecalsScene = Exporter.Get().CreateScene($"{hash}_RoadDecals", ExportType.RoadDecals, DataExportType.Map);
        ExporterScene waterDecalsScene = Exporter.Get().CreateScene($"{hash}_WaterDecals", ExportType.WaterDecals, DataExportType.Map);

        Parallel.ForEach(dataTables, data =>
        {
            if (Strategy.IsD1() && data.GetReferenceHash().Hash32 == 0x808003F6)
            {
                List<SMapDataEntry> dataEntries = FileResourcer.Get().GetSchemaTag<S808003F6>(data).TagData.EntityComponent.CollapseIntoDataEntry();
                foreach (SMapDataEntry entry in dataEntries)
                {
                    Entity entity = FileResourcer.Get().GetFile<Entity>(entry.Entity.Hash);
                    if (entity.HasGeometry())
                    {
                        entitiesScene.AddMapEntity(entry);
                        entity.SaveMaterialsFromParts(entitiesScene, entity.Load(ExportDetailLevel.MostDetailed));
                    }
                    //else
                    //    dynamicPointScene.AddEntityPoints(entry);
                }
            }
            else
            {
                Tag<SMapDataTable> dataTable = FileResourcer.Get().GetSchemaTag<SMapDataTable>(data);
                dataTable.TagData.DataEntries.ForEach(entry =>
                {
                    Entity entity = FileResourcer.Get().GetFile<Entity>(entry.Entity.Hash);
                    if (entity.HasGeometry())
                    {
                        entitiesScene.AddMapEntity(entry);
                        entity.SaveMaterialsFromParts(entitiesScene, entity.Load(ExportDetailLevel.MostDetailed));
                    }
                    else
                    {
                        foreach (FileHash? resourceHash in entity.Components)
                        {
                            EntityComponent resource = FileResourcer.Get().GetFile<EntityComponent>(resourceHash);
                            switch (resource.TagData.Unk10.GetValue(resource.GetReader()))
                            {
                                case S80809479:
                                    var a = ((S80808179)resource.TagData.Unk18.GetValue(resource.GetReader()));
                                    DynamicArray<S808091F1> b = a.Array1;
                                    b.AddRange(a.Array2);

                                    foreach (S808091F1 c in b)
                                    {
                                        if (c.Unk10.GetValue(resource.GetReader()) is S808091D1)
                                        {
                                            // I dont like adding the whole resource here, but its the only way to get the data out of it
                                            globalScene.AddToGlobalScene(resource);
                                            break;
                                        }
                                    }
                                    break;
                            }
                        }

                        //if (entry.Translation.ToVec3() == Tiger.Schema.Vector3.Zero)
                        //    System.Console.WriteLine($"World origin resource {dataTable.Hash} Resource? {entry.DataResource.GetValue(dataTable.GetReader())}");
                        //dynamicPointScene.AddEntityPoints(entry);
                    }

                    switch (entry.DataResource.GetValue(dataTable.GetReader()))
                    {
                        case SMapSkyObjectsResource skyResource:
                            skyResource.SkyObjects?.Load();
                            if (skyResource.SkyObjects is not null)
                                skyResource.SkyObjects.LoadIntoExporter(skyScene);
                            break;

                        case SMapCubemapResource cubemapResource:
                            Cubemap cubemap = new(cubemapResource);
                            cubemap.CubemapTransform = entry.Transfrom;
                            cubemap.LoadIntoExporter();
                            break;

                        case SMapLightResource mapLight:
                            mapLight.Lights?.Load();
                            if (mapLight.Lights is not null)
                                mapLight.Lights.LoadIntoExporter();
                            break;

                        case SExpensiveLightResource shadowingLight:
                            shadowingLight.ShadowingLight?.Load();
                            if (shadowingLight.ShadowingLight is not null)
                            {
                                shadowingLight.ShadowingLight.Transfrom = entry.Transfrom;
                                shadowingLight.ShadowingLight.LoadIntoExporter();
                            }
                            break;

                        case SMapDecalsResource decals:
                            decals.MapDecals?.Load();
                            if (decals.MapDecals is not null)
                                decals.MapDecals.LoadIntoExporter(entitiesScene);
                            break;

                        case SDecoratorMapResource decorator:
                            decorator.Decorator?.Load();
                            if (decorator.Decorator is not null)
                                decorator.Decorator.LoadIntoExporter(decoratorsScene, treesScene, savePath);
                            break;

                        case SMapWaterDecal waterDecal:
                            if (waterDecal.Model is null)
                                return;

                            WaterDecals water = new(waterDecal);
                            water.Transform = entry.Transfrom;
                            water.LoadIntoExporter(waterDecalsScene);
                            break;

                        case SMapAtmosphere atmosphere:
                            if (!globalScene.Any<SMapAtmosphere>())
                                globalScene.AddToGlobalScene(atmosphere, true);
                            break;

                        case SMapLensFlareResource lensFlare:
                            if (lensFlare.LensFlare is not null)
                            {
                                lensFlare.LensFlare.Transform = entry.Transfrom;
                                lensFlare.LensFlare.LoadIntoExporter(entitiesScene);
                            }
                            break;

                        case SMapRoadDecalsResource roadDecals:
                            roadDecals.RoadDecals?.Load();
                            if (roadDecals.RoadDecals is null)
                                return;

                            roadDecals.RoadDecals.LoadIntoExporter(roadDecalsScene);
                            break;

                        case S80806A71 dayCycle:
                            if (!globalScene.Any<S80806A71>())
                                globalScene.AddToGlobalScene(dayCycle, true);
                            break;

                        default:
                            break;
                    }
                });
            }
        });
    }

    private async void EntityMapView_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as DisplayEntityMap;
        Log.Info($"Loading UI for static map hash: {dc.Name}");
        MapControl.Clear();
        MapControl.Visibility = Visibility.Hidden;
        // Just gonna do highest level for entity viewing, should be fine
        ExportDetailLevel lod = ExportDetailLevel.MostDetailed; //MapControl.ModelView.GetSelectedLod();

        if (dc.Name == "Select all")
        {
            IEnumerable<DisplayEntityMap> items = EntityContainerList.Items.Cast<DisplayEntityMap>().Where(x => x.Name != "Select all");
            List<string> mapStages = items.Select(x => $"Loading to UI: {x.Hash}").ToList();
            if (mapStages.Count == 0)
            {
                Log.Error("No entries available for viewing.");
                MessageBox.Show("No entries available for viewing.");
                return;
            }
            MainWindow.Progress.SetProgressStages(mapStages);
            await Task.Run(() =>
            {
                foreach (DisplayEntityMap item in items)
                {
                    if (item.EntityType == DisplayEntityMap.Type.Map)
                    {
                        Tag<SMapContainer> tag = FileResourcer.Get().GetSchemaTag<SMapContainer>(item.Hash);
                        foreach (SMapDataTableEntry datatable in tag.TagData.MapDataTables)
                        {
                            MapControl.LoadMap(datatable.MapDataTable.Hash, lod, true);
                        }
                        MainWindow.Progress.CompleteStage();
                    }
                    else
                    {
                        foreach (FileHash datatable in item.DataTables)
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
                    Tag<SMapContainer> tag = FileResourcer.Get().GetSchemaTag<SMapContainer>(dc.Hash);
                    foreach (SMapDataTableEntry datatable in tag.TagData.MapDataTables)
                    {
                        MapControl.LoadMap(datatable.MapDataTable.Hash, lod, true);
                    }
                    MainWindow.Progress.CompleteStage();
                }
                else
                {
                    foreach (FileHash datatable in dc.DataTables)
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
        // Just gonna do highest level for entity viewing, should be fine
        ExportDetailLevel lod = ExportDetailLevel.MostDetailed; //MapControl.ModelView.GetSelectedLod();
        if (dc.DisplayName == "All Entities")
        {
            List<FileHash> items = dc.Parent;
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
                foreach (FileHash datatable in items)
                {
                    MapControl.LoadMap(datatable, lod, true);
                    MainWindow.Progress.CompleteStage();
                }
            });
        }
        else
        {
            MainWindow.Progress.SetProgressStages(new List<string> { $"Loading Entity to UI: {dc.Hash}" });
            await Task.Run(() =>
            {
                MapControl.LoadEntity(dc.Hash);
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

        if (dc.DisplayName == "All Entities")
        {
            List<FileHash> items = dc.Parent;
            List<string> mapStages = items.Select(x => $"Exporting Entities: {x}").ToList();
            if (mapStages.Count == 0)
            {
                Log.Error("No entries available for export.");
                MessageBox.Show("No entries available for export.");
                return;
            }

            MainWindow.Progress.SetProgressStages(mapStages);
            await Task.Run(() =>
            {
                foreach (FileHash datatable in items)
                {
                    List<SMapDataEntry> dataEntries = new();
                    if (Strategy.IsD1() && datatable.GetReferenceHash().Hash32 == 0x808003F6) //F6038080
                        dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<S808003F6>(datatable).TagData.EntityComponent.CollapseIntoDataEntry());
                    else
                        dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SMapDataTable>(datatable).TagData.DataEntries);

                    foreach (SMapDataEntry entry in dataEntries)
                    {
                        Entity entity = FileResourcer.Get().GetFile<Entity>(entry.Entity.Hash);
                        if (entity.HasGeometry())
                        {
                            List<Entity> entities = new() { entity };
                            entities.AddRange(entity.GetEntityChildren());
                            EntityView.Export(entities, entity.Hash);
                        }
                    }

                    MainWindow.Progress.CompleteStage();
                }
            });
        }
        else
        {
            FileHash tagHash = dc.Hash;
            MainWindow.Progress.SetProgressStages(new List<string> { $"Exporting Entity: {tagHash}" });
            await Task.Run(() =>
            {
                Entity entity = FileResourcer.Get().GetFile<Entity>(tagHash);
                List<Entity> entities = new() { entity };
                entities.AddRange(entity.GetEntityChildren());
                EntityView.Export(entities, entity.Hash);
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
    public FileHash Hash { get; set; }
    public FileHash ParentHash { get; set; }
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
    public FileHash Hash { get; set; }
    public int Count { get; set; }

    public bool Selected { get; set; }
    public Type EntityType { get; set; }
    public List<FileHash> DataTables { get; set; }
    public Dictionary<ulong, ActivityEntity> WorldIDs { get; set; }
    public DisplayEntityMap Data { get; set; }

    public enum Type
    {
        Map,
        Activity
    }
}

public class DisplayEntityList
{
    public string DisplayName { get; set; }
    public string SubName { get; set; }
    public string Name { get; set; }
    public FileHash Hash { get; set; }
    public List<FileHash> Parent { get; set; }
    public int Instances { get; set; }

    public bool Selected { get; set; }

    //public override bool Equals(object obj)
    //{
    //    var other = obj as DisplayEntityList;
    //    return other != null && Hash == other.Hash;
    //}

    public bool CompareByHash(DisplayEntityList other)
    {
        return Hash == other.Hash;
    }

    public bool CompareByName(DisplayEntityList other)
    {
        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Hash?.GetHashCode() ?? 0;
    }
}
