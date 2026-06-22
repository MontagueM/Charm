using Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;
using Tiger.Schema.Audio;
using Tiger.Schema.Entity;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Activity.DESTINY2_SHADOWKEEP_2601;

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808091DE, 0x88)] //DE918080
public struct SActivity_SK
{
    public long FileSize;
    public Tag<S80809962> LocationNames;
    public Tag<S80809780> Unk0C;
    public DynamicArray<S80807D53> Bubbles;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80807D53, 0x10)] //537D8080
public struct S80807D53
{
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Tag64 = true)]
    public Tag<SBubbleParent> MapReference;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809962, 0x88)] //62998080
public struct S80809962
{
    public long FileSize;
    public StringPointer ActivityDevName;
    public DynamicArray<S808098C4> BubbleNames;
    public DynamicArray<S808098C2> Unk30;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808098C4, 0x18)] //C4988080
public struct S808098C4
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public StringHash BubbleName;
    public short Unk0C;
    public short ThisIndex;
    public int BubbleIndex;  // index to S80807D53 in SActivity_SK
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808098C2, 0x50)] //C2988080
public struct S808098C2
{
    public StringHash BubbleName;
    public int CumulativeOffset;
    public DynamicArray<S80809B9C> Unk08;
    public DynamicArray<S80809B9C> Unk18;
    public int Unk28;
    [SchemaField(0x48)]
    public int ThisIndex;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809B9C, 2)] //9C9B8080
public struct S80809B9C
{
    public short Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809780, 0x88)] //80978080
public struct S80809780
{
    public long FileSize;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809994, 0x88)] //94998080
public struct SUnkActivity_SK
{
    public long FileSize;
    public StringHash LocationName;  // these all have actual string hashes but have no string container given directly

    [SchemaField(0x1C)]
    public StringHash Unk1C;
    public StringHash DestinationName;
    public StringHash Unk24;
    public LocalizedStrings LocalizedStrings;

    [SchemaField(0x30)]
    public StringPointer ActivityDevName;
    public Tag DescentMusic; // 0x38

    [SchemaField(0x50)]
    public DynamicArray<S8080924D> Unk50;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080924A, 0x10)] //4A928080
public struct S8080924A
{
    public StringHash UnkLocationName;
    public StringPointer UnkLocationDevName;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080924D, 0x18)] //4D928080
public struct S8080924D
{
    public StringHash LocationName;
    [SchemaField(0x08)]
    public DynamicArray<S8080924F> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080924F, 0x4C)] //4F928080
public struct S8080924F
{
    public uint Unk00;
    public StringHash UnkName0;

    [SchemaField(0x40)]
    public StringHash UnkName1;
    public Tag Unk44;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080925B, 0x18)] //5B928080
public struct S8080925B
{
    public long FileSize;
    public StringPointer ActivityDevName;
    public uint Unk10;
    public Tag<S8080925E> Unk14;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080925E, 0x3C)] //5E928080
public struct S8080925E
{
    public long FileSize;
    public DynamicArray<S80809260> Unk08;
    public DynamicArray<S80809260> Unk18;
    public DynamicArray<S80809260> Unk28; //This sometimes crashes for some reason...?
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809260, 0x4)] //60928080
public struct S80809260
{
    public Tag<S80809462> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809462, 0x58)] //62948080
public struct S80809462
{
    public long FileSize;
    public TigerHash Unk08; //BubbleName?
    public TigerHash Unk0C; //ActivityPhaseName?

    [SchemaField(0x38)]
    public DynamicArray<S80809464> Unk38;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809464, 0x18)] //64948080
public struct S80809464
{
    [SchemaField(0x8)]
    public DynamicArray<S80809466> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809466, 0x4)] //66948080
public struct S80809466
{
    public Tag<S80809468> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809468, 0x20)] //68948080
public struct S80809468
{
    public long FileSize;
    public Tag<SMapDataTable> DataTable;
    [SchemaField(0x10)]
    public DynamicArray<S80809B13> Unk10;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809B13, 0x4)] //139B8080
public struct S80809B13
{
    public Tag<S80809B14> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809B14, 0x50)] //149B8080
public struct S80809B14
{
    [SchemaField(0xC)]
    public EntityComponent EntityComponent; // Theres another after but its always the same as this one?
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80804F4C, 0x60)] //4C4F8080 // Entity Resource 0x18
public struct S80804F4C
{
    [SchemaField(0x58)]
    public Dialogue DialogueTable;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80804F54, 0x60)] //544F8080 // Entity Resource 0x18
public struct S80804F54
{
    [SchemaField(0x5C)]
    public Tag<S80808EC7> DirectiveTable;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80804E8F, 0xA8)] //8F4E8080 // Entity Resource 0x18
public struct S80804E8F
{
    [SchemaField(0x68)]
    public DynamicArray<S80804E93> Pointers;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80804E93, 0x8)] //934E8080
public struct S80804E93
{
    public ResourcePointer Pointer;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80804E95, 0xC)] //954E8080
public struct S80804E95
{
    public WwiseSound Sound;
    public TigerHash Unk04;
    public TigerHash Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80804E94, 0x8)] //944E8080
public struct S80804E94
{
    public Tag<S80809851> Unk00;
    public TigerHash Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809851, 0x18)] //80809851
public struct S80809851
{
    [SchemaField(0x8)]
    public DynamicArray<S80808E5A> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808E5A, 0x1C)] //5A8E8080
public struct S80808E5A
{
    public WwiseSound Sound;
}
