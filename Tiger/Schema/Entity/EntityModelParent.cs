namespace Tiger.Schema.Entity;

public class EntityModelParent : EntityComponent
{
    public HashSet<int> AppliedMeshGroups = new();
    public int UpdateMeshGroups; // upped when a state is applied so the renderer knows to redo materials

    private EntityMaterialVariants _materialVariants;
    public EntityMaterialVariants MaterialVariants
    {
        get
        {
            if (_materialVariants is not null)
                return _materialVariants;

            using TigerReader reader = GetReader();
            _materialVariants = new EntityMaterialVariants(reader, Reader);
            return _materialVariants;
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

    public bool IsPartEnabled(int part)
    {
        return AppliedMeshGroups.Count == 0 || AppliedMeshGroups.Contains(part);
    }

    public static uint DamageKey = Helpers.Fnv1a32("damage");
    public static uint PermutationKey = Helpers.Fnv1a32("permutation");
    public static uint RegionKey = Helpers.Fnv1a32("region");
    public static uint UnitTypeKey = Helpers.Fnv1a32("unit_type");
    public static uint TypeKey = Helpers.Fnv1a32("type");
    public static uint NoneValue = Helpers.Fnv1a32("none");

    public EntityPermutationResult GetMeshGroupsForState(Dictionary<uint, uint> state)
    {
        var conditions = state.Select(x => new PartCondition(x.Key, x.Value)).ToList();
        return new EntityPermutationResult(state, ApplyConditions(conditions), -1);
    }

    public EntityPermutationResult GetMeshGroupsWithOverrides(Dictionary<uint, uint> baseState, Dictionary<uint, uint> overrides, EntityVariant? variant = null)
    {
        var conditions = baseState.Select(x => new PartCondition(x.Key, x.Value)).ToList();

        if (variant is not null)
            conditions.AddRange(variant.Labels.Select(x => new PartCondition(x.Item1, x.Item2)));

        if (GetDistinctKeyValues().TryGetValue(RegionKey, out var regions))
        {
            foreach (uint region in regions)
            {
                uint permutation;
                if (overrides.TryGetValue(region, out uint overridePermutation))
                    permutation = overridePermutation;
                else if (variant is null || !variant.RegionPermutations.TryGetValue(region, out permutation))
                    continue;

                var condition = new PartCondition(PermutationKey, permutation);
                condition.Required.Add((RegionKey, region));
                conditions.Add(condition);
            }
        }

        return new EntityPermutationResult(baseState, ApplyConditions(conditions), -1);
    }

    public HashSet<int> ApplyConditions(List<PartCondition> conditions)
    {
        var labels = GetPartLabels();
        var result = new HashSet<int>();
        for (int i = 0; i < labels.Length; i++)
        {
            if (!conditions.Any(x => x.Disables(labels[i])))
                result.Add(i);
        }

        AppliedMeshGroups = result;
        UpdateMeshGroups++;
        return result;
    }

    private HashSet<(uint, uint)>[] _cachedPartLabels;
    public HashSet<(uint, uint)>[] GetPartLabels()
    {
        if (_cachedPartLabels != null)
            return _cachedPartLabels;

        var unk38 = Reader.Unk38;
        var result = new HashSet<(uint, uint)>[unk38.Count];
        for (int i = 0; i < unk38.Count; i++)
        {
            result[i] = new HashSet<(uint, uint)>();
            var pairs = unk38[i].Unk8;
            if (pairs == null)
                continue;

            foreach (var pair in pairs)
            {
                result[i].Add((pair.SwitchKey.Hash32, pair.Value.Hash32));
            }
        }
        return _cachedPartLabels = result;
    }

    // permutations used by the parts in each region, region -> permutations
    private Dictionary<uint, List<uint>> _cachedRegionPermutations;
    public Dictionary<uint, List<uint>> GetRegionPermutations()
    {
        if (_cachedRegionPermutations != null)
            return _cachedRegionPermutations;

        var dict = new Dictionary<uint, HashSet<uint>>();
        foreach (var labels in GetPartLabels())
        {
            var permutations = labels.Where(x => x.Item1 == PermutationKey).Select(x => x.Item2).ToList();
            if (permutations.Count == 0)
                continue;

            foreach (var (key, region) in labels)
            {
                if (key != RegionKey)
                    continue;

                if (!dict.TryGetValue(region, out var set))
                    dict[region] = set = new HashSet<uint>();
                set.UnionWith(permutations);
            }
        }

        return _cachedRegionPermutations = dict.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
    }

    private Dictionary<uint, List<uint>> _cachedDistinctKeyValues;
    public Dictionary<uint, List<uint>> GetDistinctKeyValues()
    {
        if (_cachedDistinctKeyValues != null)
            return _cachedDistinctKeyValues;

        var dict = new Dictionary<uint, HashSet<uint>>();
        foreach (var labels in GetPartLabels())
        {
            foreach (var (key, value) in labels)
            {
                if (!dict.TryGetValue(key, out var set))
                    dict[key] = set = new HashSet<uint>();
                set.Add(value);
            }
        }

        return _cachedDistinctKeyValues = dict.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
    }

    public static Dictionary<uint, uint> LoadOverrideState(List<(uint, uint)> pairs)
    {
        var state = new Dictionary<uint, uint>(pairs.Count);
        foreach (var (key, value) in pairs)
        {
            state[key] = value;
        }

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

public class PartCondition
{
    public List<(uint, uint)> Required = new();
    public List<(uint, uint)> Excluded = new();

    public PartCondition()
    {
    }

    public PartCondition(uint key, uint value)
    {
        Excluded.Add((key, value));
    }

    public bool Disables(HashSet<(uint, uint)> labels)
    {
        if (!Required.All(labels.Contains))
            return false;

        return Excluded.Any(pair => labels.Any(x => x.Item1 == pair.Item1) && !labels.Contains(pair));
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
