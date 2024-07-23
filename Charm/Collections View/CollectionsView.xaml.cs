﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tiger;
using Tiger.Schema.Investment;

namespace Charm;

public partial class CollectionsView : UserControl
{
    private static MainWindow _mainWindow = null;

    public Tag<D2Class_D7788080> PresentationNodes = Investment.Get()._presentationNodeDefinitionMap;
    public Tag<D2Class_03588080> PresentationNodeStrings = Investment.Get()._presentationNodeDefinitionStringMap;
    public int TotalItemAmount { get; set; }

    public CollectionsView()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        MouseMove += UserControl_MouseMove;
    }

    public async void LoadContent()
    {
        LoadMainItemCategory();
    }

    public void LoadMainItemCategory(int i = 0, int depth = 0)
    {
        var nodes = PresentationNodes.TagData.PresentationNodeDefinitions;
        var strings = PresentationNodeStrings.TagData.PresentationNodeDefinitionStrings;

        foreach (var node in nodes[i].PresentationNodes)
        {
            var curNode = nodes[node.PresentationNodeIndex];
            var curNodeStrings = strings[node.PresentationNodeIndex];

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

            Button btn = new Button
            {
                DataContext = itemCategory,
                Style = (Style)FindResource("MainItemsButton")
            };

            MainItemsGrid.Children.Add(btn);
        }
        DataContext = this;
    }

    public int GetItemCategoryAmount(int index)
    {
        var node = PresentationNodes.TagData.PresentationNodeDefinitions[index];
        int count = node.Collectables.Count;

        for (int j = 0; j < node.PresentationNodes.Count; j++)
        {
            count += GetItemCategoryAmount(node.PresentationNodes[j].PresentationNodeIndex);
        }

        return count;
    }

    private void ItemCategory_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ItemCategory item = ((Button)sender).DataContext as ItemCategory;

        CategoryView categoryView = new(item);
        _mainWindow.MakeNewTab(item.ItemCategoryName, categoryView);
        _mainWindow.SetNewestTabSelected();
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        System.Windows.Point position = e.GetPosition(this);
        TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        gridTransform.X = position.X * -0.01;
        gridTransform.Y = position.Y * -0.01;
    }

    public class ItemCategory
    {
        public int ItemCategoryIndex;
        public ImageSource ItemCategoryIcon { get; set; }
        public ImageSource ItemCategoryIcon2 { get; set; }
        public string ItemCategoryName { get; set; }
        public string ItemCategoryDescription { get; set; }
        public int ItemCategoryAmount { get; set; }
    }
}
