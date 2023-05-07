using Tiger.Schema;
using Tiger;

namespace Tomograph;

public interface IStaticMeshTests
{

}

[TestClass, TestCategory("DESTINY2_WITCHQUEEN_6307"), TestStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
public class D2WQ_StaticMeshTests : IStaticMeshTests
{
    [TestInitialize]
    public void Initialize()
    {
        Strategy.Reset();
        CharmInstance.ClearSubsystems();
        ConfigSubsystem config = new ConfigSubsystem("../../../../Tomograph/TestData/valid_test_config.json");
        Helpers.CallNonPublicMethod(config, "Initialise");
        Strategy.SetStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307);
    }

    [TestCleanup]
    public void Cleanup()
    {
        CharmInstance.ClearSubsystems();
    }

    [TestMethod]
    public void GetStrings()
    {
        string fileHash = "A405A080";
        StaticMesh mesh = FileResourcer.Get().GetTag<StaticMesh>(fileHash);
        // Assert.IsNotNull(strings);
        // Assert.AreEqual(fileHash, strings.Hash.ToString());

    }

    [TestMethod, ExpectedExceptionWithMessage(typeof(Exception), "Could not find string with hash FFFFFFFF")]
    public void GetString_InvalidHash()
    {
    }
}
