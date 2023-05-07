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
        _strategyPackageTypes = GetPackageTypesMap();
        _strategyPackageTypes.GetFullStrategyMap();

        // Type? packageType = null;
        // foreach (TigerStrategy strategy in Enum.GetValues(typeof(TigerStrategy)).Cast<TigerStrategy>())
        // {
        //     if (strategy == TigerStrategy.NONE)
        //     {
        //         continue;
        //     }
        //
        //     if (typeMap.TryGetValue(strategy, out Type outPackageType))
        //     {
        //         packageType = outPackageType;
        //     }
        //
        //     if (packageType == null)
        //     {
        //         throw new Exception($"No package type found for strategy {strategy}");
        //     }
        //
        //     _strategyPackageTypes.Add(strategy, packageType);
        // }
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
                    throw new Exception($"No type found for strategy {strategy}");
                }
                dict.Add(strategy, value);
            }
        }
    }
}
