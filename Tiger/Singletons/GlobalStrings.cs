using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using Arithmic;
using Tiger.Schema;
using Tiger.Schema.Strings;

namespace Tiger;

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
    private ConcurrentDictionary<uint, string> _wordlistStrings { get; set; } = new();


    protected override void Initialise()
    {
        AddFromWordlist();

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
        _wordlistStrings.Clear();
    }

    private void AddFromWordlist()
    {
        if (!File.Exists("./wordlist.txt.gz"))
            return;

        Stopwatch stopwatch = Stopwatch.StartNew();
        string line;
        //using (FileStream fs = new("./wordlist.txt", FileMode.Open, FileAccess.Read, FileShare.Read, 65536, true))
        using (var fs = File.OpenRead("./wordlist.txt.gz"))
        using (var gz = new GZipStream(fs, CompressionMode.Decompress))
        using (StreamReader sr = new(gz))
        {
            while ((line = sr.ReadLine()) != null)
            {
                _wordlistStrings.TryAdd(Helpers.Fnv(line), line);
            }
        }
        stopwatch.Stop();
        Log.Info($"Parsed Wordlist: {stopwatch.ElapsedMilliseconds}ms ({_wordlistStrings.Count} lines)");
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
        else if (_wordlistStrings.TryGetValue(hash.Hash32, out string value))
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
