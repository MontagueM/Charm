using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        ExportFullMap();  // 8CB6B580 bazaar, 17A7B580 annex, 933ED580 courtyard
    }
    
    private void ExportFullMap()
    {
        InfoConfigHandler.MakeFile();
        string meshName = Hash.GetHashString();
        string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
        if (ConfigHandler.GetSingleFolderMapsEnabled())
        {
            savePath = ConfigHandler.GetExportSavePath() + "/Maps";
        }
        InfoConfigHandler.SetMeshName(meshName);
        Directory.CreateDirectory(savePath);
        // Extract all
        List<D2Class_BD938080> extractedStatics = StaticMap.Header.Statics.DistinctBy(x => x.Static.Hash).ToList();
        // Parallel.ForEach(extractedStatics, s =>
        // {
        //     var parts = s.Static.Load(ELOD.MostDetail);
        //     FbxHandler.AddStaticToScene(parts);
        //     s.Static.SaveMaterialsFromParts(savePath, parts);
        // });
        foreach (var s in extractedStatics)
        {
            var parts = s.Static.Load(ELOD.MostDetail);
            FbxHandler.AddStaticToScene(parts, s.Static.Hash);
            s.Static.SaveMaterialsFromParts(savePath, parts);  // todo this MUST be parallel, slowest thing here
        }

        Parallel.ForEach(StaticMap.Header.InstanceCounts, c =>
        {
            var model = StaticMap.Header.Statics[c.StaticIndex].Static;
            InfoConfigHandler.AddStaticInstances(StaticMap.Header.Instances.Skip(c.InstanceOffset).Take(c.InstanceCount).ToList(), model.Hash);
        });
        
        FbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        InfoConfigHandler.SetMeshName(meshName);
        InfoConfigHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
        AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Map, ConfigHandler.GetSingleFolderMapsEnabled());
        InfoConfigHandler.WriteToFile(savePath);
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