using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Field;
using Field.General;
using Field.Models;
using Field.Statics;

namespace Charm;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public static ProgressView Progress = null;
    private static TabItem _newestTab = null;

    
    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        Progress = ProgressView;
        Task.Run(InitialiseHandlers);
    }
    
    public MainWindow()
    {
        // todo give this all a progressbar
        InitializeComponent();
        
        // Hide tab by default
        MainTabControl.Visibility = Visibility.Hidden;
            
        // Check if packages path exists in config
        ConfigHandler.CheckPackagesPathIsValid();
        MainTabControl.Visibility = Visibility.Visible;
    }

    private async void InitialiseHandlers()
    {
        Progress.SetProgressStages(new List<string>
        {
            "FNV Handler",
            "Hash64",
            "Font Handler",
            "Investment",
            "Global string cache",
            "Fbx Handler",
            "Activity Names",
        });

        // Initialise FNV handler -- must be first bc my code is shit
        await Task.Run(FnvHandler.Initialise);
        Progress.CompleteStage();

        // Get all hash64 -- must be before InvestmentHandler
        await Task.Run(TagHash64Handler.Initialise);
        Progress.CompleteStage();
        
        // Load all the fonts
        await Task.Run(() =>
        {
            RegisterFonts(FontHandler.Initialise());
        });
        Progress.CompleteStage();
        
        // Initialise investment
        await Task.Run(InvestmentHandler.Initialise);
        Progress.CompleteStage();

        // Initialise global string cache
        await Task.Run(PackageHandler.GenerateGlobalStringContainerCache);
        Progress.CompleteStage();
        
        // Initialise fbx handler
        await Task.Run(FbxHandler.Initialise);
        Progress.CompleteStage();
        
        // Get all activity names
        await Task.Run(PackageHandler.GetAllActivityNames);
        Progress.CompleteStage();
    }

    private void RegisterFonts(ConcurrentDictionary<FontHandler.FontInfo, FontFamily> initialise)
    {
        foreach (var (key, value) in initialise)
        {
            Application.Current.Resources.Add($"{key.Family} {key.Subfamily}", value);
        }

        // Debug font list
        List<string> fontList = initialise.Select(pair => pair.Key.Family + " " + pair.Key.Subfamily).ToList();
        
        var a = 0;
    }

    private void OpenConfigPanel_OnClick(object sender, RoutedEventArgs e)
    {
        MakeNewTab("Configuration", new ConfigView());
        SetNewestTabSelected();
    }
    
    public void SetNewestTabSelected()
    {
        MainTabControl.SelectedItem = _newestTab;
    }
    
    public void SetNewestTabName(string newName)
    {
        _newestTab.Header = newName.Replace('_', '.');
    }

    public void MakeNewTab(string name, UserControl content)
    {
        // Testing making it all caps
        name = name.ToUpper();
        
        // Check if the name already exists, if so set newest tab to that
        var items = MainTabControl.Items;
        foreach (TabItem item in items)
        {
            if (name == (string) item.Header)
            {
                _newestTab = item;
                return;
            }
        }
        
        _newestTab = new TabItem();
        _newestTab.Content = content;
        MainTabControl.Items.Add(_newestTab);
        SetNewestTabName(name);
    }
    
    public void AddWindow(TagHash hash)
    {
        // Adds a new tab to the tab control
        UserControl content;
        DestinyHash reference = PackageHandler.GetEntryReference(hash);
        int hType, hSubtype;
        PackageHandler.GetEntryTypes(hash, out hType, out hSubtype);
        if ((hType == 8 || hType == 16) && hSubtype == 0)
        {
            switch (reference.Hash)
            {
                case 0x80809AD8:
                    EntityView dynamicView = new EntityView();
                    dynamicView.LoadEntity(hash);
                    content = dynamicView;
                    MakeNewTab(hash, content);
                    break;
                case 0x80806D44:
                    StaticView staticView = new StaticView(hash);
                    staticView.LoadStatic(ELOD.MostDetail);
                    content = staticView;
                    MakeNewTab(hash, content);
                    break;
                case 0x808093AD:
                    MapView mapView = new MapView(hash);
                    mapView.LoadMap();
                    content = mapView;
                    MakeNewTab(hash, content);
                    break;
                case 0x80808E8E:
                    ActivityView activityView = new ActivityView();
                    activityView.LoadActivity(hash);
                    content = activityView;
                    MakeNewTab(hash, content);
                    SetNewestTabSelected();
                    break;
                default:
                    MessageBox.Show("Unknown reference: " + Endian.U32ToString(reference));
                    break;
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}