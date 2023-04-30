using System.Reflection;
using Tiger;

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

public class ExpectedExceptionWithMessageVariableAttribute : ExpectedExceptionBaseAttribute
{
    public Type ExceptionType { get; set; }
    public Type ClassWithMessage { get; set; }
    public string ExpectedMessageVariable { get; set; }

    public ExpectedExceptionWithMessageVariableAttribute(Type exceptionType)
    {
        ExceptionType = exceptionType;
    }

    public ExpectedExceptionWithMessageVariableAttribute(Type exceptionType, Type classWithMessage, string expectedMessageVariable)
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

public class ExpectedExceptionWithMessageAttribute : ExpectedExceptionBaseAttribute
{
    public Type ExceptionType { get; set; }
    public string ExpectedMessage { get; set; }

    public ExpectedExceptionWithMessageAttribute(Type exceptionType)
    {
        ExceptionType = exceptionType;
    }

    public ExpectedExceptionWithMessageAttribute(Type exceptionType, string expectedMessage)
    {
        ExceptionType = exceptionType;
        ExpectedMessage = expectedMessage;
    }

    private static List<string> GetMessageParts(string message)
    {
        List<string> parts = new();
        int partStart = 0;
        bool inFormat = false;
        for (int i = 0; i < message.Length; i++)
        {
            char c = message[i];
            if (c == '{')
            {
                if (inFormat)
                {
                    throw new ArgumentException("Invalid message format.");
                }
                inFormat = true;
                parts.Add(message.Substring(partStart, i - partStart));
            }
            else if (c == '}')
            {
                if (!inFormat)
                {
                    throw new ArgumentException("Invalid message format.");
                }
                inFormat = false;
                partStart = i + 1;
            }
            if (i == message.Length - 1)
            {
                if (inFormat)
                {
                    throw new ArgumentException("Invalid message format.");
                }
                parts.Add(message.Substring(partStart));
            }
        }
        if (inFormat)
        {
            throw new ArgumentException("Invalid message format.");
        }

        return parts;
    }

    protected override void Verify(Exception e)
    {
        if (e.GetType() != ExceptionType)
        {
            Assert.Fail($"Expected exception type: {ExceptionType.FullName}. " +
                        $"Actual exception type: {e.GetType().FullName}. Exception message: {e.Message}");
        }

        var actualMessage = e.Message.Trim();

        AssertEqualFormattedMessages(ExpectedMessage, actualMessage);
    }

    private static void AssertEqualFormattedMessages(string expectedMessage, string actualMessage)
    {
        // get the message parts for expectedMessage and check the parts exist sequentially in actualMessage
        List<string> expectedMessageParts = GetMessageParts(expectedMessage);
        int actualMessageIndex = 0;
        foreach (string expectedMessagePart in expectedMessageParts)
        {
            actualMessageIndex = actualMessage.IndexOf(expectedMessagePart, actualMessageIndex);
            if (actualMessageIndex == -1)
            {
                Assert.Fail($"Expected exception message: {expectedMessage}. Actual exception message: {actualMessage}");
            }
            actualMessageIndex += expectedMessagePart.Length;
        }
    }
}
