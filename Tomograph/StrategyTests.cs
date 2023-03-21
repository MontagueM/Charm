using Resourcer;

namespace Tomograph;

[TestClass]
public class StrategyTests
{
    private static readonly string D2Latest_ValidPackagesDirectory = @"C:\Users\monta\Desktop\Destiny 2\packages";
    private static readonly string D1PS4_ValidPackagesDirectory = @"M:\D1_PS4";

    [TestMethod]
    public void Strategy_D2Latest_ValidPackagesDirectory()
    {
        Strategy.Reset();
        // Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LATEST, D2Latest_ValidPackagesDirectory);
    }
    
    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(DirectoryNotFoundException), typeof(Strategy), "PackagesDirectoryDoesNotExistMessage")]
    public void Strategy_D2Latest_InvalidPackagesDirectory_NotExist()
    {
        Strategy.Reset();
        // Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LATEST, "../../../Packages/D2InvalidNotExist");
    }
    
    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), typeof(Strategy), "PackagesDirectoryEmptyMessage")]
    public void Strategy_D2Latest_InvalidPackagesDirectory_Empty()
    {
        Strategy.Reset();
        // Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LATEST, "../../../Packages/D2InvalidEmpty");
    }
    
    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), typeof(Strategy), "PackagesDirectoryInvalidPrefixMessage")]
    public void Strategy_D2Latest_InvalidPackagesDirectory_InvalidD2Win64Prefix()
    {
        Strategy.Reset();
        // Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LATEST, "../../../Packages/D2InvalidPrefix");
    }
    
    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), typeof(Strategy), "PackagesDirectoryInvalidExtensionMessage")]
    public void Strategy_D2Latest_InvalidPackagesDirectory_InvalidExtension()
    {
        Strategy.Reset();
        // Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LATEST, "../../../Packages/D2InvalidExtension");
    }
}