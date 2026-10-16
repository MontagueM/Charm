using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

// This file is for basic types that are used in the schema, such as bools, ints, floats, etc.

[SchemaStruct(0x80800004, 1)] // 04008080
public struct SBool
{
    public bool Value;
}

[SchemaStruct(0x80800005, 1)] // 05008080
public struct SInt8
{
    public sbyte Value;
}

[SchemaStruct(0x80800006, 2)] // 06008080
public struct SInt16
{
    public short Value;
}

[SchemaStruct(0x80800007, 4)] // 07008080
public struct SInt32
{
    public int Value;
}

[SchemaStruct(0x80800008, 8)] // 08008080
public struct SInt64
{
    public long Value;
}

[SchemaStruct(0x80800009, 1)] // 09008080
public struct SUInt8
{
    public byte Value;
}

[SchemaStruct(0x8080000A, 2)] // 0A008080
public struct SUInt16
{
    public ushort Value;
}

[SchemaStruct(0x8080000B, 4)] // 0B008080
public struct SUInt32
{
    public uint Value;
}

[SchemaStruct(0x8080000C, 8)] // 0C008080
public struct SUInt64
{
    public ulong Value;
}

[SchemaStruct(0x8080000F, 4)] // 0F008080
public struct SReal32
{
    public float Value;
}

[SchemaStruct(0x80800014, 0x4)] // 14008080
public struct SMaterialHash
{
    public Material Material;
}

[SchemaStruct(0x80800090, 0x10)] // 90008080
public struct Vec4
{
    public Vector4 Vec;
}

public static class PrimitiveArrays
{
    public static ushort[] ReadUInt16(TigerReader reader, DynamicArray<SUInt16> array) => Read<ushort, SUInt16>(reader, array);
    public static short[] ReadInt16(TigerReader reader, DynamicArray<SInt16> array) => Read<short, SInt16>(reader, array);
    public static byte[] ReadByte(TigerReader reader, DynamicArray<SUInt8> array) => Read<byte, SUInt8>(reader, array);
    public static float[] ReadSingle(TigerReader reader, DynamicArray<SReal32> array) => Read<float, SReal32>(reader, array);
    public static System.Numerics.Vector4[] ReadVector4(TigerReader reader, DynamicArray<Vec4> array) => Read<System.Numerics.Vector4, Vec4>(reader, array);

    private static T[] Read<T, TSchema>(TigerReader reader, DynamicArray<TSchema> array) where T : unmanaged
    {
        int count = array.Count;
        if (count <= 0)
        {
            return Array.Empty<T>();
        }

        long offset = array.Offset.AbsoluteOffset;
        long byteLength = (long)count * Unsafe.SizeOf<T>();
        if (offset < 0 || offset + byteLength > reader.BaseStream.Length)
        {
            throw new InvalidDataException($"Array of {count} elements at 0x{offset:X} does not fit in the tag ({reader.BaseStream.Length} bytes)");
        }

        var result = new T[count];
        reader.Seek(offset, SeekOrigin.Begin);
        reader.BaseStream.ReadExactly(MemoryMarshal.AsBytes(result.AsSpan()));
        return result;
    }
}
