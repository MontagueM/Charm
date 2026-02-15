using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using static Charm.CategoryView;
using static Charm.CollectionsView;

namespace Charm;

public partial class CategoryView : UserControl
{
    private static MainWindow _mainWindow = null;
    private Investment Investment => Investment.Get();

    private DynamicArray<SDB788080> PresentationNodes = Investment.Get()._presentationNodeDefinitionMap.TagData.PresentationNodeDefinitions;
    private DynamicArray<S07588080> PresentationNodeStrings = Investment.Get()._presentationNodeDefinitionStringMap.TagData.PresentationNodeDefinitionStrings;

    public CategoryView(Category itemCategory)
    {
#if DEBUG
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif

        DataContext = itemCategory;
        InitializeComponent();
        LoadCategories(itemCategory);
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        Focusable = true;
        Focus();

        Subcategories.ItemTemplate = (DataTemplate)FindResource("SubcategoryItemTemplate");
        SubcategoryItems.ItemTemplateSelector = new CategoryEntryTemplateSelector
        {
            RecordTemplate = this.Resources["RecordEntryTemplate"] as DataTemplate,
            CollectibleTemplate = this.Resources["CollectibleEntryTemplate"] as DataTemplate,
            CollectibleSetTemplate = this.Resources["CollectibleSetEntryTemplate"] as DataTemplate
        };

        _mainWindow = Window.GetWindow(this) as MainWindow;
        MouseMove += UserControl_MouseMove;
        PreviewKeyDown += Button_KeyDown;
    }

    public void LoadCategories(Category itemCategory)
    {
        List<Category> items = new();

        for (int i = 0; i < PresentationNodes[itemCategory.ItemCategoryIndex].PresentationNodes.Count; i++)
        {
            SED788080 node = PresentationNodes[itemCategory.ItemCategoryIndex].PresentationNodes[i];
            SDB788080 curNode = PresentationNodes[node.PresentationNodeIndex];
            S07588080 curNodeStrings = PresentationNodeStrings[node.PresentationNodeIndex];

            Category subcategory = new()
            {
                ItemCategoryIndex = node.PresentationNodeIndex,
                ItemCategoryIcon = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex),
                ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                ItemCategoryDescription = curNodeStrings.Description.Value?.ToString() ?? "",
                //ItemCategoryDescription = $"{GetItemCategoryAmount(node.PresentationNodeIndex)} Items.",
                Index = i,
            };
            items.Add(subcategory);
        }
        Categories.ItemsSource = items;
        UIHelper.SelectRadioButton(Categories, 0);

        Dispatcher.InvokeAsync(() =>
        {
            if (items.Count <= 1)
                Categories.Visibility = Visibility.Collapsed;
        }, DispatcherPriority.Background);
    }

    private async void Category_OnSelect(object sender, RoutedEventArgs e)
    {
        await Dispatcher.BeginInvoke(new Action(() =>
        {
            if ((sender as RadioButton) is null)
                return;
            Category item = ((RadioButton)sender).DataContext as Category;

            List<Category> _buttons = new();
            for (int i = 0; i < PresentationNodes[item.ItemCategoryIndex].PresentationNodes.Count; i++)
            {
                SED788080 node = PresentationNodes[item.ItemCategoryIndex].PresentationNodes[i];
                SDB788080 curNode = PresentationNodes[node.PresentationNodeIndex];
                S07588080 curNodeStrings = PresentationNodeStrings[node.PresentationNodeIndex];

                Category subcategory = new()
                {
                    ItemCategoryIndex = node.PresentationNodeIndex,
                    ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                    Index = i,
                };

                _buttons.Add(subcategory);
            }

            Subcategories.Items = _buttons;
            Subcategories.DisplayItems(true);

            SubcategoryType.Text = item.ItemCategoryName;
            if (Categories.Items.Count <= 1)
                SubcategoryType.Visibility = Visibility.Collapsed;

            UIHelper.SelectRadioButton(Subcategories.ItemList, 0);

            AnimateTextBlock();
        }), DispatcherPriority.Normal);
    }

    private async void Subcategory_OnSelect(object sender, RoutedEventArgs e)
    {
        await Dispatcher.BeginInvoke(new Action(() =>
        {
            if ((sender as RadioButton) is null)
                return;

            Category item = ((RadioButton)sender).DataContext as Category;
            if (item is null)
                return;

            LoadItems(item.ItemCategoryIndex);
        }), DispatcherPriority.Send);
    }

    private void LoadItems(int index)
    {
        List<CategoryEntry> items = new();

        int presCount = PresentationNodes[index].PresentationNodes.Count;
        int recordCount = PresentationNodes[index].Records.Count;
        int collectibleCount = PresentationNodes[index].Collectibles.Count;
        if (recordCount > 0 || collectibleCount > 0 || presCount > 0)
        {
            // I'm not sure if there can be an entry with multiple types?
            Debug.Assert((recordCount > 0) != (collectibleCount > 0) != (presCount > 0));
        }

        if (CharmApp.CharmRedacted is not null)
        {
            var loaderType = CharmApp.CharmRedacted.GetType("Charm.Redacted.RedactedAPI");
            if (loaderType != null)
            {
                dynamic loader = Activator.CreateInstance(loaderType);
                loader.LoadCategoryViewRecords(this, items, index);
            }
        }

        // Collectibles
        foreach (var collectible in PresentationNodes[index].Collectibles)
        {
            var item = Investment.GetCollectible(collectible.CollectableIndex).Value;
            var strings = Investment.GetCollectibleStrings(collectible.CollectableIndex).Value;

            var invItem = Investment.GetInventoryItem(item.InventoryItemIndex);

            CategoryEntry subcategory = new()
            {
                Collectible = new(invItem),
                ItemHash = invItem.ApiHash,
                ItemIndex = collectible.CollectableIndex,
                //ItemIcon = strings.IconIndex != -1 ? ApiImageUtils.MakeFullItemIcon(invItem) : null,
                //ItemIcon2 = ApiImageUtils.GetPlugWatermark(invItem),
                ItemName = strings.CollectibleName.Value?.ToString() ?? "",
                ItemType = invItem.Type ?? "",
                ItemDescription = invItem.FlavorText ?? "",
                EntryType = CategoryEntryType.Collectible
            };

            if (!items.Any(x => x.ItemHash == subcategory.ItemHash))
                items.Add(subcategory);

            SubcategoryItems.Columns = 3;
            SubcategoryItems.ItemsPerPage = 21;
        }

        // Collectible Sets
        // TODO taking around half a second on first load for some reason, too slow for my liking
        foreach (var collectibleSet in PresentationNodes[index].PresentationNodes)
        {
            SDB788080 curNode = PresentationNodes[collectibleSet.PresentationNodeIndex];
            S07588080 curNodeStrings = PresentationNodeStrings[collectibleSet.PresentationNodeIndex];

            CategoryEntry set = new()
            {
                ItemHash = curNode.Hash,
                ItemIndex = collectibleSet.PresentationNodeIndex,
                ItemName = curNodeStrings.Name.Value.ToString(),
                EntryType = CategoryEntryType.CollectibleSet
            };

            for (int i = 0; i < 5; i++)
            {
                if (i >= curNode.Collectibles.Count)
                {
                    set.Children.Add(new() { IsPlaceholder = true });
                    continue;
                }

                var collectible = curNode.Collectibles[i];

                var item = Investment.GetCollectible(collectible.CollectableIndex).Value;
                var strings = Investment.GetCollectibleStrings(collectible.CollectableIndex).Value;

                var invItem = Investment.GetInventoryItem(item.InventoryItemIndex);

                CategoryEntry setEntry = new()
                {
                    Collectible = new(invItem),
                    ItemHash = invItem.ApiHash,
                    ItemIndex = collectible.CollectableIndex,
                    //ItemIcon = strings.IconIndex != -1 ? ApiImageUtils.MakeFullItemIcon(invItem) : null,
                    //ItemIcon2 = ApiImageUtils.GetPlugWatermark(invItem),
                    ItemName = strings.CollectibleName.Value?.ToString() ?? "",
                    ItemType = invItem.Type ?? "",
                    ItemDescription = invItem.FlavorText ?? "",
                    EntryType = CategoryEntryType.Collectible
                };

                set.Children.Add(setEntry);
            }

            if (!items.Any(x => x.ItemHash == set.ItemHash))
                items.Add(set);

            SubcategoryItems.Columns = 1;
            SubcategoryItems.ItemsPerPage = 7;
        }

        // Gotta do this here since entries are actually sorted by index in-game?
        var sortedItems = items.OrderBy(x => x.ItemIndex).ToList();
        for (int i = 0; i < sortedItems.Count; i++)
        {
            sortedItems[i].Index = i + 1;
        }

        SubcategoryItems.Items = sortedItems;
        SubcategoryItems.DisplayItems(true);
    }

    private void AnimateTextBlock()
    {
        Storyboard textChangeAnimation = (Storyboard)FindResource("TextChangeAnimation");
        textChangeAnimation.Begin(SubcategoryType);
    }

    private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        CategoryEntry item = (sender as Button).DataContext as CategoryEntry;

        //APIItemView apiItemView = new(item.Item);
        ItemView apiItemView = new(item.Collectible.Item);
        _mainWindow.MakeNewTab(item.ItemName, apiItemView);
        _mainWindow.SetNewestTabSelected();
    }

    private void CategoryButton_MouseEnter(object sender, MouseEventArgs e)
    {
    }

    private void CategoryEntryButton_MouseEnter(object sender, MouseEventArgs e)
    {
    }

    public void PlugItem_MouseLeave(object sender, MouseEventArgs e)
    {
    }


    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        if (!ConfigSubsystem.Get().GetMotionEffects())
            return;

        float x = -12f / (float)MainWindow.Current.ActualWidth;
        float y = -12f / (float)MainWindow.Current.ActualHeight;
        Point position = e.GetPosition(this);

        TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        gridTransform.X = (int)Math.Round(position.X * x);
        gridTransform.Y = (int)Math.Round(position.Y * y);
    }

    private async void Button_KeyDown(object sender, KeyEventArgs e)
    {
        var tooltip = MainWindow.Current.ToolTip;
        if (tooltip.ActiveItem is null or not Button)
            return;

        e.Handled = true;

        if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            CategoryEntry item = (CategoryEntry)tooltip.ActiveItem.DataContext;
            Clipboard.SetText($"{item.ItemHash}");
        }

        if (e.Key == Key.Return)
        {
            CategoryEntry item = (CategoryEntry)tooltip.ActiveItem.DataContext;
            if (item.EntryType != CategoryEntryType.Collectible)
                return;

            if (!DareView2.ShouldAddToList(item.Collectible.Item))
                return;

            MainWindow.Progress.SetProgressStages(new() { $"Exporting {item.ItemName}" });
            await Task.Run(() =>
            {
                if ((item.ItemType == "Artifact" || item.ItemType == "Seasonal Artifact")
                && item.Collectible.Item.TagData.Unk28.GetValue(item.Collectible.Item.GetReader()) is SC5738080 gearSet)
                {
                    if (gearSet.ItemList.Count != 0)
                        item.Collectible.Item = Investment.GetInventoryItem(gearSet.ItemList.First().ItemIndex);
                }

                if (item.Collectible.Item.ArtArrangementIndex != -1)
                {
                    EntityView.ExportInventoryItem(item.Collectible.Item, ConfigSubsystem.Get().GetExportSavePath());
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
                    Investment.ExportShader(item.Collectible.Item, savePath, itemName, config.GetOutputTextureFormat());
                }
            });
            MainWindow.Progress.CompleteStage();
        }
    }

    // Essentially DestinyRecordDefinition
    public class CategoryEntry : CharmUIElement
    {
        public APIPlugItem Collectible { get; set; } // only used on Collectible

        public int ItemIndex { get; set; }
        public uint ItemHash { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public string ItemDescription { get; set; }

        public ImageSource ItemIcon { get; set; }
        public ImageSource ItemIcon2 { get; set; }

        public int IntervalIndex { get; set; }
        public List<int> Objectives { get; set; } = new();
        public List<int> IntervalObjectives { get; set; } = new();

        public List<CategoryEntry> Rewards { get; set; } = new();
        public List<CategoryEntry> IntervalRewards { get; set; } = new();

        //public List<Category> Parents { get; set; } // Probably not needed
        public List<CategoryEntry> Children { get; set; } = new(); // Used for collectible sets

        public bool RewardOnComplete { get; set; } = false;

        public CategoryEntryType EntryType { get; set; }
    }

    public enum CategoryEntryType
    {
        Record,
        Collectible,
        CollectibleSet
    }

    public ItemPage _subcategoryItems => SubcategoryItems;
}

public class CategoryEntryTemplateSelector : DataTemplateSelector
{
    public DataTemplate RecordTemplate { get; set; }
    public DataTemplate CollectibleTemplate { get; set; }
    public DataTemplate CollectibleSetTemplate { get; set; }
    public DataTemplate PlaceholderTemplate { get; set; } = (DataTemplate)Application.Current.FindResource("PlaceholderTemplate");

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is CategoryEntry entry)
        {
            switch (entry.EntryType)
            {
                case CategoryEntryType.Record:
                    return RecordTemplate;
                case CategoryEntryType.Collectible:
                    return CollectibleTemplate;
                case CategoryEntryType.CollectibleSet:
                    return CollectibleSetTemplate;
                default:
                    return PlaceholderTemplate;
            }
        }

        return PlaceholderTemplate;
        //return base.SelectTemplate(item, container);
    }
}
