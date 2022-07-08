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
using Serilog;
using File = System.IO.File;

namespace Charm;

public partial class EntityView : UserControl
{
    public TagHash Hash;
    private string Name;
    private readonly ILogger _entityLog = Log.ForContext<EntityView>();


    public EntityView()
    {
        InitializeComponent();
    }

    public async void LoadEntity(TagHash entityHash)
    {
        Entity entity = new Entity(entityHash);
        AddEntity(entity, ELOD.MostDetail);
        LoadUI();
        ExportFull(new List<Entity>{entity}, entityHash);
    }

    public async void LoadEntityFromApi(DestinyHash apiHash)
    {
        List<Entity> entities = InvestmentHandler.GetEntitiesFromHash(apiHash);
        foreach (var entity in entities)
        {
            // todo find out why sometimes this is null
            if (entity == null)
            {
                continue;
                var a = 0;
            }
            AddEntity(entity, ELOD.MostDetail);
        }
        LoadUI();
        // ExportFull(entities, apiHash);
    }

    private void AddEntity(Entity entity, ELOD detailLevel)
    {
        var dynamicParts = entity.Load(detailLevel);
        FbxHandler.AddEntityToScene(entity, dynamicParts, detailLevel);
    }

    private void LoadUI()
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        string filePath = $"{ConfigHandler.GetExportSavePath()}/temp.fbx";
        FbxHandler.ExportScene(filePath);
        MVM.LoadEntityFromFbx(filePath);
        FbxHandler.Clear();
    }

    public void ExportFull(List<Entity> entities, string name)
    {
        _entityLog.Debug($"Exporting entity model name:{name}");
        InfoConfigHandler.MakeFile();
        string meshName = name;
        string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
        Directory.CreateDirectory(savePath);
        
        foreach (var entity in entities)
        {
            var dynamicParts = entity.Load(ELOD.MostDetail);
            FbxHandler.AddEntityToScene(entity, dynamicParts, ELOD.MostDetail);
            entity.SaveMaterialsFromParts(savePath, dynamicParts);
            entity.SaveTexturePlates(savePath);
        }

        FbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        InfoConfigHandler.SetMeshName(meshName);
        InfoConfigHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
        AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Entity);
        InfoConfigHandler.WriteToFile(savePath);
        _entityLog.Information($"Exported (full) entity model {name} to {savePath.Replace('\\', '/')}/");
    }
    
    // // old ones vvv
    //
    // private void GetDynamicContainer(TagHash hash)
    // {
    //     Entity = new Entity(hash);
    // }
    //
    // public void LoadDynamic(TagHash hash, ELOD detailLevel)
    // {
    //     GetDynamicContainer(hash);
    //     LoadDynamic(detailLevel);
    // }
    //
    // public void LoadApiEntity(DestinyHash hash, ELOD detailLevel)
    // {
    //     MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
    //     string filePath = $"{ConfigHandler.GetExportSavePath()}/temp.fbx";
    //     List<Entity> entities = InvestmentHandler.GetEntitiesFromHash(hash);
    //     foreach (var entity in entities)
    //     {
    //         // todo refactor this so I dont have to do this stupid shit
    //         Entity = entity;
    //         if (Entity == null)
    //         {
    //             GetDynamicContainer(Hash);
    //         }
    //         dynamicParts = Entity.Load(detailLevel);
    //         FbxHandler.AddEntityToScene(Entity, dynamicParts, detailLevel);
    //     }
    //     FbxHandler.ExportScene(filePath);
    //     MVM.LoadEntityFromFbx(filePath);
    //     FbxHandler.Clear();
    //     if (Name != null)
    //     {
    //         MVM.Title = Name;
    //     }
    //     else
    //     {
    //         MVM.Title = hash.GetHashString();
    //     }
    //     ExportFullEntity();
    // }
    //
    // public void LoadEntity(ELOD detailLevel)
    // {
    //     MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
    //     // MVM.SetEntity(displayParts, Entity.Skeleton.GetBoneNodes());
    //
    //     if (Entity == null)
    //     {
    //         GetDynamicContainer(Hash);
    //     }
    //     dynamicParts = Entity.Load(detailLevel);
    //     FbxHandler.AddEntityToScene(Entity, dynamicParts, detailLevel);
    //     string filePath = $"{ConfigHandler.GetExportSavePath()}/temp.fbx";
    //     FbxHandler.ExportScene(filePath);
    //     MVM.LoadEntityFromFbx(filePath);
    //     
    //     // MVM.SetSkeleton(Entity.Skeleton.GetBoneNodes());
    //     
    //     if (Name != null)
    //     {
    //         MVM.Title = Name;
    //     }
    //     else
    //     {
    //         MVM.Title = Hash.GetHashString();
    //     }
    //     // MVM.SubTitle = "Entity";
    //     FbxHandler.Clear();
    //     ExportFullEntity();
    // }
    //
    //
    //
    // private void ExportFullEntity()
    // {
    //     InfoConfigHandler.MakeFile();
    //     string meshName = Entity.Hash.GetHashString();
    //     string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
    //     FbxHandler.AddEntityToScene(Entity, dynamicParts, ELOD.MostDetail);
    //     Directory.CreateDirectory(savePath);
    //     Entity.SaveMaterialsFromParts(savePath, dynamicParts);
    //     FbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
    //     InfoConfigHandler.SetMeshName(meshName);
    //     InfoConfigHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
    //     AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Entity);
    //     InfoConfigHandler.WriteToFile(savePath);
    // }
}