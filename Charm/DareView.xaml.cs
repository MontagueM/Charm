using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tiger;
using Tiger.Schema.Investment;
using static Charm.APIItemView;

namespace Charm;

public partial class DareView : UserControl
{
    private ConcurrentDictionary<uint, ApiItem> _allItems;
    private ObservableCollection<ApiItem> _selectedItems;

    public DareView()
    {
        InitializeComponent();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshItemList();
    }
    private void AmountBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_allItems != null) //Gotta do this for some reason
            RefreshItemList();
    }

    private void RefreshItemList()
    {
        string searchTerm = SearchBox.Text.ToLower();
        int numToDisplay = AmountBox.Text.Length > 0 ? int.Parse(AmountBox.Text) : 0;
        ConcurrentBag<ApiItem> items = new();

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
        Parallel.ForEach(_allItems.Values, parallelOptions, item =>
        {
            if (items.Count <= numToDisplay && (item.ItemName.ToLower().Contains(searchTerm)
            || item.ItemHash.Contains(searchTerm)
            || item.ItemType.ToLower().Contains(searchTerm)
            || item.ItemRarity.ToString().ToLower().Contains(searchTerm))
            || (searchTerm != "" && item.Parent != null && item.Parent.GetItemName().ToLower().Contains(searchTerm)))
            {
                items.Add(item);
            }
        });
        var sortedItems = new List<ApiItem>(items);
        sortedItems.Sort((a, b) => b.ItemRarity.CompareTo(a.ItemRarity));
        DareListView.ItemsSource = sortedItems;
    }

    public async void LoadContent()
    {
        _allItems = new ConcurrentDictionary<uint, ApiItem>();
        _selectedItems = new ObservableCollection<ApiItem>();
        await LoadApiList();
        RefreshItemList();
    }

    private async Task LoadApiList()
    {
        IEnumerable<InventoryItem> inventoryItems = await Investment.Get().GetInventoryItems();
        List<string> mapStages = inventoryItems.Select((_, i) => $"Loading {i + 1}/{inventoryItems.Count()}").ToList();
        MainWindow.Progress.SetProgressStages(mapStages, false, true);
        await Parallel.ForEachAsync(inventoryItems, async (item, ct) =>
        {
            //if (_allItems.Count > 1000)
            //{
            //    MainWindow.Progress.CompleteStage();
            //    return;
            //}

            string name = Investment.Get().GetItemName(item);
            string? type = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)).TagData.ItemType.Value;

            if (type == null)
                type = "";

            if (ShouldAddToList(item, type))
            {
                CreateOrnamentItems(item); // D1
                var isD1 = Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON;
                var isOrnament = type.Contains("Ornament");
                var isWeaponOrnament = type.Contains("Weapon Ornament");
                var isNameNotEmpty = name != "";

                if ((isD1 && !isOrnament && isNameNotEmpty) || (!isD1 && isNameNotEmpty && !isWeaponOrnament))
                {
                    var newItem = new ApiItem
                    {
                        ItemName = name,
                        ItemType = type,
                        ItemRarity = (DestinyTierType)item.TagData.ItemRarity,
                        ItemHash = item.TagData.InventoryItemHash.Hash32.ToString(),
                        ImageHeight = 96,
                        ImageWidth = 96,
                        Item = item,
                        IsD1 = isD1,
                    };
                    _allItems.TryAdd(item.TagData.InventoryItemHash.Hash32, newItem);
                }
            }
            MainWindow.Progress.CompleteStage();
        });
    }

    private void DareItemControl_OnClick(object sender, RoutedEventArgs e)
    {
        ApiItem apiItem = (sender as Button).DataContext as ApiItem;

        // Remove from _allItems, add to _selectedItems if not already there otherwise remove from _selectedItems and add back to _allItems
        if (_allItems.TryRemove(apiItem.Item.TagData.InventoryItemHash.Hash32, out _))
        {
            _selectedItems.Add(apiItem);

            //foreach (var a in Investment.Get().GetEntitiesFromHash(apiItem.Item.TagData.InventoryItemHash))
            //{
            //    if (a is null)
            //        continue;
            //    Console.WriteLine($"{a.Hash}");
            //}
        }
        else
        {
            _allItems.TryAdd(apiItem.Item.TagData.InventoryItemHash.Hash32, apiItem);
            _selectedItems.Remove(apiItem);
        }
        SelectedItemView.ItemsSource = _selectedItems;
        RefreshItemList();
    }

    private void ClearQueue_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var selectedItem in _selectedItems)
        {
            _allItems.TryAdd(selectedItem.Item.TagData.InventoryItemHash.Hash32, selectedItem);
        }
        _selectedItems.Clear();
        SelectedItemView.ItemsSource = _selectedItems;
        RefreshItemList();
    }

    private void ExecuteQueue_OnClick(object sender, RoutedEventArgs e)
    {
        List<string> apiStages = _selectedItems.Select((_, i) => $"exporting {i + 1}/{_selectedItems.Count}").ToList();
        MainWindow.Progress.SetProgressStages(apiStages);
        Task.Run(() =>
        {
            _selectedItems.ToList().ForEach(item =>
            // Parallel.ForEach(_selectedItems, item =>
            {
                if (item.ItemType == "Artifact" && item.Item.TagData.Unk28.GetValue(item.Item.GetReader()) is D2Class_C5738080 gearSet)
                {
                    if (gearSet.ItemList.Count != 0)
                        item.Item = Investment.Get().GetInventoryItem(gearSet.ItemList.First().ItemIndex);
                }

                if (item.Item.GetArtArrangementIndex() != -1)
                {
                    // if has a model
                    EntityView.ExportInventoryItem(item);
                }
                else
                {
                    // shader
                    ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
                    string savePath = config.GetExportSavePath();
                    string itemName = Helpers.SanitizeString(item.ItemName);
                    savePath += $"/{itemName}";
                    Directory.CreateDirectory(savePath);
                    Directory.CreateDirectory(savePath + "/Textures");
                    Investment.Get().ExportShader(item.Item, savePath, itemName, config.GetOutputTextureFormat());
                }
                MainWindow.Progress.CompleteStage();
            });
        });
    }

    private void RipAllShaders_OnClick(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show("This will take some time. Do you want to continue?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.No)
            return;

        var shaderItems = _allItems
                        .Where(item => item.Value.Item.GetArtArrangementIndex() == -1 && item.Value.ItemType.Contains("Shader"))
                        .ToList();
        int shaderItemCount = shaderItems.Count;

        List<string> apiStages = shaderItems
                                .Select((_, i) => $"Exporting {i + 1}/{shaderItemCount}")
                                .ToList();

        MainWindow.Progress.SetProgressStages(apiStages);
        Task.Run(() =>
        {
            ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
            string savePath = config.GetExportSavePath();
            savePath += $"/AllShaders";
            Directory.CreateDirectory(savePath);
            Directory.CreateDirectory(savePath + "/Textures");

            shaderItems.ToList().ForEach(item =>
            {
                string itemName = Helpers.SanitizeString(item.Value.ItemName);
                //itemName = Regex.Replace(string.Join("_", itemName.Split(Path.GetInvalidFileNameChars())), @"[^\u0000-\u007F]", "_");
                Investment.Get().ExportShader(item.Value.Item, savePath, itemName, config.GetOutputTextureFormat());

                item.Value.Item.GetIconPrimaryTexture().SavetoFile($"{savePath}/{itemName}");

                MainWindow.Progress.CompleteStage();
            });
        });
    }

    private void OpenOutputFolder_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        Process.Start("explorer.exe", config.GetExportSavePath());
    }

    //Surely this will only allow numbers and not fail in anyway...
    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }

    public void CreateOrnamentItems(InventoryItem parent)
    {
        var ornaments = parent.GetItemOrnaments();

        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            foreach (var item in ornaments)
            {
                if (item.GetArtArrangementIndex() == -1)
                    continue;
                var strings = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.TagData.InventoryItemHash));
                if (!strings.IsLoaded())
                    strings.Load();

                string? type = strings.TagData.ItemType.Value;
                if (type == null)
                    type = "";

                string name = Investment.Get().GetItemName(item);
                var newItem = new ApiItem
                {
                    ItemName = name,
                    ItemType = type,
                    ItemRarity = (DestinyTierType)parent.TagData.ItemRarity,
                    ItemHash = item.TagData.InventoryItemHash.Hash32.ToString(),
                    ImageHeight = 96,
                    ImageWidth = 96,
                    Item = item,
                    Parent = parent
                };
                _allItems.TryAdd(item.TagData.InventoryItemHash.Hash32, newItem);
            }
        }
        else // D1
        {
            for (int i = 0; i < ornaments.Count; i++)
            {
                var item = ornaments[i];
                var strings = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.TagData.InventoryItemHash));

                string? type = strings.TagData.ItemType.Value;
                if (type == null)
                    type = "";

                var name = type != "Armor Ornament" ? Investment.Get().GetItemName(item) : $"{Investment.Get().GetItemName(parent)} Ornament {i + 1}";
                var newItem = new ApiItem
                {
                    ItemName = name,
                    ItemType = type,
                    ItemRarity = (DestinyTierType)parent.TagData.ItemRarity,
                    ItemHash = item.TagData.InventoryItemHash.Hash32.ToString(),
                    ImageHeight = 96,
                    ImageWidth = 96,
                    Item = item,
                    Parent = parent,
                    IsD1 = true
                };
                _allItems.TryAdd(item.TagData.InventoryItemHash.Hash32, newItem);
            }
        }
    }

    private bool ShouldAddToList(InventoryItem item, string type)
    {
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

        var a = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.TagData.InventoryItemHash));
        return ((Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON && a.TagData.ItemType.Value.ToString() == "Artifact" && item.TagData.Unk28.GetValue(a.GetReader()) is D2Class_C5738080)
            || item.GetArtArrangementIndex() != -1 ||
            // Whitelist
            whitelist.Any(x => type.ToLower().Contains(x.ToLower()))) &&
            // Blacklist
            !blacklist.Any(x => type.ToLower().Contains(x.ToLower()));
    }
}

public class ApiItem
{
    public InventoryItem Item { get; set; }
    public InventoryItem Parent { get; set; }
    public int CollectableIndex { get; set; }

    public string ItemName { get; set; }
    public string ItemType { get; set; }
    public string ItemFlavorText { get; set; }
    public DestinyTierType ItemRarity { get; set; }
    public DestinyDamageTypeEnum ItemDamageType { get; set; }
    public string ItemHash { get; set; }

    public double ImageWidth { get; set; }
    public double ImageHeight { get; set; }
    public bool IsD1 { get; set; }
    public bool IsPlaceholder { get; set; } = false;
    public int Weight { get; set; } = -1; // For display ordering purposes

    private System.Windows.Media.ImageSource _ImageSource { get; set; }
    private System.Windows.Media.ImageBrush _GridBackground { get; set; }
    public System.Windows.Media.ImageSource ImageSource
    {
        get
        {
            if (_ImageSource != null)
                return _ImageSource;

            UnmanagedMemoryStream? bgStream = null;
            UnmanagedMemoryStream? primaryStream = null;
            UnmanagedMemoryStream? overlayStream = null;
            UnmanagedMemoryStream? bgOverlayStream = Item.GetIconBackgroundOverlayStream();
            var group = new DrawingGroup();

            if (IsD1)
            {
                bgStream = ItemType != "Armor Ornament" ? Item.GetIconBackgroundStream() : Parent.GetTextureFromHash(new FileHash("1935A680"));
                primaryStream = ItemType != "Armor Ornament" ? Item.GetIconPrimaryStream() : Parent.GetIconPrimaryStream();
                overlayStream = ItemType != "Armor Ornament" ? Item.GetIconOverlayStream() : Parent.GetIconOverlayStream(); //parent.GetTextureFromHash(new FileHash("E1DBA580"));
            }
            else
            {
                bgStream = Item.GetIconBackgroundStream();
                primaryStream = Item.GetIconPrimaryStream();
                overlayStream = Item.GetIconOverlayStream();
            }

            var primary = primaryStream != null ? ApiImageUtils.MakeBitmapImage(primaryStream, 96, 96) : null;
            var overlay = overlayStream != null ? ApiImageUtils.MakeBitmapImage(overlayStream, 96, 96) : null;
            var bg = bgStream != null ? ApiImageUtils.MakeBitmapImage(bgStream, 96, 96) : null;

            if (bgOverlayStream != null && Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
                primary = ApiImageUtils.MakeDyedIcon(Item);

            // Most if not all legendary armor will use the ornament overlay because of transmog (I assume)
            var bgOverlay = bgOverlayStream != null && ItemType.Contains("Ornament") ? ApiImageUtils.MakeBitmapImage(bgOverlayStream, 96, 96) : null;

            group.Children.Add(new ImageDrawing(bg, new Rect(0, 0, 96, 96)));
            group.Children.Add(new ImageDrawing(bgOverlay, new Rect(0, 0, 96, 96)));
            group.Children.Add(new ImageDrawing(primary, new Rect(0, 0, 96, 96)));
            group.Children.Add(new ImageDrawing(overlay, new Rect(0, 0, 96, 96)));
            var dw = new DrawingImage(group);
            dw.Freeze();
            _ImageSource = dw;
            return dw;
        }
    }

    public System.Windows.Media.ImageBrush GridBackground
    {
        get
        {
            if (_GridBackground != null)
                return _GridBackground;
            BitmapImage bg = null;
            UnmanagedMemoryStream? bgStream = Item.GetIconBackgroundStream();
            if (IsD1)
            {
                bgStream = ItemType != "Armor Ornament" ? bgStream : Parent.GetTextureFromHash(new FileHash("1935A680"));
                bg = bgStream != null ? ItemType != "Armor Ornament" ?
                    ApiImageUtils.MakeBitmapImage(bgStream, 96, 96) :
                    ApiImageUtils.MakeBitmapImage(bgStream, 256, 128) : null;
            }
            else
            {
                bg = bgStream != null ? ApiImageUtils.MakeBitmapImage(bgStream, 96, 96) : null;
            }

            System.Windows.Media.ImageBrush brush = new System.Windows.Media.ImageBrush(bg);
            brush.Freeze();
            _GridBackground = brush;

            return brush;
        }
    }

    public PlugItem PlugItem { get; set; }
}
