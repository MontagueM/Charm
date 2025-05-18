using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Tiger;
using Tiger.Schema.Investment;
using static Charm.APIItemView;
using static Charm.APITooltip;

namespace Charm;


// I'm not really proud of how messy this is....
public partial class BadgeView : UserControl
{
    private static MainWindow _mainWindow = null;
    private APITooltip ToolTip;

    private ConcurrentBag<ApiItem> _allItems;

    private List<SubcategoryChild> _subcategoriesChildren;

    public Tag<SD7788080> PresentationNodes = Investment.Get()._presentationNodeDefinitionMap;
    public Tag<S03588080> PresentationNodeStrings = Investment.Get()._presentationNodeDefinitionStringMap;

    private const int ItemsPerPage = 21;
    private const int ItemSetsPerPage = 7;
    private int CurrentPage = 0;

    private int SubcategoryChildrenPerPage = 9;
    private int CurrentSubcategoryChildrenPage = 0;

    private Subcategory CurrentSubcategory = null;

    public BadgeView(CollectionsView.ItemCategory itemCategory)
    {
        InitializeComponent();
        Header.DataContext = itemCategory;
        LoadSubcategories(itemCategory);

        _allItems = new();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        MouseMove += UserControl_MouseMove;
        KeyDown += Button_KeyDown;

        ToolTip = new();
        //MouseMove += ToolTip.UserControl_MouseMove;
        Panel.SetZIndex(ToolTip, 50);
        MainGrid.Children.Add(ToolTip);

        if (ConfigSubsystem.Get().GetAnimatedBackground())
        {
            SpinnerShader _spinner = new SpinnerShader();
            Spinner.Effect = _spinner;
            SizeChanged += _spinner.OnSizeChanged;
            _spinner.ScreenWidth = (float)ActualWidth;
            _spinner.ScreenHeight = (float)ActualHeight;
            _spinner.Scale = new(0, 0);
            _spinner.Offset = new(-3.6, -3.3);
        }
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
            Console.WriteLine($"{curNodeStrings.Name.Value.ToString().ToUpper()}");
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
        UIHelper.AnimateFade(SingleItemList, 0.15f, 1f, 0.1f);
    }


    private void LoadItems(int categoryIndex)
    {
        Dictionary<int, InventoryItem> inventoryItems = GetInventoryItems(categoryIndex);

        foreach (var item in inventoryItems)
        {
            string name = Investment.Get().GetItemName(item.Value);
            var itemStrings = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.Value.TagData.InventoryItemHash)).TagData;

            TigerHash plugCategoryHash = null;
            if (item.Value.TagData.Unk48.GetValue(item.Value.GetReader()) is SA1738080 plug)
                plugCategoryHash = plug.PlugCategoryHash;

            var newItem = new ApiItem
            {
                ItemName = name,
                ItemType = itemStrings.ItemType?.Value,
                ItemFlavorText = itemStrings.ItemFlavourText?.Value,
                ItemRarity = (DestinyTierType)item.Value.TagData.ItemRarity,
                ItemDamageType = DestinyDamageType.GetDamageType(item.Value.GetItemDamageTypeIndex()),
                ItemHash = item.Value.TagData.InventoryItemHash.Hash32.ToString(),
                ImageHeight = 96,
                ImageWidth = 96,
                Item = item.Value,
                Weight = item.Key,
                CollectableIndex = item.Key,
            };
            if (newItem.ItemDamageType == DestinyDamageTypeEnum.None)
            {
                if (newItem.Item.TagData.Unk70.GetValue(newItem.Item.GetReader()) is SC0778080 sockets)
                {
                    sockets.SocketEntries.ForEach(entry =>
                    {
                        if (entry.SocketTypeIndex == -1 || entry.SingleInitialItemIndex == -1)
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
                HasControls = true
            };
            newItem.PlugItem = plugItem;

            if (!_allItems.Any(x => x.ItemHash == newItem.ItemHash))
                _allItems.Add(newItem);
        }
    }

    public Dictionary<int, InventoryItem> GetInventoryItems(int index)
    {
        Dictionary<int, InventoryItem> inventoryItems = new();
        var nodes = PresentationNodes.TagData.PresentationNodeDefinitions;

        for (int i = 0; i < nodes[index].Collectables.Count; i++)
        {
            var item = nodes[index].Collectables[i];
            InventoryItem invItem = Investment.Get().GetInventoryItem(Investment.Get().GetCollectible(item.CollectableIndex).Value.InventoryItemIndex);
            inventoryItems.Add(item.CollectableIndex, invItem);
        }

        return inventoryItems;
    }


    private async void Subcategory_OnSelect(object sender, RoutedEventArgs e)
    {
        await Dispatcher.BeginInvoke(new Action(() =>
        {
            if ((sender as RadioButton) is null)
                return;

            Subcategory item = ((RadioButton)sender).DataContext as Subcategory;
            CurrentSubcategory = item;
            CurrentPage = 0;

            _allItems = new();
            LoadItems(item.ItemCategoryIndex);
            DisplayItems();

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

            CheckPages();

        }), DispatcherPriority.Background);
    }

    public void CheckPages()
    {
        PreviousPage.IsEnabled = CurrentPage != 0;
        NextPage.IsEnabled = _allItems.Count > 0 ? (CurrentPage + 1) * ItemsPerPage < _allItems.Count : false;
    }

    public void UnselectAllRadioButtons(ItemsControl itemsControl)
    {
        foreach (var item in itemsControl.Items)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter contentPresenter)
            {
                var radioButton = UIHelper.FindVisualChild<RadioButton>(contentPresenter);
                if (radioButton != null)
                {
                    radioButton.IsChecked = false;
                }
            }
        }
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
                var radioButton = UIHelper.FindVisualChild<RadioButton>(contentPresenter);
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                }
            }
        }), DispatcherPriority.Background);
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

        if (item.ItemFlavorText != null && item.ItemFlavorText != string.Empty)
        {
            PlugItem flavorText = new PlugItem
            {
                PlugOrderIndex = 0, // Flavor text is always first
                Description = item.ItemFlavorText,
            };
            ToolTip.AddToTooltip(flavorText, APITooltip.TooltipType.TextBlockItalic);
        }

        var sourceString = Investment.Get().GetCollectibleStrings(item.CollectableIndex).Value.SourceName.Value;
        if (sourceString != null && sourceString != string.Empty)
        {
            PlugItem source = new PlugItem
            {
                PlugOrderIndex = 3,
                Description = $"{sourceString}",
            };
            ToolTip.AddToTooltip(source, APITooltip.TooltipType.Source);
        }

        ToolTip.MakeTooltip(item.PlugItem);

        if (DareView.ShouldAddToList(item.Item, item.ItemType))
        {
            PlugItem inputItem2 = new PlugItem
            {
                PlugOrderIndex = 1,
                Name = $"", // Key glyph
                Type = $"", // 2nd key glyph (mouse left/right)
                Description = $"Export"
            };
            ToolTip.AddToTooltip(inputItem2, TooltipType.Input);
        }
    }

    private void CategoryButton_MouseEnter(object sender, MouseEventArgs e)
    {
        ToolTip.ActiveItem = (sender as ToggleButton);
        Subcategory item = (Subcategory)(sender as ToggleButton).DataContext;

        PlugItem plugItem = new()
        {
            Name = item.ItemCategoryName,
            Description = item.ItemCategoryDescription,
            PlugStyle = DestinySocketCategoryStyle.Reusable,
            HasControls = false
        };

        ToolTip.MakeTooltip(plugItem);
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
        gridTransform.X = position.X * -0.0075;
        gridTransform.Y = position.Y * -0.0075;
    }

    private async void Button_KeyDown(object sender, KeyEventArgs e)
    {
        if (ToolTip.ActiveItem == null || ToolTip.ActiveItem is not Button)
            return;

        e.Handled = true;
        if (e.Key == Key.Return)
        {
            ApiItem item = (ApiItem)(ToolTip.ActiveItem).DataContext;
            if (!DareView.ShouldAddToList(item.Item, item.ItemType))
                return;

            MainWindow.Progress.SetProgressStages(new() { $"Exporting {item.ItemName}" });
            await Task.Run(() =>
            {
                if ((item.ItemType == "Artifact" || item.ItemType == "Seasonal Artifact") && item.Item.TagData.Unk28.GetValue(item.Item.GetReader()) is SC5738080 gearSet)
                {
                    if (gearSet.ItemList.Count != 0)
                        item.Item = Investment.Get().GetInventoryItem(gearSet.ItemList.First().ItemIndex);
                }

                if (item.Item.GetArtArrangementIndex() != -1)
                {
                    EntityView.ExportInventoryItem(item, ConfigSubsystem.Get().GetExportSavePath());
                }
                else
                {
                    // shader
                    ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
                    string savePath = config.GetExportSavePath();
                    string itemName = Helpers.SanitizeString(item.ItemName);
                    savePath += $"/{itemName}";
                    Directory.CreateDirectory(savePath);
                    Directory.CreateDirectory(savePath + "/Textures");
                    Investment.Get().ExportShader(item.Item, savePath, itemName, config.GetOutputTextureFormat());
                }
            });
            MainWindow.Progress.CompleteStage();
        }
    }
}
