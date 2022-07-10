using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    public bool LoadEntity(TagHash entityHash)
    {
        Entity entity = new Entity(entityHash);
        AddEntity(entity, ModelView.GetSelectedLod());
        return LoadUI();
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
            AddEntity(entity, ModelView.GetSelectedLod());
        }
        LoadUI();
    }

    private void AddEntity(Entity entity, ELOD detailLevel)
    {
        var dynamicParts = entity.Load(detailLevel);
        ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();
        FbxHandler.AddEntityToScene(entity, dynamicParts, detailLevel);
    }

    private bool LoadUI()
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        string filePath = $"{ConfigHandler.GetExportSavePath()}/temp.fbx";
        FbxHandler.ExportScene(filePath);
        bool loaded = MVM.LoadEntityFromFbx(filePath);
        FbxHandler.Clear();
        return loaded;
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
}