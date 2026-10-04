namespace Tiger.Schema.Entity;

public class EntityModelParent : EntityComponent
{
    public HashSet<int> AppliedMeshGroups = new();
    public int AppliedMaterialIndex => MaterialPermutations.OverrideIndex;

    private MaterialPermutation _materialPermutations;
    public MaterialPermutation MaterialPermutations
    {
        get
        {
            if (_materialPermutations is not null)
                return _materialPermutations;

            _materialPermutations = GetMaterialPermutations();
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

    public static uint DamageKey = Helpers.Fnv1a32("damage");
    public static uint PermutationKey = Helpers.Fnv1a32("permutation");
    public static uint RegionKey = Helpers.Fnv1a32("region");
    public static uint UnitTypeKey = Helpers.Fnv1a32("unit_type");
    public static uint TypeKey = Helpers.Fnv1a32("type");
    public static uint NoneValue = Helpers.Fnv1a32("none");

    public MaterialPermutation GetMaterialPermutations()
    {
        if (!Strategy.IsLatest())
            return null;

        if (Reader.Unk400.Count == 0 || Reader.Unk410.Count == 0)
            return null;

        var config = new MaterialPermutation();

        var referencedIndices = new HashSet<int>();
        for (int i = 0; i < Reader.Unk400.Count; i++)
            referencedIndices.Add((int)Reader.Unk400[GetReader(), i].Value);

        foreach (int u0idx in referencedIndices)
        {
            var entry = Reader.Unk38[u0idx];
            if (entry.Unk8 == null || entry.Unk8.Count == 0)
                continue;

            var pair = entry.Unk8[0];
            if (!config.Keys.ContainsKey(pair.SwitchKey))
                config.Keys[pair.SwitchKey] = new HashSet<uint>();
            config.Keys[pair.SwitchKey].Add(pair.Value);
        }

        for (int i = 0; i < Reader.Unk410.Count; i++)
        {
            var u = Reader.Unk410[GetReader(), i];
            if (u.Unk02 < 0)
                continue;

            int start = (int)u.Unk02;
            int end = start + (int)u.Unk00;

            if (end > Reader.Unk400.Count)
                continue;

            var keysMap = new Dictionary<uint, uint>();
            for (int j = start; j < end; j++)
            {
                var m = Reader.Unk38[Reader.Unk400[GetReader(), j].Value];
                if (m.Unk8 == null || m.Unk8.Count == 0)
                    continue;
                keysMap[m.Unk8[0].SwitchKey] = m.Unk8[0].Value;
            }

            var pairList = keysMap.OrderBy(kv => kv.Key).Select(kv => (kv.Key, kv.Value)).ToList();
            config.PairsToPermutation.TryAdd(pairList, i);
            //if (!config.PairsToPermutation.TryAdd(pairList, i))
            //    Debug.Assert(false, $"Duplicate at index {i} already mapped to {config.PairsToPermutation[pairList]}");
        }

        return config;
    }

    public EntityPermutationResult GetMeshGroupsForState(Dictionary<uint, uint> state)
    {
        var matches = new HashSet<int>();

        for (int i = 0; i < Reader.Unk38.Count; i++)
        {
            var pairs = Reader.Unk38[i].Unk8;
            if (pairs == null)
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
                if (!state.TryGetValue(keyHash, out uint hash))
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

        AppliedMeshGroups = matches;
        return new EntityPermutationResult(state, matches, -1);
    }

    public EntityPermutationResult GetMeshGroupsWithOverrides(Dictionary<uint, uint> baseState, uint? defaultPermutation, Dictionary<uint, uint> overrides)
    {
        var entries = GetStateEntries();
        var result = new HashSet<int>();

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];

            uint? entryRegion = entry.TryGetValue(RegionKey, out var regionVals) && regionVals.Count > 0 ? regionVals[0] : null;

            uint? effectivePermutation = entryRegion.HasValue
                ? (overrides.TryGetValue(entryRegion.Value, out uint overridePerm) ? overridePerm : defaultPermutation)
                : defaultPermutation;

            bool isMatch = true;
            foreach (var (keyHash, allowedValues) in entry)
            {
                uint? desired = keyHash == PermutationKey ? effectivePermutation : baseState.TryGetValue(keyHash, out uint v) ? v : (uint?)null;

                if (!desired.HasValue)
                    continue;

                if (!allowedValues.Contains(desired.Value))
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
                result.Add(i);
        }

        AppliedMeshGroups = result;
        return new EntityPermutationResult(baseState, result, -1);
    }

    private Dictionary<uint, List<uint>> _cachedDistinctKeyValues;
    public Dictionary<uint, List<uint>> GetDistinctKeyValues()
    {
        if (_cachedDistinctKeyValues != null)
            return _cachedDistinctKeyValues;

        var dict = new Dictionary<uint, HashSet<uint>>();
        foreach (var entries in GetStateEntries())
        {
            foreach (var (key, values) in entries)
            {
                if (!dict.TryGetValue(key, out var set))
                    dict[key] = set = new HashSet<uint>();
                foreach (var v in values) set.Add(v);
            }
        }


        return _cachedDistinctKeyValues = dict.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
    }

    private Dictionary<uint, List<uint>>[] _cachedEntries;
    private Dictionary<uint, List<uint>>[] GetStateEntries()
    {
        if (_cachedEntries != null)
            return _cachedEntries;

        var unk38 = Reader.Unk38;
        var result = new Dictionary<uint, List<uint>>[unk38.Count];
        for (int i = 0; i < unk38.Count; i++)
        {
            var conditionsByKey = new Dictionary<uint, List<uint>>();
            var pairs = unk38[i].Unk8;
            if (pairs != null)
            {
                foreach (var pair in pairs)
                {
                    uint key = pair.SwitchKey.Hash32, value = pair.Value.Hash32;
                    if (!conditionsByKey.TryGetValue(key, out var values))
                        conditionsByKey[key] = values = new List<uint>();
                    values.Add(value);
                }
            }
            result[i] = conditionsByKey;
        }
        return _cachedEntries = result;
    }

    public static Dictionary<uint, uint> LoadOverrideState(List<(uint, uint)> pairs)
    {
        var state = new Dictionary<uint, uint>(pairs.Count);
        foreach (var (key, value) in pairs)
            state[key] = value;
        return state;
    }

    private static Dictionary<uint, uint> LoadOverrideFromMapData(FileHash mapDataTag)
    {
        var map = FileResourcer.Get().GetSchemaTag<SMapDataTable>(mapDataTag);
        using TigerReader reader = map.GetReader();
        var resource = (S80808019)map.TagData.DataEntries[0].DataResource.GetValue(reader);

        S80802AB2 overrides = default;
        if (resource.Unk00.GetValue(reader) is S80803FFD a)
            overrides = (S80802AB2)a.Unk00.GetValue(reader);
        else if (resource.Unk00.GetValue(reader) is S80802AB2 b)
            overrides = b;

        var state = new Dictionary<uint, uint>();
        if (overrides.Overrides != null)
        {
            foreach (var pair in overrides.Overrides)
                state[pair.SwitchKey] = pair.Value;
        }
        return state;
    }
}

public sealed class StateCombination
{
    public Dictionary<uint, uint> State { get; }
    public HashSet<int> Groups { get; }

    public StateCombination(Dictionary<uint, uint> state, HashSet<int> groups)
    {
        State = state;
        Groups = groups;
    }
}

public sealed class EntityPermutationResult
{
    public Dictionary<uint, uint> State { get; }
    public HashSet<int> MeshGroups { get; }
    public int? MaterialPermutationIndex { get; }

    public EntityPermutationResult(
        Dictionary<uint, uint> state,
        HashSet<int> meshGroups,
        int? materialPermutationIndex)
    {
        State = state;
        MeshGroups = meshGroups;
        MaterialPermutationIndex = materialPermutationIndex;
    }
}

public class MaterialPermutation
{
    public Dictionary<uint, uint> Configuration { get; set; }
    public SortedDictionary<uint, HashSet<uint>> Keys { get; set; } = new();
    public Dictionary<List<(uint, uint)>, int> PairsToPermutation = new(new ListTupleComparer());
    public int OverrideIndex { get; set; } = -1;

    public IEnumerable<(uint Key, HashSet<uint> Values)> IterateKeys()
    {
        foreach (var kv in Keys)
            yield return (kv.Key, kv.Value);
    }

    public static void UpdateConfiguration(MaterialPermutation permutations, Dictionary<uint, uint> desiredValues)
    {
        permutations.Configuration ??= new Dictionary<uint, uint>();

        bool changed = false;

        foreach (var kv in permutations.Keys)
        {
            if (!desiredValues.TryGetValue(kv.Key, out uint desiredValue))
                continue;

            if (!kv.Value.Contains(desiredValue))
                continue;

            if (!permutations.Configuration.TryGetValue(kv.Key, out uint oldVal) || oldVal != desiredValue)
            {
                permutations.Configuration[kv.Key] = desiredValue;
                changed = true;
            }
        }

        if (changed)
            permutations._dirty = true;
    }

    public int? CalculatePermutationIndex()
    {
        if (Configuration is null || Configuration.Count == 0)
            return null;

        var all = Configuration.OrderBy(kv => kv.Key).Select(kv => (kv.Key, kv.Value)).ToList();

        for (int size = all.Count; size >= 1; size--)
        {
            foreach (var subset in Combinations(all, size))
            {
                if (PairsToPermutation.TryGetValue(subset, out var index))
                    return index;
            }
        }

        return null;
    }

    private List<(uint, uint)> _cachedKeyVals;
    private int? _cachedResult;
    public bool _dirty = true;
    public int? CalculatePermutationIndexFast()
    {
        if (Configuration == null)
            return null;

        if (_dirty)
        {
            _cachedResult = CalculatePermutationIndex();
            _dirty = false;
        }

        return _cachedResult;
    }

    private static IEnumerable<List<(uint, uint)>> Combinations(List<(uint, uint)> items, int size)
    {
        int n = items.Count;
        if (size > n) yield break;

        var idx = Enumerable.Range(0, size).ToArray();
        while (true)
        {
            yield return idx.Select(i => items[i]).ToList();

            int i = size - 1;
            while (i >= 0 && idx[i] == i + n - size)
                i--;

            if (i < 0) yield break;

            idx[i]++;
            for (int j = i + 1; j < size; j++)
                idx[j] = idx[j - 1] + 1;
        }
    }
}

public class ListTupleComparer : IEqualityComparer<List<(uint, uint)>>
{
    public bool Equals(List<(uint, uint)> x, List<(uint, uint)> y)
    {
        if (x == null || y == null || x.Count != y.Count)
            return false;

        for (int i = 0; i < x.Count; i++)
            if (x[i] != y[i])
                return false;

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
