using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Tiger;
using Tiger.Schema.Investment;

namespace Charm;

public partial class MainMenuView : UserControl
{
    private static MainWindow _mainWindow = null;

    public MainMenuView()
    {
        InitializeComponent();

        ApiButton.IsEnabled = ShowAPIButton(Strategy.CurrentStrategy);
        BagsButton.IsEnabled = ShowWQButtons(Strategy.CurrentStrategy);
        WeaponAudioButton.IsEnabled = ShowIfLatest(Strategy.CurrentStrategy) || ShowIfD1(Strategy.CurrentStrategy);
        StaticsButton.IsEnabled = ShowIfD2(Strategy.CurrentStrategy);
        SoundBanksButton.Visibility = ShowIfD1(Strategy.CurrentStrategy) ? Visibility.Visible : Visibility.Hidden;
        CollectionsButton.IsEnabled = ShowIfLatest(Strategy.CurrentStrategy);

        Strategy.OnStrategyChangedEvent += delegate (StrategyEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                ApiButton.IsEnabled = ShowAPIButton(args.Strategy);
                BagsButton.IsEnabled = ShowWQButtons(args.Strategy);
                WeaponAudioButton.IsEnabled = ShowIfLatest(args.Strategy) || ShowIfD1(args.Strategy);
                StaticsButton.IsEnabled = ShowIfD2(args.Strategy);
                SoundBanksButton.Visibility = ShowIfD1(Strategy.CurrentStrategy) ? Visibility.Visible : Visibility.Hidden;
                CollectionsButton.IsEnabled = ShowIfLatest(Strategy.CurrentStrategy);
            });
        };
    }

    private bool ShowWQButtons(TigerStrategy strategy)
    {
        return strategy > TigerStrategy.DESTINY2_BEYONDLIGHT_3402;
    }

    private bool ShowIfD2(TigerStrategy strategy)
    {
        return strategy != TigerStrategy.DESTINY1_RISE_OF_IRON;
    }

    private bool ShowIfD1(TigerStrategy strategy)
    {
        return strategy == TigerStrategy.DESTINY1_RISE_OF_IRON;
    }

    private bool ShowIfLatest(TigerStrategy strategy)
    {
        return Strategy.CurrentStrategy == TigerStrategy.DESTINY2_LATEST;
    }

    private bool ShowAPIButton(TigerStrategy strategy)
    {
        return strategy > TigerStrategy.DESTINY2_BEYONDLIGHT_3402 || strategy == TigerStrategy.DESTINY1_RISE_OF_IRON;
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        GameVersion.Text = $"Game Version: {_mainWindow.GameInfo?.FileVersion}";
        MouseMove += UserControl_MouseMove;

        if (ConfigSubsystem.Get().GetAnimatedBackground())
        {
            SpinnerShader _spinner = new SpinnerShader();
            Spinner.Effect = _spinner;
            SizeChanged += _spinner.OnSizeChanged;
            _spinner.ScreenWidth = (float)ActualWidth;
            _spinner.ScreenHeight = (float)ActualHeight;
            _spinner.Scale = new(2, 2);
            _spinner.Offset = new(-1, -1);
        }
    }

    private async void ApiViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        await LoadInvestment();

        DareView apiView = new DareView();
        apiView.LoadContent();
        _mainWindow.MakeNewTab("api", apiView);
        _mainWindow.SetNewestTabSelected();
    }

    private async void CollectionsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        await LoadInvestment();

        CollectionsView apiView2 = new CollectionsView();
        apiView2.LoadContent();
        _mainWindow.MakeNewTab("Collections", apiView2);
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

    private async void WeaponAudioViewButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadInvestment();

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

    private void AllBKHDViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.BKHDGroupList);
        _mainWindow.MakeNewTab("sound banks", tagListView);
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

    private void AllMaterialsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.MaterialList);
        _mainWindow.MakeNewTab("materials", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void GithubButton_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = "https://github.com/MontagueM/Charm/tree/delta/TFS%2Bmisc", UseShellExecute = true });
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        System.Windows.Point position = e.GetPosition(this);
        TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        gridTransform.X = position.X * -0.0075;
        gridTransform.Y = position.Y * -0.0075;
    }

    private async Task LoadInvestment()
    {
        MainWindow.Progress.SetProgressStages(new() { "Loading Investment System" });
        await Task.Run(() => Investment.LazyInit());
        MainWindow.Progress.CompleteStage();
    }
}
