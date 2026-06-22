using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;

namespace Tiger.Schema.Entity;

public class EntityComponent : Tag<S80809B06>
{
    public EntityComponent(FileHash hash) : base(hash)
    {
    }

    public TigerReader Reader => GetReader();
    public dynamic GetUnk10(bool deserialize = true)
    {
        return _tag.Unk10.GetValue(Reader, deserialize);
    }

    public dynamic GetUnk18(bool deserialize = true)
    {
        return _tag.Unk18.GetValue(Reader, deserialize);
    }

    // Used only for D1 / ROI
    public List<SMapDataEntry> CollapseIntoDataEntry()
    {
        List<SMapDataEntry> entries = new();
        if (Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
            return entries;

        if (GetUnk10() is S8080092E)
            entries.AddRange(((S808007DD)GetUnk18()).DataEntries);

        return entries;
    }
}
