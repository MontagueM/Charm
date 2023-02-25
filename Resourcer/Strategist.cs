using System.Reflection;
using Tiger.Attributes;

namespace Resourcer;

public class UpdateStrategyEventArgs : EventArgs
{
    public TigerStrategy NewStrategy { get; set; }
}

public abstract class StrategistSingleton<T> where T : StrategistSingleton<T>
{
    private static Dictionary<TigerStrategy, T> _strategyInstances = new();
    
    private static T? _instance = null;
    
    public static T Get()
    {
        if (_instance == null)
        {
            AddNewStrategyInstance(Strategy.CurrentStrategy);
            _instance = _strategyInstances[Strategy.CurrentStrategy];
        }
        
        return _instance;
    }

    private static void AddNewStrategyInstance(TigerStrategy strategy)
    {
        _instance = (T) Activator.CreateInstance(typeof(T),
            Strategy.GetStrategyConfiguration(strategy));
        _strategyInstances.Add(strategy, _instance);
    }

    static StrategistSingleton()
    {
        Strategy.OnStrategyChanged += OnStrategyChanged;
    }

    private static void OnStrategyChanged(UpdateStrategyEventArgs e)
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
}

public enum TigerStrategy
{
    NONE,
    [StrategyMetadata("ps4")]
    DESTINY1_PS4,
    [StrategyMetadata("w64")]
    DESTINY2_LATEST,
    [StrategyMetadata("w64")]
    DESTINY2_111894,
}

public struct StrategyConfiguration
{
    public string PackagesDirectory;
}

public static class Strategy
{
    private static readonly Dictionary<TigerStrategy, StrategyConfiguration> _strategyConfigurations = new();

    private static readonly TigerStrategy _defaultStrategy = TigerStrategy.NONE;
    private static TigerStrategy _currentStrategy = _defaultStrategy;
    public static TigerStrategy CurrentStrategy
    {
        get => _currentStrategy;
        set => SetStrategy(value);
    }

    public delegate void UpdateStrategyEventHandler(UpdateStrategyEventArgs e);
    public static event UpdateStrategyEventHandler OnStrategyChanged = delegate { };

    static Strategy()
    {
        SetStrategy(_defaultStrategy);
    }
    
    private static void SetStrategy(TigerStrategy strategy)
    {
        _currentStrategy = strategy;
        OnStrategyChanged.Invoke(new UpdateStrategyEventArgs{NewStrategy = _currentStrategy});
    }
    
    public static StrategyConfiguration GetStrategyConfiguration(TigerStrategy strategy)
    {
        return _strategyConfigurations[strategy];
    }
    
    public static void AddNewStrategy(TigerStrategy strategy, string packagesDirectory)
    {
        CheckValidPackagesDirectory(strategy, packagesDirectory);
        var config = new StrategyConfiguration
        {
            PackagesDirectory = packagesDirectory
        };
        _strategyConfigurations.Add(strategy, config);
        if (_currentStrategy == _defaultStrategy)
        {
            SetStrategy(strategy);
        }
    }
    
    private static void CheckValidPackagesDirectory(TigerStrategy strategy, string packagesDirectory)
    {
        CheckPackagesDirectoryExists(packagesDirectory);
        CheckPackagesDirectoryEmpty(packagesDirectory);
        CheckPackagesDirectoryValidPrefix(strategy, packagesDirectory);
        CheckPackagesDirectoryValidExtension(packagesDirectory);
    }
    
    private static readonly string PackagesDirectoryDoesNotExistMessage = "The packages directory does not exist: ";
    private static void CheckPackagesDirectoryExists(string packagesDirectory)
    {
        if (!Directory.Exists(packagesDirectory))
        {
            throw new DirectoryNotFoundException(PackagesDirectoryDoesNotExistMessage + packagesDirectory);
        }
    }
    
    private static readonly string PackagesDirectoryEmptyMessage = "The packages directory is empty: ";
    private static void CheckPackagesDirectoryEmpty(string packagesDirectory)
    {
        if (!Directory.EnumerateFiles(packagesDirectory).Any())
        {
            throw new ArgumentException(PackagesDirectoryEmptyMessage + packagesDirectory);
        }
    }
    
    private static readonly string PackagesDirectoryInvalidPrefixMessage = "The packages directory does not contain a package with the correct prefix: ";
    private static void CheckPackagesDirectoryValidPrefix(TigerStrategy strategy, string packagesDirectory)
    {
        string packagePath = Directory.EnumerateFiles(packagesDirectory).First();
        string prefix = GetStrategyPackagePrefix(strategy);
        
        if (!packagePath.Contains(prefix + "_"))
        {
            throw new ArgumentException(PackagesDirectoryInvalidPrefixMessage + packagesDirectory + ", " + prefix);
        }
    }

    private static readonly string PackagesDirectoryInvalidExtensionMessage = "The packages directory does not contain a package with the extension .pkg.: ";
    private static void CheckPackagesDirectoryValidExtension(string packagesDirectory)
    {
        if (!Directory.EnumerateFiles(packagesDirectory).First().EndsWith(".pkg"))
        {
            throw new ArgumentException(PackagesDirectoryInvalidExtensionMessage + packagesDirectory);
        }
    }

    public static void Reset()
    {
        _strategyConfigurations.Clear();
        CurrentStrategy = _defaultStrategy;
    }

    public static string GetStrategyPackagePrefix(TigerStrategy strategy)
    {
        var member = typeof(TigerStrategy).GetMember(strategy.ToString());
        StrategyMetadata attribute = (StrategyMetadata)member[0].GetCustomAttribute(typeof(StrategyMetadata), false);
        return attribute.PackagePrefix;
    }
}