using System.Globalization;
using System.Runtime.InteropServices;

namespace Field.General;

public static class Endian
{
    
    public static UInt32 SwapU32(UInt32 value)
    {
        return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
               (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
    }
    
    public static UInt64 SwapU64(UInt64 value)
    {
        return (0x00000000000000FF) & (value >> 56)
               | (0x000000000000FF00) & (value >> 40)
               | (0x0000000000FF0000) & (value >> 24)
               | (0x00000000FF000000) & (value >> 8)
               | (0x000000FF00000000) & (value << 8)
               | (0x0000FF0000000000) & (value << 24)
               | (0x00FF000000000000) & (value << 40)
               | (0xFF00000000000000) & (value << 56);
    }

    public static string U32ToString(uint number)
    {
        byte[] bytes = BitConverter.GetBytes(number);
        string retval = "";
        foreach (byte b in bytes)
            retval += b.ToString("X2");
        return retval;
    }
}
    
[StructLayout(LayoutKind.Sequential, Size = 4)]
public class DestinyHash : IComparable<DestinyHash>
{
    public uint Hash;
     
    public DestinyHash(string hash)
    {
        Hash = uint.Parse(hash, NumberStyles.HexNumber);
        if (hash.EndsWith("80"))
        {
            Hash = Endian.SwapU32(Hash);
        }
    }
        
    public DestinyHash(uint hash)
    {
        Hash = hash;
    }

    public DestinyHash()
    {
    }
        
    public string GetString()
    {
        return Endian.U32ToString(Hash);
    }

    public override string ToString()
    {
        return GetString();
    }
        
    public override int GetHashCode()
    {
        return (int)Hash;
    }

    public override bool Equals(object other)
    {
        return Equals(other as DestinyHash);
    }

    public bool Equals(DestinyHash other)
    {
        return (other != null &&
                other.Hash == Hash);
    }

    public int CompareTo(DestinyHash other)
    {
        if (Hash > other.Hash)
        {
            return 1;
        }

        if (Hash < other.Hash)
        {
            return -1;
        }

        return 0;
    }
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public class TagHash : DestinyHash
{
    public TagHash(string hash) : base(hash)
    {
    }
        
    public TagHash(uint hash) : base(hash)
    {
    }
        
    public TagHash(ulong hash) : base(TagHash64Handler.GetTagHash64(hash))
    {
    }
        
    public T ToTag<T>() where T : Tag, new()
    {
        return (T)Activator.CreateInstance(typeof(T), new object[] { GetString() });
    }
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct RelativePointer
{
    [FieldOffset(0x00)]
    private long Value;
        
    public static long Original;

    private void SetOriginal()
    {
        Original = Value;
    }
        
    public long Add(long currentAddress)
    {
        SetOriginal();
        Value += currentAddress;
        return Value;
    }

    public long Get()
    {
        return Value;
    }
}

public enum EntityResourceType
{
    EntityModelHeader,
    EntitySkeleton,
}

[StructLayout(LayoutKind.Sequential, Size = 0x8)]
public struct Resource
{
    public RelativePointer Pointer;

    public static object Struct;
    public static EntityResourceType Type;

    // public void Read<T>(BinaryReader handle, EntityResourceType entityResourceType) where T : struct
    // {
    //     Type = entityResourceType;
    //     handle.BaseStream.Seek(Pointer.Get(), SeekOrigin.Begin);
    //     Struct = Tag.ReadStruct(typeof(T), handle);
    // }

    public object GetStruct()
    {
        return Struct;
    }
        
    public EntityResourceType GetType()
    {
        return Type;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct InlineClass
{
    public int Zeros00;
    public TagTypeHash Class;
}

[StructLayout(LayoutKind.Sequential)]
public struct InlineGlobalPointer
{
    public TagHash TargetTag;
    public TagTypeHash Class;
    public long Offset;
}

[StructLayout(LayoutKind.Sequential, Size = 0x4)]
public struct TagTypeHash
{
    public DestinyHash Hash;
}