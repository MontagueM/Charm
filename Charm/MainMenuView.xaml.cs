using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using Field;
using Field.Entities;
using Field.General;
using Field.Models;
using Field.Textures;
using SharpDX.Toolkit.Graphics;

namespace Charm;

public partial class MainMenuView : UserControl
{
    private static MainWindow _mainWindow = null;
        
    public MainMenuView()
    {
        InitializeComponent();
    }
        
    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
    }
    
    private void ApiViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView apiView = new TagListViewerView();
        apiView.LoadContent(ETagListType.ApiList);
        _mainWindow.MakeNewTab("api", apiView);
        _mainWindow.SetNewestTabSelected();
    }
    
    private void NamedEntitiesBagsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.DestinationGlobalTagBagList);
        _mainWindow.MakeNewTab("destination global tag bag", tagListView);
        _mainWindow.SetNewestTabSelected();
    }
    
    private void AllEntitiesViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.EntityList);
        _mainWindow.MakeNewTab("dynamics", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void ActivitiesViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.ActivityList);
        _mainWindow.MakeNewTab("activities", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllStaticsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.StaticsList);
        _mainWindow.MakeNewTab("statics", tagListView);
        _mainWindow.SetNewestTabSelected();    
    }
    
    private void WeaponAudioViewButton_Click(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.WeaponAudioGroupList);
        _mainWindow.MakeNewTab("weapon audio", tagListView);
        _mainWindow.SetNewestTabSelected();    
    }

    private void AllAudioViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.SoundsPackagesList);
        _mainWindow.MakeNewTab("sounds", tagListView);
        _mainWindow.SetNewestTabSelected();    
    }

    private void AllStringsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.StringContainersList);
        _mainWindow.MakeNewTab("strings", tagListView);
        _mainWindow.SetNewestTabSelected();      
    }

    private void AllTexturesViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.TextureList);
        _mainWindow.MakeNewTab("textures", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void CinematicsButton_OnClick(object sender, RoutedEventArgs e)
    {
        string activityHash = "9694ea80";
        Field.Activity activity = PackageHandler.GetTag(typeof(Field.Activity), new TagHash(activityHash));
        EntityResource cinematicResource = ((D2Class_0C468080) activity.Header.Unk40[0].Unk70[0].UnkEntityReference.Header.Unk18.Header.EntityResources[1]
            .EntityResourceParent.Header.EntityResource.Header.Unk18).CinematicEntity.Header.EntityResources.Last().ResourceHash;
        HashSet<string> cinematicModels = new HashSet<string>();
        foreach (D2Class_AE5F8080 groupEntry in ((D2Class_B75F8080)cinematicResource.Header.Unk18).CinematicEntityGroups)
        {
            foreach (D2Class_B15F8080 entityEntry in groupEntry.CinematicEntities)
            {
                var entityWithModel = entityEntry.CinematicEntityModel;
                var entityWithAnims = entityEntry.CinematicEntityAnimations;
                if (entityWithModel != null)
                {
                    cinematicModels.Add(entityWithModel.Hash.ToString());
                    if (entityWithAnims.AnimationGroup != null) // caiatl
                    {
                        foreach (var animation in ((D2Class_F8258080) entityWithAnims.AnimationGroup.Header.Unk18).AnimationGroup.Header.Animations)
                        {
                            if (animation.Animation == null)
                                continue;
                            animation.Animation.ParseTag();
                            animation.Animation.Load();
                            FbxHandler fbxHandler = new FbxHandler();
                            fbxHandler.AddEntityToScene(entityWithModel, entityWithModel.Load(ELOD.MostDetail), ELOD.MostDetail, animation.Animation);
                            fbxHandler.ExportScene($"C:/T/cinematic/{entityWithModel.Hash}_{animation.Animation.Hash}_{animation.Animation.Header.FrameCount}_{Math.Round((float)animation.Animation.Header.FrameCount/30)}.fbx");
                            fbxHandler.Dispose();
                        }
                    }
                    if (entityWithModel.Hash == "91EBA880" && entityWithAnims.AnimationGroup != null) // player
                    {
                        foreach (var animation in ((D2Class_F8258080) entityWithAnims.AnimationGroup.Header.Unk18).AnimationGroup.Header.Animations)
                        {
                            animation.Animation.ParseTag();
                            animation.Animation.Load();
                            FbxHandler fbxHandler = new FbxHandler();
                            fbxHandler.AddPlayerSkeletonAndMesh();
                            fbxHandler.AddAnimationToEntity(animation.Animation);
                            fbxHandler.ExportScene($"C:/T/cinematic/player_{animation.Animation.Hash}_{animation.Animation.Header.FrameCount}_{Math.Round((float)animation.Animation.Header.FrameCount/30)}.fbx");
                            fbxHandler.Dispose();
                        }

                        var c = 0;
                    }
                }
                var a = 0;
            }
        }

        var b = 0;
    }

    private void AnimationsButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.AnimationPackageList);
        _mainWindow.MakeNewTab("animations", tagListView);
        _mainWindow.SetNewestTabSelected();
    }
}