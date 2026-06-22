using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tiger;
using Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;
using Tiger.Schema.Audio;
using Tiger.Schema.Entity;

namespace Charm;

public partial class MusicWemsControl : UserControl
{
    public MusicWemsControl()
    {
        InitializeComponent();
    }

    private WemItem _currentWem;

    private ConcurrentBag<WemItem> GetWemItems(WwiseSound tag)
    {
        if (tag is null)
            return new ConcurrentBag<WemItem>();

        var items = new ConcurrentBag<WemItem>();
        Parallel.ForEach(tag.TagData.Wems, wem =>
        {
            items.Add(new WemItem
            {
                Name = wem.Hash,
                Hash = wem.Hash.GetReferenceHash(),
                Duration = wem.Duration,
                Wem = wem,
            });
        });

        return items;
    }

    private void Play_OnClick(object sender, RoutedEventArgs e)
    {
        WemItem item = (WemItem)(sender as Button).DataContext;
        PlayWem(item);
    }

    public void PlayWem(WemItem wem)
    {
        if (MusicPlayer.SetWem(wem.Wem))
        {
            MusicPlayer.Play();
            _currentWem = wem;
        }
    }

    public WemItem GetWem()
    {
        return _currentWem;
    }

    public void Load(S808045F5 res)
    {
        WwiseSound loop = res.MusicLoopSound;
        WemList.ItemsSource = GetWemItems(loop).OrderByDescending(x => x.Wem.GetDuration());
    }

    public void Load(List<SSequenceAudioEvent> res)
    {
        var sounds = new ConcurrentBag<WemItem>(
            res.SelectMany(x => GetWemItems(x.Sound))
        );

        WemList.ItemsSource = sounds.OrderByDescending(x => x.Wem.GetDuration());
    }

    public void Load(List<WwiseSound> res)
    {
        var sounds = new ConcurrentBag<WemItem>(
            res.SelectMany(x => GetWemItems(x))
        );

        WemList.ItemsSource = sounds.OrderByDescending(x => x.Wem.GetDuration());
    }

    public void Load(WwiseSound res)
    {
        var sounds = new ConcurrentBag<WemItem>(GetWemItems(res));
        WemList.ItemsSource = sounds.OrderByDescending(x => x.Wem.GetDuration());
    }

    public async void Load(S808045F7 res)
    {
        if (res.AmbientMusicSet == null)
            return;
        // ambient_music_set instead of wwise_loop
        MainWindow.Progress.SetProgressStages(res.AmbientMusicSet.TagData.Unk08.Select((x, i) => $"Loading ambient music {i + 1}/{res.AmbientMusicSet.TagData.Unk08.Count}").ToList());

        ConcurrentBag<WemItem> wemItems = new();
        await Task.Run(() =>
        {
            Parallel.ForEach(res.AmbientMusicSet.TagData.Unk08, entry =>
            {
                ConcurrentBag<WemItem> items = GetWemItems(entry.MusicLoopSound);
                foreach (WemItem wemItem in items)
                {
                    wemItem.Name += $" (Ambient group {entry.MusicLoopSound.Hash})";
                    wemItems.Add(wemItem);
                }
                MainWindow.Progress.CompleteStage();
            });
        });

        WemList.ItemsSource = wemItems.OrderByDescending(x => x.Wem.GetDuration());
    }

    public void Export()
    {
        List<WemItem> wemItems = new();
        Dispatcher.Invoke(() =>
        {
            WemItem? wemItem = GetWem();
            if ((wemItem is null && !ExportAll.IsChecked.Value) || WemList.Items.Count == 0)
            {
                MessageBox.Show("Nothing selected to export");
                return;
            }
            wemItems.Add(wemItem);
            if (ExportAll.IsChecked.Value)
                wemItems = WemList.Items.Cast<WemItem>().ToList();

            List<string> stages = wemItems.Select((x, i) => $"Exporting {x.Hash}_{x.Name} ({i + 1}/{wemItems.Count})").ToList();
            MainWindow.Progress.SetProgressStages(stages);

            if (MusicPlayer.IsPlaying())
                MusicPlayer.Pause();
        });

        string saveDirectory = $"{ConfigSubsystem.Get().GetExportSavePath()}/Sound/Music";
        Directory.CreateDirectory(saveDirectory);
        wemItems.ForEach(wemItem =>
        {
            wemItem.Wem.SaveToFile($"{saveDirectory}/{wemItem.Hash}_{wemItem.Name}.wav");
            MainWindow.Progress.CompleteStage();
        });
    }
}

public class WemItem
{
    public string Name { get; set; }
    public string Duration { get; set; }
    public string Hash { get; set; }
    public Wem Wem { get; set; }
}
