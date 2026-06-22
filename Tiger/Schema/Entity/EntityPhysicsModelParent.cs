namespace Tiger.Schema.Entity;

public class EntityPhysicsModelParent : EntityComponent
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

    public EntityPhysicsModelParent(FileHash resource) : base(resource)
    {
    }

    public S80806D6C Reader => ((S80806D6C)TagData.Unk18.GetValue(GetReader()));

    public EntityModel GetModel()
    {
        return Reader.PhysicsModel;
    }

    public AABB GetBoundingBox()
    {
        return Reader.BoundingBox;
    }

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

