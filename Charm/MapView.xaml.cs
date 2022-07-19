﻿using System.Collections.Concurrent;
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
using Serilog;

namespace Charm;

public partial class MapView : UserControl
{
    // public StaticMapData StaticMap;
    // public TagHash Hash;

    private static MainWindow _mainWindow = null;

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        ModelView.LodCombobox.SelectedIndex = 1; // default to least detail
    }
    
    public MapView()
    {
        InitializeComponent();
    }

    public void LoadMap(TagHash tagHash, ELOD detailLevel)
    {
        GetStaticMapData(tagHash, detailLevel);
        // _mainWindow.SetNewestTabSelected();
    }

    private void GetStaticMapData(TagHash tagHash, ELOD detailLevel)
    {
        StaticMapData staticMapData = PackageHandler.GetTag(typeof(StaticMapData), tagHash);
        SetMapUI(staticMapData, detailLevel);
    }

    private void SetMapUI(StaticMapData staticMapData, ELOD detailLevel)
    {
        var displayParts = MakeDisplayParts(staticMapData, detailLevel);
        Dispatcher.Invoke(() =>
        {
            MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
            MVM.SetChildren(displayParts);
        });
        displayParts.Clear();
    }

    public void Clear()
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        MVM.Clear();
    }

    public void Dispose()
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        MVM.Dispose();
    }
    
    public static void ExportFullMap(StaticMapData staticMapData)
    {
        FbxHandler fbxHandler = new FbxHandler();
        string meshName = staticMapData.Hash.GetHashString();
        string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
        if (ConfigHandler.GetSingleFolderMapsEnabled())
        {
            savePath = ConfigHandler.GetExportSavePath() + "/Maps";
        }
        fbxHandler.InfoHandler.SetMeshName(meshName);
        Directory.CreateDirectory(savePath);
        // Extract all
        List<D2Class_BD938080> extractedStatics = staticMapData.Header.Statics.DistinctBy(x => x.Static.Hash).ToList();
        // foreach (var s in extractedStatics)
        Parallel.ForEach(extractedStatics, s =>
        {
            var parts = s.Static.Load(ELOD.MostDetail);
            fbxHandler.AddStaticToScene(parts, s.Static.Hash);
            s.Static.SaveMaterialsFromParts(savePath, parts, ConfigHandler.GetUnrealInteropEnabled());
        });

        Parallel.ForEach(staticMapData.Header.InstanceCounts, c =>
        {
            var model = staticMapData.Header.Statics[c.StaticIndex].Static;
            fbxHandler.InfoHandler.AddStaticInstances(staticMapData.Header.Instances.Skip(c.InstanceOffset).Take(c.InstanceCount).ToList(), model.Hash);
        });
        if (ConfigHandler.GetUnrealInteropEnabled())
        {
            fbxHandler.InfoHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
            AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Map, ConfigHandler.GetOutputTextureFormat(), ConfigHandler.GetSingleFolderMapsEnabled());
        }
        if (ConfigHandler.GetBlenderInteropEnabled())
        {
            //Only gonna export a blender py for maps (for now)
            AutomatedImporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedImporter.EImportType.Map, ConfigHandler.GetOutputTextureFormat(), ConfigHandler.GetSingleFolderMapsEnabled());
        }

        fbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        fbxHandler.Dispose();
    }

    private List<MainViewModel.DisplayPart> MakeDisplayParts(StaticMapData staticMap, ELOD detailLevel)
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