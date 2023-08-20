using System.Reflection;
using System.Text;

namespace Tiger;

public static class Helpers
{
    public static string DebugString<T>(this T value)
    {
        StringBuilder sb = new();
        sb.Append($"{typeof(T).Name}(");
        var fields = typeof(T).GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            sb.Append($"{fieldInfo.Name}: {fieldInfo.GetValue(value)}, ");
        }
        sb.Remove(sb.Length - 2, 2);
        sb.Append(')');

        return sb.ToString();
    }

    public static string DebugString<T>(this List<T> value)
    {
        StringBuilder sb = new();
        sb.Append($"{typeof(T).Name}List[");
        foreach (T item in value)
        {
            sb.Append($"{item.DebugString()}, ");
        }
        sb.Remove(sb.Length - 2, 2);
        sb.Append(']');

        return sb.ToString();
    }
}

public static class NestedTypeHelpers
{
    public static Type? FindNestedGenericType<T>()
    {
        Type? nestedType = null;

        Type testType = typeof(T);
        while (nestedType == null && testType != null && testType != typeof(object))
        {
            if (testType.IsGenericType)
            {
                nestedType = testType.GenericTypeArguments[0];
            }
            else
            {
                testType = testType.BaseType;
            }
        }

        return nestedType;
    }

    public static Type? GetNonGenericParent(this Type inTestType, Type inheritParentType)
    {
        Type? testType = inTestType;
        while (testType != null && testType != typeof(object))
        {
            if (testType.IsGenericType && testType.GenericTypeArguments.Length > 0 && testType.GetGenericTypeDefinition() == inheritParentType)
            {
                return testType;
            }
            else
            {
                testType = testType.BaseType;
            }
        }

        return null;
    }
}
