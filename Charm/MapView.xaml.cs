using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Field;
using Field.General;
using Field.Models;
using Field.Statics;

namespace Charm;

public partial class MapView : UserControl
{
    // public StaticMapData StaticMap;
    // public TagHash Hash;

    private static MainWindow _mainWindow = null;

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
    }
    
    public MapView()
    {
        InitializeComponent();
    }

    public void LoadMap(TagHash tagHash)
    {
        GetStaticMapData(tagHash);
        _mainWindow.SetNewestTabSelected();
    }

    private async void GetStaticMapData(TagHash tagHash)
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "loading map tag",
            "making UI",
        });
        await Task.Run(() =>
        {
            StaticMapData staticMapData = new StaticMapData(tagHash);
            MainWindow.Progress.CompleteStage();
            SetMapUI(staticMapData);
            MainWindow.Progress.CompleteStage();
        });
    }

    private void SetMapUI(StaticMapData staticMapData)
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        var displayParts = MakeDisplayParts(staticMapData);
        MVM.SetChildren(displayParts);
    }
    
    public static void ExportFullMap(StaticMapData staticMapData)
    {
        InfoConfigHandler.MakeFile();
        string meshName = staticMapData.Hash.GetHashString();
        string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
        if (ConfigHandler.GetSingleFolderMapsEnabled())
        {
            savePath = ConfigHandler.GetExportSavePath() + "/Maps";
        }
        InfoConfigHandler.SetMeshName(meshName);
        Directory.CreateDirectory(savePath);
        // Extract all
        List<D2Class_BD938080> extractedStatics = staticMapData.Header.Statics.DistinctBy(x => x.Static.Hash).ToList();
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

        Parallel.ForEach(staticMapData.Header.InstanceCounts, c =>
        {
            var model = staticMapData.Header.Statics[c.StaticIndex].Static;
            InfoConfigHandler.AddStaticInstances(staticMapData.Header.Instances.Skip(c.InstanceOffset).Take(c.InstanceCount).ToList(), model.Hash);
        });
        
        FbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        InfoConfigHandler.SetMeshName(meshName);
        InfoConfigHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
        AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Map, ConfigHandler.GetSingleFolderMapsEnabled());
        InfoConfigHandler.WriteToFile(savePath);
    }

    private List<MainViewModel.DisplayPart> MakeDisplayParts(StaticMapData staticMap)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();
        Parallel.ForEach(staticMap.Header.InstanceCounts, c =>
        {
            // inefficiency as sometimes there are two instance count entries with same hash. why? idk
            var model = staticMap.Header.Statics[c.StaticIndex].Static;
            var parts = model.Load(ELOD.MostDetail);
            for (int i = c.InstanceOffset; i < c.InstanceOffset + c.InstanceCount; i++)
            {
                foreach (var part in parts)
                {
                    MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
                    displayPart.BasePart = part;
                    displayPart.Translations.Add(staticMap.Header.Instances[i].Position);
                    displayPart.Rotations.Add(staticMap.Header.Instances[i].Rotation);
                    displayPart.Scales.Add(staticMap.Header.Instances[i].Scale);
                    displayParts.Add(displayPart);
                }

            }
        });
        return displayParts.ToList();
    }
}