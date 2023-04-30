using System.Reflection;
using Tiger;
using SKPackage = Tiger.DESTINY2_SHADOWKEEP_2601.Package;
using WQPackage = Tiger.DESTINY2_WITCHQUEEN_6307.Package;

namespace Tomograph;

public interface IPackageResourcerTests {
    void Get_ValidSingletonObject();
    void Get_CorrectStrategyChange();
    void SinglePackage_ValidPackageObject_ValidId();
    void SinglePackage_InvalidPackageObject_InvalidId();
}



[TestClass, TestCategory("DESTINY2_WITCHQUEEN_6307"), TestStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
public class DESTINY2_WITCHQUEEN_6307_PackageResourcerTests : CharmPackageTests, IPackageResourcerTests
{
    [TestInitialize]
    public void Initialize()
    {
        Strategy.Reset();
        TestPackage.TestPackageStrategy = Helpers.GetTestClassStrategy(GetType());
        Strategy.AddNewStrategy(Helpers.GetTestClassStrategy(GetType()), TestPackage.TestPackageDataDirectory);
        TestDataSystem.VerifyTestData(GetType());
    }

    [TestCleanup]
    public void Cleanup() { Strategy.Reset(); }

    [TestMethod]
    public void Get_ValidSingletonObject()
    {
        PackageResourcer resourcer = PackageResourcer.Get();
        Assert.IsNotNull(resourcer);
        DirectoryAssert.DirectoryEquals(Helpers.GetCurrentStrategy().GetStrategyConfiguration().PackagesDirectory, resourcer.PackagesDirectory);
    }

    [TestMethod]
    public void Get_CorrectStrategyChange()
    {
        // Assert.AreEqual(ValidPackagesDirectory, PackageResourcer.Get().PackagesDirectory);
        
        // Strategy.AddNewStrategy(TigerStrategy.DESTINY1_PS4);
        // Strategy.CurrentStrategy = TigerStrategy.DESTINY1_PS4;
        
        // Assert.AreEqual(D1PS4_ValidPackageDirectory, PackageResourcer.Get().PackagesDirectory);
    }

    [TestMethod]
    public void SinglePackage_ValidPackageObject_ValidId()
    {
        ushort expectedPackageId = 0x100;
        IPackage package = PackageResourcer.Get().GetPackage(expectedPackageId);
        Assert.IsInstanceOfType(package, typeof(WQPackage));
        PackageMetadata actualPackageMetadata = package.GetPackageMetadata();
        Assert.AreEqual(expectedPackageId, actualPackageMetadata.Id);
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "The package id '{packageId}' is not in the package paths cache")]
    public void SinglePackage_InvalidPackageObject_InvalidId()
    {
        IPackage package = PackageResourcer.Get().GetPackage(0x1);
        Assert.IsNull(package);
    }
}