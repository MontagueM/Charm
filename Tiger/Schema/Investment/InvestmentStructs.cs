﻿using Tiger.Schema.Entity;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Investment;

/// <summary>
/// Stores all the inventory item definitions in a huge hashmap.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "33198080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "97798080", 0x18)]
public struct D2Class_97798080
{
    public long FileSize;
    public DynamicArrayUnloaded<D2Class_9B798080> InventoryItemDefinitionEntries;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "BD168080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "9B798080", 0x20)]
public struct D2Class_9B798080
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10), NoLoad]
    public InventoryItem InventoryItem;  // I don't want to parse all these, should be Tag<D2Class_9D798080>, todo revisit this
}

#region InventoryItemDefinition

/// <summary>
/// Inventory item definition.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "06188080", 0x9C)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "9D798080", 0x120)]
public struct D2Class_9D798080
{
    public long FileSize;
    public ResourcePointer Unk08;  // D2Class_E4768080, 16198080 D1
    [SchemaField(0x18)]
    public ResourcePointer Unk18;  // D2Class_E7778080, 06178080 D1

    [SchemaField(0x48, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)] // probably not obsolete, just dont care
    public ResourcePointer Unk48;  // 15108080 D1

    [SchemaField(0x50)]
    public ResourcePointer Unk50; // 8B178080 D1

    [SchemaField(0x58, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public ResourcePointer Unk78;  // D2Class_81738080, BD178080 D1

    //[SchemaField(0x88, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    //public ResourcePointer Unk88;  // D2Class_7F738080

    [SchemaField(0x60, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x90, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public ResourcePointer Unk90;  // D2Class_77738080

    [SchemaField(0x78, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xA8, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash InventoryItemHash;
    public TigerHash UnkAC;

    [SchemaField(0x8A, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xC2, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public byte ItemRarity; //Not sure

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x108, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public short SummaryItemIndex;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x110, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<D2Class_05798080> TraitIndices;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "15108080", 0x1C)]
public struct S15108080
{
    public DynamicArray<S13108080> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "13108080", 0x2)]
public struct S13108080
{
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
/// "translationBlock" from API, "equippingBlock" in D1
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "20108080", 0x68)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "77738080", 0x60)]
public struct D2Class_77738080
{
    // D1 has "customDyeExpression" at 0x40 but idk what its used for

    public DynamicArrayUnloaded<D2Class_7D738080> Arrangements;  // "arrangements" from API

    [SchemaField(0x50, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<D2Class_7B738080> CustomDyes;  // "customDyes" from API

    [SchemaField(0x30, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x38, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<D2Class_7B738080> DefaultDyes;  // "defaultDyes" from API

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<D2Class_7B738080> LockedDyes;  // "lockedDyes" from API

    [SchemaField(0x60, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x58, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public short WeaponPatternIndex;  // "weaponPatternHash" from API, "weaponSandboxPatternIndex" in D1
}

/// <summary>
/// "arrangement" from API
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "1A108080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "7D738080", 4)]
public struct D2Class_7D738080
{
    public short ClassHash;  // "classHash" from API
    public short ArtArrangementHash;  // "artArrangementHash" from API, "gearArtArrangementIndex" in D1
}

/// <summary>
/// "lockedDyes" from API
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "1C108080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "7B738080", 4)]
public struct D2Class_7B738080
{
    public short ChannelIndex;  // "channelHash" from API
    public short DyeIndex;  // "dyeHash" from API
}

#endregion

#region String Stuff

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C7348080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "99548080", 0x18)]
public struct D2Class_99548080
{
    public long FileSize;
    public DynamicArrayUnloaded<D2Class_9D548080> StringThings;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C7348080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "9D548080", 0x20)]
public struct D2Class_9D548080
{
    public TigerHash ApiHash;

    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag<D2Class_9F548080> StringThingROI;

    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307), Tag64]
    public Tag<D2Class_9F548080> StringThing;

    public Tag<D2Class_9F548080> GetStringThing()
    {
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
            return StringThingROI;
        else
            return StringThing;
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "84348080", 0xB4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "9F548080", 0x130)]
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

    [SchemaField(0x60, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag<D2Class_B83E8080> IconContainer;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x88, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public short IconIndex;
    public short Unk8A;

    [SchemaField(0x78, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x8C, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public StringIndexReference ItemName;  // "displayProperties" -> "name"

    [SchemaField(0x80, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x98, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public StringIndexReference ItemType;  // "itemTypeDisplayName"

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xA0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public StringIndexReference ItemUnkA0; // "displaySource"?

    [SchemaField(0x88, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xB0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public StringIndexReference ItemFlavourText;  // "flavorText"

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<D2Class_F1598080> UnkB8;

    public TigerHash UnkC8;  // "bucketTypeHash" / "equipmentSlotTypeHash"
    public TigerHash UnkCC;  // DestinySandboxPatternDefinition hash
    public TigerHash UnkD0;  // DestinySandboxPatternDefinition hash

    // ive missed lots of stuff here

    [SchemaField(0x120, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
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
    public DynamicArrayUnloaded<D2Class_ED6F8080> ArtArrangementHashes;
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
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D7348080", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "CE558080", 0x28)]
public struct D2Class_CE558080
{
    public long FileSize;
    public DynamicArrayUnloaded<D2Class_D4558080> ArtArrangementEntityAssignments;
    // [DestinyField(FieldType.TablePointer)]
    // public DynamicArray<D2Class_D8558080> FinalAssignment;  // this is not needed as the above table has resource pointers
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "33348080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "D4558080", 0x20)]
public struct D2Class_D4558080
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash ArtArrangementHash;

    [SchemaField(0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash MasculineSingleEntityAssignment; // things like armour only have 1 entity, so can skip the jumps
    public TigerHash FeminineSingleEntityAssignment;

    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<D2Class_D7558080> MultipleEntityAssignments;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "635D8080", 8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "D7558080", 8)]
public struct D2Class_D7558080
{
    public ResourceInTablePointer<D2Class_D8558080> EntityAssignmentResource;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C1338080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "D8558080", 0x18)]
public struct D2Class_D8558080
{
    public long Unk00;
    public DynamicArray<D2Class_DA558080> EntityAssignments;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "A3338080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "DA558080", 4)]
public struct D2Class_DA558080
{
    public TigerHash EntityAssignmentHash;
}

/// <summary>
/// The "final" assignment map of assignment hash : entity hash
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "AA3A8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "434F8080", 0x18)]
public struct D2Class_434F8080
{
    public long FileSize;
    // This is large but kept as a DynamicArray so we can perform binary searches... todo implement binary search for DynamicArray
    // We could do binary searches... or we could not and transform into a dictionary
    public DynamicArrayUnloaded<D2Class_454F8080> EntityArrangementMap;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "A93A8080", 8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "454F8080", 8)]
public struct D2Class_454F8080 : IComparer<D2Class_454F8080>
{
    public TigerHash AssignmentHash;
    [NoLoad]
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
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "41038080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "8C978080", 0x28)]
public struct D2Class_8C978080
{
    public long FileSize;
    public DynamicArrayUnloaded<D2Class_0F878080> AssignmentBSL;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<D2Class_0B008080> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D7058080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "0F878080", 0x18)]
public struct D2Class_0F878080 : IComparer<D2Class_0F878080>
{
    public TigerHash ApiHash;
    [SchemaField(0x4, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public FileHash EntityRelationHashROI;

    [SchemaField(0x8, TigerStrategy.DESTINY2_WITCHQUEEN_6307), Tag64]
    public FileHash EntityRelationHash;  // can be entity or smth else, if SandboxPattern is entity if ArtDyeReference idk

    public int Compare(D2Class_0F878080 x, D2Class_0F878080 y)
    {
        if (x.ApiHash.Equals(y.ApiHash)) return 0;
        return x.ApiHash.CompareTo(y.ApiHash);
    }

    public FileHash GetEntityRelationHash()
    {
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
            return EntityRelationHashROI;
        else
            return EntityRelationHash;
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "9A338080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "AA528080", 0x18)]
public struct D2Class_AA528080
{
    public long FileSize;
    public DynamicArrayUnloaded<D2Class_AE528080> SandboxPatternGlobalTagId;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "BC338080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "AE528080", 0x30)]
public struct D2Class_AE528080
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash PatternHash;  // "patternHash" from API

    [SchemaField(0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x4, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash PatternGlobalTagIdHash;  // "patternGlobalTagIdHash" from API

    [SchemaField(0x4, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash WeaponContentGroupHash; // "weaponContentGroupHash" from API
    public TigerHash WeaponTypeHash; // "weaponTypeHash" from API
    // filters are also in here but idc
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x18)] // Non-8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "A36F8080", 0x18)]
public struct D2Class_A36F8080
{
    public long FileSize;
    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public FileHash EntityDataROI;

    [SchemaField(8, TigerStrategy.DESTINY2_WITCHQUEEN_6307), Tag64]
    public FileHash EntityData;  // can be entity, can be audio group for entity

    public FileHash GetEntityData()
    {
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
            return EntityDataROI;
        else
            return EntityData;
    }
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
    public DynamicArrayUnloaded<D2Class_075A8080> InventoryItemIconsMap;
}

[SchemaStruct("075A8080", 0x20)]
public struct D2Class_075A8080
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10), Tag64]
    public Tag<D2Class_B83E8080> IconContainer;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "97278080", 0x80)] // Non-8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B83E8080", 0x80)]
public struct D2Class_B83E8080
{
    public long FileSize;
    [SchemaField(0x10)]
    public TigerHash Unk10;
    public Tag<D2Class_CF3E8080> IconPrimaryContainer;

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)] // Unsure
    [SchemaField(0x18, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag<D2Class_CF3E8080> IconAdContainer; //Eververse item advertisement

    [SchemaField(0x24, TigerStrategy.DESTINY1_RISE_OF_IRON)] // Icon dyemap?
    [SchemaField(0x1C, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag<D2Class_CF3E8080> IconBGOverlayContainer;

    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag<D2Class_CF3E8080> IconBackgroundContainer;

    [SchemaField(0x1C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x24, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag<D2Class_CF3E8080> IconOverlayContainer;
    //public Tag Unk28; //Always null?
    //public Tag EmblemContainer; //For Emblems, not worth loading atm
}


[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "70208080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "CF3E8080", 0x18)]
public struct D2Class_CF3E8080
{
    public long FileSize;
    [SchemaField(0x10)]
    public ResourcePointer Unk10;  // cd3e8080, CD298080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CD298080", 0x1C)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "CD3E8080", 0x20)]
public struct D2Class_CD3E8080
{
    public DynamicArrayUnloaded<D2Class_D23E8080> Unk00;
}

[SchemaStruct("CB3E8080", 0x20)]
public struct D2Class_CB3E8080
{
    public DynamicArrayUnloaded<D2Class_D03E8080> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "78248080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "D23E8080", 0x10)]
public struct D2Class_D23E8080
{
    public DynamicArrayUnloaded<D2Class_D53E8080> TextureList;
}

[SchemaStruct("D03E8080", 0x10)]
public struct D2Class_D03E8080
{
    public DynamicArrayUnloaded<D2Class_D43E8080> TextureList;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "992B8080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "D53E8080", 4)]
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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "20348080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "C2558080", 0x18)]
public struct D2Class_C2558080
{
    public long FileSize;
    public DynamicArrayUnloaded<D2Class_C6558080> ArtDyeReferences;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CC338080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "C6558080", 8)]
public struct D2Class_C6558080
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash ArtDyeHash;
    [SchemaField(0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(4, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "F6178080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "DE5B8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "F2518080", 0x18)]
public struct SDyeChannels
{
    public long FileSize;
    public DynamicArrayUnloaded<SDyeChannelHash> ChannelHashes;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "21188080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "E25B8080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "2C4F8080", 4)]
public struct SDyeChannelHash
{
    public TigerHash ChannelHash;
}


#endregion

#region String container hash + indexmap

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CB348080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "095A8080", 0x18)]
public struct D2Class_095A8080
{
    public long FileSize;
    public DynamicArrayUnloaded<D2Class_0E5A8080> StringContainerMap;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "73348080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "0E5A8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "0E5A8080", 0x20)]
public struct D2Class_0E5A8080
{
    public TigerHash BankFnvHash;  // some kind of name for the bank

    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public LocalizedStrings LocalizedStringsROI;

    [SchemaField(0x8, TigerStrategy.DESTINY2_WITCHQUEEN_6307), Tag64, NoLoad]
    public LocalizedStrings LocalizedStrings;
    public short Unk18;

    public LocalizedStrings GetLocalizedStrings()
    {
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
            return LocalizedStringsROI;
        else
            return LocalizedStrings;
    }
}

#endregion

#region Destiny 1 API stuff

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "BD178080", 0x4)]
public struct SBD178080
{
    public short TalenGridIndex; // "talentGridHash" from API
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C2188080", 0x18)]
public struct SC2188080
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<SCB178080> TalentGridEntries;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CB178080", 0x18)]
public struct SCB178080
{
    public TigerHash TalentGridHash;
    [SchemaField(0x10)]
    public Tag<S63198080> TalentGrid;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C2188080", 0x38)]
public struct S63198080
{
    [SchemaField(0x10)]
    public DynamicArray<S28178080> Nodes;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "28178080", 0x40)]
public struct S28178080
{
    public TigerHash NodeHash; // ??
    [SchemaField(0x18)]
    public DynamicArray<S58178080> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "58178080", 0x90)]
public struct S58178080
{
    public DynamicArray<SDE168080> Unk00;
    [SchemaField(0x20)]
    public DynamicArray<SF1458080> Unk20;
    public TigerHash Unk30;
    public int Unk34;
    [SchemaField(0x70)]
    public DynamicArray<S940F8080> Unk70;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "DE168080", 0x50)]
public struct SDE168080
{
    public DynamicArray<SE8188080> Unk00;
    public DynamicArray<S87178080> Unk10;
    public DynamicArray<S28468080> Unk20;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "F1458080", 0x2)]
public struct SF1458080
{
    public short Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "940F8080", 0x4)]
public struct S940F8080
{
    public short Unk00; // socketTypeHash?
    public short PlugItemIndex; // plugItemHash
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "E8188080", 0x10)]
public struct SE8188080
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "87178080", 0x10)]
public struct S87178080
{
    public int Unk00;
    public float Unk04; // min value?
    public float Unk08; // max value?
    public byte Unk0C; // index?
    public byte Unk0D;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "28468080", 0x4)]
public struct S28468080
{
}

#endregion
