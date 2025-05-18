using System.Collections.Concurrent;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tiger;
using Tiger.Schema.Investment;
using static Charm.APIItemView;

namespace Charm;

public partial class CollectionsView : UserControl
{
    private static MainWindow _mainWindow = null;

    public Tag<SD7788080> PresentationNodes = Investment.Get()._presentationNodeDefinitionMap;
    public Tag<S03588080> PresentationNodeStrings = Investment.Get()._presentationNodeDefinitionStringMap;
    public int TotalItemAmount { get; set; }
    private APITooltip ToolTip;

    public CollectionsView()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        MouseMove += UserControl_MouseMove;

        ToolTip = new();
        Panel.SetZIndex(ToolTip, 50);
        MainContainer.Children.Add(ToolTip);
    }

    public void LoadContent()
    {
        LoadMainItemCategory();
        LoadBadges();
    }

    // Badges -> hash 498211331
    public void LoadMainItemCategory(int i = 0)
    {
        DynamicArray<SDB788080> nodes = PresentationNodes.TagData.PresentationNodeDefinitions;
        DynamicArray<S07588080> strings = PresentationNodeStrings.TagData.PresentationNodeDefinitionStrings;

        foreach (SED788080 node in nodes[0].PresentationNodes)
        {
            SDB788080 curNode = nodes[node.PresentationNodeIndex];
            S07588080 curNodeStrings = strings[node.PresentationNodeIndex];

            ItemCategory itemCategory = new()
            {
                ItemCategoryIndex = node.PresentationNodeIndex,
                ItemCategoryIcon = ApiImageUtils.MakeFullIcon(curNodeStrings.IconIndex),
                ItemCategoryIcon2 = ApiImageUtils.MakeFullIcon(curNodeStrings.IconIndex, 0, 0, 1),
                ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                ItemCategoryDescription = curNodeStrings.Description.Value,
                ItemCategoryAmount = GetItemCategoryAmount(node.PresentationNodeIndex)
            };
            TotalItemAmount += itemCategory.ItemCategoryAmount;

            Button btn = new()
            {
                DataContext = itemCategory,
                Style = (Style)FindResource("MainItemsButton")
            };

            MainItemsGrid.Children.Add(btn);
        }
        DataContext = this;
    }

    // Badges -> hash 498211331
    public void LoadBadges()
    {
        ConcurrentBag<Button> _buttons = new();

        int totalItemAmount = 0;
        var nodes = PresentationNodes.TagData.PresentationNodeDefinitions;
        var strings = PresentationNodeStrings.TagData.PresentationNodeDefinitionStrings;

        var presNodes = nodes.Find(x => x.Hash.Hash32 == 498211331).PresentationNodes;

        foreach (var node in presNodes)
        {
            var curNode = nodes[node.PresentationNodeIndex];
            var curNodeStrings = strings[node.PresentationNodeIndex];

            ItemCategory itemCategory = new()
            {
                ItemCategoryIndex = node.PresentationNodeIndex,
                ItemCategoryIcon = ApiImageUtils.MakeFullIcon(curNodeStrings.IconIndex),
                ItemCategoryIcon2 = ApiImageUtils.MakeFullIcon(curNodeStrings.IconIndex, 0, 2),
                ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                ItemCategoryDescription = curNodeStrings.Description.Value,
                ItemCategoryAmount = GetItemCategoryAmount(node.PresentationNodeIndex),
                Order = totalItemAmount
            };
            totalItemAmount++;

            _buttons.Add(new Button
            {
                DataContext = itemCategory,
                Style = (Style)FindResource("BadgeItemButton")
            });
        }
        BadgesTextTab.Text = $"BADGES - {totalItemAmount}";
        BadgesList.ItemsPerPage = 9;
        BadgesList.Columns = 3;

        // Dumb but need to reverse since its like that in game
        BadgesList.Buttons = _buttons.OrderBy(x => (x.DataContext as ItemCategory).Order).ToList();
    }

    public int GetItemCategoryAmount(int index)
    {
        SDB788080 node = PresentationNodes.TagData.PresentationNodeDefinitions[index];
        int count = node.Collectables.Count;

        for (int j = 0; j < node.PresentationNodes.Count; j++)
        {
            count += GetItemCategoryAmount(node.PresentationNodes[j].PresentationNodeIndex);
        }

        return count;
    }

    private void BadgeCategory_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ItemCategory item = ((Button)sender).DataContext as ItemCategory;

        BadgeView categoryView = new(item);
        _mainWindow.MakeNewTab(item.ItemCategoryName, categoryView);
        _mainWindow.SetNewestTabSelected();
    }

    private void ItemCategory_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ItemCategory item = ((Button)sender).DataContext as ItemCategory;

        CategoryView categoryView = new(item);
        _mainWindow.MakeNewTab(item.ItemCategoryName, categoryView);
        _mainWindow.SetNewestTabSelected();
    }

    private void ItemCategory_MouseEnter(object sender, RoutedEventArgs e)
    {
        ToolTip.ActiveItem = (sender as Button);
        ItemCategory item = ((Button)sender).DataContext as ItemCategory;

        PlugItem plugItem = new()
        {
            Name = $"{item.ItemCategoryName}",
            Description = $"{item.ItemCategoryDescription}",
            PlugStyle = DestinySocketCategoryStyle.Reusable
        };

        ToolTip.MakeTooltip(plugItem);
    }

    public void ItemCategory_MouseLeave(object sender, MouseEventArgs e)
    {
        ToolTip.ClearTooltip();
        ToolTip.ActiveItem = null;
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        System.Windows.Point position = e.GetPosition(this);
        TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        gridTransform.X = position.X * -0.0075;
        gridTransform.Y = position.Y * -0.0075;
    }

    public class ItemCategory
    {
        public int ItemCategoryIndex;
        public ImageSource ItemCategoryIcon { get; set; }
        public ImageSource ItemCategoryIcon2 { get; set; }
        public string ItemCategoryName { get; set; }
        public string ItemCategoryDescription { get; set; }
        public int ItemCategoryAmount { get; set; }
        public int Order;
    }
}

