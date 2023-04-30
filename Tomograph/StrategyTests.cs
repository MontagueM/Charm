using Tiger;

namespace Tomograph;

[TestClass]
public class StrategyTests
{
    [TestInitialize]
    public void Initialize()
    {
        Strategy.Reset();
        CharmInstance.ClearSubsystems();
        ConfigSubsystem config = new ConfigSubsystem("../../../../Tomograph/TestData/valid_test_config.json");
        Helpers.CallNonPublicMethod(config, "Initialise");
    }

    [TestCleanup]
    public void Cleanup()
    {
        Strategy.Reset();
        CharmInstance.ClearSubsystems();
    }

    [TestMethod]
    public void LoadConfigCorrectly()
    {
        List<TigerStrategy> strategies = Strategy.GetAllStrategies().ToList();
        Assert.AreEqual(2, strategies.Count);
        Assert.AreEqual(TigerStrategy.DESTINY2_SHADOWKEEP_2601, strategies[0]);
        Assert.AreEqual("../../../../Tomograph/TestData/DESTINY2_SHADOWKEEP_2601/packages/", strategies[0].GetStrategyConfiguration().PackagesDirectory);
        Assert.AreEqual(TigerStrategy.DESTINY2_WITCHQUEEN_6307, strategies[1]);
        Assert.AreEqual("../../../../Tomograph/TestData/DESTINY2_WITCHQUEEN_6307/packages/", strategies[1].GetStrategyConfiguration().PackagesDirectory);
    }

    [TestMethod]
    public void AddPackagesDirectory_Valid()
    {
        Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LIGHTFALL_7003, "../../../../Tomograph/TestData/test_DESTINY2_LIGHTFALL_7003/packages");
        Assert.IsTrue(Strategy.GetAllStrategies().Contains(TigerStrategy.DESTINY2_LIGHTFALL_7003));
    }

    [TestMethod]
    public void ChangeStrategy_Exists()
    {
        Assert.AreEqual(TigerStrategy.NONE, Helpers.GetCurrentStrategy());
        Strategy.SetStrategy(TigerStrategy.DESTINY2_SHADOWKEEP_2601);
        Assert.AreEqual(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Helpers.GetCurrentStrategy());
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "Game strategy does not exist")]
    public void ChangeStrategy_DoesNotExist()
    {
        Assert.AreEqual(TigerStrategy.NONE, Helpers.GetCurrentStrategy());
        Strategy.SetStrategy(TigerStrategy.DESTINY2_LIGHTFALL_7003);
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "Game strategy already exists")]
    public void AddPackagesDirectory_StrategyAlreadyExists()
    {
        Strategy.AddNewStrategy(TigerStrategy.NONE, "");
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "Game strategy cannot re-use an existing packages path")]
    public void AddPackagesDirectory_PackagePathAlreadySet()
    {
        Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LIGHTFALL_7003, "../../../../Tomograph/TestData/DESTINY2_SHADOWKEEP_2601/packages/");
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(DirectoryNotFoundException), "The packages directory does not exist")]
    public void AddPackagesDirectory_PackagesDirectoryDoesNotExist()
    {
        Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LIGHTFALL_7003, "../../../../Tomograph/TestData/D2InvalidDoesNotExist/");
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "The packages directory is empty")]
    public void AddPackagesDirectory_PackagesDirectoryEmpty()
    {
        Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LIGHTFALL_7003, "../../../../Tomograph/TestData/D2InvalidEmpty/");
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "The packages directory contains a package without the correct prefix")]
    public void AddPackagesDirectory_InvalidD2Win64Prefix()
    {
        Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LIGHTFALL_7003, "../../../../Tomograph/TestData/D2InvalidPrefix/");
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "The packages directory contains a package without the correct extension")]
    public void AddPackagesDirectory_InvalidExtension()
    {
        Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LIGHTFALL_7003, "../../../../Tomograph/TestData/D2InvalidExtension/");
    }

    [TestMethod]
    public void GetStrategyConfiguration_Exists()
    {
        StrategyConfiguration? config = TigerStrategy.DESTINY2_SHADOWKEEP_2601.GetStrategyConfiguration();
        Assert.IsTrue(config.HasValue);
        Assert.AreEqual(Path.GetFullPath("../../../../Tomograph/TestData/DESTINY2_SHADOWKEEP_2601/packages/"), Path.GetFullPath(config?.PackagesDirectory));
    }

    [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "Game strategy does not exist")]
    public void GetStrategyConfiguration_DoesNotExist()
    {
        StrategyConfiguration? config = TigerStrategy.DESTINY2_LIGHTFALL_7003.GetStrategyConfiguration();
    }
}
