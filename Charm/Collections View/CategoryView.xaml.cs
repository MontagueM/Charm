using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Tiger;
using Tiger.Schema.Investment;
using static Charm.APIItemView;

namespace Charm;


// I'm not really proud of how messy this is....
public partial class CategoryView : UserControl
{
    private static MainWindow _mainWindow = null;
    private APITooltip ToolTip;

    private ConcurrentBag<ApiItem> _allItems;
    private ConcurrentBag<CollectableSet> _allItemSets;

    private List<SubcategoryChild> _subcategoriesChildren;

    public Tag<D2Class_D7788080> PresentationNodes = Investment.Get()._presentationNodeDefinitionMap;
    public Tag<D2Class_03588080> PresentationNodeStrings = Investment.Get()._presentationNodeDefinitionStringMap;

    private const int ItemsPerPage = 21;
    private const int ItemSetsPerPage = 7;
    private int CurrentPage = 0;

    private int SubcategoryChildrenPerPage = 9;
    private int CurrentSubcategoryChildrenPage = 0;

    public CategoryView(CollectionsView.ItemCategory itemCategory)
    {
        InitializeComponent();
        Header.DataContext = itemCategory;
        LoadSubcategories(itemCategory);

        _allItemSets = new();
        _allItems = new();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        MouseMove += UserControl_MouseMove;

        ToolTip = new();
        ToolTip.UserInput.Visibility = Visibility.Visible;
        MouseMove += ToolTip.UserControl_MouseMove;
        Panel.SetZIndex(ToolTip, 50);
        MainGrid.Children.Add(ToolTip);
    }

    public void LoadSubcategories(CollectionsView.ItemCategory itemCategory)
    {
        var nodes = PresentationNodes.TagData.PresentationNodeDefinitions;
        var strings = PresentationNodeStrings.TagData.PresentationNodeDefinitionStrings;

        List<Subcategory> items = new List<Subcategory>();
        for (int i = 0; i < nodes[itemCategory.ItemCategoryIndex].PresentationNodes.Count; i++)
        {
            var node = nodes[itemCategory.ItemCategoryIndex].PresentationNodes[i];
            var curNode = nodes[node.PresentationNodeIndex];
            var curNodeStrings = strings[node.PresentationNodeIndex];

            Subcategory subcategory = new()
            {
                ItemCategoryIndex = node.PresentationNodeIndex,
                ItemCategoryIcon = ApiImageUtils.MakeFullIcon(curNodeStrings.IconIndex),
                ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                ItemCategoryDescription = curNodeStrings.Description.Value,
                Index = i,
            };
            items.Add(subcategory);
        }
        Subcategories.ItemsSource = items;

        SelectRadioButton(Subcategories, 0);
    }

    private void DisplaySubcategoryChildren()
    {
        if (_subcategoriesChildren.Count > 9)
        {
            SubcategoryChildrenPerPage = 8;
            PreviousChildPage.Visibility = Visibility.Visible;
            NextChildPage.Visibility = Visibility.Visible;
        }
        else
        {
            SubcategoryChildrenPerPage = 9;
            PreviousChildPage.Visibility = Visibility.Collapsed;
            NextChildPage.Visibility = Visibility.Collapsed;
        }

        var itemsToShow = _subcategoriesChildren.Skip(CurrentSubcategoryChildrenPage * SubcategoryChildrenPerPage).Take(SubcategoryChildrenPerPage).ToList();
        int placeholderCount = SubcategoryChildrenPerPage - itemsToShow.Count;
        for (int i = 0; i < placeholderCount; i++)
        {
            itemsToShow.Add(new SubcategoryChild { IsPlaceholder = true });
        }
        SubcategoryChildren.ItemsSource = itemsToShow;
        AnimationHelper.AnimateFadeIn(SubcategoryChildren, 0.15f, 1f, 0.1f);
    }

    private void DisplayItems()
    {
        PreviousPage.Visibility = _allItems.Count > ItemsPerPage ? Visibility.Visible : Visibility.Hidden;
        NextPage.Visibility = _allItems.Count > ItemsPerPage ? Visibility.Visible : Visibility.Hidden;

        var itemsToShow = _allItems.OrderBy(x => x.Weight).Skip(CurrentPage * ItemsPerPage).Take(ItemsPerPage).ToList();

        // Add placeholders if necessary
        int placeholderCount = ItemsPerPage - itemsToShow.Count;
        for (int i = 0; i < placeholderCount; i++)
        {
            itemsToShow.Add(new ApiItem { IsPlaceholder = true });
        }

        SingleItemList.ItemsSource = itemsToShow;
        AnimationHelper.AnimateFadeIn(SingleItemList, 0.15f, 1f, 0.1f);
    }

    private void DisplayItemSets()
    {
        if (_allItemSets.Count > ItemSetsPerPage)
        {
            PreviousPage.Visibility = Visibility.Visible;
            NextPage.Visibility = Visibility.Visible;
        }
        else
        {
            PreviousPage.Visibility = Visibility.Hidden;
            NextPage.Visibility = Visibility.Hidden;
        }

        var itemsToShow = _allItemSets.OrderBy(x => x.Index).Skip(CurrentPage * ItemSetsPerPage).Take(ItemSetsPerPage).ToList();

        // Add placeholders if necessary
        int placeholderCount = ItemSetsPerPage - itemsToShow.Count;
        for (int i = 0; i < placeholderCount; i++)
        {
            itemsToShow.Add(new CollectableSet { IsPlaceholder = true });
        }

        ItemSetList.ItemsSource = itemsToShow;
        AnimationHelper.AnimateFadeIn(ItemSetList, 0.15f, 1f, 0.1f);
    }

    private void LoadItems(int categoryIndex)
    {
        Dictionary<int, InventoryItem> inventoryItems = GetInventoryItems(categoryIndex);

        foreach (var item in inventoryItems)
        {
            string name = Investment.Get().GetItemName(item.Value);
            string? type = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.Value.TagData.InventoryItemHash)).TagData.ItemType.Value;

            TigerHash plugCategoryHash = null;
            if (item.Value.TagData.Unk48.GetValue(item.Value.GetReader()) is D2Class_A1738080 plug)
                plugCategoryHash = plug.PlugCategoryHash;

            var newItem = new ApiItem
            {
                ItemName = name,
                ItemType = type,
                ItemRarity = (DestinyTierType)item.Value.TagData.ItemRarity,
                ItemDamageType = DestinyDamageType.GetDamageType(item.Value.GetItemDamageTypeIndex()),
                ItemHash = item.Value.TagData.InventoryItemHash.Hash32.ToString(),
                ImageHeight = 96,
                ImageWidth = 96,
                Item = item.Value,
                Weight = item.Key,
                CollectableIndex = item.Key
            };
            if (newItem.ItemDamageType == DestinyDamageTypeEnum.None)
            {
                if (newItem.Item.TagData.Unk70.GetValue(newItem.Item.GetReader()) is D2Class_C0778080 sockets)
                {
                    sockets.SocketEntries.ForEach(entry =>
                    {
                        if (entry.SocketTypeIndex == -1)
                            return;
                        var socket = Investment.Get().GetSocketType(entry.SocketTypeIndex);
                        foreach (var a in socket.PlugWhitelists)
                        {
                            if (a.PlugCategoryHash.Hash32 == 1466776700) // 'v300.weapon.damage_type.energy', Y1 weapon that uses a damage type mod from ye olden days
                            {
                                var item = Investment.Get().GetInventoryItem(entry.SingleInitialItemIndex);
                                item.Load(true); // idk why the item sometimes isnt fully loaded
                                var index = item.GetItemDamageTypeIndex();
                                newItem.ItemDamageType = DestinyDamageType.GetDamageType(index);
                            }
                        }
                    });
                }
            }

            PlugItem plugItem = new PlugItem
            {
                Item = newItem.Item,
                Hash = newItem.Item.TagData.InventoryItemHash,
                Name = newItem.ItemName,
                Type = newItem.ItemType,
                Description = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(newItem.Item.TagData.InventoryItemHash)).TagData.ItemDisplaySource.Value.ToString(),
                PlugCategoryHash = plugCategoryHash,
                PlugWatermark = ApiImageUtils.GetPlugWatermark(newItem.Item),
                PlugRarity = (DestinyTierType)newItem.Item.TagData.ItemRarity,
                PlugRarityColor = ((DestinyTierType)newItem.Item.TagData.ItemRarity).GetColor(),
                PlugStyle = DestinySocketCategoryStyle.Consumable,
                PlugDamageType = newItem.ItemDamageType,
                PlugSelected = false,
            };
            newItem.PlugItem = plugItem;

            _allItems.Add(newItem);
        }
    }

    private void LoadItemSets(int categoryIndex)
    {
        var node = PresentationNodes.TagData.PresentationNodeDefinitions[categoryIndex];
        var strings = PresentationNodeStrings.TagData.PresentationNodeDefinitionStrings;

        for (int i = 0; i < node.PresentationNodes.Count; i++)
        {
            var CurNode = node.PresentationNodes[i];
            Dictionary<int, InventoryItem> inventoryItems = GetInventoryItems(CurNode.PresentationNodeIndex);

            CollectableSet collectableSet = new()
            {
                Items = new(),
                ItemCategoryIndex = CurNode.PresentationNodeIndex,
                ItemCategoryName = strings[CurNode.PresentationNodeIndex].Name.Value,
                Index = i
            };

            foreach (var item in inventoryItems)
            {
                string name = Investment.Get().GetItemName(item.Value);
                string? type = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.Value.TagData.InventoryItemHash)).TagData.ItemType.Value;

                TigerHash plugCategoryHash = null;
                if (item.Value.TagData.Unk48.GetValue(item.Value.GetReader()) is D2Class_A1738080 plug)
                    plugCategoryHash = plug.PlugCategoryHash;

                var newItem = new ApiItem
                {
                    ItemName = name,
                    ItemType = type,
                    ItemRarity = (DestinyTierType)item.Value.TagData.ItemRarity,
                    ItemHash = item.Value.TagData.InventoryItemHash.Hash32.ToString(),
                    ImageHeight = 96,
                    ImageWidth = 96,
                    Item = item.Value,
                    Weight = item.Key
                };
                PlugItem plugItem = new PlugItem
                {
                    Item = newItem.Item,
                    Hash = newItem.Item.TagData.InventoryItemHash,
                    Name = newItem.ItemName,
                    Type = newItem.ItemType,
                    Description = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(newItem.Item.TagData.InventoryItemHash)).TagData.ItemDisplaySource.Value.ToString(),
                    PlugCategoryHash = plugCategoryHash,
                    PlugWatermark = ApiImageUtils.GetPlugWatermark(newItem.Item),
                    PlugRarity = (DestinyTierType)newItem.Item.TagData.ItemRarity,
                    PlugRarityColor = ((DestinyTierType)newItem.Item.TagData.ItemRarity).GetColor(),
                    PlugStyle = DestinySocketCategoryStyle.Consumable,
                    PlugDamageType = newItem.ItemDamageType,
                    PlugSelected = false,
                };
                newItem.PlugItem = plugItem;

                collectableSet.Items.Add(newItem);
            }

            int placeholderCount = 5 - collectableSet.Items.Count;
            for (int j = 0; j < placeholderCount; j++)
            {
                collectableSet.Items.Add(new ApiItem { IsPlaceholder = true });
            }

            _allItemSets.Add(collectableSet);
        }
    }

    public Dictionary<int, InventoryItem> GetInventoryItems(int index)
    {
        Dictionary<int, InventoryItem> inventoryItems = new();
        var nodes = PresentationNodes.TagData.PresentationNodeDefinitions;

        for (int i = 0; i < nodes[index].Collectables.Count; i++)
        {
            var item = nodes[index].Collectables[i];
            InventoryItem invItem = Investment.Get().GetInventoryItem(Investment.Get().GetCollectible(item.Index).Value.InventoryItemIndex);
            inventoryItems.Add(item.Index, invItem);
        }

        return inventoryItems;
    }


    private void Subcategory_OnSelect(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if ((sender as RadioButton) is null)
                return;

            Subcategory item = ((RadioButton)sender).DataContext as Subcategory;

            var nodes = PresentationNodes.TagData.PresentationNodeDefinitions;
            var strings = PresentationNodeStrings.TagData.PresentationNodeDefinitionStrings;

            _subcategoriesChildren = new();
            for (int i = 0; i < nodes[item.ItemCategoryIndex].PresentationNodes.Count; i++)
            {
                var node = nodes[item.ItemCategoryIndex].PresentationNodes[i];
                var curNode = nodes[node.PresentationNodeIndex];
                var curNodeStrings = strings[node.PresentationNodeIndex];

                SubcategoryChild subcategory = new()
                {
                    ItemCategoryIndex = node.PresentationNodeIndex,
                    ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                    Index = i,
                    //IsSelected = i == 0
                };
                _subcategoriesChildren.Add(subcategory);
            }

            CurrentSubcategoryChildrenPage = 0;
            DisplaySubcategoryChildren();
            SelectRadioButton(SubcategoryChildren, 0);

            SubcategoryType.Text = item.ItemCategoryName;
            AnimateTextBlock();
        }), DispatcherPriority.Background);
    }

    private async void SubcategoryChild_OnSelect(object sender, RoutedEventArgs e)
    {
        await Dispatcher.BeginInvoke(new Action(() =>
        {
            if ((sender as RadioButton) is null)
                return;

            SubcategoryChild item = ((RadioButton)sender).DataContext as SubcategoryChild;
            CurrentPage = 0;

            // Not ideal but it works for what it needs to do
            if (PresentationNodes.TagData.PresentationNodeDefinitions[item.ItemCategoryIndex].PresentationNodes.Count > 0)
            {
                _allItemSets = new();
                LoadItemSets(item.ItemCategoryIndex);
                DisplayItemSets();
            }
            else
            {
                _allItems = new();
                LoadItems(item.ItemCategoryIndex);
                DisplayItems();
            }

            CheckPages();
        }), DispatcherPriority.Background);
    }

    // Collection Items
    private void PreviousPage_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_allItems is not null && _allItems.Count > 0)
            {
                if (CurrentPage > 0)
                {
                    CurrentPage--;
                    DisplayItems();
                }
            }

            if (_allItemSets is not null && _allItemSets.Count > 0)
            {
                if (CurrentPage > 0)
                {
                    CurrentPage--;
                    DisplayItemSets();
                }
            }

            CheckPages();

        }), DispatcherPriority.Background);
    }

    private void NextPage_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_allItems is not null && _allItems.Count > 0)
            {
                if ((CurrentPage + 1) * ItemsPerPage < _allItems.Count)
                {
                    CurrentPage++;
                    DisplayItems();
                }
            }

            if (_allItemSets is not null && _allItemSets.Count > 0)
            {
                if ((CurrentPage + 1) * ItemSetsPerPage < _allItemSets.Count)
                {
                    CurrentPage++;
                    DisplayItemSets();
                }
            }

            CheckPages();

        }), DispatcherPriority.Background);
    }

    public void CheckPages()
    {
        PreviousPage.IsEnabled = CurrentPage != 0;
        NextPage.IsEnabled =
            _allItemSets.Count > 0 ? (CurrentPage + 1) * ItemSetsPerPage < _allItemSets.Count :
            _allItems.Count > 0 ? (CurrentPage + 1) * ItemsPerPage < _allItems.Count : false;
    }

    // Subcategory Children
    private void PreviousChildPage_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_subcategoriesChildren is null)
                return;

            if (CurrentSubcategoryChildrenPage > 0)
            {
                CurrentSubcategoryChildrenPage--;
                UnselectAllRadioButtons(SubcategoryChildren);
                DisplaySubcategoryChildren();
                SelectRadioButton(SubcategoryChildren, 0);
            }
        }), DispatcherPriority.Background);
    }

    private void NextChildPage_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_subcategoriesChildren is null)
                return;

            if ((CurrentSubcategoryChildrenPage + 1) * SubcategoryChildrenPerPage < _subcategoriesChildren.Count)
            {
                CurrentSubcategoryChildrenPage++;
                UnselectAllRadioButtons(SubcategoryChildren);
                DisplaySubcategoryChildren();
                SelectRadioButton(SubcategoryChildren, 0);
            }
        }), DispatcherPriority.Background);
    }

    public void UnselectAllRadioButtons(ItemsControl itemsControl)
    {
        foreach (var item in itemsControl.Items)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter contentPresenter)
            {
                var radioButton = FindVisualChild<RadioButton>(contentPresenter);
                if (radioButton != null)
                {
                    radioButton.IsChecked = false;
                }
            }
        }
    }

    private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
            {
                return t;
            }
            var result = FindVisualChild<T>(child);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    public void SelectRadioButton(ItemsControl itemsControl, int index)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (index < 0 || index >= itemsControl.Items.Count)
                return;

            var item = itemsControl.Items[index];
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter contentPresenter)
            {
                var radioButton = FindVisualChild<RadioButton>(contentPresenter);
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                }
            }
        }), DispatcherPriority.Background);
    }

    private void AnimateTextBlock()
    {
        Storyboard textChangeAnimation = (Storyboard)FindResource("TextChangeAnimation");
        textChangeAnimation.Begin(SubcategoryType);
    }

    private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        ApiItem item = (sender as Button).DataContext as ApiItem;

        APIItemView apiItemView = new APIItemView(item);
        _mainWindow.MakeNewTab(item.ItemName, apiItemView);
        _mainWindow.SetNewestTabSelected();
    }

    private void PlugItem_MouseEnter(object sender, MouseEventArgs e)
    {
        ToolTip.ActiveItem = (sender as Button);
        ApiItem item = (ApiItem)(sender as Button).DataContext;

        ToolTip.MakeTooltip(item.PlugItem);

        PlugItem source = new PlugItem
        {
            Description = $"{Investment.Get().GetCollectibleStrings(item.CollectableIndex).Value.SourceName.Value}",
        };
        if (source.Description != string.Empty)
            ToolTip.AddToTooltip(source, APITooltip.TooltipType.InfoBlock);
    }

    public void PlugItem_MouseLeave(object sender, MouseEventArgs e)
    {
        ToolTip.ClearTooltip();
        ToolTip.ActiveItem = null;
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        Point position = e.GetPosition(this);

        TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        gridTransform.X = position.X * -0.01;
        gridTransform.Y = position.Y * -0.01;
    }
}

public class Subcategory
{
    public int ItemCategoryIndex;
    public ImageSource ItemCategoryIcon { get; set; }
    public ImageSource ItemCategoryIcon2 { get; set; }
    public string ItemCategoryName { get; set; }
    public string ItemCategoryDescription { get; set; }
    public int ItemCategoryAmount { get; set; }

    public int Index { get; set; }
    public bool IsSelected { get; set; } = false;
}

public class SubcategoryChild
{
    public int ItemCategoryIndex;
    public string ItemCategoryName { get; set; }
    public string ItemCategoryDescription { get; set; }
    public int ItemCategoryAmount { get; set; }

    public int Index { get; set; }
    public bool IsSelected { get; set; } = false;
    public bool IsPlaceholder { get; set; } = false;
}

public class CollectableSet
{
    public List<ApiItem> Items { get; set; }
    public int ItemCategoryIndex { get; set; }
    public string ItemCategoryName { get; set; }
    public int ItemCategoryAmount { get; set; }

    public int Index { get; set; }
    public bool IsSelected { get; set; } = false;
    public bool IsPlaceholder { get; set; } = false;
}

public class ItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate NormalItemTemplate { get; set; }
    public DataTemplate PlaceholderTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        var itemObj = item as ApiItem;
        return itemObj != null && itemObj.IsPlaceholder ? PlaceholderTemplate : NormalItemTemplate;
    }
}

public class ItemSetTemplateSelector : DataTemplateSelector
{
    public DataTemplate NormalItemTemplate { get; set; }
    public DataTemplate PlaceholderTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        var itemObj = item as CollectableSet;
        return itemObj != null && itemObj.IsPlaceholder ? PlaceholderTemplate : NormalItemTemplate;
    }
}

public class SubcategoryChildItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate NormalItemTemplate { get; set; }
    public DataTemplate PlaceholderTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        var itemObj = item as SubcategoryChild;
        return itemObj != null && itemObj.IsPlaceholder ? PlaceholderTemplate : NormalItemTemplate;
    }
}

