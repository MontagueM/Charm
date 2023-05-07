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

    public T ReadSchemaType<T>() where T : ITigerDeserialize, new()
    {
        var t = new T();
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
public class DynamicArray<T> : List<T>, ITigerDeserialize where T : struct
{
    public int Count;
    public RelativePointer Offset;
    // private byte[] _arrayData;
    protected int _elementSize;

    // todo verify that T is valid for the hash we get given by checking SchemaStruct against hash defined for array

    public void Deserialize(TigerReader reader)
    {
        Count = reader.ReadInt32();
        reader.Seek(4, SeekOrigin.Current);
        Offset = reader.ReadSchemaType<RelativePointer>();
        Offset.AddExtraOffset(0x10);
        reader.Seek(4, SeekOrigin.Current);

        _elementSize = SchemaDeserializer.Get().GetSchemaStructSize<T>();
        // _arrayData = reader.ReadBytes(Count * _elementSize);
    }

    public IEnumerable<T> Enumerate(TigerReader reader)
    {
        // TigerReader reader = new TigerReader(_arrayData);
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

    // public new T this[int index]
    // {
    //     get => ElementAt(index);
    //     set => throw new NotSupportedException("DynamicArray is read-only");
    // }

    public T ElementAt(TigerReader reader, int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new IndexOutOfRangeException("Index out of range");
        }

        // TigerReader reader = new TigerReader(_arrayData);
        return ReadElement(reader, index);
    }

    private T ReadElement(TigerReader reader, int index)
    {
        reader.Seek(Offset.AbsoluteOffset + index * _elementSize, SeekOrigin.Begin);
        return reader.ReadSchemaStruct<T>();
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

    private long _basePosition;
    private long _relativeOffset;
    private long _extraOffset;

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
