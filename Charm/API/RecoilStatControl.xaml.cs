using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Charm;

/// From DIM https://github.com/DestinyItemManager/DIM/blob/master/src/app/item-popup/RecoilStat.tsx
public partial class RecoilStatControl : UserControl
{
    public static readonly DependencyProperty ValueProperty =
           DependencyProperty.Register(nameof(Value), typeof(double), typeof(RecoilStatControl),
               new PropertyMetadata(0.0, OnValueChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private const double VerticalScale = 0.8;
    private const double MaxSpread = 180.0; // degrees

    public RecoilStatControl()
    {
        InitializeComponent();
        Loaded += (s, e) => Redraw();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RecoilStatControl control)
            control.Redraw();
    }

    private void Redraw()
    {
        DrawingCanvas.Children.Clear();

        double direction = Math.Sin((Value + 5) * (Math.PI / 10)) * (100 - Value);
        double radians = direction * VerticalScale * (Math.PI / 180);

        double x = Math.Sin(radians);
        double y = Math.Cos(radians);

        double spread =
            ((100 - Value) / 100.0) *
            (MaxSpread / 2.0) *
            (Math.PI / 180.0) *
            Math.Sign(direction);

        double xSpreadMore = Math.Sin(radians + spread);
        double ySpreadMore = Math.Cos(radians + spread);
        double xSpreadLess = Math.Sin(radians - spread);
        double ySpreadLess = Math.Cos(radians - spread);

        double cx = 20, cy = 20;
        double r = 20;

        // Background half-circle (bottom semicircle, centered at cx, cy)
        PathFigure semicircle = new PathFigure
        {
            StartPoint = new Point(cx - r, cy), // left
            IsClosed = true
        };

        // Arc from left to right (bottom half of the circle)
        semicircle.Segments.Add(new ArcSegment
        {
            Point = new Point(cx + r, cy), // right
            Size = new Size(r, r),
            IsLargeArc = false,
            SweepDirection = SweepDirection.Clockwise
        });

        // Line back to start to close the path (but stay on the arc line)
        PathGeometry geometry = new PathGeometry();
        geometry.Figures.Add(semicircle);

        DrawingCanvas.Children.Add(new Path
        {
            Data = geometry,
            Fill = Brushes.White,
            Opacity = 0.1
        });

        if (Value >= 95)
        {
            DrawingCanvas.Children.Add(new Line
            {
                X1 = cx - x * r,
                Y1 = cy + y * r,
                X2 = cx + x * r,
                Y2 = cy - y * r,
                Stroke = Brushes.White,
                StrokeThickness = 1.2
            });
        }
        else
        {
            PathFigure arc = new PathFigure { StartPoint = new Point(cx + xSpreadMore * r, cy - ySpreadMore * r) };
            bool isLargeArc = false;
            bool isSweep = direction < 0;

            arc.Segments.Add(new ArcSegment
            {
                Point = new Point(cx + xSpreadLess * r, cy - ySpreadLess * r),
                Size = new Size(r, r),
                SweepDirection = isSweep ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                IsLargeArc = isLargeArc
            });

            arc.Segments.Add(new LineSegment { Point = new Point(cx, cy) });
            arc.IsClosed = true;

            PathGeometry geometry2 = new PathGeometry();
            geometry2.Figures.Add(arc);

            DrawingCanvas.Children.Add(new Path
            {
                Data = geometry2,
                Fill = Brushes.White
            });
        }
    }
}

