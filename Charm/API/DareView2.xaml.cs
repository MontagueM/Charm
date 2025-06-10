using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tiger;
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

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    private DestinyTraitID? TypeFilter = null;

    public DareView2()
    {
#if DEBUG
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif
        InitializeComponent();
        Categories.CustomNextButton = NextPage;
        Categories.CustomPrevButton = PreviousPage;
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Focusable = true;
        Focus();
    }

    public async void LoadContent()
    {
        await LoadApiList();
        CreateFilterOptions();
    }

    private void CreateFilterOptions()
    {
        List<ComboBoxItem> types = new();
        ComboBoxControl presets = new();
        presets.Text = "Presets";
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
    }

    private async Task LoadApiList()
    {
        ItemCategories.Clear();
        IEnumerable<InventoryItem> inventoryItems = await Investment.Get().GetInventoryItems();
        List<string> mapStages = inventoryItems.Select((_, i) => $"Loading {i + 1}/{inventoryItems.Count()}").ToList();
        MainWindow.Progress.SetProgressStages(mapStages, false, true);

        await Parallel.ForEachAsync(inventoryItems, async (item, ct) =>
        {
            string name = item.GetItemName();
            string? type_string = item.GetItemType();
            type_string ??= "";

            if (ShouldAddToList(item, type_string) && item.Name != string.Empty)
            {
                if (!item.GetItemTraits().Any())
                {
                    if (!SortedItems.ContainsKey(DestinyTraitID.other))
                        SortedItems[DestinyTraitID.other] = new List<InventoryItem>();

                    SortedItems[DestinyTraitID.other].Add(item);
                }

                foreach (var trait in item.GetItemTraits().Where(x => x.ToString().StartsWith("item_")))
                {
                    var _trait = trait;
                    if (item.GetItemType() == "Trace Rifle" && _trait == DestinyTraitID.item_weapon_auto_rifle) // bungo pls fix
                        _trait = DestinyTraitID.item_weapon_trace_rifle;

                    if (!SortedItems.ContainsKey(_trait))
                        SortedItems[_trait] = new List<InventoryItem>();

                    SortedItems[_trait].Add(item);
                }
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
                            || $"{x.Hash}" == searchStr));
            }
            else
                newItem.Items = item.Items;

            if (newItem.Items.Count != 0)
                itemCategories.Add(newItem);
        }
        Categories.Items = itemCategories;
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

        Tag<S9F548080>? a = item.GetItemStrings();
        string? b = a.TagData.ItemType.Value.ToString();
        return ((Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON
            && (b == "Artifact" || b == "Seasonal Artifact")
            && item.TagData.Unk28.GetValue(a.GetReader()) is SC5738080)
            || item.GetArtArrangementIndex() != -1
            ||
            // Whitelist
            whitelist.Any(x => type.ToLower().Contains(x.ToLower()))) &&
            // Blacklist
            !blacklist.Any(x => type.ToLower().Contains(x.ToLower()));
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
