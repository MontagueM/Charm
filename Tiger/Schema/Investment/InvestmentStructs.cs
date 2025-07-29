using Tiger.Schema.Strings;

namespace Tiger.Schema.Investment;

/// <summary>
/// Stores all the inventory item definitions in a huge hashmap.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "33198080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "97798080", 0x18)]
public struct S97798080
{
    public long FileSize;
    public DynamicArrayUnloaded<S9B798080> InventoryItemDefinitionEntries;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "BD168080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "9B798080", 0x20)]
public struct S9B798080
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10), NoLoad]
    public InventoryItem InventoryItem;
}

#region InventoryItemDefinition

/// <summary>
/// Inventory item definition.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "06188080", 0x9C)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "9D798080", 0x124)]
public struct S9D798080
{
    public long FileSize;
    public ResourcePointer Unk08;  // SE4768080, 16198080 D1

    [SchemaField(0x10)]
    public ResourcePointer Unk10;  // S49298080 D2

    [SchemaField(0x18)]
    public ResourcePointer Unk18;  // SE7778080, 06178080 D1

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk28;  // SC5738080, 'gearset'

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk30;  // SB6738080, lore entry index (map CF508080 BDA1A780)

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x38, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk38;  // B0738080, 'objectives'

    [SchemaField(0x48, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk48;  // 15108080 D1, A1738080 D2 'plug'

    [SchemaField(0x50, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk50; // 8B178080 D1

    [SchemaField(0x70, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x60, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk70;  // C0778080 socketEntries

    [SchemaField(0x58, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk78;  // S81738080, BD178080 D1

    //[SchemaField(0x88, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    //public ResourcePointer Unk88;  // S7F738080

    [SchemaField(0x60, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk90;  // S77738080

    [SchemaField(0x78, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x90, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x88, TigerStrategy.DESTINY2_LATEST)]
    public TigerHash InventoryItemHash;
    public TigerHash UnkAC;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xA0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x98, TigerStrategy.DESTINY2_LATEST)]
    public byte BucketTypeIndex; // 'bucketTypeHash'

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xA1, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x99, TigerStrategy.DESTINY2_LATEST)]
    public byte RecoveryBucketIndex; // 'recoveryBucketTypeHash'

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xA2, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x9A, TigerStrategy.DESTINY2_LATEST)]
    public short RecipeItemIndex; // 'recipeItemHash'

    [SchemaField(0x8A, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xA4, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x9C, TigerStrategy.DESTINY2_LATEST)]
    public byte ItemRarity;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xA5, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x9D, TigerStrategy.DESTINY2_LATEST)]
    public bool IsInstanceItem; // 'isInstanceItem'?

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xE0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0xD8, TigerStrategy.DESTINY2_LATEST)]
    public byte SeasonIndex; // 'seasonHash', not used for gear

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xE8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0xE0, TigerStrategy.DESTINY2_LATEST)]
    public short SummaryItemIndex;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xD0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0xC8, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S05798080> TraitIndices;
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

/// <summary>
/// D2 "equippingBlock"
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "E7778080", 0x28)]
public struct SE7778080
{
    public StringHash UniqueLabel;
    public TigerHash UniqueLabelHash;

    [SchemaField(0xC)]
    public byte Attributes; // EquippingItemBlockAttributes (just 0 or 1)
    public byte EquipmentSlotTypeIndex; // 'equipmentSlotTypeHash'

    [SchemaField(0x10)]
    public short ItemSetIndex; // 'equipableItemSetHash'

    [SchemaField(0x18)]
    public DynamicArray<S387A8080> Unk00;
}

[SchemaStruct("387A8080", 0x10)]
public struct S387A8080
{
    public DynamicArray<S3A7A8080> Unk00;
}

[SchemaStruct("3A7A8080", 8)]
public struct S3A7A8080
{
    public int Unk00;
    public int Unk04;
}

// 'crafting'
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "49298080", 0x90)]
public struct S49298080
{
    public short ItemIndex; // 'outputItemHash'

    [SchemaField(0x18)]
    public DynamicArrayUnloaded<SC9778080> RequiredSocketTypes; // 'requiredSocketTypeHashes'

    [SchemaField(0x70)]
    public DynamicArrayUnloaded<S5F298080> BonusPlugs; // 'bonusPlugs'
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "C9778080", 0x2)]
public struct SC9778080
{
    public short Index;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "5F298080", 0x18)]
public struct S5F298080
{
    [SchemaField(0x12)]
    public short Index; // 'plugItemHash'
}

// 'quality'
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "DC778080", 0x70)]
public struct SDC778080
{
    [SchemaField(0x08)]
    public short ProgressionLevelRequirementIndex; // 'progressionLevelRequirementHash'

    //[SchemaField(0x10)]
    //public DynamicArray<SStringHash> InfusionCategoryHashes;

    [SchemaField(0x28)]
    public DynamicArray<S2D788080> DisplayVersionWatermarkIcons; // Unsure

    [SchemaField(0x50, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<SDE778080> Versions;
}

[SchemaStruct("2D788080", 2)]
public struct S2D788080
{
    public short IconIndex;
}

[SchemaStruct("DE778080", 2)]
public struct SDE778080
{
    public short PowerCapIndex; // 'powerCapHash' DestinyPowerCapDefinition
}

[SchemaStruct("05798080", 2)]
public struct S05798080
{
    public short Index;
}

[SchemaStruct("81738080", 0x30)]
public struct S81738080
{
    public DynamicArray<S86738080> InvestmentStats;  // "investmentStats" from API
    public DynamicArray<S87738080> Perks;  // 'perks'
}

/// <summary>
/// "investmentStat" from API
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "86738080", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "86738080", 0x30)]
public struct S86738080
{
    public int StatTypeIndex;  // "statTypeHash" from API
    public int Value;  // "value" from API
}

[SchemaStruct("87738080", 0x18)]
public struct S87738080
{
    public int PerkIndex;  // "perkHash" from API
}

[SchemaStruct("7F738080", 2)]
public struct S7F738080
{
    public short Unk00;
}

[SchemaStruct("B6738080", 0x4)]
public struct SB6738080
{
    public short LoreEntryIndex;
}

// 'gearset'
[SchemaStruct("C5738080", 0x38)]
public struct SC5738080
{
    public DynamicArray<S26908080> ItemList;
}

[SchemaStruct("26908080", 0x2)]
public struct S26908080
{
    public short ItemIndex;
}

/// <summary>
/// "translationBlock" from API, "equippingBlock" in D1
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "20108080", 0x68)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "77738080", 0x60)]
public struct S77738080
{
    // D1 has "customDyeExpression" at 0x40 but idk what its used for

    public DynamicArrayUnloaded<S7D738080> Arrangements;  // "arrangements" from API

    [SchemaField(0x50, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S7B738080> CustomDyes;  // "customDyes" from API

    [SchemaField(0x30, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x38, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S7B738080> DefaultDyes;  // "defaultDyes" from API

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S7B738080> LockedDyes;  // "lockedDyes" from API

    [SchemaField(0x60, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x58, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public short WeaponPatternIndex;  // "weaponPatternHash" from API, "weaponSandboxPatternIndex" in D1
}

/// <summary>
/// "arrangement" from API
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "1A108080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "7D738080", 4)]
public struct S7D738080
{
    public short ClassHash;  // "classHash" from API
    public short ArtArrangementHash;  // "artArrangementHash" from API, "gearArtArrangementIndex" in D1
}

/// <summary>
/// "lockedDyes" from API
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "1C108080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "7B738080", 4)]
public struct S7B738080
{
    public short ChannelIndex;  // "channelHash" from API
    public short DyeIndex;  // "dyeHash" from API
}

#endregion

#region Stats
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "BE548080", 0x18)]
public struct SBE548080
{
    public ulong FileSize;
    public DynamicArrayUnloaded<SC4548080> StatGroupDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "C4548080", 0x38)]
public struct SC4548080
{
    public TigerHash StatGroupHash;
    public short Unk04;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    [SchemaField(0x10)]
    public DynamicArray<SC8548080> ScaledStats;
    [SchemaField(0x30)]
    public int MaximumValue;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "C8548080", 0x18)]
public struct SC8548080
{
    public byte StatIndex; // 'statHash'
    public byte DisplayAsNumeric;
    public byte Unk02;
    public byte IsLinear; // not in api, means the value "isnt" interpolated? WYSIWYG
    [SchemaField(0x8)]
    public DynamicArray<S257A8080> DisplayInterpolation;

}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "257A8080", 0x8)]
public struct S257A8080
{
    public int Value;
    public int Weight;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "6B588080", 0x18)]
public struct S6B588080
{
    public ulong FileSize;
    public DynamicArrayUnloaded<S6F588080> StatDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "6F588080", 0x24)]
public struct S6F588080
{
    public TigerHash StatHash;
    public StringIndexReference StatName;
    public StringIndexReference StatDescription;
    public short StatIconIndex;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "C9798080", 0x18)]
public struct SC9798080
{
    [SchemaField(0x8)]
    public DynamicArray<SCF798080> PowerCapDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "CF798080", 0x8)]
public struct SCF798080
{
    public TigerHash PowerCapHash;
    public float PowerCap; // needs multiplied by 10 for some reason?
}
#endregion

#region String Stuff

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C7348080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "99548080", 0x18)]
public struct S99548080
{
    public long FileSize;
    public DynamicArrayUnloaded<S9D548080> StringThings;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C7348080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "9D548080", 0x20)]
public struct S9D548080
{
    public TigerHash ApiHash;

    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag<S9F548080> StringThing;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "84348080", 0xB4)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "9F548080", 0x130)]
public struct S9F548080
{
    public long FileSize;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public ResourcePointer Unk38;  // SD8548080

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public ResourcePointer Unk40;  // SD7548080

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x60, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public ResourcePointer Unk60;  // SCF548080

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public ResourcePointer Unk78;  // SB4548080

    [SchemaField(0x60, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Obsolete = true)]
    public Tag<SB83E8080> IconContainer;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public short IconIndex;
    public short FoundryIconIndex; // the banner that appears on foundry weapons (Hakke, veist, etc)
    public short EmblemContainerIndex; // Can be the emblem or foundry container post-TFS

    [SchemaField(0x78, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x80, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringIndexReference ItemName;  // "displayProperties" -> "name"

    [SchemaField(0x80, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x8C, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringIndexReference ItemType;  // "itemTypeDisplayName"

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x94, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringIndexReference ItemDisplaySource; // "displaySource"

    [SchemaField(0x88, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xA4, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringIndexReference ItemFlavourText;  // "flavorText"

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xB0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArrayUnloaded<SF1598080> UnkB8;

    public TigerHash UnkC8;  // "bucketTypeHash" / "equipmentSlotTypeHash"
    public TigerHash UnkCC;  // DestinySandboxPatternDefinition hash
    public TigerHash UnkD0;  // DestinySandboxPatternDefinition hash
    public TigerHash UnkD4;

    public DestinyTooltipStyle TooltipStyle; // 'tooltipStyle' as fnv hash
    public DestinyUIDisplayStyle DisplayStyle; // 'uiItemDisplayStyle'

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xD8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<SB2548080> TooltipNotifications;
}

[SchemaStruct("D8548080", 0x88)]
public struct SD8548080
{
    [SchemaField(0x10)]
    public DynamicArray<SDC548080> InsertionRules;
}

[SchemaStruct("DC548080", 0x8)]
public struct SDC548080
{
    public StringIndexReference FailureMessage;
}

[SchemaStruct("D7548080", 0x20)]
public struct SD7548080 // 'preview'
{
    public DestinyScreenStyle ScreenStyle; // screenStyle
    public int PreviewVendorIndex; // previewVendorHash
    public StringIndexReference PreviewActionString; // previewActionString
}

[SchemaStruct("CF548080", 0x8)]
public struct SCF548080 // 'details'
{
    public StringIndexReference DetailsActionString;
}

[SchemaStruct("B2548080", 0x20)]
public struct SB2548080
{
    [SchemaField(0x10)]
    public StringIndexReference DisplayString;
    public DestinyUIDisplayStyle DisplayStyle; // No actual strings, fnv (B4437851 = ui_display_style_item_add_on)
}

[SchemaStruct("F1598080", 2)]
public struct SF1598080
{
    public short Unk00;
}

[SchemaStruct("59238080", 0x18)]
public struct S59238080
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
public struct SEF548080
{
    public StringIndexReference DestructionTerm;
    // some other terms, integers
}

[SchemaStruct("CA548080", 0x18)]
public struct SCA548080
{
    [SchemaField(0x1)]
    public byte StatGroupIndex; // TFS Episode 2
}

/// <summary>
/// Item inspection, includes the term "Details".
/// </summary>
[SchemaStruct("B4548080", 0x18)]
public struct SB4548080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0xC)]
    public StringIndexReference InspectionTerm;
    public int StatGroupIndex;
}

[SchemaStruct("2D548080", 0x18)]
public struct S2D548080
{
    public long FileSize;
    public DynamicArrayUnloaded<S33548080> SandboxPerkDefinitionEntries;
}

[SchemaStruct("33548080", 0x28)]
public struct S33548080
{
    public TigerHash SandboxPerkHash;
    public TigerHash Unk04;
    public StringIndexReference SandboxPerkName;
    public StringIndexReference SandboxPerkDescription;
    public short IconIndex;
}

[SchemaStruct("AA768080", 0x18)]
public struct SAA768080
{
    public long FileSize;
    public DynamicArrayUnloaded<SAE7680800> SandboxPerkDefinitionEntries;
}

[SchemaStruct("AE768080", 0xC)]
public struct SAE7680800
{
    public TigerHash SandboxPerkHash;
    public int UnkIndex;
    public int Unk08;
}

#endregion

#region ArtArrangement

/// <summary>
/// Stores all the art arrangement hashes in an index-accessed DynamicArray.
/// </summary>
[SchemaStruct("F2708080", 0x18)]
public struct SF2708080
{
    public long FileSize;
    public DynamicArrayUnloaded<SED6F8080> ArtArrangementHashes;
}

[SchemaStruct("ED6F8080", 4)]
public struct SED6F8080
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
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "CE558080", 0x28)]
public struct SCE558080
{
    public long FileSize;
    public DynamicArrayUnloaded<SD4558080> ArtArrangementEntityAssignments;
    // [DestinyField(FieldType.TablePointer)]
    // public DynamicArray<SD8558080> FinalAssignment;  // this is not needed as the above table has resource pointers
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "33348080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "D4558080", 0x20)]
public struct SD4558080
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash ArtArrangementHash;

    [SchemaField(0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash MasculineSingleEntityAssignment; // things like armour only have 1 entity, so can skip the jumps
    public TigerHash FeminineSingleEntityAssignment;

    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<SD7558080> MultipleEntityAssignments;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "635D8080", 8)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "D7558080", 8)]
public struct SD7558080
{
    public ResourceInTablePointer<SD8558080> EntityAssignmentResource;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C1338080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "D8558080", 0x18)]
public struct SD8558080
{
    public long Unk00;
    public DynamicArray<SDA558080> EntityAssignments;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "A3338080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "DA558080", 4)]
public struct SDA558080
{
    public TigerHash EntityAssignmentHash;
}

/// <summary>
/// The "final" assignment map of assignment hash : entity hash
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "AA3A8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "434F8080", 0x18)]
public struct S434F8080
{
    public long FileSize;
    // This is large but kept as a DynamicArray so we can perform binary searches... todo implement binary search for DynamicArray
    // We could do binary searches... or we could not and transform into a dictionary
    public DynamicArrayUnloaded<S454F8080> EntityArrangementMap;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "A93A8080", 8)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "454F8080", 8)]
public struct S454F8080 : IComparer<S454F8080>
{
    public TigerHash AssignmentHash;
    [NoLoad]
    public Tag<SA36F8080> EntityParent;

    public int Compare(S454F8080 x, S454F8080 y)
    {
        if (x.AssignmentHash.Equals(y.AssignmentHash)) return 0;
        return x.AssignmentHash.CompareTo(y.AssignmentHash);
    }
}

[SchemaStruct("A44E8080", 0x38)]
public struct SA44E8080
{
    public long FileSize;
    [SchemaField(0x10, Tag64 = true)]
    public Tag<S8C978080> SandboxPatternAssignmentsTag;
    [SchemaField(0x28, Tag64 = true)]
    public Tag<S434F8080> EntityAssignmentsMap;
}

/// <summary>
/// The assignment map for api entity sandbox patterns, for things like skeletons and audio || OR art dye references
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "41038080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "8C978080", 0x28)]
public struct S8C978080
{
    public long FileSize;
    public DynamicArrayUnloaded<S0F878080> AssignmentBSL;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<SUInt32> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D7058080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "0F878080", 0x18)]
public struct S0F878080 : IComparer<S0F878080>
{
    public TigerHash ApiHash;

    [SchemaField(0x4, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public FileHash EntityRelationHash;  // can be entity or smth else, if SandboxPattern is entity if ArtDyeReference idk

    public int Compare(S0F878080 x, S0F878080 y)
    {
        if (x.ApiHash.Equals(y.ApiHash)) return 0;
        return x.ApiHash.CompareTo(y.ApiHash);
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "9A338080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "AA528080", 0x18)]
public struct SAA528080
{
    public long FileSize;
    public DynamicArrayUnloaded<SAE528080> SandboxPatternGlobalTagId;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "BC338080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "AE528080", 0x30)]
public struct SAE528080
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash PatternHash;  // "patternHash" from API

    [SchemaField(0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x4, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash PatternGlobalTagIdHash;  // "patternGlobalTagIdHash" from API

    [SchemaField(0x4, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash WeaponContentGroupHash; // "weaponContentGroupHash" from API
    public TigerHash WeaponTypeHash; // "weaponTypeHash" from API
    // filters are also in here but idc
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x18)] // Non-8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "A36F8080", 0x18)]
public struct SA36F8080
{
    public long FileSize;

    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public FileHash EntityData;  // can be entity, can be audio group for entity
}

#endregion

#region InventoryItem hashmap

[SchemaStruct("8C798080", 0x28)]
public struct S8C798080
{
    public long FileSize;
    // These tables are just placeholders, instead we transform the bytes into a dict for best performance
    public DynamicArray<S96798080> ExoticHashmap;
    public DynamicArray<S96798080> GeneralHashmap;
}

[SchemaStruct("96798080", 8)]
public struct S96798080
{
    public TigerHash ApiHash;
    public int HashIndex;
}

#endregion

#region InventoryItem Icons

[SchemaStruct("015A8080", 0x18)]
public struct S015A8080
{
    public long FileSize;
    public DynamicArrayUnloaded<S075A8080> InventoryItemIconsMap;
}

[SchemaStruct("075A8080", 0x20)]
public struct S075A8080
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10, Tag64 = true), NoLoad]
    public Tag<SB83E8080> IconContainer;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "97278080", 0x80)] // Non-8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "B83E8080", 0x80)]
public struct SB83E8080
{
    public long FileSize;
    [SchemaField(0x10)]
    public TigerHash Unk10;
    public Tag<SCF3E8080> IconPrimaryContainer;

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)] // Unsure
    [SchemaField(0x18, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<SCF3E8080> IconAdContainer; //Eververse item advertisement

    [SchemaField(0x24, TigerStrategy.DESTINY1_RISE_OF_IRON)] // Icon dyemap?
    [SchemaField(0x1C, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<SCF3E8080> IconBGOverlayContainer;

    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<SCF3E8080> IconBackgroundContainer;

    [SchemaField(0x1C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x24, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<SCF3E8080> IconOverlayContainer;

    [SchemaField(0x28, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<SCF3E8080> IconSpecialContainer;

    [SchemaField(0x30, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    public Vector4 DyeColorR;
    public Vector4 DyeColorG;
    public Vector4 DyeColorB;
}


[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "70208080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "CF3E8080", 0x18)]
public struct SCF3E8080
{
    public long FileSize;
    [SchemaField(0x10)]
    public ResourcePointer Unk10;  // cd3e8080, CD298080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CD298080", 0x1C)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "CD3E8080", 0x20)]
public struct SCD3E8080
{
    public DynamicArrayUnloaded<SD23E8080> Unk00;
}

[SchemaStruct("CB3E8080", 0x20)]
public struct SCB3E8080
{
    public DynamicArrayUnloaded<SD03E8080> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "78248080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "D23E8080", 0x10)]
public struct SD23E8080
{
    public DynamicArrayUnloaded<SD53E8080> TextureList;
}

[SchemaStruct("D03E8080", 0x10)]
public struct SD03E8080
{
    public DynamicArrayUnloaded<SD43E8080> TextureList;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "992B8080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "D53E8080", 4)]
public struct SD53E8080
{
    public Texture IconTexture;
}

[SchemaStruct("D43E8080", 4)]
public struct SD43E8080
{
    public Texture IconTexture;
}


#endregion

#region Dyes

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "20348080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "C2558080", 0x18)]
public struct SC2558080
{
    public long FileSize;
    public DynamicArrayUnloaded<SC6558080> ArtDyeReferences;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CC338080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "C6558080", 8)]
public struct SC6558080
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash ArtDyeHash;
    [SchemaField(0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(4, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash DyeManifestHash;
}

[SchemaStruct("E36C8080", 8)]
public struct SE36C8080
{
    public long FileSize;
    [SchemaField(0x0C)]
    public Dye Dye;
    // same thing + some unknown flags and info
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "F6178080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "F2518080", 0x18)]
public struct SDyeChannels
{
    public long FileSize;
    public DynamicArrayUnloaded<SDyeChannelHash> ChannelHashes;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "21188080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "2C4F8080", 4)]
public struct SDyeChannelHash
{
    public TigerHash ChannelHash;
}


#endregion

#region String container hash + indexmap

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CB348080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "095A8080", 0x18)]
public struct S095A8080
{
    public long FileSize;
    public DynamicArrayUnloaded<S0E5A8080> StringContainerMap;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "73348080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "0E5A8080", 0x20)]
public struct S0E5A8080
{
    public TigerHash BankFnvHash;  // some kind of name for the bank

    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true), NoLoad]
    public LocalizedStrings LocalizedStrings;

    [SchemaField(0x18, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public short Index; // Index into 26BA8080 container is LocalizedStrings is null
    public short Unk1A;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "26BA8080", 0x18)]
public struct S26BA8080
{
    public long FileSize;
    public DynamicArray<S2CBA8080> LocalizedStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "2CBA8080", 0x20)]
public struct S2CBA8080
{
    public TigerHash BankFnvHash;  // some kind of name for the bank

    [SchemaField(0x10, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true), NoLoad]
    public LocalizedStrings LocalizedStrings;
}


[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "CF508080", 0x18)]
public struct SCF508080
{
    public long FileSize;
    public DynamicArrayUnloaded<SD3508080> LoreStringMap;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "D3508080", 0x28)]
public struct SD3508080
{
    public long Unk00;
    public TigerHash LoreHash;
    public StringIndexReference LoreName;
    public StringIndexReference LoreSubtitle;
    public StringIndexReference LoreDescription;
}

#endregion

#region Socket+Plug Entries
[SchemaStruct("C0778080", 0x20)]
public struct SC0778080
{
    public DynamicArray<SC3778080> SocketEntries;
    public DynamicArray<SC8778080> IntrinsicSockets;
}

/// <summary>
/// "socketEntries" from API
/// </summary>
[SchemaStruct("C3778080", 0x58)]
public struct SC3778080
{
    public short SocketTypeIndex; // 'socketTypeHash' 
    public short Unk02;
    public short Unk04;
    public short SingleInitialItemIndex; // 'singleInitialItemHash'
    [SchemaField(0x10)]
    public short ReusablePlugSetIndex1; // randomizedPlugSetHash -> reusablePlugItems
    //[SchemaField(0x18)]
    //public DynamicArray<S3A7A8080> Unk18;
    [SchemaField(0x28)]
    public short ReusablePlugSetIndex2; // randomizedPlugSetHash -> reusablePlugItems
    [SchemaField(0x48)]
    public DynamicArray<SD5778080> PlugItems; // reusablePlugSetHash -> reusablePlugItems
}

[SchemaStruct("CD778080", 0x18)]
public struct SCD778080
{
    public long FileSize;
    public DynamicArrayUnloaded<SD3778080> PlugSetDefinitionEntries;
}

[SchemaStruct("D3778080", 0x18)]
public struct SD3778080
{
    public TigerHash PlugSetHash;
    [SchemaField(0x8)]
    public DynamicArray<SD5778080> ReusablePlugItems;
}

[SchemaStruct("D5778080", 0x40)]
public struct SD5778080
{
    [SchemaField(0x20)]
    public int PlugInventoryItemIndex;
    [SchemaField(0x28)]
    public DynamicArray<S3A7A8080>? UnkUnlocks;
}

[SchemaStruct("C8778080", 0x4)]
public struct SC8778080
{
    public short SocketTypeIndex; // socketTypeHash
    public short PlugItemIndex; // plugItemHash
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "A1738080", 0x124)]
public struct SA1738080 // 'plug'
{
    public TigerHash PlugCategoryHash;
    [SchemaField(0xE8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringHash PlugStyle; // 'uiPlugLabel', theres only none (invalid) and masterwork (6048A01E)
}

#endregion

#region Socket Category
[SchemaStruct("B6768080", 0x18)]
public struct SB6768080
{
    public long FileSize;
    public DynamicArrayUnloaded<SBA768080> SocketTypeEntries;
}

[SchemaStruct("BA768080", 0x68)]
public struct SBA768080
{
    public TigerHash SocketTypeHash;
    public short Unk04;
    public short SocketCategoryIndex; // 'socketCategoryHash'
    public int SocketVisiblity; // 'visibility'

    [SchemaField(0x30)]
    public DynamicArray<SC5768080> PlugWhitelists;
}

[SchemaStruct("C5768080", 0x8)]
public struct SC5768080
{
    public TigerHash PlugCategoryHash;
    public short Unk04;
}

[SchemaStruct("594F8080", 0x18)]
public struct S594F8080
{
    public long FileSize;
    public DynamicArrayUnloaded<S5D4F8080> SocketCategoryEntries;
}

[SchemaStruct("5D4F8080", 0x18)]
public struct S5D4F8080
{
    public TigerHash SocketCategoryHash;
    public StringIndexReference SocketName;
    public StringIndexReference SocketDescription;
    public DestinySocketCategoryStyle CategoryStyle; // 'uiCategoryStyle'
}
#endregion

#region Collectables

[SchemaStruct("28788080", 0x18)]
public struct S28788080
{
    public long FileSize;
    public DynamicArrayUnloaded<S2C788080> CollectibleDefinitionEntries;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "2C788080", 0xB0)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "2C788080", 0xC0)]
public struct S2C788080
{
    [SchemaField(0x18)]
    public DynamicArray<SF7788080> ParentNodeHashes;
    public TigerHash CollectibleHash;
    public short InventoryItemIndex;
    [SchemaField(0x30)]
    public DynamicArrayUnloaded<S3A7A8080> UnkUnlock30;
    [SchemaField(0x60)]
    public DynamicArrayUnloaded<S3A7A8080> UnkUnlockClass;
    public DynamicArrayUnloaded<S3A7A8080> Unk70;
}

[SchemaStruct("F7788080", 2)]
public struct SF7788080
{
    public short ParentNodeHashIndex;
}


[SchemaStruct("BF598080", 0x18)]
public struct SBF598080
{
    public long FileSize;
    public DynamicArrayUnloaded<SC3598080> CollectibleDefinitionStringEntries;
}

[SchemaStruct("C3598080", 0x60)]
public struct SC3598080
{
    public TigerHash CollectibleHash;
    public int IconIndex;
    public StringIndexReference CollectibleName;
    public StringIndexReference CollectibleDescription;
    [SchemaField(0x18)]
    public StringIndexReference SourceString;
    public StringIndexReference RequirementDescription;
}

#endregion

#region Objectives
// objective definition
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "3C758080", 0x18)]
public struct S3C758080
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S40758080> ObjectiveDefinitionEntries;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "40758080", 0xA8)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "40758080", 0x98)]
public struct S40758080
{
    public TigerHash ObjectiveHash;
    [SchemaField(0x14, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public int CompletionValue;
}

// objective definition strings
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "4C588080", 0x18)]
public struct S4C588080
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S50588080> ObjectiveDefinitionStringEntries;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "50588080", 0x58)]
public struct S50588080
{
    public TigerHash ObjectiveHash;
    public short IconIndex;
    [SchemaField(0x18)]
    public StringIndexReference ProgressDescription;
    public byte InProgressValueStyle; // enum DestinyUnlockValueUIStyle ?
    public byte CompletedValueStyle;
    public short LocationIndex; // 'locationHash' DestinyLocationDefinition
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "B0738080", 0x28)]
public struct SB0738080
{
    public DynamicArray<S15908080> Objectives;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "15908080", 0x2)]
public struct S15908080
{
    public short ObjectiveIndex;
}
#endregion

#region DestinyPresentationNodeDefinitions
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "D7788080", 0x18)]
public struct SD7788080
{
    [SchemaField(0x8)]
    public DynamicArray<SDB788080> PresentationNodeDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "DB788080", 0xC8)]
public struct SDB788080
{
    [SchemaField(0x18)]
    public DynamicArray<SF7788080> ParentNodes;
    [SchemaField(0x2C)]
    public int MaxCategoryRecordScore;
    [SchemaField(0x30)]
    public TigerHash Hash;
    public byte NodeType;
    public byte Scope;
    [SchemaField(0x58)]
    public short ObjectiveIndex;
    public short CompletionRecordIndex; // completionRecordHash
    [SchemaField(0x70)]
    public DynamicArray<SED788080> PresentationNodes; // children -> presentationNodes
    public DynamicArray<SEA788080> Collectibles; // children -> collectibles
    public DynamicArray<SE7788080> Records; // children -> records
    // Assuming metrics and craftables are right after as well
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "ED788080", 0x18)]
public struct SED788080
{
    public short Unk00; // nodeDisplayPriority? Always 0 in api though
    public short PresentationNodeIndex; // presentationNodeHash
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "EA788080", 0x4)]
public struct SEA788080
{
    public short Unk00;
    public short CollectableIndex; // Collectable index
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "E7788080", 0x6)]
public struct SE7788080
{
    public short Unk00;
    public short RecordDefinitionIndex; // RecordDefinition index
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "03588080", 0x18)]
public struct S03588080
{
    [SchemaField(0x8)]
    public DynamicArray<S07588080> PresentationNodeDefinitionStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "07588080", 0x2C)]
public struct S07588080
{
    public TigerHash NodeHash;
    public int IconIndex;
    public StringIndexReference Name;
    public StringIndexReference Description;
    public DestinyPresentationDisplayStyle DisplayStyle;
    public DestinyPresentationScreenStyle ScreenStyle;
}
#endregion

#region DestinyRecordDefinition
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "1F718080", 0x18)]
public struct S1F718080
{
    [SchemaField(0x8)]
    public DynamicArray<SC16F8080> RecordDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "C16F8080", 0xE8)]
public struct SC16F8080
{
    public int Unk00; // DestinyPresentationNodeType?

    [SchemaField(0x18)]
    public DynamicArray<SF7788080> ParentNodeHashes;

    [SchemaField(0x30)]
    public TigerHash Hash;
    public short LoreIndex;
    public short Unk36;
    public DynamicArray<SC96F8080> Objectives;
    public DynamicArray<SC86F8080> IntervalObjectives;

    [SchemaField(0x64)]
    public int ScoreValue;

    [SchemaField(0xCC)]
    public int GildingTrackingRecordIndex; // 'gildingTrackingRecordHash'
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "C96F8080", 0x2)]
public struct SC96F8080
{
    public short ObjectiveIndex;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "C86F8080", 0x8)]
public struct SC86F8080
{
    public short ObjectiveIndex;
    public short Unk02; // unlock, unlock value, or unlock expression mapping index...?
    public int ScoreValue; // 'intervalScoreValue'
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "87588080", 0x18)]
public struct S87588080
{
    [SchemaField(0x8)]
    public DynamicArray<S8B588080> RecordDefinitionStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "8B588080", 0x90)]
public struct S8B588080
{
    public TigerHash Hash;
    public int IconIndex;
    public StringIndexReference Name;
    public StringIndexReference Description;
    public StringIndexReference RecordTypeName;
    public StringIndexReference ObscuredName;
    public StringIndexReference ObscuredDescription;

    [SchemaField(0x40)]
    public TigerHash Unk40; // 'DestinyRecordToastStyle'?
    public bool ForTitleGilding; // 'forTitleGilding'
    public bool ShouldFireToast; // 'shouldFireToast', 98% sure

    [SchemaField(0x48)]
    public TigerHash Unk48; // 'DestinyPresentationNodeType'? or is it swapped?
    public bool ShowLargeIcons; // 'shouldShowLargeIcons'

    [SchemaField(0x50)]
    public DynamicArray<S93588080> RewardItems;
    public DynamicArray<S91588080> IntervalRewardItems;

    // 'titlesByGender'
    [SchemaField(0x80)]
    public StringIndexReference TitleName; // Male
    //public StringIndexReference TitleName; // Female
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "93588080", 0x18)]
public struct S93588080
{
    public int ItemIndex; // InventoryItem index
    public int Quantity;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "91588080", 0x10)]
public struct S91588080
{
    public DynamicArray<S93588080> Rewards;
}
#endregion

#region DestinySeasonDefinition
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "80807108", 0x18)]
public struct S80807108
{
    [SchemaField(0x8)]
    public DynamicArray<SF76F8080> SeasonDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "F76F8080", 0xA8)]
public struct SF76F8080
{
    public TigerHash SeasonHash;
    public int SeasonNumber;
    public DynamicArray<S3A7A8080> Unk08;
    public DynamicArray<SBDB38080> SeasonPassIndexes;

    [SchemaField(0x20)]
    public DynamicArray<S3A7A8080> Unk20;

    [SchemaField(0x38)] // No longer valid in EoF
    public int NumberOfActs;

    [SchemaField(0x40)] // No longer valid in EoF
    public long Act1StartTime;
    public long Act2StartTime;
    public long Act3StartTime;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "BDB38080", 0x20)]
public struct SBDB38080
{
    public int SeasonPassIndex; // 'seasonPassHash' -> DestinySeasonPassDefinition
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "80804F7E", 0x18)]
public struct S80804F7E
{
    [SchemaField(0x8)]
    public DynamicArray<S824F8080> SeasonDefinitionStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "824F8080", 0x48)]
public struct S824F8080
{
    [SchemaField(0x8)]
    public TigerHash SeasonHash;

    [SchemaField(0x10)]
    public DynamicArray<S3A7A8080> Unk10;

    public int IconIndex;
    public StringIndexReference SeasonName;
    public StringIndexReference SeasonDescription;
    public short Unk34; // index in S80805615??
}
#endregion

#region Trait Definition
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "80807900", 0x28)]
public struct S80807900
{
    [SchemaField(0x8)]
    public DynamicArray<S09798080> Traits;
    // Another table here but its the same as above but unordered with its index where Unk04 would be?
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "09798080", 0x8)]
public struct S09798080
{
    public DestinyTraitID TraitHash;
    public int Unk04; // Sometimes its index, sometimes not
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "808057F6", 0x18)]
public struct S808057F6
{
    [SchemaField(0x8)]
    public DynamicArray<SFA578080> TraitStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "FA578080", 0x1C)]
public struct SFA578080
{
    public DestinyTraitID TraitHash;
    public int IconIndex;
    public StringIndexReference TraitName;
    public StringIndexReference TraitDescription;
    public TigerHash Unk18; // always 'keyword'?
}
#endregion

#region Event/Activity/Seasonal style(?) container stuff
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "80805615", 0x18)]
public struct S80805615
{
    [SchemaField(0x8)]
    public DynamicArray<S1B568080> Entries;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "1B568080", 0x8)]
public struct S1B568080
{
    public TigerHash Unk00;
    public Tag<S80803EA5> Container;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "80803EA5", 0x70)]
public struct S80803EA5
{
    [SchemaField(0x8)]
    public TigerHash CodeName;

    public Tag<S80803EBA> Container;

    [SchemaField(Tag64 = true)]
    public LocalizedStrings Strings;

    public DynamicArray<SB73B8080> ColorSchemes;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "B73B8080", 0x20)]
public struct SB73B8080
{
    public TigerHash Type; // primary, secondary, tertiary
    [SchemaField(0x10)]
    public Vector4 Color;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "80803EBA", 0x20)]
public struct S80803EBA
{
    [SchemaField(0x8)]
    public DynamicArray<SBE3E8080> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "BE3E8080", 0x70)]
public struct SBE3E8080
{
    public TigerHash Unk00;
    public Tag<SCF3E8080> Container;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "8080B3ED", 0x18)]
public struct S8080B3ED // DestinyItemFilterDefinitions
{
    [SchemaField(0x8)]
    public DynamicArray<SC1B38080> Filters;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "C1B38080", 0x18)]
public struct SC1B38080 // DestinyItemFilterDefinitions, currently only FeaturedItems 
{
    public TigerHash FilterHash;
    [SchemaField(0x8)]
    public DynamicArray<S26908080> FilterList;
}
#endregion

#region DestinyEquipableItemSetDefinition

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "8080B44E", 0x28)]
public struct S8080B44E // DestinyEquipableItemSetDefinition
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S54B48080> ItemSetDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "54B48080", 0x28)]
public struct S54B48080
{
    public TigerHash SetHash;
    public int Unk04;

    [SchemaField(0x8)]
    public DynamicArray<S58B48080> SetItems;
    public DynamicArray<S57B48080> SetPerks;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "58B48080", 0x2)]
public struct S58B48080
{
    public short ItemIndex;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "57B48080", 0x28)]
public struct S57B48080
{
    [SchemaField(0x20)]
    public short PerkIndex;
    public short SetCount; // 'requiredSetCount'
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "8080B2C6", 0x18)]
public struct S8080B2C6 // DestinyEquipableItemSetDefinition Strings
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S7AB28080> ItemSetDefinitionStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "7AB28080", 0x28)]
public struct S7AB28080
{
    public TigerHash SetHash;
    public int IconIndex; // Maybe its actually Unk04 in the main definition?
    public StringIndexReference SetName;
    public StringIndexReference SetDescription;
    //public DynamicArray<S7CB28080> Unk18; // idk, all are zeros
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
