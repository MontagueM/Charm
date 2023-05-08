using Tiger;
using Tiger.Schema;

namespace Tomograph;

public interface ILocalizedTextTests
{
    public void GetStrings();
    public void GetString_InvalidHash();
}

[TestClass, TestCategory("DESTINY2_WITCHQUEEN_6307"), TestStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
public class D2WQ_LocalizedStringsTests : ILocalizedTextTests
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
        LocalizedStrings strings = FileResourcer.Get().GetTag<LocalizedStrings>(fileHash);
        Assert.IsNotNull(strings);
        Assert.AreEqual(fileHash, strings.Hash.ToString());

        string firstStringHash = "29403800";
        TigerString firstString = strings.GetStringFromHash(new StringHash(firstStringHash));
        Assert.AreEqual("block", firstString.RawValue);

        string lastStringHash = "1A12FBFF";
        TigerString lastString = strings.GetStringFromHash(new StringHash(lastStringHash));
        Assert.AreEqual(" Text Chat", lastString.RawValue);

        string manyPartsStringHash = "3270E5F9";
        TigerString manyPartsString = strings.GetStringFromHash(new StringHash(manyPartsStringHash));
        Assert.AreEqual(" ::", manyPartsString.RawValue);

        string noPartsStringHash = "E30D7BF2";
        TigerString noPartsString = strings.GetStringFromHash(new StringHash(noPartsStringHash));
        Assert.AreEqual("", noPartsString.RawValue);
    }

    [TestMethod, ExpectedExceptionWithMessage(typeof(Exception), "Could not find string with hash FFFFFFFF")]
    public void GetString_InvalidHash()
    {
        string fileHash = "A405A080";
        LocalizedStrings strings = FileResourcer.Get().GetTag<LocalizedStrings>(fileHash);
        Assert.IsNotNull(strings);

        string invalidStringHash = "FFFFFFFF";
        TigerString invalidString = strings.GetStringFromHash(new StringHash(invalidStringHash));
    }
}

[TestClass, TestCategory("DESTINY2_SHADOWKEEP_2601"), TestStrategy(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
public class D2SK_LocalizedStringsTests : ILocalizedTextTests
{

    [TestInitialize]
    public void Initialize()
    {
        TestPackage.TestPackageStrategy = TigerStrategy.DESTINY2_SHADOWKEEP_2601;
        Strategy.Reset();
        CharmInstance.ClearSubsystems();
        ConfigSubsystem config = new ConfigSubsystem("../../../../Tomograph/TestData/valid_test_config.json");
        Helpers.CallNonPublicMethod(config, "Initialise");
        Strategy.SetStrategy(TigerStrategy.DESTINY2_SHADOWKEEP_2601);
        TestDataSystem.VerifyTestData(GetType());
    }

    [TestCleanup]
    public void Cleanup()
    {
        CharmInstance.ClearSubsystems();
    }

    [TestMethod]
    public void GetStrings()
    {
        string fileHash = "0D20A180";
        LocalizedStrings strings = FileResourcer.Get().GetTag<LocalizedStrings>(fileHash);
        Assert.IsNotNull(strings);
        Assert.AreEqual(fileHash, strings.Hash.ToString());

        string firstStringHash = "9AC45D00";
        TigerString firstString = strings.GetStringFromHash(new StringHash(firstStringHash));
        Assert.AreEqual("Accept Friend Request", firstString.RawValue);

        string lastStringHash = "D4EBF4FF";
        TigerString lastString = strings.GetStringFromHash(new StringHash(lastStringHash));
        Assert.AreEqual("Look", lastString.RawValue);

        string manyPartsStringHash = "3270E5F9";
        TigerString manyPartsString = strings.GetStringFromHash(new StringHash(manyPartsStringHash));
        Assert.AreEqual(" ::", manyPartsString.RawValue);

        string noPartsStringHash = "DDBADEED";
        TigerString noPartsString = strings.GetStringFromHash(new StringHash(noPartsStringHash));
        Assert.AreEqual("", noPartsString.RawValue);
    }

    [TestMethod]
    public void GetStrings_NonUtf8()
    {
        string fileHash = "67F8B480";
        LocalizedStrings strings = FileResourcer.Get().GetTag<LocalizedStrings>(fileHash);
        Assert.IsNotNull(strings);

        string emdashStringHash = "043568E7";
        TigerString emdashString = strings.GetStringFromHash(new StringHash(emdashStringHash));
        Assert.AreEqual("Welcome back. This time I did make tea for you—but I seem to have drunk it all. Perhaps if you were a mite quicker. Heh.", emdashString.RawValue);
    }

    [TestMethod, ExpectedExceptionWithMessage(typeof(Exception), "Could not find string with hash FFFFFFFF")]
    public void GetString_InvalidHash()
    {
        string fileHash = "0D20A180";
        LocalizedStrings strings = FileResourcer.Get().GetTag<LocalizedStrings>(fileHash);
        Assert.IsNotNull(strings);

        string invalidStringHash = "FFFFFFFF";
        TigerString invalidString = strings.GetStringFromHash(new StringHash(invalidStringHash));
    }
}
