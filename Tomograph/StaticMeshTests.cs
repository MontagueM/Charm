using Tiger;
using Tiger.Schema;

namespace Tomograph;

public interface IStaticMeshTests
{

}

[TestClass, TestCategory("DESTINY2_WITCHQUEEN_6307"), TestStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
public class D2WQ_StaticMeshTests : IStaticMeshTests
{
#pragma warning disable S1144
    private static readonly TestPackage meshPackage = new("w64_city_tower_d2_01ad_7.pkg", 1674718010);
    private static readonly TestPackage meshPackage2 = new("w64_city_tower_d2_02aa_7.pkg", 1674718010);
    private static readonly TestPackage meshPackage3 = new("w64_city_tower_d2_02a9_6.pkg", 1668960051);
    // for textures
    private static readonly TestPackage meshPackage4 = new("w64_sr_environments_0110_7.pkg", 1674718365);
    private static readonly TestPackage meshPackage5 = new("w64_sr_environments_01e9_6.pkg", 1668960415);
    private static readonly TestPackage meshPackage6 = new("w64_sr_environments_01e6_6.pkg", 1668960415);
    private static readonly TestPackage meshPackage7 = new("w64_sr_environments_01ea_7.pkg", 1674718365);
    private static readonly TestPackage meshPackage8 = new("w64_sr_globals_011a_7.pkg", 1674717839);
    private static readonly TestPackage meshPackage9 = new("w64_sr_environments_026f_7.pkg", 1674718365);
    private static readonly TestPackage meshPackage10 = new("w64_sr_environments_0271_7.pkg", 1674718365);
    private static readonly TestPackage meshPackage11 = new("w64_sr_environments_01e3_6.pkg", 1668960415);
    private static readonly TestPackage meshPackage12 = new("w64_sr_fx_010c_7.pkg", 1674717892);
    private static readonly TestPackage meshPackage13 = new("w64_sr_environments_01e2_7.pkg", 1674718365);
#pragma warning restore S1144

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
        // string fileHash = "62A2B580";
        // StaticMesh mesh = FileResourcer.Get().GetTag<StaticMesh>(fileHash);
        // List<StaticPart> parts = mesh.Load(ExportDetailLevel.MostDetaileded);
        // FbxHandler handler = new FbxHandler();
        // handler.AddStaticToScene(parts, "Test");
        // handler.ExportScene("TestModels/Test.fbx");

        string fileHash = "685BD580";
        StaticMesh mesh = FileResourcer.Get().GetFile<StaticMesh>(fileHash);
        List<StaticPart> parts = mesh.Load(ExportDetailLevel.MostDetailed);
        FbxHandler handler = new FbxHandler();
        handler.AddStaticToScene(parts, "Test");
        handler.ExportScene("TestModels/Test.fbx");
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
