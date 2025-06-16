using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Investment;

namespace Charm;

/// <summary>
/// Interaction logic for DareView2.xaml
/// </summary>
public partial class DareView2 : UserControl, INotifyPropertyChanged
{
    public ConcurrentDictionary<DestinyTraitID, List<InventoryItem>> SortedItems { get; set; } = new();

    private ObservableCollection<Dare_ItemCategory> _itemCategories = new();
    public ObservableCollection<Dare_ItemCategory> ItemCategories
    {
        get => _itemCategories;
        set
        {
            if (_itemCategories != value)
            {
                _itemCategories = value;
                OnPropertyChanged(nameof(ItemCategories));
            }
        }
    }

    private ObservableCollection<APIPlugItem> _selectedItems = new();
    public ObservableCollection<APIPlugItem> SelectedItems
    {
        get => _selectedItems;
        set
        {
            if (_selectedItems != value)
            {
                _selectedItems = value;
                OnPropertyChanged(nameof(SelectedItems));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    private DestinyTraitID? TypeFilter = null;
    private DestinyTierType? RarityFilter = null;

    public DareView2()
    {
#if DEBUG
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif
        InitializeComponent();
        MouseMove += DareView2_MouseMove;
        Categories.CustomNextButton = NextPage;
        Categories.CustomPrevButton = PreviousPage;
        SelectedItemsList.Items = SelectedItems;

        // By default, DisplayItems only gets called if the whole collection is reassigned
        // These trigger if something in the collection changes (add/remove), which will call DisplayItems.
        SelectedItems.CollectionChanged += (s, e) => SelectedItemsList.DisplayItems();
        ItemCategories.CollectionChanged += (s, e) => Categories.DisplayItems();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        this.DataContext = this;
        Focusable = true;
        Focus();
    }

    public async void LoadContent()
    {
        List<string> loading = new() { "Loading API Items" };
        MainWindow.Progress.SetProgressStages(loading, false, true);

        await LoadApiList();
        CreateFilterOptions();
    }

    private void CreateFilterOptions()
    {
        List<ComboBoxItem> types = new();
        ComboBoxControl presets = new();
        presets.Text = "Filter By Type";
        presets.FontSize = 14;
        foreach (var type in SortedItems.Keys)
        {
            types.Add(new()
            {
                Content = type.GetEnumDescription(),
                Tag = type,
                FontSize = 10
            });
        }

        types = types.OrderBy(x => x.Content as string).ToList();
        types.Insert(0, new() { Content = "All", FontSize = 10 });
        presets.Combobox.ItemsSource = types;

        if (presets.Combobox.SelectedIndex == -1)
        {
            presets.Combobox.SelectedIndex = 0;
        }
        presets.Combobox.MinWidth = 200;
        presets.Combobox.SelectionChanged += Filters_OnSelectionChanged;
        FilterOptions.Children.Add(presets);

        List<ComboBoxItem> rarities = new();
        ComboBoxControl rarity_presets = new();
        rarity_presets.Text = "Filter By Rarity";
        rarity_presets.FontSize = 14;

        var values = Enum.GetValues(typeof(DestinyTierType)).Cast<DestinyTierType>().ToList();
        foreach (var rarity in values.Where(x => x != DestinyTierType.Unknown))
        {
            rarities.Add(new()
            {
                Content = rarity.GetEnumDescription(),
                Tag = rarity,
                FontSize = 10
            });
        }

        rarities.Insert(0, new() { Content = "All", FontSize = 10 });
        rarity_presets.Combobox.ItemsSource = rarities;

        if (rarity_presets.Combobox.SelectedIndex == -1)
        {
            rarity_presets.Combobox.SelectedIndex = 0;
        }
        rarity_presets.Combobox.MinWidth = 200;
        rarity_presets.Combobox.SelectionChanged += RarityFilters_OnSelectionChanged;
        FilterOptions.Children.Add(rarity_presets);
    }

    private async Task LoadApiList()
    {
        ItemCategories.Clear();
        IEnumerable<InventoryItem> inventoryItems = await Investment.Get().GetInventoryItems();
        MainWindow.Progress.CompleteStage();

        List<string> mapStages = inventoryItems.Select((_, i) => $"Loading {i + 1}/{inventoryItems.Count()}").ToList();
        MainWindow.Progress.SetProgressStages(mapStages, false, true);

        await Parallel.ForEachAsync(inventoryItems, async (item, ct) =>
        {
            string name = item.GetItemName();
            string? type_string = item.GetItemType();
            type_string ??= "";

            if (ShouldAddToList(item, type_string) && item.Name != string.Empty)
            {
                if (!item.GetItemTraits().Any() || item.GetItemTraits().Contains(DestinyTraitID.other))
                {
                    if (!SortedItems.ContainsKey(DestinyTraitID.other))
                        SortedItems[DestinyTraitID.other] = new List<InventoryItem>();

                    SortedItems[DestinyTraitID.other].Add(item);
                }

                foreach (var trait in item.GetItemTraits().Where(x => x.ToString().StartsWith("item_")))
                {
                    if (trait is DestinyTraitID.item_engram)
                        continue;

                    var _trait = trait;
                    if (item.GetItemType() == "Trace Rifle" && _trait == DestinyTraitID.item_weapon_auto_rifle) // bungo pls fix
                        _trait = DestinyTraitID.item_weapon_trace_rifle;

                    if (!SortedItems.ContainsKey(_trait))
                        SortedItems[_trait] = new List<InventoryItem>();

                    SortedItems[_trait].Add(item);
                }

                // this is needed to make sure its ornaments are loaded (if it has any)
                // which in turn will set the ornaments parent item
                _ = item.Ornaments;
            }
            MainWindow.Progress.CompleteStage();
        });

        foreach ((var trait, var items) in SortedItems.OrderBy(x => x.Key.GetEnumDescription()).Where(x => x.Value.Count != 0))
        {
            ItemCategories.Add(new Dare_ItemCategory
            {
                CategoryName = trait.GetEnumDescription(),
                CategoryType = trait,
                ItemsPerPage = 24,
                Items = new ObservableCollection<APIPlugItem>(
                    items.DistinctBy(x => x.ApiHash)
                    .Select(x => new APIPlugItem(x))
                    .OrderByDescending(x => x.Item.GetItemIndex())
                    .OrderByDescending(x => x.Item.GetItemRarity()))

            });
        }
        Categories.Items = ItemCategories;
    }

    private void RefreshItemList()
    {
        if (ItemCategories is null || ItemCategories.Count == 0)
            return;

        List<Dare_ItemCategory> curItems = new(ItemCategories.ToList());
        List<Dare_ItemCategory> itemCategories = new();
        string searchStr = SearchBox.Text;

        foreach (var item in curItems)
        {
            if (TypeFilter is not null && item.CategoryType != TypeFilter)
                continue;

            Dare_ItemCategory newItem = new()
            {
                CategoryName = item.CategoryName,
                CategoryType = item.CategoryType
            };

            if (TypeFilter is not null) // if we're filtering by type, display more items since there will be only one category
                newItem.ItemsPerPage = 72;

            if (searchStr is not null && searchStr != string.Empty)
            {
                newItem.Items = new ObservableCollection<APIPlugItem>(item.Items
                .Where(x => x.Item.GetItemName().ToLower().Contains(searchStr.ToLower())
                            || x.Item.GetItemType().ToLower().Contains(searchStr.ToLower())
                            || x.Item.Parent?.GetItemName().ToLower().Contains(searchStr.ToLower()) == true
                            || $"{x.Hash}" == searchStr));
            }
            else
                newItem.Items = item.Items;

            if (RarityFilter is not null)
                newItem.Items = new ObservableCollection<APIPlugItem>(newItem.Items.Where(x => x.Item.GetItemRarity() == RarityFilter));

            if (newItem.Items.Count != 0)
                itemCategories.Add(newItem);
        }
        if (itemCategories.Count == 1)
            itemCategories.First().ItemsPerPage = 72;

        Categories.Items = itemCategories;
    }

    private void SelectedDareEntry_Click(object sender, RoutedEventArgs e)
    {
        var element = (sender as FrameworkElement);
        APIPlugItem apiItem = element.DataContext as APIPlugItem;
        if (SelectedItems.Contains(apiItem) && !apiItem.IsNewlyAdded) // Using IsNewlyAdded just to stop multi-clicking 
        {
            apiItem.IsNewlyAdded = true;

            UIHelper.AnimateSlide((UIElement)UIHelper.GetParentAtDepth(element, 2),
                0.1f, new(25, 0), new(0, 0), easing: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            UIHelper.AnimateFade((UIElement)UIHelper.GetParentAtDepth(element, 2), 0.1f, 0f, 1f, completed: async (s, e) =>
            {
                await Task.Delay(100);
                SelectedItems.Remove(apiItem);
            });
        }
    }

    private void DareEntry_Click(object sender, RoutedEventArgs e)
    {
        APIPlugItem apiItem = (sender as FrameworkElement).DataContext as APIPlugItem;
        if (!SelectedItems.Contains(apiItem))
        {
            apiItem.IsNewlyAdded = true;
            SelectedItems.Add(apiItem);
        }
    }

    private void DareSelectedEntry_Loaded(object sender, RoutedEventArgs e)
    {
        var element = (sender as FrameworkElement);
        APIPlugItem apiItem = element.DataContext as APIPlugItem;
        if (apiItem is null)
            return;

        if (apiItem.IsNewlyAdded)
        {
            UIHelper.AnimateSlide(element, 0.15f, new(0, 0), new(-15, 0));
            UIHelper.AnimateFade(element, 0.15f, completed: (s, e) =>
            {
                apiItem.IsNewlyAdded = false; // reset newly added state when the item is loaded
            });
        }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedItems.Count == 0)
        {
            NotificationBanner notify = new()
            {
                Icon = "⚠",
                Title = "NOTHING TO EXPORT!",
                Description = $"Select some items to export, you silly goose!",
                Style = NotificationBanner.PopupStyle.Warning
            };
            notify.Show();
            return;
        }

        List<string> apiStages = SelectedItems.Select((_, i) => $"Exporting {SelectedItems[i].Item.Name} ({i + 1}/{SelectedItems.Count})").ToList();
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        string savePath = config.GetExportSavePath();
        bool aggregateOutput = (bool)AggregateOutputButton.IsChecked;

        if (aggregateOutput && SelectedItems.Any(x => !x.Item.IsShader))
            savePath = CreateNextOutputFolder(config.GetExportSavePath());

        MainWindow.Progress.SetProgressStages(apiStages);
        Task.Run(() =>
        {
            SelectedItems.ToList().ForEach(item =>
            {
                var curItem = item.Item;
                if ((curItem.Type is "Artifact" or "Seasonal Artifact") && curItem.TagData.Unk28.GetValue(curItem.GetReader()) is SC5738080 gearSet)
                {
                    if (gearSet.ItemList.Count != 0)
                    {
                        curItem = Investment.Get().GetInventoryItem(gearSet.ItemList.First().ItemIndex);
                        curItem.Name = item.Item.Name;
                    }
                }

                if (curItem.GetArtArrangementIndex() != -1)
                {
                    // if has a model
                    EntityView.ExportInventoryItem(curItem, savePath, aggregateOutput);
                }
                else
                {
                    // shader
                    string itemName = Helpers.SanitizeString(curItem.Name);
                    string savePath = config.GetExportSavePath(); // need to set again here
                    savePath += $"/{itemName}";
                    Directory.CreateDirectory(savePath);
                    Directory.CreateDirectory(savePath + "/Textures");
                    Investment.Get().ExportShader(curItem, savePath, itemName, config.GetOutputTextureFormat());
                }
                MainWindow.Progress.CompleteStage();
            });

            Dispatcher.Invoke(() =>
            {
                NotificationBanner notify = new()
                {
                    Icon = "☑️",
                    Title = "EXPORT COMPLETE",
                    Description = $"Exported " +
                    $"{(SelectedItems.Count == 1 ? $"{SelectedItems.First().Item.Name}" : $"{SelectedItems.Count} items")}" +
                    $" to \"{config.GetExportSavePath()}\"",
                    Style = NotificationBanner.PopupStyle.Information
                };
                notify.Show();
            });
        });
    }

    private void OpenOutputFolder_Click(object sender, RoutedEventArgs e)
    {
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        Process.Start("explorer.exe", config.GetExportSavePath());
    }


    private bool _isClearing = false;
    private async void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isClearing) return;
        _isClearing = true;

        var items = SelectedItemsList.CurrentPageItems.Where(x => !x.IsPlaceholder).ToList();
        foreach (var item in items)
        {
            var element = SelectedItemsList.ItemList.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
            if (!item.IsNewlyAdded) // Using IsNewlyAdded just to stop multi-clicking 
            {
                item.IsNewlyAdded = false;

                UIHelper.AnimateSlide(element, 0.1f, new(25, 0), new(0, 0), easing: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

                UIHelper.AnimateFade(element, 0.1f, 0f, 1f);
            }
            await Task.Delay(50);
        }
        SelectedItems.Clear();
        _isClearing = false;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshItemList();
    }

    private void Filters_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (((sender as ComboBox).SelectedItem as ComboBoxItem).Tag is not null)
            TypeFilter = (DestinyTraitID)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        else
            TypeFilter = null;

        RefreshItemList();
    }

    private void RarityFilters_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (((sender as ComboBox).SelectedItem as ComboBoxItem).Tag is not null)
            RarityFilter = (DestinyTierType)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        else
            RarityFilter = null;

        RefreshItemList();
    }

    private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            Categories.SelectNextPage();
        }

        if (e.Key == Key.Up)
        {
            Categories.SelectPreviousPage();
        }
    }

    private void APIItem_View(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (Strategy.IsD1())
            return;

        e.Handled = true;
        APIPlugItem apiItem = (sender as FrameworkElement).DataContext as APIPlugItem;

        ItemView apiItemView = new(apiItem.Item);
        MainWindow.Current.MakeNewTab(apiItem.Item.Name, apiItemView);
        MainWindow.Current.SetNewestTabSelected();
    }

    public static bool ShouldAddToList(InventoryItem item, string type)
    {
        if (type is null)
            return false;

        string[] blacklist = new[]
        {
            "Ghost Projection",
            "Emote",
            "Finisher",
            "Ship Schematics"
        };

        string[] whitelist = new[]
        {
            // TODO: Add emotes and ghost projections for fx mesh exporting
            "Shader",
        };

        return ((!Strategy.IsD1() && (type is "Artifact" or "Seasonal Artifact") && item.TagData.Unk28.GetValue(item.GetReader()) is SC5738080)
            || item.GetArtArrangementIndex() != -1
            || (whitelist.Any(x => type.ToLower().Contains(x.ToLower())))  // Whitelist
            && !blacklist.Any(x => type.ToLower().Contains(x.ToLower()))); // Blacklist
    }

    // For aggregated outputs
    public static string CreateNextOutputFolder(string baseDirectory)
    {
        // Get all subdirectories that match the "Output#" pattern
        string[] existingFolders = Directory.GetDirectories(baseDirectory, "ApiOutput*");
        int maxNumber = 0;

        // Regex to capture the numeric part of "Output#"
        Regex regex = new(@"ApiOutput(\d+)$");

        foreach (string folder in existingFolders)
        {
            Match match = regex.Match(Path.GetFileName(folder));
            if (match.Success)
            {
                // Parse the number from the folder name
                int folderNumber = int.Parse(match.Groups[1].Value);
                if (folderNumber > maxNumber)
                {
                    maxNumber = folderNumber;
                }
            }
        }

        // Increment the max number to get the next available folder
        int nextNumber = maxNumber + 1;
        string newFolderName = $"ApiOutput{nextNumber}";
        string newFolderPath = Path.Combine(baseDirectory, newFolderName);

        // Create the new directory
        Directory.CreateDirectory(newFolderPath);

        return newFolderPath;
    }

    private void DareView2_MouseMove(object sender, MouseEventArgs e)
    {
        var group = UIHelper.EnsureTransformGroup(MainGrid);
        var translate = UIHelper.GetOrAddTransform<TranslateTransform>(group);

        float x = -7f / (float)MainWindow.Current.ActualWidth;
        float y = -7f / (float)MainWindow.Current.ActualHeight;
        Point position = Mouse.GetPosition(this);

        translate.X = (int)Math.Round(position.X * x);
        translate.Y = (int)Math.Round(position.Y * y);
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        PopupBanner about = new()
        {
            DarkenBackground = true,
            //Icon = "❓",
            IconImage = ApiImageUtils.MakeBitmapImage(Texture.GetTextureFromHash(new(0x80E65764)), 120, 120),
            Title = $"WELCOME TO DARE",
            Subtitle = "The Destiny API Ripping Extension",
            Description = "You may already be familar with the old DARE, but if you're not, DARE used to be a program used to rip gear models/shaders from the Bungie API." +
            "\n\nThis is it's spiritual successor. Charm rips player gear directly from the game files, which means you can rip even if the API is down or you are offline." +
            "\n\n• Use the search bar to look for specific items and/or the drop downs to filter them." +
            "\n• Clicking an items icon will add it to the export list on the right side." +
            "\n• You can Shift+Click to skip to the start/end of a category, or Ctrl+Click to skip 1/4.",
            Style = PopupBanner.PopupStyle.Information
        };
        about.Show();
    }

    public class Dare_ItemCategory : CharmUIElement
    {
        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                if (_categoryName != value)
                {
                    _categoryName = value;
                    OnPropertyChanged(nameof(CategoryName));
                }
            }
        }

        private DestinyTraitID _categoryType;
        public DestinyTraitID CategoryType
        {
            get => _categoryType;
            set
            {
                if (_categoryType != value)
                {
                    _categoryType = value;
                    OnPropertyChanged(nameof(CategoryType));
                }
            }
        }

        private ObservableCollection<APIPlugItem> _items = new();
        public ObservableCollection<APIPlugItem> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged(nameof(Items));
                }
            }
        }

        private int _itemsPerPage = 24;
        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set
            {
                if (_itemsPerPage != value)
                {
                    _itemsPerPage = value;
                    OnPropertyChanged(nameof(ItemsPerPage));
                }
            }
        }
    }
}
