using System.Collections.Concurrent;
using Tiger.Schema;
using Tiger.Schema.Strings;

namespace Tiger;

[InitializeAfter(typeof(PackageResourcer))]
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
