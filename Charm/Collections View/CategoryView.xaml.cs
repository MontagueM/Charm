using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
    Button? ActivePlugItemButton;

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
        DataContext = itemCategory;
        LoadSubcategories(itemCategory);
        MouseMove += UserControl_MouseMove;

        _allItemSets = new();
        _allItems = new();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
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
        AnimationHelper.AnimateFadeIn(SingleItemList, 0.2f, 1f, 0.5f);
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
        AnimationHelper.AnimateFadeIn(ItemSetList, 0.2f, 1f, 0.5f);
    }

    private void LoadItems(int categoryIndex)
    {
        Dictionary<int, InventoryItem> inventoryItems = GetInventoryItems(categoryIndex);

        Parallel.ForEach(inventoryItems, item =>
        {
            string name = Investment.Get().GetItemName(item.Value);
            string? type = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.Value.TagData.InventoryItemHash)).TagData.ItemType.Value;

            var newItem = new ApiItem
            {
                ItemName = name,
                ItemType = type,
                ItemRarity = (DestinyTierType)item.Value.TagData.ItemRarity,
                ItemHash = item.Value.TagData.InventoryItemHash.Hash32.ToString(),
                ImageHeight = 96,
                ImageWidth = 96,
                Item = item.Value,
                Weight = item.Key,
                CollectableIndex = item.Key
            };
            _allItems.Add(newItem);
        });
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

            Parallel.ForEach(inventoryItems, item =>
            {
                string name = Investment.Get().GetItemName(item.Value);
                string? type = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.Value.TagData.InventoryItemHash)).TagData.ItemType.Value;

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
                collectableSet.Items.Add(newItem);
            });

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
        Dispatcher.BeginInvoke(new Action(() =>
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

    ////////////////////////////////////////////////////////////////////////////
    // TODO: MOVE ALL TOOLTIP STUFF TO ITS OWN CLASS SO IT CAN BE USED ANYWHERE SO THIS SPAGHETTI CODE ISNT REUSED EVERYWHERE
    // AAAAAAAAAAAAAAAHHHHHHHHHHHHHHHHHHHHHHHHHH
    ////////////////////////////////////////////////////////////////////////////

    private void MakeTooltip(PlugItem item)
    {
        InfoBox.Visibility = Visibility.Visible;
        var fadeIn = FindResource("FadeIn 0.2") as Storyboard;
        InfoBox.BeginStoryboard(fadeIn);
        InfoBox.DataContext = item;

        if (item.Item.GetItemStrings().TagData.Unk38.GetValue(item.Item.GetItemStrings().GetReader()) is D2Class_D8548080 warning)
        {
            foreach (var rule in warning.InsertionRules)
            {
                if (rule.FailureMessage.Value is null)
                    continue;

                AddToTooltip(new PlugItem
                {
                    Description = rule.FailureMessage.Value,
                    PlugRarityColor = System.Windows.Media.Color.FromArgb(255, 255, 0, 0)
                }, TooltipType.WarningBlock);
            }
        }

        if (item.Item.GetItemStrings().TagData.Unk40.GetValue(item.Item.GetItemStrings().GetReader()) is D2Class_D7548080 preview)
        {
            if (preview.ScreenStyleHash.Hash32 == 3797307284) // 'screen_style_emblem'
            {
                AddToTooltip(new PlugItem
                {
                    PlugImageSource = ApiImageUtils.MakeFullIcon(item.Item.GetItemStrings().TagData.EmblemContainerIndex)
                }, TooltipType.Emblem);
            }
        }

        if (item.Description != "")
            AddToTooltip(item, TooltipType.TextBlock);

        if (item.Item.TagData.Unk38.GetValue(item.Item.GetReader()) is D2Class_B0738080 objectives)
        {
            foreach (var objective in objectives.Objectives)
            {
                var obj = Investment.Get().GetObjective(objective.ObjectiveIndex);
                if (obj is null)
                    continue;

                PlugItem objItem = new PlugItem
                {
                    Description = obj.Value.ProgressDescription.Value,
                    Type = $"0/{Investment.Get().GetObjectiveValue(objective.ObjectiveIndex)}",
                    PlugImageSource = obj.Value.IconIndex != -1 ? ApiImageUtils.MakeIcon(obj.Value.IconIndex) : null
                };

                TooltipType tooltipType = TooltipType.ObjectiveInteger;
                switch ((DestinyUnlockValueUIStyle)obj.Value.InProgressValueStyle)
                {
                    case DestinyUnlockValueUIStyle.Percentage:
                        tooltipType = TooltipType.ObjectivePercentage;
                        break;
                    case DestinyUnlockValueUIStyle.Integer:
                        objItem.Type = $"{Investment.Get().GetObjectiveValue(objective.ObjectiveIndex)}";
                        tooltipType = TooltipType.ObjectiveInteger;
                        break;
                }

                if (item.PlugStyle == DestinySocketCategoryStyle.Reusable)
                    AddToTooltip(null, TooltipType.Seperator);

                AddToTooltip(objItem, tooltipType); // TODO: Other styles
            }
        }

        foreach (var notif in item.Item.GetItemStrings().TagData.TooltipNotifications)
        {
            PlugItem notifItem = new PlugItem
            {
                Description = $"★ {notif.DisplayString.Value}",
                PlugImageSource = null
            };
            AddToTooltip(notifItem, TooltipType.InfoBlock);
        }
    }

    private void ClearTooltip()
    {
        InfoBoxStackPanel.Children.Clear();
        WarningBoxStackPanel.Children.Clear();
        InfoBox.Visibility = Visibility.Collapsed;
    }

    private void AddToTooltip(PlugItem item, TooltipType type)
    {
        switch (type)
        {
            case TooltipType.TextBlock:
                DataTemplate infoTextTemplate = (DataTemplate)FindResource("InfoBoxTextTemplate");
                FrameworkElement textUI = (FrameworkElement)infoTextTemplate.LoadContent();
                textUI.DataContext = item;
                InfoBoxStackPanel.Children.Add(textUI);
                break;
            case TooltipType.Grid:
                DataTemplate infoGridTemplate = (DataTemplate)FindResource("InfoBoxGridTemplate");
                FrameworkElement gridUi = (FrameworkElement)infoGridTemplate.LoadContent();
                gridUi.DataContext = item;
                InfoBoxStackPanel.Children.Add(gridUi);
                break;
            case TooltipType.InfoBlock:
                if (InfoBoxStackPanel.Children.Count != 0)
                {
                    DataTemplate infoBlockSepTemplate = (DataTemplate)FindResource("InfoBoxSeperatorTemplate");
                    FrameworkElement infoBlockSepUi = (FrameworkElement)infoBlockSepTemplate.LoadContent();
                    InfoBoxStackPanel.Children.Add(infoBlockSepUi);
                }

                DataTemplate infoBlockTemplate = (DataTemplate)FindResource("InfoBoxGridTemplate");
                FrameworkElement infoBlockUi = (FrameworkElement)infoBlockTemplate.LoadContent();
                infoBlockUi.DataContext = item;
                InfoBoxStackPanel.Children.Add(infoBlockUi);
                break;
            case TooltipType.WarningBlock:
                DataTemplate warningTextTemplate = (DataTemplate)FindResource("InfoBoxWarningTextTemplate");
                FrameworkElement warningTextUI = (FrameworkElement)warningTextTemplate.LoadContent();
                warningTextUI.DataContext = item;
                WarningBoxStackPanel.Children.Add(warningTextUI);
                break;
            case TooltipType.Seperator:
                DataTemplate seperatorTemplate = (DataTemplate)FindResource("InfoBoxSeperatorTemplate");
                FrameworkElement seperatorUi = (FrameworkElement)seperatorTemplate.LoadContent();
                InfoBoxStackPanel.Children.Add(seperatorUi);
                break;
            case TooltipType.Emblem:
                DataTemplate emblemTemplate = (DataTemplate)FindResource("InfoBoxEmblemTemplate");
                FrameworkElement emblemUi = (FrameworkElement)emblemTemplate.LoadContent();
                emblemUi.DataContext = item;
                InfoBoxStackPanel.Children.Add(emblemUi);
                break;

            case TooltipType.ObjectivePercentage:
                DataTemplate objPercentTemplate = (DataTemplate)FindResource("InfoBoxObjectivePercentageTemplate");
                FrameworkElement objPercentUi = (FrameworkElement)objPercentTemplate.LoadContent();
                objPercentUi.DataContext = item;
                InfoBoxStackPanel.Children.Add(objPercentUi);
                break;
            case TooltipType.ObjectiveInteger:
                DataTemplate objIntTemplate = (DataTemplate)FindResource("InfoBoxObjectiveIntegerTemplate");
                FrameworkElement objIntUi = (FrameworkElement)objIntTemplate.LoadContent();
                objIntUi.DataContext = item;
                InfoBoxStackPanel.Children.Add(objIntUi);
                break;
        }
    }

    private void PlugItem_MouseEnter(object sender, MouseEventArgs e)
    {
        ActivePlugItemButton = (sender as Button);
        ApiItem item = (ApiItem)(sender as Button).DataContext;

        TigerHash plugCategoryHash = null;
        if (item.Item.TagData.Unk48.GetValue(item.Item.GetReader()) is D2Class_A1738080 plug)
            plugCategoryHash = plug.PlugCategoryHash;

        PlugItem PlugItem = new PlugItem
        {
            Item = item.Item,
            Hash = item.Item.TagData.InventoryItemHash,
            Name = item.ItemName,
            Type = item.ItemType,
            Description = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.Item.TagData.InventoryItemHash)).TagData.ItemDisplaySource.Value.ToString(),
            //PlugSocketIndex = socketIndex,
            PlugCategoryHash = plugCategoryHash,
            PlugWatermark = ApiImageUtils.GetPlugWatermark(item.Item),
            PlugRarity = (DestinyTierType)item.Item.TagData.ItemRarity,
            PlugRarityColor = ((DestinyTierType)item.Item.TagData.ItemRarity).GetColor(),
            PlugSelected = false,
            PlugStyle = DestinySocketCategoryStyle.Consumable
        };

        MakeTooltip(PlugItem);

        PlugItem source = new PlugItem
        {
            Description = $"{Investment.Get().GetCollectibleStrings(item.CollectableIndex).Value.SourceName.Value}",
        };
        if (source.Description != string.Empty)
            AddToTooltip(source, TooltipType.InfoBlock);
    }

    private void PlugItem_MouseLeave(object sender, MouseEventArgs e)
    {
        ClearTooltip();
        ActivePlugItemButton = null;
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        System.Windows.Point position = e.GetPosition(this);
        if (InfoBox.Visibility == Visibility.Visible && ActivePlugItemButton != null)
        {
            float xOffset = 25;
            float yOffset = 25;
            float padding = 25;

            // this is stupid
            if (position.X >= ActualWidth / 2)
                xOffset = (-1 * xOffset) - (float)InfoBox.Width;

            if (position.Y - yOffset - padding - (float)InfoBox.ActualHeight <= 0)
                yOffset += (float)(position.Y - yOffset - padding - (float)InfoBox.ActualHeight);

            TranslateTransform infoBoxtransform = (TranslateTransform)InfoBox.RenderTransform;
            infoBoxtransform.X = position.X + xOffset;
            infoBoxtransform.Y = position.Y - yOffset - ActualHeight;
        }

        TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        gridTransform.X = position.X * -0.01;
        gridTransform.Y = position.Y * -0.01;
    }

    private enum TooltipType
    {
        InfoBlock,
        TextBlock,
        Grid,
        WarningBlock,
        Seperator,
        Emblem,

        ObjectivePercentage,
        ObjectiveInteger,
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

