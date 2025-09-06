using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using ConcurrentCollections;
using Newtonsoft.Json;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;
using Tiger.Schema.Activity.DESTINY2_SHADOWKEEP_2601;
using Tiger.Schema.Entity;
using static Charm.PackageList;

namespace Charm;

public partial class EntityListView : UserControl
{
    private ConfigSubsystem Config = TigerInstance.GetSubsystem<ConfigSubsystem>();
    private ConcurrentBag<EntityItem> Entities = new();
    private ConcurrentDictionary<string, List<string>> NamedEntities = new();

    private int SortByIndex = 4;
    private FileHash _currentEntity;

    public EntityListView()
    {
        InitializeComponent();
#if DEBUG
        // I can't be asked to fix these seemingly harmless but lag inducing xaml binding errors
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif

        PackageList.OnSearchBarChanged += (s, e) => RefreshPackageListCustom();
        PackageList.PackageItemChecked += async (s, item) =>
        {
            await LoadEntityList(item);
        };
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
    }

    public async void LoadContent()
    {
        EntityViewer.Visibility = Visibility.Hidden;
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Caching Entity Names, may take some time.",
            "Creating Entity List.",
        });
        await Task.Run(() =>
        {
            if (NamedEntities.IsEmpty)
                NamedEntities = TryGetEntityNames().Result;
            MainWindow.Progress.CompleteStage();
        });

        await PackageList.MakePackageItems<Entity>();
        await CleanPackageList();
        MainWindow.Progress.CompleteStage();

        CreateFilterOptions();
    }

    private async Task CleanPackageList()
    {
        ConcurrentBag<PackageItem> cleanList = new();
        TigerHash tagClass = new(Helpers.GetClassHashForStrategy(typeof(S8A6D8080), Strategy.CurrentStrategy));

        await Task.Run(() =>
        {
            foreach (var pkg in PackageList.PackageItems)
            {
                ConcurrentHashSet<FileHash> newHashes = new();
                Parallel.ForEach(pkg.Hashes, hash =>
                {
                    if (hash.GetReferenceHash().IsInvalid())
                        return;

                    Tag<SEntity> entityTag = FileResourcer.Get().GetSchemaTag<SEntity>(hash);
                    foreach (var resource in entityTag.TagData.UnkResources.Enumerate(entityTag.GetReader()))
                    {
                        if (resource.Unk10ClassHash == tagClass && !newHashes.Contains(hash))
                        {
                            newHashes.Add(hash);
                            break;
                        }
                    }
                });

                if (newHashes.Count > 0)
                {
                    cleanList.Add(new PackageItem
                    {
                        Name = pkg.Name,
                        ID = pkg.ID,
                        Count = newHashes.Count(),
                        Hashes = newHashes,
                        Content = pkg.Content
                    });
                }
            }
        });
        Dispatcher.Invoke(() =>
        {
            PackageList.PackageItems = cleanList;
            PackageList.RefreshPackageList();
        });
    }

    private void CreateFilterOptions()
    {
        ComboBoxControl sortBy = new();
        sortBy.Text = "Sort By";
        sortBy.FontSize = 14;
        sortBy.Combobox.MinWidth = 175;
        sortBy.Combobox.ItemsSource = new List<ComboBoxItem>()
        {
            new() { Content = "Hash ↓", Tag = 4 },
            new() { Content = "Hash ↑", Tag = 3 },
            new() { Content = "Name ↓", Tag = 2 },
            new() { Content = "Name ↑", Tag = 1 }
        };
        if (sortBy.Combobox.SelectedIndex == -1)
        {
            sortBy.Combobox.SelectedIndex = 0;
        }

        sortBy.Combobox.SelectionChanged += SortBy_OnSelectionChanged;
        FilterOptions.Children.Add(sortBy);
    }

    private async Task LoadEntityList(PackageItem item)
    {
        await Task.Run(() =>
        {
            Dispatcher.Invoke(() => EntityViewer.Visibility = Visibility.Hidden);
            MainWindow.Progress.SetProgressStages(new List<string>
            {
                "Loading Entities."
            });

            if (Entities.Count != 0)
                Entities.Clear();

            Parallel.ForEach(item.Hashes, hash =>
            {
                if (hash.GetReferenceHash().IsInvalid())
                    return;

                var entity = FileResourcer.Get().GetFile<Entity>(hash);
                if (entity.HasGeometry())
                {
                    string entityName = entity.EntityName != null ? entity.EntityName : entity.Hash;

                    // Most of the time the most specific entity name comes from a map resource (bosses usually)
                    if (NamedEntities.ContainsKey(hash))
                    {
                        if (!NamedEntities[hash].Contains(entityName) && entityName != entity.Hash)
                            NamedEntities[hash].Add(entityName);

                        foreach (string entry in NamedEntities[entity.Hash])
                        {
                            Entities.Add(new()
                            {
                                Hash = hash,
                                DisplayName = entry,
                                ResourceCount = entity.TagData.EntityResources.Count,
                                HasSkeleton = entity.Skeleton != null,
                            });
                        }
                    }
                    else
                    {
                        Entities.Add(new()
                        {
                            Hash = hash,
                            DisplayName = $"[{hash}]",
                            ResourceCount = entity.TagData.EntityResources.Count,
                            HasSkeleton = entity.Skeleton != null,
                        });
                    }
                }
            });

            MainWindow.Progress.CompleteStage();
        });

        RefreshEntityList();
    }

    private void RefreshEntityList()
    {
        if (Entities == null)
            return;
        if (Entities.IsEmpty)
        {
            EntityList.ItemsSource = null;
            return;
        }

        string searchStr = EntitySearchBox.Text;

        uint parsedHash = 0;
        bool isHash = Helpers.ParseHash(searchStr, out parsedHash);

        var displayItems = new ConcurrentBag<EntityItem>();
        Parallel.ForEach(Entities, ent =>
        {
            if ((isHash && ent.Hash.Hash32 == parsedHash)
            || ent.Hash.ToString().Contains(searchStr, StringComparison.OrdinalIgnoreCase)
            || ent.DisplayName.Contains(searchStr, StringComparison.OrdinalIgnoreCase))
            {
                displayItems.Add(ent);
            }
        });

        List<EntityItem> items = displayItems.ToList();

        items = SortByIndex switch
        {
            4 => items.OrderByDescending(x => x.Hash).ToList(),
            3 => items.OrderBy(x => x.Hash).ToList(),
            2 => items.OrderByDescending(x => x.DisplayName).ToList(),
            1 => items.OrderBy(x => x.DisplayName).ToList(),
            _ => items
        };

        EntityList.ItemsSource = items;
        BulkExportButton.IsEnabled = items.Count > 0;
    }

    private void Entity_OnClick(object sender, RoutedEventArgs e)
    {
        if ((sender as RadioButton) is null)
            return;

        EntityItem item = ((RadioButton)sender).DataContext as EntityItem;
        LoadEntity(item.Hash);
    }

    private void LoadEntity(FileHash hash)
    {
        ExportButton.IsEnabled = true;
        EntityViewer.LoadEntity(hash);
        EntityViewer.ModelView.SetModelFunction(() => EntityViewer.LoadEntity(hash));
        Dispatcher.Invoke(() => EntityViewer.Visibility = Visibility.Visible);
        _currentEntity = hash;
    }

    private void EntitySearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshEntityList();
    }

    private void SortBy_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SortByIndex = (int)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        RefreshEntityList();
    }

    private async void BulkExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (EntityList.ItemsSource is not IEnumerable<EntityItem> items || !items.Any())
            return;

        // Hopefully this works fine, and not just for me
        var exportItems = items.DistinctBy(x => x.Hash);
        if (exportItems.Count() == 0)
            return;

        bool exportChildren = ExportChildren.IsChecked.Value;
        Dispatcher.Invoke(() =>
        {
            EntityViewer.ModelView.Visibility = Visibility.Hidden;
        });

        string pkgName = PackageResourcer.Get().GetPackage(items.First().Hash.PackageId).GetPackageMetadata().Name.Split(".")[0];
        string savePath = $"{Config.GetExportSavePath()}/{pkgName}";
        Directory.CreateDirectory(savePath);

        MainWindow.Progress.SetProgressStages(exportItems.Select((x, i) => $"Exporting {i + 1}/{exportItems.Count()}: {x.Hash}").ToList());
        await Task.Run(() =>
        {
            foreach (var item in exportItems)
            {
                var curEnt = FileResourcer.Get().GetFile<Entity>(item.Hash);
                List<Entity> entities = new() { curEnt };
                if (exportChildren)
                    entities.AddRange(curEnt.GetEntityChildren());

                EntityView.Export(entities, curEnt.Hash, savePath);
                MainWindow.Progress.CompleteStage();
            }
        });

        Dispatcher.Invoke(() =>
        {
            EntityViewer.ModelView.Visibility = Visibility.Visible;
            NotificationBanner notify = new()
            {
                Icon = "☑️",
                Title = "Bulk Export Complete",
                Description = $"Exported {exportItems.Count()} Entites to \"{savePath}\"",
                Style = NotificationBanner.PopupStyle.Information
            };
            notify.Show();
        });
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentEntity is null)
            return;

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            $"Exporting Entity {_currentEntity}",
        });

        var curEnt = FileResourcer.Get().GetFile<Entity>(_currentEntity);
        List<Entity> entities = new() { curEnt };
        Dispatcher.Invoke(() =>
        {
            if (ExportChildren.IsChecked.Value == true)
                entities.AddRange(curEnt.GetEntityChildren());

            EntityViewer.ModelView.Visibility = Visibility.Hidden;
        });

        await Task.Run(() =>
        {
            EntityView.Export(entities, _currentEntity);
            MainWindow.Progress.CompleteStage();
        });

        Dispatcher.Invoke(() =>
        {
            EntityViewer.ModelView.Visibility = Visibility.Visible;
            NotificationBanner notify = new()
            {
                Icon = "☑️",
                Title = "Export Complete",
                Description = $"Exported Entity {_currentEntity} to \"{ConfigSubsystem.Get().GetExportSavePath()}\\{_currentEntity}\\\"",
                Style = NotificationBanner.PopupStyle.Information
            };
            notify.Show();
        });
    }

    private CancellationTokenSource _EntitySelectionCts;
    private async void EntityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _EntitySelectionCts?.Cancel();
        _EntitySelectionCts = new CancellationTokenSource();
        var token = _EntitySelectionCts.Token;

        try
        {
            await Task.Delay(100, token); // Debounce time
            if (token.IsCancellationRequested)
                return;

            Dispatcher.Invoke(() =>
            {
                if (EntityList.SelectedIndex >= 0)
                {
                    var container = EntityList.ItemContainerGenerator.ContainerFromIndex(EntityList.SelectedIndex);
                    RadioButton currentButton = UIHelper.GetChildOfType<RadioButton>(container);
                    if (currentButton != null)
                        currentButton.IsChecked = true;
                }
            });
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void RefreshPackageListCustom()
    {
        if (PackageList.PackageItems == null)
            return;
        if (PackageList.PackageItems.IsEmpty)
            return;

        string searchStr = PackageList.SearchBox.Text;

        uint parsedHash = 0;
        bool isHash = Helpers.ParseHash(searchStr, out parsedHash);

        var displayItems = new ConcurrentBag<PackageItem>();
        Parallel.ForEach(PackageList.PackageItems, pkg =>
        {
            if (isHash && pkg.Hashes.Any(x => x.Hash32 == parsedHash))
            {
                IEnumerable<FileHash> hashes = pkg.Hashes.Where(x => x.Hash32 == parsedHash);
                displayItems.Add(new PackageItem
                {
                    Name = pkg.Name,
                    ID = pkg.ID,
                    Count = hashes.Count(),
                    Hashes = new(hashes),
                    Content = pkg.Content
                });
            }
            else if (pkg.Name.Contains(searchStr, StringComparison.OrdinalIgnoreCase))
            {
                displayItems.Add(pkg);
            }
            else
            {
                // Only include hashes where NamedEntities contains the hash and at least one name matches the search string
                var filteredHashes = pkg.Hashes
                    .Where(x => NamedEntities.TryGetValue(x, out var names) &&
                                names.Any(n => n is not null && n.Contains(searchStr, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (filteredHashes.Count > 0)
                {
                    displayItems.Add(new PackageItem
                    {
                        Name = pkg.Name,
                        ID = pkg.ID,
                        Count = filteredHashes.Count,
                        Hashes = new(filteredHashes),
                        Content = pkg.Content
                    });
                }
            }
        });

        List<PackageItem> items = displayItems.OrderBy(x => x.Name).ToList();
        PackageList.PackageListView.ItemsSource = items;
    }

    private async void Tag_Loaded(object sender, RoutedEventArgs e)
    {
        //if (sender is Button btn && btn.DataContext is EntityItem tag)
        //{
        //    await tag.DrawWaveform();
        //    btn.Tag = tag;
        //}
    }

    private void Tag_Unloaded(object sender, RoutedEventArgs e)
    {
        //if (sender is Button btn && btn.DataContext is EntityItem tag)
        //{
        //    tag.ClearWaveform();
        //}
    }

    private class EntityItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public FileHash Hash { get; set; }

        public int ResourceCount { get; set; } = 0;
        public bool HasSkeleton { get; set; } = false;

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    private async Task<ConcurrentDictionary<string, List<string>>> TryGetEntityNames()
    {
        NamedEntities Ents = new()
        {
            EntityNames = new()
        };

        if (!File.Exists($"./EntityNames.json"))
            File.WriteAllText($"./EntityNames.json", JsonConvert.SerializeObject(Ents, Formatting.Indented));

        try
        {
            Ents = JsonConvert.DeserializeObject<NamedEntities>(File.ReadAllText($"./EntityNames.json"));
        }
        catch (JsonSerializationException) // Likely old version of the json
        {
            File.Delete($"./EntityNames.json");
            File.WriteAllText($"./EntityNames.json", JsonConvert.SerializeObject(Ents, Formatting.Indented));
        }

        if (Ents.EntityNames.TryGetValue(Strategy.CurrentStrategy, out ConcurrentDictionary<string, List<string>>? names) && !Ents.EntityNames[Strategy.CurrentStrategy].IsEmpty)
        {
            return names;
        }
        else
        {
            ConcurrentDictionary<StringHash, string> entityNameCache = new();

            Stopwatch stopwatch = Stopwatch.StartNew();

            Ents.EntityNames[Strategy.CurrentStrategy] = new();
            if (Strategy.IsD1())
            {
                // Name and entity is in a map data table
                ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<SD9128080>();
                Parallel.ForEach(vals, val =>
                {
                    Tag<SD9128080> entry = FileResourcer.Get().GetSchemaTag<SD9128080>(val);
                    foreach (SD6148080 a in entry.TagData.Unk20)
                    {
                        foreach (S48138080 b in a.Unk08)
                        {
                            if (b.Pointer.GetValue(entry.GetReader()) is SMapDataEntry datatable)
                            {
                                if (datatable.DataResource.GetValue(entry.GetReader()) is S33138080 name)
                                {
                                    if (name.EntityName.IsValid())
                                    {
                                        FileHash entityHash = datatable.Entity.Hash;
                                        string entityName = GlobalStrings.Get().GetString(name.EntityName);

                                        Ents.AddEntityName(Strategy.CurrentStrategy, entityHash, entityName);
                                    }
                                }
                            }
                        }
                    }
                });

                // Name is in an EntityResource, with the entity in a map data table in that EntityResource
                ConcurrentHashSet<FileHash> vals2 = await PackageResourcer.Get().GetAllHashesAsync<SF6038080>();
                Parallel.ForEach(vals2, val =>
                {
                    Tag<SF6038080> entry = FileResourcer.Get().GetSchemaTag<SF6038080>(val);
                    if (entry.TagData.EntityResource is not null)
                    {
                        if (entry.TagData.EntityResource.TagData.Unk10.GetValue(entry.TagData.EntityResource.GetReader()) is S2E098080)
                        {
                            var resource = (SDD078080)entry.TagData.EntityResource.TagData.Unk18.GetValue(entry.TagData.EntityResource.GetReader());
                            foreach (SMapDataEntry dataentry in resource.DataEntries)
                            {
                                if (dataentry.Entity.Hash.IsValid())
                                {
                                    FileHash entityHash = dataentry.Entity.Hash;
                                    string entityName = resource.DevName.Value ?? entityHash.ToString();

                                    Ents.AddEntityName(Strategy.CurrentStrategy, entityHash, entityName);
                                }
                            }
                        }
                    }
                });
            }
            else if (Strategy.IsPreBL()) // SK
            {
                ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<S149B8080>();
                Parallel.ForEach(vals, val =>
                {
                    //Console.WriteLine($"Resource {val}");
                    Tag<S149B8080> entry = FileResourcer.Get().GetSchemaTag<S149B8080>(val);
                    if (entry.TagData.EntityResource is not null)
                    {
                        EntityResource resource = entry.TagData.EntityResource;
                        if (resource.TagData.Unk10.GetValue(resource.GetReader()) is S3B9A8080)
                        {
                            var D2Class8F948080 = (S8F948080)resource.TagData.Unk18.GetValue(resource.GetReader());
                            foreach (S56838080 entry2 in D2Class8F948080.UnkA8)
                            {
                                List<DynamicArray<S58838080>> tables = new() { entry2.Table1, entry2.Table2, entry2.Table3, entry2.Table4, entry2.Table5, entry2.Table6 };

                                foreach (DynamicArray<S58838080> datatable in tables)
                                {
                                    foreach (S58838080 dataEntry in datatable)
                                    {
                                        SMapDataEntry? value = dataEntry.Datatable.Value;
                                        if (value is null)
                                            continue;

                                        if (value.Value.DataResource.GetValue(resource.GetReader()) is SB67E8080 name)
                                        {
                                            if (name.EntityName.IsValid())
                                            {
                                                FileHash entityHash = value.Value.Entity.Hash;
                                                string entityName = GlobalStrings.Get().GetString(name.EntityName);

                                                Ents.AddEntityName(Strategy.CurrentStrategy, entityHash, entityName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }
            else if (Strategy.IsPostBL()) // WQ+
            {
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

                // Name and entity is in a map data table
                ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<SMapDataTable>();
                Log.Debug($"{vals.Count} Map Data Tables");

                await Task.Run(() =>
                {
                    Parallel.ForEach(vals, parallelOptions, val =>
                    {
                        if (!val.ContainsHash(0x80808019))
                            return;

                        var entry = FileResourcer.Get().GetSchemaTag<SMapDataTable>(val, shouldCache: false);
                        foreach (var dataEntry in entry.TagData.DataEntries)
                        {
                            if (dataEntry.DataResource.GetValue(entry.GetReader()) is S19808080 name && name.EntityName.IsValid())
                            {
                                string entityName = entityNameCache.GetOrAdd(name.EntityName, key => GlobalStrings.Get().GetString(key));
                                Ents.AddEntityName(Strategy.CurrentStrategy, dataEntry.Entity.Hash, entityName);
                            }
                        }
                    });
                });

                stopwatch.Stop();
                Log.Debug($"Stage 1: Map Data Table Entity Names took {stopwatch.Elapsed.TotalSeconds} seconds to process.");
                stopwatch = Stopwatch.StartNew();

                // Name is in an EntityResource, with the entity in a map data table in that EntityResource
                ConcurrentHashSet<FileHash> resources = await PackageResourcer.Get().GetAllHashesAsync<EntityResource>();
                Log.Debug($"{resources.Count} Entity Resources");

                await Task.Run(() =>
                {
                    Parallel.ForEach(resources, parallelOptions, val =>
                    {
                        if (!val.ContainsHash(0x8080470E))
                            return;

                        var resource = FileResourcer.Get().GetFile<EntityResource>(val, shouldCache: false);
                        var sb546 = (SB5468080)resource.TagData.Unk18.GetValue(resource.GetReader());
                        foreach (S96468080 entry in sb546.Unk80)
                        {
                            if (entry.DataTable is null)
                                continue;

                            foreach (SMapDataEntry dataEntry in entry.DataTable.TagData.DataEntries)
                            {
                                if (dataEntry.DataResource.GetValue(entry.DataTable.GetReader()) is S19808080 name)
                                {
                                    if (entry.Name.IsValid())
                                    {
                                        string entityName = entityNameCache.GetOrAdd(entry.Name, key => GlobalStrings.Get().GetString(key));
                                        Ents.AddEntityName(Strategy.CurrentStrategy, dataEntry.Entity.Hash, entityName);
                                    }
                                }
                            }
                        }
                    });
                });

                stopwatch.Stop();
                Log.Debug($"Stage 2: Entity Resources took {stopwatch.Elapsed.TotalSeconds} seconds to process.");
            }

            stopwatch = Stopwatch.StartNew();

            // Lastly gonna get all entities and see if their "default" name is valid and add those
            if (Ents.EntityNames.TryGetValue(Strategy.CurrentStrategy, out var namedEnts))
            {
                TigerHash tagClass = new(Helpers.GetClassHashForStrategy(typeof(S8A6D8080), Strategy.CurrentStrategy));
                var hashes = await PackageResourcer.Get().GetAllHashesAsync(typeof(SEntity));
                Log.Debug($"{hashes.Count} Entities");

                Parallel.ForEach(hashes, hash =>
                {
                    if (!hash.ContainsHash(tagClass.Hash32))
                        return;

                    var entity = FileResourcer.Get().GetFile<Entity>(hash, shouldCache: false);
                    string entityName = entity.EntityName != null ? entity.EntityName : entity.Hash;
                    if (entityName != entity.Hash)
                    {
                        Ents.AddEntityName(Strategy.CurrentStrategy, hash, entityName);
                    }
                });
            }

            stopwatch.Stop();
            Log.Debug($"Stage 3: All Entity Base Names took {stopwatch.Elapsed.TotalSeconds} seconds to process.");

            File.WriteAllText($"./EntityNames.json", JsonConvert.SerializeObject(Ents, Formatting.Indented));
        }

        return Ents.EntityNames[Strategy.CurrentStrategy];
    }
}
