using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using Tiger;
using Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;

namespace Charm;

public partial class MusicEventsControl : UserControl
{
    public MusicEventsControl()
    {
        InitializeComponent();
    }

    public void Load(S808045F5 res)
    {
        MusicLoopName.Text = res.WwiseMusicLoopName?.Value;
        EventList.ItemsSource = GetEventItems(res.Unk18);
    }

    public void Load(S808045F7 res)
    {
        MusicLoopName.Text = res.AmbientMusicSetName?.Value;
        EventList.ItemsSource = GetEventItems(res.Unk18);
    }


    public void Load(SUnkMusicE6BF8080 rese6Bf, string name)
    {
        MusicLoopName.Text = name;
        EventList.ItemsSource = GetEventItems(rese6Bf.Unk28);
    }

    private IEnumerable GetEventItems(DynamicArray<SUnkMusicE8BF8080> array)
    {
        var items = new List<EventItem>();
        foreach (SUnkMusicE8BF8080 entry in array)
        {
            items.Add(new EventItem
            {
                Name = entry.EventDescription?.Value,
                Hash = entry.EventHash,
            });
        }

        return items;
    }

    // both of these are lists to maintain the original order

    private List<EventItem> GetEventItems(List<S808045FB> array)
    {
        var items = new List<EventItem>();
        foreach (S808045FB entry in array)
        {
            items.Add(new EventItem
            {
                Name = entry.EventName?.Value,
                Hash = entry.EventHash,
            });
        }

        return items;
    }

    private List<EventItem> GetEventItems(List<S808045FA> array)
    {
        var items = new List<EventItem>();
        foreach (S808045FA entry in array)
        {
            items.Add(new EventItem
            {
                Name = entry.EventName?.Value,
                Hash = entry.EventHash,
            });
        }

        return items;
    }
}

public class EventItem
{
    public string Name { get; set; }
    public string Hash { get; set; }
}
