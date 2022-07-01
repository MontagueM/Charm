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
    public TagHash Hash;
    private List<DynamicPart> dynamicParts;
    private string Name;

    public DynamicView()
    {
        InitializeComponent();
    }

    public DynamicView(TagHash hash)
    {
        InitializeComponent();
        Hash = hash;
    }

    public void LoadDynamic(ELOD detailLevel, Entity entity, string name)
    {
        Entity = entity;
        LoadDynamic(detailLevel);
        Name = name;
    }

    private void GetDynamicContainer(TagHash hash)
    {
        Entity = new Entity(hash);
    }

    public void LoadDynamic(ELOD detailLevel)
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
            MVM.Title = Hash.GetHashString();
        }
        // MVM.SubTitle = "Entity";
        FbxHandler.Clear();
        ExportFullEntity();
    }

    private void ExportFullEntity()
    {
        InfoConfigHandler.MakeFile();
        string meshName = Entity.Hash.GetHashString();
        string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
        FbxHandler.AddEntityToScene(Entity, dynamicParts, ELOD.MostDetail);
        Directory.CreateDirectory(savePath);
        Entity.SaveMaterialsFromParts(savePath, dynamicParts);
        FbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        InfoConfigHandler.SetMeshName(meshName);
        InfoConfigHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
        AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Entity);
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