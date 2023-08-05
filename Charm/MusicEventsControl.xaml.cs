using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Tiger.Schema.Activity;
using Tiger.Schema.Activity.DESTINY2_WITCHQUEEN_6307;

namespace Charm;

public partial class MusicEventsControl : UserControl
{
    public MusicEventsControl()
    {
        InitializeComponent();
    }

    public void Load(D2Class_F5458080 res)
    {
        return;
        MusicLoopName.Text = res.WwiseMusicLoopName.Value;
        EventList.ItemsSource = GetEventItems(res.Unk18);
    }

    public void Load(D2Class_F7458080 res)
    {
        return;
        MusicLoopName.Text = res.AmbientMusicSetName.Value;
        EventList.ItemsSource = GetEventItems(res.Unk18);
    }

    // both of these are lists to maintain the original order

    private List<EventItem> GetEventItems(List<D2Class_FB458080> array)
    {
        var items = new List<EventItem>();
        foreach (var entry in array)
        {
            items.Add(new EventItem
            {
                Name = entry.EventName.Value,
                Hash = entry.EventHash,
            });
        }

        return items;
    }

    private List<EventItem> GetEventItems(List<D2Class_FA458080> array)
    {
        var items = new List<EventItem>();
        foreach (var entry in array)
        {
            items.Add(new EventItem
            {
                Name = entry.EventName.Value,
                Hash = $"{entry.Unk00}/{entry.Unk10}",
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
