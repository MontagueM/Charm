using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tiger;
using Tiger.Schema.Investment;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Point = System.Drawing.Point;

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

    private void RefreshItemList()
    {
        string searchTerm = SearchBox.Text.ToLower();
        ConcurrentBag<ApiItem> items = new();

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 16 };
        Parallel.ForEach(_allItems.Values, parallelOptions, item =>
        {
            if (items.Count < 100 && item.ItemName.ToLower().Contains(searchTerm))
            {
                items.Add(item);
            }
        });
        var sortedItems = new List<ApiItem>(items);
        sortedItems.Sort((a, b) => b.Item.Hash.CompareTo(a.Item.Hash));
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
        List<string> mapStages = inventoryItems.Select((_, i) => $"loading {i+1}/{inventoryItems.Count()}").ToList();
        MainWindow.Progress.SetProgressStages(mapStages, false, true);
        await Parallel.ForEachAsync(inventoryItems, async (item, ct) =>
        {
            // if (_allItems.Count > 500)
            // {
            //     MainWindow.Progress.CompleteStage();
            //     return;
            // }
            string name = Investment.Get().GetItemName(item);
            string? type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value;
            // todo not sure why the type can now be null, be whatever
            if (type == null)
            {
                type = "";
            }
            if (item.GetArtArrangementIndex() != -1 || type.Contains("Shader"))
            {
                if (!type.Contains("Finisher") && !type.Contains("Emote")) // they point to Animation instead of Entity
                {
                    // icon bg
                    var bgStream = item.GetIconBackgroundStream();
                    var primaryStream = item.GetIconPrimaryStream();
                    var overlayStream = item.GetIconOverlayStream();
                    if (bgStream != null && primaryStream != null)
                    {
                        var bg = MakeBitmapImage(bgStream, 96, 96);
                        var primary = MakeBitmapImage(primaryStream, 96, 96);

                        BitmapImage? overlay = null;
                        if (overlayStream != null)
                        {
                            overlay = MakeBitmapImage(overlayStream, 96, 96);
                        }

                        var group = new DrawingGroup();
                        group.Children.Add(new ImageDrawing(bg, new Rect(0, 0, 96, 96)));
                        group.Children.Add(new ImageDrawing(primary, new Rect(0, 0, 96, 96)));
                        if (overlay != null)
                        {
                            group.Children.Add(new ImageDrawing(overlay, new Rect(0, 0, 96, 96)));
                        }

                        var dw = new DrawingImage(group);
                        dw.Freeze();

                        ImageBrush brush = new ImageBrush(bg);
                        brush.Freeze();

                        var newItem = new ApiItem
                        {
                            ItemName = name,
                            ItemType = type,
                            ImageSource = dw,
                            ImageHeight = 96,
                            ImageWidth = 96,
                            GridBackground = brush,
                            Item = item
                        };
                        _allItems.TryAdd(item.TagData.InventoryItemHash.Hash32, newItem);
                    }
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
        List<string> apiStages = _selectedItems.Select((_, i) => $"exporting {i+1}/{_selectedItems.Count}").ToList();
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
                // EntityView.ExportShader();

                MainWindow.Progress.CompleteStage();
            });
        });
    }

    private void OpenOutputFolder_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        Process.Start("explorer.exe", config.GetExportSavePath());
    }
}

public class ApiItem
{
    public string ItemName { get; set; }
    public string ItemType { get; set; }
    public double ImageWidth { get; set; }
    public double ImageHeight { get; set; }
    public System.Windows.Media.ImageSource ImageSource { get; set; }

    public System.Windows.Media.ImageBrush GridBackground { get; set; }

    public InventoryItem Item { get; set; }
}
