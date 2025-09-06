using System.Diagnostics;
using System.Text;
using Tiger.Schema.Investment;
using Tiger.Schema.Strings;

namespace Tiger;

/// <summary>
/// The heart of it all.
/// A specialized binary reader for deserializing data using the Tiger schema system.
/// Provides helper methods for reading schema-defined structs and types.
/// The optional <see cref="Hash"/> property can be used to identify the context or origin of the stream.
/// </summary>
public class TigerReader : BinaryReader
{
    public TigerReader(Stream stream, uint hash = 0xFFFFFFFF) : base(stream) { Hash = hash; }

    public TigerReader(byte[] data) : base(new MemoryStream(data)) { }

    public TigerReader(string filePath) : base(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
    }

    public uint Hash { get; set; } = 0xFFFFFFFF;

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
        DateTime curTime = DateTime.Now;
        string dump = $"dump_{curTime.Date.ToShortDateString().Replace('/', '-')}_{curTime.Hour}.{curTime.Minute}.{curTime.Second}";
        Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/dumps/");

        long pos = Position;
        Seek(0, SeekOrigin.Begin);
        byte[] data = ReadBytes((int)BaseStream.Length);
        File.WriteAllBytes($"{Directory.GetCurrentDirectory()}/dumps/{dump}.bin", data);
        Seek(pos, SeekOrigin.Begin);
    }

    public string ReadNullTerminatedString()
    {
        StringBuilder sb = new();
        char c;
        while ((c = ReadChar()) != 0)
        {
            sb.Append(c);
        }
        return sb.ToString();
    }
}

public interface ITigerDeserialize
{
    public void Deserialize(TigerReader reader);
}

/// <summary>
/// A dynamic array structure used to deserialize a list of items from data when the count and offset are explicitly defined.
/// 
/// This type first reads a 32-bit integer <c>Count</c>, which defines how many elements to read, followed by a <see cref="RelativePointer"/> <c>Offset</c>,
/// which points to the start of the array data relative to the current base position.
/// 
/// Each element is assumed to have a fixed size based on its schema definition, and is read sequentially starting from the absolute offset.
/// </summary>
[SchemaType(0x10)]
public class DynamicArray<T> : List<T>, ITigerDeserialize
{
    public int Count;
    public RelativePointer Offset;
    protected int _elementSize;
    // todo verify that T is valid for the hash we get given by checking SchemaStruct against hash defined for array

    public virtual void Deserialize(TigerReader reader)
    {
        try
        {
            Count = reader.ReadInt32();
            reader.Seek(4, SeekOrigin.Current);
            Offset = reader.ReadSchemaType<RelativePointer>();
            Offset.AddExtraOffset(0x10);
            reader.Seek(4, SeekOrigin.Current);

            _elementSize = SchemaDeserializer.Get().GetSchemaStructSize<T>();

            for (int i = 0; i < Count; i++)
            {
                reader.Seek(Offset.AbsoluteOffset + i * _elementSize, SeekOrigin.Begin);
                Add(ReadElement(reader, i));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"[{reader.Hash:X2}] Failed to read at position {reader.BaseStream.Position:X2}. Stream length: {reader.BaseStream.Length:X2}", ex);
        }
    }

    protected T ReadElement(TigerReader reader, int index)
    {
        // todo consolidate with SchemaDeserializer
        reader.Seek(Offset.AbsoluteOffset + index * _elementSize, SeekOrigin.Begin);
        if (typeof(T).IsValueType) // if T is a struct
        {
            return reader.ReadSchemaStruct(typeof(T));
        }
        else if (SchemaDeserializer.Get().IsTigerDeserializeType(typeof(T)))
        {
            return (T)reader.ReadSchemaType(typeof(T));
        }
        else if (SchemaDeserializer.Get().IsTigerFileType(typeof(T)))
        {
            FileHash fileHash = new(reader.ReadUInt32());
            return FileResourcer.Get().GetFile(typeof(T), fileHash);
        }
        else
        {
            throw new NotSupportedException($"T must be a struct or a type that implements ITigerDeserialize ({reader.Hash:X})");
        }
    }
}

/// <summary>
/// This behaves similarly to <see cref="DynamicArray{T}"/>, except it only captures the <c>Count</c> and <c>Offset</c> for later use.
/// 
/// Useful for handling large amounts of data sets where immediate in-memory storage is undesirable.
/// Instead of materializing the full array, the <see cref="Enumerate"/> and <see cref="ElementAt(TigerReader, int)"/> methods allow access
/// to elements directly from the stream via a <see cref="TigerReader"/> as needed.
/// 
/// Attempting to access or manipulate the array using standard list methods (e.g., indexers or LINQ) will throw, as the underlying data is not stored.
/// </summary>
public class DynamicArrayUnloaded<T> : DynamicArray<T>
{
    public override void Deserialize(TigerReader reader)
    {
        try
        {
            Count = reader.ReadInt32();
            reader.Seek(4, SeekOrigin.Current);
            Offset = reader.ReadSchemaType<RelativePointer>();
            Offset.AddExtraOffset(0x10);
            reader.Seek(4, SeekOrigin.Current);

            _elementSize = SchemaDeserializer.Get().GetSchemaStructSize<T>();
        }
        catch (Exception ex)
        {
            throw new Exception($"[{reader.Hash:X2}] Failed to read at position {reader.BaseStream.Position:X2}. Stream length: {reader.BaseStream.Length:X2}", ex);
        }
    }

    public IEnumerable<T> Enumerate(TigerReader reader)
    {
        for (int i = 0; i < Count; i++)
        {
            yield return ReadElement(reader, i);
        }
    }

    public new IEnumerable<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        throw new NotSupportedException(
            "DynamicArray does not store data locally, use with TigerReader or change to DynamicArray");
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
        get => throw new NotSupportedException(
            "DynamicArray does not store data locally, use with TigerReader or change to DynamicArray");
        set => throw new NotSupportedException("DynamicArray is read-only");
    }

    public void ForEach(Action<T> action)
    {
        throw new NotSupportedException(
            "DynamicArray does not store data locally, use with TigerReader or change to DynamicArray");
    }

    public new Enumerator GetEnumerator()
    {
        throw new NotSupportedException(
            "DynamicArray does not store data locally, use with TigerReader or change to DynamicArray");
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

    public new T ElementAt(int index)
    {
        throw new NotSupportedException(
            "DynamicArray does not store data locally, use with TigerReader or change to DynamicArray");
    }

    public T ElementAt(TigerReader reader, int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new IndexOutOfRangeException($"Index ({index}) out of range ({reader.Hash:X}, Position {reader.Position:X})");
        }

        return ReadElement(reader, index);
    }

    protected T ReadElement(TigerReader reader, int index)
    {
        // todo consolidate with SchemaDeserializer
        reader.Seek(Offset.AbsoluteOffset + index * _elementSize, SeekOrigin.Begin);
        if (typeof(T).IsValueType) // if T is a struct
        {
            return reader.ReadSchemaStruct(typeof(T));
        }
        else if (SchemaDeserializer.Get().IsTigerDeserializeType(typeof(T)))
        {
            return (T)reader.ReadSchemaType(typeof(T));
        }
        else if (SchemaDeserializer.Get().IsTigerFileType(typeof(T)))
        {
            FileHash fileHash = new(reader.ReadUInt32());
            return FileResourcer.Get().GetFile(typeof(T), fileHash);
        }
        else
        {
            throw new NotSupportedException($"T must be a struct or a type that implements ITigerDeserialize ({reader.Hash:X})");
        }
    }

    // todo interpolation search
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

/// <summary>
/// Assume the first field of T is a uint32 sort key.
/// This allows for binary searching.
/// We actually use interpolation searching as its more efficent since hashes are uniformly distributed.
/// </summary>
public class SortedDynamicArray<T> : DynamicArrayUnloaded<T> where T : struct
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

        if (searchValue == lowValue)
        {
            return lowIndex;
        }
        else if (searchValue == highValue)
        {
            return highIndex;
        }

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
        try
        {
            _basePosition = reader.BaseStream.Position;
            _relativeOffset = reader.ReadInt64();
        }
        catch (Exception ex)
        {
            throw new Exception($"[{reader.Hash:X2}] Failed to read at position {reader.BaseStream.Position:X}. Stream length: {reader.BaseStream.Length}", ex);
        }
    }

    public void AddExtraOffset(int extraOffset)
    {
        _extraOffset += extraOffset;
    }
}

/// <summary>
/// Represents a pointer to a globally located structure within the file.
/// Reads an absolute offset and, if non-zero, seeks to that position to deserialize a value of type <typeparamref name="T"/>.
/// </summary>
[SchemaType(0x04)]
public class GlobalPointer<T> : ITigerDeserialize where T : struct
{
    public int AbsoluteOffset;
    public T Value;

    public virtual void Deserialize(TigerReader reader)
    {
        try
        {
            AbsoluteOffset = reader.ReadInt32();
            if (AbsoluteOffset == 0)
            {
                return;
            }
            reader.Seek(AbsoluteOffset, SeekOrigin.Begin);
            Value = reader.ReadSchemaStruct<T>();
        }
        catch (Exception ex)
        {
            throw new Exception($"[{reader.Hash:X2}] Failed to read at position {reader.BaseStream.Position:X}. Stream length: {reader.BaseStream.Length}", ex);
        }
    }
}

/// <summary>
/// A relative pointer that points to a resource, and the class of resource is specified
/// </summary>
public class ResourcePointer : RelativePointer
{
    // public dynamic? Value
    public uint ResourceClassHash = TigerHash.InvalidHash32;

    // not ideal, but it stops the recursive deserialization. in future should make some kind of ref system
    // and use that instead
    public dynamic? GetValue(TigerReader reader, bool deserialize = true)
    {
        try
        {
            if (ResourceClassHash == TigerHash.InvalidHash32)
            {
                return null;
            }
            if (SchemaDeserializer.Get().TryGetSchemaType(ResourceClassHash, out Type schemaType))
            {
                if (deserialize)
                {
                    reader.Seek(AbsoluteOffset, SeekOrigin.Begin);
                    return reader.ReadSchemaStruct(schemaType);
                }
                else
                    return Activator.CreateInstance(schemaType);
            }
            else
            {
                // Log.Debug($"Unknown resource class hash {ResourceClassHash:X8}");
                return null;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"[{reader.Hash:X2}] Failed to read at position {reader.BaseStream.Position:X}. Stream length: {reader.BaseStream.Length}", ex);
        }
    }

    public uint GetValueRaw(TigerReader reader)
    {
        return ResourceClassHash;
    }

    public override void Deserialize(TigerReader reader)
    {
        try
        {
            base.Deserialize(reader);

            if (_relativeOffset == 0)
                return;

            reader.Seek(AbsoluteOffset - 4, SeekOrigin.Begin);
            ResourceClassHash = reader.ReadUInt32();
            if (ResourceClassHash == 0)
                ResourceClassHash = TigerHash.InvalidHash32;
        }
        catch (Exception ex)
        {
            throw new Exception($"[{reader.Hash:X2}] Failed to read at position {reader.BaseStream.Position:X}. Stream length: {reader.BaseStream.Length}", ex);
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
        if (localizedStrings == null || stringHash.IsInvalid())
        {
            return;
        }
        Value = localizedStrings.GetStringFromHash(stringHash);
    }

    public static implicit operator string(StringReference stringReference) => stringReference.Value.ToString();
}

/// <summary>
/// References a 32-bit index, pointing to a <see cref="Tiger.Schema.LocalizedStrings"/>, and a 32-bit string hash.
/// </summary>
[SchemaType(0x08)]
public class StringIndexReference : ITigerDeserialize
{
    public TigerString? Value;

    public void Deserialize(TigerReader reader)
    {
        int index = reader.ReadInt32();
        if (index == 0xFF_FF)
        {
            return;
        }

        LocalizedStrings localizedStrings = Investment.Get().GetLocalizedStringsFromIndex(index);
        StringHash stringHash = new(reader.ReadUInt32());

        if (localizedStrings is null)
            Value = new TigerString($"NotFound-{stringHash}");
        else
            Value = localizedStrings.GetStringFromHash(stringHash);
    }

    public static implicit operator string(StringIndexReference stringIndexReference) => stringIndexReference.Value.ToString();
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
        if (localizedStrings == null || stringHash.IsInvalid())
        {
            return;
        }
        Value = localizedStrings.GetStringFromHash(stringHash);
    }

    public static implicit operator string(StringReference64 stringReference64) => stringReference64.Value.ToString();
}

/// <summary>
/// A string that ends when a null character is reached
/// </summary>
//public class StringNullTerminated : ITigerDeserialize
//{
//    public string? Value;

//    public void Deserialize(TigerReader reader)
//    {
//        StringBuilder sb = new();
//        while (true)
//        {
//            char c = reader.ReadChar();
//            if (c == '\0')
//            {
//                break;
//            }
//            sb.Append(c);
//        }
//        Value = sb.ToString();
//    }

//    public static implicit operator string?(StringNullTerminated stringNullTerminated) => stringNullTerminated.Value;
//}

/// <summary>
///  A pointer to a resource in a specified table (has a constant type)
/// </summary>
[SchemaType(0x08)]
public class ResourceInTablePointer<T> : ITigerDeserialize where T : struct
{
    public T? Value;

    public void Deserialize(TigerReader reader)
    {
        long resourcePointer = reader.ReadInt64();
        if (resourcePointer == 0)
        {
            Value = null;
            return;
        }

        reader.Seek(resourcePointer - 8, SeekOrigin.Current);
        Value = reader.ReadSchemaStruct<T>();
    }
}

/// <summary>
/// An inline struct used when no class hash is provided, but the structure layout is known and consistent.
/// Useful for blocks of repeating data where something like <see cref="DynamicArray{T}"/> wouldn't apply.
/// <see cref="Tiger.Schema.SMaterial"/> is one such example, where each shader stage (Vertex, Pixel, Compute)
/// share the same data layout.
/// </summary>
[SchemaType(0x0)]
public class DynamicStruct<T> : ITigerDeserialize where T : struct
{
    public T Value;

    public void Deserialize(TigerReader reader)
    {
        Value = reader.ReadSchemaStruct<T>();
    }
}

/// <summary>
/// A pointer to a resource in a specified tag, currently does nothing
/// </summary>
[SchemaType(0x10)]
public class ResourceInTagPointer : ITigerDeserialize
{
    public void Deserialize(TigerReader reader)
    {
        // Method intentionally left empty.
    }
}

/// <summary>
/// A weird version of <see cref="ResourceInTagWeirdPointer"/>, currently does nothing
/// </summary>
[SchemaType(0x18)]
public class ResourceInTagWeirdPointer : ITigerDeserialize
{
    public void Deserialize(TigerReader reader)
    {
        // Method intentionally left empty.
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
        reader.Seek(_basePosition + 8, SeekOrigin.Begin);
        uint extraResourceClassHash = reader.ReadUInt32();
        if (ResourceClassHash == 0)
        {
            ResourceClassHash = extraResourceClassHash;
        }
        Debug.Assert(extraResourceClassHash == ResourceClassHash);
    }
}

public interface ISchema
{
}
