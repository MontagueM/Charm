using System.Collections.Concurrent;
using System.Reflection;
using Arithmic;

namespace Tiger;



public class StrategyEventArgs : EventArgs
{
    public TigerStrategy Strategy { get; set; }
}


// todo separate the metadata out, no need to put it all in one place - just gets too long and nasty
// the package type should be done by the resourcer, the app/depot etc should be done by tomograph data

// Order of values matters; Tag definitions are processed top to bottom so if you only provide a tag definition for WQ and not LF,
// the code will presume that LF is the same as WQ. If this is not the case it will throw an exception.
public enum TigerStrategy
{
    NONE = 0,
    [StrategyMetadata("ps4")]
    DESTINY1_RISE_OF_IRON = 1000,
    [StrategyMetadata("w64", 1085660, 1085661, 7002268313830901797, 1085662, 2399965969279284756)]
    DESTINY2_SHADOWKEEP_2601 = 2601,
    [StrategyMetadata("w64", 1085660, 1085661, 4160053308690659072, 1085662, 4651412338057797072)]
    DESTINY2_SHADOWKEEP_2999 = 2999,
    [StrategyMetadata("w64", 1085660, 1085661, 5631185797932644936, 1085662, 3832609057880895101)]
    DESTINY2_BEYONDLIGHT_3402 = 3402,
    [StrategyMetadata("w64", 1085660, 1085661, 6051526863119423207, 1085662, 1078048403901153652)]
    DESTINY2_WITCHQUEEN_6307 = 6307,
    [StrategyMetadata("w64", 1085660, 1085661, 7707143404100984016, 1085662, 5226038440689554798)]
    DESTINY2_LIGHTFALL_7366 = 7366,
    [StrategyMetadata("w64")]
    DESTINY2_LATEST = 20000,  // there probably wont be a tiger version higher than this
}

public struct StrategyConfiguration
{
    public string PackagesDirectory;
}

public class Strategy
{
    [ConfigField("StrategyConfigurations")]
    private static Dictionary<TigerStrategy, StrategyConfiguration> _strategyConfigurations = new();

    public static IEnumerable<TigerStrategy> GetAllStrategies() => _strategyConfigurations.Keys;

    private static readonly TigerStrategy _defaultStrategy = TigerStrategy.NONE;

    [ConfigField("CurrentStrategy")]
    private static TigerStrategy _currentStrategy = _defaultStrategy;

    // Using this is not recommended, as most classes shouldn't modify behaviour internally based on the strategy.
    // Instead, you should use StrategistSingleton to abstract the strategy away.
    public static TigerStrategy CurrentStrategy { get => _currentStrategy; }

    public delegate void BeforeStrategyEventHandler(StrategyEventArgs e);
    public static event BeforeStrategyEventHandler BeforeStrategyEvent = delegate { };

    public delegate void DuringStrategyEventHandler(StrategyEventArgs e);
    public static event DuringStrategyEventHandler DuringStrategyEvent = delegate { };

    public delegate void AfterStrategyEventHandler(StrategyEventArgs e);
    public static event AfterStrategyEventHandler AfterStrategyEvent = delegate { };

    public delegate void UpdateStrategyEventHandler(StrategyEventArgs e);
    public static event UpdateStrategyEventHandler OnStrategyChangedEvent = delegate { };

    public delegate void ResetStrategyEventHandler(StrategyEventArgs e);
    public static event ResetStrategyEventHandler OnStrategyResetEvent = delegate { };

    static Strategy() { SetStrategy(_defaultStrategy); }

    public static bool IsLatest() => CurrentStrategy == TigerStrategy.DESTINY2_LATEST;
    public static bool IsPostBL() => CurrentStrategy > TigerStrategy.DESTINY2_BEYONDLIGHT_3402;
    public static bool IsBL() => CurrentStrategy == TigerStrategy.DESTINY2_BEYONDLIGHT_3402;
    public static bool IsPreBL() => CurrentStrategy == TigerStrategy.DESTINY2_SHADOWKEEP_2601 || CurrentStrategy == TigerStrategy.DESTINY2_SHADOWKEEP_2999;
    public static bool IsD1() => CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON;

    /// <exception cref="ArgumentException">'strategyString' does not exist.</exception>
    public static void SetStrategy(string strategyString)
    {
        bool strategyExists = Enum.TryParse(strategyString, true, out TigerStrategy strategy);
        if (!strategyExists)
        {
            throw new ArgumentException($"Game strategy string does not exist '{strategyString}'");
        }
        SetStrategy(strategy);
    }

    public static bool SetStrategy(TigerStrategy strategy)
    {
        if (strategy != TigerStrategy.NONE && !_strategyConfigurations.ContainsKey(strategy))
        {
            Log.Error($"Game strategy does not exist '{strategy}', ignoring.");
            return false;
        }
        _currentStrategy = strategy;

        if (strategy == TigerStrategy.NONE)
        {
            return true;
        }

        BeforeStrategyEvent.Invoke(new StrategyEventArgs { Strategy = _currentStrategy });
        OnStrategyChangedEvent.Invoke(new StrategyEventArgs { Strategy = _currentStrategy });
        AfterStrategyEvent.Invoke(new StrategyEventArgs { Strategy = _currentStrategy });

        //Task.Run(() =>
        //{
        //    BeforeStrategyEvent.Invoke(new StrategyEventArgs { Strategy = _currentStrategy });
        //    OnStrategyChangedEvent.Invoke(new StrategyEventArgs { Strategy = _currentStrategy });
        //    AfterStrategyEvent.Invoke(new StrategyEventArgs { Strategy = _currentStrategy });
        //});

        return true;
    }


    /// <exception cref="ArgumentException">'strategy' does not exist.</exception>
    public static StrategyConfiguration GetStrategyConfiguration(TigerStrategy strategy)
    {
        bool strategyExists = _strategyConfigurations.TryGetValue(strategy, out var configuration);
        if (!strategyExists)
        {
            throw new ArgumentException($"Game strategy does not exist '{strategy}'");
        }
        return configuration;
    }

    public static void UpdateStrategyConfiguration(TigerStrategy strategy, StrategyConfiguration configuration)
    {
        if (!_strategyConfigurations.ContainsKey(strategy))
        {
            throw new ArgumentException($"Game strategy does not exist '{strategy}'");
        }

        bool bResetState = _strategyConfigurations[strategy].PackagesDirectory != configuration.PackagesDirectory;

        _strategyConfigurations[strategy] = configuration;

        if (bResetState)
        {
            OnStrategyResetEvent.Invoke(new StrategyEventArgs() { Strategy = strategy });
        }
    }

    /// <summary>
    /// Add a new strategy to the list of available strategies.
    /// </summary>
    public static bool AddNewStrategy(TigerStrategy strategy, string packagesDirectory, bool set = true)
    {
        if (strategy == TigerStrategy.NONE || _strategyConfigurations.ContainsKey(strategy))
        {
            Log.Error($"Game strategy '{strategy}' already exists.");
            return false;
        }
        if (!Enum.IsDefined(typeof(TigerStrategy), strategy))
        {
            Log.Error($"Game strategy '{strategy}' cannot be set as is not defined.");
            return false;
        }
        if (_strategyConfigurations.Values.Any(config => config.PackagesDirectory == packagesDirectory))
        {
            Log.Error($"Game strategy '{strategy}' cannot re-use an existing packages path '{packagesDirectory}'");
            return false;
        }

        bool isValid = CheckValidPackagesDirectory(strategy, packagesDirectory);

        if (!isValid)
        {
            Log.Error($"Game strategy cannot use an invalid packages path '{packagesDirectory}', ignoring.");
            return false;
        }

        var config = new StrategyConfiguration { PackagesDirectory = packagesDirectory };
        _strategyConfigurations.Add(strategy, config);
        if (set && _currentStrategy == _defaultStrategy)
        {
            return SetStrategy(strategy);
        }
        else
        {
            return false;
        }
    }

    public static void CheckValidPackagesDirectory(string packagesDirectory)
    {
        CheckValidPackagesDirectory(_currentStrategy, packagesDirectory);
    }

    /// <summary>
    /// Throws exceptions based on the invalid state of the packages directory, otherwise does nothing.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">Package directory does not exist.</exception>
    /// <exception cref="ArgumentException">Package directory contents is invalid.</exception>
    private static bool CheckValidPackagesDirectory(TigerStrategy strategy, string packagesDirectory)
    {
        if (PackagesDirectoryDoesNotExist(packagesDirectory))
        {
            Log.Error($"The packages directory does not exist: {packagesDirectory}");
            return false;
        }

        if (PackagesDirectoryEmpty(packagesDirectory))
        {
            Log.Error($"The packages directory is empty: {packagesDirectory}");
            return false;
        }

        if (PackageFilesHasInvalidPrefix(strategy, packagesDirectory))
        {
            Log.Error($"The packages directory contains a package without the correct prefix '{GetStrategyPackagePrefix(strategy)}': {packagesDirectory}");
            return false;
        }

        if (PackageFilesHasInvalidExtension(packagesDirectory))
        {
            Log.Error($"The packages directory contains a package without the correct extension '.pkg': {packagesDirectory}");
            return false;
        }

        return true;
    }

    private static bool PackagesDirectoryDoesNotExist(string packagesDirectory)
    {
        return !Directory.Exists(packagesDirectory);
    }

    private static bool PackagesDirectoryEmpty(string packagesDirectory)
    {
        return !Directory.EnumerateFiles(packagesDirectory).Any();
    }

    private static bool PackageFilesHasInvalidPrefix(TigerStrategy strategy, string packagesDirectory)
    {
        var packagePaths = Directory.EnumerateFiles(packagesDirectory);
        string prefix = GetStrategyPackagePrefix(strategy);

        return packagePaths.Any(path => !path.Contains(prefix + "_"));
    }

    private static bool PackageFilesHasInvalidExtension(string packagesDirectory)
    {
        var packagePaths = Directory.EnumerateFiles(packagesDirectory);
        return packagePaths.Any(path => !path.EndsWith(".pkg"));
    }

    public static void Reset()
    {
        _strategyConfigurations.Clear();
        SetStrategy(_defaultStrategy);
    }

    public static string GetStrategyPackagePrefix(TigerStrategy strategy)
    {
        var member = typeof(TigerStrategy).GetMember(strategy.ToString());
        StrategyMetadataAttribute? attribute = (StrategyMetadataAttribute?)member[0].GetCustomAttribute(typeof(StrategyMetadataAttribute), false);
        if (attribute == null)
        {
            throw new ArgumentException($"Strategy '{strategy}' does not have a metadata attribute");
        }
        return attribute.PackagePrefix;
    }

    public abstract class StrategistSingleton<T>
        where T : StrategistSingleton<T>
    {
        private static ConcurrentDictionary<TigerStrategy, T> _strategyInstances = new();

        protected static T? _instance = null;

        protected TigerStrategy _strategy;

        /// <summary>
        /// Called after the StrategySingleton instance is created.
        /// </summary>
        protected abstract void Initialise();

        /// <summary>
        /// Called after a reset operation is called for this strategy, e.g. when the packages directory changes.
        /// </summary>
        protected abstract void Reset();

        public static T Get()
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("Strategy instance has not been created");
            }

            return _instance;
        }

        public static void Setup()
        {
            AddNewStrategyInstance(_currentStrategy);
            _instance = _strategyInstances[_currentStrategy];
        }

        // todo test this
        public static T Get(TigerStrategy strategy)
        {
            if (_strategyInstances.TryGetValue(strategy, out var instance))
            {
                return instance;
            }

            throw new ArgumentException($"Strategy '{strategy}' does not exist");
        }

        protected static void AddNewStrategyInstance(TigerStrategy strategy)
        {
            if (typeof(T).GetConstructor(new[] { typeof(TigerStrategy) }) != null)
            {
                _instance = (T)Activator.CreateInstance(typeof(T), strategy);
            }
            else if (typeof(T).GetConstructor(new[] { typeof(TigerStrategy), typeof(StrategyConfiguration) }) != null)
            {
                _instance = (T)Activator.CreateInstance(typeof(T), strategy, Strategy.GetStrategyConfiguration(strategy));
            }
            else
            {
                throw new Exception($"Invalid constructor for {typeof(T)}");
            }

            _strategyInstances.TryAdd(strategy, _instance);

            _instance.Initialise();
        }

        static StrategistSingleton()
        {
        }

        protected StrategistSingleton(TigerStrategy strategy)
        {
            _strategy = strategy;
        }

#pragma warning disable S1144 // Unused private types or members should be removed
        private static void RegisterEvents()
        {
            Strategy.OnStrategyChangedEvent += OnStrategyChangedEvent;
            Strategy.OnStrategyResetEvent += OnStrategyResetEvent;
        }

        private static void OnStrategyChangedEvent(StrategyEventArgs e)
        {
            if (e.Strategy == TigerStrategy.NONE)
            {
                _instance = null;
                return;
            }

            if (!_strategyInstances.ContainsKey(e.Strategy))
            {
                AddNewStrategyInstance(e.Strategy);
            }
            _instance = _strategyInstances[e.Strategy];
            DuringStrategyEvent.Invoke(e);
        }

        private static void OnStrategyResetEvent(StrategyEventArgs e)
        {
            if (!_strategyInstances.ContainsKey(e.Strategy))
            {
                throw new Exception("Strategy to reset does not exist");
            }
            _strategyInstances[e.Strategy].Reset();
            _strategyInstances[e.Strategy].Initialise();
            DuringStrategyEvent.Invoke(e);
        }
    }

    public abstract class LazyStrategistSingleton<T> : StrategistSingleton<T> where T : LazyStrategistSingleton<T>
    {
        protected LazyStrategistSingleton(TigerStrategy strategy) : base(strategy)
        {
        }

        /// <summary>
        /// Manual init is only available for lazy singletons to avoid mistakes.
        /// Only used when trying to do something that is slow and not used all the time (e.g. investment)
        /// </summary>
        /// <returns></returns>
        public static void LazyInit()
        {
            if (_instance == null)
            {
                AddNewStrategyInstance(_currentStrategy);
            }
        }
    }

    public static bool HasConfiguration(TigerStrategy strategy)
    {
        return strategy == TigerStrategy.NONE || _strategyConfigurations.ContainsKey(strategy);
    }
}

public static class StrategyExtensions
{
    public static StrategyMetadataAttribute GetStrategyMetadata(this TigerStrategy strategy)
    {
        var member = typeof(TigerStrategy).GetMember(strategy.ToString());
        StrategyMetadataAttribute attribute = (StrategyMetadataAttribute)member[0].GetCustomAttribute(typeof(StrategyMetadataAttribute), false);
        return attribute;
    }

    public static StrategyConfiguration GetStrategyConfiguration(this TigerStrategy strategy)
    {
        var member = typeof(TigerStrategy).GetMember(strategy.ToString());
        StrategyConfiguration strategyConfiguration = Strategy.GetStrategyConfiguration(strategy);
        return strategyConfiguration;
    }
}
