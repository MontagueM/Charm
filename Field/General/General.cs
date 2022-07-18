﻿using System.Globalization;
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
    public uint Hash = 0x811c9dc5;
    private string _string;
     
    public DestinyHash(string hash, bool bBigEndianString = false)
    {
        bool parsed = uint.TryParse(hash, NumberStyles.HexNumber, null, out Hash);
        if (parsed)
        {
            if (hash.EndsWith("80") || bBigEndianString)
            {
                Hash = Endian.SwapU32(Hash);
            }
            _string = FnvHandler.GetStringFromHash(Hash);
        }
    }
        
    public DestinyHash(uint hash)
    {
        Hash = hash;
        _string = FnvHandler.GetStringFromHash(Hash);
    }

    public DestinyHash()
    {
    }
    
    public virtual bool IsValid()
    {
        if (Hash == 0x811c9dc5)
        {
            return false;
        }
        return true;
    }

    public int GetPkgId()
    {
        return (int)((Hash >> 0xD) & 0x3ff);
    }
        
    public string GetHashString()
    {
        return Endian.U32ToString(Hash);
    }

    
    public override string ToString()
    {
        if (_string != "") return _string;
        return GetHashString();
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
    
    public bool CompareTo(D2Class_454F8080 x, DestinyHash y)
    {
        return x.AssignmentHash.Equals(y);
    }
    
    public static implicit operator uint(DestinyHash d) => d.Hash;
    public static implicit operator string(DestinyHash d) => d.GetHashString();
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public class TagHash : DestinyHash
{
    public TagHash(string hash) : base(hash)
    {
    }
    
    public TagHash(DestinyHash hash) : base(hash)
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
        return (T)Activator.CreateInstance(typeof(T), new object[] { GetHashString() });
    }
    
    public override bool IsValid()
    {
        if (Hash < 0x80a00000 || Hash > 0x80ffffff)
        {
            return false;
        }
        
        // If really wanted to, could check if the pkg id exists etc but bad for performance

        return true;
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

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct InlineGlobalPointer
{
    [DestinyField(FieldType.TagHash)]
    public Tag TargetTag;
    public TagTypeHash Class;
    public long Offset;
}

[StructLayout(LayoutKind.Sequential, Size = 0x4)]
public struct TagTypeHash
{
    public DestinyHash Hash;
}