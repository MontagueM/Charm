using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Field;
using Field.General;
using NAudio.Vorbis;
using NAudio.Wave;

namespace Charm;

public partial class MusicView : UserControl
{
    public MusicView()
    {
        InitializeComponent();
    }

    public async void Load(Activity activity)
    {
        if (activity.Header.Unk18 is D2Class_6A988080)
        {
            var music = ((D2Class_6A988080) activity.Header.Unk18).Music;
            if (music.Header.Unk28.Count != 1)
            {
                throw new NotImplementedException();
            }

            var resource = music.Header.Unk28[0].Unk00;
            if (resource is D2Class_F5458080)
            {
                WwiseLoop loop = ((D2Class_F5458080) resource).MusicLoopSound;
                MusicLoopName.Text = ((D2Class_F5458080) resource).WwiseMusicLoopName;
                WemList.ItemsSource = GetWemItems(loop);
                EventList.ItemsSource = GetEventItems(((D2Class_F5458080) resource).Unk18);
            }
            else
            {
                if (resource is not D2Class_F7458080)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

    private ConcurrentBag<WemItem> GetWemItems(WwiseLoop tag)
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

    private void PlayWem_OnClick(object sender, RoutedEventArgs e)
    {
        WemItem item = (WemItem) (sender as Button).DataContext;
        PlayWem(item.Wem);
    }

    public void PlayWem(Wem wem)
    {
        MusicPlayer.SetWem(wem);
        MusicPlayer.Play();
    }
    
    private ConcurrentBag<EventItem> GetEventItems(List<D2Class_FB458080> array)
    {
        var items = new ConcurrentBag<EventItem>();
        Parallel.ForEach(array, entry =>
        {
            items.Add(new EventItem
            {
                Name = entry.EventName,
                Hash = entry.EventHash,
            });
        });

        return items;
    }
}

public class WemItem
{
    public string Name { get; set; }
    public string Duration { get; set; }
    public string Hash { get; set; }
    public Wem Wem { get; set; }
}

public class EventItem
{
    public string Name { get; set; }
    public string Hash { get; set; }
}