using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
    public FileVersionInfo GameInfo = null;

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        if (MainMenuTab.Visibility == Visibility.Visible)
        {
            Task.Run(InitialiseHandlers);
            _bHasInitialised = true;
        }

        Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
        CharmIcon.Source = GetBitmapSource(appIcon);
    }

    public MainWindow()
    {
        InitializeComponent();

        Progress = ProgressView;

        int numSingletons = InitialiseStrategistSingletons();

        Strategy.BeforeStrategyEvent += args => { Progress.SetProgressStages(Enumerable.Range(1, numSingletons).Select(num => $"Initialising game version {args.Strategy}: {num}/{numSingletons}").ToList()); };
        Strategy.DuringStrategyEvent += _ => { Progress.CompleteStage(); };
        Strategy.OnStrategyChangedEvent += args =>
        {
            Dispatcher.Invoke(() =>
            {
                // remove all tabs marked with .Tag == 1 as this means we added it manually
                MainTabControl.Items.SourceCollection
                    .Cast<TabItem>()
                    .Where(t => t.Tag is 1 && !t.Header.ToString().Contains("configuration", StringComparison.InvariantCultureIgnoreCase))
                    .ToList()
                    .ForEach(t => MainTabControl.Items.Remove(t));
                CurrentStrategyText.Text = args.Strategy.ToString().Split(".").Last();
            });
        };

        InitialiseSubsystems();

        _logView = new LogView();
        LogHandler.Initialise(_logView);

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

        Strategy.AfterStrategyEvent += delegate (StrategyEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                if (Commandlet.RunCommandlet())
                {
                    // Environment.Exit(0);
                }
            });
        };
    }

    private int InitialiseStrategistSingletons()
    {
        HashSet<Type> lazyStrategistSingletons = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Select(t => t.GetNonGenericParent(typeof(Strategy.LazyStrategistSingleton<>)))
            .Where(t => t is { ContainsGenericParameters: false })
            .Select(t => t.GetNonGenericParent(typeof(Strategy.StrategistSingleton<>)))
            .ToHashSet();

        // Get all classes that inherit from StrategistSingleton<>
        // Then call RegisterEvents() on each of them
        HashSet<Type> allStrategistSingletons = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Select(t => t.GetNonGenericParent(typeof(Strategy.StrategistSingleton<>)))
            .Where(t => t is { ContainsGenericParameters: false })
            .ToHashSet();

        allStrategistSingletons.ExceptWith(lazyStrategistSingletons);

        // order dependencies from InitializesAfterAttribute
        List<Type> strategistSingletons = SortByInitializationOrder(allStrategistSingletons.ToList()).ToList();

        foreach (Type strategistSingleton in strategistSingletons)
        {
            strategistSingleton.GetMethod("RegisterEvents", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        }

        return strategistSingletons.Count;
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
        Arithmic.Log.Info("Initialising Charm subsystems");
        string[] args = Environment.GetCommandLineArgs();
        CharmInstance.Args = new CharmArgs(args);
        CharmInstance.InitialiseSubsystems();
        Arithmic.Log.Info("Initialised Charm subsystems");

    }

    private void CheckGameVersion()
    {
        try
        {
            ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
            var path = config.GetPackagesPath(Strategy.CurrentStrategy).Split("packages")[0] + "destiny2.exe";
            var versionInfo = FileVersionInfo.GetVersionInfo(path);
            string version = versionInfo.FileVersion;
            GameInfo = versionInfo;
            Arithmic.Log.Info("Game version: " + version);
        }
        catch (Exception e)
        {
            Arithmic.Log.Error($"Could not get game version error {e}.");
        }
    }

    private async void CheckVersion()
    {
        var currentVersion = new ApplicationVersion("2.0.0");
        var versionChecker = new ApplicationVersionChecker("https://github.com/MontagueM/Charm/raw/main/", currentVersion);
        versionChecker.LatestVersionName = "version";
        try
        {
            var upToDate = await versionChecker.IsUpToDate();
            if (!upToDate)
            {
                MessageBox.Show($"New version available on GitHub! (local {versionChecker.CurrentVersion.Id} vs ext {versionChecker.LatestVersion.Id})");
                Arithmic.Log.Info($"Version is not up-to-date (local {versionChecker.CurrentVersion.Id} vs ext {versionChecker.LatestVersion.Id}).");
            }
            else
            {
                Arithmic.Log.Info($"Version is up to date ({versionChecker.CurrentVersion.Id}).");
            }
        }
        catch (Exception e)
        {
            // Could not get or parse version file
#if !DEBUG
            MessageBox.Show("Could not get version.");
#endif
            Arithmic.Log.Error($"Could not get version error {e}.");
        }
    }

    private async void InitialiseHandlers()
    {
        // Set texture format
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        TextureExtractor.SetTextureFormat(config.GetOutputTextureFormat());
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
        // MainTabControl.SelectedItem = MainMenuTab;
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
        _newestTab.Tag = 1;
        _newestTab.MouseDown += MenuTab_OnMouseDown;
        _newestTab.HorizontalAlignment = HorizontalAlignment.Left;
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
        else if (e.Key == Key.Escape)
        {
            var tab = (TabItem)MainTabControl.Items[MainTabControl.SelectedIndex];
            dynamic content = tab.Content;
            if (content is APIItemView || content is CategoryView)
                MainTabControl.Items.Remove(tab);
        }
    }

    private BitmapSource GetBitmapSource(Icon icon)
    {
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                 icon.Handle,
                 new Int32Rect(0, 0, icon.Width, icon.Height),
                 BitmapSizeOptions.FromEmptyOptions());
    }
}
