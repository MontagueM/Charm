using System.Collections.Concurrent;
using ConcurrentCollections;

namespace Tiger;

/// <summary>
/// Provides an interface for getting IPackage objects from a directory of packages.
/// There is one PackageResourcer per game.
/// </summary>
public class PackageResourcer : Strategy.StrategistSingleton<PackageResourcer>
{
    private PackagePathsCache? _packagePathsCache;

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
    }

    protected override void Reset()
    {
        _packagesCache.Clear();
        _packagePathsCache = new PackagePathsCache(_strategy);
    }

    /// <summary>
    /// Gets a package which represents a .pkg disk file. Blocking call.
    /// </summary>
    /// <returns>IPackage object, type determined by the selected strategy.</returns>
    public IPackage GetPackage(ushort packageId)
    {
        if (_packagesCache.TryGetValue(packageId, out Package package))
        {
            return package;
        }

        return LoadPackageIntoCacheFromDisk(packageId);
    }

    // // this method is used by PackagePathsCache, so cannot use itself
    public IPackage GetPackage(string packagePath)
    {
        // Don't add to cache as we're getting multiple packages from the same id, in order to identify their patch.
        return LoadPackageNoCacheFromDisk(packagePath);
    }

    // todo this needs to be a producer-consumer style queue thing to avoid locking maybe
    // could try it this way first then compare performance with a queue approach

    private IPackage LoadPackageIntoCacheFromDisk(ushort packageId)
    {
        string packagePath = PackagePathsCache.GetPackagePathFromId(packageId);
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
            throw new Exception($"Failed to add package to package cache: '{packageId}', '{packagePath}'");
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

    public byte[] GetFileData(FileHash fileHash) { return GetPackage(fileHash.PackageId).GetFileBytes(fileHash); }

    private void LoadAllPackages()
    {
        List<ushort> packageIds = PackagePathsCache.GetAllPackageIds();
        Parallel.ForEach(packageIds, packageId => GetPackage(packageId));
    }

    public HashSet<T> GetAllFiles<T>() where T : TigerFile
    {
        PackagePathsCache.GetAllPackageIds();
        HashSet<T> tags = new();
        foreach (Package package in _packagesCache.Values)
        {
            package.GetAllFiles<T>().ForEach(t => tags.Add(t));
        }
        return tags;
    }

    public HashSet<TigerFile> GetAllFiles(Type fileType)
    {
        PackagePathsCache.GetAllPackageIds();
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

    public Task<ConcurrentHashSet<FileHash>> GetAllHashes<T>()
    {
        return GetAllHashes(typeof(T));
    }

    public async Task<ConcurrentHashSet<FileHash>> GetAllHashes(Type schemaType)
    {
        PackagePathsCache.GetAllPackageIds();
        ConcurrentHashSet<FileHash> fileHashes = new();

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 16, CancellationToken = CancellationToken.None };
        await Parallel.ForEachAsync(_packagesCache.Values, parallelOptions, async (package, ct) =>
        {
            fileHashes.UnionWith(await Task.Run(() => package.GetAllHashes(schemaType), ct));
        });

        return fileHashes;
    }

    public async Task<ConcurrentHashSet<FileHash>> GetAllHashes(Func<string, bool> packageFilterFunc)
    {
        PackagePathsCache.GetAllPackageIds();
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
        return "todo get activity name";
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
