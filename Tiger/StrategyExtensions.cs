namespace Tiger;

public static class ResourcerStrategyExtensions
{
    private static Dictionary<TigerStrategy, Type>? _strategyPackageTypes = null;

    public static Type GetPackageType(this TigerStrategy strategy)
    {
        if (_strategyPackageTypes == null)
        {
            FillPackageTypes();
        }
        return _strategyPackageTypes?[strategy] ?? throw new Exception($"No package type found for strategy {strategy}");
    }

    private static void FillPackageTypes()
    {
        _strategyPackageTypes = new Dictionary<TigerStrategy, Type>();

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t => t.ImplementsIPackage());

        foreach (Type type in types)
        {
            HashSet<TigerStrategy> typeStrategies = GetStrategiesFromType(type);
            foreach (TigerStrategy strategyValue in typeStrategies)
            {
                if (_strategyPackageTypes.TryGetValue(strategyValue, out var packageType))
                {
                    throw new Exception($"Multiple package types found for strategy {strategyValue}: {packageType} and {type}");
                }
                _strategyPackageTypes.Add(strategyValue, type);
            }
        }
    }

    private static bool ImplementsIPackage(this Type classType)
    {
        return classType.FindInterfaces((type, _) => type == typeof(IPackage), null).Length > 0;
    }

    private static HashSet<TigerStrategy> GetStrategiesFromType(Type type)
    {
        HashSet<TigerStrategy> strategies = new HashSet<TigerStrategy>();

        GetStrategyFromNamespace(type, ref strategies);

        type.GetCustomAttributes(typeof(StrategyClassAttribute), true)
            .Select(x => ((StrategyClassAttribute)x).Strategy)
            .ToList().ForEach(x => strategies.Add(x));

        return strategies;
    }

    private static void GetStrategyFromNamespace(Type type, ref HashSet<TigerStrategy> strategies)
    {
        string? typeNamespaceStrategy = type.Namespace?.Split('.').Last();
        if (typeNamespaceStrategy != null)
        {
            if (Enum.TryParse(typeNamespaceStrategy, out TigerStrategy strategy))
            {
                strategies.Add(strategy);
            }
        }
    }
}
