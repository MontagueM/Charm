using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Tiger;
using Tiger.Schema;
using VersionChecker;
using static Charm.CategoryView;
using static Charm.CollectionsView;

namespace Charm;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public static ProgressView Progress = null;
    private static TabItem _newestTab = null;
    public TabItem CurrentTab = null;
    private static LogView _logView = null;
    private static TabItem _logTab = null;
    private bool _bHasInitialised = false;
    public FileVersionInfo GameInfo = null;

    public static MainWindow Current;
    public Spinner2 Spinner;
    public Tooltip2 _ToolTip => ToolTip;
    public TabControl _MainTabControl => MainTabControl;

    public MainWindow()
    {
        InitializeComponent();
        Current = this;
        Progress = ProgressView;
        Initialize();
        CompositionTarget.Rendering += OnRender;
    }

    private void OnRender(object sender, EventArgs e)
    {
        if (!ConfigSubsystem.Get().GetMotionEffects())
            return;

        float x = -12f / (float)this.ActualWidth;
        float y = -12f / (float)this.ActualHeight;

        System.Windows.Point position = Mouse.GetPosition(this);
        TranslateTransform gridTransform = (TranslateTransform)OverlayRoot.RenderTransform;
        gridTransform.X = (int)Math.Round(position.X * x);
        gridTransform.Y = (int)Math.Round(position.Y * y);
    }

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

    public void Initialize()
    {
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
                CurrentStrategyText.Text = $"{App.CurrentVersion.Id}: {args.Strategy.GetEnumDescription().ToUpper()}";
            });
        };

        InitialiseSubsystems();

        if (ConfigSubsystem.Get().GetAnimatedBackground())
        {
            if (Spinner is null)
                Spinner = new Spinner2((int)Width, (int)Height);

            SpinnerContainer.Children.Add(Spinner);
        }
        else
            SpinnerContainer.Visibility = Visibility.Collapsed;

        _logView = new LogView();
        LogHandler.Initialise(_logView);

        // Hide tab by default
        HideMainMenu();

        // Check if packages path exists in config
        // ConfigSubsystem.CheckPackagesPathIsValid();
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        if (config.GetPackagesPath(Strategy.CurrentStrategy) != "" && config.GetExportSavePath() != "")
        {
            MainMenuTab.Visibility = Visibility.Visible;

            // Check version
            CheckVersion();

            // Log game version
            CheckGameVersion();

            if (!ConfigSubsystem.Get().GetAcceptedAgreement())
            {
                ShowAgreement();
            }

            // Log package count and package path
            Arithmic.Log.Info($"Package Path: {config.GetPackagesPath(Strategy.CurrentStrategy)}");
            Arithmic.Log.Info($"Total Package Count: {Directory.GetFiles(config.GetPackagesPath(Strategy.CurrentStrategy)).Where(x => x.EndsWith(".pkg")).Count()}");
        }
        else
        {
            //MakeNewTab("Configuration", new ConfigView());
            SetCurrentTab("settings");
            SetNewestTabSelected();
        }

        Strategy.AfterStrategyEvent += delegate (StrategyEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                NotificationBanner versionChanged = new()
                {
                    Icon = "",
                    Title = "GAME VERSION",
                    Description = $"Changed game version to {EnumExtensions.GetEnumDescription(args.Strategy)}",
                    Style = NotificationBanner.PopupStyle.Information
                };
                versionChanged.Show();
            });
        };

        // Global ToolTip detection
        EventManager.RegisterClassHandler(
            typeof(ButtonBase),
            UIElement.MouseEnterEvent,
            new MouseEventHandler(OnAnyButtonMouseEnter)
        );

        EventManager.RegisterClassHandler(
            typeof(ButtonBase),
            UIElement.MouseLeaveEvent,
            new MouseEventHandler(OnAnyButtonMouseLeave)
        );
    }

    private void ShowAgreement()
    {
        PopupBanner warn = new()
        {
            DarkenBackground = true,
            //Icon = "⚠️",
            Title = "ATTENTION",
            Subtitle = "Charm is NOT a datamining tool!",
            Description =
            "Charm is intended for 3D artists, content preservation, and understanding how the Tiger engine works." +
            "\n\nBy using Charm, you agree to the following:" +
            "\n• You will not use Charm to share spoilers or ruin the experience for others." +
            "\n• You will not use Charm to leak or distribute unreleased content." +
            "\n     - Including but not limited to screenshots, recordings, or exports." +
            "\n• You will not use Charm in any way that violates Bungie’s Terms of Service." +
            "\n     - Including but not limited to using code to develop cheats and/or exploits." +
            "\n\nBreaking any of the above reduces the likelihood of future public releases and updates. Don't ruin it for others." +
            "\nDiscover things the way they were intended!",

            Style = PopupBanner.PopupStyle.Warning,
            UserInput = $"Accept{(!FontHandler.FontsLoaded ? " (Left Mouse)" : "")}",
            UserInputSecondary = $"Reject{(!FontHandler.FontsLoaded ? " (Right Mouse)" : "")}",
            HoldDuration = 4000,
            Progress = true
        };
        warn.MouseRightButtonDown += (s, e) =>
        {
            warn.Remove(true);
            PopupBanner warn2 = new()
            {
                DarkenBackground = true,
                //Icon = "⚠️",
                Title = "THAT'S TOO BAD",
                Subtitle = "You must accept the agreement to use Charm!",
                Description = "Charm will now close. You can try reading it again if you want.",
                Style = PopupBanner.PopupStyle.Warning,
                UserInput = "Okay",
            };
            warn2.OnProgressComplete += () => Application.Current.Shutdown(0);
            warn2.Show();
        };

        warn.OnProgressComplete += () => ConfigSubsystem.Get().SetAcceptedAgreement(true);
        warn.Show();
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

    private static List<Type> SortByInitializationOrder(IEnumerable<Type> types)
    {
        var dependencyMap = new Dictionary<Type, List<Type>>();
        var dependencyCount = new Dictionary<Type, int>();

        // Build dependency map and count dependencies
        foreach (Type type in types)
        {
            object[] attributes = type.GenericTypeArguments[0].GetCustomAttributes(typeof(InitializeAfterAttribute), true);
            foreach (InitializeAfterAttribute attribute in attributes)
            {
                Type? dependentType = attribute.TypeToInitializeAfter.GetNonGenericParent(
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
            Type type = queue.Dequeue();
            sortedTypes.Add(type);

            if (dependencyMap.ContainsKey(type))
            {
                foreach (Type dependentType in dependencyMap[type])
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
        TigerInstance.Args = new TigerArgs(args);
        TigerInstance.InitialiseSubsystems();
        Arithmic.Log.Info("Initialised Charm subsystems");

    }

    private void CheckGameVersion()
    {
        try
        {
            ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
            string path = config.GetPackagesPath(Strategy.CurrentStrategy).Split("packages")[0] + "destiny2.exe";
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
        Arithmic.Log.Info($"Charm Version: {App.CurrentVersion.Id}");
        var versionChecker = new ApplicationVersionChecker("https://github.com/MontagueM/Charm/raw/delta/EOF", App.CurrentVersion);
        versionChecker.LatestVersionName = "version";
        try
        {
            ApplicationVersion latestVersion = await versionChecker.GetLatestVersion();
            int latestID = int.Parse(latestVersion.Id.Replace(".", ""));
            int currentID = int.Parse(App.CurrentVersion.Id.Replace(".", ""));

            bool upToDate = currentID >= latestID;
            if (!upToDate)
            {
                //MessageBox.Show($"New version available on GitHub! (local {versionChecker.CurrentVersion.Id} vs ext {versionChecker.LatestVersion.Id})");
                Arithmic.Log.Info($"Version is not up-to-date (Local {versionChecker.CurrentVersion.Id} vs Github {latestVersion.Id}).");

                PopupBanner update = new()
                {
                    DarkenBackground = true,
                    Icon = "",
                    Title = "UPDATE AVAILABLE",
                    Subtitle = "A new Charm update is available!",
                    Description =
                    $"Current Version: v{App.CurrentVersion.Id}\n" +
                    $"Latest Version: v{latestVersion.Id}",
                    UserInput = "Update",
                    UserInputSecondary = "Dismiss"
                };

                update.MouseLeftButtonDown += OpenLatestRelease;
                update.MouseRightButtonDown += update.Remove;

                update.Style = PopupBanner.PopupStyle.Information;
                update.Show();
            }
            else
            {
                Arithmic.Log.Info($"Version is up to date (Local v{versionChecker.CurrentVersion.Id}, Github v{latestVersion.Id}).");
            }
        }
        catch (Exception e)
        {
            Arithmic.Log.Error($"Could not get version. Error {e}.");
        }
    }

    private void OpenLatestRelease(object sender, MouseButtonEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = $"https://github.com/MontagueM/Charm/releases/latest", UseShellExecute = true });
    }

    private async void InitialiseHandlers()
    {
        // Set texture format
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        TextureExtractor.SetTextureFormat(config.GetOutputTextureFormat());
    }

    private void OpenLogPanel_OnClick(object sender, RoutedEventArgs e)
    {
        MakeNewTab("Log", _logView);
        SetNewestTabSelected();
    }

    public void SetLoggerSelected()
    {
        MainTabControl.SelectedItem = _logTab;
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

        if (!ConfigSubsystem.Get().GetAcceptedAgreement())
        {
            ShowAgreement();
        }
    }

    public void SetNewestTabSelected()
    {
        MainTabControl.SelectedItem = _newestTab;
    }

    public void SetNewestTabName(string newName)
    {
        _newestTab.Header = newName.Replace('_', '.');
    }

    public bool SetCurrentTab(string name)
    {
        // Testing making it all caps
        name = name.ToUpper();
        name = name.Replace('_', '.');
        // Check if the name already exists, if so set newest tab to that
        ItemCollection items = MainTabControl.Items;
        foreach (TabItem item in items)
        {
            if (name == (string)item.Header)
            {
                _newestTab = item;
                SetNewestTabSelected();
                return true;
            }
        }
        return false;
    }

    public void MakeNewTab(string name, UserControl content)
    {
        // Testing making it all caps
        name = name.ToUpper();
        name = name.Replace('_', '.');
        // Check if the name already exists, if so set newest tab to that
        ItemCollection items = MainTabControl.Items;
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
            else if (content is AudioListView audioView)
            {
                audioView.MusicPlayer.Dispose();
            }
        }
    }

    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl tabControl)
        {
            if (tabControl.SelectedItem is TabItem selectedTab)
            {
                CurrentTab = selectedTab;
                switch (selectedTab.Content)
                {
                    case null: // bug, first time start up
                        if (Spinner is not null)
                            Spinner.PositionScale = new(2, 2, -1, -1);
                        break;
                    case MainMenuView:
                        UIHelper.AnimateFade(SpinnerContainer, 0.1f, 1.0f, 0.5f);
                        if (Spinner is not null)
                            Spinner.PositionScale = new(2, 2, -1, -1);
                        break;
                    case ConfigView:
                        if (Spinner is not null)
                            Spinner.PositionScale = new(4f, 4f, -3.6f, -3.3f);
                        UIHelper.AnimateFade(SpinnerContainer, 0.1f, 0.5f, 1);
                        break;
                    default:
                        if (Spinner is not null)
                            Spinner.PositionScale = new(100f, 100f, -100f, -100f); // Setting all to 0 has bad side effects
                        UIHelper.AnimateFade(SpinnerContainer, 0.1f, 0.5f, 1);
                        break;
                }
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
            throw new ExternalException("Crash induced. I don't know why you did that but good job.");
        }
        else if (e.Key == Key.Escape)
        {
            var tab = (TabItem)MainTabControl.Items[MainTabControl.SelectedIndex];
            dynamic content = tab.Content;
            if (content is ItemView or CategoryView)
                MainTabControl.Items.Remove(tab);
        }
        else if (e.Key == Key.W
            && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
            && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            PopupBanner test = new()
            {
                DarkenBackground = false,
                //Icon = "ℹ️",
                Title = "INFORMATION",
                Subtitle = "Test Information Popup Subtitle",
                Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                Style = PopupBanner.PopupStyle.Information
            };
            test.Show();

            NotificationBanner test2 = new()
            {
                Icon = "",
                Title = "WAYPOINT ADDED",
                Description = "The location of this quest is highlighted on your map.",
                Style = NotificationBanner.PopupStyle.Information
            };
            test2.Show();
        }
        else if (e.Key == Key.E
            && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
            && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            PopupBanner test = new()
            {
                DarkenBackground = false,
                //Icon = "⚠️",
                Title = "ERROR",
                Subtitle = "Test Error Popup Subtitle",
                Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\n\nError code: Valumptious",
                Style = PopupBanner.PopupStyle.Warning,
                UserInput = "Hold To Accept",
                HoldDuration = 300,
                Progress = true
            };
            test.Show();

            //PopupBanner test = new()
            //{
            //    DarkenBackground = false,
            //    Icon = "⚠️",
            //    IconImage = ApiImageUtils.MakeBitmapImage(new Texture(new FileHash("7180DC80")).GetTexture(), 648, 495),
            //    Title = "OOPS",
            //    Subtitle = "WE DELETED THE FUCKING SERVERS",
            //    Description = "Jimmy the new intern downloaded a 72 yottabyte zip bomb and deleted all of our server data. The game is gone.\n\nThank you for all of your time and money for Pete...I mean supporting Destiny 2!",
            //    Style = PopupBanner.PopupStyle.Warning,
            //};
            //test.Show();

            NotificationBanner test2 = new()
            {
                Icon = "",
                Title = "ATTENTION",
                Description = "Contacting Destiny 2 servers.",
                Style = NotificationBanner.PopupStyle.Warning
            };
            test2.Show();
        }
        else if (e.Key == Key.Q
            && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
            && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            PopupBanner test = new()
            {
                DarkenBackground = false,
                //Icon = "💬",
                Title = "GENERAL",
                Subtitle = "Test General Popup Subtitle",
                Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                UserInput = "Ok",
                Style = PopupBanner.PopupStyle.Generic
            };
            test.Show();

            NotificationBanner test2 = new()
            {
                Icon = "",
                Title = "EVERVERSE",
                Description = "Buy Silver now! Pete needs a new car!",
                Style = NotificationBanner.PopupStyle.Generic
            };
            test2.Show();
        }
        else if (e.Key == Key.A
            && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
            && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt
            && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
        {
            ShowAgreement();
        }
    }

    public static BitmapSource GetBitmapSource(Icon icon)
    {
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                 icon.Handle,
                 new Int32Rect(0, 0, icon.Width, icon.Height),
                 BitmapSizeOptions.FromEmptyOptions());
    }

    private void OnAnyButtonMouseEnter(object sender, MouseEventArgs e)
    {
        FrameworkElement element = sender as FrameworkElement;
        if (element != null)
        {
            ToolTip.ActiveItem = element;
            if (element.DataContext != null && !GenericTooltipProperties.HasTooltipData(element))
            {
                switch (element.DataContext)
                {
                    case APIPlugItem item:
                        ToolTip.MakeTooltip(item.Item, item.ParentSocketStyle);
                        break;
                    case Category item:
                        ToolTip.MakeTooltip(item);
                        break;

                    case CategoryEntry item:
                        if (item.EntryType == CategoryEntryType.Record)
                        {
                            ToolTip.MakeTooltip(item);
                        }
                        else if (item.EntryType == CategoryEntryType.Collectible)
                        {
                            ToolTip.MakeTooltip(item.Item);
                        }
                        break;
                }
            }
            else
            {
                var tooltipData = GenericTooltipProperties.GetTooltipData(element);
                if (tooltipData == null)
                {
                    // If not set directly, look up the visual tree to check the parent
                    DependencyObject current = element;

                    while (current != null)
                    {
                        if (current is not ContainerVisual)
                        {
                            tooltipData = GenericTooltipProperties.GetTooltipData((UIElement)current);
                            if (tooltipData != null)
                                break;
                        }
                        else
                            return;

                        current = VisualTreeHelper.GetParent(current);
                    }
                }

                if (tooltipData != null)
                {
                    ToolTip.MakeTooltip(tooltipData);
                }
            }
        }
    }

    private void OnAnyButtonMouseLeave(object sender, MouseEventArgs e)
    {
        ToolTip.ActiveItem = null;
        ToolTip.ClearTooltip();
    }
}
