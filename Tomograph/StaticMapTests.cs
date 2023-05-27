using Tiger;
using Tiger.Schema;

namespace Tomograph;

public interface IStaticMapTests
{

}

// todo make it so CI can run tests without needing packages
[TestClass, TestCategory("DESTINY2_WITCHQUEEN_6307"), TestStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
public class StaticMapTests
{
    [TestInitialize]
    public void Initialize()
    {
        Strategy.Reset();
        CharmInstance.ClearSubsystems();
        ConfigSubsystem config = new ConfigSubsystem("../../../../Tomograph/TestData/valid_test_config.json");
        Helpers.CallNonPublicMethod(config, "Initialise");
        Strategy.SetStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307);

        TestPackage.TestPackageStrategy = Helpers.GetTestClassStrategy(GetType());
        TestDataSystem.VerifyTestData(GetType());
    }

    [TestCleanup]
    public void Cleanup()
    {
        CharmInstance.ClearSubsystems();
    }

    [TestMethod]
    public void GetStaticMapData()
    {
        // var config = Strategy.GetStrategyConfiguration(TigerStrategy.DESTINY2_WITCHQUEEN_6307);
        // config.PackagesDirectory = "I:/v6307/packages/";
        // Strategy.UpdateStrategyConfiguration(TigerStrategy.DESTINY2_WITCHQUEEN_6307, config);

        string fileHash = "17A7B580";
        StaticMapData mapData = FileResourcer.Get().GetFile<StaticMapData>(fileHash);
        FbxHandler handler = new();
        // mapData.LoadArrangedIntoFbxScene(handler);
        mapData.LoadIntoFbxScene(handler, "TestModels/TestMap", true);
        handler.ExportScene("TestModels/TestMap.fbx");
        var a = 0;
        // mesh.
        // Assert.IsNotNull(strings);
        // Assert.AreEqual(fileHash, strings.Hash.ToString());
    }
}
