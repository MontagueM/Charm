using Tiger;

namespace Tomograph;

[TestClass]
public class PackagePathsCacheTests
{
    [TestInitialize]
    public void Initialize()
    {
        Strategy.Reset();
        CharmInstance.ClearSubsystems();
        ConfigSubsystem config = new ConfigSubsystem("../../../../Tomograph/TestData/valid_test_config.json");
        Helpers.CallNonPublicMethod(config, "Initialise");
        PackagePathsCache.ClearCacheFiles();
    }

    [TestCleanup]
    public void Cleanup()
    {
        CharmInstance.ClearSubsystems();
        PackagePathsCache.ClearCacheFiles();
    }

    [TestMethod]
    public void Create_ValidStrategy()
    {
        Strategy.SetStrategy(TigerStrategy.DESTINY2_SHADOWKEEP_2601);
        PackagePathsCache cache = new PackagePathsCache(Helpers.GetCurrentStrategy());
        string ui_startup_unp1 = cache.GetPackagePathFromId(0x0109);
        Assert.AreEqual("../../../../Tomograph/TestData/DESTINY2_SHADOWKEEP_2601/packages/w64_ui_startup_unp1_0.pkg", ui_startup_unp1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException), "Game strategy does not exist")]
    public void Create_InvalidStrategy()
    {
        PackagePathsCache cache = new PackagePathsCache(TigerStrategy.NONE);
    }
}
