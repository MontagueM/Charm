using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;

namespace Tiger.Schema.Entity;

public class EntityResource : Tag<D2Class_069B8080>
{
    public EntityResource(FileHash hash) : base(hash)
    {
    }

    // Used only for D1 / ROI
    public List<SMapDataEntry> CollapseIntoDataEntry()
    {
        List<SMapDataEntry> entries = new List<SMapDataEntry>();
        if (Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
            return entries;

        if (_tag.Unk10.GetValue(GetReader()) is S2E098080)
            entries.AddRange(((SDD078080)_tag.Unk18.GetValue(GetReader())).DataEntries);

        return entries;
    }
}
