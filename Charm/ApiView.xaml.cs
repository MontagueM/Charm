using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Field;
using Field.Entities;
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
        _apiItems = new ConcurrentBag<ApiItem>();
        Parallel.ForEach(InvestmentHandler.InventoryItems, kvp =>
        {
            if (kvp.Value.GetArtArrangementIndex() == -1) return;
            string name = InvestmentHandler.GetItemName(kvp.Value);
            _apiItems.Add(new ApiItem {Hash = kvp.Key, Name = $"{name}-{kvp.Key.Hash.ToString()}"});
        });
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
        ApiItemList.ItemsSource = displayItems;
        FontFamily fontFamily = new FontFamily(@"fonts/#ald55");
        textBox.FontFamily = fontFamily;
    }

private void DisplayApiEntityButton_OnClick(object sender, RoutedEventArgs e)
{
    var apiHash = new DestinyHash((sender as Button).Tag as string, true);
    List<Entity> entities = InvestmentHandler.GetEntitiesFromHash(apiHash);
    EntityView.LoadEntityFromApi(apiHash);
    var a = 0;
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