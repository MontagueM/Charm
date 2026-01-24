using Tiger.Schema.Audio;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Entity;

// todo?
public class EntitySequencer : EntityResource
{
    public EntitySequencer(FileHash resource) : base(resource)
    {
    }
}

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


[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "B9678080", 0x110)]
public struct SSequenceParticleSystem
{
    public DynamicStruct<SSequenceNodeBase> Base;

    [SchemaField(0x28)]
    public DynamicArray<SBB678080> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "BB678080", 0x18)]
public struct SBB678080
{
    [SchemaField(0x10)]
    public Tag<SParticleSystem> ParticleSystem;
}

// Particle system
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "20698080", 0x40)]
public struct SParticleSystem
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)] // TODO
    [SchemaField(0x14, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Material UnkMat;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag<S29698080> ModelContainer;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "29698080", 0x18)]
public struct S29698080
{
    [SchemaField(0x10)]
    public DynamicArrayUnloaded<S066F8080> Models;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "066F8080", 4)]
public struct S066F8080
{
    public EntityModel Model;
}

