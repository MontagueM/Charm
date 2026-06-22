using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Tiger;
using Tiger.Schema.Investment;
using static Charm.CollectionsView;

namespace Charm.Collections;

// I'm not really proud of how messy this is....
public partial class LoreView : UserControl
{
    private static MainWindow _mainWindow = null;

    private DynamicArray<S808078DB> PresentationNodes = Investment.Get()._presentationNodeDefinitionMap.TagData.PresentationNodeDefinitions;
    private DynamicArray<S80805807> PresentationNodeStrings = Investment.Get()._presentationNodeDefinitionStringMap.TagData.PresentationNodeDefinitionStrings;

    private DynamicArray<S80806FC1> Records = Investment.Get()._recordNodeDefinitionMap.TagData.RecordDefinitions;
    private DynamicArray<S8080588B> RecordStrings = Investment.Get()._recordNodeDefinitionStringMap.TagData.RecordDefinitionStrings;

    public LoreView(Category itemCategory)
    {
        InitializeComponent();
        DataContext = itemCategory;
        LoadCategories(itemCategory);

        LoreBookView.LoreBookEntries.OnAfterPageChange += (s, item) =>
        {
            AddTooltips();
        };

        LoreBooks.OnBeforePageChange += (s, item) =>
        {
            UIHelper.UnselectAllRadioButtons(item);
        };
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        LoreBooks.ItemTemplate = (DataTemplate)FindResource("LoreItemTemplate");

        _mainWindow = Window.GetWindow(this) as MainWindow;
        MouseMove += UserControl_MouseMove;
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
                ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                ItemCategoryDescription = curNodeStrings.Description.Value,
                Index = i,
            };
            items.Add(subcategory);
        }
        Categories.ItemsSource = items;

        UIHelper.SelectRadioButton(Categories, 0);
    }

    private void Category_OnSelect(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if ((sender as RadioButton) is null)
                return;

            ConcurrentBag<Category> _buttons = new();

            Category item = ((RadioButton)sender).DataContext as Category;

            for (int i = 0; i < PresentationNodes[item.ItemCategoryIndex].PresentationNodes.Count; i++)
            {
                S808078ED node = PresentationNodes[item.ItemCategoryIndex].PresentationNodes[i];
                S808078DB curNode = PresentationNodes[node.PresentationNodeIndex];
                S80805807 curNodeStrings = PresentationNodeStrings[node.PresentationNodeIndex];

                Category subcategory = new()
                {
                    ItemCategoryIndex = node.PresentationNodeIndex,
                    ItemCategoryIcon = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 0, 0),
                    ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                    Index = i,
                };

                _buttons.Add(subcategory);
            }

            // Game sorts oldest to newest, for some reason
            LoreBooks.Items = _buttons.Reverse();
            LoreBooks.DisplayItems(true);

            UIHelper.SelectRadioButton(LoreBooks._ItemList, 0);

            SubcategoryType.Text = item.ItemCategoryName;
            AnimateTextBlock();
        }), DispatcherPriority.Background);
    }

    private async void Subcategory_OnSelect(object sender, RoutedEventArgs e)
    {
        await Dispatcher.BeginInvoke(new Action(() =>
        {
            if ((sender as RadioButton) is null)
                return;

            Category item = ((RadioButton)sender).DataContext as Category;
            LoreBookView.LoadLoreBook(item.ItemCategoryIndex);

            AddTooltips(); // stupid stupid stupid stupid stupid

        }), DispatcherPriority.Background);
    }

    private void AddTooltips()
    {
        // Adds tooltips to the lore book entry (pages) buttons here instead of adding it directly to the user control
        // its stupid but it works
        Dispatcher.InvokeAsync(() =>
        {
            foreach (var item in LoreBookView.LoreBookEntries.Items)
            {
                var container = LoreBookView.LoreBookEntries._ItemList.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null)
                {
                    var radioButton = UIHelper.FindVisualChild<RadioButton>(container);
                    if (radioButton != null)
                    {
                        radioButton.MouseEnter += LoreEntry_MouseEnter;
                        radioButton.MouseLeave += PlugItem_MouseLeave;
                    }
                }
            }
        }, DispatcherPriority.Render);
    }

    private void AnimateTextBlock()
    {
        Storyboard textChangeAnimation = (Storyboard)FindResource("TextChangeAnimation");
        textChangeAnimation.Begin(SubcategoryType);
    }

    private void CategoryButton_MouseEnter(object sender, MouseEventArgs e)
    {
    }

    private void LoreEntry_MouseEnter(object sender, MouseEventArgs e)
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
