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
using Arithmic;
using NAudio.Wave;
using Restless.WaveForm.Renderer;
using Restless.WaveForm.Settings;
using Tiger;
using Tiger.Schema.Audio;
using Tiger.Schema.Entity;
using Tiger.Schema.Investment;

namespace Charm;

// All the duplicate code across these views is starting to get out of hand...

public partial class WeaponAudioListView : UserControl
{
    private static SineSettings _sinePreviewSettings = SineSettings.CreatePreview();
    private static SineSettings _sineExportSettings = SineSettings.CreateExport();
    private ConfigSubsystem Config = TigerInstance.GetSubsystem<ConfigSubsystem>();

    public ConcurrentBag<WeaponItem> WeaponItems;
    private ConcurrentBag<WeaponAudioCategory> Sounds = new();

    private WeaponAudioItem _currentSound;
    private WaveStream _currentSoundStream;

    private WeaponItem _currentWeapon;

    public WeaponAudioListView()
    {
        InitializeComponent();
#if DEBUG
        // I can't be asked to fix these seemingly harmless but lag inducing xaml binding errors
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif

    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        MusicPlayer.ProgressBar.ValueChanged -= (s, e) => UpdateWaveformProgress();
        MusicPlayer.ProgressBar.ValueChanged += (s, e) => UpdateWaveformProgress();
    }

    public async void LoadContent()
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Creating Audio List",
        });
        await MakeWeaponItems();
        MainWindow.Progress.CompleteStage();
    }

    public async Task MakeWeaponItems()
    {
        if (WeaponItems != null)
            return;

        WeaponItems = new();

        IEnumerable<InventoryItem> inventoryItems = await Investment.Get().GetInventoryItems();
        Parallel.ForEach(inventoryItems, item =>
        {
            if (item.GetWeaponPatternIndex() == -1)
                return;
            string name = item.Name;
            string type = item.Type;
            if (type == null)
            {
                type = "";
            }
            if (type is "Vehicle" or "Ship" or "Ship Schematics" or "Ghost Shell")
                return;

            WeaponItems.Add(new WeaponItem
            {
                Hash = item.TagData.InventoryItemHash,
                Name = name,
                Rarity = ((DestinyTierType)item.TagData.ItemRarity).ToString(),
                Type = type.Trim(),
            });
        });

        RefreshWeaponList();
    }

    public void RefreshWeaponList()
    {
        if (WeaponItems == null)
            return;
        if (WeaponItems.IsEmpty)
            return;

        string searchStr = SearchBox.Text;
        var displayItems = new ConcurrentBag<WeaponItem>();
        Parallel.ForEach(WeaponItems, item =>
        {
            if (searchStr == item.Hash.Hash32.ToString()
            || item.Name.Contains(searchStr, StringComparison.OrdinalIgnoreCase)
            || item.Rarity.Contains(searchStr, StringComparison.OrdinalIgnoreCase)
            || item.Type.Contains(searchStr, StringComparison.OrdinalIgnoreCase))
                displayItems.Add(item);
        });

        List<WeaponItem> items = displayItems.DistinctBy(x => x.Name).OrderBy(x => x.Name).ToList();
        WeaponListView.ItemsSource = items;
    }

    private void WeaponItem_Checked(object sender, RoutedEventArgs e)
    {
        if ((sender as RadioButton) is null)
            return;

        WeaponItem item = ((RadioButton)sender).DataContext as WeaponItem;
        _currentWeapon = item;
        LoadWeaponAudioList(item.Hash);
    }

    private void RefreshSoundList()
    {
        if (Sounds == null)
            return;
        if (Sounds.IsEmpty)
            return;

        string searchStr = AudioSearchBox.Text;

        uint parsedHash = 0;
        bool isHash = Helpers.ParseHash(searchStr, out parsedHash);

        var displayItems = new ConcurrentBag<WeaponAudioCategory>();
        Parallel.ForEach(Sounds, sound =>
        {
            sound.Sounds = sound.Sounds.OrderBy(x => x.Seconds).ToList();
            displayItems.Add(sound);
        });

        List<WeaponAudioCategory> items = displayItems.OrderBy(x => x.Name).ToList();

        WeaponAudioEntries.ItemsSource = items;
        BulkExportButton.IsEnabled = items.Count > 0;
    }

    private void Audio_OnClick(object sender, RoutedEventArgs e)
    {
        if ((sender as RadioButton) is null)
            return;

        WeaponAudioItem item = ((RadioButton)sender).DataContext as WeaponAudioItem;
        _currentSound = item;

        LoadSound(item.Hash);
    }

    private void LoadSound(FileHash hash)
    {
        Wem wem = FileResourcer.Get().GetFile<Wem>(hash, true, false);

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

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshWeaponList();
    }

    private void SortBy_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSoundList();
    }

    private void Presets_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string preset = (string)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        AudioSearchBox.Text = preset;
    }

    private async void BulkExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (WeaponAudioEntries.ItemsSource is not IEnumerable<WeaponAudioCategory> items || !items.Any())
            return;

        string wepName = _currentWeapon.Name;
        string savePath = Config.GetExportSavePath() + $"/Sound/{wepName}/";
        Directory.CreateDirectory(savePath);

        int totalSounds = items.Sum(x => x.Sounds?.Count ?? 0);
        int currentIndex = 0;

        MainWindow.Progress.SetProgressStages(
            items.SelectMany(category => category.Sounds.Select(sound =>
                $"Exporting {++currentIndex}/{totalSounds}: {category.Name}")).ToList()
        );

        await Task.Run(() =>
        {
            foreach (var category in items)
            {
                string savePath = Config.GetExportSavePath() + $"/Sound/{wepName}/{category.Name}/";
                Directory.CreateDirectory(savePath);

                Parallel.ForEach(category.Sounds, item =>
                {
                    Wem wem = FileResourcer.Get().GetFile<Wem>(item.Hash, false, false);
                    wem.SaveToFile($"{savePath}/{wem.GetReferenceHash()}_{wem.Hash}.wav");
                    MainWindow.Progress.CompleteStage();
                });
            }
        });

        NotificationBanner notify = new()
        {
            Icon = "☑️",
            Title = "Bulk Export Complete",
            Description = $"Exported {totalSounds} Sounds to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        };
        notify.Show();
    }

    private async void BulkExportCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        var datacontext = (UIHelper.GetParentAtDepth(sender as FrameworkElement, 3) as FrameworkElement).DataContext;
        WeaponAudioCategory category = datacontext as WeaponAudioCategory;
        var items = category.Sounds;
        if (items.Count == 0)
            return;

        string wepName = _currentWeapon.Name;
        string savePath = Config.GetExportSavePath() + $"/Sound/{wepName}/{category.Name}/";
        Directory.CreateDirectory(savePath);

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

        Wem wem = FileResourcer.Get().GetFile<Wem>(_currentSound.Hash, false, false);

        string wepName = _currentWeapon.Name;
        string savePath = Config.GetExportSavePath() + $"/Sound/{wepName}/{_currentSound.ParentCategory}/";
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

        Wem wem = FileResourcer.Get().GetFile<Wem>(_currentSound.Hash, false, false);

        string pkgName = PackageResourcer.Get()
            .GetPackage(_currentSound.Hash.PackageId)
            .GetPackageMetadata()
            .Name.Split(".")[0];

        string savePath = Path.Combine(Config.GetExportSavePath(), "Sound", pkgName);
        Directory.CreateDirectory(savePath);

        wem.Load();
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


    #region Audio loading

    // Sword audio 0x18 B6368080, E043EA80 (E143EA80 pattern ent) for testing
    private async void LoadWeaponAudioList(TigerHash apiHash)
    {
        if (Sounds.Count != 0)
            Sounds.Clear();

        Entity? val = Investment.Get().GetPatternEntityFromHash(apiHash);
        if (val == null || (val.PatternAudio == null && val.PatternAudioUnnamed == null))
            return;

        var resourceUnnamed = (SF42C8080)val.PatternAudioUnnamed.GetUnk18();
        var resource = (S6E358080)val.PatternAudio.GetUnk18();

        InventoryItem item = Investment.Get().GetInventoryItem(apiHash);
        TigerHash weaponContentGroupHash = Investment.Get().GetWeaponContentGroupHash(item);

        Log.Verbose($"Loading weapon entity audio {val.Hash}, ContentGroupHash {weaponContentGroupHash}");

        // Named
        Tag<S0D8C8080>? audioGroup = null;
        if (!resource.PatternAudioGroups.Where(x => x.WeaponContentGroup1Hash == weaponContentGroupHash).Any())
        {
            Log.Verbose($"No PatterAudioGroups with matching Content Group Hash {weaponContentGroupHash}, trying fallback audio");
            if (resource.FallbackAudioGroup != null)
            {
                audioGroup = FileResourcer.Get().GetSchemaTag<S0D8C8080>(resource.FallbackAudioGroup.TagData.EntityData);
            }
        }
        else
        {
            foreach (S9B318080 entry in resource.PatternAudioGroups)
            {
                if (entry.WeaponContentGroup1Hash.Equals(weaponContentGroupHash) && entry.AudioGroup != null)
                {
                    audioGroup = FileResourcer.Get().GetSchemaTag<S0D8C8080>(entry.AudioGroup.TagData.EntityData);
                }
            }
        }

        if (audioGroup != null)
        {
            foreach (var audio in audioGroup.TagData.Audio)
            {
                foreach (S138C8080 s in audio.Sounds)
                {
                    WwiseSound categorySounds = FileResourcer.Get().GetFile<WwiseSound>(s.Data);
                    if (categorySounds == null)
                        continue;

                    WeaponAudioCategory category = new()
                    {
                        Name = s.WwiseEventName.Value?.Split("\\").Last().Split(".")[0] ?? "",
                        Sounds = new()
                    };
                    foreach (var sound in categorySounds.TagData.Wems)
                    {
                        if (sound is null)
                            continue;

                        WeaponAudioItem soundItem = new()
                        {
                            ParentCategory = category.Name,
                            Hash = sound.Hash,
                            DisplayHash = $"[{sound.Hash}]"
                        };
                        await soundItem.LoadWEMAsync();

                        category.Sounds.Add(soundItem);
                    }

                    Sounds.Add(category);
                }
            }
        }

        // Unnamed
        List<WwiseSound> sounds = GetWeaponUnnamedSounds(resourceUnnamed, weaponContentGroupHash, val.PatternAudioUnnamed.Reader);
        foreach (WwiseSound categorySounds in sounds)
        {
            if (categorySounds == null)
                continue;

            if (categorySounds.Hash.GetReferenceHash().IsInvalid())
                return;

            string name = categorySounds.TagData.GetSoundbank().GetNameFromBank();
            WeaponAudioCategory category = new()
            {
                Name = name == "" ? categorySounds.Hash : name,
                Sounds = new()
            };
            foreach (var sound in categorySounds.TagData.Wems)
            {
                if (sound is null)
                    continue;

                WeaponAudioItem soundItem = new()
                {
                    ParentCategory = category.Name,
                    Hash = sound.Hash,
                    DisplayHash = $"[{sound.Hash}]"
                };
                await soundItem.LoadWEMAsync();

                category.Sounds.Add(soundItem);
            }

            Sounds.Add(category);
        }

        RefreshSoundList();
    }

    public List<WwiseSound> GetWeaponUnnamedSounds(SF42C8080 resource, TigerHash weaponContentGroupHash, TigerReader reader)
    {
        List<WwiseSound> sounds = new();
        List<Entity> entities = new();

        if (!resource.PatternAudioGroups.Where(x => x.WeaponContentGroupHash == weaponContentGroupHash).Any())
        {
            Log.Verbose($"No unnamed PatterAudioGroups with matching Content Group Hash {weaponContentGroupHash}, trying fallback audio");
            if (resource.FallbackAudio1 != null)
                entities.Add(resource.FallbackAudio1);
            if (resource.FallbackAudio2 != null)
                entities.Add(resource.FallbackAudio2);
            if (resource.FallbackAudio3 != null)
                entities.Add(resource.FallbackAudio3);
        }
        else
        {
            resource.PatternAudioGroups.ForEach(entry =>
            {
                if (!entry.WeaponContentGroupHash.Equals(weaponContentGroupHash))
                    return;

                List<TigerFile> entitiesParents = new() { entry.Unk60, entry.Unk78, entry.Unk90, entry.UnkA8, entry.UnkC0, entry.UnkD8, entry.AudioEntityParent, entry.Unk130, entry.Unk148, entry.Unk1C0, entry.Unk1D8, entry.Unk248 };

                if (entry.Unk118.GetValue(reader) is S0A2D8080 or S40238080)
                {
                    dynamic resourceUnk118 = Strategy.IsD1() ? (S40238080)entry.Unk118.GetValue(reader) : (S0A2D8080)entry.Unk118.GetValue(reader);
                    if (resourceUnk118.Unk08 != null)
                        entities.Add(resourceUnk118.Unk08);
                    if (resourceUnk118.Unk20 != null)
                        entities.Add(resourceUnk118.Unk20);
                    if (resourceUnk118.Unk38 != null)
                        entities.Add(resourceUnk118.Unk38);
                }

                foreach (TigerFile tag in entitiesParents)
                {
                    if (tag == null)
                        continue;

                    FileHash? reference = Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON ? tag.Hash.GetReferenceHash() : tag.Hash.GetReferenceFromManifest();
                    if (reference == 0x80806fa3 || reference == 0x80803463)
                    {
                        FileHash entityData = FileResourcer.Get().GetSchemaTag<SA36F8080>(tag.Hash).TagData.EntityData;
                        FileHash reference2 = entityData.GetReferenceHash();
                        if (reference2 == 0x80802d09 || reference2 == 0x80803165)
                        {
                            if (Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
                            {
                                Tag<S092D8080> tagInner = FileResourcer.Get().GetSchemaTag<S092D8080>(entityData);
                                if (tagInner.TagData.Unk18 != null)
                                    entities.Add(tagInner.TagData.Unk18);
                                if (tagInner.TagData.Unk30 != null)
                                    entities.Add(tagInner.TagData.Unk30);
                                if (tagInner.TagData.Unk48 != null)
                                    entities.Add(tagInner.TagData.Unk48);
                                if (tagInner.TagData.Unk60 != null)
                                    entities.Add(tagInner.TagData.Unk60);
                                if (tagInner.TagData.Unk78 != null)
                                    entities.Add(tagInner.TagData.Unk78);
                                if (tagInner.TagData.Unk90 != null)
                                    entities.Add(tagInner.TagData.Unk90);
                            }
                            else
                            {
                                // These have tag paths but getting the names from the soundbank is better (93% of the time)
                                Tag<S65318080> tagInner = FileResourcer.Get().GetSchemaTag<S65318080>(entityData);
                                if (tagInner.TagData.Entity1 != null)
                                    entities.Add(tagInner.TagData.Entity1);
                                if (tagInner.TagData.Entity2 != null)
                                    entities.Add(tagInner.TagData.Entity2);
                                if (tagInner.TagData.Entity3 != null)
                                    entities.Add(tagInner.TagData.Entity3);
                                if (tagInner.TagData.Entity4 != null)
                                    entities.Add(tagInner.TagData.Entity4);
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else if (reference == 0x80809ad8)
                    {
                        entities.Add(FileResourcer.Get().GetFile<Entity>(tag.Hash));
                    }
                    else if (reference != 0x8080325a)  // 0x8080325a materials,
                    {
                        throw new NotImplementedException();
                    }
                }
            });
        }

        foreach (Entity entity in entities)
        {
            foreach (FileHash? resourceHash in entity.Components)
            {
                if (Strategy.IsD1() && resourceHash.GetReferenceHash() != 0x80800861)
                    continue;

                EntityComponent e = FileResourcer.Get().GetFile<EntityComponent>(resourceHash);
                if (e.TagData.Unk18.GetValue(e.GetReader()) is S79818080 a)
                {
                    var arrays = a.Array1;
                    arrays.AddRange(a.Array2);
                    if (Strategy.IsD1())
                        arrays.AddRange(a.D1Array3);

                    foreach (SF1918080 d2ClassF1918080 in arrays)
                    {
                        if (d2ClassF1918080.Unk10.GetValue(e.GetReader()) is SSequenceAudioEvent b)
                        {
                            sounds.Add(b.Sound);
                        }
                    }
                }
            }
        }
        return sounds;
    }
    #endregion


    private CancellationTokenSource _audioSelectionCts;
    private async void AudioList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var list = sender as ListView;
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
                if (list.SelectedIndex >= 0)
                {
                    var container = list.ItemContainerGenerator.ContainerFromIndex(list.SelectedIndex);
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
            if (_currentSoundStream is null || _currentSound.Channels > 4)
                return;

            // Somethings up with the wave stream somehow becoming null? Idfk whats going on
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

    public class WeaponItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public TigerHash Hash { get; set; }
        public bool IsSelected { get; set; } = false;
    }

    public class WeaponAudioCategory
    {
        public string Name { get; set; }
        public List<WeaponAudioItem> Sounds { get; set; }
    }

    public class WeaponAudioItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public FileHash Hash { get; set; }

        public string ParentCategory { get; set; }

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
                Duration = wem.Duration;
                Seconds = wem.Seconds;
                Channels = wem.Channels;
                SampleRate = wem.SampleRate;
            });
        }
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
}


