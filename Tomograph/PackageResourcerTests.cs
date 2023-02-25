using System.Reflection;
using Resourcer;

namespace Tomograph;


[TestClass]
public class PackageResourcerTests
{
    private static readonly string D2Latest_ValidPackageDirectory = @"C:\Users\monta\Desktop\Destiny 2\packages";
    private static readonly string D1PS4_ValidPackageDirectory = @"M:\D1_PS4";
    
    private void D2Latest_SetupValidState()
    {
        Strategy.Reset();
        Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LATEST, D2Latest_ValidPackageDirectory);
        Strategy.CurrentStrategy = TigerStrategy.DESTINY2_LATEST;
    }

    [TestMethod]
    public void Get_ValidSingletonObject()
    {
        D2Latest_SetupValidState();
        PackageResourcer resourcer = PackageResourcer.Get();
        Assert.IsNotNull(resourcer);
        Assert.AreEqual(D2Latest_ValidPackageDirectory, resourcer.PackagesDirectory);
    }
    
    [TestMethod]
    public void Get_CorrectStrategyChange()
    {
        D2Latest_SetupValidState();
        Assert.AreEqual(D2Latest_ValidPackageDirectory, PackageResourcer.Get().PackagesDirectory);
        
        Strategy.AddNewStrategy(TigerStrategy.DESTINY1_PS4, D1PS4_ValidPackageDirectory);
        Strategy.CurrentStrategy = TigerStrategy.DESTINY1_PS4;
        
        Assert.AreEqual(D1PS4_ValidPackageDirectory, PackageResourcer.Get().PackagesDirectory);
    }

    [TestMethod]
    public void Resourcer_SinglePackage_CorrectPackageObject_ValidId()
    {
        D2Latest_SetupValidState();
        IPackage package = PackageResourcer.Get().GetPackage(0x100);
        if (package.GetType().IsInstanceOfType(typeof(D2Package)))
        {
            
        }
    }
    
    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), typeof(PackageResourcer), "PackageIdInvalidMessage")]
    public void Resourcer_SinglePackage_CorrectPackageObject_InvalidId()
    {
        D2Latest_SetupValidState();
        IPackage package = PackageResourcer.Get().GetPackage(0x100);
        Assert.IsNull(package);
    }
}