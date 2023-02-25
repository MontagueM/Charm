namespace Resourcer;


/// <summary>
/// Represents string that has been hashed via the FNV1-32 algorithm.
/// </summary>
public class StringHash
{
    private protected readonly uint Hash32;
    public static readonly uint INVALID_HASH32 = 0x811c9dc5;
    
    public StringHash(uint hash32)
    {
        Hash32 = hash32;
    }
}

/// <summary>
/// A TigerHash represents any hash that is used to identify something within Tiger.
/// These are all implemented as a 32-bit hash.
/// See "FileHash", "File64Hash", and "TagClassHash" for children of this class.
/// </summary>
public class TigerHash : IComparable<TigerHash>, IEquatable<TigerHash>
{
    public uint Hash32 { get; }
    public static readonly uint INVALID_HASH32 = 0xFFFFFFFF;
    
    public TigerHash(uint hash32)
    {
        Hash32 = hash32;
    }

    public int CompareTo(TigerHash? other)
    {
        if (other == null)
        {
            return 1;
        }

        return Hash32.CompareTo(other.Hash32);
    }

    public bool Equals(TigerHash? other)
    {
        if (other == null)
        {
            return false;
        }

        return Hash32 == other.Hash32;
    }
}

/// <summary>
/// Represents a package hash, which is a combination of the EntryId and PkgId e.g. ABCD5680.
/// </summary>
public class FileHash : TigerHash
{
    public FileHash(int packageId, uint fileIndex) : base(GetHash32(packageId, fileIndex))
    {
    }

    public FileHash(uint hash32) : base(hash32)
    {
    }

    public static uint GetHash32(int packageId, uint fileIndex)
    {
        return (uint)(0x80800000 + (packageId << 0xD) + fileIndex);
    }

    public ushort GetPackageId()
    {
        return (ushort)((Hash32 >> 0xd) & 0x3ff | (Hash32 & 0x1000000) >> 0x0E);
    }
    
    public ushort GetFileIndex()
    {
        return (ushort)(Hash32 & 0x1fff);
    }
    
    public override string ToString()
    {
        return Endian.U32ToString(Hash32);
    }
}

/// <summary>
/// Same as FileHash, but represents a 64-bit version that is used as a hard reference to a tag. Helps to keep
/// files more similar as FileHash's can change, but the 64-bit version will always be the same.
/// </summary>
public class File64Hash : FileHash
{
    public File64Hash(ulong hash64) : base(GetHash32(hash64))
    {
    }
    
    private static uint GetHash32(ulong hash64)
    {
        return INVALID_HASH32;
    }
}

/// <summary>
/// Represents the type a tag can be. These can exist as package references which stores the type of the tag file,
/// or within the tag file itself which represents the resource types and other tags inside e.g. ABCD8080.
/// </summary>
public class TagClassHash : TigerHash
{
    public TagClassHash(uint hash32) : base(hash32)
    {
    }
}