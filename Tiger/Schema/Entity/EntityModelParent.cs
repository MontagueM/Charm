namespace Tiger.Schema.Entity;

public class EntityModelParent : EntityResource
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
    }

    public EntityModelParent(FileHash resource) : base(resource)
    {
    }

    public S8F6D8080 Reader => ((S8F6D8080)TagData.Unk18.GetValue(GetReader()));

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
