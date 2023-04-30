using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Arithmic;
using Newtonsoft.Json;

namespace Tiger;

public class PackagePathsCache
{
    private readonly ConcurrentDictionary<ushort, string> PackageIdToPathMap = new();
    private PackagePathCacheData _cacheData;
    private readonly string _cacheFilePath;
    private readonly string _packagesDirectory;
    private const byte CacheVersion = 1;
    private static readonly string CacheDirectory = "./PackagePathsCaches";

    struct PackagePathCacheData
    {
        public PackagePathCacheVersion Version;
        public string PackageDirectory;
        public Dictionary<ushort, string> PackageIdToNameMap;
    }

    struct PackagePathCacheVersion
    {
        public byte CacheVersion;
        public uint GameVersionHash;
    }

    public static void ClearCacheFiles()
    {
        Log.Info("Clearing package paths cache files.");
        foreach (string cacheFilePath in Directory.GetFiles(CacheDirectory))
        {
            File.Delete(cacheFilePath);
        }
    }

    /// <summary>
    /// 'strategy' is used to name the cache file.
    /// the code here is no where near as optimal as it could be, but it's not a bottleneck + json is easier to read
    /// </summary>
    public PackagePathsCache(TigerStrategy strategy)
    {
        string packagesDirectory = strategy.GetStrategyConfiguration().PackagesDirectory;
        Strategy.CheckValidPackagesDirectory(packagesDirectory);
        _packagesDirectory = packagesDirectory;
        _cacheFilePath = MakeCacheFileName(strategy);
        FillCache();
    }

    private string MakeCacheFileName(TigerStrategy strategy)
    {
        return Path.Join(CacheDirectory, $"{strategy}_cache.json");
    }

    private void FillCache()
    {
        if (IsCacheFileInvalid())
        {
            Log.Info($"Cache file is invalid, creating new from packages directory {_packagesDirectory}.");
            SaveCacheToFile();
        }

        Log.Info($"Loading package paths cache from cache file {_cacheFilePath}.");
        FillCacheFromCacheFile();
    }

    private bool IsCacheFileInvalid()
    {
        if (!File.Exists(_cacheFilePath))
        {
            Log.Info($"Cache file '{_cacheFilePath}' does not exist.");
            return true;
        }

        _cacheData = JsonConvert.DeserializeObject<PackagePathCacheData>(File.ReadAllText(_cacheFilePath));

        if (CacheVersion != _cacheData.Version.CacheVersion)
        {
            Log.Info($"Cache file '{_cacheFilePath}' has old version '{_cacheData.Version.CacheVersion}' (current is {CacheVersion}).");
            return true;
        }

        uint gameVersionHash = GetGameVersionHash();
        if (_cacheData.Version.GameVersionHash != gameVersionHash)
        {
            Log.Info($"Cache file '{_cacheFilePath}' has old game version hash '{_cacheData.Version.GameVersionHash}' (current is {gameVersionHash}.");
            return true;
        }

        int currentPackageCount = Directory.GetFiles(_packagesDirectory, "*.pkg", SearchOption.TopDirectoryOnly).Length;
        if (_cacheData.PackageIdToNameMap.Count != currentPackageCount)
        {
            Log.Info($"Cache file '{_cacheFilePath}' has different number of packages '{_cacheData.PackageIdToNameMap.Count}' (current is {currentPackageCount}.");
            return true;
        }

        return false;
    }

    /// <summary>
    /// If we can't get a game version, presume its static and set it to 0.
    /// </summary>
    private uint GetGameVersionHash()
    {
        var path = _packagesDirectory.Split("packages")[0] + "destiny2.exe";
        if (!File.Exists(path))
        {
            Log.Warning($"Could not get find game executable '{path}' for game version, assuming static.");
            return 0;
        }
        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
        string? version = versionInfo.FileVersion;
        if (version == null)
        {
            Log.Warning($"Could not find game version of executable '{path}', assuming static.");
            return 0;
        }
        byte[] encoded = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(version));
        var value = BitConverter.ToUInt32(encoded, 0);
        return value;
    }

    private void FillCacheFromCacheFile()
    {
        PackagePathCacheData data = JsonConvert.DeserializeObject<PackagePathCacheData>(File.ReadAllText(_cacheFilePath));
        foreach (KeyValuePair<ushort, string> entry in data.PackageIdToNameMap)
        {
            PackageIdToPathMap.TryAdd(entry.Key, Path.Combine(data.PackageDirectory, entry.Value));
        }
    }

    private void SaveCacheToFile()
    {
        string? cacheFileDirectory = Path.GetDirectoryName(_cacheFilePath);
        if (cacheFileDirectory != null && !Directory.Exists(cacheFileDirectory))
        {
            Directory.CreateDirectory(cacheFileDirectory);
        }

        PackagePathCacheData cacheData = new()
        {
            Version = new PackagePathCacheVersion
            {
                CacheVersion = CacheVersion,
                GameVersionHash = GetGameVersionHash()
            },
            PackageDirectory = _packagesDirectory,
            PackageIdToNameMap = GetPackagePathCacheEntries()
        };

        string jsonString = JsonConvert.SerializeObject(cacheData);
        File.WriteAllText(_cacheFilePath, jsonString);
    }

    /// <summary>
    /// Only gets the highest patch version of each package.
    /// </summary>
    private Dictionary<ushort, string> GetPackagePathCacheEntries()
    {
        Dictionary<ushort, string> highestName = new Dictionary<ushort, string>();
        Dictionary<int, int> highestPatch = new Dictionary<int, int>();

        foreach (string file in Directory.GetFiles(_packagesDirectory, "*.pkg", SearchOption.TopDirectoryOnly))
        {
            IPackage package = PackageResourcer.Get().GetPackage(file);
            PackageMetadata packageMetadata = package.GetPackageMetadata();
            if (!highestPatch.ContainsKey(packageMetadata.Id))
            {
                highestName.Add(packageMetadata.Id, Path.GetFileName(file));
                highestPatch.Add(packageMetadata.Id, packageMetadata.PatchId);
            }
            else
            {
                if (packageMetadata.PatchId > highestPatch[packageMetadata.Id])
                {
                    highestName[packageMetadata.Id] = Path.GetFileName(file);
                    highestPatch[packageMetadata.Id] = packageMetadata.PatchId;
                }
            }
        }

        return highestName;
    }

    public string GetPackagePathFromId(ushort packageId)
    {
        if (PackageIdToPathMap.TryGetValue(packageId, out string packagePath))
        {
            return packagePath;
        }
        throw new ArgumentException($"The package id '{packageId}' is not in the package paths cache");
    }

    private static readonly string PackageStringNotInPackagePathsCacheMessage = "The package string is not in the package paths cache: ";
    public ushort GetPackageIdFromPath(string packagePath)
    {
        // This search is not performant but will be rarely used
        ushort packageId = PackageIdToPathMap.FirstOrDefault(x => x.Value == packagePath).Key;
        if (packageId != default(ushort))
        {
            return packageId;
        }
        throw new ArgumentException(PackageStringNotInPackagePathsCacheMessage + packageId);
    }

    public List<ushort> GetPackageIds()
    {
        return PackageIdToPathMap.Keys.ToList();
    }
}
