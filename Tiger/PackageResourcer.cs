using System.Collections.Concurrent;
using System.Globalization;
using Arithmic;
using ConcurrentCollections;
using Tiger.Schema;

namespace Tiger;

/// <summary>
/// Provides an interface for getting IPackage objects from a directory of packages.
/// There is one PackageResourcer per game.
/// </summary>
[InitializeAfter(typeof(SchemaDeserializer))]
public class PackageResourcer : Strategy.StrategistSingleton<PackageResourcer>
{
    private PackagePathsCache? _packagePathsCache;
    private Dictionary<uint, string> _activityNames = new();
    private Dictionary<FileHash, TagClassHash> _d1NamedTags = new();
    private Dictionary<ulong, Dictionary<byte[], byte[]>> _keys = new();

    public PackagePathsCache PackagePathsCache
    {
        get
        {
            if (_packagePathsCache == null)
            {
                throw new NullReferenceException("PackagePathsCache has not been initialized.");
            }
            return _packagePathsCache;
        }
    }

    public Dictionary<ulong, Dictionary<byte[], byte[]>> Keys
    {
        get
        {
            if (_keys == null)
            {
                throw new NullReferenceException("Package Keys have not been loaded");
            }
            return _keys;
        }
    }

    private readonly ConcurrentDictionary<ushort, Package> _packagesCache = new();
    public string PackagesDirectory { get; }

    public PackageResourcer(TigerStrategy strategy, StrategyConfiguration strategyConfiguration) : base(strategy)
    {
        PackagesDirectory = strategyConfiguration.PackagesDirectory;
    }

    protected override void Initialise()
    {
        _packagePathsCache = new PackagePathsCache(_strategy);
        LoadAllPackages();
        LoadPackageKeys();
        CacheAllActivityNames();

        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
            CacheAllD1NamedTags();
    }

    protected override void Reset()
    {
        _packagesCache.Clear();
        _packagePathsCache = new PackagePathsCache(_strategy);
    }

    /// <summary>
    /// Loads keys for redacted packages from the keys.txt (if it exists) in the Charm folder 
    /// </summary>
    public void LoadPackageKeys()
    {
        if (!File.Exists("./keys.txt"))
            return;

        string[] txt = File.ReadAllLines("./keys.txt");
        foreach (var entry in txt)
        {
            try
            {
                // Split the entry by ':' and trim any whitespace
                var parts = entry.Split(':');
                if (parts.Length < 3)
                {
                    Log.Error($"Invalid key entry format: {entry}");
                    continue;
                }

                var pkgGroup = ulong.Parse(parts[0].Trim(), NumberStyles.HexNumber);
                var key = Helpers.HexStringToByteArray(parts[1].Trim());
                var nonce = Helpers.HexStringToByteArray(parts[2].Split("//")[0].Trim());

                if (!_keys.ContainsKey(pkgGroup))
                    _keys[pkgGroup] = new Dictionary<byte[], byte[]>();

                var keyDict = _keys[pkgGroup];
                if (!keyDict.ContainsKey(key))
                    keyDict[key] = nonce;
                else
                    Log.Error($"Duplicate key for package group {pkgGroup:X}: {entry}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing entry: {entry}\n{ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets a package from a given ID which represents a .pkg disk file. Blocking call.
    /// </summary>
    /// <returns>IPackage object, type determined by the selected strategy.</returns>
    public Package GetPackage(ushort packageId)
    {
        if (_packagesCache.TryGetValue(packageId, out Package package))
        {
            return package;
        }

        return LoadPackageIntoCacheFromDisk(packageId);
    }

    /// <summary>
    /// Gets a package from a given FileHash which represents a .pkg disk file. Blocking call.
    /// </summary>
    /// <returns>IPackage object, type determined by the selected strategy.</returns>
    public Package GetPackage(FileHash hash)
    {
        if (_packagesCache.TryGetValue(hash.PackageId, out Package package))
        {
            return package;
        }

        return LoadPackageIntoCacheFromDisk(hash.PackageId, hash);
    }

    // // this method is used by PackagePathsCache, so cannot use itself
    public Package GetPackage(string packagePath)
    {
        // Don't add to cache as we're getting multiple packages from the same id, in order to identify their patch.
        return LoadPackageNoCacheFromDisk(packagePath);
    }

    // todo this needs to be a producer-consumer style queue thing to avoid locking maybe
    // could try it this way first then compare performance with a queue approach

    private Package LoadPackageIntoCacheFromDisk(ushort packageId, FileHash? hash = null)
    {
        string packagePath = PackagePathsCache.GetPackagePathFromId(packageId, hash);
        return LoadPackageIntoCacheFromDisk(packageId, packagePath);
    }

    /// <summary>
    /// Creates an IPackage of the type determined by the selected strategy, and adds it to the package cache.
    /// </summary>
    /// <exception cref="Exception">Package is null or failed to add to concurrent dictionary as already added.</exception>
    private Package LoadPackageIntoCacheFromDisk(ushort packageId, string packagePath)
    {
        Package? package = (Package?)Activator.CreateInstance(_strategy.GetPackageType(), packagePath);

        if (package == null || !_packagesCache.TryAdd(packageId, package))
        {
            Log.Verbose($"Failed to add package to package cache: '{packageId}', '{packagePath}'");
        }
        return package;
    }

    private Package LoadPackageNoCacheFromDisk(string packagePath)
    {
        Package? package = (Package?)Activator.CreateInstance(_strategy.GetPackageType(), packagePath);
        if (package == null)
        {
            throw new Exception($"Failed to get package: '{packagePath}'");
        }
        return package;
    }

    public byte[] GetFileData(FileHash fileHash) { return GetPackage(fileHash).GetFileBytes(fileHash); }

    private void LoadAllPackages()
    {
        List<ushort> packageIds = PackagePathsCache.GetAllPackageIds();
        Parallel.ForEach(packageIds, packageId => GetPackage(packageId));
    }

    public HashSet<T> GetAllFiles<T>() where T : TigerFile
    {
        HashSet<T> tags = new();
        foreach (Package package in _packagesCache.Values)
        {
            package.GetAllFiles<T>().ForEach(t => tags.Add(t));
        }
        return tags;
    }

    public HashSet<TigerFile> GetAllFiles(Type fileType)
    {
        HashSet<TigerFile> tags = new();
        foreach (Package package in _packagesCache.Values)
        {
            package.GetAllFiles(fileType).ForEach(t => tags.Add(t));
        }
        return tags;
    }

    public FileMetadata GetFileMetadata(FileHash fileHash)
    {
        return GetPackage(fileHash.PackageId).GetFileMetadata(fileHash);
    }

    public Task<ConcurrentHashSet<FileHash>> GetAllHashesAsync<T>()
    {
        return GetAllHashesAsync(typeof(T));
    }

    public async Task<ConcurrentHashSet<FileHash>> GetAllHashesAsync(Type schemaType)
    {
        ConcurrentHashSet<FileHash> fileHashes = new();

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 16, CancellationToken = CancellationToken.None };
        await Parallel.ForEachAsync(_packagesCache.Values, parallelOptions, async (package, ct) =>
        {
            fileHashes.UnionWith(await Task.Run(() => package.GetAllHashes(schemaType), ct));
        });

        return fileHashes;
    }

    public ConcurrentHashSet<FileHash> GetAllHashes<T>()
    {
        return GetAllHashes(typeof(T));
    }

    public ConcurrentHashSet<FileHash> GetAllHashes(Type schemaType)
    {
        ConcurrentHashSet<FileHash> fileHashes = new();

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 5, CancellationToken = CancellationToken.None };
        Parallel.ForEach(_packagesCache.Values, parallelOptions, (package) =>
        {
            fileHashes.UnionWith(package.GetAllHashes(schemaType));
        });

        return fileHashes;
    }

    public ConcurrentHashSet<FileHash> GetAllHashes(Func<string, bool> packageFilterFunc)
    {
        ConcurrentHashSet<FileHash> fileHashes = new();

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 5, CancellationToken = CancellationToken.None };
        IEnumerable<Package> packages = _packagesCache.Values.Where(package => packageFilterFunc(package.PackagePath));
        Parallel.ForEach(packages, parallelOptions, (package) =>
        {
            fileHashes.UnionWith(package.GetAllHashes());
        });

        return fileHashes;
    }

    public ConcurrentHashSet<FileHash> GetAllHashes<T>(Func<string, bool> packageFilterFunc)
    {
        ConcurrentHashSet<FileHash> fileHashes = new();

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 5, CancellationToken = CancellationToken.None };
        IEnumerable<Package> packages = _packagesCache.Values.Where(package => packageFilterFunc(package.PackagePath));
        Parallel.ForEach(packages, parallelOptions, (package) =>
        {
            fileHashes.UnionWith(package.GetAllHashes(typeof(T)));
        });

        return fileHashes;
    }

    public async Task<ConcurrentHashSet<FileHash>> GetAllHashesAsync(Func<string, bool> packageFilterFunc)
    {
        ConcurrentHashSet<FileHash> fileHashes = new();

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 5, CancellationToken = CancellationToken.None };
        IEnumerable<Package> packages = _packagesCache.Values.Where(package => packageFilterFunc(package.PackagePath));
        await Parallel.ForEachAsync(packages, parallelOptions, async (package, ct) =>
        {
            fileHashes.UnionWith(await Task.Run(package.GetAllHashes, ct));
        });

        return fileHashes;
    }

    public string GetActivityName(FileHash fileHash)
    {
        if (_activityNames.TryGetValue(fileHash.Hash32, out string activityName))
        {
            return activityName;
        }
        return "Activity name not found.";
    }

    public FileHash GetNamedTag(string name)
    {
        if (_activityNames.ContainsValue(name))
        {
            return new FileHash(_activityNames.First(x => x.Value == name).Key);
        }
        throw new NullReferenceException($"Can not find Named Tag '{name}'");
    }

    public Dictionary<FileHash, TagClassHash> GetD1Activities()
    {
        return _d1NamedTags;
    }

    private void CacheAllActivityNames()
    {
        ConcurrentHashSet<PackageActivityEntry> activityEntries = new();

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 5, CancellationToken = CancellationToken.None };
        Parallel.ForEach(_packagesCache.Values, parallelOptions, async (package, ct) =>
        {
            activityEntries.UnionWith(package.GetAllActivities());
        });

        _activityNames = new Dictionary<uint, string>();

        foreach (PackageActivityEntry entry in activityEntries)
        {
            if (_activityNames.ContainsKey(entry.TagHash.Hash32))
            {
                if (entry.Name.Length > _activityNames[entry.TagHash.Hash32].Length)
                {
                    _activityNames[entry.TagHash.Hash32] = entry.Name;
                }
            }
            else
            {
                _activityNames.Add(entry.TagHash.Hash32, entry.Name);
            }
        }
    }

    private async void CacheAllD1NamedTags()
    {
        ConcurrentHashSet<PackageActivityEntry> activityEntries = new();

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 5, CancellationToken = CancellationToken.None };
        await Parallel.ForEachAsync(_packagesCache.Values, parallelOptions, async (package, ct) =>
        {
            activityEntries.UnionWith(package.GetAllActivities());
        });

        _d1NamedTags = new();

        foreach (PackageActivityEntry entry in activityEntries)
        {
            _d1NamedTags.TryAdd(entry.TagHash, entry.TagClassHash);
        }
    }
}

public static class ConcurrentHashSetExtensions
{
    public static void UnionWith<T>(this ConcurrentHashSet<T> set, IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            set.Add(item);
        }
    }
}
