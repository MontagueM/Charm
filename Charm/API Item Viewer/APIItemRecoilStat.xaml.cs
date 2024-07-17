using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Charm;

/// <summary>
/// Interaction logic for APIItemRecoilStat.xaml
/// </summary>
public partial class APIItemRecoilStat : UserControl
{
    public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(APIItemRecoilStat), new PropertyMetadata(0.0, OnValueChanged));

    public double Value
    {
        get { return (double)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public APIItemRecoilStat()
    {
        InitializeComponent();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        APIItemRecoilStat control = (APIItemRecoilStat)d;
        control.UpdateRecoilVisual();
    }

    private void UpdateRecoilVisual()
    {
        // How much to bias the direction towards the center - at 1.0 this would mean recoil would swing ±90°
        double verticalScale = 0.8;
        // The maximum angle of the pie, where zero recoil is the widest and 100 recoil is the narrowest
        double maxSpread = 180; // degrees

        double direction = RecoilDirection(Value) * verticalScale * (Math.PI / 180); // Convert to radians

        double spread =
                    // Higher value means less spread
                    ((100 - Value) / 100) *
                    // scaled by the spread factor (halved since we expand to either side)
                    (maxSpread / 2) *
                    // in radians
                    (Math.PI / 180) *
                    // flipped for negative
                    Math.Sign(direction);

        double xSpreadMore = Math.Sin(direction + spread);
        double ySpreadMore = Math.Cos(direction + spread);
        double xSpreadLess = Math.Sin(direction - spread);
        double ySpreadLess = Math.Cos(direction - spread);

        var d = $"M1,1 L{1 + xSpreadMore},{1 - ySpreadMore} A1,1 0 0,{(direction <= 0 ? '1' : '0')} {1 + xSpreadLess}, {1 - ySpreadLess} Z";
        //Console.WriteLine($"{Value} {direction} {d} {(float)direction < 0}");
        if (Value < 95)
        {
            recoilPath.Data = Geometry.Parse(d);
            // stupid dumb hacky fix for the position being wrong (works 70% of the time)
            // bool a = direction < 0;
            //recoilPath.RenderTransformOrigin = new Point(a ? 1.0 : 0.5, 0.5);
            recoilPath.RenderTransformOrigin = new Point(0.5, 0.5);
        }
        else
        {
            recoilPath.Data = Geometry.Parse($"M1,1 L1.05,0 A1,1 0 0,0 0.95, 0 Z");
            recoilPath.RenderTransformOrigin = new Point(0.5, 0.5);
        }

    }

    private double RecoilDirection(double value)
    {
        return Math.Sin((value + 5) * (Math.PI / 10)) * (100 - value);
    }
}

