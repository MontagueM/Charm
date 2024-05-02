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

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 16 };
        Parallel.ForEach(_allItems.Values, parallelOptions, item =>
        {
            if (items.Count <= numToDisplay && (item.ItemName.ToLower().Contains(searchTerm)
            || item.ItemHash.Contains(searchTerm)
            || item.ItemType.ToLower().Contains(searchTerm)
            || item.ItemRarity.ToString().ToLower().Contains(searchTerm)))
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
        List<string> mapStages = inventoryItems.Select((_, i) => $"loading {i + 1}/{inventoryItems.Count()}").ToList();
        MainWindow.Progress.SetProgressStages(mapStages, false, true);
        await Parallel.ForEachAsync(inventoryItems, async (item, ct) =>
        {
            string name = Investment.Get().GetItemName(item);
            string? type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value;

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
                    var image = MakeIcon(item);
                    var newItem = new ApiItem
                    {
                        ItemName = name,
                        ItemType = type,
                        ItemRarity = (DestinyTierType)item.TagData.ItemRarity,
                        ItemHash = item.TagData.InventoryItemHash.Hash32.ToString(),
                        ImageSource = image.Keys.First(),
                        ImageHeight = 96,
                        ImageWidth = 96,
                        GridBackground = image.Values.First(),
                        Item = item
                    };
                    _allItems.TryAdd(item.TagData.InventoryItemHash.Hash32, newItem);
                }
            }
            MainWindow.Progress.CompleteStage();
        });
    }

    private BitmapImage MakeBitmapImage(UnmanagedMemoryStream ms, int width, int height)
    {
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = ms;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.DecodePixelWidth = width;
        bitmapImage.DecodePixelHeight = height;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return bitmapImage;
    }

    private void DareItemControl_OnClick(object sender, RoutedEventArgs e)
    {
        ApiItem apiItem = (sender as Button).DataContext as ApiItem;

        // Remove from _allItems, add to _selectedItems if not already there otherwise remove from _selectedItems and add back to _allItems
        if (_allItems.TryRemove(apiItem.Item.TagData.InventoryItemHash.Hash32, out _))
        {
            _selectedItems.Add(apiItem);

            //if (Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
            //{
            //    System.Console.WriteLine($"---{apiItem.Item.Hash}---");
            //    if (apiItem.Item.TagData.Unk70.GetValue(apiItem.Item.GetReader()) is D2Class_C0778080 sockets)
            //    {
            //        foreach (var socket in sockets.SocketEntries)
            //        {
            //            System.Console.WriteLine($"SocketTypeIndex: {socket.SocketTypeIndex} " + (socket.SocketTypeIndex != -1 ? Investment.Get().SocketCategoryStringThings[Investment.Get().GetSocketCategoryIndex(socket.SocketTypeIndex)].SocketName.Value : ""));
            //            System.Console.WriteLine($"SingleInitialItemIndex: {socket.SingleInitialItemIndex} " + (socket.SingleInitialItemIndex != -1 ? Investment.Get().GetItemName(Investment.Get().GetInventoryItem(socket.SingleInitialItemIndex)) : ""));
            //            System.Console.WriteLine($"ReusablePlugSetIndex1: {socket.ReusablePlugSetIndex1}");
            //            if (socket.ReusablePlugSetIndex1 != -1)
            //            {
            //                foreach (var randomPlugs in Investment.Get().GetRandomizedPlugSet(socket.ReusablePlugSetIndex1))
            //                {
            //                    if (randomPlugs.PlugInventoryItemIndex == -1)
            //                        continue;

            //                    System.Console.WriteLine($"-Reusable PlugInventoryItemIndex: {Investment.Get().GetItemName(Investment.Get().GetInventoryItem(randomPlugs.PlugInventoryItemIndex))}");
            //                }
            //            }

            //            System.Console.WriteLine($"ReusablePlugSetIndex2: {socket.ReusablePlugSetIndex2}");
            //            if (socket.ReusablePlugSetIndex2 != -1)
            //            {
            //                foreach (var randomPlugs in Investment.Get().GetRandomizedPlugSet(socket.ReusablePlugSetIndex2))
            //                {
            //                    if (randomPlugs.PlugInventoryItemIndex == -1)
            //                        continue;

            //                    System.Console.WriteLine($"-Reusable PlugInventoryItemIndex: {Investment.Get().GetItemName(Investment.Get().GetInventoryItem(randomPlugs.PlugInventoryItemIndex))}");
            //                }
            //            }

            //            foreach (var plug in socket.PlugItems)
            //            {
            //                System.Console.WriteLine($"-PlugInventoryItemIndex: {plug.PlugInventoryItemIndex} " + (plug.PlugInventoryItemIndex != -1 ? Investment.Get().GetItemName(Investment.Get().GetInventoryItem(plug.PlugInventoryItemIndex)) : ""));
            //            }
            //            System.Console.WriteLine($"-------------------------");
            //        }
            //    }
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
                    string meshName = item.ItemName;
                    savePath += $"/{meshName}";
                    Directory.CreateDirectory(savePath);
                    Directory.CreateDirectory(savePath + "/Textures");
                    Investment.Get().ExportShader(item.Item, savePath, meshName, config.GetOutputTextureFormat());
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
                string itemName = item.Value.ItemName;
                itemName = Regex.Replace(string.Join("_", itemName.Split(Path.GetInvalidFileNameChars())), @"[^\u0000-\u007F]", "_");
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

    // TODO: D2 maybe?
    public void CreateOrnamentItems(InventoryItem parent)
    {
        var ornaments = parent.GetItemOrnaments();

        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            foreach (var item in ornaments)
            {
                if (item.GetArtArrangementIndex() == -1)
                    continue;

                string? type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value;
                if (type == null)
                    type = "";

                string name = Investment.Get().GetItemName(item);
                var icon = MakeIcon(item);
                var newItem = new ApiItem
                {
                    ItemName = name,
                    ItemType = type,
                    ItemRarity = (DestinyTierType)parent.TagData.ItemRarity,
                    ItemHash = item.TagData.InventoryItemHash.Hash32.ToString(),
                    ImageSource = icon.Keys.First(),
                    ImageHeight = 96,
                    ImageWidth = 96,
                    GridBackground = icon.Values.First(),
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
                string? type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value;
                if (type == null)
                    type = "";

                var bgStream = type != "Armor Ornament" ? item.GetIconBackgroundStream() : parent.GetTextureFromHash(new FileHash("1935A680"));
                var primaryStream = type != "Armor Ornament" ? item.GetIconPrimaryStream() : parent.GetIconPrimaryStream();
                var overlayStream = type != "Armor Ornament" ? item.GetIconOverlayStream() : parent.GetIconOverlayStream(); //parent.GetTextureFromHash(new FileHash("E1DBA580"));

                var primary = primaryStream != null ? MakeBitmapImage(primaryStream, 96, 96) : null;
                var overlay = overlayStream != null ? MakeBitmapImage(overlayStream, 96, 96) : null;

                var group = new DrawingGroup();
                group.Children.Add(new ImageDrawing(primary, new Rect(0, 0, 96, 96)));
                group.Children.Add(new ImageDrawing(overlay, new Rect(0, 0, 96, 96)));

                var dw = new DrawingImage(group);
                dw.Freeze();

                var background = bgStream != null ? MakeBitmapImage(bgStream, 512, 256) : null;
                ImageBrush brush = new ImageBrush(background);
                brush.Freeze();

                var name = type != "Armor Ornament" ? Investment.Get().GetItemName(item) : $"{Investment.Get().GetItemName(parent)} Ornament {i + 1}";
                var newItem = new ApiItem
                {
                    ItemName = name,
                    ItemType = type,
                    ItemRarity = (DestinyTierType)parent.TagData.ItemRarity,
                    ItemHash = item.TagData.InventoryItemHash.Hash32.ToString(),
                    ImageSource = dw,
                    ImageHeight = 96,
                    ImageWidth = 96,
                    GridBackground = brush,
                    Item = item,
                    Parent = parent
                };
                _allItems.TryAdd(item.TagData.InventoryItemHash.Hash32, newItem);
            }
        }
    }

    private bool ShouldAddToList(InventoryItem item, string type)
    {
        return (item.GetArtArrangementIndex() != -1 || type.Contains("Shader") || type.Contains("Ghost Shell")) && (!type.Contains("Finisher") && !type.Contains("Emote") && !type.Contains("Ship Schematics"));
    }

    public Dictionary<DrawingImage, ImageBrush> MakeIcon(InventoryItem item)
    {
        Dictionary<DrawingImage, ImageBrush> icon = new();
        string? type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value;
        if (type is null)
            type = "";

        // streams
        var bgStream = item.GetIconBackgroundStream();
        var bgOverlayStream = item.GetIconBackgroundOverlayStream();
        var primaryStream = item.GetIconPrimaryStream();
        var overlayStream = item.GetIconOverlayStream();

        //sometimes only the primary icon is valid
        var primary = primaryStream != null ? MakeBitmapImage(primaryStream, 96, 96) : null;
        var bg = bgStream != null ? MakeBitmapImage(bgStream, 96, 96) : null;

        //Most if not all legendary armor will use the ornament overlay because of transmog (I assume)
        var bgOverlay = bgOverlayStream != null && type.Contains("Ornament") ? MakeBitmapImage(bgOverlayStream, 96, 96) : null;
        var overlay = overlayStream != null ? MakeBitmapImage(overlayStream, 96, 96) : null;

        var group = new DrawingGroup();
        group.Children.Add(new ImageDrawing(bg, new Rect(0, 0, 96, 96)));
        group.Children.Add(new ImageDrawing(bgOverlay, new Rect(0, 0, 96, 96)));
        group.Children.Add(new ImageDrawing(primary, new Rect(0, 0, 96, 96)));
        group.Children.Add(new ImageDrawing(overlay, new Rect(0, 0, 96, 96)));

        var dw = new DrawingImage(group);
        dw.Freeze();

        ImageBrush brush = new ImageBrush(bg);
        brush.Freeze();

        icon.TryAdd(dw, brush);

        return icon;
    }
}

public class ApiItem
{
    public string ItemName { get; set; }
    public string ItemType { get; set; }
    public DestinyTierType ItemRarity { get; set; }
    public string ItemHash { get; set; }
    public double ImageWidth { get; set; }
    public double ImageHeight { get; set; }
    public System.Windows.Media.ImageSource ImageSource { get; set; }

    public System.Windows.Media.ImageBrush GridBackground { get; set; }

    public InventoryItem Item { get; set; }
    public InventoryItem Parent { get; set; }
}
