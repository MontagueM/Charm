using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Field.Entities;
using Field.General;
using Field.Models;
using HelixToolkit.SharpDX.Core.Model.Scene;
using File = System.IO.File;

namespace Charm;

public partial class DynamicView : UserControl
{
    public Entity Entity;
    public string Hash;
    private List<DynamicPart> dynamicParts;
    private string Name;

    public DynamicView()
    {
        InitializeComponent();
    }

    public DynamicView(string hash)
    {
        InitializeComponent();
        Hash = hash;
    }
    
    private void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        if (Hash != null)
        {
            LoadDynamic(ELOD.MostDetail);    
        }
    }

    public void LoadDynamic(ELOD detailLevel, Entity entity, string name)
    {
        Entity = entity;
        LoadDynamic(detailLevel);
        Name = name;
    }

    private void GetDynamicContainer(string hash)
    {
        Entity = new Entity(hash);
    }

    private void LoadDynamic(ELOD detailLevel)
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        // MVM.SetEntity(displayParts, Entity.Skeleton.GetBoneNodes());

        if (Entity == null)
        {
            GetDynamicContainer(Hash);
        }
        dynamicParts = Entity.Load(detailLevel);
        FbxHandler.AddEntityToScene(Entity, dynamicParts, detailLevel);
        string filePath = $"{ConfigHandler.GetExportSavePath()}/temp.fbx";
        FbxHandler.ExportScene(filePath);
        MVM.LoadEntityFromFbx(filePath);
        
        // MVM.SetSkeleton(Entity.Skeleton.GetBoneNodes());
        
        if (Name != null)
        {
            MVM.Title = Name;
        }
        else
        {
            MVM.Title = Hash;
        }
        // MVM.SubTitle = "Entity";
        FbxHandler.Clear();
        ExportFullEntity();
    }

    private void ExportFullEntity()
    {
        InfoConfigHandler.MakeFile();
        string meshName = Entity.Hash;
        string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
        Console.WriteLine("A");
        FbxHandler.AddEntityToScene(Entity, dynamicParts, ELOD.MostDetail);
        Directory.CreateDirectory(savePath);
        Console.WriteLine("B");
        Entity.SaveMaterialsFromParts(savePath, dynamicParts);
        FbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        InfoConfigHandler.SetMeshName(meshName);
        Console.WriteLine("C");
        InfoConfigHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
        AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName);
        Console.WriteLine("D");
        InfoConfigHandler.WriteToFile(savePath);
    }

    private List<MainViewModel.DisplayPart> MakeDisplayParts(List<DynamicPart> containerParts)
    {
        List<MainViewModel.DisplayPart> displayParts = new List<MainViewModel.DisplayPart>();
        foreach (DynamicPart part in containerParts)
        {
            MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
            displayPart.EntityPart = part;
            displayPart.Translations.Add(Vector3.Zero);
            displayPart.Rotations.Add(Vector4.Quaternion);
            displayPart.Scales.Add(Vector3.One);
            displayParts.Add(displayPart);
        }
        return displayParts;
    }
}