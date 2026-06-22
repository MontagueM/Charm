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
using System.Windows.Threading;
using Tiger;
using Tiger.Schema.Investment;
using static Charm.CategoryView;
using static Charm.CollectionsView;

namespace Charm.Collections;

// TODO? Combine into CategoryView
public partial class BadgeView : UserControl
{
    private static MainWindow _mainWindow = null;

    private DynamicArray<S808078DB> PresentationNodes = Investment.Get()._presentationNodeDefinitionMap.TagData.PresentationNodeDefinitions;
    private DynamicArray<S80805807> PresentationNodeStrings = Investment.Get()._presentationNodeDefinitionStringMap.TagData.PresentationNodeDefinitionStrings;

    private DynamicArray<S80806FC1> Records = Investment.Get()._recordNodeDefinitionMap.TagData.RecordDefinitions;
    private DynamicArray<S8080588B> RecordStrings = Investment.Get()._recordNodeDefinitionStringMap.TagData.RecordDefinitionStrings;

    public BadgeView(Category itemCategory)
    {
#if DEBUG
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif

        InitializeComponent();

        if (itemCategory.DisplayStyle == DestinyPresentationDisplayStyle.Medals)
        {
            LoadItems(itemCategory.ItemCategoryIndex);
            itemCategory.CategoryBannerColor = null;
            BadgeProgress.Visibility = Visibility.Collapsed;
            TitleProgress.Visibility = Visibility.Visible;
            TitleName.Text = RecordStrings[PresentationNodes[itemCategory.ItemCategoryIndex].CompletionRecordIndex].TitleName.Value;
        }
        else
        {
            LoadCategories(itemCategory);
            BadgeProgress.Visibility = Visibility.Visible;
            TitleProgress.Visibility = Visibility.Collapsed;
        }

        DataContext = itemCategory;
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        SubcategoryItems.ItemTemplate = (DataTemplate)FindResource("CategoryEntryItemTemplate");

        _mainWindow = Window.GetWindow(this) as MainWindow;
        MouseMove += UserControl_MouseMove;
        PreviewKeyDown += Button_KeyDown;

        BadgeCornerDecor.Source = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C2409D : 0x80E6470E));
        UIHelper.AnimateFade(CornerBanner, 0.25f);
    }

    public void LoadCategories(Category itemCategory)
    {
        List<Category> items = new();

        for (int i = 0; i < PresentationNodes[itemCategory.ItemCategoryIndex].PresentationNodes.Count; i++)
        {
            S808078ED node = PresentationNodes[itemCategory.ItemCategoryIndex].PresentationNodes[i];
            S808078DB curNode = PresentationNodes[node.PresentationNodeIndex];
            S80805807 curNodeStrings = PresentationNodeStrings[node.PresentationNodeIndex];

            Category subcategory = new()
            {
                ItemCategoryIndex = node.PresentationNodeIndex,
                ItemCategoryIcon = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex),
                ItemCategoryName = curNodeStrings.Name.Value.ToString(),
                ItemCategoryDescription = curNodeStrings.Description.Value?.ToString() ?? "",
                Tag = curNode.Collectibles.Count,
                Index = i,
            };
            items.Add(subcategory);
        }
        CategoryDetails.ItemsSource = items;
        Categories.ItemsSource = items;
        UIHelper.SelectRadioButton(Categories, 0);

        Dispatcher.InvokeAsync(() =>
        {
            if (items.Count <= 1)
                Categories.Visibility = Visibility.Collapsed;
        }, DispatcherPriority.Background);
    }

    private async void Subcategory_OnSelect(object sender, RoutedEventArgs e)
    {
        await Dispatcher.BeginInvoke(new Action(() =>
        {
            if ((sender as RadioButton) is null)
                return;

            Category item = ((RadioButton)sender).DataContext as Category;

            LoadItems(item.ItemCategoryIndex);
        }), DispatcherPriority.Background);
    }

    private void LoadItems(int index)
    {
        List<CategoryEntry> items = new();

        // Collectibles
        foreach (var collectible in PresentationNodes[index].Collectibles)
        {
            var item = Investment.Get().GetCollectible(collectible.CollectableIndex).Value;
            var strings = Investment.Get().GetCollectibleStrings(collectible.CollectableIndex).Value;

            var invItem = Investment.Get().GetInventoryItem(item.InventoryItemIndex);

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
                Index = collectible.Unk00,
                EntryType = CategoryEntryType.Collectible
            };

            if (!items.Any(x => x.ItemHash == subcategory.ItemHash))
                items.Add(subcategory);

            SubcategoryItems.Columns = 3;
            SubcategoryItems.ItemsPerPage = 21;
        }

        // Records
        foreach (var record in PresentationNodes[index].Records)
        {
            var strings = RecordStrings[record.RecordDefinitionIndex];

            // Good lord this sucks but I can't figure out how a record gets its "Triumph Completed Rewards" flag
            // Sooo either check if interval rewards is more than 1 and only the last one actually gives something
            // or check if all the interval rewards are the exact same
            var nonEmpty = strings.IntervalRewardItems
                          .Select((item, index) => new { item, index })
                          .Where(x => x.item.Rewards.Count != 0)
                          .ToList();

            bool onlyLastIsValid = strings.IntervalRewardItems.Count > 1
            && nonEmpty.Count == 1
            && nonEmpty[0].index == strings.IntervalRewardItems.Count - 1;

            var nonEmptyRewards = strings.IntervalRewardItems
                                .Where(x => x.Rewards.Count != 0)
                                .Select(x => x.Rewards)
                                .ToList();

            bool allSame = strings.IntervalRewardItems.Count > 1
                && nonEmptyRewards.Count == strings.IntervalRewardItems.Count
                && nonEmptyRewards
                .Skip(1)
                .All(r => r.SequenceEqual(nonEmptyRewards[0]));

            CategoryEntry subcategory = new()
            {
                ItemIndex = record.RecordDefinitionIndex,
                ItemIcon = strings.IconIndex != -1 ? ApiImageUtils.MakeIcon(strings.IconIndex, 0, 0, 0) : null,
                ItemName = strings.Name.Value?.ToString() ?? "",
                ItemType = strings.RecordTypeName.Value?.ToString() ?? "",
                ItemDescription = strings.Description.Value?.ToString() ?? "",
                EntryType = CategoryEntryType.Record,
                RewardOnComplete = onlyLastIsValid || allSame
            };

            foreach (var objectives in Records[record.RecordDefinitionIndex].Objectives)
            {
                subcategory.Objectives.Add(objectives.ObjectiveIndex);
            }

            foreach (var objectives in Records[record.RecordDefinitionIndex].IntervalObjectives)
            {
                subcategory.IntervalObjectives.Add(objectives.ObjectiveIndex);
            }

            for (int i = 0; i < strings.RewardItems.Count; i++)
            {
                var reward = strings.RewardItems[i];
                var item = Investment.Get().GetInventoryItem(reward.ItemIndex);

                if (strings.ShowLargeIcons && i == 0)
                    subcategory.ItemIcon2 = ApiImageUtils.MakeFullItemIcon(item);

                subcategory.Rewards.Add(new()
                {
                    Collectible = new(item),
                    ItemIndex = reward.ItemIndex,
                    ItemName = item.Name,
                    ItemIcon = ApiImageUtils.MakeFullItemIcon(item),
                    ItemIcon2 = item.Type == "Emblem" ? ApiImageUtils.MakeIcon(item.GetItemStrings().TagData.EmblemContainerIndex) : null,
                    IsPlaceholder = strings.ShowLargeIcons && i == 0
                });
            }

            for (int i = 0; i < strings.IntervalRewardItems.Count; i++)
            {
                var intervalRewards = strings.IntervalRewardItems[(strings.IntervalRewardItems.Count - 1) - i];
                foreach (var reward in intervalRewards.Rewards)
                {
                    if (subcategory.IntervalRewards.Any(x => x.ItemIndex == reward.ItemIndex))
                        continue;

                    var item = Investment.Get().GetInventoryItem(reward.ItemIndex);
                    subcategory.IntervalRewards.Add(new()
                    {
                        IntervalIndex = i,
                        Collectible = new(item),
                        ItemIndex = reward.ItemIndex,
                        ItemName = item.Name,
                        ItemIcon = ApiImageUtils.MakeFullItemIcon(item),
                        ItemIcon2 = item.Type == "Emblem" ? ApiImageUtils.MakeIcon(item.GetItemStrings().TagData.EmblemContainerIndex) : null,
                    });
                }
            }


            items.Add(subcategory);

            SubcategoryItems.Columns = 2;
            SubcategoryItems.ItemsPerPage = 10;
        }

        // Gotta do this here since entries are actually sorted by index in-game?
        var sortedItems = items.OrderBy(x => x.Index).ToList();
        for (int i = 0; i < sortedItems.Count; i++)
        {
            sortedItems[i].Index = i + 1;
        }

        SubcategoryItems.Items = sortedItems;
        SubcategoryItems.DisplayItems(true);

        UIHelper.AnimateFade(SubcategoryItems._ItemList, 0.1f, 1f, 0.5f);
    }

    public int GetItemCategoryAmount(int index)
    {
        S808078DB node = PresentationNodes[index];
        int count = 0;

        for (int i = 0; i < node.PresentationNodes.Count; i++)
        {
            count += PresentationNodes[node.PresentationNodes[i].PresentationNodeIndex].Records.Count;
        }

        return count;
    }

    private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        CategoryEntry item = (sender as Button).DataContext as CategoryEntry;

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
        var tooltip = MainWindow.Current._ToolTip;
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
                && item.Collectible.Item.TagData.Unk28.GetValue(item.Collectible.Item.GetReader()) is S808073C5 gearSet)
                {
                    if (gearSet.ItemList.Count != 0)
                        item.Collectible.Item = Investment.Get().GetInventoryItem(gearSet.ItemList.First().ItemIndex);
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
                    Investment.Get().ExportShader(item.Collectible.Item, savePath, itemName, config.GetOutputTextureFormat());
                }
            });
            MainWindow.Progress.CompleteStage();
        }
    }
}
