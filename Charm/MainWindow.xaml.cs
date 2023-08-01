using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Arithmic;
using Tiger;
using Tiger.Schema;
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

        Strategy.BeforeStrategyEvent += ((StrategyEventArgs e, int invocationCount) => { Progress.SetProgressStages(Enumerable.Range(1, invocationCount).Select(num => $"Initialising game version {e.Strategy}: {num}/{invocationCount}").ToList()); });
        Strategy.DuringStrategyEvent += ((StrategyEventArgs e) => { Progress.CompleteStage(); });
        Strategy.AfterStrategyEvent += ((StrategyEventArgs e) => { Progress.CompleteStage(); });

        InitialiseStrategistSingletons();
        InitialiseSubsystems();

        _logView = new LogView();
        LogHandler.Initialise(_logView);

        // Make log
        MakeNewTab("Log", _logView);
        _logTab = _newestTab;

        // Hide tab by default
        HideMainMenu();

        // Check if packages path exists in config
        // ConfigSubsystem.CheckPackagesPathIsValid();
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        if (config.GetPackagesPath(Strategy.CurrentStrategy) != "" && config.GetExportSavePath() != "")
        {
            MainMenuTab.Visibility = Visibility.Visible;
        }
        else
        {
            MakeNewTab("Configuration", new ConfigView());
            SetNewestTabSelected();
        }

        // Check version
        CheckVersion();

        // Log game version
        CheckGameVersion();

        if (Commandlet.RunCommandlet())
        {
            Environment.Exit(0);
        }
    }

    private void InitialiseStrategistSingletons()
    {
        // Get all classes that inherit from StrategistSingleton<>
        // Then call RegisterEvents() on each of them
        List<Type> allStrategistSingletons = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Select(t => t.GetNonGenericParent(typeof(Strategy.StrategistSingleton<>)))
            .Where(t => t != null)
            .ToHashSet().ToList();

        // order dependencies from InitializesAfterAttribute
        allStrategistSingletons = SortByInitializationOrder(allStrategistSingletons).ToList();

        foreach (Type strategistSingleton in allStrategistSingletons)
        {
            strategistSingleton.GetMethod("RegisterEvents", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        }
    }

    private static IEnumerable<Type> SortByInitializationOrder(IEnumerable<Type> types)
    {
            var dependencyMap = new Dictionary<Type, List<Type>>();
            var dependencyCount = new Dictionary<Type, int>();

            // Build dependency map and count dependencies
            foreach (var type in types)
            {
                var attributes = type.GenericTypeArguments[0].GetCustomAttributes(typeof(InitializeAfterAttribute), true);
                foreach (InitializeAfterAttribute attribute in attributes)
                {
                    var dependentType = attribute.TypeToInitializeAfter.GetNonGenericParent(
                        typeof(Strategy.StrategistSingleton<>));
                    if (!dependencyMap.ContainsKey(dependentType))
                    {
                        dependencyMap[dependentType] = new List<Type>();
                        dependencyCount[dependentType] = 0;
                    }
                    dependencyMap[dependentType].Add(type);
                    dependencyCount[type] = dependencyCount.ContainsKey(type) ? dependencyCount[type] + 1 : 1;
                }
            }

            // Perform topological sorting
            var sortedTypes = types.Where(t => !dependencyCount.ContainsKey(t)).ToList();
            var queue = new Queue<Type>(dependencyMap.Keys.Where(k => dependencyCount[k] == 0));
            while (queue.Count > 0)
            {
                var type = queue.Dequeue();
                sortedTypes.Add(type);

                if (dependencyMap.ContainsKey(type))
                {
                    foreach (var dependentType in dependencyMap[type])
                    {
                        dependencyCount[dependentType]--;
                        if (dependencyCount[dependentType] == 0)
                        {
                            queue.Enqueue(dependentType);
                        }
                    }
                }
            }

            if (sortedTypes.Count < types.Count())
            {
                throw new InvalidOperationException("Circular dependency detected.");
            }

            return sortedTypes;
        }

    private void InitialiseSubsystems()
    {
        Log.Info("Initialising Charm subsystems");
        string[] args = Environment.GetCommandLineArgs();
        CharmInstance.Args = new CharmArgs(args);
        CharmInstance.InitialiseSubsystems();
        Log.Info("Initialised Charm subsystems");

    }

    private void CheckGameVersion()
    {
        try
        {
            ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
            var path = config.GetPackagesPath(Strategy.CurrentStrategy).Split("packages")[0] + "destiny2.exe";
            var versionInfo = FileVersionInfo.GetVersionInfo(path);
            string version = versionInfo.FileVersion;
            Log.Info("Game version: " + version);
        }
        catch (Exception e)
        {
            Log.Error($"Could not get game version error {e}.");
        }
    }

    private async void CheckVersion()
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
                Log.Info($"Version is not up-to-date (local {versionChecker.CurrentVersion.Id} vs ext {versionChecker.LatestVersion.Id}).");
            }
            else
            {
                Log.Info($"Version is up to date ({versionChecker.CurrentVersion.Id}).");
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
        // Progress.SetProgressStages(new List<string>
        // {
        //     "packages cache",
        //     "fonts",
        //     "fnv hashes",
        //     "hash 64",
        //     "investment",
        //     "global string cache",
        //     "activity names",
        // });
        // to check if we need to update caches
        // PackageHandler.Initialise();
        // Progress.CompleteStage();

        // Load all the fonts
        // await Task.Run(() =>
        // {
        //     RegisterFonts(FontHandler.Initialise());
        // });
        // Progress.CompleteStage();

        // // Initialise FNV handler -- must be first bc my code is shit
        // await Task.Run(FnvHandler.Initialise);
        // Progress.CompleteStage();
        //
        // // Get all hash64 -- must be before InvestmentHandler
        // await Task.Run(TagHash64Handler.Initialise);
        // Progress.CompleteStage();
        //
        // // Initialise investment
        // await Task.Run(InvestmentHandler.Initialise);
        // Progress.CompleteStage();
        //
        // // Initialise global string cache
        // await Task.Run(PackageHandler.GenerateGlobalStringContainerCache);
        // Progress.CompleteStage();
        //
        // // Get all activity names
        // await Task.Run(PackageHandler.GetAllActivityNames);
        // Progress.CompleteStage();

        // Set texture format
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        TextureExtractor.SetTextureFormat(config.GetOutputTextureFormat());
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

public static class NestedTypeHelpers
{
    public static Type? FindNestedGenericType<T>()
    {
        Type? nestedType = null;

        Type testType = typeof(T);
        while (nestedType == null && testType != null && testType != typeof(object))
        {
            if (testType.IsGenericType)
            {
                nestedType = testType.GenericTypeArguments[0];
            }
            else
            {
                testType = testType.BaseType;
            }
        }

        return nestedType;
    }

    public static Type? GetNonGenericParent(this Type inTestType, Type inheritParentType)
    {
        Type? testType = inTestType;
        while (testType != null && testType != typeof(object))
        {
            if (testType.IsGenericType && testType.GenericTypeArguments.Length > 0 && testType.GetGenericTypeDefinition() == inheritParentType)
            {
                return testType;
            }
            else
            {
                testType = testType.BaseType;
            }
        }

        return null;
    }
}
