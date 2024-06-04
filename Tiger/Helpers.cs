using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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

    public static void DecorateSignaturesWithBufferIndex(ref DXBCIOSignature[] inputSignatures, List<int> strides)
    {
        if (!strides.Any())
        {
            return;
        }
        int bufferIndex = 0;
        int offset = 0;
        int strideBound = strides[bufferIndex];
        foreach (ref DXBCIOSignature inputSignature in inputSignatures.AsSpan())
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

            if (inputSignature.Semantic == DXBCSemantic.Colour)
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
        if (fieldInfo == null)
            return "";

        var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
    }
}

public static class ColorUtility
{
    public static Color[] GenerateShades(Color baseColor, int numberOfShades, float lightnessFactor)
    {
        if (numberOfShades < 1)
        {
            throw new ArgumentException("Number of shades must be at least 1", nameof(numberOfShades));
        }

        Color[] shades = new Color[numberOfShades];

        if (numberOfShades == 1)
        {
            shades[0] = ChangeColorBrightness(baseColor, lightnessFactor);
        }
        else
        {
            // Calculate step size for adjusting lightness
            float step = lightnessFactor / (numberOfShades - 1);

            // Generate lighter shades
            for (int i = 0; i < numberOfShades; i++)
            {
                float newLightness = Math.Min(1f, baseColor.GetBrightness() + i * step);
                shades[i] = ChangeColorBrightness(baseColor, newLightness);
            }
        }

        return shades;
    }

    private static Color ChangeColorBrightness(Color color, float newBrightness)
    {
        float hue, saturation;
        int r = color.R;
        int g = color.G;
        int b = color.B;

        // Convert RGB to HSL
        ColorToHSL(color, out hue, out saturation, out _);

        // Convert HSL to RGB with the new brightness
        HSLToColor(hue, saturation, newBrightness, out r, out g, out b);

        return Color.FromArgb(color.A, r, g, b);
    }

    private static void ColorToHSL(Color color, out float hue, out float saturation, out float lightness)
    {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        float min = Math.Min(Math.Min(r, g), b);
        float max = Math.Max(Math.Max(r, g), b);

        float delta = max - min;

        // Calculate lightness
        lightness = (max + min) / 2f;

        // Calculate hue
        hue = 0f;
        if (delta != 0)
        {
            if (max == r)
            {
                hue = ((g - b) / delta) % 6f;
            }
            else if (max == g)
            {
                hue = ((b - r) / delta) + 2f;
            }
            else
            {
                hue = ((r - g) / delta) + 4f;
            }
        }
        hue *= 60;

        // Calculate saturation
        saturation = delta == 0 ? 0 : delta / (1 - Math.Abs(2 * lightness - 1));
    }

    private static void HSLToColor(float hue, float saturation, float lightness, out int r, out int g, out int b)
    {
        if (saturation == 0)
        {
            r = g = b = (int)(lightness * 255);
        }
        else
        {
            float q = lightness < 0.5 ? lightness * (1 + saturation) : lightness + saturation - lightness * saturation;
            float p = 2 * lightness - q;

            float hueNormalized = hue / 360f;

            r = (int)(255 * HueToRGB(p, q, hueNormalized + 1f / 3f));
            g = (int)(255 * HueToRGB(p, q, hueNormalized));
            b = (int)(255 * HueToRGB(p, q, hueNormalized - 1f / 3f));
        }
    }

    private static float HueToRGB(float p, float q, float t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }

    public static Color BlendColors(Color baseColor, Color overlayColor, byte mask)
    {
        return Color.FromArgb(baseColor.A,
            BlendColors(BlendColors(baseColor.R, overlayColor.R), mask),
            BlendColors(BlendColors(baseColor.G, overlayColor.G), mask),
            BlendColors(BlendColors(baseColor.B, overlayColor.B), mask));
    }

    public static byte BlendColors(byte baseColor, byte overlayColor)
    {
        return (byte)(((baseColor * overlayColor) + 0xFF) >> 8);
    }

    public static Color AddColors(Color color1, Color color2)
    {
        return Color.FromArgb(color1.A,
             AddColors(color1.R, color2.R),
             AddColors(color1.G, color2.G),
             AddColors(color1.B, color2.B));
    }

    public static byte AddColors(byte baseColor, byte overlayColor)
    {
        return (byte)(baseColor + overlayColor);
    }

    public static bool IsZero(this Color color)
    {
        return (color.R <= 0 && color.G <= 0 && color.B <= 0);
    }
}
