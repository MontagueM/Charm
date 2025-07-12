using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tiger;
using Tiger.Schema;
using static Charm.PackageList;

namespace Charm;

public partial class StaticListView : UserControl
{
    private ConfigSubsystem Config = TigerInstance.GetSubsystem<ConfigSubsystem>();
    private ConcurrentBag<StaticItem> Statics = new();

    private int SortByIndex = 4;
    private FileHash _currentStatic;

    public StaticListView()
    {
        InitializeComponent();
#if DEBUG
        // I can't be asked to fix these seemingly harmless but lag inducing xaml binding errors
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif

        //PackageList.OnSearchBarChanged += (s, e) => RefreshPackageListCustom();
        PackageList.PackageItemChecked += async (s, item) =>
        {
            await LoadStaticList(item);
        };
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
    }

    public async void LoadContent()
    {
        StaticViewer.Visibility = Visibility.Hidden;
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Creating Statics List.",
        });

        await PackageList.MakePackageItems<StaticMesh>();
        MainWindow.Progress.CompleteStage();

        CreateFilterOptions();
    }

    private void CreateFilterOptions()
    {
        ComboBoxControl sortBy = new();
        sortBy.Text = "Sort By";
        sortBy.FontSize = 14;
        sortBy.Combobox.MinWidth = 175;
        sortBy.Combobox.ItemsSource = new List<ComboBoxItem>()
        {
            new() { Content = "Hash ↓", Tag = 4 },
            new() { Content = "Hash ↑", Tag = 3 },
        };
        if (sortBy.Combobox.SelectedIndex == -1)
        {
            sortBy.Combobox.SelectedIndex = 0;
        }

        sortBy.Combobox.SelectionChanged += SortBy_OnSelectionChanged;
        FilterOptions.Children.Add(sortBy);
    }

    private async Task LoadStaticList(PackageItem item)
    {
        await Task.Run(() =>
        {
            Dispatcher.Invoke(() => StaticViewer.Visibility = Visibility.Hidden);
            MainWindow.Progress.SetProgressStages(new List<string>
            {
                "Loading Statics."
            });

            if (Statics.Count != 0)
                Statics.Clear();

            Parallel.ForEach(item.Hashes, hash =>
            {
                if (hash.GetReferenceHash().IsInvalid())
                    return;

                var staticMesh = FileResourcer.Get().GetFile<StaticMesh>(hash);
                Statics.Add(new()
                {
                    Hash = hash,
                });
            });

            MainWindow.Progress.CompleteStage();
        });

        RefreshStaticList();
    }

    private void RefreshStaticList()
    {
        if (Statics == null)
            return;
        if (Statics.IsEmpty)
        {
            StaticList.ItemsSource = null;
            return;
        }

        string searchStr = StaticSearchBox.Text;

        uint parsedHash = 0;
        bool isHash = Helpers.ParseHash(searchStr, out parsedHash);

        var displayItems = new ConcurrentBag<StaticItem>();
        Parallel.ForEach(Statics, ent =>
        {
            if ((isHash && ent.Hash.Hash32 == parsedHash) || ent.Hash.ToString().Contains(searchStr, StringComparison.OrdinalIgnoreCase))
            {
                displayItems.Add(ent);
            }
        });

        List<StaticItem> items = displayItems.ToList();

        items = SortByIndex switch
        {
            2 => items.OrderByDescending(x => x.Hash).ToList(),
            1 => items.OrderBy(x => x.Hash).ToList(),
            _ => items
        };

        StaticList.ItemsSource = items;
        BulkExportButton.IsEnabled = items.Count > 0;
    }

    private void Static_OnClick(object sender, RoutedEventArgs e)
    {
        if ((sender as RadioButton) is null)
            return;

        StaticItem item = ((RadioButton)sender).DataContext as StaticItem;
        LoadStatic(item.Hash);
    }

    private void LoadStatic(FileHash hash)
    {
        ExportButton.IsEnabled = true;
        StaticViewer.LoadStatic(hash, ExportDetailLevel.MostDetailed);
        StaticViewer.ModelView.SetModelFunction(() => StaticViewer.LoadStatic(hash, ExportDetailLevel.MostDetailed));
        Dispatcher.Invoke(() => StaticViewer.Visibility = Visibility.Visible);
        _currentStatic = hash;
    }

    private void StaticSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshStaticList();
    }

    private void SortBy_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SortByIndex = (int)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        RefreshStaticList();
    }

    private async void BulkExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (StaticList.ItemsSource is not IEnumerable<StaticItem> items || !items.Any())
            return;

        // Hopefully this works fine, and not just for me
        var exportItems = items.DistinctBy(x => x.Hash);
        if (exportItems.Count() == 0)
            return;

        Dispatcher.Invoke(() =>
        {
            StaticViewer.ModelView.Visibility = Visibility.Hidden;
        });

        string pkgName = PackageResourcer.Get().GetPackage(items.First().Hash.PackageId).GetPackageMetadata().Name.Split(".")[0];
        string savePath = $"{Config.GetExportSavePath()}/Statics/{pkgName}";
        Directory.CreateDirectory(savePath);

        MainWindow.Progress.SetProgressStages(exportItems.Select((x, i) => $"Exporting {i + 1}/{exportItems.Count()}: {x.Hash}").ToList());
        await Task.Run(() =>
        {
            foreach (var item in exportItems)
            {
                var curStatic = FileResourcer.Get().GetFile<StaticMesh>(item.Hash);

                StaticView.ExportStatic(curStatic.Hash, curStatic.Hash, ExportTypeFlag.Full, $"{savePath}/{item.Hash}");
                MainWindow.Progress.CompleteStage();
            }
        });

        Dispatcher.Invoke(() =>
        {
            StaticViewer.ModelView.Visibility = Visibility.Visible;
            NotificationBanner notify = new()
            {
                Icon = "☑️",
                Title = "Bulk Export Complete",
                Description = $"Exported {exportItems.Count()} Statics to \"{savePath}\"",
                Style = NotificationBanner.PopupStyle.Information
            };
            notify.Show();
        });
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStatic is null)
            return;

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            $"Exporting Static {_currentStatic}",
        });

        var curStatic = FileResourcer.Get().GetFile<StaticMesh>(_currentStatic);
        Dispatcher.Invoke(() =>
        {
            StaticViewer.ModelView.Visibility = Visibility.Hidden;
        });
        StaticView.ExportStatic(curStatic.Hash, curStatic.Hash, ExportTypeFlag.Full);
        MainWindow.Progress.CompleteStage();

        Dispatcher.Invoke(() =>
        {
            StaticViewer.ModelView.Visibility = Visibility.Visible;
            NotificationBanner notify = new()
            {
                Icon = "☑️",
                Title = "Export Complete",
                Description = $"Exported Static {_currentStatic} to \"{ConfigSubsystem.Get().GetExportSavePath()}\\{_currentStatic}\\\"",
                Style = NotificationBanner.PopupStyle.Information
            };
            notify.Show();
        });
    }

    private CancellationTokenSource _StaticSelectionCts;
    private async void StaticList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _StaticSelectionCts?.Cancel();
        _StaticSelectionCts = new CancellationTokenSource();
        var token = _StaticSelectionCts.Token;

        try
        {
            await Task.Delay(100, token); // Debounce time
            if (token.IsCancellationRequested)
                return;

            Dispatcher.Invoke(() =>
            {
                if (StaticList.SelectedIndex >= 0)
                {
                    var container = StaticList.ItemContainerGenerator.ContainerFromIndex(StaticList.SelectedIndex);
                    RadioButton currentButton = UIHelper.GetChildOfType<RadioButton>(container);
                    if (currentButton != null)
                        currentButton.IsChecked = true;
                }
            });
        }
        catch (TaskCanceledException)
        {
        }
    }

    private class StaticItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public FileHash Hash { get; set; }
    }
}


