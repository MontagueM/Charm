using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using Field.General;
using Field.Models;
using Field;
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
        // TagListViewerView apiView = new TagListViewerView();
        // apiView.LoadContent(ETagListType.ApiList);
        DareView apiView = new DareView();
        apiView.LoadContent();
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
    
    private void AllScriptsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.ScriptContainersList);
        _mainWindow.MakeNewTab("scripts", tagListView);
        _mainWindow.SetNewestTabSelected();      
    }
}