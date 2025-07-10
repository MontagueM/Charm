using Tiger.Schema.Audio;

namespace Tiger.Schema.Entity;

[NonSchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x20)]
[NonSchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x3C)]
[NonSchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x24)]
public struct SSequenceNodeBase
{
    [SchemaField(0x0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public TigerHash Name;
    public short Unk04;
    public short ParentIndex;

    [SchemaField(0x10)]
    public float StartTime;

    [SchemaField(0x18)]
    public float Duration;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "001F8080", 0x54)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "386B8080", 0x7C)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "40668080", 0x6C)]
public struct SSequenceAudioEvent
{
    public DynamicStruct<SSequenceNodeBase> Base;

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(0x50, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public WwiseSound Sound;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "6D1A8080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "496F8080", 0x150)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "526A8080", 0x130)]
public struct SSequenceLight
{
    public DynamicStruct<SSequenceNodeBase> Base;

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x24, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public ExpensiveLight Light;

    [SchemaField(0x30, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x50, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public float UnkFloat;
}

