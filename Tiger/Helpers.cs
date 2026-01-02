using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Tiger.Schema;

namespace Tiger;

public static class Helpers
{
    public static string DebugString<T>(this T value)
    {
        StringBuilder sb = new();
        sb.Append($"{typeof(T).Name}(");
        FieldInfo[] fields = typeof(T).GetFields();
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

    public static uint Fnv1a32(string fnvString, bool le = false)
    {
        uint value = 0x811c9dc5;
        for (int i = 0; i < fnvString.Length; i++)
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

    public static void EnsureCapacity(ref byte[] buf, int required, ArrayPool<byte> pool)
    {
        if (buf.Length >= required) return;
        int newSize = buf.Length * 2;
        while (newSize < required) newSize *= 2;
        byte[] newBuf = pool.Rent(newSize);
        Buffer.BlockCopy(buf, 0, newBuf, 0, buf.Length);
        pool.Return(buf);
        buf = newBuf;
    }

    public static string SanitizeString(string input, string replacement = "_")
    {
        string pattern = @"[^a-zA-Z0-9 ]";
        return Regex.Replace(input, pattern, replacement).Trim();
    }

    public static byte[] HexStringToByteArray(string hex)
    {
        // Ensure the string length is even
        if (hex.Length % 2 != 0)
        {
            throw new ArgumentException("The hexadecimal string must have an even length.");
        }

        // Create a byte array with half the length of the hexadecimal string
        byte[] bytes = new byte[hex.Length / 2];

        for (int i = 0; i < hex.Length; i += 2)
        {
            // Parse each pair of hexadecimal characters
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }

    public static string GetReadableSize(long byteLength)
    {
        string[] sizeSuffixes = { "B", "KB", "MB", "GB" };
        if (byteLength is 0 or < 0) return "0 B";

        int suffixIndex = (int)Math.Floor(Math.Log(byteLength, 1024));
        double readableValue = byteLength / Math.Pow(1024, suffixIndex);

        return $"{readableValue:0.##} {sizeSuffixes[suffixIndex]}";
    }

    // This is fine :)
    public static (ushort width, ushort height, ushort depth, ushort array_size) GetTextureDimensionsRaw(FileHash hash)
    {
        byte[] data = PackageResourcer.Get().GetFileData(hash);
        using (TigerReader br = new(data))
        {
            int offset = Strategy.IsD1() ? 0x28 : Strategy.IsPreBL() ? 0x0E : 0x22;
            br.Seek(offset, SeekOrigin.Begin);
            ushort width = br.ReadUInt16();
            ushort height = br.ReadUInt16();
            ushort depth = br.ReadUInt16();
            ushort array_size = br.ReadUInt16();
            return (width, height, depth, array_size);
        }
    }

    public static bool IsValidHexHash(string input)
    {
        return input.Length == 8 &&
               input.All(c => Uri.IsHexDigit(c));
    }

    public static bool ParseHash(in string searchStr, out uint parsedHash)
    {
        bool isValidHash = Helpers.IsValidHexHash(searchStr);
        if (isValidHash &&
            (searchStr.StartsWith("80") || searchStr.StartsWith("81")) &&
            (!searchStr.EndsWith("80") && !searchStr.EndsWith("81")))
        {
            byte[] bytes = Helpers.HexStringToByteArray(searchStr);
            Array.Reverse(bytes);
            parsedHash = new TigerHash(BitConverter.ToUInt32(bytes)).Hash32;
            return true;
        }
        else if (isValidHash && (searchStr.EndsWith("80") || searchStr.EndsWith("81")))
        {
            parsedHash = new TigerHash(searchStr).Hash32;
            return true;
        }
        parsedHash = 0;
        return false;
    }

    public static string? GetClassHashForStrategy(Type structType, TigerStrategy strategy)
    {
        var attrs = structType.GetCustomAttributes(inherit: false)
            .OfType<SchemaStructAttribute>()
            .ToList();

        // Try exact match first
        var match = attrs.FirstOrDefault(a => a.Strategy == strategy);
        if (match != null)
            return match.ClassHash;

        // If not found, try the highest lower strategy
        // ex: if SHADOWKEEP_2999 isnt defined, use SHADOWKEEP_2601 (or RISE_OF_IRON if 2601 isnt defined either)
        var lower = attrs
            .Where(a => a.Strategy < strategy)
            .OrderByDescending(a => a.Strategy)
            .FirstOrDefault();

        if (lower != null)
            return lower?.ClassHash;

        // Worst case, use the next higher strategy (which will probably be the wrong class hash)
        var nextHighest = attrs
            .Where(a => a.Strategy > strategy)
            .OrderBy(a => a.Strategy)
            .FirstOrDefault();

        if (nextHighest != null)
            return nextHighest.ClassHash;

        return null;
    }

    public static uint HashCombine(params uint[] values)
    {
        unchecked
        {
            uint hash = 0;
            foreach (uint v in values)
                hash ^= v + 0x9e3779b9u + (hash << 6) + (hash >> 2);

            return hash;
        }
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

        return Color.FromArgb(Math.Clamp(color.A, (byte)0, (byte)255), Math.Clamp(r, 0, 255), Math.Clamp(g, 0, 255), Math.Clamp(b, 0, 255));
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

public static class EnumExtensions
{
    public static IEnumerable<Enum> GetFlags(Enum input)
    {
        foreach (Enum value in Enum.GetValues(input.GetType()))
        {
            if (input.HasFlag(value))
            {
                yield return value;
            }
        }
    }

    public static string GetEnumDescription(this Enum enumValue)
    {
        var underlyingType = Enum.GetUnderlyingType(enumValue.GetType());
        long value = underlyingType == typeof(uint) || underlyingType == typeof(ulong)
            ? Convert.ToInt64(Convert.ToUInt64(enumValue))
            : Convert.ToInt64(enumValue);

        if (value == -1)
            return string.Empty;

        FieldInfo? fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        if (fieldInfo == null)
            return "";

        var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
    }
}
