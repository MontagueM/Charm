using System.Collections.Concurrent;

namespace Tiger;


public static class FileOrder
{
    private const int RangeSize = 128;

    public static FileHash[] Sorted(IEnumerable<FileHash> hashes)
    {
        FileHash[] sorted = hashes.ToArray();
        Array.Sort(sorted, (a, b) => a.Hash32.CompareTo(b.Hash32));
        return sorted;
    }

    public static void ParallelForEach(IEnumerable<FileHash> hashes, ParallelOptions options, Action<FileHash> body)
    {
        FileHash[] sorted = Sorted(hashes);
        if (sorted.Length == 0)
        {
            return;
        }

        Parallel.ForEach(Partitioner.Create(0, sorted.Length, RangeSize), options, range =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                body(sorted[i]);
            }
        });
    }

    public static void ParallelForEach(IEnumerable<FileHash> hashes, Action<FileHash> body)
    {
        ParallelForEach(hashes, new ParallelOptions(), body);
    }
}
