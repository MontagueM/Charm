namespace Tiger.Schema.Entity;

public class EntityAnimations : EntityComponent
{
    public HashSet<Animation.Animation> AnimationTags = new();
    public EntityAnimations(FileHash resource) : base(resource)
    {
    }

    public void GetAllAnimations()
    {
        if (GetUnk18() is not S808025F8 resource)
            return;

        foreach (var entry in resource.Container.TagData.Clips)
        {
            if (entry.Animation is null)
                continue;

            AnimationTags.Add(entry.Animation);
        }

        foreach (var entry in resource.Container.TagData.Clips2)
        {
            if (entry.Animation is null)
                continue;

            AnimationTags.Add(entry.Animation);
        }

        if (resource.Container.TagData.UnkExtraClips != null)
        {
            foreach (var entry in resource.Container.TagData.UnkExtraClips.TagData.Unk08)
            {
                foreach (var entry2 in entry.Unk10)
                {
                    if (entry2.Animation is null)
                        continue;

                    AnimationTags.Add(entry2.Animation);
                }
                foreach (var entry2 in entry.Unk20)
                {
                    if (entry2.Animation is null)
                        continue;

                    AnimationTags.Add(entry2.Animation);
                }
            }
        }
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803466, 0x700)] //66348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80802613, 0x830)] //13268080, component 0x8
public struct S80802613
{
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080344B, 0x120)] //4B348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808025F8, 0x130)] //F8258080
public struct S808025F8
{
    [SchemaField(0x90, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xA8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<S8080289F> Container;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808036F6, 0x90)] //F6368080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080289F, 0x78)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080289F, 0x80)] //9F288080
public struct S8080289F
{
    [SchemaField(0x8)]
    public DynamicArray<S80808BDF> Clips;

    [SchemaField(0x58, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<S80808AEA> ClipNames; // unsure
    public DynamicArray<S80808BDE> Clips2;

    [SchemaField(0x8C, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x74, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<S80802889> UnkExtraClips;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808BDF, 0x4)] //488F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808BDF, 0x10)] //DF8B8080
public struct S80808BDF
{
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Animation.Animation Animation;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F1C, 0x4)] //1C8F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808AEA, 0x4)] //EA8A8080
public struct S80808AEA
{
    public StringHash ClipName;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808BDE, 0x20)] //02908080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808BDE, 0x30)] //DE8B8080
public struct S80808BDE
{
    [SchemaField(0x10)]
    public TigerHash Unk10;
    public float Unk14;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Animation.Animation Animation;

    //public TigerHash Unk24;
    //public int Unk28;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808036BD, 0x18)] //BD368080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80802889, 0x18)] //89288080
public struct S80802889
{
    [SchemaField(0x8)]
    public DynamicArray<S808023DF> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080323C, 0x40)] //3C328080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808023DF, 0x40)] //DF238080
public struct S808023DF
{
    public TigerHash Unk00;
    [SchemaField(0x10)]
    public DynamicArray<S8080255E> Unk10;
    public DynamicArray<S8080255D> Unk20;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808033B1, 0xC)] //B1338080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080255E, 0x18)] //5E258080
public struct S8080255E
{
    public TigerHash Unk00;
    public float Unk04;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Animation.Animation Animation;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808033B0, 0x10)] //B0338080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080255D, 0x38)] //5D258080
public struct S8080255D
{
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Animation.Animation Animation;

    public TigerHash Unk18;
}
