using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Field.General;
using Field.Models;
using Field.Statics;

namespace Charm;

public partial class MapView : UserControl
{
    public StaticMapData StaticMap;
    public TagHash Hash;

    public MapView(TagHash hash)
    {
        InitializeComponent();
        Hash = hash;
    }

    public void LoadMap()
    {
        GetStaticMapData();
    }

    private void GetStaticMapData()
    {
        StaticMap = new StaticMapData(Hash);
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        var displayParts = MakeDisplayParts();
        MVM.SetChildren(displayParts);
    }

    private List<MainViewModel.DisplayPart> MakeDisplayParts()
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();
        Parallel.ForEach(StaticMap.Header.InstanceCounts, c =>
        {
            // inefficiency as sometimes there are two instance count entries with same hash. why? idk
            var model = StaticMap.Header.Statics[c.StaticIndex].Static;
            var parts = model.Load(ELOD.LeastDetail);
            for (int i = c.InstanceOffset; i < c.InstanceOffset + c.InstanceCount; i++)
            {
                foreach (var part in parts)
                {
                    MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
                    displayPart.BasePart = part;
                    displayPart.Translations.Add(StaticMap.Header.Instances[i].Position);
                    displayPart.Rotations.Add(StaticMap.Header.Instances[i].Rotation);
                    displayPart.Scales.Add(StaticMap.Header.Instances[i].Scale);
                    displayParts.Add(displayPart);
                }

            }
        });
        return displayParts.ToList();
    }
}