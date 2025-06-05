using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tiger;
using Tiger.Schema.Investment;

namespace Charm;

public partial class CollectionsView : UserControl
{
    private static MainWindow _mainWindow = null;

    public Tag<SD7788080> PresentationNodes = Investment.Get()._presentationNodeDefinitionMap;
    public Tag<S03588080> PresentationNodeStrings = Investment.Get()._presentationNodeDefinitionStringMap;

    public CollectionsView()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        MouseMove += UserControl_MouseMove;
    }

    public void LoadContent()
    {
        LoadCollectibles();
    }

    public void LoadCollectibles()
    {
        List<Category> _buttons = new();

        int totalItemAmount = 0;
        var nodes = PresentationNodes.TagData.PresentationNodeDefinitions;
        var strings = PresentationNodeStrings.TagData.PresentationNodeDefinitionStrings;

        var presNodes = nodes.Find(x => x.Hash.Hash32 == 3790247699).PresentationNodes;

        foreach (var node in presNodes)
        {
            var curNode = nodes[node.PresentationNodeIndex];
            var curNodeStrings = strings[node.PresentationNodeIndex];

            Category itemCategory = new()
            {
                ItemCategoryIndex = node.PresentationNodeIndex,
                ItemCategoryIcon = ApiImageUtils.MakeIcon(new FileHash(0x80E64A0B)), // inactive
                Tag = ApiImageUtils.MakeIcon(new FileHash(0x80E64A0D)), // active
                ItemCategoryIcon2 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 0, 1),
                ItemCategoryIcon3 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 0, 0),
                ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                ItemCategoryDescription = curNodeStrings.Description.Value,
                ItemCategoryAmount = GetItemCategoryAmount(node.PresentationNodeIndex),
                CategoryBannerColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x93, 0x82, 0x4F)),
                ScreenStyle = curNodeStrings.ScreenStyle,
                DisplayStyle = curNodeStrings.DisplayStyle,
            };
            totalItemAmount += itemCategory.ItemCategoryAmount;

            _buttons.Add(itemCategory);
        }

        ItemsTextTab.Text = UIHelper.AddSpacesBetweenChars($"ITEMS - {totalItemAmount}", 2);
        CollectiblesList.Items = _buttons;

        DataContext = this;
    }

    private void Category_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Category item = ((Button)sender).DataContext as Category;
        UserControl userControl = new CategoryView(item);

        _mainWindow.MakeNewTab(item.ItemCategoryName, userControl);
        _mainWindow.SetNewestTabSelected();
    }

    public int GetItemCategoryAmount(int index)
    {
        SDB788080 node = PresentationNodes.TagData.PresentationNodeDefinitions[index];
        int count = node.Collectibles.Count;

        for (int j = 0; j < node.PresentationNodes.Count; j++)
        {
            count += GetItemCategoryAmount(node.PresentationNodes[j].PresentationNodeIndex);
        }

        return count;
    }

    private void ItemCategory_MouseEnter(object sender, RoutedEventArgs e)
    {
    }

    public void ItemCategory_MouseLeave(object sender, MouseEventArgs e)
    {
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        float x = -12f / (float)MainWindow.Current.ActualWidth;
        float y = -12f / (float)MainWindow.Current.ActualHeight;
        Point position = e.GetPosition(this);

        TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        gridTransform.X = (int)Math.Round(position.X * x);
        gridTransform.Y = (int)Math.Round(position.Y * y);
    }

    // Essentially DestinyPresentationNodeDefinition
    public class Category : CharmUIElement
    {
        public uint ItemCategoryHash;
        public int ItemCategoryIndex;

        public string ItemCategoryName { get; set; }
        public string ItemCategoryType { get; set; }
        public string ItemCategoryDescription { get; set; }

        public string ItemCategoryLabel1 { get; set; }
        public string ItemCategoryLabel2 { get; set; }
        public string ItemCategoryLabel3 { get; set; }

        public DestinyPresentationScreenStyle ScreenStyle { get; set; }
        public DestinyPresentationDisplayStyle DisplayStyle { get; set; }

        public ImageSource ItemCategoryIcon { get; set; }
        public ImageSource ItemCategoryIcon2 { get; set; }
        public ImageSource ItemCategoryIcon3 { get; set; }
        public ImageSource ItemCategoryIcon4 { get; set; }
        public ImageSource ItemCategoryBackground { get; set; }

        public SolidColorBrush CategoryBannerColor { get; set; } = new SolidColorBrush(Color.FromArgb(0xFF, 0x72, 0xB6, 0xB1));

        public int ItemCategoryAmount { get; set; }
        public int Order;
    }
}

