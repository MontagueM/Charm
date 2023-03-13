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
using System.Windows.Input;
using System.Windows.Media;
using Field;
using Field.General;
using Field.Models;
using Field.Statics;
using NAudio.SoundFont;
using Serilog;
using VersionChecker;

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
    private bool _bHasInitialised = false;

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        Progress = ProgressView;
        if (MainMenuTab.Visibility == Visibility.Visible)
        {
            Task.Run(InitialiseHandlers);
            _bHasInitialised = true;
        }
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
        HideMainMenu();

        // Check if packages path exists in config
        // ConfigHandler.CheckPackagesPathIsValid();
        if (ConfigHandler.DoesPathKeyExist("packagesPath") && ConfigHandler.DoesPathKeyExist("exportSavePath"))
        {
            MainMenuTab.Visibility = Visibility.Visible;
        }
        else
        {
            MakeNewTab("Configuration", new ConfigView());
            SetNewestTabSelected();
        }

        // Check version
        CheckVersionAsync();

        // Log game version
        CheckGameVersion();


        var a = 0;
    }

    public string CheckGameVersion()
    {
        string version = "";
        try
        {
            var path = ConfigHandler.GetPackagesPath().Split("packages")[0] + "destiny2.exe";
            var versionInfo = FileVersionInfo.GetVersionInfo(path);
            version = versionInfo.FileVersion;
            Log.Information("Game version: " + version);
        }
        catch (Exception e)
        {
            Log.Error($"Could not get game version error {e}.");
        }
        return version;
    }

    private async void CheckVersionAsync()
    {
        var currentVersion = new ApplicationVersion("1.3.2");
        var versionChecker = new ApplicationVersionChecker("https://github.com/MontagueM/Charm/raw/main/", currentVersion);
        versionChecker.LatestVersionName = "version";
        try
        {
            var upToDate = await versionChecker.IsUpToDate();
            if (!upToDate)
            {
                MessageBox.Show($"New version available on GitHub! (local {versionChecker.CurrentVersion.Id} vs ext {versionChecker.LatestVersion.Id})");
                Log.Information($"Version is not up-to-date (local {versionChecker.CurrentVersion.Id} vs ext {versionChecker.LatestVersion.Id}).");
            }
            else
            {
                Log.Information($"Version is up to date ({versionChecker.CurrentVersion.Id}).");
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
            "packages cache",
            "fonts",
            "fnv hashes",
            "hash 64",
            "investment",
            "global string cache",
            "activity names",
        });
        // to check if we need to update caches
        PackageHandler.Initialise();
        Progress.CompleteStage();

        // Load all the fonts
        await Task.Run(() =>
        {
            RegisterFonts(FontHandler.Initialise());
        });
        Progress.CompleteStage();

        // Get all hash64 -- must be before InvestmentHandler
        await Task.Run(TagHash64Handler.Initialise);
        Progress.CompleteStage();

		// Initialise FNV handler -- must be first bc my code is shit
		await Task.Run(FnvHandler.Initialise);
		Progress.CompleteStage();

		// Initialise investment
		await Task.Run(InvestmentHandler.Initialise);
        Progress.CompleteStage();

        // Initialise global string cache
        await Task.Run(PackageHandler.GenerateGlobalStringContainerCache);
        Progress.CompleteStage();

        // Get all activity names
        await Task.Run(PackageHandler.GetAllActivityNames);
        Progress.CompleteStage();

        // Set texture format
        TextureExtractor.SetTextureFormat(ConfigHandler.GetOutputTextureFormat());
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

    public void HideMainMenu()
    {
        MainMenuTab.Visibility = Visibility.Collapsed;
    }

    public void ShowMainMenu()
    {
        MainMenuTab.Visibility = Visibility.Visible;
        MainTabControl.SelectedItem = MainMenuTab;
        if (_bHasInitialised == false)
        {
            Task.Run(InitialiseHandlers);
            _bHasInitialised = true;
        }
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
        name = name.Replace('_', '.');
        // Check if the name already exists, if so set newest tab to that
        var items = MainTabControl.Items;
        foreach (TabItem item in items)
        {
            if (name == (string)item.Header)
            {
                _newestTab = item;
                return;
            }
        }

        _newestTab = new TabItem();
        _newestTab.Content = content;
        _newestTab.MouseDown += MenuTab_OnMouseDown;
        MainTabControl.Items.Add(_newestTab);
        SetNewestTabName(name);
    }

    private void MenuTab_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle && e.Source is TabItem)
        {
            TabItem tab = (TabItem)sender;
            MainTabControl.Items.Remove(tab);
            dynamic content = tab.Content;
            if (content is ActivityView av)
            {
                av.Dispose();
            }
        }
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.D && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            MakeNewTab("Dev", new DevView());
            SetNewestTabSelected();
        }
        else if (e.Key == Key.C 
                 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
                 && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift
                 && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            throw new ExternalException("Crash induced.");
        }
    }
}