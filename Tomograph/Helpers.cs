using System.Reflection;
using Resourcer;

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
}