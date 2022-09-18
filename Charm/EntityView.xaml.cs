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
using Field;
using Field.Entities;
using Field.General;
using Field.Investment;
using Field.Models;
using HelixToolkit.SharpDX.Core.Model.Scene;
using Internal.Fbx;
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

    public static void Export(List<Entity> entities, string name, EExportTypeFlag exportType, EntitySkeleton overrideSkeleton = null)
    {
        FbxHandler fbxHandler = new FbxHandler(exportType == EExportTypeFlag.Full);

        List<FbxNode> boneNodes = null;
        if (overrideSkeleton != null)
            boneNodes = fbxHandler.AddSkeleton(overrideSkeleton.GetBoneNodes());
        
        Log.Debug($"Exporting entity model name: {name}");
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
            fbxHandler.AddEntityToScene(entity, dynamicParts, ELOD.MostDetail, boneNodes);
            if (exportType == EExportTypeFlag.Full)
            {
                entity.SaveMaterialsFromParts(savePath, dynamicParts, ConfigHandler.GetUnrealInteropEnabled() || ConfigHandler.GetS2ShaderExportEnabled());
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
        
        // Scale and rotate
        fbxHandler.ScaleAndRotateForBlender(boneNodes[0]);
        
        fbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        fbxHandler.Dispose();
        Log.Information($"Exported entity model {name} to {savePath.Replace('\\', '/')}/");
    }

    public static void ExportInventoryItem(ApiItem item)
    {
        string name = $"{item.Item.Header.InventoryItemHash.Hash}_{item.ItemName}";
        // Export the model
        // todo bad, should be replaced
        EntitySkeleton overrideSkeleton = new EntitySkeleton(new TagHash("BC38AB80"));
        EntityView.Export(InvestmentHandler.GetEntitiesFromHash(item.Item.Header.InventoryItemHash),
            name, EExportTypeFlag.Full, overrideSkeleton);
        
        // Export the dye info
        Dictionary<DestinyHash, Dye> dyes = new Dictionary<DestinyHash, Dye>();
        if (item.Item.Header.Unk90 is D2Class_77738080 translationBlock)
        {
            foreach (var dyeEntry in translationBlock.DefaultDyes)
            {
                Dye dye = InvestmentHandler.GetDyeFromIndex(dyeEntry.DyeIndex);
                dyes.Add(InvestmentHandler.GetChannelHashFromIndex(dyeEntry.ChannelIndex), dye);
            }
            foreach (var dyeEntry in translationBlock.LockedDyes)
            {
                Dye dye = InvestmentHandler.GetDyeFromIndex(dyeEntry.DyeIndex);
                dyes.Add(InvestmentHandler.GetChannelHashFromIndex(dyeEntry.ChannelIndex), dye);
            }
        }
        
        string savePath = ConfigHandler.GetExportSavePath();
        string meshName = name;
        savePath += $"/{meshName}";
        Directory.CreateDirectory(savePath);
        AutomatedImporter.SaveBlenderApiFile(savePath, item.ItemName, ConfigHandler.GetOutputTextureFormat(), dyes.Values.ToList());
    }
}