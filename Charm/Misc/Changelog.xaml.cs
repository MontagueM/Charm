using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
namespace Charm;

/// <summary>
/// Interaction logic for Changelog.xaml
/// </summary>
public partial class Changelog : UserControl
{
    public List<ChangelogEntry> Entries = new List<ChangelogEntry>();
    public ChangelogEntry SelectedEntry;

    public Changelog()
    {
        InitializeComponent();
    }

    public void Load()
    {
        Focusable = true;
        Focus();

        var json = File.ReadAllText($"./Changelog.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Entries = JsonSerializer.Deserialize<List<ChangelogEntry>>(json, options);

        ChangelogVersions.Items = Entries;
        ChangelogVersions.DisplayItems();
    }

    private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
            MainWindow.Current.ViewboxGrid.Children.Remove(this);
    }

    private void ChangelogEntry_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DataContext = (sender as RadioButton).DataContext as ChangelogEntry;
        ChangeLogPanel.Visibility = Visibility.Visible;
    }

    private void ChangelogEntry_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        DataContext = (sender as RadioButton).DataContext as ChangelogEntry;
        ChangeLogPanel.Visibility = Visibility.Visible;
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Current.ViewboxGrid.Children.Remove(this);
    }
}

public class ChangelogEntry : CharmUIElement
{
    public string Version { get; set; } = "";
    public string Date { get; set; } = "";
    public List<ChangeItem> Changes { get; set; } = new();
}

public class ChangeItem : CharmUIElement
{
    public string Title { get; set; } = "";
    public List<string> Notes { get; set; }
}
