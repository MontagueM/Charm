using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Field;
using Field.General;

namespace Charm;

public partial class MusicWemsControl : UserControl
{
    public MusicWemsControl()
    {
        InitializeComponent();
    }
    
    private ConcurrentBag<WemItem> GetWemItems(WwiseSound tag)
    {
        var items = new ConcurrentBag<WemItem>();
        Parallel.ForEach(tag.Header.Unk20, wem =>
        {
            items.Add(new WemItem
            {
                Name = wem.Hash,
                Hash = PackageHandler.GetEntryReference(wem.Hash),
                Duration = wem.Duration,
                Wem = wem,
            });
        });

        return items;
    }
    
    private void Play_OnClick(object sender, RoutedEventArgs e)
    {
        WemItem item = (WemItem) (sender as Button).DataContext;
        PlayWem(item.Wem);
    }

    public void PlaySound(WwiseSound sound)
    {
        MusicPlayer.SetSound(sound);
        MusicPlayer.Play();
    }
    
    public void PlayWem(Wem wem)
    {
        if (MusicPlayer.SetWem(wem))
            MusicPlayer.Play();
    }

    public void Load(D2Class_F5458080 res)
    {
        WwiseSound loop = res.MusicLoopSound;
        WemList.ItemsSource = GetWemItems(loop);
    }
    
    public async void Load(D2Class_F7458080 res)
    {
        if (res.AmbientMusicSet == null)
            return;
        // ambient_music_set instead of wwise_loop
        MainWindow.Progress.SetProgressStages(res.AmbientMusicSet.Header.Unk08.Select((x,i) => $"Loading ambient music {i+1}/{res.AmbientMusicSet.Header.Unk08.Count}").ToList());
        
        ConcurrentBag<WemItem> wemItems = new ConcurrentBag<WemItem>();
        await Task.Run(() =>
        {
            Parallel.ForEach(res.AmbientMusicSet.Header.Unk08, entry =>
            {
                var items = GetWemItems(entry.MusicLoopSound);
                foreach (var wemItem in items)
                {
                    wemItem.Name += $" (Ambient group {entry.MusicLoopSound.Hash})";
                    wemItems.Add(wemItem);
                }
                MainWindow.Progress.CompleteStage();
            });
        });

        WemList.ItemsSource = wemItems;
    }
}

public class WemItem
{
    public string Name { get; set; }
    public string Duration { get; set; }
    public string Hash { get; set; }
    public Wem Wem { get; set; }
}