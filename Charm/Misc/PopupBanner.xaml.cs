using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
namespace Charm;

/// <summary>
/// Interaction logic for UserControl1.xaml
/// </summary>
public partial class PopupBanner : UserControl
{
    private DispatcherTimer holdTimer;
    private const int TickInterval = 1;
    private int elapsedTime = 0;

    public bool DarkenBackground = false;
    public bool Progress = false;
    public int HoldDuration = 1000;

    public PopupStyle Style = PopupStyle.Information;
    public string Icon { get; set; }
    public ImageSource IconImage { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string Description { get; set; }
    public string UserInput { get; set; } = "Dismiss";
    public string UserInputSecondary { get; set; }

    public Action OnProgressComplete = null;

    public SolidColorBrush ExpanderColor { get; set; }
    public SolidColorBrush BodyColor { get; set; }
    public SolidColorBrush IconColor { get; set; }

    public PopupBanner()
    {
        InitializeComponent();
    }

    public void Show()
    {
        MainWindow.Current.ViewboxGrid.Children.Add(this);
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DarkenBackground)
            MainGrid.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 0, 0, 0));

        switch (Style)
        {
            case PopupStyle.Warning:
                ExpanderColor = new SolidColorBrush(Color.FromArgb(0xBF, 0x96, 0x30, 0x30));
                BodyColor = new SolidColorBrush(Color.FromArgb(0x7F, 0x96, 0x30, 0x30));
                IconColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x96, 0x30, 0x30));
                break;
            case PopupStyle.Information:
                ExpanderColor = new SolidColorBrush(Color.FromArgb(0xBF, 0x2F, 0x7F, 0x96));
                BodyColor = new SolidColorBrush(Color.FromArgb(0x7F, 0x2F, 0x7F, 0x96));
                IconColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x2F, 0x7F, 0x96));
                break;
            case PopupStyle.Generic:
                ExpanderColor = new SolidColorBrush(Color.FromArgb(0xBF, 0x96, 0x96, 0x96));
                BodyColor = new SolidColorBrush(Color.FromArgb(0x7F, 0x96, 0x96, 0x96));
                IconColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x96, 0x96, 0x96));
                break;
        }

        if (Progress)
        {
            this.MouseLeftButtonDown += HoldElement_MouseLeftButtonDown;
            this.MouseLeftButtonUp += HoldElement_MouseLeftButtonUp;

            holdTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TickInterval) };
            holdTimer.Tick += HoldTimer_Tick;
        }
        else
        {
            this.MouseLeftButtonDown += Remove;
        }

        if (Subtitle is null || Subtitle == string.Empty)
            SubtitleText.Visibility = Visibility.Collapsed;
        if (Description is null || Description == string.Empty)
            DescriptionText.Visibility = Visibility.Collapsed;
        if (UserInputSecondary is null || UserInputSecondary == string.Empty)
            SecondaryUserInput.Visibility = Visibility.Collapsed;


        if (Icon is null && IconImage is null)
        {
            DefaultIconsGrid.Visibility = Visibility.Visible;
            if (Style == PopupStyle.Warning)
                AlertIcon.Visibility = Visibility.Visible;
            else
                InfoIcon.Visibility = Visibility.Visible;
        }


        DataContext = this;
    }

    public void Remove(object sender, MouseButtonEventArgs e)
    {
        Remove();
    }

    private void HoldElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        elapsedTime = 0;
        HoldProgress.Value = 0;
        holdTimer.Start();
    }

    private void HoldElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        holdTimer.Stop();
        HoldProgress.Value = 0;
    }

    private void HoldTimer_Tick(object sender, EventArgs e)
    {
        elapsedTime += TickInterval;
        HoldProgress.Value = (double)elapsedTime / HoldDuration * 1000;
        if (HoldProgress.Value >= 100)
        {
            holdTimer.Stop();
            Remove();
        }
    }

    // isnt actually removed here, just starts the animation that calls the actual function when it ends
    public void Remove()
    {
        double currentHeight = ElementStack.ActualHeight;
        var heightAnimation = new DoubleAnimation
        {
            From = currentHeight,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.15),
            FillBehavior = FillBehavior.HoldEnd,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        ElementStack.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);

        Storyboard fadeOut = (Storyboard)FindResource("PopupFadeAnimation");
        fadeOut.Begin();
    }

    private void FadeOutAnimation_Completed(object sender, EventArgs e)
    {
        if (OnProgressComplete is not null)
            OnProgressComplete.Invoke();

        if (this.Parent is Panel parentPanel)
        {
            parentPanel.Children.Remove(this);
        }
    }

    public enum PopupStyle
    {
        Warning,
        Information,
        Generic
    }
}

public class SpacedTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            // Add a space between each character, and replace spaces with double spaces
            return string.Concat(text.Select(c => c == ' ' ? "  " : $"{c} ")).TrimEnd();
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

