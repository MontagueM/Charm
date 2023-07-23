using System.Diagnostics;
using System.Text;
using Serilog;
using Tiger.Schema;

namespace Tiger;

// could just be BinaryReader extensions but I like renaming it and bundling the changes together
public class TigerReader : BinaryReader
{
    public TigerReader(Stream stream) : base(stream) { }

    public TigerReader(byte[] data) : base(new MemoryStream(data)) { }

    public TigerReader(string filePath) : base(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
    }

    public long Position => BaseStream.Position;

    public long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

    public T ReadSchemaStruct<T>() where T : struct
    {
        return SchemaDeserializer.Get().DeserializeSchema<T>(this);
    }

    public dynamic ReadSchemaStruct(Type type)
    {
        return SchemaDeserializer.Get().DeserializeSchema(this, type);
    }

    public T ReadSchemaType<T>() where T : ITigerDeserialize, new()
    {
        var t = new T();
        t.Deserialize(this);
        return t;
    }

    public dynamic? ReadSchemaType(Type type)
    {
        if (!SchemaDeserializer.Get().IsTigerDeserializeType(type))
        {
            return null;
        }
        ITigerDeserialize t = (ITigerDeserialize)Activator.CreateInstance(type);
        t.Deserialize(this);
        return t;
    }

    public void DumpToFile()
    {
        long pos = Position;
        Seek(0, SeekOrigin.Begin);
        byte[] data = ReadBytes((int)BaseStream.Length);
        File.WriteAllBytes("dump.bin", data);
        Seek(pos, SeekOrigin.Begin);
    }
}

public interface ITigerDeserialize
{
    public void Deserialize(TigerReader reader);
}

[SchemaType(0x10)]
public class DynamicArray<T> : List<T>, ITigerDeserialize
{
    public int Count;
    public RelativePointer Offset;
    protected int _elementSize;

    // todo verify that T is valid for the hash we get given by checking SchemaStruct against hash defined for array

    public virtual void Deserialize(TigerReader reader)
    {
        Count = reader.ReadInt32();
        reader.Seek(4, SeekOrigin.Current);
        Offset = reader.ReadSchemaType<RelativePointer>();
        Offset.AddExtraOffset(0x10);
        reader.Seek(4, SeekOrigin.Current);

        _elementSize = SchemaDeserializer.Get().GetSchemaStructSize<T>();
    }

    public IEnumerable<T> Enumerate(TigerReader reader)
    {
        for (int i = 0; i < Count; i++)
        {
            yield return ReadElement(reader, i);
        }
    }

    public IEnumerable<TResult> Select<TResult>(TigerReader reader, Func<T, TResult> selector)
    {
        return Enumerate(reader).Select(selector);
    }

    public IEnumerable<TResult> Select<TResult>(TigerReader reader, Func<T, int, TResult> selector)
    {
        return Enumerate(reader).Select(selector);
    }

    public new T this[int index]
    {
        get => throw new NotSupportedException("DynamicArray does not store data locally, use with TigerReader or change to DynamicArrayLoaded");
        set => throw new NotSupportedException("DynamicArray is read-only");
    }

    public T this[TigerReader reader, int index]
    {
        get => ElementAt(reader, index);
        set => throw new NotSupportedException("DynamicArray is read-only");
    }

    protected T OriginalIndexAccess(int index)
    {
        return base[index];
    }

    public T ElementAt(TigerReader reader, int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new IndexOutOfRangeException("Index out of range");
        }

        // TigerReader reader = new TigerReader(_arrayData);
        return ReadElement(reader, index);
    }

    protected T ReadElement(TigerReader reader, int index)
    {
        reader.Seek(Offset.AbsoluteOffset + index * _elementSize, SeekOrigin.Begin);
        if (typeof(T).IsValueType) // if T is a struct
        {
            return reader.ReadSchemaStruct(typeof(T));
        }
        else if (SchemaDeserializer.Get().IsTigerDeserializeType(typeof(T)))
        {
            return (T)reader.ReadSchemaType(typeof(T));
        }
        else
        {
            throw new NotSupportedException("T must be a struct or a type that implements ITigerDeserialize");
        }
    }

    /// <summary>
    /// We assume the list is sorted via a position 0 DestinyHash uint32.
    /// </summary>
    /// <param name="hash">Hash to find if it exists.</param>
    /// <returns>The entry of the list if found, otherwise null.</returns>
    public Optional<T> BinarySearch(TigerReader reader, TigerHash hash)
    {
        uint compareValue = hash.Hash32;
        int min = 0;
        int max = Count - 1;
        while (min <= max)
        {
            int mid = (min + max) / 2;
            reader.Seek(Offset.AbsoluteOffset + mid * _elementSize, SeekOrigin.Begin);
            uint midValue = reader.ReadUInt32();
            if (midValue == compareValue)
            {
                return ElementAt(reader, mid);
            }
            if (midValue < compareValue)
            {
                min = mid + 1;
            }
            else
            {
                max = mid - 1;
            }
        }
        return Optional<T>.CreateEmpty();
    }
}

public class Optional<T>
{
    public T? Value { get; private set; }
    public bool HasValue { get; private set; }

    private Optional()
    {
        HasValue = false;
    }

    private Optional(T data)
    {
        HasValue = true;
        Value = data;
    }

    public static implicit operator Optional<T>(T data)
    {
        return new Optional<T>(data);
    }

    public static Optional<T> Create(T value)
    {
        return new Optional<T>(value);
    }

    public static Optional<T> CreateEmpty()
    {
        return new Optional<T>();
    }
}

public class DynamicArrayLoaded<T> : DynamicArray<T> where T : struct
{
    public override void Deserialize(TigerReader reader)
    {
        base.Deserialize(reader);

        for (int i = 0; i < Count; i++)
        {
            reader.Seek(Offset.AbsoluteOffset + i * _elementSize, SeekOrigin.Begin);
            Add(ReadElement(reader, i));
        }
    }

    public new T this[int index]
    {
        get => OriginalIndexAccess(index);
        set => throw new NotSupportedException("DynamicArray is read-only");
    }
}

/// <summary>
/// Assume the first field of T is a uint32 sort key.
/// This allows for binary searching.
/// We actually use interpolation searching as its more efficent since hashes are uniformly distributed.
/// </summary>
public class SortedDynamicArray<T> : DynamicArray<T> where T : struct
{
    public T? InterpolationSearch(TigerReader reader, TigerHash hash)
    {
        int index = InterpolationSearchIndex(reader, 0, Count - 1, hash.Hash32);
        return index == -1 ? null : ElementAt(reader, index);
    }

    public int InterpolationSearchIndex(TigerReader reader, TigerHash hash)
    {
        return InterpolationSearchIndex(reader, 0, Count - 1, hash.Hash32);
    }

    public int InterpolationSearchIndex(TigerReader reader, int lowIndex, int highIndex, uint searchValue)
    {
        reader.Seek(Offset.AbsoluteOffset + lowIndex * _elementSize, SeekOrigin.Begin);
        uint lowValue = reader.ReadUInt32();
        reader.Seek(Offset.AbsoluteOffset + highIndex * _elementSize, SeekOrigin.Begin);
        uint highValue = reader.ReadUInt32();

        // Since array is sorted, an element present in array must be in range defined by corner
        if (lowIndex < highIndex && searchValue >= lowValue && searchValue <= highValue)
        {
            // Probing the position with keeping uniform distribution in mind.
            int testIndex = lowIndex + (int)(((double)(highIndex - lowIndex) / (highValue - lowValue)) * (searchValue - lowValue));
            reader.Seek(Offset.AbsoluteOffset + testIndex * _elementSize, SeekOrigin.Begin);
            uint testValue = reader.ReadUInt32();

            // Condition of target found
            if (testValue == searchValue)
            {
                return testIndex;
            }

            // If x is larger, x is in right sub array
            if (testValue < searchValue)
            {
                return InterpolationSearchIndex(reader, testIndex + 1, highIndex, searchValue);
            }

            // If x is smaller, x is in left sub array
            if (testValue > searchValue)
            {
                return InterpolationSearchIndex(reader, lowIndex, testIndex - 1, searchValue);
            }
        }
        return -1;
    }
}

[SchemaType(0x08)]
public class RelativePointer : ITigerDeserialize
{
    public long AbsoluteOffset => _basePosition + _relativeOffset + _extraOffset;

    protected long _basePosition;
    protected long _relativeOffset;
    protected long _extraOffset;

    public virtual void Deserialize(TigerReader reader)
    {
        _basePosition = reader.BaseStream.Position;
        _relativeOffset = reader.ReadInt64();
    }

    public void AddExtraOffset(int extraOffset)
    {
        _extraOffset += extraOffset;
    }
}

[SchemaType(0x04)]
public class GlobalPointer<T> : ITigerDeserialize where T : struct
{
    public int AbsoluteOffset;
    public T Value;

    public virtual void Deserialize(TigerReader reader)
    {
        AbsoluteOffset = reader.ReadInt32();
        reader.Seek(AbsoluteOffset, SeekOrigin.Begin);
        Value = reader.ReadSchemaStruct<T>();
    }
}

/// <summary>
/// A relative pointer that points to a resource, and the class of resource is specified
/// </summary>
public class ResourcePointer : RelativePointer
{
    public dynamic? Value;
    public uint ResourceClassHash;

    public override void Deserialize(TigerReader reader)
    {
        base.Deserialize(reader);

        if (_relativeOffset == 0)
        {
            Value = null;
            return;
        }

        reader.Seek(AbsoluteOffset - 4, SeekOrigin.Begin);
        ResourceClassHash = reader.ReadUInt32();

        if (SchemaDeserializer.Get().TryGetSchemaType(ResourceClassHash, out Type schemaType))
        {
            Value = reader.ReadSchemaStruct(schemaType);
        }
        else
        {
            Log.Warning($"Unknown resource class hash {ResourceClassHash:X8}");
        }
    }
}

/// <summary>
/// A string that is serialized at the end of the file and referenced by a relative 64-bit pointer.
/// </summary>
public class StringPointer : RelativePointer
{
    public string? Value;

    public override void Deserialize(TigerReader reader)
    {
        base.Deserialize(reader);

        if (_relativeOffset == 0)
        {
            Value = null;
            return;
        }

        reader.Seek(AbsoluteOffset, SeekOrigin.Begin);
        StringBuilder sb = new();
        while (true)
        {
            char c = reader.ReadChar();
            if (c == '\0')
            {
                break;
            }
            sb.Append(c);
        }
        Value = sb.ToString();
    }

    public static implicit operator string?(StringPointer stringPointer) => stringPointer.Value;
}

/// <summary>
/// References a 32-bit <see cref="Tiger.Schema.LocalizedStrings"/> and a 32-bit string hash.
/// </summary>
[SchemaType(0x08)]
public class StringReference : ITigerDeserialize
{
    public TigerString Value;

    public void Deserialize(TigerReader reader)
    {
        FileHash fileHash = new(reader.ReadUInt32());
        LocalizedStrings localizedStrings = FileResourcer.Get().GetFile<LocalizedStrings>(fileHash);
        StringHash stringHash = new(reader.ReadUInt32());
        Value = localizedStrings.GetStringFromHash(stringHash);
    }
}


/// <summary>
/// References a 64-bit <see cref="Tiger.Schema.LocalizedStrings"/> and a 32-bit string hash.
/// </summary>
[SchemaType(0x14)]
public class StringReference64 : ITigerDeserialize
{
    public TigerString Value;

    public void Deserialize(TigerReader reader)
    {
        LocalizedStrings localizedStrings = SchemaDeserializer.DeserializeTag64<LocalizedStrings>(reader);
        StringHash stringHash = new(reader.ReadUInt32());
        Value = localizedStrings.GetStringFromHash(stringHash);
    }
}

/// <summary>
///  A pointer to a resource in a specified table (has a constant type)
/// </summary>
[SchemaType(0x08)]
public class ResourceInTablePointer<T> : ITigerDeserialize where T : struct
{
    public T Value;

    public void Deserialize(TigerReader reader)
    {
        long resourcePointer = reader.ReadInt64();
        reader.Seek(resourcePointer-8, SeekOrigin.Current);
        Value = reader.ReadSchemaStruct<T>();
    }
}

/// <summary>
/// A pointer to a resource in a specified tag
/// </summary>
[SchemaType(0x10)]
public class ResourceInTagPointer : ITigerDeserialize
{
    public void Deserialize(TigerReader reader)
    {
    }
}

/// <summary>
/// The weird one of above todo improve this comment
/// </summary>
[SchemaType(0x18)]
public class ResourceInTagWeirdPointer: ITigerDeserialize
{
    public void Deserialize(TigerReader reader)
    {
    }
}

/// <summary>
///  A relative pointer that points to a resource, and the class of resource is specified
/// </summary>
[SchemaType(0x0C)]
public class ResourcePointerWithClass : ResourcePointer
{
    // todo check this actually works, i cant remember the layout it might not actually have the original class hash?
    public override void Deserialize(TigerReader reader)
    {
        base.Deserialize(reader);
        uint extraResourceClassHash = reader.ReadUInt32();
        Debug.Assert(extraResourceClassHash == ResourceClassHash);
    }
}
