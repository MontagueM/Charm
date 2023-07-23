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
        return GetFile(typeof(FileHash), fileHash, shouldLoad);
    }

    public dynamic? GetFile(Type type, FileHash hash, bool shouldLoad = true)
    {
        if (!hash.IsValid())
        {
            return null;
        }

        if (_fileCache.ContainsKey(hash.Hash32))
        {
            // checks that the type of the cached file is the same as the type we're looking for
            if (_fileCache[hash.Hash32].GetType() == type)
            {
                return _fileCache[hash.Hash32];
            }

            // want a Tag<T>
            if (type.IsValueType)
            {
                if (_fileCache[hash.Hash32].GetType().GenericTypeArguments.Length > 0 && _fileCache[hash.Hash32].GetType().GenericTypeArguments[0].UnderlyingSystemType == type)
                {
                    return _fileCache[hash.Hash32];
                }
            }

            _fileCache.TryRemove(hash.Hash32, out dynamic? r);
        }

        // want a Tag<T>
        if (type.IsValueType)
        {
            type = typeof(Tag<>).MakeGenericType(type);
        }

        dynamic? file;
        if (!shouldLoad)
        {
            file = Activator.CreateInstance(type, hash, shouldLoad);
        }
        else
        {
            file = Activator.CreateInstance(type, hash);
        }

        _fileCache.TryAdd(hash.Hash32, file);
        return file;
    }

    public async Task<T> GetFileAsync<T>(TigerHash hash) where T : TigerFile
    {
        return await Task.Run(() => GetFile<T>(hash));
    }
}
