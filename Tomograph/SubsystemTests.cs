using Resourcer;

namespace Tomograph;

class TestSubsystem : CharmSubsystem
{
    public static bool ReturnBool { get; set; } = true;
    
    protected override bool Initialise()
    {
        return ReturnBool;
    }
}

class ChildTestSubsystem : TestSubsystem
{
    protected override bool Initialise()
    {
        return ReturnBool;
    }
}

[TestClass]
public class SubsystemTests
{
    [TestCleanup]
    public void Cleanup()
    {
        CharmInstance.ClearSubsystems();
        TestSubsystem.ReturnBool = true;
    }

    [TestMethod]
    public void GetSubsystem_CreateNew()
    {
        Assert.IsFalse(CharmInstance.HasSubsystem<TestSubsystem>());
        TestSubsystem subsystem = CharmInstance.GetSubsystem<TestSubsystem>();
        Assert.IsNotNull(subsystem);
        Assert.IsTrue(CharmInstance.HasSubsystem<TestSubsystem>());
    }
    
    [TestMethod, ExpectedExceptionWithMessage(typeof(Exception), "Failed to initialise subsystem")]
    public void GetSubsystem_CreateNew_FailInit()
    {
        Assert.IsFalse(CharmInstance.HasSubsystem<TestSubsystem>());
        TestSubsystem.ReturnBool = false;
        TestSubsystem subsystem = CharmInstance.GetSubsystem<TestSubsystem>();
    }
    
    [TestMethod]
    public void GetSubsystem_CreateNew_ChildInheritance()
    {
        Assert.IsFalse(CharmInstance.HasSubsystem<ChildTestSubsystem>());
        ChildTestSubsystem subsystem = CharmInstance.GetSubsystem<ChildTestSubsystem>();
        Assert.IsNotNull(subsystem);
        Assert.IsTrue(CharmInstance.HasSubsystem<ChildTestSubsystem>());
    }
    
    [TestMethod]
    public void GetSubsystem_CreateNew_InitAll()
    {
        Assert.IsFalse(CharmInstance.HasSubsystem<TestSubsystem>());
        Assert.IsFalse(CharmInstance.HasSubsystem<ChildTestSubsystem>());
        CharmInstance.InitialiseSubsystems();
        TestSubsystem subsystem = CharmInstance.GetSubsystem<TestSubsystem>();
        Assert.IsNotNull(subsystem);
        Assert.IsTrue(CharmInstance.HasSubsystem<TestSubsystem>());
        Assert.IsTrue(CharmInstance.HasSubsystem<ChildTestSubsystem>());
    }
}