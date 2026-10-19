namespace Tiger.Schema.Entity;

public class EntityVariants : EntityComponent
{
    private List<EntityVariant> _variants;
    public List<EntityVariant> Variants
    {
        get
        {
            if (_variants is not null)
                return _variants;

            _variants = GetVariants();
            return _variants;
        }
    }

    public EntityVariants(FileHash resource) : base(resource)
    {
    }

    public S80802AB8 Reader => ((S80802AB8)TagData.Unk18.GetValue(GetReader()));

    public int FindVariantIndex(Dictionary<uint, uint>? state)
    {
        if (state is null || state.Count == 0)
            return -1;

        int best = -1;
        int bestMatches = 0;
        for (int i = 0; i < Variants.Count; i++)
        {
            int matches = 0;
            bool conflicts = false;
            foreach (var (key, value) in Variants[i].Labels)
            {
                if (!state.TryGetValue(key, out uint stateValue))
                    continue;

                if (stateValue != value)
                {
                    conflicts = true;
                    break;
                }
                matches++;
            }

            if (!conflicts && matches > bestMatches)
            {
                best = i;
                bestMatches = matches;
            }
        }

        return best;
    }

    private List<EntityVariant> GetVariants()
    {
        var resource = Reader;
        var regions = resource.Regions.Select(x => x.StringHash.Hash32).ToList();

        var variants = new List<EntityVariant>();
        foreach (var row in resource.Variants)
        {
            var variant = new EntityVariant
            {
                Name = row.Name.Hash32,
                Weight = row.Weight,
            };

            foreach (var pair in row.SwitchTable)
                variant.Labels.Add((pair.SwitchKey.Hash32, pair.Value.Hash32));

            // every row should have one value per region
            for (int i = 0; i < row.RegionPermutations.Count && i < regions.Count; i++)
            {
                variant.RegionPermutations[regions[i]] = row.RegionPermutations[i].StringHash.Hash32;
            }

            variants.Add(variant);
        }

        return variants;
    }
}

public class EntityVariant
{
    public uint Name;
    public float Weight;
    public List<(uint, uint)> Labels = new();
    public Dictionary<uint, uint> RegionPermutations = new(); // region -> permutation
}
