using Avalonia.Controls;
using Resourcer;

namespace Charm.Views;

public partial class MainWindow : Window
{
    private static readonly string ValidD2LatestPackageDirectory = @"I:\SteamLibrary\steamapps\common\Destiny 2\packages";
    private static readonly string ValidD1Ps4LatestPackageDirectory = @"I:\SteamLibrary\steamapps\common\Destiny 1\packages";

    public MainWindow()
    {
        InitializeComponent();

        Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LATEST, ValidD2LatestPackageDirectory);
        // SchemaHandle.Initialise();
        var a = PackageResourcer.Get().PackagesDirectory;
        Strategy.AddNewStrategy(TigerStrategy.DESTINY1_PS4, ValidD1Ps4LatestPackageDirectory);
        var b = PackageResourcer.Get().PackagesDirectory;
        var c = 0;
    }
}