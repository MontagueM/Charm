using System.Collections.Concurrent;
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


    protected override void Initialise()
    {
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            var vals = PackageResourcer.Get().GetAllHashes<S50058080>();
            Parallel.ForEach(vals, val =>
            {
                var tag = FileResourcer.Get().GetSchemaTag<S50058080>(val);
                AddStrings(tag.TagData.CharacterNames);
                AddStrings(tag.TagData.ActivityGlobalStrings);
            });
        }
        // surely this is fine..
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402)
        {
            var vals = PackageResourcer.Get().GetAllHashes<D2Class_02218080>(); //TODO: Beyond Light
            Parallel.ForEach(vals, val =>
            {
                var tag = FileResourcer.Get().GetSchemaTag<D2Class_02218080>(val);
                foreach (var entry in tag.TagData.Unk28)
                {
                    if (entry.Unk10 is not null && entry.Unk10.Hash.GetReferenceHash() == 0x808099EF) // EF998080
                    {
                        AddStrings(FileResourcer.Get().GetFile<LocalizedStrings>(entry.Unk10.Hash));
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

    public string GetString(StringHash hash)
    {
        if (_strings.TryGetValue(hash, out List<StringBiasView>? sv))
        {
            if (!_localizedStringsBias.IsEmpty)
            {
                var bias = sv.Find(s => _localizedStringsBias.Contains(s.ContainerHash));
                if (!string.IsNullOrEmpty(bias.String))
                {
                    return bias.String;
                }
            }

            return sv[0].String;
        }

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
