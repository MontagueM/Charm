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
using Charm.Shared;
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
    private Entity _currentEntity;

    private IRenderer Renderer = null;
    private EntityView RendererBasic = null;
    private EntityListViewType _loadType = EntityListViewType.Entities;

    public EntityListView(EntityListViewType loadType = EntityListViewType.Entities)
    {
        InitializeComponent();
#if DEBUG
        // I can't be asked to fix these seemingly harmless but lag inducing xaml binding errors
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif
        _loadType = loadType;

        PackageList.OnSearchBarChanged += (s, e) => RefreshPackageListCustom();
        PackageList.PackageItemChecked += async (s, item) =>
        {
            if (_loadType == EntityListViewType.Entities)
                await LoadEntityList(item);
            else
                await LoadNamedBag(item);
        };

        if (IRenderer.CanUseRenderer())
        {
            Renderer = IRenderer.CreateRenderer(nameof(EntityListView));
            RendererGrid.Children.Add(Renderer as UserControl);
        }
        else
        {
            RendererBasic = new EntityView();
            RendererGrid.Children.Add(RendererBasic);
            HideBasicRenderer();
        }
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
    }

    public async void LoadContent()
    {
        if (_loadType == EntityListViewType.Entities)
            CreateEntityList();
        else
            await CreateNamedBagsList();

        CreateFilterOptions();
    }

    private async void CreateEntityList()
    {
        HideBasicRenderer();
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
    }

    private async Task CleanPackageList()
    {
        ConcurrentBag<PackageItem> cleanList = new();
        TigerHash tagClass = new(Helpers.GetClassHashForStrategy(typeof(S80806D8A), Strategy.CurrentStrategy));

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
        sortBy.TextFontSize = 16;
        sortBy.Box.MinWidth = 175;
        sortBy.Box.ItemsSource = new List<ComboBoxItem>()
        {
            new() { Content = "Hash ↓", Tag = 4 },
            new() { Content = "Hash ↑", Tag = 3 },
            new() { Content = "Name ↓", Tag = 2 },
            new() { Content = "Name ↑", Tag = 1 }
        };
        if (sortBy.Box.SelectedIndex == -1)
        {
            sortBy.Box.SelectedIndex = 0;
        }

        sortBy.Box.SelectionChanged += SortBy_OnSelectionChanged;
        FilterOptions.Children.Add(sortBy);
    }

    private async Task LoadEntityList(PackageItem item)
    {
        await Task.Run(() =>
        {
            Dispatcher.Invoke(() => HideBasicRenderer());
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
                                ResourceCount = entity.Components.Count(),
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
                            ResourceCount = entity.Components.Count(),
                            HasSkeleton = entity.Skeleton != null,
                        });
                    }
                }
            });

            MainWindow.Progress.CompleteStage();
            Dispatcher.Invoke(() => ShowBasicRenderer());
        });

        RefreshEntityList();
    }

    #region Named Bags
    private async Task CreateNamedBagsList()
    {
        if (PackageList.PackageItems != null)
            return;

        HideBasicRenderer();
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Creating Named Bags List.",
        });

        await Task.Run(() =>
        {
            PackageList.PackageItems = new();
            if (Strategy.IsPreBL())
            {
                ConcurrentHashSet<FileHash> vals = PackageResourcer.Get().GetAllHashes<S80808930>();
                Parallel.ForEach(vals, val =>
                {
                    Tag<S80808930> bag = FileResourcer.Get().GetSchemaTag<S80808930>(val);
                    if (bag.TagData.Entries.Count == 0)
                        return;

                    PackageList.PackageItems.Add(new PackageItem
                    {
                        Name = bag.Hash,
                        ID = bag.Hash.PackageId,
                        Count = bag.TagData.Entries.Count,
                        Hashes = new() { val },
                        Content = PackageItemContents.NamedBag,
                        DisplayHash = val
                    });
                });
            }
            else
            {
                ConcurrentHashSet<FileHash> vals = PackageResourcer.Get().GetAllHashes<S8080471D>();
                Parallel.ForEach(vals, val =>
                {
                    Tag<S8080471D> dgtbParent = FileResourcer.Get().GetSchemaTag<S8080471D>(val);

                    if (Strategy.IsBL())
                    {
                        if (dgtbParent.TagData.DestinationGlobalTagBagBL is not null)
                        {
                            var bag = dgtbParent.TagData.DestinationGlobalTagBagBL;
                            string fullPath = dgtbParent.TagData.DestinationGlobalTagBagNameBL ?? "";
                            string name = string.IsNullOrEmpty(fullPath)
                                    ? $"{bag.Hash}"
                                    : Path.GetFileNameWithoutExtension(fullPath).Split(".")[0];
                            PackageList.PackageItems.Add(new PackageItem
                            {
                                Name = name,
                                ID = bag.Hash.PackageId,
                                Hashes = new() { bag.Hash },
                                Content = PackageItemContents.NamedBag,
                                DisplayHash = bag.Hash
                            });
                        }
                    }
                    else
                    {
                        if (dgtbParent.TagData.DestinationGlobalTagBags.Count == 0)
                            return;

                        foreach (S808059D3 bag in dgtbParent.TagData.DestinationGlobalTagBags)
                        {
                            if (!bag.DestinationGlobalTagBag.IsValid())
                                continue;

                            string name = bag.DestinationGlobalTagBagName;
                            PackageList.PackageItems.Add(new PackageItem
                            {
                                Name = name,
                                ID = bag.DestinationGlobalTagBag.PackageId,
                                Hashes = new() { bag.DestinationGlobalTagBag },
                                Content = PackageItemContents.NamedBag,
                                DisplayHash = bag.DestinationGlobalTagBag
                            });
                        }
                    }

                });
            }
        });

        PackageList.RefreshPackageList();
        MainWindow.Progress.CompleteStage();
    }

    private async Task LoadNamedBag(PackageItem item)
    {
        await Task.Run(() =>
        {
            Dispatcher.Invoke(HideBasicRenderer);
            MainWindow.Progress.SetProgressStages(new List<string> { $"Loading {item.Name}" });

            Entities.Clear();

            Parallel.ForEach(item.Hashes, hash =>
            {
                var tagBag = FileResourcer.Get().GetSchemaTag<S80808930>(hash);
                foreach (var val in tagBag.TagData.Entries)
                {
                    FileHash reference = val.Tag?.Hash.GetReferenceHash() ?? null;
                    if (reference is null || reference.IsInvalid())
                        continue;

                    switch (reference.Hash32)
                    {
                        case 0x808099D1 when Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999:
                        case 0x8080987E when Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402:
                            LoadBudgetSetEntities(val);
                            break;

                        case 0x80809C0F when Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999:
                        case 0x80809AD8 when Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402:
                            TryAddEntity(val.Tag.Hash, val.TagPath ?? "", tagNote: val.TagNote ?? "");
                            break;
                    }
                }
            });

            MainWindow.Progress.CompleteStage();
            Dispatcher.Invoke(ShowBasicRenderer);
        });

        RefreshEntityList();
    }

    private void LoadBudgetSetEntities(S80808933 entry)
    {
        var budgetSetHeader = FileResourcer.Get().GetSchemaTag<S8080987E>(entry.Tag.Hash);
        var budgetSet = FileResourcer.Get().GetSchemaTag<S80809EED>(budgetSetHeader.TagData.Bag.Hash);

        foreach (var val in budgetSet.TagData.Unk28)
        {
            FileHash reference = val.Tag?.Hash.GetReferenceHash() ?? null;
            if (reference is null || reference.IsInvalid())
                continue;

            bool isEntityHash =
                (reference.Hash32 == 0x80809C0F && Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999) ||
                (reference.Hash32 == 0x80809AD8 && Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402);

            if (!isEntityHash) continue;

            string parentPath = val.TagPath.Value ?? "";
            string tagPath = string.IsNullOrEmpty(parentPath)
                ? ""
                : parentPath[..(parentPath.LastIndexOf('\\') + 1)];

            string budgetSetName = string.IsNullOrEmpty(entry.TagPath)
                ? ""
                : $"[{entry.TagPath.Value.Split("\\").Last().Split(".")[0]}]";

            TryAddEntity(val.Tag.Hash, val.TagPath ?? "", tagPath, budgetSetName, entry.TagNote ?? "");
        }
    }

    private void TryAddEntity(FileHash hash,
        string fullPath,
        string tagPath = "",
        string budgetSetName = "",
        string tagNote = "")
    {
        if (hash.GetReferenceHash().IsInvalid()) return;

        string name = string.IsNullOrEmpty(fullPath)
            ? ""
            : Path.GetFileNameWithoutExtension(fullPath).Split(".")[0];

        if (string.IsNullOrEmpty(tagPath))
            tagPath = string.IsNullOrEmpty(fullPath)
                ? ""
                : fullPath[..(fullPath.LastIndexOf('\\') + 1)];

        var entity = FileResourcer.Get().GetFile<Entity>(hash);
        Entities.Add(new()
        {
            Hash = hash,
            HasGeometry = entity.HasGeometry(),
            DisplayName = name,
            ResourceCount = entity.Components.Count(),
            HasSkeleton = entity.Skeleton != null,
            TagPath = tagPath ?? "",
            BudgetSetName = budgetSetName ?? "",
            TagNote = tagNote ?? ""
        });
    }
    #endregion

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
            4 => items.OrderByDescending(x => x.Hash.Hash32).ToList(),
            3 => items.OrderBy(x => x.Hash.Hash32).ToList(),
            2 => items.OrderByDescending(x => x.DisplayName).ToList(),
            1 => items.OrderBy(x => x.DisplayName).ToList(),
            _ => items
        };

        EntityList.ItemsSource = items;
        UIHelper.ScrollToTop(EntityList);
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
        Log.Info($"Loading entity {hash}");

        ExportButton.IsEnabled = true;
        if (RendererBasic is not null)
        {
            RendererBasic.LoadEntity(hash);
            RendererBasic.ModelView.SetModelFunction(() => RendererBasic.LoadEntity(hash));
            Dispatcher.Invoke(() => RendererBasic.Visibility = Visibility.Visible);
        }
        else if (Renderer is not null)
        {
            Renderer.LoadEntity(hash);
        }

        _currentEntity = FileResourcer.Get().GetFile<Entity>(hash);
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
            HideBasicRenderer();
        });

        string pkgName = PackageResourcer.Get().GetPackage(items.First().Hash.PackageId).GetPackageMetadata().Name.Split(".")[0];
        string savePath = $"{Config.GetExportSavePath()}/{pkgName}";
        Directory.CreateDirectory(savePath);

        MainWindow.Progress.SetProgressStages(exportItems.Select((x, i) => $"Exporting {i + 1}/{exportItems.Count()}: {x.Hash}").ToList());
        await Task.Run(() =>
        {
            foreach (var item in exportItems)
            {
                List<Entity> entities = new() { _currentEntity };
                if (exportChildren)
                    entities.AddRange(_currentEntity.GetEntityChildren());

                EntityView.Export(entities, _currentEntity.Hash, savePath);
                MainWindow.Progress.CompleteStage();
            }
        });

        Dispatcher.Invoke(() =>
        {
            ShowBasicRenderer();
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
            $"Exporting Entity {_currentEntity.Hash}",
        });

        List<Entity> entities = new() { _currentEntity };
        Dispatcher.Invoke(() =>
        {
            if (ExportChildren.IsChecked.Value == true)
                entities.AddRange(_currentEntity.GetEntityChildren());

            HideBasicRenderer();
        });

        await Task.Run(() =>
        {
            EntityView.Export(entities, _currentEntity.Hash);
            MainWindow.Progress.CompleteStage();
        });

        Dispatcher.Invoke(() =>
        {
            ShowBasicRenderer();
            NotificationBanner notify = new()
            {
                Icon = "☑️",
                Title = "Export Complete",
                Description = $"Exported Entity {_currentEntity.Hash} to \"{ConfigSubsystem.Get().GetExportSavePath()}\\{_currentEntity.Hash}\\\"",
                Style = NotificationBanner.PopupStyle.Information
            };
            notify.Show();
        });
    }

    private void ShowBasicRenderer()
    {
        if (RendererBasic is not null)
        {
            RendererBasic.ModelView.Visibility = Visibility.Visible;
        }
    }

    private void HideBasicRenderer()
    {
        if (RendererBasic is not null)
        {
            RendererBasic.ModelView.Visibility = Visibility.Hidden;
        }
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
        Dispatcher.Invoke(() =>
        {
            PackageList.PackageListView.ItemsSource = items;
        });
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

        public bool HasGeometry { get; set; } = true;

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

        public string BudgetSetName { get; set; } = "";
        public string TagPath { get; set; } = "";
        public string TagNote { get; set; } = "";
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
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

            ConcurrentDictionary<StringHash, string> entityNameCache = new();

            Stopwatch stopwatch = Stopwatch.StartNew();

            Ents.EntityNames[Strategy.CurrentStrategy] = new();
            if (Strategy.IsD1())
            {
                // Name and entity is in a map data table
                ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<S808012D9>();
                Log.Debug($"{vals.Count} S808012D9 Tags");

                await Task.Run(() =>
                {
                    Parallel.ForEach(vals, parallelOptions, val =>
                    {
                        Tag<S808012D9> entry = FileResourcer.Get().GetSchemaTag<S808012D9>(val);
                        foreach (S808014D6 a in entry.TagData.Unk20)
                        {
                            foreach (S80801348 b in a.Unk08)
                            {
                                if (b.Pointer.GetValue(entry.GetReader()) is SMapDataEntry datatable)
                                {
                                    if (datatable.DataResource.GetValue(entry.GetReader()) is S80801333 name)
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
                });

                stopwatch.Stop();
                Log.Debug($"Stage 1: S808012D9 Entity Names took {stopwatch.Elapsed.TotalSeconds} seconds to process.");
                stopwatch = Stopwatch.StartNew();

                // Name is in an EntityComponent, with the entity in a map data table in that EntityComponent
                ConcurrentHashSet<FileHash> vals2 = await PackageResourcer.Get().GetAllHashesAsync<S808003F6>();
                Log.Debug($"{vals2.Count} S808003F6 Tags");

                await Task.Run(() =>
                {
                    Parallel.ForEach(vals2, parallelOptions, val =>
                    {
                        Tag<S808003F6> entry = FileResourcer.Get().GetSchemaTag<S808003F6>(val);
                        if (entry.TagData.EntityComponent is not null)
                        {
                            if (entry.TagData.EntityComponent.TagData.Unk10.GetValue(entry.TagData.EntityComponent.GetReader()) is S8080092E)
                            {
                                var resource = (S808007DD)entry.TagData.EntityComponent.TagData.Unk18.GetValue(entry.TagData.EntityComponent.GetReader());
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
                });

                stopwatch.Stop();
                Log.Debug($"Stage 2: S808003F6 Entity Names took {stopwatch.Elapsed.TotalSeconds} seconds to process.");
            }
            else if (Strategy.IsPreBL()) // SK
            {
                ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<S80809B14>();
                Log.Debug($"{vals.Count} S80809B14 Tags");

                await Task.Run(() =>
                {
                    Parallel.ForEach(vals, parallelOptions, val =>
                    {
                        //Console.WriteLine($"Resource {val}");
                        Tag<S80809B14> entry = FileResourcer.Get().GetSchemaTag<S80809B14>(val);
                        if (entry.TagData.EntityComponent is not null)
                        {
                            EntityComponent resource = entry.TagData.EntityComponent;
                            if (resource.TagData.Unk10.GetValue(resource.GetReader()) is S80809A3B)
                            {
                                var D2Class8F948080 = (S8080948F)resource.TagData.Unk18.GetValue(resource.GetReader());
                                foreach (S80808356 entry2 in D2Class8F948080.UnkA8)
                                {
                                    List<DynamicArray<S80808358>> tables = new() { entry2.Table1, entry2.Table2, entry2.Table3, entry2.Table4, entry2.Table5, entry2.Table6 };

                                    foreach (DynamicArray<S80808358> datatable in tables)
                                    {
                                        foreach (S80808358 dataEntry in datatable)
                                        {
                                            SMapDataEntry? value = dataEntry.Datatable.Value;
                                            if (value is null)
                                                continue;

                                            if (value.Value.DataResource.GetValue(resource.GetReader()) is S80807EB6 name)
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
                });
                stopwatch.Stop();
                Log.Debug($"Stage 1: S80809B14 Entity Names took {stopwatch.Elapsed.TotalSeconds} seconds to process.");
            }
            else if (Strategy.IsBL() || Strategy.IsPostBL()) // BL+
            {
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
                            if (dataEntry.DataResource.GetValue(entry.GetReader()) is S80808019 name && name.EntityName.IsValid())
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

                // Name is in an EntityComponent, with the entity in a map data table in that EntityComponent
                ConcurrentHashSet<FileHash> resources = await PackageResourcer.Get().GetAllHashesAsync<EntityComponent>();
                Log.Debug($"{resources.Count} Entity Resources");

                await Task.Run(() =>
                {
                    Parallel.ForEach(resources, parallelOptions, val =>
                    {
                        if (!val.ContainsHash(0x8080470E))
                            return;

                        var resource = FileResourcer.Get().GetFile<EntityComponent>(val, shouldCache: false);
                        var sb546 = (S808046B5)resource.TagData.Unk18.GetValue(resource.GetReader());
                        foreach (S80804696 entry in sb546.Unk80)
                        {
                            if (entry.DataTable is null)
                                continue;

                            foreach (SMapDataEntry dataEntry in entry.DataTable.TagData.DataEntries)
                            {
                                if (dataEntry.DataResource.GetValue(entry.DataTable.GetReader()) is S80808019 name)
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
                TigerHash tagClass = new(Helpers.GetClassHashForStrategy(typeof(S80806D8A), Strategy.CurrentStrategy));
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

    public void Dispose()
    {
        if (Renderer is not null)
        {
            IRenderer.UnregisterRenderer(Renderer);
        }

        if (RendererBasic is null)
            return;

        HelixModelView HelixMV = (HelixModelView)RendererBasic.ModelView.UCModelView.Resources["HelixMV"];
        HelixMV.Dispose();
    }

    public enum EntityListViewType
    {
        Entities,
        NamedBags
    }
}
