using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Charm.Shared;

public static class Draggable
{
    private class DragState
    {
        public bool IsDragging;
        public Point Offset;
        public FrameworkElement Target;
        public FrameworkElement Parent;
    }

    private static readonly DependencyProperty DragStateProperty =
        DependencyProperty.RegisterAttached(
            "DragState",
            typeof(DragState),
            typeof(Draggable));

    // ---------------------------
    // IsEnabled
    // ---------------------------
    public static bool GetIsEnabled(DependencyObject obj)
        => (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value)
        => obj.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(Draggable),
            new PropertyMetadata(false, OnIsEnabledChanged));

    // ---------------------------
    // Initial positions
    // ---------------------------
    public static double GetInitialLeft(DependencyObject obj)
    => (double)obj.GetValue(InitialLeftProperty);

    public static void SetInitialLeft(DependencyObject obj, double value)
        => obj.SetValue(InitialLeftProperty, value);

    public static readonly DependencyProperty InitialLeftProperty =
        DependencyProperty.RegisterAttached(
            "InitialLeft",
            typeof(double),
            typeof(Draggable),
            new PropertyMetadata(double.NaN));

    public static double GetInitialTop(DependencyObject obj)
    => (double)obj.GetValue(InitialTopProperty);

    public static void SetInitialTop(DependencyObject obj, double value)
        => obj.SetValue(InitialTopProperty, value);

    public static readonly DependencyProperty InitialTopProperty =
        DependencyProperty.RegisterAttached(
            "InitialTop",
            typeof(double),
            typeof(Draggable),
            new PropertyMetadata(double.NaN));

    // ---------------------------
    // Handle (drag region)
    // ---------------------------
    public static FrameworkElement GetHandle(DependencyObject obj)
        => (FrameworkElement)obj.GetValue(HandleProperty);

    public static void SetHandle(DependencyObject obj, FrameworkElement value)
        => obj.SetValue(HandleProperty, value);

    public static readonly DependencyProperty HandleProperty =
        DependencyProperty.RegisterAttached(
            "Handle",
            typeof(FrameworkElement),
            typeof(Draggable));

    // ---------------------------
    // ClampToParent
    // ---------------------------
    public static bool GetClampToParent(DependencyObject obj)
        => (bool)obj.GetValue(ClampToParentProperty);

    public static void SetClampToParent(DependencyObject obj, bool value)
        => obj.SetValue(ClampToParentProperty, value);

    public static readonly DependencyProperty ClampToParentProperty =
        DependencyProperty.RegisterAttached(
            "ClampToParent",
            typeof(bool),
            typeof(Draggable),
            new PropertyMetadata(true));

    // ---------------------------
    // Enable / Disable
    // ---------------------------
    private static void OnIsEnabledChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement target)
            return;

        if ((bool)e.NewValue)
            Attach(target);
        else
            Detach(target);
    }

    private static void Attach(FrameworkElement target)
    {
        var state = new DragState { Target = target };
        target.SetValue(DragStateProperty, state);

        double left = Canvas.GetLeft(target);
        double top = Canvas.GetTop(target);

        if (double.IsNaN(left))
        {
            double initialLeft = GetInitialLeft(target);
            Canvas.SetLeft(
                target,
                double.IsNaN(initialLeft) ? 0 : initialLeft);
        }

        if (double.IsNaN(top))
        {
            double initialTop = GetInitialTop(target);
            Canvas.SetTop(
                target,
                double.IsNaN(initialTop) ? 0 : initialTop);
        }

        target.Loaded += Target_Loaded;
    }

    private static void Detach(FrameworkElement target)
    {
        target.Loaded -= Target_Loaded;
        target.ClearValue(DragStateProperty);
    }

    private static void Target_Loaded(object sender, RoutedEventArgs e)
    {
        var target = (FrameworkElement)sender;
        var state = new DragState
        {
            Target = target,
            Parent = VisualTreeHelper.GetParent(target) as FrameworkElement
        };

        var handle = GetHandle(target) ?? target;
        handle.SetValue(DragStateProperty, state);
        target.SetValue(DragStateProperty, state);

        handle.Cursor = Cursors.SizeAll;
        handle.MouseLeftButtonDown += OnMouseDown;
        handle.MouseMove += OnMouseMove;
        handle.MouseLeftButtonUp += OnMouseUp;
        target.SizeChanged += (_, __) => ReClamp(state);
    }

    // ---------------------------
    // Mouse Logic
    // ---------------------------
    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement handle)
            return;

        var state = (DragState)handle.GetValue(DragStateProperty);
        if (state == null)
            return;

        state.IsDragging = true;
        handle.CaptureMouse();

        var mousePos = e.GetPosition(state.Parent);

        state.Offset = new Point(
            mousePos.X - GetLeft(state.Target),
            mousePos.Y - GetTop(state.Target));
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement handle)
            return;

        var state = (DragState)handle.GetValue(DragStateProperty);
        if (state == null || !state.IsDragging)
            return;

        var mousePos = e.GetPosition(state.Parent);

        double x = mousePos.X - state.Offset.X;
        double y = mousePos.Y - state.Offset.Y;

        if (GetClampToParent(state.Target))
        {
            x = Clamp(x, 0, state.Parent.ActualWidth - state.Target.ActualWidth);
            y = Clamp(y, 0, state.Parent.ActualHeight - state.Target.ActualHeight);
        }

        Canvas.SetLeft(state.Target, x);
        Canvas.SetTop(state.Target, y);
    }

    private static void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement handle)
            return;

        var state = (DragState)handle.GetValue(DragStateProperty);
        if (state == null)
            return;

        state.IsDragging = false;
        handle.ReleaseMouseCapture();
    }

    // ---------------------------
    // Helpers
    // ---------------------------
    private static FrameworkElement FindTarget(FrameworkElement handle)
    {
        return (FrameworkElement)
            LogicalTreeHelper.GetParent(handle)
            ?? handle;
    }

    private static double Clamp(double value, double min, double max)
        => value < min ? min : value > max ? max : value;

    private static double GetLeft(FrameworkElement element)
    {
        var value = Canvas.GetLeft(element);
        return double.IsNaN(value) ? 0 : value;
    }

    private static double GetTop(FrameworkElement element)
    {
        var value = Canvas.GetTop(element);
        return double.IsNaN(value) ? 0 : value;
    }

    private static void ReClamp(DragState state)
    {
        if (!GetClampToParent(state.Target))
            return;

        if (state.Parent == null)
            return;

        double left = GetLeft(state.Target);
        double top = GetTop(state.Target);

        double maxX = Math.Max(0, state.Parent.ActualWidth - state.Target.ActualWidth);
        double maxY = Math.Max(0, state.Parent.ActualHeight - state.Target.ActualHeight);

        double clampedX = Clamp(left, 0, maxX);
        double clampedY = Clamp(top, 0, maxY);

        Canvas.SetLeft(state.Target, clampedX);
        Canvas.SetTop(state.Target, clampedY);
    }
}
