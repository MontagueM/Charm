using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Charm;

public partial class HoverPopupWrapper : UserControl
{
    private Border _floatingContainer;
    private FrameworkElement _parent;

    public HoverPopupWrapper()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        //CompositionTarget.Rendering += OnRender;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (PopupParent != null)
        {
            _parent = UIHelper.FindElementWithDataType(this, PopupParent);
        }

        // If not explicitly set, try to grab the shared app-wide overlay
        if (HoverOverlayTarget == null && Application.Current.MainWindow is MainWindow main)
        {
            HoverOverlayTarget = EnableMotionEffect ? main.OverlayRoot : main.OverlayRootNoMotion;
        }

        if (Target is FrameworkElement target)
        {
            target.MouseEnter += (_, _) => ShowPopup();
            target.MouseLeave += async (_, _) => await TryClosePopup();
        }
    }

    private void ShowPopup()
    {
        if (_floatingContainer != null || HoverOverlayTarget == null || Target is not FrameworkElement target) return;
        Point position = target.TranslatePoint(new Point(0, target.ActualHeight), HoverOverlayTarget);

        _floatingContainer = new Border();
        _floatingContainer.IsHitTestVisible = true;
        var style = (Style)FindResource("FloatingContainerStyle");
        if (style != null)
            _floatingContainer.Style = style;

        var contentPresenter = new ContentPresenter
        {
            Content = PopupContent
        };

        _floatingContainer.Child = contentPresenter;
        _floatingContainer.MouseEnter += (_, _) => { };
        _floatingContainer.MouseLeave += async (_, _) => await TryClosePopup();

        if (_parent is FrameworkElement parent)
        {
            PopupContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            double popupHeight = PopupContent.DesiredSize.Height;
            double popupWidth = PopupContent.DesiredSize.Width;

            double overlayHeight = HoverOverlayTarget.ActualHeight;
            double overlayWidth = HoverOverlayTarget.ActualWidth;

            Point targetBottom = parent
                .TransformToVisual(HoverOverlayTarget)
                .Transform(new Point(0, parent.ActualHeight));

            double x = targetBottom.X;
            double y = targetBottom.Y;
            if (y + popupHeight > overlayHeight)
                y = targetBottom.Y - popupHeight - parent.ActualHeight;

            if (x + popupWidth > overlayWidth)
                x = targetBottom.X - popupWidth + target.ActualWidth;

            x = Math.Clamp(x, 0, overlayWidth - popupWidth);
            y = Math.Clamp(y, 0, overlayHeight - popupHeight);

            position = new Point(x, y);
        }

        Canvas.SetLeft(_floatingContainer, position.X);
        Canvas.SetTop(_floatingContainer, position.Y);

        UIHelper.AnimateFade(_floatingContainer, 0.125f, 1, 0);
        UIHelper.AnimateScale(_floatingContainer, 0.15f, new(1, 1), new(0, 0));

        HoverOverlayTarget.Children.Add(_floatingContainer);
        HoverOverlayTarget.IsHitTestVisible = true;
    }

    private async Task TryClosePopup()
    {
        if (_floatingContainer != null && !_floatingContainer.IsMouseOver && (Target as FrameworkElement)?.IsMouseOver == false)
        {
            await Close();
        }
    }

    public async void ForceClose()
    {
        if (_floatingContainer != null)
        {
            if (_floatingContainer.IsMouseOver)
                _floatingContainer.IsHitTestVisible = false; // turns out its that shrimple

            else if ((Target as FrameworkElement)?.IsMouseOver == true)
            {
                await Close();
            }
        }
    }

    private async Task Close()
    {
        UIHelper.AnimateFade(_floatingContainer, 0.1f, 0, additive: true);
        UIHelper.AnimateScale(_floatingContainer, 0.15f, new(0, 0), new(1, 1));
        await Task.Delay(100);
        if (_floatingContainer?.Parent is Panel parentPanel)
        {
            parentPanel.Children.Remove(_floatingContainer);
        }

        HoverOverlayTarget.Children.Remove(_floatingContainer);
        _floatingContainer = null;
    }

    private void OnRender(object sender, EventArgs e)
    {
        //if (_floatingContainer is null)
        //    return;

        //// Try to find the TranslateTransform in the TransformGroup
        //var translateTransform = UIHelper.EnsureTransformGroup(_floatingContainer).Children.OfType<TranslateTransform>().FirstOrDefault();
        //if (translateTransform == null)
        //    return;

        //System.Windows.Point position = Mouse.GetPosition(this);
        //translateTransform.X = Math.Round(position.X * -0.0075f);
        //translateTransform.Y = Math.Round(position.Y * -0.0075f);
    }

    // -- Dependency Properties --

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(UIElement), typeof(HoverPopupWrapper), new PropertyMetadata(null));

    public UIElement Target
    {
        get => (UIElement)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public static readonly DependencyProperty PopupContentProperty =
        DependencyProperty.Register(nameof(PopupContent), typeof(UIElement), typeof(HoverPopupWrapper), new PropertyMetadata(null));

    public UIElement PopupContent
    {
        get => (UIElement)GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }

    public static readonly DependencyProperty HoverOverlayTargetProperty =
        DependencyProperty.Register(nameof(HoverOverlayTarget), typeof(Canvas), typeof(HoverPopupWrapper), new PropertyMetadata(null));

    public Canvas HoverOverlayTarget
    {
        get => (Canvas)GetValue(HoverOverlayTargetProperty);
        set => SetValue(HoverOverlayTargetProperty, value);
    }

    public static readonly DependencyProperty PopupParentProperty =
        DependencyProperty.Register(nameof(PopupParent), typeof(Type), typeof(HoverPopupWrapper), new PropertyMetadata(null));

    public Type PopupParent
    {
        get => (Type)GetValue(PopupParentProperty);
        set => SetValue(PopupParentProperty, value);
    }

    private bool _enableMotionEffect = true;
    public bool EnableMotionEffect
    {
        get => _enableMotionEffect;
        set => _enableMotionEffect = value;
    }
}

