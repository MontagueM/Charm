using System.Globalization;
using Tiger.Schema;
using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;

namespace Tiger;

public interface IHash
{
    public bool IsValid();
}


/// <summary>
/// Represents string that has been hashed via the FNV1-32 algorithm.
/// </summary>
[SchemaType(0x04)]
public class StringHash : TigerHash
{
    public const uint InvalidHash32 = 0x811c9dc5;

    public StringHash(uint hash32) : base(hash32)
    {
        Hash32 = hash32;
    }

    public StringHash() : base(InvalidHash32)
    {
    }

    public StringHash(string hash) : base(hash)
    {
    }

    public static StringHash Invalid => new(InvalidHash32);

    public override bool IsValid()
    {
        return Hash32 != InvalidHash32 && Hash32 != 0;
    }
}

/// <summary>
/// A TigerHash represents any hash that is used to identify something within Tiger.
/// These are all implemented as a 32-bit hash.
/// See "FileHash", "FileHash64", and "TagClassHash" for children of this class.
/// </summary>
[SchemaType(0x04)]
public class TigerHash : IHash, ITigerDeserialize, IComparable<TigerHash>, IEquatable<TigerHash>, IEqualityComparer<TigerHash>
{
#pragma warning disable S1104
    public uint Hash32;
    public const uint InvalidHash32 = 0xFFFFFFFF;

    public TigerHash()
    {
        Hash32 = InvalidHash32;
    }

    public TigerHash(uint hash32)
    {
        Hash32 = hash32;
    }

    public TigerHash(string hash, bool bBigEndianString = true)
    {
        bool parsed = uint.TryParse(hash, NumberStyles.HexNumber, null, out Hash32);
        if (parsed)
        {
            if (hash.EndsWith("80") || hash.EndsWith("81") || bBigEndianString)
            {
                Hash32 = Endian.SwapU32(Hash32);
            }
        }
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

    public virtual bool IsValid()
    {
        return Hash32 != InvalidHash32 && Hash32 != 0;
    }

    public bool IsInvalid()
    {
        return !IsValid();
    }

    public override string ToString()
    {
        return Endian.U32ToString(Hash32);
    }

    public static implicit operator string(TigerHash hash) => hash.ToString();

    public static implicit operator uint(TigerHash hash) => hash.Hash32;

    public static bool operator ==(TigerHash x, TigerHash y)
    {
        if (x is null)
        {
            return y is null;
        }

        return x.Equals(y);
    }

    public static bool operator !=(TigerHash x, TigerHash y)
    {
        if (x is null)
        {
            return y is not null;
        }
        return !x.Equals(y);
    }

    public virtual void Deserialize(TigerReader reader)
    {
        Hash32 = reader.ReadUInt32();
    }

    public override bool Equals(object? obj)
    {
        return Equals(this, obj as TigerHash);
    }

    public bool Equals(TigerHash x, TigerHash y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Hash32 == y.Hash32;
    }

    public override int GetHashCode()
    {
        return (int)Hash32;
    }

    public int GetHashCode(TigerHash obj)
    {
        return obj.GetHashCode();
    }
}

/// <summary>
/// Represents a package hash, which is a combination of the EntryId and PkgId e.g. ABCD5680.
/// </summary>
[SchemaType(0x04)]
public class FileHash : TigerHash
{
    public FileHash() : base()
    {
    }

    public FileHash(int packageId, uint fileIndex) : base(GetHash32(packageId, fileIndex))
    {
    }

    public FileHash(uint hash32) : base(hash32)
    {
    }

    public FileHash(string hash) : base(hash)
    {
    }

    public static uint GetHash32(int packageId, uint fileIndex)
    {
        return (uint)(0x80800000 + (packageId << 0xD) + fileIndex);
    }

    public ushort PackageId
    {
        get
        {
            return (ushort)(((Hash32 >> 0xd) & 0x3ff) + (((Hash32 >> 0x17) & 3) - 1) * 0x400);
        }
    }

    public ushort FileIndex => (ushort)(Hash32 & 0x1fff);
}

public static class FileHashExtensions
{
    public static FileHash GetReferenceHash(this FileHash fileHash)
    {
        if (fileHash.IsInvalid())
        {
            throw new Exception($"Cannot get reference hash for invalid file hash {fileHash}.");
        }
        return new FileHash(fileHash.GetFileMetadata().Reference.Hash32);
    }

    public static FileMetadata GetFileMetadata(this FileHash fileHash)
    {
        return PackageResourcer.Get().GetFileMetadata(fileHash);
    }

    public static byte[] GetFileData(this FileHash fileHash)
    {
        return PackageResourcer.Get().GetFileData(fileHash);
    }

    // D1 Only, TagGlobals use a non 8080 reference tag that has a reference of 48018080, that "parent" tag has the class hash
    public static FileHash? GetReferenceFromManifest(this FileHash fileHash)
    {
        if (Strategy.CurrentStrategy > TigerStrategy.DESTINY1_RISE_OF_IRON)
            return fileHash.GetReferenceHash();

        var temp = FileResourcer.Get().GetSchemaTag<S48018080>(fileHash.GetReferenceHash());
        return new FileHash(temp.TagData.Reference.Hash32);
    }

    public static bool ContainsHash(this FileHash fileHash, uint searchValue)
    {
        var data = PackageResourcer.Get().GetFileData(fileHash);
        using (TigerReader br = new TigerReader(data))
        {
            long position = 0;
            long length = data.Length;
            while (position + sizeof(uint) <= length)
            {
                uint value = br.ReadUInt32();
                if (value == searchValue)
                {
                    return true;
                }
                position += sizeof(uint);
            }
            return false;
        }
    }
}

/// <summary>
/// Same as FileHash, but represents a 64-bit version that is used as a hard reference to a tag. Helps to keep
/// files more similar as FileHash's can change, but the 64-bit version will always be the same.
/// </summary>
[SchemaType(0x10)]
public class FileHash64 : FileHash
{
    private ulong Hash64 { get; set; }
    private bool IsHash32 { get; set; }
    private uint FallbackHash32 { get; set; }

    public FileHash64() : base()
    {
    }

    public FileHash64(ulong hash64) : base(GetHash32(hash64))
    {
        Hash64 = hash64;
    }

    private static uint GetHash32(ulong hash64)
    {
        return Hash64Map.Get().GetHash32(hash64);
    }

    public override void Deserialize(TigerReader reader)
    {
        FallbackHash32 = reader.ReadUInt32();
        uint _isHash32 = reader.ReadUInt32();
        IsHash32 = _isHash32 == 1 || _isHash32 == 2;
        Hash64 = reader.ReadUInt64();
        Hash32 = IsHash32 ? FallbackHash32 : GetHash32(Hash64);
    }
}

/// <summary>
/// Represents the type a tag can be. These can exist as package references which stores the type of the tag file,
/// or within the tag file itself which represents the resource types and other tags inside e.g. ABCD8080.
/// </summary>
public class TagClassHash : TigerHash
{
    public TagClassHash() : base()
    {
    }

    public TagClassHash(uint hash32) : base(hash32)
    {
    }
}
