using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Charm;

/// <summary>
/// Interaction logic for UserControl1.xaml
/// </summary>
public partial class NotificationBanner : UserControl
{
    public PopupStyle Style = PopupStyle.Information;
    public string Icon { get; set; }
    public ImageSource IconImage { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    public Action OnProgressComplete = null;

    public SolidColorBrush ExpanderColor { get; set; }

    public NotificationBanner()
    {
        InitializeComponent();
    }

    public void Show()
    {
        MainWindow.Current.ViewboxGrid.Children.Add(this);
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        switch (Style)
        {
            case PopupStyle.Warning:
                ExpanderColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x96, 0x30, 0x30));
                break;
            case PopupStyle.Information:
                ExpanderColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x2F, 0x7F, 0x96));
                break;
            case PopupStyle.Generic:
                ExpanderColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x96, 0x96, 0x96));
                break;
        }

        if (Title is null || Title == string.Empty)
            TitleText.Visibility = Visibility.Collapsed;
        if (Description is null || Description == string.Empty)
            DescriptionText.Visibility = Visibility.Collapsed;

        DataContext = this;
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
