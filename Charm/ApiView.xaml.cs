using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Field;
using Field.General;

namespace Charm;

public partial class ApiView : UserControl
{
    private ConcurrentBag<ApiItem> _apiItems = null;
    
    public ApiView()
    {
        InitializeComponent();
    }
    
    public async void LoadApiView()
    {
        LoadUI();
    }

    private void LoadUI()
    {
        InitialiseItems();
        RefreshItemList();
    }

    private void InitialiseItems()
    {
        GetInventoryItemsWithGeometry();
        SetItemListByString("");
    }

    private void GetInventoryItemsWithGeometry()
    {
        var s = Stopwatch.StartNew();
        _apiItems = new ConcurrentBag<ApiItem>();
        // foreach (var (key, value) in InvestmentHandler.InventoryItemHashmap)
        // {
        //     InventoryItem item = InvestmentHandler.GetInventoryItem(key);
        //     if (item.GetArtArrangementIndex() == -1) continue;
        //     string name = InvestmentHandler.GetItemName(key);
        //     _apiItems.Add(new ApiItem {Hash = key, Name = name});
        // }
        
        // accessing a single file must be done 1. using locks or 2. synchronously
        // add code to the IndexAccessList to use a single thread to read all the data, but then use
        // parallel code to get all the child files
        // the key requirement is that the parallel must NOT use duplicate handles
        // if so, we will cause thread-dangerous situations
        //
        Parallel.ForEach(InvestmentHandler.InventoryItems, kvp =>
        {
            if (kvp.Value.GetArtArrangementIndex() == -1) return;
            string name = InvestmentHandler.GetItemName(kvp.Value);
            _apiItems.Add(new ApiItem {Hash = kvp.Key, Name = $"{name}-{kvp.Key.Hash.ToString()}"});
        });
        // {
        //
        // }
        s.Stop();
        var t = s.ElapsedMilliseconds;

        
        var a = 0;
    }

    private void RefreshItemList()
    {
        string searchStr = textBox.Text.ToLower();
        SetItemListByString(searchStr);
    }

    private void SetItemListByString(string searchStr)
    {
        var displayItems = new ConcurrentBag<ApiItem>();
        // Select and sort by relevance to selected string
        Parallel.ForEach(_apiItems, item =>
        { 
            if (displayItems.Count > 50) return;
            if (item.Name.ToLower().Contains(searchStr))
            {
                displayItems.Add(item);
            }
        });
        // todo make sure we dont ever do activator create instance / new Tag stuff bc we should do PackageHandler.GetTag ...
        // todo better perf from initial caching now... also need to check if they have art arrangements lol
        // foreach (var (key, value) in InvestmentHandler.InventoryItemHashmap)
        // {
        //     if (apiItems.Count > 50) break;
        //     InventoryItem item = InvestmentHandler.GetInventoryItem(key);
        //     if (item.GetArtArrangementIndex() == -1) continue;
        //     string name = InvestmentHandler.GetItemName(key);
        //     if (key.Hash.ToString().Contains(searchStr) || name.Contains(searchStr))
        //     {
        //         apiItems.Add(new ApiItem {Hash = key, Name = name});
        //     }
        // }
        ApiItemList.ItemsSource = displayItems;
    }

private void DisplayApiEntityButton_OnClick(object sender, RoutedEventArgs e)
{
        
}
    
private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
{
    RefreshItemList();
}
}

public class ApiItem
{
    private string name;
    private string hash;
    
    public string Name
    {
        get => name;
        set => name = value;
    }
    
    public string Hash
    {
        get => hash;
        set => hash = value;
    }
}