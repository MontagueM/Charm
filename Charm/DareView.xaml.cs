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
using Field.General;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Point = System.Drawing.Point;

namespace Charm;

public partial class DareView : UserControl
{
    private ConcurrentDictionary<DestinyHash, ApiItem> _allItems;
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
        ConcurrentBag<ApiItem> items = new ConcurrentBag<ApiItem>();

        Parallel.ForEach(_allItems, pair =>
        {
            if (items.Count < 100 && pair.Value.ItemName.ToLower().Contains(searchTerm))
            {
                items.Add(pair.Value);
            }
        });
            
        DareListView.ItemsSource = items;
    }

    public async void LoadContent()
    {
        _allItems = new ConcurrentDictionary<DestinyHash, ApiItem>();
        _selectedItems = new ObservableCollection<ApiItem>();
        await Task.Run(LoadApiList);
        RefreshItemList();
    }
    
    private void LoadApiList()
    {
        List<string> mapStages = InvestmentHandler.InventoryItems.Select((_, i) => $"loading {i+1}/{InvestmentHandler.InventoryItems.Count}").ToList();
        MainWindow.Progress.SetProgressStages(mapStages, false, true);
        Parallel.ForEach(InvestmentHandler.InventoryItems, kvp =>
        {
            if (_allItems.Count > 500)
            {
                MainWindow.Progress.CompleteStage();
                return;
            }
            string name = InvestmentHandler.GetItemName(kvp.Value);
            string type = InvestmentHandler.InventoryItemStringThings[InvestmentHandler.GetItemIndex(kvp.Key)].Header.ItemType;
            if (kvp.Value.GetArtArrangementIndex() != -1 || type.Contains("Shader"))
            {
                
                if (!type.Contains("Finisher") && !type.Contains("Emote")) // they point to Animation instead of Entity
                {
                    // icon bg
                    var bgStream = kvp.Value.GetIconBackgroundStream();
                    var primaryStream = kvp.Value.GetIconPrimaryStream();
                    var overlayStream = kvp.Value.GetIconOverlayStream();
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
                            Item = kvp.Value
                        };
                        _allItems.TryAdd(kvp.Key, newItem);
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
        if (_allItems.TryRemove(apiItem.Item.Header.InventoryItemHash, out _))
        {
            _selectedItems.Add(apiItem);
        }
        else
        {
            _allItems.TryAdd(apiItem.Item.Header.InventoryItemHash, apiItem);
            _selectedItems.Remove(apiItem);
        }
        SelectedItemView.ItemsSource = _selectedItems;
        RefreshItemList();
    }

    private void ClearQueue_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var selectedItem in _selectedItems)
        {
            _allItems.TryAdd(selectedItem.Item.Header.InventoryItemHash, selectedItem);
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
            Parallel.ForEach(_selectedItems, item =>
            {
                if (item.Item.GetArtArrangementIndex() != -1)
                {
                    // if has a model
                    EntityView.ExportInventoryItem(item);
                }
                else
                {
                    // shader
                    string savePath = ConfigHandler.GetExportSavePath();
                    string meshName = item.ItemName;
                    savePath += $"/{meshName}";
                    Directory.CreateDirectory(savePath);
                    Directory.CreateDirectory(savePath + "/Textures");
                    InvestmentHandler.ExportShader(item.Item, savePath, meshName, ConfigHandler.GetOutputTextureFormat());
                }
                // EntityView.ExportShader();

                MainWindow.Progress.CompleteStage();
            });
        });
    }

    private void OpenOutputFolder_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(ConfigHandler.GetExportSavePath());
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