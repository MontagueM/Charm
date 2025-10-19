using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using Arithmic;
using Tiger.Schema;
using Tiger.Schema.Strings;

namespace Tiger;

public static class Wordlist
{
    private static readonly Dictionary<uint, string> _strings = new Dictionary<uint, string>();
    private const int FileStreamBuffer = 1 << 20;   // 1 MiB file buffer
    private const int ReadBuffer = 1 << 16;         // 64 KiB read buffer

    public static Dictionary<uint, string> Strings => _strings;

    public static void AddFromWordlist()
    {
        if (_strings.Count > 0) // Already filled, dont fill when changing versions as its a waste of time.
            return;

        const string path = "./wordlist.txt.gz";
        if (!File.Exists(path))
        {
            Log.Info($"Wordlist not found, skipping");
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                                      bufferSize: FileStreamBuffer, options: FileOptions.SequentialScan);
        using var gz = new GZipStream(fs, CompressionMode.Decompress, leaveOpen: false);

        var pool = ArrayPool<byte>.Shared;
        byte[] readBuf = pool.Rent(ReadBuffer);
        byte[] lineBuf = pool.Rent(ReadBuffer * 4);
        int lineBufLen = 0;

        try
        {
            int bytesRead;
            while ((bytesRead = gz.Read(readBuf, 0, readBuf.Length)) > 0)
            {
                int pos = 0;
                while (pos < bytesRead)
                {
                    int nl = Array.IndexOf(readBuf, (byte)'\n', pos, bytesRead - pos);
                    if (nl == -1)
                    {
                        Helpers.EnsureCapacity(ref lineBuf, lineBufLen + (bytesRead - pos), pool);
                        Buffer.BlockCopy(readBuf, pos, lineBuf, lineBufLen, bytesRead - pos);
                        lineBufLen += bytesRead - pos;
                        break;
                    }
                    else
                    {
                        int chunkLen = nl - pos;
                        Helpers.EnsureCapacity(ref lineBuf, lineBufLen + chunkLen, pool);
                        Buffer.BlockCopy(readBuf, pos, lineBuf, lineBufLen, chunkLen);
                        lineBufLen += chunkLen;

                        int actualLen = lineBufLen;
                        if (actualLen > 0 && lineBuf[actualLen - 1] == (byte)'\r')
                            actualLen--;

                        string s = Encoding.UTF8.GetString(lineBuf, 0, actualLen);
                        uint hash = Helpers.Fnv1a32(s);
                        if (!_strings.ContainsKey(hash))
                        {
                            _strings.Add(hash, s);
                        }

                        lineBufLen = 0;
                        pos = nl + 1;
                    }
                }
            }

            if (lineBufLen > 0)
            {
                int actualLen = lineBufLen;
                if (actualLen > 0 && lineBuf[actualLen - 1] == (byte)'\r')
                    actualLen--;

                string s = Encoding.UTF8.GetString(lineBuf, 0, actualLen);
                uint hash = Helpers.Fnv1a32(s);
                if (!_strings.ContainsKey(hash))
                {
                    _strings.Add(hash, s);
                }
            }
        }
        finally
        {
            pool.Return(readBuf);
            pool.Return(lineBuf);
        }

        stopwatch.Stop();
        Log.Info($"Parsed Wordlist: {stopwatch.ElapsedMilliseconds}ms ({_strings.Count} lines)");
    }

}

[InitializeAfter(typeof(Hash64Map))]
public class GlobalStrings : Strategy.StrategistSingleton<GlobalStrings>
{
    struct StringBiasView
    {
        public string String;
        public TigerHash ContainerHash;
    }

    private readonly ConcurrentDictionary<StringHash, List<StringBiasView>> _strings = new();
    private readonly ConcurrentBag<TigerHash> _addedLocalizedStrings = new();
    private readonly ConcurrentBag<TigerHash> _localizedStringsBias = new();

    protected override void Initialise()
    {
        Wordlist.AddFromWordlist();

        if (Strategy.IsD1())
        {
            ConcurrentCollections.ConcurrentHashSet<FileHash> vals = PackageResourcer.Get().GetAllHashes<S50058080>();
            Parallel.ForEach(vals, val =>
            {
                Tag<S50058080> tag = FileResourcer.Get().GetSchemaTag<S50058080>(val);
                AddStrings(tag.TagData.CharacterNames);
                AddStrings(tag.TagData.ActivityGlobalStrings);
            });
        }
        // surely this is fine..
        else
        {
            ConcurrentCollections.ConcurrentHashSet<FileHash> vals = PackageResourcer.Get().GetAllHashes<S02218080>(); //TODO: Beyond Light
            Parallel.ForEach(vals, val =>
            {
                Tag<S02218080> tag = FileResourcer.Get().GetSchemaTag<S02218080>(val);
                foreach (S0E3C8080 entry in tag.TagData.Unk28)
                {
                    if (Strategy.IsPostBL() && entry.Unk10 is not null && entry.Unk10.Hash.GetReferenceHash() == 0x808099EF) // EF998080
                    {
                        AddStrings(FileResourcer.Get().GetFile<LocalizedStrings>(entry.Unk10.Hash));
                    }
                    else if (Strategy.IsBL() && entry.Unk00 is not null)
                    {
                        Tag<S8080760A> tag2 = FileResourcer.Get().GetSchemaTag<S8080760A>(entry.Unk00.Hash);
                        if (tag2.TagData.Container is not null && tag2.TagData.Container.Hash.GetReferenceHash() == 0x808099EF)
                            AddStrings(FileResourcer.Get().GetFile<LocalizedStrings>(tag2.TagData.Container.Hash));
                    }
                    else if (Strategy.IsPreBL() && entry.Unk00 is not null && entry.Unk00.Hash.GetReferenceHash() == 0x80809A88)
                    {
                        AddStrings(FileResourcer.Get().GetFile<LocalizedStrings>(entry.Unk00.Hash));
                    }
                }
            });
        }
    }

    protected override void Reset()
    {
        _strings.Clear();
        _localizedStringsBias.Clear();
    }

    public string GetString(TigerHash hash)
    {
        return GetString(new StringHash(hash));
    }

    public string GetString(StringHash hash)
    {
        if (_strings.TryGetValue(hash, out List<StringBiasView>? sv))
        {
            if (!_localizedStringsBias.IsEmpty)
            {
                StringBiasView bias = sv.Find(s => _localizedStringsBias.Contains(s.ContainerHash));
                if (!string.IsNullOrEmpty(bias.String))
                {
                    return bias.String;
                }
            }

            return sv[0].String;
        }
        else if (Wordlist.Strings.TryGetValue(hash.Hash32, out string value))
            return value;

        return hash;
    }

    public void AddStrings(LocalizedStrings? localizedStrings)
    {
        if (localizedStrings == null || _addedLocalizedStrings.Contains(localizedStrings.Hash))
        {
            return;
        }

        _addedLocalizedStrings.Add(localizedStrings.Hash);
        localizedStrings.GetAllStringViews().ForEach(s =>
        {
            if (!_strings.ContainsKey(s.StringHash))
            {
                _strings.TryAdd(s.StringHash, new List<StringBiasView>());
            }
            _strings[s.StringHash].Add(new StringBiasView
            {
                String = s.RawString,
                ContainerHash = localizedStrings.Hash
            });
        });
    }

    /// <summary>
    /// It's possible for strings to have clashing hashes, so we allow a bias to be added to the lookup
    /// Used with activities as they know what container the strings should come from
    /// </summary>
    public void AddLocalizedStringsBias(LocalizedStrings localizedStrings)
    {
        _localizedStringsBias.Add(localizedStrings.Hash);
    }

    public GlobalStrings(TigerStrategy strategy) : base(strategy)
    {
    }
}
