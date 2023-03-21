using System.Reflection;
using Resourcer;

namespace Tomograph;

[AttributeUsage(AttributeTargets.Class)]
public class TestStrategyAttribute : Attribute
{
    public TigerStrategy Strategy { get; }

    public TestStrategyAttribute(TigerStrategy strategy)
    {
        Strategy = strategy;
    }
}

public class ExpectedExceptionWithMessageAttribute : ExpectedExceptionBaseAttribute
{
    public Type ExceptionType { get; set; }
    public Type ClassWithMessage { get; set; }
    public string ExpectedMessageVariable { get; set; }

    public ExpectedExceptionWithMessageAttribute(Type exceptionType)
    {
        ExceptionType = exceptionType;
    }

    public ExpectedExceptionWithMessageAttribute(Type exceptionType, Type classWithMessage, string expectedMessageVariable)
    {
        ExceptionType = exceptionType;
        ClassWithMessage = classWithMessage;
        ExpectedMessageVariable = expectedMessageVariable;
    }

    protected override void Verify(Exception e)
    {
        if (e.GetType() != ExceptionType)
        {
            Assert.Fail($"Expected exception type: {ExceptionType.FullName}. " +
                        $"Actual exception type: {e.GetType().FullName}. Exception message: {e.Message}");
        }

        var actualMessage = e.Message.Trim();
        BindingFlags bindFlags = BindingFlags.Static | BindingFlags.NonPublic;

        FieldInfo field = ClassWithMessage.GetField(ExpectedMessageVariable, bindFlags);
        if (field == null)
        {
            Assert.Fail(
                $"Provided message variable {ClassWithMessage}.{ExpectedMessageVariable} is invalid.");
        }
        string expectedMessage = (string)field.GetValue(null);
        if (expectedMessage != null)
        {
            StringAssert.Contains(actualMessage, expectedMessage);
        }
    }
}