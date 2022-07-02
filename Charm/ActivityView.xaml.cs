using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Field;
using Field.General;

namespace Charm;

public partial class ActivityView : UserControl
{
    private Activity _activity;
    
    public ActivityView()
    {
        InitializeComponent();
    } 
    public string LoadActivity(TagHash hash)
    {
        _activity = new Activity(hash);
        LoadUI();
        return PackageHandler.GetActivityName(hash);
    }

    private void LoadUI()
    {
        MapList.ItemsSource = GetMapList();
    }

    private ObservableCollection<DisplayMap> GetMapList()
    {
        var maps = new ObservableCollection<DisplayMap>();
        foreach (var mapEntry in _activity.Header.Unk50)
        {
            DisplayMap displayMap = new DisplayMap();
            // mapEntry.
            displayMap.Name = mapEntry.Unk10.BubbleName;  // assuming Unk10 is 0F978080
            // only first is wrong, but for now anyway - actually maybe do the largest one?
            var mapresourcelist = mapEntry.MapReference.Header.ChildMapReference.Header.MapResources;
            string mapHash = "";
            long largestSize = 0;
            foreach (var d2Class03878080 in mapresourcelist)
            {
                if (d2Class03878080.MapResource.Header.DataTables.Count > 1)
                {
                    if (d2Class03878080.MapResource.Header.DataTables[1].DataTable.Header.DataEntries.Count > 0)
                    {
                        StaticMapData tag = d2Class03878080.MapResource.Header.DataTables[1].DataTable.Header.DataEntries[0].DataResource.StaticMapParent.Header.StaticMap;
                        if (tag.Header.FileSize > largestSize)
                        {
                            largestSize = tag.Header.FileSize;
                            mapHash = tag.Hash;
                        } 
                    }
                }
            }

            displayMap.Hash = mapHash;
            maps.Add(displayMap);
        }
        return maps;
    }

    private void DisplayMapButton_OnClick(object sender, RoutedEventArgs e)
    {
        MainMenuView.AddWindow(new TagHash((sender as Button).Tag as string));
    }
}

public class DisplayMap
{
    private string name;
    private string hash;

    public string Name
    {
        get => name;
        set => name = value;
    }
    
    public string Hash
    {
        get => hash;
        set => hash = value;
    }
}