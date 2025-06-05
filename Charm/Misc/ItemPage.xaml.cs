using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Charm;

public partial class ItemPage : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty ItemsProperty =
    DependencyProperty.Register(
        nameof(Items),
        typeof(IEnumerable<CharmUIElement>),
        typeof(ItemPage),
        new PropertyMetadata(Enumerable.Empty<CharmUIElement>(), OnItemsChanged));

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ItemPage)d;
        var newItems = e.NewValue as IEnumerable<CharmUIElement>;

        control.Items = newItems;
        if (control.Items != null)
            control.DisplayItems(true);
    }

    public IEnumerable<CharmUIElement> Items
    {
        get => (IEnumerable<CharmUIElement>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    #region Backing Fields
    private int _itemsPerPage = 3;
    private int _columns = 3;
    private bool _isVertical = false;
    private bool _usePlaceholders = false;
    private bool _collapsePageButtons = true;
    private bool _hidePageButtons = false;
    private bool _expand = false;
    private Thickness _itemMargin = new(0, 0, 0, 0);
    private Thickness _pageIndicatorMargin = new(0, 0, 0, -20);
    private int _currentPage = 0;
    private int _totalPages = 1;
    private bool _useStackPanel = false;
    private bool _selectOnPageChange = true;
    private float _slideDistance = 3.0f;
    #endregion

    public int ItemsPerPage
    {
        get => _itemsPerPage;
        set
        {
            if (_itemsPerPage != value)
            {
                _itemsPerPage = value;
                OnPropertyChanged();
            }
        }
    }

    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        set
        {
            if (_totalPages != value)
            {
                _totalPages = value;
                OnPropertyChanged();
            }
        }
    }

    public int Columns
    {
        get => _columns;
        set
        {
            if (_columns != value)
            {
                _columns = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsVertical
    {
        get => _isVertical;
        set
        {
            if (_isVertical != value)
            {
                _isVertical = value;
                OnPropertyChanged();
            }
        }
    }

    public bool UsePlaceholders
    {
        get => _usePlaceholders;
        set
        {
            if (_usePlaceholders != value)
            {
                _usePlaceholders = value;
                OnPropertyChanged();
            }
        }
    }

    public Thickness ItemMargin
    {
        get => _itemMargin;
        set
        {
            if (_itemMargin != value)
            {
                _itemMargin = value;
                OnPropertyChanged();
            }
        }
    }

    public Thickness PageIndicatorMargin
    {
        get => _pageIndicatorMargin;
        set
        {
            if (_pageIndicatorMargin != value)
            {
                _pageIndicatorMargin = value;
                OnPropertyChanged();
            }
        }
    }


    /// <summary>
    /// Collapse the Prev/Next page buttons instead of hiding them
    /// </summary>
    public bool CollapsePageButtons
    {
        get => _collapsePageButtons;
        set
        {
            if (_collapsePageButtons != value)
            {
                _collapsePageButtons = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Hide the default page buttons (aka using custom controls to change pages)
    /// </summary>
    public bool HidePageButtons
    {
        get => _hidePageButtons;
        set
        {
            if (_hidePageButtons != value)
            {
                _hidePageButtons = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Add an additional entry if collapsing page buttons
    /// </summary>
    public bool Expand
    {
        get => _expand;
        set
        {
            if (_expand != value)
            {
                _expand = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Use a StackPanel for items instead of a UniformGrid
    /// </summary>
    public bool UseStackPanel
    {
        get => _useStackPanel;
        set
        {
            if (_useStackPanel != value)
            {
                _useStackPanel = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// How far a page will slide when changing
    /// </summary>
    public float SlideDistance
    {
        get => _slideDistance;
        set
        {
            if (_slideDistance != value)
            {
                _slideDistance = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Select the first element of the Items list on page change (only RadioButtons currently)
    /// </summary>
    public bool SelectOnPageChange
    {
        get => _selectOnPageChange;
        set
        {
            if (_selectOnPageChange != value)
            {
                _selectOnPageChange = value;
                OnPropertyChanged();
            }
        }
    }

    public static readonly DependencyProperty ItemTemplateProperty =
       DependencyProperty.Register(nameof(Template), typeof(DataTemplate), typeof(ItemPage), new PropertyMetadata(null));

    public static readonly DependencyProperty ItemTemplateSelectorProperty =
        DependencyProperty.Register(nameof(ItemTemplateSelector), typeof(DataTemplateSelector), typeof(ItemPage), new PropertyMetadata(null));

    /// <summary>
    /// The DataTemplate to use for the items added to this page
    /// </summary>
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// The DataTemplateSelector to use for the items added to this page, falls back to ItemTemplate if none is given
    /// </summary>
    public DataTemplateSelector ItemTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty);
        set => SetValue(ItemTemplateSelectorProperty, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }


    public event EventHandler<ItemsControl> OnPreviousPageClicked;
    public event EventHandler<ItemsControl> OnNextPageClicked;

    public event EventHandler<ItemsControl> OnBeforePageChange;
    public event EventHandler<ItemsControl> OnAfterPageChange;

    private bool IsAnimating = false;

    public ItemPage()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        DataContext = this;
        DisplayItems();
    }

    public void DisplayItems(bool fromStart = false)
    {
        if (fromStart)
            CurrentPage = 0;
        if (Items is null)
            return;

        int count = Items.Count();

        var itemsPerPage = (count > ItemsPerPage && (Expand && CollapsePageButtons) && ItemsPerPage != 1) ? ItemsPerPage - 1 : ItemsPerPage;

        TotalPages = (int)Math.Ceiling((double)count / itemsPerPage);

        if (!HidePageButtons)
        {
            PreviousPage.Visibility = count > itemsPerPage ? Visibility.Visible : (CollapsePageButtons ? Visibility.Collapsed : Visibility.Hidden);
            NextPage.Visibility = count > itemsPerPage ? Visibility.Visible : (CollapsePageButtons ? Visibility.Collapsed : Visibility.Hidden);
        }

        var itemsToShow = Items.Skip(CurrentPage * itemsPerPage).Take(itemsPerPage).ToList();

        if (UsePlaceholders)
        {
            int placeholderCount = itemsPerPage - itemsToShow.Count;
            for (int i = 0; i < placeholderCount; i++)
            {
                itemsToShow.Add(new()
                {
                    IsPlaceholder = true
                });
            }
        }
        ItemList.ItemsSource = itemsToShow;

        UIHelper.AnimateFade(ItemList, 0.075f, 1f, 0);
        CheckPages();
    }

    /// <summary>
    /// Previous/Next page if using custom controls to change pages
    /// </summary>
    /// <returns></returns>
    public bool SelectNextPage()
    {
        return ChangePage(1,
            beforeChange: null,
            afterChange: () => OnNextPageClicked?.Invoke(this, ItemList),
            completeAction: (s, e2) =>
            {
                OnAfterPageChange?.Invoke(this, ItemList);
                if (SelectOnPageChange)
                    UIHelper.SelectRadioButton(ItemList, 0);
            });
    }

    public bool SelectPreviousPage()
    {
        return ChangePage(-1,
            beforeChange: null,
            afterChange: () => OnPreviousPageClicked?.Invoke(this, ItemList),
            completeAction: (s, e2) =>
            {
                OnAfterPageChange?.Invoke(this, ItemList);
                if (SelectOnPageChange)
                    UIHelper.SelectRadioButton(ItemList, 0);
            });
    }

    private void PreviousPage_Click(object sender, RoutedEventArgs e)
    {
        ChangePage(-1,
            beforeChange: null,
            afterChange: () => OnPreviousPageClicked?.Invoke(this, ItemList),
            completeAction: (s, e2) =>
            {
                OnAfterPageChange?.Invoke(this, ItemList);
                if (SelectOnPageChange)
                    UIHelper.SelectRadioButton(ItemList, 0);
            });
    }

    private void NextPage_Click(object sender, RoutedEventArgs e)
    {
        ChangePage(1,
            beforeChange: null,
            afterChange: () => OnNextPageClicked?.Invoke(this, ItemList),
            completeAction: (s, e2) =>
            {
                OnAfterPageChange?.Invoke(this, ItemList);
                if (SelectOnPageChange)
                    UIHelper.SelectRadioButton(ItemList, 0);
            });
    }

    private bool ChangePage(int direction, Action beforeChange, Action afterChange, EventHandler completeAction)
    {
        if (IsAnimating) return false;

        var itemsPerPage = (Items.Count() > ItemsPerPage && (Expand && CollapsePageButtons) && ItemsPerPage != 1) ? ItemsPerPage - 1 : ItemsPerPage;
        int targetPage = CurrentPage + direction;

        bool canChange = direction < 0
            ? CurrentPage > 0
            : Items.Count() > 0 && targetPage * itemsPerPage < Items.Count();

        if (!canChange) return false;

        IsAnimating = true;
        DisableNavigation();

        beforeChange?.Invoke();
        OnBeforePageChange?.Invoke(this, ItemList);
        if (SelectOnPageChange)
            UIHelper.UnselectAllRadioButtons(ItemList);

        Point entrySlide = direction < 0 ? new(SlideDistance, 0) : new(-SlideDistance, 0);
        Point exitSlide = direction < 0 ? new(-SlideDistance, 0) : new(SlideDistance, 0);

        if (UseStackPanel && IsVertical)
        {
            entrySlide = direction < 0 ? new(0, SlideDistance) : new(0, -SlideDistance);
            exitSlide = direction < 0 ? new(0, -SlideDistance) : new(0, SlideDistance);
        }

        UIHelper.AnimateSlide(ItemList, 0.075f, entrySlide, new(0, 0));

        UIHelper.AnimateFade(ItemList, 0.075f, 0f, 1f, (s, e) =>
        {
            CurrentPage = targetPage;
            DisplayItems();
            afterChange?.Invoke();

            IsAnimating = false;
            EnableNavigation();

            UIHelper.AnimateSlide(ItemList, 0.075f, new(0, 0), exitSlide);

            completeAction?.Invoke(s, e);
        });
        return true;
    }

    public void CheckPages()
    {
        var itemsPerPage = (Items.Count() > ItemsPerPage && (Expand && CollapsePageButtons) && ItemsPerPage != 1) ? ItemsPerPage - 1 : ItemsPerPage;
        if (Items.Count() == 0)
            CurrentPage = 0;

        if (HidePageButtons)
        {
            PreviousPage.Visibility = Visibility.Collapsed;
            NextPage.Visibility = Visibility.Collapsed;
        }

        PreviousPage.IsEnabled = CurrentPage != 0;
        NextPage.IsEnabled = Items.Count() > 0 ? (CurrentPage + 1) * itemsPerPage < Items.Count() : false;

        PageIndicatorItem.Visibility = (IsVertical || TotalPages <= 1) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void DisableNavigation()
    {
        PreviousPage.IsHitTestVisible = false;
        NextPage.IsHitTestVisible = false;
    }

    private void EnableNavigation()
    {
        PreviousPage.IsHitTestVisible = true;
        NextPage.IsHitTestVisible = true;
        CheckPages();
    }
}

