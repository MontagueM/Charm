using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Arithmic;
using Charm.Shared;
using ConcurrentCollections;
using NAudio.Wave;
using Restless.WaveForm.Renderer;
using Restless.WaveForm.Settings;
using Tiger;
using Tiger.Schema.Audio;
using static Charm.PackageList;

namespace Charm;

// TODO? Forward and Back buttons for playback, arrow keys are good enough for now

public partial class AudioListView : UserControl
{
    private static SineSettings _sinePreviewSettings = SineSettings.CreatePreview();
    private static SineSettings _sineExportSettings = SineSettings.CreateExport();
    private ConfigSubsystem Config = TigerInstance.GetSubsystem<ConfigSubsystem>();

    private ConcurrentBag<AudioItem> Sounds = new();

    private int SortByIndex = 5;
    private PackageItem _currentPkg;
    private Wem _currentSound;
    private WaveStream _currentSoundStream;

    public AudioListViewType _loadType = AudioListViewType.Sounds;

    public AudioListView(AudioListViewType loadType = AudioListViewType.Sounds)
    {
        InitializeComponent();
#if DEBUG
        // I can't be asked to fix these seemingly harmless but lag inducing xaml binding errors
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif
        _loadType = loadType;


        PackageList.PackageItemChecked += async (s, item) =>
        {
            MainWindow.Progress.SetProgressStage($"Loading Audio From {item.Name}");
            await Task.Run(() => LoadAudioList(item));
            MainWindow.Progress.CompleteStage();
        };

        if (_loadType == AudioListViewType.SoundBanks)
            PackageList.OnSearchBarChanged += (s, e) => RefreshSoundbankList();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        MusicPlayer.ProgressBar.ValueChanged -= (s, e) => UpdateWaveformProgress();
        MusicPlayer.ProgressBar.ValueChanged += (s, e) => UpdateWaveformProgress();
    }

    public async void LoadContent()
    {
        MainWindow.Progress.SetProgressStage(_loadType == AudioListViewType.Sounds ? "Creating Audio List" : "Loading Soundbanks");
        if (_loadType == AudioListViewType.Sounds)
            await PackageList.MakePackageItems<Wem>();
        else
            await MakeSoundbanks();

        MainWindow.Progress.CompleteStage();

        CreateFilterOptions();
    }

    private async Task MakeSoundbanks()
    {
        if (PackageList.PackageItems != null)
            return;

        Stopwatch stopwatch = Stopwatch.StartNew();
        await Task.Run(() =>
        {
            PackageList.PackageItems = new();

            var hashes = PackageResourcer.Get().GetAllHashes<WwiseSound>();

            stopwatch.Stop();
            Log.Debug($"Stage 1: Getting all WwiseSound Tags took {stopwatch.Elapsed.TotalSeconds} seconds to process. ({hashes.Count})");
            stopwatch = Stopwatch.StartNew();

            Parallel.ForEach(hashes, hash =>
            {
                var bank = FileResourcer.Get().GetFile<WwiseSound>(hash, true, false);
                if (bank.TagData.Wems.Count > 0)
                {
                    string name = bank.TagData.GetSoundbank().GetNameFromBank();
                    if (name == string.Empty || name is null)
                        name = bank.TagData.GetSoundbank().Hash;

                    PackageList.PackageItems.Add(new PackageItem
                    {
                        Name = name,
                        ID = bank.Hash.PackageId,
                        Count = bank.TagData.Wems.Count,
                        Hashes = new ConcurrentHashSet<FileHash>(bank.TagData.Wems.Where(x => x != null).Select(x => x.Hash)),
                        Content = PackageItemContents.Sounds
                    });
                }
            });

            stopwatch.Stop();
            Log.Debug($"Stage 2: Creating all Bank entries took {stopwatch.Elapsed.TotalSeconds} seconds to process. ({PackageList.PackageItems.Count})");
            stopwatch = Stopwatch.StartNew();
        });


        RefreshSoundbankList();

        stopwatch.Stop();
        Log.Debug($"Stage 3: Refreshing List took {stopwatch.Elapsed.TotalSeconds} seconds to process.");
    }

    private void CreateFilterOptions()
    {
        ComboBoxControl sortBy = new();
        sortBy.Text = "Sort By";
        sortBy.TextFontSize = 16;
        sortBy.Box.MinWidth = 175;
        sortBy.Box.ItemsSource = new List<ComboBoxItem>()
        {
            new() { Content = "Hash ↓", Tag = 5 },
            new() { Content = "Hash ↑", Tag = 4 },
            new() { Content = "Duration ↓", Tag = 3 },
            new() { Content = "Duration ↑", Tag = 2 },
            new() { Content = "Channels ↓", Tag = 1 }
        };
        if (sortBy.Box.SelectedIndex == -1)
        {
            sortBy.Box.SelectedIndex = 0;
        }

        sortBy.Box.SelectionChanged += SortBy_OnSelectionChanged;
        FilterOptions.Children.Add(sortBy);
    }

    private async Task LoadAudioList(PackageItem item)
    {
        if (Sounds.Count != 0)
            Sounds.Clear();

        _currentPkg = item;
        await Task.Run(() => Parallel.ForEachAsync(item.Hashes, async (hash, ct) =>
        {
            if (hash.GetReferenceHash().IsInvalid())
                return;

            AudioItem item = new()
            {
                Hash = hash,
                Index = hash.FileIndex,
                DisplayHash = $"[{hash}]",
                DisplayID = hash.GetReferenceHash(),
            };
            await item.LoadWEMAsync();

            Sounds.Add(item);
        }));

        RefreshSoundList();
    }

    private void RefreshSoundList()
    {
        if (Sounds == null)
            return;
        if (Sounds.IsEmpty)
            return;

        Dispatcher.Invoke(() =>
        {
            string searchStr = AudioSearchBox.Text;

            uint parsedHash = 0;
            bool isHash = Helpers.ParseHash(searchStr, out parsedHash);

            var displayItems = new ConcurrentBag<AudioItem>();
            Parallel.ForEach(Sounds, sound =>
            {
                if ((isHash && sound.Hash.Hash32 == parsedHash)
                || sound.Hash.GetReferenceHash().ToString().Contains(searchStr, StringComparison.OrdinalIgnoreCase)
                || sound.Hash.ToString().Contains(searchStr, StringComparison.OrdinalIgnoreCase))
                {
                    displayItems.Add(sound);
                }
            });

            List<AudioItem> items = displayItems.ToList();

            items = SortByIndex switch
            {
                5 => items.OrderByDescending(x => x.Hash).ToList(),
                4 => items.OrderBy(x => x.Hash).ToList(),
                3 => items.OrderByDescending(x => x.Seconds).ToList(),
                2 => items.OrderBy(x => x.Seconds).ToList(),
                1 => items.OrderByDescending(x => x.Channels).ToList(),
                _ => items
            };

            AudioList.ItemsSource = items;
            UIHelper.ScrollToTop(AudioList);
            BulkExportButton.IsEnabled = items.Count > 0;
        });
    }

    public void RefreshSoundbankList()
    {
        if (PackageList.PackageItems == null)
            return;
        if (PackageList.PackageItems.IsEmpty)
            return;

        string searchStr = PackageList.SearchBox.Text;
        var displayItems = new ConcurrentBag<PackageItem>();
        Parallel.ForEach(PackageList.PackageItems, pkg =>
        {
            if (pkg.Name.Contains(searchStr, StringComparison.InvariantCultureIgnoreCase))
            {
                displayItems.Add(pkg);
            }
        });

        List<PackageItem> items = displayItems.OrderBy(x => x.Name).ToList();
        Dispatcher.Invoke(() =>
        {
            PackageList.PackageListView.ItemsSource = items;
        });
    }

    private void Audio_OnClick(object sender, RoutedEventArgs e)
    {
        if ((sender as RadioButton) is null)
            return;

        AudioItem item = ((RadioButton)sender).DataContext as AudioItem;
        LoadSound(item.Hash);
    }

    private void LoadSound(FileHash hash)
    {
        Wem wem = FileResourcer.Get().GetFile<Wem>(hash, true, false);
        _currentSound = wem;

        if (MusicPlayer.SetWem(wem))
        {
            _currentSoundStream = wem.Clone();
            MusicPlayer.Play();
            DrawWaveform();
            Log.Verbose($"Playing {wem.Hash}");
        }

        ExportButton.IsEnabled = true;
        ExportWaveform.IsEnabled = true;
        //_currentSoundStream?.Dispose();
    }

    private void AudioSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshSoundList();
    }

    private void SortBy_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SortByIndex = (int)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        RefreshSoundList();
    }

    private void Presets_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string preset = (string)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        AudioSearchBox.Text = preset;
    }

    private async void BulkExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (AudioList.ItemsSource is not IEnumerable<AudioItem> items || !items.Any())
            return;

        //string pkgName = PackageResourcer.Get().GetPackage(items.First().Hash.PackageId).GetPackageMetadata().Name.Split(".")[0];
        string pkgName = _currentPkg.Name;
        string savePath = Config.GetExportSavePath() + $"/Sound/{pkgName}";
        Directory.CreateDirectory(savePath);

        // Hopefully this works fine, and not just for me
        MainWindow.Progress.SetProgressStages(items.Select((x, i) => $"Exporting {i + 1}/{items.Count()}: {x.Hash}").ToList());
        await Task.Run(() =>
        {
            Parallel.ForEach(items, item =>
            {
                Wem wem = FileResourcer.Get().GetFile<Wem>(item.Hash, false, false);
                wem.SaveToFile($"{savePath}/{wem.GetReferenceHash()}_{wem.Hash}.wav");
                MainWindow.Progress.CompleteStage();
            });
        });

        NotificationBanner notify = new()
        {
            Icon = "☑️",
            Title = "Bulk Export Complete",
            Description = $"Exported {items.Count()} Sounds to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        };
        notify.Show();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSound is null)
            return;

        Wem wem = _currentSound;

        string pkgName = _currentPkg.Name;

        string savePath = Config.GetExportSavePath() + $"/Sound/{pkgName}";
        Directory.CreateDirectory(savePath);

        wem.SaveToFile($"{savePath}/{wem.GetReferenceHash()}_{wem.Hash}.wav");

        NotificationBanner notify = new()
        {
            Icon = "☑️",
            Title = "Export Complete",
            Description = $"Exported {wem.Hash} to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        };
        notify.Show();
    }

    private void ExportWaveform_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSound is null)
            return;

        string pkgName = PackageResourcer.Get()
            .GetPackage(_currentSound.Hash.PackageId)
            .GetPackageMetadata()
            .Name.Split(".")[0];

        string savePath = Path.Combine(Config.GetExportSavePath(), "Sound", pkgName);
        Directory.CreateDirectory(savePath);

        _currentSound.Load();
        using var stream = _currentSoundStream;
        var wave = WaveFormRenderer.Create(stream, _sineExportSettings);

        // Overlay Right and Left
        using var combined = new Bitmap(wave.ImageLeft.Width, wave.ImageLeft.Height);
        using (var g = Graphics.FromImage(combined))
        {
            g.DrawImage(wave.ImageLeft, 0, 0);
            g.DrawImage(wave.ImageRight, 0, 0);
        }

        string saveFile = Path.Combine(savePath, $"{_currentSound.Hash}_Waveform.png");
        combined.Save(saveFile, ImageFormat.Png);

        new NotificationBanner
        {
            Icon = "☑️",
            Title = "Export Complete",
            Description = $"Exported Waveform to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        }.Show();
    }

    private void ExportButtons_MouseEnter(object sender, MouseEventArgs e)
    {
    }

    public void ExportButtons_MouseLeave(object sender, MouseEventArgs e)
    {
    }


    private CancellationTokenSource _audioSelectionCts;
    private async void AudioList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _audioSelectionCts?.Cancel();
        _audioSelectionCts = new CancellationTokenSource();
        var token = _audioSelectionCts.Token;

        try
        {
            await Task.Delay(100, token); // Debounce time
            if (token.IsCancellationRequested)
                return;

            Dispatcher.Invoke(() =>
            {
                if (AudioList.SelectedIndex >= 0)
                {
                    var container = AudioList.ItemContainerGenerator.ContainerFromIndex(AudioList.SelectedIndex);
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

    private async void DrawWaveform()
    {
        await Task.Run(() =>
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Waveform.Source = null;
                WaveformLoading.Visibility = Visibility.Visible;
            });
            if (_currentSound is null || _currentSound.Channels > 4)
                return;

            //using var stream = _currentSound.WemReaderClone;
            var wave = WaveFormRenderer.Create(_currentSoundStream, _sinePreviewSettings);

            // Overlay Right and Left
            using var combined = new Bitmap(wave.ImageLeft.Width, wave.ImageLeft.Height);
            using (var g = Graphics.FromImage(combined))
            {
                g.DrawImage(wave.ImageLeft, 0, 0);
                g.DrawImage(wave.ImageRight, 0, 0);
            }

            using var memory = new MemoryStream();
            combined.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = ApiImageUtils.MakeBitmapImage(memory, wave.ImageLeft.Width, wave.ImageLeft.Height);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Waveform.Source = bitmapImage;
                WaveformLoading.Visibility = Visibility.Collapsed;
            });
        });
    }

    private void UpdateWaveformProgress()
    {
        Task.Run(() =>
        {
            if (_currentSound is null)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                double width = Waveform.ActualWidth;
                double height = Waveform.ActualHeight;
                double progress = MusicPlayer.ProgressBar.Value;

                WaveformProgressBar.Width = width;
                WaveformProgressBar.Height = height;

                WaveformTintClip.Rect = new Rect(0, 0, width * progress, height);
            });
        });
    }

    private async void Tag_Loaded(object sender, RoutedEventArgs e)
    {
        //if (sender is Button btn && btn.DataContext is AudioItem tag)
        //{
        //    await tag.DrawWaveform();
        //    btn.Tag = tag;
        //}
    }

    private void Tag_Unloaded(object sender, RoutedEventArgs e)
    {
        //if (sender is Button btn && btn.DataContext is AudioItem tag)
        //{
        //    tag.ClearWaveform();
        //}
    }

    private class AudioItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public FileHash Hash { get; set; }
        public int Index { get; set; }

        private string _displayHash;
        public string DisplayHash
        {
            get => _displayHash;
            set
            {
                _displayHash = value;
                OnPropertyChanged(nameof(DisplayHash));
            }
        }

        private string _displayID;
        public string DisplayID
        {
            get => _displayID;
            set
            {
                _displayID = value;
                OnPropertyChanged(nameof(DisplayID));
            }
        }

        private string _duration;
        public string Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }

        private float _seconds;
        public float Seconds
        {
            get => _seconds;
            set
            {
                _seconds = value;
                OnPropertyChanged(nameof(Seconds));
            }
        }

        private int _channels;
        public int Channels
        {
            get => _channels;
            set
            {
                _channels = value;
                OnPropertyChanged(nameof(Channels));
            }
        }

        private int _sampleRate;
        public int SampleRate
        {
            get => _sampleRate;
            set
            {
                _sampleRate = value;
                OnPropertyChanged(nameof(SampleRate));
            }
        }

        public async Task LoadWEMAsync()
        {
            if (Hash == null)
                return;

            Wem wem = await FileResourcer.Get().GetFileAsync<Wem>(Hash, false, false);
            if (wem == null || wem.GetReferenceHash().IsInvalid())
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                DisplayHash = $"[{Hash}] {(wem.Channels > 2 ? "⚠" : "")}";
                DisplayID = Hash.GetReferenceHash();
                Duration = wem.Duration;
                Seconds = wem.Seconds;
                Channels = wem.Channels;
                SampleRate = wem.SampleRate;
            });
        }

        // TODO Display waveform on item?
        //private ImageSource _waveformSource;
        //public ImageSource WaveformSource
        //{
        //    get => _waveformSource;
        //    private set
        //    {
        //        _waveformSource = value;
        //        OnPropertyChanged(nameof(WaveformSource));
        //    }
        //}

        //public void ClearWaveform()
        //{
        //    WaveformSource = null;
        //}
    }

    private class SineSettings : RenderSettings
    {
        public SineSettings(int width, int height)
        {
            DisplayName = "Sine";
            Width = width;
            Height = height;
            SampleResolution = 8;
            PrimaryLineColor = System.Drawing.Color.White;
            LineThickness = 1f;
            CenterLineColor = System.Drawing.Color.Transparent;
            XStep = 2f;
            VolumeBoost = 1f;
            AutoWidth = false;
        }

        public static SineSettings CreatePreview() => new SineSettings(800, 200);
        public static SineSettings CreateExport() => new SineSettings(4096, 1024);
    }

    public enum AudioListViewType
    {
        Sounds,
        SoundBanks,
    }
}


