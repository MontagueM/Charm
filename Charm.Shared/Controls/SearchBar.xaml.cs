using System.Windows;
using System.Windows.Controls;

namespace Charm.Shared;

/// <summary>
/// Interaction logic for SearchBar.xaml
/// </summary>
public partial class SearchBar : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(SearchBar),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchBar),
            new PropertyMetadata(string.Empty, OnPlaceholderChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public SearchBar()
    {
        InitializeComponent();
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        Text = SearchTextBox.Text;
        PlaceholderText.Visibility = string.IsNullOrEmpty(SearchTextBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (SearchBar)d;
        if (box.SearchTextBox.Text != (string)e.NewValue)
            box.SearchTextBox.Text = (string)e.NewValue;
    }

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SearchBar)d).PlaceholderText.Text = (string)e.NewValue;
    }
}
