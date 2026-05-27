using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tiger;
using Tiger.Schema.Investment;
using static Charm.CollectionsView;

namespace Charm.Collections;

public partial class TriumphView : UserControl
{
    private static MainWindow _mainWindow = null;

    private DynamicArray<SDB788080> PresentationNodes = Investment.Get()._presentationNodeDefinitionMap.TagData.PresentationNodeDefinitions;
    private DynamicArray<S07588080> PresentationNodeStrings = Investment.Get()._presentationNodeDefinitionStringMap.TagData.PresentationNodeDefinitionStrings;

    private DynamicArray<SC16F8080> Records = Investment.Get()._recordNodeDefinitionMap.TagData.RecordDefinitions;
    private DynamicArray<S8B588080> RecordStrings = Investment.Get()._recordNodeDefinitionStringMap.TagData.RecordDefinitionStrings;

    private DestinyPresentationDisplayStyle DisplayStyle;

    public TriumphView(Category itemCategory)
    {
        InitializeComponent();
        DisplayStyle = itemCategory.DisplayStyle;

        // dumb but screw it
        itemCategory.ItemCategoryLabel1 = PresentationNodes[itemCategory.ItemCategoryIndex].MaxCategoryRecordScore.ToString("#,##0");

        DataContext = itemCategory;
        LoadTriumphs(itemCategory);
        LoadLegacyTriumphs(itemCategory);

        // also dumb
        if (DisplayStyle == DestinyPresentationDisplayStyle.Medals)
        {
            HeaderIcon.MaxWidth = 70;
            HeaderIcon.Margin = new(0, 0, 10, 0);
            Triumphs.ItemMargin = new(0, 0, 0, 10);
            Triumphs.PageIndicatorMargin = new(0, 0, 0, -5);
            TriumphsTextBlock.Text = UIHelper.AddSpacesBetweenChars("TITLES", 1);

            LegacyTriumphs.ItemMargin = new(0);
            LegacyTriumphs.PageIndicatorMargin = new(0, 0, 0, -20);
            LegacyTextBlock.Text = UIHelper.AddSpacesBetweenChars("LEGACY TITLES", 1);
        }
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        Triumphs.ItemTemplate = (DataTemplate)FindResource("TriumphItemTemplate");
        LegacyTriumphs.ItemTemplate = (DataTemplate)FindResource("LegacyTriumphItemTemplate");

        _mainWindow = Window.GetWindow(this) as MainWindow;
        MouseMove += UserControl_MouseMove;
    }

    public void LoadTriumphs(Category itemCategory)
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
                ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                ItemCategoryDescription = curNodeStrings.Description.Value.ToString(),
                ItemCategoryAmount = curNode.MaxCategoryRecordScore,
                DisplayStyle = curNodeStrings.DisplayStyle,
                ScreenStyle = curNodeStrings.ScreenStyle,
                Index = i,
            };

            // The Undertaker title from Renegades has a DisplayStyle of Category even though it should be Medals (bungie made an oopsie?)
            if (subcategory.DisplayStyle != DisplayStyle)
                subcategory.DisplayStyle = DisplayStyle;

            // Icon used as inactive icon
            // Icon2 used in Category/Badge view
            // Icon3 used as inner icon (triumphs)
            // Icon4 used as active icon
            if (subcategory.DisplayStyle == DestinyPresentationDisplayStyle.Medals)
            {
                subcategory.ItemCategoryIcon = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 0, 0);
                subcategory.ItemCategoryIcon2 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 2, 0);
                subcategory.ItemCategoryIcon4 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 1, 0);
                subcategory.ItemCategoryAmount = Investment.Get().GetObjectiveValue(Records[PresentationNodes[subcategory.ItemCategoryIndex].CompletionRecordIndex].Objectives.FirstOrDefault().ObjectiveIndex);
            }
            else
            {
                subcategory.ItemCategoryIcon = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C232C3 : 0x80E64A4E));
                subcategory.ItemCategoryIcon2 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 0, 1);
                subcategory.ItemCategoryIcon3 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex);
                subcategory.ItemCategoryIcon4 = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C232C5 : 0x80E64A50));
            }

            items.Add(subcategory);
        }
        Triumphs.Items = items;
    }

    public void LoadLegacyTriumphs(Category itemCategory)
    {
        List<Category> items = new();
        if (itemCategory.DisplayStyle == DestinyPresentationDisplayStyle.Category)
        {
            var legacyTriumphNodes = PresentationNodes.Find(x => x.Hash.Hash32 == 3215903653).PresentationNodes;
            foreach (var legacyTriumph in legacyTriumphNodes)
            {
                SDB788080 curNode = PresentationNodes[legacyTriumph.PresentationNodeIndex];
                S07588080 curNodeStrings = PresentationNodeStrings[legacyTriumph.PresentationNodeIndex];
                Category subcategory = new()
                {
                    ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                    ItemCategoryDescription = curNodeStrings.Description.Value.ToString(),
                    ItemCategoryIndex = legacyTriumph.PresentationNodeIndex,
                    ItemCategoryIcon = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C232C3 : 0x80E64A4E)),
                    ItemCategoryIcon2 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 0, 1),
                    ItemCategoryIcon3 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex),
                    ItemCategoryAmount = curNode.MaxCategoryRecordScore,

                    Tag = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C232C5 : 0x80E64A50)),
                    DisplayStyle = curNodeStrings.DisplayStyle,
                    ScreenStyle = curNodeStrings.ScreenStyle,
                };
                items.Add(subcategory);
            }
        }
        else
        {
            var legacyTitleNodes = PresentationNodes.Find(x => x.Hash.Hash32 == 1881970629).PresentationNodes;
            foreach (var legacyTitle in legacyTitleNodes)
            {
                SDB788080 curNode = PresentationNodes[legacyTitle.PresentationNodeIndex];
                S07588080 curNodeStrings = PresentationNodeStrings[legacyTitle.PresentationNodeIndex];
                Category subcategory = new()
                {
                    ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                    ItemCategoryDescription = curNodeStrings.Description.Value.ToString(),
                    ItemCategoryIndex = legacyTitle.PresentationNodeIndex,
                    ItemCategoryIcon = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 0, 1),
                    ItemCategoryIcon2 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 2, 1),
                    ItemCategoryIcon4 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 1, 1),
                    ItemCategoryAmount = Investment.Get().GetObjectiveValue(Records[PresentationNodes[legacyTitle.PresentationNodeIndex].CompletionRecordIndex].Objectives.FirstOrDefault().ObjectiveIndex),

                    DisplayStyle = curNodeStrings.DisplayStyle,
                    ScreenStyle = curNodeStrings.ScreenStyle,
                };
                items.Add(subcategory);
            }
        }

        LegacyTriumphs.Items = items;
    }

    private void Triumph_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Category item = ((Button)sender).DataContext as Category;
        UserControl userControl = item.DisplayStyle == DestinyPresentationDisplayStyle.Medals ? new BadgeView(item) : new CategoryView(item);

        _mainWindow.MakeNewTab(item.ItemCategoryName, userControl);
        _mainWindow.SetNewestTabSelected();
    }

    public int GetItemCategoryAmount(int index)
    {
        SDB788080 node = PresentationNodes[index];
        int count = 0;

        for (int i = 0; i < node.PresentationNodes.Count; i++)
        {
            count += PresentationNodes[node.PresentationNodes[i].PresentationNodeIndex].Records.Count;
        }

        return count;
    }

    private void CategoryButton_MouseEnter(object sender, MouseEventArgs e)
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
}
