using System.Collections.Concurrent;

namespace Tiger;

public class FileResourcer : Strategy.StrategistSingleton<FileResourcer>
{
    private readonly ConcurrentDictionary<uint, dynamic> _fileCache = new();

    public FileResourcer(TigerStrategy strategy, StrategyConfiguration strategyConfiguration) : base(strategy)
    {
    }

    protected override void Initialise()
    {
    }

    protected override void Reset()
    {
        _fileCache.Clear();
    }

    public T GetFile<T>(string fileHash, bool shouldLoad = true) where T : TigerFile
    {
        return GetFile<T>(new FileHash(fileHash), shouldLoad);
    }

    public T GetFile<T>(FileHash fileHash, bool shouldLoad = true) where T : TigerFile
    {
        return GetFile(typeof(T), fileHash, shouldLoad);
    }

    public T GetFileInterface<T>(string fileHash, bool shouldLoad = true) where T : ISchema
    {
        return GetFileInterface<T>(new FileHash(fileHash), shouldLoad);
    }

    public T GetFileInterface<T>(FileHash fileHash, bool shouldLoad = true) where T : ISchema
    {
        return GetFile(SchemaDeserializer.Get().GetSchemaInterfaceType(typeof(T)), fileHash, shouldLoad);
    }

    public Tag<T> GetSchemaTag<T>(string fileHash, bool shouldLoad = true) where T : struct
    {
        return GetSchemaTag<T>(new FileHash(fileHash), shouldLoad);
    }

    public Tag<T> GetSchemaTag<T>(FileHash fileHash, bool shouldLoad = true) where T : struct
    {
        return GetFile(typeof(T), fileHash, shouldLoad);
    }

    public TigerFile GetFile(string fileHash, bool shouldLoad = true)
    {
        return GetFile(new FileHash(fileHash), shouldLoad);
    }

    public TigerFile GetFile(FileHash fileHash, bool shouldLoad = true)
    {
        return GetFile<TigerFile>(fileHash, shouldLoad);
    }

    public dynamic? GetFile(Type type, FileHash hash, bool shouldLoad = true)
    {
        if (!hash.IsValid())
        {
            return null;
        }

        if (_fileCache.TryGetValue(hash.Hash32, out dynamic? cachedFile))
        {
            // checks that the type of the cached file is the same as the type we're looking for
            if (cachedFile.GetType() == type)
            {
                return cachedFile;
            }

            // want a Tag<T>
            if (type.IsValueType)
            {
                if (cachedFile.GetType().GenericTypeArguments.Length > 0 && cachedFile.GetType().GenericTypeArguments[0].UnderlyingSystemType == type)
                {
                    return cachedFile;
                }
            }

            // todo causes thread safety issues
            // _fileCache.TryRemove(hash.Hash32, out dynamic? r);
        }

        // want a Tag<T>
        if (type.IsValueType)
        {
            type = typeof(Tag<>).MakeGenericType(type);
        }

        dynamic? file;
        if (type.GetConstructor(new[] { typeof(FileHash), typeof(bool) }) != null)
        {
            file = Activator.CreateInstance(type, hash, shouldLoad);
        }
        else if (type.GetConstructor(new[] { typeof(FileHash) }) != null)
        {
            file = Activator.CreateInstance(type, hash);
        }
        else
        {
            throw new Exception($"Invalid constructor for {type} with hash {hash}");
        }

        _fileCache.TryAdd(hash.Hash32, file);
        return file;
    }

    public async Task<T> GetFileAsync<T>(TigerHash hash) where T : TigerFile
    {
        return await Task.Run(() => GetFile<T>(hash));
    }
}
