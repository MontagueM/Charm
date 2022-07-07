﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Field;
using Field.Entities;
using Field.General;

namespace Charm;

public partial class ApiView : UserControl
{
    private ConcurrentBag<ApiItem> _apiItems = null;
    private ToggleButton _pressedButton = null;
    
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
            string type = InvestmentHandler.InventoryItemStringThings[InvestmentHandler.GetItemIndex(kvp.Key)].Header.ItemType;
            _apiItems.Add(new ApiItem {Hash = kvp.Key, Name = $"{name} | {kvp.Key.Hash.ToString()}", Type = type});
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
    }
    
    private void DisplayApiEntityButton_OnClick(object sender, RoutedEventArgs e)
    {
        var btn = sender as ToggleButton;
        if (_pressedButton != null)
            _pressedButton.IsChecked = false;
        _pressedButton = btn;
        btn.IsChecked = true;
        var apiHash = new DestinyHash((btn.DataContext as ApiItem).Hash, true);
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
    public string Name { get; set; }
    public string Hash { get; set; }
    public string Type { get; set; }
    public bool IsChecked { get; set; }
}