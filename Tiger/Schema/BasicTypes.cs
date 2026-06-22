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
