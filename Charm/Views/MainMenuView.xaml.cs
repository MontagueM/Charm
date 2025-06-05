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
        BagsButton.IsEnabled = ShowIfD2(Strategy.CurrentStrategy);
        WeaponAudioButton.IsEnabled = ShowAPIButton(Strategy.CurrentStrategy);
        StaticsButton.IsEnabled = ShowIfD2(Strategy.CurrentStrategy);
        SoundBanksButton.Visibility = ShowIfD1(Strategy.CurrentStrategy) ? Visibility.Visible : Visibility.Hidden;
        CollectionsButton.IsEnabled = ShowIfLatest(Strategy.CurrentStrategy);

        Strategy.OnStrategyChangedEvent += delegate (StrategyEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                ApiButton.IsEnabled = ShowAPIButton(args.Strategy);
                BagsButton.IsEnabled = ShowIfD2(args.Strategy);
                WeaponAudioButton.IsEnabled = ShowIfLatest(args.Strategy) || ShowIfD1(args.Strategy);
                StaticsButton.IsEnabled = ShowIfD2(args.Strategy);
                SoundBanksButton.Visibility = ShowIfD1(args.Strategy) ? Visibility.Visible : Visibility.Hidden;
                CollectionsButton.IsEnabled = ShowIfLatest(args.Strategy);
            });
        };
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        GameVersion.Text = $"Game Version: {_mainWindow.GameInfo?.FileVersion}";
        MouseMove += UserControl_MouseMove;

        if (MainWindow.Current.Spinner is not null)
            MainWindow.Current.Spinner.PositionScale = new(2, 2, -1, -1);
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
        return strategy is TigerStrategy.DESTINY1_RISE_OF_IRON;
    }

    private bool ShowIfLatest(TigerStrategy strategy)
    {
        return strategy is TigerStrategy.DESTINY2_LATEST;
    }

    private bool ShowAPIButton(TigerStrategy strategy)
    {
        return strategy is TigerStrategy.DESTINY2_LATEST or TigerStrategy.DESTINY1_RISE_OF_IRON;
    }

    private void CategoryButton_MouseEnter(object sender, MouseEventArgs e)
    {
    }

    public void CategoryButton_MouseLeave(object sender, MouseEventArgs e)
    {
    }

    private async void ApiViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        await LoadInvestment();

        DareView apiView = new();
        apiView.LoadContent();
        _mainWindow.MakeNewTab("API", apiView);
        _mainWindow.SetNewestTabSelected();
    }

    private async void CollectionsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        await LoadInvestment();

        CollectionsView apiView2 = new();
        apiView2.LoadContent();
        _mainWindow.MakeNewTab("Collections", apiView2);
        _mainWindow.SetNewestTabSelected();
    }

    private void NamedEntitiesBagsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new();
        tagListView.LoadContent(ETagListType.DestinationGlobalTagBagList);
        _mainWindow.MakeNewTab("Destination Global Tag Bag", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllEntitiesViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new();
        tagListView.LoadContent(ETagListType.EntityList);
        _mainWindow.MakeNewTab("Dynamics", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void ActivitiesViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new();
        tagListView.LoadContent(ETagListType.ActivityList);
        _mainWindow.MakeNewTab("Activities", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllStaticsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new();
        tagListView.LoadContent(ETagListType.StaticsList);
        _mainWindow.MakeNewTab("Statics", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private async void WeaponAudioViewButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadInvestment();

        TagListViewerView tagListView = new();
        tagListView.LoadContent(ETagListType.WeaponAudioGroupList);
        _mainWindow.MakeNewTab("Weapon Audio", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllAudioViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        AudioListView audioListView = new();
        audioListView.LoadContent();
        _mainWindow.MakeNewTab("Sounds", audioListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllBKHDViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new();
        tagListView.LoadContent(ETagListType.BKHDGroupList);
        _mainWindow.MakeNewTab("Sound Banks", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllStringsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new();
        tagListView.LoadContent(ETagListType.StringContainersList);
        _mainWindow.MakeNewTab("strings", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllTexturesView2Button_OnClick(object sender, RoutedEventArgs e)
    {
        TextureListView textureListView = new();
        textureListView.LoadContent();
        _mainWindow.MakeNewTab("Textures", textureListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllMaterialsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new();
        tagListView.LoadContent(ETagListType.MaterialList);
        _mainWindow.MakeNewTab("Materials", tagListView);
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

    private void AboutButton_OnClick(object sender, RoutedEventArgs e)
    {
        PopupBanner about = new()
        {
            DarkenBackground = true,
            Icon = "",
            //about.IconImage = MainWindow.GetBitmapSource(System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location));
            Title = $"CHARM {App.CurrentVersion.Id}",
            Subtitle = "Charm was created by Montague",
            Description =
            "Charm was developed for 3D artists, to preserve vaulted content as much as possible, and for learning how the Tiger engine works in general!\n\n" +
            "Additional help/development from:\n" +
            "• Delta\n" +
            "• nblock\n" +
            "• Cohae\n" +
            "• BIOS\n" +
            "• HighRTT\n",
            Style = PopupBanner.PopupStyle.Information
        };
        about.Show();
    }
}
