using System.Collections.Concurrent;
using System.Diagnostics;
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
    private readonly TigerStrategy _strategy;
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
        _strategy = strategy;
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
            // Writing directly to console since Log.Info gets held up here, but still want the info in the log file
            string log = $"Cache file is invalid, creating new cache from packages directory '{_packagesDirectory}'. This may take a few seconds.";
            Log.Console(log);
            Log.Info(log);

            if (File.Exists("./EntityNames.json")) // Surely this is fine
            {
                NamedEntities Ents;
                try
                {
                    Ents = JsonConvert.DeserializeObject<NamedEntities>(File.ReadAllText($"./EntityNames.json"));
                    if (Ents.EntityNames.ContainsKey(_strategy))
                        Ents.EntityNames[_strategy].Clear();

                    File.WriteAllText($"./EntityNames.json", JsonConvert.SerializeObject(Ents, Formatting.Indented));
                }
                catch (JsonSerializationException) // Likely old version of the json
                {
                    File.Delete($"./EntityNames.json");
                }
            }
            SaveCacheToFile();
        }

        Log.Info($"Loading package paths cache from cache file '{_cacheFilePath}'.");
        FillCacheFromCacheFile();
    }

    private bool IsCacheFileInvalid()
    {
        if (!File.Exists(_cacheFilePath))
        {
            Log.Info($"Cache file '{_cacheFilePath}' does not exist.");
            return true;
        }

        try
        {
            _cacheData = JsonConvert.DeserializeObject<PackagePathCacheData>(File.ReadAllText(_cacheFilePath));
        }
        catch (JsonException e)
        {
            Log.Info($"Cache file '{_cacheFilePath}' could not be read: {e.Message}");
            return true;
        }

        if (_cacheData.PackageIdToNameMap == null || _cacheData.PackageDirectory == null)
        {
            Log.Info($"Cache file '{_cacheFilePath}' is incomplete.");
            return true;
        }

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

        // The cache file is named per strategy, so it has to be rebuilt if that strategy now points at a different install
        if (!IsSameDirectory(_cacheData.PackageDirectory, _packagesDirectory))
        {
            Log.Info($"Cache file '{_cacheFilePath}' is for packages directory '{_cacheData.PackageDirectory}' (current is '{_packagesDirectory}').");
            return true;
        }

        // var currentPackages = Directory.GetFiles(_packagesDirectory, "*.pkg", SearchOption.TopDirectoryOnly);
        // HashSet<ushort> allIds = GetPackageIdsFromPaths(currentPackages);

        // var cachedIds = new HashSet<ushort>(_cacheData.PackageIdToNameMap.Keys);
        // allIds.UnionWith(cachedIds);
        // if (allIds.Count != cachedIds.Count)
        // {
        //     Log.Info($"Cache file '{_cacheFilePath}' has different number of packages '{_cacheData.PackageIdToNameMap.Count}' (current is {currentPackages.Length})");
        //     return true;
        // }

        return false;
    }

    // private HashSet<ushort> GetPackageIdsFromPaths(string[] packagePaths)
    // {
    //     HashSet<ushort> packageIds = new();
    //     foreach (string packagePath in packagePaths)
    //     {
    //         IPackage package = PackageResourcer.Get().GetPackage(packagePath);
    //         PackageMetadata packageMetadata = package.GetPackageMetadata();
    //         packageIds.Add(packageMetadata.Id);
    //     }
    //     return packageIds;
    // }

    /// <summary>
    /// If we can't get a game version, presume its static and set it to 0.
    /// </summary>
    private static bool IsSameDirectory(string a, string b)
    {
        static string Normalise(string path) => Path.GetFullPath(path).TrimEnd('/', '\\');
        return string.Equals(Normalise(a), Normalise(b), StringComparison.OrdinalIgnoreCase);
    }

    private uint GetGameVersionHash()
    {
        // The executable sits next to the packages folder
        string gameDirectory = Path.GetDirectoryName(_packagesDirectory.TrimEnd('/', '\\')) ?? _packagesDirectory;
        string path = Path.Combine(gameDirectory, "destiny2.exe");
        if (!File.Exists(path))
        {
            Log.Warning($"Could not find game executable '{path}' for game version, assuming static.");
            return 0;
        }
        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
        string? version = versionInfo.FileVersion;
        if (version == null)
        {
            Log.Warning($"Could not find game version of executable '{path}', assuming static.");
            return 0;
        }
        byte[] encoded = SHA256.HashData(Encoding.UTF8.GetBytes(version));
        uint value = BitConverter.ToUInt32(encoded, 0);
        return value;
    }

    private void FillCacheFromCacheFile()
    {
        PackagePathCacheData data = JsonConvert.DeserializeObject<PackagePathCacheData>(File.ReadAllText(_cacheFilePath));
        foreach (KeyValuePair<ushort, string> entry in data.PackageIdToNameMap)
        {
            PackageIdToPathMap.TryAdd(entry.Key, Path.Combine(_packagesDirectory, entry.Value));
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

        string jsonString = JsonConvert.SerializeObject(cacheData, Formatting.Indented);
        File.WriteAllText(_cacheFilePath, jsonString);
    }

    /// <summary>
    /// Only gets the highest patch version of each package.
    /// </summary>
    private Dictionary<ushort, string> GetPackagePathCacheEntries()
    {
        Dictionary<ushort, string> highestName = new();
        Dictionary<int, int> highestPatch = new();

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

    public string GetPackagePathFromId(ushort packageId, FileHash? hash = null)
    {
        if (PackageIdToPathMap.TryGetValue(packageId, out string packagePath))
        {
            return packagePath;
        }
        throw new ArgumentException($"The package id '{packageId:x4}' from Hash '{(hash ?? "NULL")} ({hash?.Hash32})' is not in the package paths cache");
    }

    private static readonly string PackageStringNotInPackagePathsCacheMessage = "The package string is not in the package paths cache: ";
    public ushort GetPackageIdFromPath(string packagePath)
    {
        // This search is not performant but will be rarely used
        foreach ((ushort packageId, string path) in PackageIdToPathMap)
        {
            if (path == packagePath)
                return packageId;
        }
        throw new ArgumentException(PackageStringNotInPackagePathsCacheMessage + packagePath);
    }

    public List<ushort> GetAllPackageIds()
    {
        return PackageIdToPathMap.Keys.ToList();
    }

    public Dictionary<ushort, string> GetAllPackagesMap()
    {
        return new Dictionary<ushort, string>(PackageIdToPathMap);
    }
}

// Idk where else to put this, or if this is even needed
public struct NamedEntities
{
    // strategy -> entity hash -> entity name -> state
    public ConcurrentDictionary<TigerStrategy, ConcurrentDictionary<string, ConcurrentDictionary<string, EntityState>>> EntityNames;

    public NamedEntities(TigerStrategy version, FileHash entityHash, ConcurrentDictionary<string, EntityState> entityNames)
    {
        var innerDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<string, EntityState>>();
        innerDictionary[entityHash] = entityNames;

        EntityNames = new ConcurrentDictionary<TigerStrategy, ConcurrentDictionary<string, ConcurrentDictionary<string, EntityState>>>();
        EntityNames[version] = innerDictionary;
    }

    public void AddEntityName(TigerStrategy strategy, string entityHash, string entityName, Dictionary<uint, uint>? state = null)
    {
        var hashes = EntityNames.GetOrAdd(strategy, _ => new());
        var names = hashes.GetOrAdd(entityHash, _ => new());
        var entry = names.GetOrAdd(entityName, _ => new EntityState());
        if (state == null)
            return;

        lock (entry)
        {
            entry.State ??= new();
            foreach (var (key, value) in state)
                entry.State[key] = value;
        }
    }
}

public class EntityState
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(HexDictionaryConverter))]
    public Dictionary<uint, uint>? State;
}

public class HexDictionaryConverter : JsonConverter<Dictionary<uint, uint>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<uint, uint>? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        foreach (var (key, val) in value)
        {
            writer.WritePropertyName($"0x{key:X8}");
            writer.WriteValue($"0x{val:X8}");
        }
        writer.WriteEndObject();
    }

    public override Dictionary<uint, uint>? ReadJson(JsonReader reader, Type objectType, Dictionary<uint, uint>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        Dictionary<uint, uint> result = new();
        foreach (var (key, val) in serializer.Deserialize<Dictionary<string, string>>(reader)!)
            result[Parse(key)] = Parse(val);

        return result;
    }

    private static uint Parse(string s) => s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
        ? Convert.ToUInt32(s[2..], 16)
        : uint.Parse(s);
}
