using System.Windows;
using System.Windows.Controls;
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

        Strategy.OnStrategyChangedEvent += delegate (StrategyEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                ApiButton.IsEnabled = ShowAPIButton(args.Strategy);
                BagsButton.IsEnabled = ShowWQButtons(args.Strategy);
                WeaponAudioButton.IsEnabled = ShowIfLatest(args.Strategy) || ShowIfD1(args.Strategy);
                StaticsButton.IsEnabled = ShowIfD2(args.Strategy);
                SoundBanksButton.Visibility = ShowIfD1(Strategy.CurrentStrategy) ? Visibility.Visible : Visibility.Hidden;
            });
        };
    }

    private bool ShowWQButtons(TigerStrategy strategy)
    {
        return strategy > TigerStrategy.DESTINY2_BEYONDLIGHT_3402;
    }

    private bool ShowIfD2(TigerStrategy strategy)
    {
        return strategy >= TigerStrategy.DESTINY2_SHADOWKEEP_2601;
    }

    private bool ShowIfD1(TigerStrategy strategy)
    {
        return strategy == TigerStrategy.DESTINY1_RISE_OF_IRON;
    }

    private bool ShowIfLatest(TigerStrategy strategy)
    {
        return strategy == TigerStrategy.DESTINY2_LATEST;
    }

    private bool ShowAPIButton(TigerStrategy strategy)
    {
        return strategy > TigerStrategy.DESTINY2_BEYONDLIGHT_3402 || strategy == TigerStrategy.DESTINY1_RISE_OF_IRON;
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
    }

    private void ApiViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        // TagListViewerView apiView = new TagListViewerView();
        // apiView.LoadContent(ETagListType.ApiList);
        // todo actually make this show the progress bar, cba rn
        MainWindow.Progress.SetProgressStages(new() { "Start investment system" });
        Investment.LazyInit();
        MainWindow.Progress.CompleteStage();

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
        // todo actually make this show the progress bar, cba rn
        MainWindow.Progress.SetProgressStages(new() { "Start investment system" });
        Investment.LazyInit();
        MainWindow.Progress.CompleteStage();

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
}
