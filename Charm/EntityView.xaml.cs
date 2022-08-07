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
using Field.Models;
using HelixToolkit.SharpDX.Core.Model.Scene;
using Serilog;
using File = System.IO.File;

namespace Charm;

public partial class EntityView : UserControl
{
    private readonly ILogger _entityLog = Log.ForContext<EntityView>();
    private static MainWindow _mainWindow = null;
    private Entity _loadedEntity = null;

    public EntityView()
    {
        InitializeComponent();
    }
    
    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
    }

    public bool LoadEntity(TagHash entityHash, FbxHandler fbxHandler, bool bBlockRecursion=false)
    {
        fbxHandler.Clear();
        Entity entity = PackageHandler.GetTag(typeof(Entity), entityHash);
        _loadedEntity = entity;
        // todo fix this
        if (entity.AnimationGroup != null && !bBlockRecursion)  // Make a new tab and use that with FullEntityView
        {
            var fev = new FullEntityView();
            _mainWindow.MakeNewTab(entityHash, fev);
            _mainWindow.SetNewestTabSelected();
            return fev.LoadEntity(entityHash, fbxHandler);
        }
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

    private void AddEntity(Entity entity, ELOD detailLevel, FbxHandler fbxHandler, Animation animation=null)
    {
        var dynamicParts = entity.Load(detailLevel);
        if (dynamicParts.Count == 0)
            return;
        ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();
        if (animation != null)
        {
            animation.tra = new Vector3(int.Parse(TraX.Text), int.Parse(TraY.Text), int.Parse(TraZ.Text), true);
            animation.rot = new Vector3(int.Parse(RotX.Text), int.Parse(RotY.Text), int.Parse(RotZ.Text), true);
            animation.flipTra = new Vector3(Convert.ToInt32(FlipTraX.IsChecked), Convert.ToInt32(FlipTraY.IsChecked), Convert.ToInt32(FlipTraZ.IsChecked), true);
            animation.flipRot = new Vector3(Convert.ToInt32(FlipRotX.IsChecked), Convert.ToInt32(FlipRotY.IsChecked), Convert.ToInt32(FlipRotZ.IsChecked), true);
            animation.traXYZ = new [] { TraXX.Text, TraYY.Text, TraZZ.Text };
            animation.rotXYZ = new [] { RotXX.Text, RotYY.Text, RotZZ.Text };
        }
        fbxHandler.AddEntityToScene(entity, dynamicParts, detailLevel, animation);
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

    public void Export(List<Entity> entities, string name, EExportType exportType)
    {
        FbxHandler fbxHandler = new FbxHandler(exportType == EExportType.Full);
        _entityLog.Debug($"Exporting entity model name: {name}");
        string savePath = ConfigHandler.GetExportSavePath();
        string meshName = name;
        if (exportType == EExportType.Full)
        {
            savePath += $"/{meshName}";
        }
        Directory.CreateDirectory(savePath);
        
        foreach (var entity in entities)
        {
            var dynamicParts = entity.Load(ELOD.MostDetail);
            fbxHandler.AddEntityToScene(entity, dynamicParts, ELOD.MostDetail);
            if (exportType == EExportType.Full)
            {
                entity.SaveMaterialsFromParts(savePath, dynamicParts, ConfigHandler.GetUnrealInteropEnabled());
                entity.SaveTexturePlates(savePath);
            }
        }

        if (exportType == EExportType.Full)
        {
            fbxHandler.InfoHandler.SetMeshName(meshName);
            if (ConfigHandler.GetUnrealInteropEnabled())
            {
                fbxHandler.InfoHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
                AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Entity, ConfigHandler.GetOutputTextureFormat());
                //AutomatedImporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedImporter.EImportType.Entity, ConfigHandler.GetOutputTextureFormat());

            }
        }
        fbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        fbxHandler.Dispose();
        _entityLog.Information($"Exported entity model {name} to {savePath.Replace('\\', '/')}/");
    }

    public void LoadAnimation(TagHash tagHash, FbxHandler fbxHandler)
    {
        Animation animation = PackageHandler.GetTag(typeof(Animation), tagHash);
        // to load an animation into the viewer, we need to save the fbx then load
        fbxHandler.Clear();
        AddEntity(_loadedEntity, ModelView.GetSelectedLod(), fbxHandler, animation);
        LoadUI(fbxHandler);
    }
    
    public void LoadAnimationWithPlayerModels(TagHash tagHash, FbxHandler fbxHandler)
    {
        Animation animation = PackageHandler.GetTag(typeof(Animation), tagHash);
        // to load an animation into the viewer, we need to save the fbx then load
        fbxHandler.Clear();

        fbxHandler.AddPlayerSkeletonAndMesh();

        // Add animation
        animation.Load();
        // animation.SaveToFile($"C:/T/animation_{animHash}.json");
        fbxHandler.AddAnimationToEntity(animation, fbxHandler._globalSkeletonNodes);

        LoadUI(fbxHandler);
    }
}