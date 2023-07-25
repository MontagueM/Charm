using System.Reflection;

namespace Tiger;

public class UpdateStrategyEventArgs : EventArgs
{
    public TigerStrategy NewStrategy { get; set; }
}


public class ResetStrategyEventArgs : EventArgs
{
    public TigerStrategy StrategyToReset { get; set; }
}


// todo separate the metadata out, no need to put it all in one place - just gets too long and nasty
// the package type should be done by the resourcer, the app/depot etc should be done by tomograph data

// Order matters; Tag definitions are processed top to bottom so if you only provide a tag definition for WQ and not LF,
// the code will presume that LF is the same as WQ. If this is not the case it will throw an exception.
public enum TigerStrategy
{
    NONE,
    // [StrategyMetadata("ps4", typeof(IPackage))]
    // DESTINY1_PS4,
    [StrategyMetadata("w64", 1085660, 1085661, 7002268313830901797, 1085662, 2399965969279284756)]
    DESTINY2_SHADOWKEEP_2601,
    [StrategyMetadata("w64", 1085660, 1085661, 6051526863119423207, 1085662, 1078048403901153652)]
    DESTINY2_WITCHQUEEN_6307,
    [StrategyMetadata("w64")]
    DESTINY2_LATEST,
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

    public delegate void UpdateStrategyEventHandler(UpdateStrategyEventArgs e);
    public static event UpdateStrategyEventHandler OnStrategyChangedEvent = delegate { };

    public delegate void ResetStrategyEventHandler(ResetStrategyEventArgs e);
    public static event ResetStrategyEventHandler OnStrategyResetEvent = delegate { };

    static Strategy() { SetStrategy(_defaultStrategy); }

    /// <exception cref="ArgumentException">'strategyString' does not exist.</exception>
    public static void SetStrategy(string strategyString)
    {
        bool strategyExists = Enum.TryParse(strategyString, true, out TigerStrategy strategy);
        if (!strategyExists)
        {
            throw new ArgumentException($"Game strategy does not exist '{strategyString}'");
        }
        SetStrategy(strategy);
    }

    /// <exception cref="ArgumentException">'strategy' does not exist.</exception>
    public static void SetStrategy(TigerStrategy strategy)
    {
        if (strategy != TigerStrategy.NONE && !_strategyConfigurations.ContainsKey(strategy))
        {
            throw new ArgumentException($"Game strategy does not exist '{strategy}'");
        }
        _currentStrategy = strategy;
        OnStrategyChangedEvent.Invoke(new UpdateStrategyEventArgs { NewStrategy = _currentStrategy });
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
            OnStrategyResetEvent.Invoke(new ResetStrategyEventArgs() { StrategyToReset = strategy });
        }
    }

    /// <summary>
    /// Add a new strategy to the list of available strategies.
    /// </summary>
    /// <exception cref="ArgumentException">Strategy already exists, or packages directory is invalid.</exception>
    /// <exception cref="DirectoryNotFoundException">Package directory does not exist.</exception>
    public static void AddNewStrategy(TigerStrategy strategy, string packagesDirectory)
    {
        if (strategy == TigerStrategy.NONE || _strategyConfigurations.ContainsKey(strategy))
        {
            throw new ArgumentException($"Game strategy already exists '{strategy}'");
        }
        if (_strategyConfigurations.Values.Any(config => config.PackagesDirectory == packagesDirectory))
        {
            throw new ArgumentException($"Game strategy cannot re-use an existing packages path '{packagesDirectory}'");
        }

        CheckValidPackagesDirectory(strategy, packagesDirectory);

        var config = new StrategyConfiguration { PackagesDirectory = packagesDirectory };
        _strategyConfigurations.Add(strategy, config);
        if (_currentStrategy == _defaultStrategy)
        {
            SetStrategy(strategy);
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
    private static void CheckValidPackagesDirectory(TigerStrategy strategy, string packagesDirectory)
    {
        if (PackagesDirectoryDoesNotExist(packagesDirectory))
        {
            throw new DirectoryNotFoundException($"The packages directory does not exist: {packagesDirectory}");
        }
        if (PackagesDirectoryEmpty(packagesDirectory))
        {
            throw new ArgumentException($"The packages directory is empty: {packagesDirectory}");
        }

        if (PackageFilesHasInvalidPrefix(strategy, packagesDirectory))
        {
            throw new ArgumentException($"The packages directory contains a package without the correct prefix '{GetStrategyPackagePrefix(strategy)}': {packagesDirectory}");
        }

        if (PackageFilesHasInvalidExtension(packagesDirectory))
        {
            throw new ArgumentException($"The packages directory contains a package without the correct extension '.pkg': {packagesDirectory}");
        }
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
        private static Dictionary<TigerStrategy, T> _strategyInstances = new();

        private static T? _instance = null;

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
            if (_instance == null) // todo make this maybe work? disallowed due to thread safety concerns
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

        private static void AddNewStrategyInstance(TigerStrategy strategy)
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

            _strategyInstances.Add(strategy, _instance);

            _instance.Initialise();
        }

        static StrategistSingleton()
        {
        }

        protected StrategistSingleton(TigerStrategy strategy)
        {
            _strategy = strategy;
        }

        private static void RegisterEvents()
        {
            Strategy.OnStrategyChangedEvent += OnStrategyChangedEvent;
            Strategy.OnStrategyResetEvent += OnStrategyResetEvent;
        }

        private static void OnStrategyChangedEvent(UpdateStrategyEventArgs e)
        {
            if (e.NewStrategy == TigerStrategy.NONE)
            {
                _instance = null;
                return;
            }

            if (!_strategyInstances.ContainsKey(e.NewStrategy))
            {
                AddNewStrategyInstance(e.NewStrategy);
            }
            _instance = _strategyInstances[e.NewStrategy];
        }

        private static void OnStrategyResetEvent(ResetStrategyEventArgs e)
        {
            if (!_strategyInstances.ContainsKey(e.StrategyToReset))
            {
                throw new Exception("Strategy to reset does not exist");
            }
            _strategyInstances[e.StrategyToReset].Reset();
            _strategyInstances[e.StrategyToReset].Initialise();
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
