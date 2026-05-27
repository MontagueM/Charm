using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Charm;

public partial class ItemPage : UserControl, INotifyPropertyChanged
{
    public ItemsControl _ItemList => ItemList;

    #region Backing Fields
    private int _columns = 3;
    private bool _isVertical = false;
    private bool _usePlaceholders = false;
    private bool _collapsePageButtons = true;
    private bool _hidePageButtons = false;
    private bool _expand = false;
    private Thickness _itemMargin = new(0, 0, 0, 0);
    private Thickness _pageIndicatorMargin = new(0, 0, 0, -20);
    private SolidColorBrush _pageBackgroundColor = new(Color.FromArgb(0, 0, 0, 0));
    private bool _showVerticalPageIndicator = false;
    private int _currentPage = 0;
    private int _totalPages = 1;
    private bool _useStackPanel = false;
    private bool _selectOnPageChange = true;
    private float _slideDistance = 3.0f;
    private float _transitionSpeed = 0.075f;
    private bool _fadeInOnLoad = true;
    #endregion

    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        nameof(Items),
        typeof(IEnumerable<CharmUIElement>),
        typeof(ItemPage),
        new PropertyMetadata(Enumerable.Empty<CharmUIElement>(), OnItemsChanged));
    public IEnumerable<CharmUIElement> Items
    {
        get => (IEnumerable<CharmUIElement>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ItemPage)d;
        var newItems = e.NewValue as IEnumerable<CharmUIElement>;

        control.Items = newItems;
        if (control.Items != null)
            control.DisplayItems(true);
    }

    // needing to do this just to use bindings on it is so stupid
    public static readonly DependencyProperty ItemsPerPageProperty = DependencyProperty.Register(
           nameof(ItemsPerPage),
           typeof(int),
           typeof(ItemPage),
           new PropertyMetadata(0)
       );
    public int ItemsPerPage
    {
        get => (int)GetValue(ItemsPerPageProperty);
        set => SetValue(ItemsPerPageProperty, value);
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

    public bool ShowVerticalPageIndicator
    {
        get => _showVerticalPageIndicator;
        set
        {
            if (_showVerticalPageIndicator != value)
            {
                _showVerticalPageIndicator = value;
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

    public SolidColorBrush PageBackgroundColor
    {
        get => _pageBackgroundColor;
        set
        {
            if (_pageBackgroundColor != value)
            {
                _pageBackgroundColor = value;
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
    /// How long it'll take to change pages, if you want to change that for some reason
    /// </summary>
    public float TransitionSpeed
    {
        get => _transitionSpeed;
        set
        {
            if (_transitionSpeed != value)
            {
                _transitionSpeed = value;
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

    /// <summary>
    /// Fade in on first time load
    /// </summary>
    public bool FadeInOnLoad
    {
        get => _fadeInOnLoad;
        set
        {
            if (_fadeInOnLoad != value)
            {
                _fadeInOnLoad = value;
                OnPropertyChanged();
            }
        }
    }

    public static readonly DependencyProperty ItemTemplateProperty =
       DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(ItemPage), new PropertyMetadata(null));

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

    public event EventHandler<ItemsControl> OnPreviousPageClicked;
    public event EventHandler<ItemsControl> OnNextPageClicked;

    public event EventHandler<ItemsControl> OnBeforePageChange;
    public event EventHandler<ItemsControl> OnAfterPageChange;

    private ButtonBase _customNextButton = null;
    public ButtonBase CustomNextButton
    {
        get => _customNextButton;
        set
        {
            if (_customNextButton != value)
            {
                _customNextButton = value;
                _customNextButton.Click += NextPage_Click;
                OnPropertyChanged();
            }
        }
    }

    private ButtonBase _customPrevButton = null;
    public ButtonBase CustomPrevButton
    {
        get => _customPrevButton;
        set
        {
            if (_customPrevButton != value)
            {
                _customPrevButton = value;
                _customPrevButton.Click += PreviousPage_Click;
                OnPropertyChanged();
            }
        }
    }

    private bool _isAnimating = false;
    private bool _firstTimeLoad = true;

    public List<CharmUIElement> CurrentPageItems { get; private set; } = new();

    public ItemPage()
    {
#if DEBUG
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        DataContext = this;
        // stops returning to the first page if the user switches tabs and back
        // doesnt happen in CategoryView but does in DareView, whack
        if (_firstTimeLoad)
        {
            _firstTimeLoad = false;
            DisplayItems();
        }
    }

    public async void DisplayItems(bool fromStart = false)
    {
        if (fromStart)
            CurrentPage = 0;

        var items = Items?.ToList() ?? new List<CharmUIElement>();
        int itemsPerPage = GetItemsPerPage();
        int count = items.Count;
        int totalPages = Math.Max(1, (int)Math.Ceiling((double)count / itemsPerPage));
        var itemsToShow = items.Skip(CurrentPage * itemsPerPage).Take(itemsPerPage).ToList();

        if (UsePlaceholders)
        {
            int placeholderCount = itemsPerPage - itemsToShow.Count;
            for (int i = 0; i < placeholderCount; i++)
                itemsToShow.Add(new CharmUIElement { IsPlaceholder = true });
        }

        await Dispatcher.InvokeAsync(() =>
        {
            if (fromStart && FadeInOnLoad)
                UIHelper.AnimateFade(ItemList, TransitionSpeed * 2, 1f, 0);

            TotalPages = totalPages;

            if (!HidePageButtons)
            {
                PreviousPage.Visibility = count > itemsPerPage ? Visibility.Visible : (CollapsePageButtons ? Visibility.Collapsed : Visibility.Hidden);
                NextPage.Visibility = count > itemsPerPage ? Visibility.Visible : (CollapsePageButtons ? Visibility.Collapsed : Visibility.Hidden);
            }

            if (itemsToShow.Count == 0)
                SelectPreviousPage();

            CurrentPageItems = itemsToShow;
            ItemList.ItemsSource = itemsToShow;
            CheckPages();
        });
    }

    /// <summary>
    /// Previous/Next page if using custom controls to change pages (arrow keys for example)
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
        var targetPage = -1;
        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            targetPage = -TotalPages;
        else if (((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) && TotalPages >= 4)
            targetPage = (int)Math.Floor(-TotalPages / 4f);

        ChangePage(targetPage,
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
        var targetPage = 1;
        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            targetPage = TotalPages;
        else if (((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) && TotalPages >= 4)
            targetPage = (int)Math.Ceiling(TotalPages / 4f);

        ChangePage(targetPage,
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
        if (_isAnimating) return false;

        var itemsPerPage = GetItemsPerPage();
        int targetPage = Math.Clamp(CurrentPage + direction, 0, TotalPages - 1);

        bool canChange = direction < 0
            ? CurrentPage > 0
            : Items.Count() > 0 && targetPage * itemsPerPage < Items.Count();

        if (!canChange) return false;

        _isAnimating = true;
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

        UIHelper.AnimateSlide(ItemList, TransitionSpeed, entrySlide, new(0, 0));

        UIHelper.AnimateFade(ItemList, TransitionSpeed, 0f, 1f, (s, e) =>
        {
            CurrentPage = targetPage;
            DisplayItems();
            afterChange?.Invoke();

            UIHelper.AnimateSlide(ItemList, TransitionSpeed, new(0, 0), exitSlide);
            UIHelper.AnimateFade(ItemList, TransitionSpeed, 1f, 0, (s, e) =>
            {
                _isAnimating = false;
                EnableNavigation();
                completeAction?.Invoke(s, e);

            }, additive: true);

        }, additive: true);

        return true;
    }

    public void CheckPages()
    {
        var itemsPerPage = GetItemsPerPage();
        if (Items.Count() == 0)
            CurrentPage = 0;

        if (HidePageButtons)
        {
            PreviousPage.Visibility = Visibility.Collapsed;
            NextPage.Visibility = Visibility.Collapsed;
        }

        PreviousPage.IsEnabled = CurrentPage != 0;
        NextPage.IsEnabled = Items.Count() > 0 ? (CurrentPage + 1) * itemsPerPage < Items.Count() : false;

        CustomPrevButton?.SetValue(UIElement.IsEnabledProperty, PreviousPage.IsEnabled);
        CustomNextButton?.SetValue(UIElement.IsEnabledProperty, NextPage.IsEnabled);

        PageIndicatorItem.Visibility = ((IsVertical && !ShowVerticalPageIndicator) || TotalPages <= 1) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void DisableNavigation()
    {
        PreviousPage.IsHitTestVisible = false;
        NextPage.IsHitTestVisible = false;

        CustomPrevButton?.SetValue(UIElement.IsHitTestVisibleProperty, PreviousPage.IsHitTestVisible);
        CustomNextButton?.SetValue(UIElement.IsHitTestVisibleProperty, NextPage.IsHitTestVisible);
    }

    private void EnableNavigation()
    {
        PreviousPage.IsHitTestVisible = true;
        NextPage.IsHitTestVisible = true;

        CustomPrevButton?.SetValue(UIElement.IsHitTestVisibleProperty, PreviousPage.IsHitTestVisible);
        CustomNextButton?.SetValue(UIElement.IsHitTestVisibleProperty, NextPage.IsHitTestVisible);
        CheckPages();
    }

    private int GetItemsPerPage()
    {
        if (ItemsPerPage <= 1) return ItemsPerPage;
        if (Expand && CollapsePageButtons && Items.Count() > ItemsPerPage)
            return ItemsPerPage - 1;
        return ItemsPerPage;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

