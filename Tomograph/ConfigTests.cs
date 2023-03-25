using System.Reflection;
using Newtonsoft.Json;
using Resourcer;

namespace Tomograph;

// We can't really test the actual contents since its designed to be very changeable, but we can at least test
// parsing and known-values get changed and set correctly
[TestClass]
public class ConfigTests
{
    private static string _configPath;
    
    [TestInitialize]
    public void Setup()
    {
        ConfigSubsystem config = new ConfigSubsystem();
        _configPath = (string)typeof(ConfigSubsystem).GetField("_configFilePath", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(config);
    }

    [TestCleanup]
    public void Cleanup()
    {
        File.Delete(_configPath);
        CharmInstance.ClearSubsystems();
    }
    
    [TestMethod]
    public void Initialise_Create()
    {
        Assert.IsFalse(File.Exists(_configPath));
        ConfigSubsystem config = new ConfigSubsystem();
        Helpers.CallNonPublicMethod(config, "Initialise");
        Assert.IsTrue(File.Exists(_configPath));
        
        // Open to check valid format
        Helpers.CallNonPublicMethod(config, "Initialise");
    }
    
    [TestMethod]
    public void Initialise_Open()
    {
        // Create the most up-to-date copy
        ConfigSubsystem config = new ConfigSubsystem();
        Helpers.CallNonPublicMethod(config, "Initialise");

        // Open it to check valid format - not ideal since this relies on Create working but meh
        Helpers.CallNonPublicMethod(config, "Initialise");
    }

    [TestMethod]
    public void ValueSetCorrectly()
    {
        // Set some new value
        Helpers.SetNonPublicStaticField(typeof(Strategy), "_currentStrategy", TigerStrategy.DESTINY2_WITCHQUEEN_6307);
        
        // Create the most up-to-date copy
        ConfigSubsystem config = new ConfigSubsystem();
        Helpers.CallNonPublicMethod(config, "Initialise");
        
        // Set a different value
        Helpers.SetNonPublicStaticField(typeof(Strategy), "_currentStrategy", TigerStrategy.NONE);

        Helpers.CallNonPublicMethod(config, "Initialise");
        
        // Check the value is set correctly
        Assert.AreEqual(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Helpers.GetNonPublicStaticField(typeof(Strategy), "_currentStrategy"));
    }

    [TestMethod, ExpectedExceptionWithMessage(typeof(JsonSerializationException), "Failed to load config file")]
    public void Initialise_Open_InvalidFormat()
    {
        ConfigSubsystem config = new ConfigSubsystem();
        Helpers.CallNonPublicMethod(config, "Initialise");
        
        // Modify to an invalid format
        string configData = File.ReadAllText(_configPath);
        configData = configData.Replace("}", "");
        File.WriteAllText(_configPath, configData);
        
        // Open to check invalid format
        Helpers.CallNonPublicMethod(config, "Initialise");
    }
}