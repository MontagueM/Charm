using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Tiger.Schema;

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

    public static void DecorateSignaturesWithBufferIndex(ref InputSignature[] inputSignatures, List<int> strides)
    {
        if (!strides.Any())
        {
            return;
        }
        int bufferIndex = 0;
        int offset = 0;
        int strideBound = strides[bufferIndex];
        foreach (ref InputSignature inputSignature in inputSignatures.AsSpan())
        {
            if (offset < strideBound)
            {
                inputSignature.BufferIndex = bufferIndex;
            }
            else
            {
                strideBound += strides[bufferIndex++];
                inputSignature.BufferIndex = bufferIndex;
            }

            if (inputSignature.Semantic == InputSemantic.Colour)
            {
                offset += inputSignature.GetNumberOfComponents() * 1;  // 1 byte per component
            }
            else
            {
                if (inputSignature.ComponentType == RegisterComponentType.Float32)
                {
                    // todo figure out how to handle this
                    offset += inputSignature.GetNumberOfComponents() * 2;  // 4 bytes per component
                }
                else
                {
                    offset += inputSignature.GetNumberOfComponents() * 2;  // 2 bytes per component
                }
            }
        }
        // its possible for there to be buffers that are used as direct buffers instead of per-vertex (e.g. vertex colour)
        // however, it's impossible for there to be more semantics than the stride max
        Debug.Assert(strideBound + 4 >= offset);
    }

    public static uint Fnv(string fnvString, bool le = false)
    {
        uint value = 0x811c9dc5;
        for (var i = 0; i < fnvString.Length; i++)
        {
            value *= 0x01000193;
            value ^= fnvString[i];
        }
        if (le)
        {
            byte[] littleEndianBytes = BitConverter.GetBytes(value);
            Array.Reverse(littleEndianBytes);
            return BitConverter.ToUInt32(littleEndianBytes, 0);
        }
        else
            return value;
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

    public static string GetEnumDescription(this Enum enumValue)
    {
        if (Convert.ToInt32(enumValue) == -1)
            return string.Empty;

        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
    }
}
