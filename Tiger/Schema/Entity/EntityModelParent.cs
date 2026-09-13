namespace Tiger.Schema.Entity;

public class EntityModelParent : EntityComponent
{
    private ModelPermutation _materialPermutations;
    public ModelPermutation MaterialPermutations
    {
        get
        {
            if (_materialPermutations is not null)
                return _materialPermutations;

            _materialPermutations = GetModelPermutations();
            return _materialPermutations;
        }
        set
        {
            _materialPermutations = value;
        }
    }

    public EntityModelParent(FileHash resource) : base(resource)
    {
    }

    public S80806D8F Reader => ((S80806D8F)TagData.Unk18.GetValue(GetReader()));

    public EntityModel GetModel()
    {
        return Reader.Model;
    }

    public AABB GetBoundingBox()
    {
        return Reader.BoundingBox;
    }

    // materials, 8109A271 to test
    public ModelPermutation? GetModelPermutations()
    {
        if (!Strategy.IsLatest())
            return null;

        if (Reader.Unk400.Count == 0 || Reader.Unk410.Count == 0)
            return null;

        var config = new ModelPermutation();

        //foreach (var keys1 in tag.Unk38)
        //{
        //    foreach (var keys in keys1.Unk8)
        //    {
        //        if (keys.Value.Hash32 == 0x871AC0EA)
        //            continue;

        //        config.Configuration.TryAdd(keys.SwitchKey, keys.Value);
        //    }
        //}

        foreach (var u0 in Reader.Unk38)
        {
            foreach (var pair in u0.Unk8)
            {
                if (!config.Keys.ContainsKey(pair.SwitchKey))
                    config.Keys[pair.SwitchKey] = new HashSet<uint>();

                config.Keys[pair.SwitchKey].Add(pair.Value);
            }
        }

        for (int i = 0; i < Reader.Unk410.Count; i++)
        {
            var u = Reader.Unk410[GetReader(), i];
            if (u.Unk02 < 0)
                continue;

            int start = (int)u.Unk02;
            int end = start + (int)u.Unk00;
            var keysMap = new Dictionary<uint, uint>();

            if (end >= Reader.Unk400.Count)
                continue;

            for (int j = start; j < end; j++)
            {
                var m = Reader.Unk38[Reader.Unk400[GetReader(), j].Value];
                keysMap[m.Unk8[0].SwitchKey] = m.Unk8[0].Value;
            }

            var pairList = keysMap.OrderBy(kv => kv.Key).Select(kv => (kv.Key, kv.Value)).ToList();
            config.PairsToPermutation[pairList] = i;
        }

        return config;
    }

    public void PermutationTest()
    {
        var map = FileResourcer.Get().GetSchemaTag<SMapDataTable>(new FileHash(0x80D26A51));

        using TigerReader reader = map.GetReader();
        var resource = (S80808019)map.TagData.DataEntries[0].DataResource.GetValue(reader);

        S80802AB2 overrides = default;
        if (resource.Unk00.GetValue(reader) is S80803FFD a)
            overrides = (S80802AB2)a.Unk00.GetValue(reader);
        else if (resource.Unk00.GetValue(reader) is S80802AB2 b)
            overrides = b;

        var pairs = new List<(uint, uint)>();
        foreach (var pair in overrides.Overrides)
        {
            var key = pair.SwitchKey;
            var value = pair.Value;
            pairs.Add((key, value));
        }

        var overrideState = LoadOverrideState(pairs); // faction, type, weapon_type, unit_type from map data

        var query = new Dictionary<uint, uint>(overrideState)
        {
            [0x6A807A1A] = 0x2CA33BDB,
        };

        var combinations = GetAllCombinations(baseState: query);
        foreach (var c in combinations)
        {
            foreach (var state in c.State)
            {
                Console.WriteLine($"{GlobalStrings.Get().GetString(state.Key)} : {GlobalStrings.Get().GetString(state.Value)}");
            }
            foreach (var group in c.Groups)
            {
                Console.WriteLine($"\tMesh Group: {group}");
            }
            Console.WriteLine();
        }
    }

    public List<int> GetMeshGroupsForState(IReadOnlyDictionary<uint, uint> hashes)
    {
        var matches = new List<int>();

        for (int i = 0; i < Reader.Unk38.Count; i++)
        {
            var pairs = Reader.Unk38[i].Unk8;
            if (pairs == null || pairs.Count == 0)
                continue;

            var conditionsByKey = new Dictionary<uint, List<uint>>();
            foreach (var pair in pairs)
            {
                var key = pair.SwitchKey.Hash32;
                var value = pair.Value.Hash32;

                if (!conditionsByKey.TryGetValue(key, out var values))
                    conditionsByKey[key] = values = new List<uint>();
                values.Add(value);
            }

            bool isMatch = true;
            foreach (var (keyHash, allowedValues) in conditionsByKey)
            {
                if (!hashes.TryGetValue(keyHash, out uint hash))
                    continue;

                if (!allowedValues.Contains(hash))
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
                matches.Add(i);
        }

        return matches;
    }

    public Dictionary<uint, List<uint>> GetDistinctKeyValues()
    {
        var dict = new Dictionary<uint, HashSet<uint>>();
        foreach (var entry in Reader.Unk38)
        {
            var pairs = entry.Unk8;
            if (pairs.Count == 0)
                continue;

            foreach (var pair in pairs)
            {
                var key = pair.SwitchKey.Hash32;
                var value = pair.Value.Hash32;

                if (!dict.TryGetValue(key, out var values))
                    dict[key] = values = new HashSet<uint>();
                values.Add(value);
            }
        }

        return dict.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
    }

    public List<StateCombination> GetAllCombinations(
    IReadOnlyDictionary<uint, uint>? baseState = null,
    IEnumerable<uint>? keysToVary = null)
    {
        var distinctValues = GetDistinctKeyValues();
        baseState ??= new Dictionary<uint, uint>();

        List<uint> keys = (keysToVary ?? distinctValues
                .Where(kv => kv.Value.Count > 1 && !baseState.ContainsKey(kv.Key))
                .Select(kv => kv.Key)).ToList();

        var product = new List<Dictionary<uint, uint>> { new Dictionary<uint, uint>(baseState) };
        foreach (uint key in keys)
        {
            if (!distinctValues.TryGetValue(key, out var values) || values.Count == 0)
                continue;

            product = product.SelectMany(partial => values.Select(value =>
            {
                var next = new Dictionary<uint, uint>(partial) { [key] = value };
                return next;
            })).ToList();
        }

        var results = new List<StateCombination>();
        foreach (var state in product)
        {
            var groups = GetMeshGroupsForState(state);
            if (groups.Count > 0)
                results.Add(new StateCombination(state, groups));
        }

        return results;
    }

    public static Dictionary<uint, uint> LoadOverrideState(List<(uint, uint)> pairs)
    {
        var state = new Dictionary<uint, uint>(pairs.Count);
        foreach (var (key, value) in pairs)
            state[key] = value;
        return state;
    }
}

public sealed class StateCombination
{
    public IReadOnlyDictionary<uint, uint> State { get; }
    public IReadOnlyList<int> Groups { get; }

    public StateCombination(IReadOnlyDictionary<uint, uint> state, IReadOnlyList<int> groups)
    {
        State = state;
        Groups = groups;
    }
}


public class ModelPermutation
{
    public Dictionary<uint, uint> Configuration { get; set; }
    public SortedDictionary<uint, HashSet<uint>> Keys { get; set; } = new();
    public Dictionary<List<(uint, uint)>, int> PairsToPermutation = new Dictionary<List<(uint, uint)>, int>(new ListTupleComparer());
    public int OverrideIndex { get; set; } = -1;

    public IEnumerable<(uint Key, HashSet<uint> Values)> IterateKeys()
    {
        foreach (var kv in Keys)
        {
            yield return (kv.Key, kv.Value);
        }
    }

    public static void UpdateConfiguration(ModelPermutation permutations, Dictionary<uint, uint> desiredValues)
    {
        if (permutations.Configuration is null)
            permutations.Configuration = new();

        bool changed = false;

        foreach (var kv in permutations.Keys)
        {
            if (!desiredValues.TryGetValue(kv.Key, out uint desiredValue))
                continue;

            uint newVal = kv.Value.FirstOrDefault(v => v == desiredValue);
            if (newVal != 0)
            {
                if (!permutations.Configuration.TryGetValue(kv.Key, out uint oldVal) || oldVal != newVal)
                {
                    permutations.Configuration[kv.Key] = newVal;
                    changed = true;
                }
            }
        }

        if (changed)
            permutations._dirty = true;
    }

    public int? CalculatePermutationIndex()
    {
        if (Configuration is null)
            return null;

        var keyVals = Configuration
            .OrderBy(kv => kv.Key)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();

        if (PairsToPermutation.TryGetValue(keyVals, out var index))
            return index;

        return null;
    }

    private List<(uint, uint)>? _cachedKeyVals;
    private bool _dirty = true;
    public int? CalculatePermutationIndexFast()
    {
        if (Configuration == null)
            return null;

        if (_dirty)
        {
            BuildCachedKey();
            _dirty = false;
        }

        return PairsToPermutation.TryGetValue(_cachedKeyVals!, out int index) ? index : null;
    }

    private void BuildCachedKey()
    {
        if (_cachedKeyVals == null)
            _cachedKeyVals = new List<(uint, uint)>(Configuration.Count);
        else
            _cachedKeyVals.Clear();

        foreach (var kv in Configuration)
            _cachedKeyVals.Add((kv.Key, kv.Value));

        _cachedKeyVals.Sort(static (a, b) => a.Item1.CompareTo(b.Item1));
    }
}

public class ListTupleComparer : IEqualityComparer<List<(uint, uint)>>
{
    public bool Equals(List<(uint, uint)>? x, List<(uint, uint)>? y)
    {
        if (x == null || y == null || x.Count != y.Count)
            return false;

        for (int i = 0; i < x.Count; i++)
        {
            if (x[i] != y[i])
                return false;
        }

        return true;
    }

    public int GetHashCode(List<(uint, uint)> obj)
    {
        int hash = 17;
        foreach (var (k, v) in obj)
        {
            hash = hash * 31 + k.GetHashCode();
            hash = hash * 31 + v.GetHashCode();
        }
        return hash;
    }
}
