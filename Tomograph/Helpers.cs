using System.Reflection;
using Tiger;

namespace Tomograph;

public class Helpers
{
    public static TigerStrategy GetTestClassStrategy(Type T)
    {
        TestStrategyAttribute? attribute = (TestStrategyAttribute)T.GetCustomAttribute(typeof(TestStrategyAttribute), false);
        return attribute?.Strategy ?? TigerStrategy.NONE;
    }
    
    public static string GetTestClassDataDirectory(Type T)
    {
        return Path.Join("../../..", "TestData", GetTestClassStrategy(T).ToString());
    }
    
    public static T CallNonPublicMethod<T>(dynamic? instance, string methodName, params object[] parameters)
    {
        MethodInfo dynMethod = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        try
        {
            return (T) dynMethod.Invoke(instance, parameters);
        }
        catch (Exception e)
        {
            throw e.InnerException;
        }
    }

    public static void CallNonPublicMethod(dynamic? instance, string methodName, object[] parameters=null)
    {
        MethodInfo dynMethod = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        try
        {
            dynMethod.Invoke(instance, parameters);
        }
        catch (Exception e)
        {
            throw e.InnerException;
        }
    }

    public static void SetNonPublicStaticField(Type objectType, string fieldName, dynamic? newFieldValue)
    {
        objectType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, newFieldValue);
    }
    
    public static dynamic? GetNonPublicStaticField(Type objectType, string fieldName)
    {
        return objectType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    }

    public static TigerStrategy GetCurrentStrategy()
    {
        // return GetNonPublicStaticField(typeof(Strategy), "_currentStrategy");
        return Strategy.CurrentStrategy;
    }
}