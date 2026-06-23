using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Arithmic;
using Charm.Shared;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using static Charm.PackageList;

namespace Charm;

/// <summary>
/// Interaction logic for TextureListView.xaml
/// </summary>
public partial class TextureListView : UserControl
{
    private ConfigSubsystem Config = TigerInstance.GetSubsystem<ConfigSubsystem>();

    private ConcurrentBag<TextureItem> Textures = new();

    private int SortByIndex = 4;

    private FileHash _currentDisplayedTexture;
    private bool _isCubemap = false;

    public TextureListView()
    {
#if DEBUG
        // I can't be asked to fix these seemingly harmless but lag inducing xaml binding errors
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif
        InitializeComponent();

        PackageList.PackageItemChecked += async (s, item) =>
        {
            await LoadTextureList(item);
        };
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
    }

    public async void LoadContent()
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Creating Texture List",
        });

        await PackageList.MakePackageItems<Texture>();
        MainWindow.Progress.CompleteStage();
        CreateFilterOptions();
    }


    private void CreateFilterOptions()
    {
        ComboBoxControl presets = new();
        presets.Text = "Presets";
        presets.TextFontSize = 16;
        presets.Box.ItemsSource = new List<ComboBoxItem>()
        {
            new() { Content = "None", Tag = "", FontSize = 10 },
            new() { Content = "(De)Buff Icons", Tag = "75x75", FontSize = 10 },
            new() { Content = "Items/Perks", Tag = "96x96", FontSize = 10 },
            new() { Content = "Ability Icons", Tag = "54x54", FontSize = 10 },
            new() { Content = "Weapon Icons", Tag = "137x76", FontSize = 10 },
            new() { Content = "Upsell Screen", Tag = "1920x830", FontSize = 10 },
            new() { Content = "Cubemap", Tag = "Cubemap", FontSize = 10 },
            new() { Content = "Volume", Tag = "Volume", FontSize = 10 },
            new() { Content = "1K", Tag = "1024", FontSize = 10 },
            new() { Content = "2K", Tag = "2048", FontSize = 10 },
            new() { Content = "4K", Tag = "4096", FontSize = 10 }

        };
        if (presets.Box.SelectedIndex == -1)
        {
            presets.Box.SelectedIndex = 0;
        }
        presets.Box.MinWidth = 175;
        presets.Box.ToolTip = "Based on texture resolutions";
        presets.Box.SelectionChanged += Presets_OnSelectionChanged;
        FilterOptions.Children.Add(presets);

        //----------------------------------------------

        ComboBoxControl sortBy = new();
        sortBy.Text = "Sort By";
        sortBy.TextFontSize = 16;
        sortBy.Box.ItemsSource = new List<ComboBoxItem>()
        {
            new() { Content = "Hash ↓", Tag = 4 },
            new() { Content = "Hash ↑", Tag = 3 },
            new() { Content = "Size ↓", Tag = 2 },
            new() { Content = "Size ↑", Tag = 1 }
        };
        if (sortBy.Box.SelectedIndex == -1)
        {
            sortBy.Box.SelectedIndex = 0;
        }

        sortBy.Box.SelectionChanged += SortBy_OnSelectionChanged;
        FilterOptions.Children.Add(sortBy);
    }

    private async Task LoadTextureList(PackageItem item)
    {
        if (Textures.Count != 0)
        {
            TextureList.ItemsSource = null;
            Textures.Clear();
        }

        Dispatcher.Invoke(() => TextureListLoading.Visibility = Visibility.Visible);

        await Task.Run(() => Parallel.ForEachAsync(item.Hashes, async (hash, ct) =>
        {
            // Get the textures dimensions directly from the raw data but only if we're loading from a parent pkg.
            // Adds a slight delay to loading but allows searching by dimensions
            (ushort width, ushort height, ushort depth, ushort array_size) dims = Helpers.GetTextureDimensionsRaw(hash);
            string dims_str = $"{dims.width}x{dims.height}";

            Textures.Add(new()
            {
                Hash = hash,
                Dimensions = dims_str,
                Width = dims.width,
                Height = dims.height,
                Depth = dims.depth,
                ArraySize = dims.array_size
            });
        }));

        TextureList.ItemsSource = Textures.OrderBy(x => x.Hash.Hash32);
        RefreshTextureList();
        Dispatcher.Invoke(() => TextureListLoading.Visibility = Visibility.Collapsed);
    }

    private void RefreshTextureList()
    {
        if (Textures == null)
            return;
        if (Textures.IsEmpty)
            return;

        string searchStr = TextureSearchBox.Text;

        uint parsedHash = 0;
        bool isHash = Helpers.ParseHash(searchStr, out parsedHash);

        var displayItems = new ConcurrentBag<TextureItem>();
        Parallel.ForEach(Textures, tex =>
        {
            if (isHash && tex.Hash.Hash32 == parsedHash) // hacky but eh
            {
                displayItems.Add(tex);
            }
            else if ((searchStr == "Cubemap" && tex.ArraySize == 6) || (searchStr == "Volume" && tex.Depth > 1)) // also dumb
            {
                displayItems.Add(tex);
            }
            else if (tex.Dimensions.Contains(searchStr, StringComparison.OrdinalIgnoreCase))
            {
                displayItems.Add(tex);
            }
        });

        List<TextureItem> items = displayItems.ToList();

        items = SortByIndex switch
        {
            4 => items.OrderBy(x => x.Hash.Hash32).ToList(),
            3 => items.OrderByDescending(x => x.Hash.Hash32).ToList(),
            2 => items.OrderBy(x => x.Width * x.Height).ToList(),
            1 => items.OrderByDescending(x => x.Width * x.Height).ToList(),
            _ => items
        };

        TextureList.ItemsSource = items;
        UIHelper.ScrollToTop(TextureList);
        BulkExportButton.IsEnabled = items.Count > 0;
    }

    private void Texture_OnClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button) is null)
            return;

        TextureItem item = ((Button)sender).DataContext as TextureItem;
        LoadTexture(item.Hash);
    }

    private void LoadTexture(FileHash fileHash)
    {
        if (fileHash.IsRedacted)
        {
            Log.Error($"Texture {fileHash} is redacted. Can not load.");
            return;
        }

        Texture textureHeader = FileResourcer.Get().GetFile<Texture>(fileHash);
        _isCubemap = textureHeader.IsCubemap();
        _currentDisplayedTexture = fileHash;

        TextureControl.Visibility = textureHeader.IsCubemap() ? Visibility.Hidden : Visibility.Visible;
        CubemapControl.Visibility = !textureHeader.IsCubemap() ? Visibility.Hidden : Visibility.Visible;
        ExportButton.IsEnabled = true;

        if (textureHeader.IsCubemap())
        {
            CubemapControl.LoadCubemap(textureHeader);
        }
        else
        {
            TextureControl.CurrentSlice = 0;
            TextureControl.LoadTexture(textureHeader);
        }
    }

    private void TextureSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshTextureList();
    }

    private void SortBy_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SortByIndex = (int)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        RefreshTextureList();
    }

    private void Presets_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string preset = (string)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        TextureSearchBox.Text = preset;
    }

    private async void BulkExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (TextureList.ItemsSource is not IEnumerable<TextureItem> items || !items.Any())
            return;

        // Hopefully this works fine, and not just for me
        MainWindow.Progress.SetProgressStages(items.Select((x, i) => $"Exporting {i + 1}/{items.Count()}: {x.Hash}").ToList());
        await Task.Run(() =>
        {
            Parallel.ForEach(items, item =>
            {
                TextureExporter.ExportTexture(item.Hash);
                MainWindow.Progress.CompleteStage();
            });
        });

        string pkgName = PackageResourcer.Get().GetPackage(items.First().Hash.PackageId).GetPackageMetadata().Name.Split(".")[0];
        string savePath = Config.GetExportSavePath() + $"/Textures/{pkgName}";
        NotificationBanner notify = new()
        {
            Icon = "☑️",
            Title = "Bulk Export Complete",
            Description = $"Exported {items.Count()} textures to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        };
        notify.Show();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentDisplayedTexture is null)
            return;

        FileHash hash = _currentDisplayedTexture;
        if (_isCubemap)
            CubemapControl.ExportCurrent();
        else
            TextureControl.ExportCurrent();

        string pkgName = PackageResourcer.Get().GetPackage(hash.PackageId).GetPackageMetadata().Name.Split(".")[0];
        string savePath = Config.GetExportSavePath() + $"/Textures/{pkgName}";
        NotificationBanner notify = new()
        {
            Icon = "☑️",
            Title = "Export Complete",
            Description = $"Exported {hash} to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        };
        notify.Show();
    }

    private void ExportButtons_MouseEnter(object sender, MouseEventArgs e)
    {
    }

    public void ExportButtons_MouseLeave(object sender, MouseEventArgs e)
    {
    }

    private async void TagImage_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.DataContext is TextureItem tag)
        {
            //Console.WriteLine($"Loaded {tag.Hash}");
            await tag.LoadTagImageAsync();
            img.Tag = tag;
        }
    }

    private void TagImage_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.Tag is TextureItem tag)
        {
            tag.ClearImageSource();
            //img.Source = null;
        }
    }

    private class TextureItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public FileHash Hash { get; set; }
        public string Dimensions { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
        public int ArraySize { get; set; }

        private ImageSource _tagImageSource;
        public ImageSource TagImageSource
        {
            get => _tagImageSource;
            private set
            {
                _tagImageSource = value;
                OnPropertyChanged(nameof(TagImageSource));
            }
        }

        public async Task LoadTagImageAsync()
        {
            if (Hash == null || TagImageSource != null)
                return;

            Texture texture = await FileResourcer.Get().GetFileAsync<Texture>(Hash, shouldCache: true);
            if (texture == null)
                return;

            ImageSource image = await Task.Run(() => TextureLoader.LoadTexture(texture, 96, 96));

            if (image != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TagImageSource = image;
                });
            }
        }

        public void ClearImageSource()
        {
            TagImageSource = null;
        }
    }
}


