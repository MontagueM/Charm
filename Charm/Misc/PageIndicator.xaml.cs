using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Charm;

public partial class PageIndicator : UserControl
{
    private ObservableCollection<PageIndicatorDot> _indicators = new();
    public ObservableCollection<PageIndicatorDot> Indicators
    {
        get => _indicators;
        set
        {
            _indicators = value;
            OnPropertyChanged(nameof(Indicators));
        }
    }

    private bool _shrinkInactive = true;
    public bool ShrinkInactive
    {
        get => _shrinkInactive;
        set
        {
            if (_shrinkInactive != value)
            {
                _shrinkInactive = value;
                OnPropertyChanged(nameof(_shrinkInactive));
            }
        }
    }

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set
        {
            SetValue(CurrentPageProperty, value);
            UpdateIndicators();
        }
    }

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PageIndicator), new PropertyMetadata(0, OnCurrentPageChanged));

    private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (PageIndicator)d;
        control.UpdateIndicators();
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set
        {
            SetValue(TotalPagesProperty, value);
            UpdateIndicators();
        }
    }

    public static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(PageIndicator), new PropertyMetadata(1, OnTotalPagesChanged));

    private static void OnTotalPagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (PageIndicator)d;
        control.UpdateIndicators();
    }

    public PageIndicator()
    {
        InitializeComponent();
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        ShrinkInactive = TotalPages > 7;

        Indicators.Clear();
        for (int i = 0; i < TotalPages; i++)
        {
            Indicators.Add(new PageIndicatorDot { IsCurrent = i == CurrentPage });
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public class PageIndicatorDot
{
    public bool IsCurrent { get; set; }
}

public class BoolToColorConverter : IValueConverter
{
    public Color ActiveColor { get; set; } = Colors.White;
    public Color InactiveColor { get; set; } = Colors.Gray;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? ActiveColor : InactiveColor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
