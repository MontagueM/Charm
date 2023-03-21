using System.Reflection;
using Resourcer;

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
        // Assert.AreEqual(ValidPackagesDirectory, resourcer.PackagesDirectory);
    }

    [TestMethod]
    public void Get_CorrectStrategyChange()
    {
        // Assert.AreEqual(ValidPackagesDirectory, PackageResourcer.Get().PackagesDirectory);
        
        // Strategy.AddNewStrategy(TigerStrategy.DESTINY1_PS4);
        Strategy.CurrentStrategy = TigerStrategy.DESTINY1_PS4;
        
        // Assert.AreEqual(D1PS4_ValidPackageDirectory, PackageResourcer.Get().PackagesDirectory);
    }

    [TestMethod]
    public void SinglePackage_ValidPackageObject_ValidId()
    {
        ushort expectedPackageId = 0x100;
        IPackage package = PackageResourcer.Get().GetPackage(expectedPackageId);
        Assert.IsInstanceOfType(package, typeof(D2Package));
        PackageMetadata actualPackageMetadata = package.GetPackageMetadata();
        Assert.AreEqual(expectedPackageId, actualPackageMetadata.PackageId);
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), typeof(PackageResourcer), "PackageIdInvalidMessage")]
    public void SinglePackage_InvalidPackageObject_InvalidId()
    {
        IPackage package = PackageResourcer.Get().GetPackage(0x100);
        Assert.IsNull(package);
    }
}