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
using Field.Textures;
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

    public bool LoadEntity(TagHash entityHash, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        Entity entity = PackageHandler.GetTag(typeof(Entity), entityHash);
        AddEntity(entity, ModelView.GetSelectedLod(), fbxHandler);
        return LoadUI(fbxHandler);
    }

    public async void LoadEntityFromApi(DestinyHash apiHash, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        List<Entity> entities = InvestmentHandler.GetEntitiesFromHash(apiHash);
        foreach (var entity in entities)
        {
            // todo find out why sometimes this is null
            if (entity == null)
            {
                continue;
            }
            AddEntity(entity, ModelView.GetSelectedLod(), fbxHandler);
        }
        LoadUI(fbxHandler);
    }

    private void AddEntity(Entity entity, ELOD detailLevel, FbxHandler fbxHandler)
    {
        var dynamicParts = entity.Load(detailLevel);
        ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();
        fbxHandler.AddEntityToScene(entity, dynamicParts, detailLevel);
    }

    private bool LoadUI(FbxHandler fbxHandler)
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        string filePath = $"{ConfigHandler.GetExportSavePath()}/temp.fbx";
        fbxHandler.ExportScene(filePath);
        bool loaded = MVM.LoadEntityFromFbx(filePath);
        fbxHandler.Clear();
        return loaded;
    }

    public void Export(List<Entity> entities, string name, EExportTypeFlag exportType)
    {
        FbxHandler fbxHandler = new FbxHandler(exportType == EExportTypeFlag.Full);
        _entityLog.Debug($"Exporting entity model name: {name}");
        string savePath = ConfigHandler.GetExportSavePath();
        string meshName = name;
        if (exportType == EExportTypeFlag.Full)
        {
            savePath += $"/{meshName}";
        }
        Directory.CreateDirectory(savePath);
        
        foreach (var entity in entities)
        {
            var dynamicParts = entity.Load(ELOD.MostDetail);
            fbxHandler.AddEntityToScene(entity, dynamicParts, ELOD.MostDetail);
            if (exportType == EExportTypeFlag.Full)
            {
                var settings = new ExportSettings() {
                    Unreal = ConfigHandler.GetUnrealInteropEnabled(),
                    Blender = ConfigHandler.GetBlenderInteropEnabled(),
                    Source2 = true,
                    Raw = true
                };
                entity.SaveMaterialsFromParts(savePath, dynamicParts, settings);
                entity.SaveTexturePlates(savePath);
            }
        }

        if (exportType == EExportTypeFlag.Full)
        {
            fbxHandler.InfoHandler.SetMeshName(meshName);
            if (ConfigHandler.GetUnrealInteropEnabled())
            {
                fbxHandler.InfoHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
                AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Entity, ConfigHandler.GetOutputTextureFormat());
                AutomatedImporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedImporter.EImportType.Entity, ConfigHandler.GetOutputTextureFormat());

            }
        }
        fbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        fbxHandler.Dispose();
        _entityLog.Information($"Exported entity model {name} to {savePath.Replace('\\', '/')}/");
    }
}