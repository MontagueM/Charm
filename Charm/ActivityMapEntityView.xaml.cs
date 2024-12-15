using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Numerics;
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
using Tiger.Schema.Static;

namespace Charm;

public partial class ActivityMapEntityView : UserControl
{
    private FbxHandler _globalFbxHandler = null;
    private static ConfigSubsystem _config = CharmInstance.GetSubsystem<ConfigSubsystem>();

    private IActivity _currentActivity;
    private DisplayEntBubble _currentBubble;
    private string _destinationName;

    public ActivityMapEntityView()
    {
        InitializeComponent();
        _globalFbxHandler = new FbxHandler(false);
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
    }

    private ObservableCollection<DisplayEntBubble> GetMapList(IActivity activity)
    {
        var maps = new ObservableCollection<DisplayEntBubble>();
        foreach (var bubble in activity.EnumerateBubbles())
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
                displayActivity.Hash = $"{activity.FileHash}";
                displayActivity.LoadType = DisplayEntBubble.Type.Activity;
                displayActivity.Data = displayActivity;
                maps.Add(displayActivity);

                var ambient = (activity as Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402.Activity).TagData.AmbientActivity;
                if (ambient is not null)
                {
                    DisplayEntBubble ambientActivity = new();
                    ambientActivity.Name = $"{PackageResourcer.Get().GetActivityName(ambient.Hash)}";
                    ambientActivity.Hash = $"{ambient.Hash}";
                    ambientActivity.LoadType = DisplayEntBubble.Type.Activity;
                    ambientActivity.Data = ambientActivity;
                    maps.Add(ambientActivity);
                }
                break;

            case TigerStrategy.DESTINY2_SHADOWKEEP_2999 or TigerStrategy.DESTINY2_SHADOWKEEP_2601:
                // This sucks. A lot.
                var valsSK = PackageResourcer.Get().GetAllHashes<SUnkActivity_SK>();
                foreach (var val in valsSK)
                {
                    Tag<SUnkActivity_SK> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_SK>(val);
                    string activityName = PackageResourcer.Get().GetActivityName(activity.FileHash).Split(':')[1];

                    if (tag.TagData.ActivityDevName.Value.Contains(activityName)) //This is probably really bad...
                    {
                        DisplayEntBubble displayActivitySK = new();
                        displayActivitySK.Name = $"{PackageResourcer.Get().GetActivityName(val)}";
                        displayActivitySK.Hash = $"{tag.Hash}";
                        displayActivitySK.ParentHash = $"{activity.FileHash}";
                        displayActivitySK.LoadType = DisplayEntBubble.Type.Activity;
                        displayActivitySK.Data = displayActivitySK;
                        maps.Add(displayActivitySK);
                    }
                }
                break;

            case TigerStrategy.DESTINY1_RISE_OF_IRON:
                // This also sucks. A lot.
                var valsROI = PackageResourcer.Get().GetD1Activities();
                foreach (var val in valsROI)
                {
                    if (val.Value == "16068080")
                    {
                        Tag<SUnkActivity_ROI> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_ROI>(val.Key);

                        string activityName = PackageResourcer.Get().GetActivityName(activity.FileHash).Split(':')[1];
                        if (tag.TagData.ActivityDevName.Value.Contains(activityName))
                        {
                            DisplayEntBubble displayActivityROI = new();
                            displayActivityROI.Name = $"{PackageResourcer.Get().GetActivityName(val.Key).Split(":").First()}";
                            displayActivityROI.Hash = $"{tag.Hash}";
                            displayActivityROI.ParentHash = $"{activity.FileHash}";
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
            FileHash hash = new FileHash(tagData.Hash);
            _currentBubble = tagData;

            Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(hash);
            await Task.Run(() => PopulateEntityContainerList(bubbleMaps));
        }
        else
        {
            MainWindow.Progress.SetProgressStages(new() { $"Loading Activity Entities for {tagData.Name}" });
            FileHash hash = new FileHash(tagData.Hash);
            if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999)
            {
                FileHash parentHash = new FileHash(tagData.ParentHash);
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

                    ConcurrentBag<FileHash> sMapDataTables = new ConcurrentBag<FileHash>(map.TagData.MapDataTables.Select(entry => entry.MapDataTable.Hash));
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
        ConcurrentBag<DisplayEntityMap> items = new ConcurrentBag<DisplayEntityMap>();
        Parallel.ForEach(bubbleMaps.TagData.MapResources, m =>
        {
            if (m.GetMapContainer().TagData.MapDataTables.Count > 0)
            {
                DisplayEntityMap entityMap = new();
                entityMap.Name = $"{m.GetMapContainer().Hash}";
                entityMap.Hash = m.GetMapContainer().Hash;
                entityMap.Count = m.GetMapContainer().TagData.MapDataTables.Count;
                entityMap.EntityType = DisplayEntityMap.Type.Map;
                entityMap.DataTables = m.GetMapContainer().TagData.MapDataTables.Select(entry => entry.MapDataTable.Hash).ToList();
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
        ConcurrentBag<DisplayEntityMap> items = new ConcurrentBag<DisplayEntityMap>();

        foreach (var entry in activity.EnumerateActivityEntities(UnkActivity))
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
        ConcurrentBag<DisplayEntityList> items = new ConcurrentBag<DisplayEntityList>();
        ConcurrentDictionary<FileHash, ConcurrentBag<ulong>> entities = new();

        Parallel.ForEach(dataTables, data =>
        {
            List<SMapDataEntry> dataEntries = new();
            if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON && data.GetReferenceHash().Hash32 == 0x808003F6) //F6038080
                dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SF6038080>(data).TagData.EntityResource.CollapseIntoDataEntry());
            else
                dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SMapDataTable>(data).TagData.DataEntries);

            dataEntries.ForEach(entry =>
            {
                if (!entities.ContainsKey(entry.GetEntityHash()))
                {
                    entities[entry.GetEntityHash()] = new ConcurrentBag<ulong>();
                }
                entities[entry.GetEntityHash()].Add(entry.WorldID);
            });
        });

        entities.AsParallel().ForAll(entityHash =>
        {
            Entity entity = FileResourcer.Get().GetFile(typeof(Entity), entityHash.Key);
            if (entity.HasGeometry())
            {
                foreach (var namedEnt in entityHash.Value)
                {
                    if (worldIDs is not null && worldIDs.ContainsKey(namedEnt))
                    {
                        var Name = worldIDs[namedEnt].Name;
                        var SubName = worldIDs[namedEnt].SubName;
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
            AutomatedExporter.SaveInteropUnrealPythonFile(savePath, hash, AutomatedExporter.ImportType.Map, _config.GetOutputTextureFormat(), _config.GetSingleFolderMapsEnabled());
        }
    }

    private static void ExtractDataTables(List<FileHash> dataTables, string hash, string savePath)
    {
        GlobalExporterScene globalScene = Tiger.Exporters.Exporter.Get().GetOrCreateGlobalScene();
        // todo these scenes can be combined
        ExporterScene dynamicPointScene = Exporter.Get().CreateScene($"{hash}_EntityPoints", ExportType.EntityPoints);
        ExporterScene dynamicScene = Exporter.Get().CreateScene($"{hash}_Entities", ExportType.Map);
        ExporterScene skyScene = Exporter.Get().CreateScene($"{hash}_SkyEnts", ExportType.Map);
        ExporterScene decoratorScene = Exporter.Get().CreateScene($"{hash}_Decorators", ExportType.Map);

        Parallel.ForEach(dataTables, data =>
        {
            if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON && data.GetReferenceHash().Hash32 == 0x808003F6)
            {
                var dataEntries = FileResourcer.Get().GetSchemaTag<SF6038080>(data).TagData.EntityResource.CollapseIntoDataEntry();
                foreach (var entry in dataEntries)
                {
                    Entity entity = FileResourcer.Get().GetFile<Entity>(entry.GetEntityHash());
                    if (entity.HasGeometry())
                    {
                        dynamicScene.AddMapEntity(entry, entity);
                        entity.SaveMaterialsFromParts(dynamicScene, entity.Load(ExportDetailLevel.MostDetailed));
                    }
                    else
                        dynamicPointScene.AddEntityPoints(entry);
                }
            }
            else
            {
                var dataTable = FileResourcer.Get().GetSchemaTag<SMapDataTable>(data);
                dataTable.TagData.DataEntries.ForEach(entry =>
                {
                    Entity entity = FileResourcer.Get().GetFile<Entity>(entry.GetEntityHash());
                    if (entity.HasGeometry())
                    {
                        dynamicScene.AddMapEntity(entry, entity);
                        entity.SaveMaterialsFromParts(dynamicScene, entity.Load(ExportDetailLevel.MostDetailed));
                    }
                    else
                    {
                        //if (entry.Translation.ToVec3() == Tiger.Schema.Vector3.Zero)
                        //    System.Console.WriteLine($"World origin resource {dataTable.Hash} Resource? {entry.DataResource.GetValue(dataTable.GetReader())}");
                        dynamicPointScene.AddEntityPoints(entry);
                    }

                    switch (entry.DataResource.GetValue(dataTable.GetReader()))
                    {
                        case SMapSkyEntResource skyResource:
                            skyResource.SkyEntities.Load();
                            if (skyResource.SkyEntities.TagData.Entries is null)
                                return;

                            foreach (var element in skyResource.SkyEntities.TagData.Entries)
                            {
                                if (element.Model.TagData.Model is null || (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 && element.Unk70 == 5))
                                    continue;

                                System.Numerics.Matrix4x4 matrix = element.Transform.ToSys();

                                System.Numerics.Vector3 scale = new();
                                System.Numerics.Vector3 trans = new();
                                Quaternion quat = new();
                                System.Numerics.Matrix4x4.Decompose(matrix, out scale, out quat, out trans);

                                skyScene.AddMapModel(element.Model.TagData.Model,
                                    new Tiger.Schema.Vector4(trans.X, trans.Y, trans.Z, 1.0f),
                                    new Tiger.Schema.Vector4(quat.X, quat.Y, quat.Z, quat.W),
                                    new Tiger.Schema.Vector3(scale.X, scale.Y, scale.Z));

                                foreach (DynamicMeshPart part in element.Model.TagData.Model.Load(ExportDetailLevel.MostDetailed, null))
                                {
                                    if (part.Material == null) continue;
                                    skyScene.Materials.Add(new ExportMaterial(part.Material));
                                }
                            }
                            break;

                        case SMapCubemapResource cubemap:
                            cubemap.CubemapPosition = entry.Translation; // Shouldn't be modifiying things like this but eh
                            dynamicScene.AddCubemap(cubemap);
                            break;

                        case SMapLightResource mapLight:
                            mapLight.Lights?.Load();
                            if (mapLight.Lights is not null)
                                mapLight.Lights.LoadIntoExporter(dynamicScene, savePath);
                            break;

                        case SMapShadowingLightResource shadowingLight:
                            shadowingLight.ShadowingLight?.Load();
                            if (shadowingLight.ShadowingLight is not null)
                                shadowingLight.ShadowingLight.LoadIntoExporter(dynamicScene, entry, savePath);
                            break;

                        case SMapDecalsResource decals:
                            decals.MapDecals?.Load();
                            if (decals.MapDecals is null)
                                return;

                            dynamicScene.AddDecals(decals);
                            foreach (var item in decals.MapDecals.TagData.DecalResources)
                            {
                                if (item.StartIndex >= 0 && item.StartIndex < decals.MapDecals.TagData.Locations.Count)
                                {
                                    for (int i = item.StartIndex; i < item.StartIndex + item.Count && i < decals.MapDecals.TagData.Locations.Count; i++)
                                    {
                                        dynamicScene.Materials.Add(new ExportMaterial(item.Material));
                                    }
                                }
                            }
                            break;

                        case SDecoratorMapResource decorator:
                            decorator.Decorator?.Load();
                            decorator.Decorator.LoadIntoExporter(decoratorScene, savePath);
                            break;

                        case SMapWaterDecal waterDecal:
                            dynamicScene.AddMapModel(waterDecal.Model,
                            entry.Translation,
                            entry.Rotation,
                            new Tiger.Schema.Vector3(entry.Translation.W));
                            foreach (DynamicMeshPart part in waterDecal.Model.Load(ExportDetailLevel.MostDetailed, null))
                            {
                                if (part.Material == null) continue;
                                dynamicScene.Materials.Add(new ExportMaterial(part.Material));
                            }
                            break;

                        case SMapAtmosphere atmosphere:
                            globalScene.AddToGlobalScene(atmosphere);
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
        var lod = MapControl.ModelView.GetSelectedLod();
        if (dc.Name == "Select all")
        {
            var items = EntityContainerList.Items.Cast<DisplayEntityMap>().Where(x => x.Name != "Select all");
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
        if (dc.DisplayName == "All Entities")
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
                    MapControl.LoadMap(datatable, lod, true);
                    MainWindow.Progress.CompleteStage();
                }
            });
        }
        else
        {
            Entity entity = FileResourcer.Get().GetFile<Entity>(dc.Hash);
            MainWindow.Progress.SetProgressStages(new List<string> { $"Loading Entity to UI: {entity.Hash}" });
            List<Entity> entities = new List<Entity> { entity };
            entities.AddRange(entity.GetEntityChildren());
            await Task.Run(() =>
            {
                MapControl.LoadEntity(entities, _globalFbxHandler);
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
            var items = dc.Parent;
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
                foreach (var datatable in items)
                {
                    List<SMapDataEntry> dataEntries = new();
                    if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON && datatable.GetReferenceHash().Hash32 == 0x808003F6) //F6038080
                        dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SF6038080>(datatable).TagData.EntityResource.CollapseIntoDataEntry());
                    else
                        dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SMapDataTable>(datatable).TagData.DataEntries);

                    foreach (var entry in dataEntries)
                    {
                        Entity entity = FileResourcer.Get().GetFile<Entity>(entry.GetEntityHash());
                        if (entity.HasGeometry())
                        {
                            List<Entity> entities = new List<Entity> { entity };
                            entities.AddRange(entity.GetEntityChildren());
                            EntityView.Export(entities, entity.Hash, ExportTypeFlag.Full);
                        }
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
                List<Entity> entities = new List<Entity> { entity };
                entities.AddRange(entity.GetEntityChildren());
                EntityView.Export(entities, entity.Hash, ExportTypeFlag.Full);
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
    public string ParentHash { get; set; }
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
    public string Hash { get; set; }
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
