using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
using Serilog;
using VersionChecker;
using Encoding = SharpDX.Text.Encoding;

namespace Charm;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public static ProgressView Progress = null;
    private static TabItem _newestTab = null;
    private static LogView _logView = null;
    private static TabItem _logTab = null;

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        Progress = ProgressView;
        Task.Run(InitialiseHandlers);
    }
    
    public MainWindow()
    {
        InitializeComponent();
        
        _logView = new LogView();
        LogHandler.Initialise(_logView);
        
        // Make log
        MakeNewTab("Log", _logView);
        _logTab = _newestTab;
        
        // Hide tab by default
        MainTabControl.Visibility = Visibility.Hidden;
            
        // Check if packages path exists in config
        ConfigHandler.CheckPackagesPathIsValid();
        MainTabControl.Visibility = Visibility.Visible;

        // Check version
        try
        {
            CheckVersion();
        }
        finally
        {
            Log.Information("Version check complete.");
        }
    }

    private async void CheckVersion()
    {
        var currentVersion = new ApplicationVersion("1.0.0");
        var versionChecker = new ApplicationVersionChecker("https://github.com/MontagueM/Charm/tree/main/Charm/versions.xml", currentVersion);
        try
        {
            var upToDate = await versionChecker.IsUpToDate();
            if (!upToDate)
            {
                MessageBox.Show("New version available on GitHub!");
            }
        }
        catch (Exception e)
        {
            // Could not get or parse version file
            #if !DEBUG
            MessageBox.Show("Could not get version.");
            #endif
            Log.Error($"Could not get version error {e}.");
        }
    }

    private async void InitialiseHandlers()
    {
        Progress.SetProgressStages(new List<string>
        {
            "fonts",
            "fnv hashes",
            "hash 64",
            "investment",
            "global string cache",
            "fbx",
            "activity names",
        });

        // Load all the fonts
        await Task.Run(() =>
        {
            RegisterFonts(FontHandler.Initialise());
        });
        Progress.CompleteStage();

        // Initialise FNV handler -- must be first bc my code is shit
        await Task.Run(FnvHandler.Initialise);
        Progress.CompleteStage();

        // Get all hash64 -- must be before InvestmentHandler
        await Task.Run(TagHash64Handler.Initialise);
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
        List<string> fontList = initialise.Select(pair => (pair.Key.Family + " " + pair.Key.Subfamily).Trim()).ToList();
        foreach (var s in fontList)
        {
            Debug.WriteLine(s);
        }
        /*
        AXIS Std H
        AXIS Std R
        AXIS Std M
        Cromwell NF
        Neue Haas Grotesk Text Pro 66 Medium Italic
        Neue Haas Grotesk Text Pro 55 Roman
        Neue Haas Grotesk Text Pro 65 Medium
        Neue Haas Grotesk Text Pro 56 Italic
        Aldine 401 BT
        Cromwell HPLHS
        Destiny Symbols
        Sandoll MjNeo1Uni 04 Md
        Neue Haas Unica W1G Bold
        Neue Haas Unica W1G Medium Italic
        Neue Haas Unica W1G Medium
        Neue Haas Unica W1G Regular
        Neue Haas Unica W1G Italic
        Neue Haas Grotesk Display Pro 75 Bold
        Iwata New Reisho Pro M
        M Ying Hei HK W8
        M Ying Hei HK W4
        M Ying Hei HK W7
        Sandoll GothicNeo1Unicode 09 Hv
        Sandoll GothicNeo1Unicode 07 Bd
        Sandoll GothicNeo1Unicode 05 Md
        M Ying Hei PRC W8
        M Ying Hei PRC W4
        Destiny Keys Regular
        M Ying Hei PRC W7
        */
        var a = 0;
    }

    private void OpenConfigPanel_OnClick(object sender, RoutedEventArgs e)
    {
        MakeNewTab("Configuration", new ConfigView());
        SetNewestTabSelected();
    }
    
    private void OpenLogPanel_OnClick(object sender, RoutedEventArgs e)
    {
        MakeNewTab("Log", _logView);
        SetNewestTabSelected();    
    }
    
    public void SetNewestTabSelected()
    {
        MainTabControl.SelectedItem = _newestTab;
    }

    public void SetLoggerSelected()
    {
        MainTabControl.SelectedItem = _logTab;
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
                    MakeNewTab(hash, dynamicView);
                    break;
                case 0x80806D44:
                    // StaticView staticView = new StaticView(hash);
                    // staticView.LoadStatic(ELOD.MostDetail);
                    // MakeNewTab(hash, staticView);
                    break;
                case 0x808093AD:
                    // MapView mapView = new MapView(hash);
                    // mapView.LoadMap();
                    // MakeNewTab(hash, mapView);
                    break;
                case 0x80808E8E:
                    ActivityView activityView = new ActivityView();
                    activityView.LoadActivity(hash);
                    MakeNewTab(hash, activityView);
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