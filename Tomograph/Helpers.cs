using System.Reflection;
using Resourcer;

namespace Tomograph;

public class Helpers
{
    public static void Throws<T, U>(Action func, U classWithMessageVariable, string messageVariableName)
        where T : Exception
        where U : class
    {
        var exceptionThrown = false;
        try
        {
            func.Invoke();
        }
        catch ( T )
        {
            exceptionThrown = true;
        }

        if ( !exceptionThrown )
        {
            throw new AssertFailedException(
                String.Format("An exception of type {0} was expected, but not thrown", typeof(T))
            );
        }
        
        BindingFlags bindFlags = BindingFlags.Static | BindingFlags.NonPublic;
        FieldInfo field = classWithMessageVariable.GetType().GetField(messageVariableName, bindFlags);
        var x = field.GetValue(null);
        var a = 0;
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