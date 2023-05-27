using System.Collections.Concurrent;

namespace Tiger.Schema;

public class Hash64Map : Strategy.StrategistSingleton<Hash64Map>
{
    private readonly ConcurrentDictionary<ulong, uint> _map = new();

    public Hash64Map(TigerStrategy strategy) : base(strategy)
    {
    }

    /// <summary>
    /// We assume it exists, otherwise will throw an exception
    /// </summary>
    public uint GetHash32(ulong tag64)
    {
        if (!_map.ContainsKey(tag64))
        {
            throw new KeyNotFoundException($"Hash64 {tag64}/{Endian.U64ToString(tag64)} not found in map");
        }
        return _map[tag64];
    }

    // todo race condition where
    protected override void Initialise()
    {
        // Pre-BL has no Hash64s
        if (_strategy < TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            return;
        }

        List<ushort> packageIds = PackageResourcer.Get().PackagePathsCache.GetAllPackageIds();
        Parallel.ForEach(packageIds, packageId =>
        {
            IPackage package = PackageResourcer.Get().GetPackage(packageId);
            foreach (Hash64Definition definition in package.GetHash64List())
            {
                if (definition.Hash64 == 16580767463058767872)
                {
                    var a = 0;
                }
                _map.TryAdd(definition.Hash64, definition.Hash32);
            }
        });
    }

    protected override void Reset()
    {
        _map.Clear();
    }
}
