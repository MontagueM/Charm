using Tiger.Schema.Audio;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Entity;

// todo?
public class EntitySequencer : EntityComponent
{
    public EntitySequencer(FileHash resource) : base(resource)
    {
    }

    // todo, figure out where/how else this is used
    public List<Entity> GetSequencerEntities()
    {
        List<Entity> entities = new();
        if (GetUnk18() is S80808179 sequencer)
        {
            foreach (S808091F1 entry in sequencer.Array2)
            {
                if (entry.Unk10.GetValue(Reader) is S80808881 entry2)
                {
                    if (entry2.Entity is null)
                        continue;

                    Entity entity = FileResourcer.Get().GetFile<Entity>(entry2.Entity.Hash);
                    if (!entities.Contains(entity) && entity.HasGeometry())
                    {
                        entities.Add(entity);
                        //Just in case
                        foreach (Entity child in entity.GetEntityChildren())
                            entities.Add(child);
                    }
                }
            }
        }

        return entities;
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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801F00, 0x54)] //001F8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806B38, 0x7C)] //386B8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806640, 0x6C)] //40668080
public struct SSequenceAudioEvent
{
    public DynamicStruct<SSequenceNodeBase> Base;

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(0x50, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public WwiseSound Sound;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A6D, 0x50)] //6D1A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806F49, 0x150)] //496F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806A52, 0x130)] //526A8080
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


[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808067B9, 0x110)] //B9678080
public struct SSequenceParticleSystem
{
    public DynamicStruct<SSequenceNodeBase> Base;

    [SchemaField(0x28)]
    public DynamicArray<S808067BB> Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808067BB, 0x18)] //BB678080
public struct S808067BB
{
    [SchemaField(0x10)]
    public Tag<SParticleSystem> ParticleSystem;
}

// Particle system
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806920, 0x40)] //20698080
public struct SParticleSystem
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)] // TODO
    [SchemaField(0x14, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Material UnkMat;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag<S80806929> ModelContainer;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80806929, 0x18)] //29698080
public struct S80806929
{
    [SchemaField(0x10)]
    public DynamicArrayUnloaded<S80806F06> Models;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80806F06, 4)] //066F8080
public struct S80806F06
{
    public EntityModel Model;
}

