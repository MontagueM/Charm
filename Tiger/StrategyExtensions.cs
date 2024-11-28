namespace Tiger;

public static class ResourcerStrategyExtensions
{
    private static Dictionary<TigerStrategy, Type>? _strategyPackageTypes = null;

    public static Type GetPackageType(this TigerStrategy strategy)
    {
        return _strategyPackageTypes?[strategy] ?? throw new Exception($"No package type found for strategy {strategy}");
    }

    static ResourcerStrategyExtensions()
    {
        FillPackageTypes();
    }

    public static void FillPackageTypes()
    {
        _strategyPackageTypes = GetPackageTypesMap();
        _strategyPackageTypes.GetFullStrategyMap();
    }

    private static bool ImplementsIPackage(this Type classType)
    {
        return classType.FindInterfaces((type, _) => type == typeof(IPackage), null).Length > 0;
    }

    private static Dictionary<TigerStrategy, Type> GetPackageTypesMap()
    {
        Dictionary<TigerStrategy, Type> packageTypesMap = new();

        var packageTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t => t.ImplementsIPackage());

        foreach (Type packageType in packageTypes)
        {
            IEnumerable<TigerStrategy> strategies = packageType
                .GetCustomAttributes(typeof(StrategyClassAttribute), true)
                .Select(x => ((StrategyClassAttribute)x).Strategy);
            foreach (TigerStrategy strategy in strategies)
            {
                if (packageTypesMap.TryGetValue(strategy, out var existingPackageType))
                {
                    throw new Exception($"Multiple package types found for strategy {strategy}: {existingPackageType} and {packageType}");
                }

                packageTypesMap.Add(strategy, packageType);
            }
        }

        return packageTypesMap;
    }

    // Takes a map partially filled with TigerStrategy keys and fills it with all other TigerStrategy keys
    // Assumes if it's missing, we take the value of the strategy before it
    public static void GetFullStrategyMap<TValue>(this IDictionary<TigerStrategy, TValue> dict)
    {
        TValue? value = default;
        foreach (TigerStrategy strategy in Enum.GetValues(typeof(TigerStrategy)).Cast<TigerStrategy>())
        {
            if (strategy == TigerStrategy.NONE)
            {
                continue;
            }

            if (dict.TryGetValue(strategy, out TValue outValue))
            {
                value = outValue;
            }
            else
            {
                if (value == null || value.Equals(default))
                {
                    // todo do something about it, its bad but shouldnt be fatal for testing purposes
                    // throw new Exception($"No type found for strategy {strategy}");
                }
                else
                {
                    dict.Add(strategy, value);
                }
            }
        }
    }
}
