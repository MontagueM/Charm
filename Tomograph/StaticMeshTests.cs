using Tiger;
using Tiger.Schema;

namespace Tomograph;

public interface IStaticMeshTests
{

}

[TestClass, TestCategory("DESTINY2_WITCHQUEEN_6307"), TestStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
public class D2WQ_StaticMeshTests : IStaticMeshTests
{
    private static readonly TestPackage meshPackage = new("w64_city_tower_d2_01ad_7.pkg", 1674718010);

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
    public void GetMeshData()
    {
        string fileHash = "62A2B580";
        StaticMesh mesh = FileResourcer.Get().GetTag<StaticMesh>(fileHash);
        var a = 0;
        // mesh.
        // Assert.IsNotNull(strings);
        // Assert.AreEqual(fileHash, strings.Hash.ToString());

    }

    [TestMethod]
    public void ExportMeshData()
    {
        // Method intentionally left empty.
    }

    [TestMethod, ExpectedExceptionWithMessage(typeof(Exception), "Could not find string with hash FFFFFFFF")]
    public void GetString_InvalidHash()
    {
        // Method intentionally left empty.
    }
}
