using System.Collections.Concurrent;
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

    public void Load(D2Class_F5458080 res)
    {
        WwiseLoop loop = res.MusicLoopSound;
        WemList.ItemsSource = GetWemItems(loop);
    }
}

public class WemItem
{
    public string Name { get; set; }
    public string Duration { get; set; }
    public string Hash { get; set; }
    public Wem Wem { get; set; }
}