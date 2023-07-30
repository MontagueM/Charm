using Tiger.Schema.Entity;

namespace Tiger.Schema.Investment;

/// <summary>
/// Stores all the inventory item definitions in a huge hashmap.
/// </summary>
[SchemaStruct("97798080", 0x18)]
public struct D2Class_97798080
{
    public long FileSize;
    public DynamicArray<D2Class_9B798080> InventoryItemDefinitionEntries;
}

[SchemaStruct("9B798080", 0x20)]
public struct D2Class_9B798080
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10)]
    public InventoryItem InventoryItem;  // I don't want to parse all these, should be Tag<D2Class_9D798080>, todo revisit this
}

#region InventoryItemDefinition

/// <summary>
/// Inventory item definition.
/// </summary>
[SchemaStruct("9D798080", 0x120)]
public struct D2Class_9D798080
{
    public long FileSize;
    public ResourcePointer Unk08;  // D2Class_E4768080
    [SchemaField(0x18)]
    public ResourcePointer Unk18;  // D2Class_E7778080
    [SchemaField(0x50)]
    public ResourcePointer Unk50;
    [SchemaField(0x78)]
    public ResourcePointer Unk78;  // D2Class_81738080
    [SchemaField(0x88)]
    public ResourcePointer Unk88;  // D2Class_7F738080
    [SchemaField(0x90)]
    public ResourcePointer Unk90;  // D2Class_77738080
    [SchemaField(0xA8)]
    public TigerHash InventoryItemHash;
    public TigerHash UnkAC;
    [SchemaField(0x110)]
    public DynamicArray<D2Class_05798080> TraitIndices;
}

[SchemaStruct("E4768080", 0x90)]
public struct D2Class_E4768080
{
    [SchemaField(0x48)]
    public TigerHash Unk48;
    public TigerHash Unk4C;
    [SchemaField(0x54)]
    public int Unk54;
    [SchemaField(0x70)]
    public int Unk70;
    [SchemaField(0x88)]
    public float Unk88;
}

[SchemaStruct("E7778080", 0x20)]
public struct D2Class_E7778080
{
    public DynamicArray<D2Class_387A8080> Unk00;
}

[SchemaStruct("387A8080", 0x10)]
public struct D2Class_387A8080
{
    public DynamicArray<D2Class_3A7A8080> Unk00;
}

[SchemaStruct("3A7A8080", 8)]
public struct D2Class_3A7A8080
{
    public int Unk00;
    public int Unk04;
}

[SchemaStruct("DC778080", 0x70)]
public struct D2Class_DC778080
{
    [SchemaField(0x08)]
    public short Unk08;
    [SchemaField(0x60)]
    public DynamicArray<D2Class_DE778080> Unk60;
}

[SchemaStruct("DE778080", 2)]
public struct D2Class_DE778080
{
    public short Unk00;
}

[SchemaStruct("05798080", 2)]
public struct D2Class_05798080
{
    public short Unk00;
}

[SchemaStruct("81738080", 0x30)]
public struct D2Class_81738080
{
    public DynamicArray<D2Class_86738080> InvestmentStats;  // "investmentStats" from API
}

/// <summary>
/// "investmentStat" from API
/// </summary>
[SchemaStruct("86738080", 0x28)]
public struct D2Class_86738080
{
    public int StatTypeIndex;  // "statTypeHash" from API
    public int Value;  // "value" from API
}

[SchemaStruct("7F738080", 2)]
public struct D2Class_7F738080
{
    public short Unk00;
}


/// <summary>
/// "translationBlock" from API
/// </summary>
[SchemaStruct("77738080", 0x60)]
public struct D2Class_77738080
{
    public DynamicArray<D2Class_7D738080> Arrangements;  // "arrangements" from API
    [SchemaField(0x28)]
    public DynamicArray<D2Class_7B738080> CustomDyes;  // "customDyes" from API
    public DynamicArray<D2Class_7B738080> DefaultDyes;  // "defaultDyes" from API
    public DynamicArray<D2Class_7B738080> LockedDyes;  // "lockedDyes" from API
    public short WeaponPatternIndex;  // "weaponPatternHash" from API
}

/// <summary>
/// "arrangement" from API
/// </summary>
[SchemaStruct("7D738080", 4)]
public struct D2Class_7D738080
{
    public short ClassHash;  // "classHash" from API
    public short ArtArrangementHash;  // "artArrangementHash" from API
}

/// <summary>
/// "lockedDyes" from API
/// </summary>
[SchemaStruct("7B738080", 4)]
public struct D2Class_7B738080
{
    public short ChannelIndex;  // "channelHash" from API
    public short DyeIndex;  // "dyeHash" from API
}

#endregion

#region String Stuff

[SchemaStruct("99548080", 0x18)]
public struct D2Class_99548080
{
    public long FileSize;
    public DynamicArray<D2Class_9D548080> StringThings;
}

[SchemaStruct("9D548080", 0x20)]
public struct D2Class_9D548080
{
    public TigerHash ApiHash;
    [SchemaField(0x10), Tag64]
    public Tag<D2Class_9F548080> StringThing;
}

[SchemaStruct("9F548080", 0x130)]
public struct D2Class_9F548080
{
    public long FileSize;
    // commented out as not useful rn
    // [DestinyField(FieldType.ResourcePointer)]
    // public dynamic? Unk08;  // D2Class_EF548080
    // [DestinyField(FieldType.ResourcePointer)]
    // public dynamic? Unk10;  // D2Class_E7548080
    // [DestinyField(FieldType.ResourcePointer)]
    // public dynamic? Unk18;
    // [DestinyField(FieldType.ResourcePointer)]
    // public dynamic? Unk20;  // D2Class_E5548080
    // [DestinyField(FieldType.ResourcePointer)]
    // public dynamic? Unk28;  // D2Class_E4548080
    // [SchemaField(0x68), DestinyField(FieldType.ResourcePointer)]
    // public dynamic? Unk68;  // D2Class_CA548080
    // [SchemaField(0x78), DestinyField(FieldType.ResourcePointer)]
    // public dynamic? Unk78;  // D2Class_B4548080

    [SchemaField(0x88)]
    public short IconIndex;
    public short Unk8A;
    public StringIndexReference ItemName;  // "displayProperties" -> "name"
    [SchemaField(0x98)]
    public StringIndexReference ItemType;  // "itemTypeDisplayName"
    [SchemaField(0xB0)]
    public StringIndexReference ItemFlavourText;  // "flavorText"
    public DynamicArray<D2Class_F1598080> UnkB8;

    public TigerHash UnkC8;  // "bucketTypeHash" / "equipmentSlotTypeHash"
    public TigerHash UnkCC;  // DestinySandboxPatternDefinition hash
    public TigerHash UnkD0;  // DestinySandboxPatternDefinition hash

    // ive missed lots of stuff here

    [SchemaField(0x120)]
    public DynamicArray<D2Class_59238080> Unk120;
}

[SchemaStruct("F1598080", 2)]
public struct D2Class_F1598080
{
    public short Unk00;
}

[SchemaStruct("59238080", 0x18)]
public struct D2Class_59238080
{
    [SchemaField(0x10)]
    public short Unk10;
    [SchemaField(0x14)]
    public TigerHash Unk14;
}


/// <summary>
/// Item destruction, includes the term "Dismantle".
/// </summary>
[SchemaStruct("EF548080", 0x1C)]
public struct D2Class_EF548080
{
    public StringIndexReference DestructionTerm;
    // some other terms, integers
}

[SchemaStruct("E7548080", 8)]
public struct D2Class_E7548080
{
    public short Unk00;
}

[SchemaStruct("E5548080", 0x28)]
public struct D2Class_E5548080
{
    public short Unk00;
    public short Unk02;
    public short Unk04;
    [SchemaField(0x8)]
    public DynamicArray<D2Class_F2598080> Unk08;
    public DynamicArray<D2Class_AE578080> Unk18;
}

[SchemaStruct("F2598080", 8)]
public struct D2Class_F2598080
{
    public short Unk00;
    [SchemaField(0x4)]
    public TigerHash Unk04;
}

[SchemaStruct("AE578080", 2)]
public struct D2Class_AE578080
{
    public short Unk00;
}

[SchemaStruct("E4548080", 8)]
public struct D2Class_E4548080
{
    public short Unk00;
    [SchemaField(0x4)]
    public TigerHash Unk04;
}

[SchemaStruct("CA548080", 0x18)]
public struct D2Class_CA548080
{
}

/// <summary>
/// Item inspection, includes the term "Details".
/// </summary>
[SchemaStruct("B4548080", 0x18)]
public struct D2Class_B4548080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0x0C)]
    public StringIndexReference InspectionTerm;
    public int Unk14;
}


#endregion

#region ArtArrangement

/// <summary>
/// Stores all the art arrangement hashes in an index-accessed DynamicArray.
/// </summary>
[SchemaStruct("F2708080", 0x18)]
public struct D2Class_F2708080
{
    public long FileSize;
    public DynamicArray<D2Class_ED6F8080> ArtArrangementHashes;
}

[SchemaStruct("ED6F8080", 4)]
public struct D2Class_ED6F8080
{
    public TigerHash ArtArrangementHash;
}

#endregion

#region ApiEntities

/// <summary>
/// Entity assignment tag header. The assignment can be accessed via the art arrangement index.
/// The file is massive so I don't auto-parse it.
/// </summary>
[SchemaStruct("CE558080", 0x28)]
public struct D2Class_CE558080
{
    public long FileSize;
    public DynamicArray<D2Class_D4558080> ArtArrangementEntityAssignments;
    // [DestinyField(FieldType.TablePointer)]
    // public DynamicArray<D2Class_D8558080> FinalAssignment;  // this is not needed as the above table has resource pointers
}

[SchemaStruct("D4558080", 0x20)]
public struct D2Class_D4558080
{
    public TigerHash ArtArrangementHash;
    [SchemaField(0x8)]
    public TigerHash MasculineSingleEntityAssignment; // things like armour only have 1 entity, so can skip the jumps
    public TigerHash FeminineSingleEntityAssignment;
    public DynamicArray<D2Class_D7558080> MultipleEntityAssignments;
}


[SchemaStruct("D7558080", 8)]
public struct D2Class_D7558080
{
    public ResourceInTablePointer<D2Class_D8558080> EntityAssignmentResource;
}

[SchemaStruct("D8558080", 0x18)]
public struct D2Class_D8558080
{
    public long Unk00;
    public DynamicArray<D2Class_DA558080> EntityAssignments;
}

[SchemaStruct("DA558080", 4)]
public struct D2Class_DA558080
{
    public TigerHash EntityAssignmentHash;
}

/// <summary>
/// The "final" assignment map of assignment hash : entity hash
/// </summary>
[SchemaStruct("434F8080", 0x18)]
public struct D2Class_434F8080
{
    public long FileSize;
    // This is large but kept as a DynamicArray so we can perform binary searches... todo implement binary search for DynamicArray
    // We could do binary searches... or we could not and transform into a dictionary
    public DynamicArray<D2Class_454F8080> EntityArrangementMap;
}

[SchemaStruct("454F8080", 8)]
public struct D2Class_454F8080 : IComparer<D2Class_454F8080>
{
    public TigerHash AssignmentHash;
    public Tag<D2Class_A36F8080> EntityParent;

    public int Compare(D2Class_454F8080 x, D2Class_454F8080 y)
    {
        if (x.AssignmentHash.Equals(y.AssignmentHash)) return 0;
        return x.AssignmentHash.CompareTo(y.AssignmentHash);
    }
}

[SchemaStruct("A44E8080", 0x38)]
public struct D2Class_A44E8080
{
    public long FileSize;
    [SchemaField(0x10), Tag64]
    public Tag<D2Class_8C978080> SandboxPatternAssignmentsTag;
    [SchemaField(0x28), Tag64]
    public Tag<D2Class_434F8080> EntityAssignmentsMap;
}

/// <summary>
/// The assignment map for api entity sandbox patterns, for things like skeletons and audio || OR art dye references
/// </summary>
[SchemaStruct("8C978080", 0x28)]
public struct D2Class_8C978080
{
    public long FileSize;
    public DynamicArray<D2Class_0F878080> AssignmentBSL;
    public DynamicArray<D2Class_0B008080> Unk18;
}

[SchemaStruct("0F878080", 0x18)]
public struct D2Class_0F878080 : IComparer<D2Class_0F878080>
{
    public TigerHash ApiHash;
    [SchemaField(0x8), Tag64]
    public FileHash EntityRelationHash;  // can be entity or smth else, if SandboxPattern is entity if ArtDyeReference idk

    public int Compare(D2Class_0F878080 x, D2Class_0F878080 y)
    {
        if (x.ApiHash.Equals(y.ApiHash)) return 0;
        return x.ApiHash.CompareTo(y.ApiHash);
    }
}

[SchemaStruct("AA528080", 0x18)]
public struct D2Class_AA528080
{
    public long FileSize;
    public DynamicArray<D2Class_AE528080> SandboxPatternGlobalTagId;
}

[SchemaStruct("AE528080", 0x30)]
public struct D2Class_AE528080
{
    public TigerHash PatternHash;  // "patternHash" from API
    public TigerHash PatternGlobalTagIdHash;  // "patternGlobalTagIdHash" from API

    [SchemaField(0x10)]
    public TigerHash WeaponContentGroupHash; // "weaponContentGroupHash" from API
    public TigerHash WeaponTypeHash; // "weaponTypeHash" from API
    // filters are also in here but idc
}

[SchemaStruct("A36F8080", 0x18)]
public struct D2Class_A36F8080
{
    public long FileSize;
    [Tag64]
    public FileHash EntityData;  // can be entity, can be audio group for entity
}

#endregion

#region InventoryItem hashmap

[SchemaStruct("8C798080", 0x28)]
public struct D2Class_8C798080
{
    public long FileSize;
    // These tables are just placeholders, instead we transform the bytes into a dict for best performance
    public DynamicArray<D2Class_96798080> ExoticHashmap;
    public DynamicArray<D2Class_96798080> GeneralHashmap;
}

[SchemaStruct("96798080", 8)]
public struct D2Class_96798080
{
    public TigerHash ApiHash;
    public int HashIndex;
}

#endregion

#region InventoryItem Icons

[SchemaStruct("015A8080", 0x18)]
public struct D2Class_015A8080
{
    public long FileSize;
    public DynamicArray<D2Class_075A8080> InventoryItemIconsMap;
}

[SchemaStruct("075A8080", 0x20)]
public struct D2Class_075A8080
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10), Tag64]
    public Tag<D2Class_B83E8080> IconContainer;
}

[SchemaStruct("B83E8080", 0x80)]
public struct D2Class_B83E8080
{
    public long FileSize;
    [SchemaField(0x10)]
    public TigerHash Unk10;
    public Tag<D2Class_CF3E8080> IconPrimaryContainer;
    [SchemaField(0x20)]
    public Tag<D2Class_CF3E8080> IconBackgroundContainer;
    public Tag<D2Class_CF3E8080> IconOverlayContainer;
}


[SchemaStruct("CF3E8080", 0x18)]
public struct D2Class_CF3E8080
{
    public long FileSize;
    [SchemaField(0x10)]
    public ResourcePointer Unk10;  // cd3e8080, cb3e8080
}

[SchemaStruct("CD3E8080", 0x20)]
public struct D2Class_CD3E8080
{
    public DynamicArray<D2Class_D23E8080> Unk00;
}

[SchemaStruct("CB3E8080", 0x20)]
public struct D2Class_CB3E8080
{
    public DynamicArray<D2Class_D03E8080> Unk00;
}

[SchemaStruct("D23E8080", 0x10)]
public struct D2Class_D23E8080
{
    public DynamicArray<D2Class_D53E8080> TextureList;
}

[SchemaStruct("D03E8080", 0x10)]
public struct D2Class_D03E8080
{
    public DynamicArray<D2Class_D43E8080> TextureList;
}

[SchemaStruct("D53E8080", 4)]
public struct D2Class_D53E8080
{
    public Texture IconTexture;
}

[SchemaStruct("D43E8080", 4)]
public struct D2Class_D43E8080
{
    public Texture IconTexture;
}


#endregion

#region Dyes

[SchemaStruct("C2558080", 0x18)]
public struct D2Class_C2558080
{
    public long FileSize;
    public DynamicArray<D2Class_C6558080> ArtDyeReferences;
}

[SchemaStruct("C6558080", 8)]
public struct D2Class_C6558080
{
    public TigerHash ArtDyeHash;
    public TigerHash DyeManifestHash;
}

[SchemaStruct("E36C8080", 8)]
public struct D2Class_E36C8080
{
    public long FileSize;
    [SchemaField(0x0C)]
    public Dye Dye;
    // same thing + some unknown flags and info
}

[SchemaStruct("F2518080", 0x18)]
public struct D2Class_F2518080
{
    public long FileSize;
    public DynamicArray<D2Class_2C4F8080> ChannelHashes;
}

[SchemaStruct("2C4F8080", 4)]
public struct D2Class_2C4F8080
{
    public TigerHash ChannelHash;
}


#endregion

#region String container hash + indexmap

[SchemaStruct("095A8080", 0x18)]
public struct D2Class_095A8080
{
    public long FileSize;
    public DynamicArray<D2Class_0E5A8080> StringContainerMap;
}

[SchemaStruct("0E5A8080", 0x18)]
public struct D2Class_0E5A8080
{
    public TigerHash BankFnvHash;  // some kind of name for the bank
    [SchemaField(0x8), Tag64]
    public LocalizedStrings LocalizedStrings;
}

#endregion
