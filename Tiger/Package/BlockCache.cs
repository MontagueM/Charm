using System.Collections.Concurrent;

namespace Tiger;

/// <summary>
/// Keeps recently decompressed package blocks in memory.
/// Small files are packed many to a block (on average ~200 files share a 256 KB block), and without this every
/// file read decrypts and decompresses its whole block again, so bulk scans repeat the same work many times over.
/// Only blocks for files that fit in a single block are cached; large multi-block files (textures, buffers) would
/// just push the useful blocks out.
/// </summary>
public static class BlockCache
{
    // Keyed by the package instance (compared by reference), not its id: packages loaded by path for an older patch
    // share the id but have a different block table, so an id-based key could hand them another patch's block.
    // Instances are also per game version, so versions can't collide either.
    private readonly record struct BlockKey(Package Package, int BlockIndex);

    private sealed class Entry
    {
        public readonly byte[] Data;
        public long LastAccess;

        public Entry(byte[] data, long lastAccess)
        {
            Data = data;
            LastAccess = lastAccess;
        }
    }

    // 1024 x 256 KB = 256 MB
    private const int MaxBlocks = 1024;
    // When full, drop this many of the least recently used blocks at once so eviction stays rare
    private const int EvictCount = MaxBlocks / 8;

    private static readonly ConcurrentDictionary<BlockKey, Entry> _blocks = new();
    private static readonly object _evictLock = new();
    private static long _clock;
    private static long _hits;
    private static long _misses;

    // Launch with -noblockcache to compare timings without the cache
    private static readonly bool _disabled = TigerInstance.Args.IsArgPresent("noblockcache");

    /// <summary>
    /// Returns the cached block, or runs <paramref name="load"/> and caches its result.
    /// The returned array is shared, so callers must only read from it.
    /// </summary>
    public static byte[] GetOrLoad(Package package, int blockIndex, Func<byte[]> load)
    {
        if (_disabled)
        {
            Interlocked.Increment(ref _misses);
            return load();
        }

        BlockKey key = new(package, blockIndex);
        if (_blocks.TryGetValue(key, out Entry? entry))
        {
            entry.LastAccess = Interlocked.Increment(ref _clock);
            Interlocked.Increment(ref _hits);
            return entry.Data;
        }

        Interlocked.Increment(ref _misses);
        // Two threads can miss the same block at once and both load it; that's rare and only costs the duplicate work
        byte[] data = load();
        _blocks[key] = new Entry(data, Interlocked.Increment(ref _clock));

        if (_blocks.Count > MaxBlocks)
        {
            Evict();
        }

        return data;
    }

    private static void Evict()
    {
        lock (_evictLock)
        {
            if (_blocks.Count <= MaxBlocks)
            {
                return;
            }

            // ToArray takes an atomic snapshot; enumerating the dictionary through LINQ isn't safe while other threads add blocks
            KeyValuePair<BlockKey, Entry>[] snapshot = _blocks.ToArray();
            // Copy the access times first, since other threads keep updating them and a sort needs stable values
            long[] lastAccess = new long[snapshot.Length];
            for (int i = 0; i < snapshot.Length; i++)
            {
                lastAccess[i] = snapshot[i].Value.LastAccess;
            }
            Array.Sort(lastAccess, snapshot);
            for (int i = 0; i < Math.Min(EvictCount, snapshot.Length); i++)
            {
                _blocks.TryRemove(snapshot[i].Key, out _);
            }
        }
    }

    public static void Clear()
    {
        _blocks.Clear();
    }

    /// <summary>
    /// Hit/miss counts since the last call, for logging how well a scan used the cache. Resets the counters.
    /// </summary>
    public static string TakeStats()
    {
        long hits = Interlocked.Exchange(ref _hits, 0);
        long misses = Interlocked.Exchange(ref _misses, 0);
        long total = hits + misses;
        double rate = total == 0 ? 0 : 100.0 * hits / total;
        return $"block cache: {hits} hits, {misses} misses ({rate:0.0}% hit rate), {_blocks.Count} blocks held";
    }
}
