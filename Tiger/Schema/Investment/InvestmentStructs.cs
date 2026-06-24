using Tiger.Schema.Strings;

namespace Tiger.Schema.Investment;

/// <summary>
/// Stores all the inventory item definitions in a huge hashmap.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801933, 0x18)] //33198080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80807997, 0x18)] //97798080
public struct S80807997
{
    public long FileSize;
    public DynamicArrayUnloaded<S8080799B> InventoryItemDefinitionEntries;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808016BD, 0x18)] //BD168080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080799B, 0x20)] //9B798080
public struct S8080799B
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10), NoLoad]
    public InventoryItem InventoryItem;
}

#region InventoryItemDefinition

/// <summary>
/// Inventory item definition.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801806, 0x9C)] //06188080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080799D, 0x124)] //9D798080
public struct S8080799D
{
    public long FileSize;
    public ResourcePointer Unk08;  // S808076E4, 16198080 D1

    [SchemaField(0x10)]
    public ResourcePointer Unk10;  // S80802949 D2

    [SchemaField(0x18)]
    public ResourcePointer Unk18;  // S808077E7, 06178080 D1

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk28;  // S808073C5, 'gearset'

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk30;  // S808073B6, lore entry index (map CF508080 BDA1A780)

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk38;  // B0738080, 'objectives'

    [SchemaField(0x48, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk48;  // 15108080 D1, A1738080 D2 'plug'

    [SchemaField(0x50, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk50; // 8B178080 D1

    [SchemaField(0x58, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk58; // 88738080 D2

    [SchemaField(0x70, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x60, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk70;  // C0778080 socketEntries

    [SchemaField(0x58, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk78;  // S80807381, BD178080 D1

    //[SchemaField(0x88, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    //public ResourcePointer Unk88;  // S8080737F

    [SchemaField(0x60, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk90;  // S80807377

    [SchemaField(0x78, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk78_EoF;  // S80807377

    [SchemaField(0x80, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk80_EoF;  // S8080757C

    [SchemaField(0x78, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x88, TigerStrategy.DESTINY2_LATEST)]
    public TigerHash InventoryItemHash;
    public TigerHash UnkAC;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x98, TigerStrategy.DESTINY2_LATEST)]
    public byte BucketTypeIndex; // 'bucketTypeHash'

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x99, TigerStrategy.DESTINY2_LATEST)]
    public byte RecoveryBucketIndex; // 'recoveryBucketTypeHash'

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x9A, TigerStrategy.DESTINY2_LATEST)]
    public short RecipeItemIndex; // 'recipeItemHash'

    [SchemaField(0x8A, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xA0, TigerStrategy.DESTINY2_LATEST)] // EoF x9C, Rng xA0
    public byte ItemRarity;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xE8, TigerStrategy.DESTINY2_LATEST)] // EoF xD8, Rng xE0?
    public byte SeasonIndex; // 'seasonHash', not used for gear

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xF0, TigerStrategy.DESTINY2_LATEST)] // EoF x9C, Rng xE8
    public short SummaryItemIndex;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xD8, TigerStrategy.DESTINY2_LATEST)] // EoF 0xC8, Renegades 0xD0, MoT 0xD8
    public DynamicArray<S80807905> TraitIndices;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801015, 0x1C)] //15108080
public struct S80801015
{
    public DynamicArray<S80801013> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801013, 0x2)] //13108080
public struct S80801013
{
}

/// <summary>
/// D2 "equippingBlock"
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808077E7, 0x28)] //E7778080
public struct S808077E7
{
    public StringHash UniqueLabel;
    public TigerHash UniqueLabelHash;

    [SchemaField(0xC)]
    public byte Attributes; // EquippingItemBlockAttributes (just 0 or 1)
    public byte EquipmentSlotTypeIndex; // 'equipmentSlotTypeHash'

    [SchemaField(0x10)]
    public short ItemSetIndex; // 'equipableItemSetHash'

    [SchemaField(0x18)]
    public DynamicArray<S80807A38> Unk00;
}

[SchemaStruct(0x80807A38, 0x10)] //387A8080
public struct S80807A38
{
    public DynamicArray<S80807A3A> Unk00;
}

[SchemaStruct(0x80807A3A, 8)] //3A7A8080
public struct S80807A3A
{
    public int Unk00;
    public int Unk04;
}

// 'crafting'
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80802949, 0x90)] //49298080
public struct S80802949
{
    public short ItemIndex; // 'outputItemHash'

    [SchemaField(0x18)]
    public DynamicArrayUnloaded<S808077C9> RequiredSocketTypes; // 'requiredSocketTypeHashes'

    [SchemaField(0x70)]
    public DynamicArrayUnloaded<S8080295F> BonusPlugs; // 'bonusPlugs'
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808077C9, 0x2)] //C9778080
public struct S808077C9
{
    public short Index;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080295F, 0x18)] //5F298080
public struct S8080295F
{
    [SchemaField(0x12)]
    public short Index; // 'plugItemHash'
}

// 'quality'
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808077DC, 0x70)] //DC778080
public struct S808077DC
{
    [SchemaField(0x08)]
    public short ProgressionLevelRequirementIndex; // 'progressionLevelRequirementHash'

    //[SchemaField(0x10)]
    //public DynamicArray<SStringHash> InfusionCategoryHashes;

    [SchemaField(0x28)]
    public DynamicArray<S8080782D> DisplayVersionWatermarkIcons; // Unsure

    [SchemaField(0x50, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S808077DE> Versions;
}

[SchemaStruct(0x8080782D, 2)] //2D788080
public struct S8080782D
{
    public short IconIndex;
}

[SchemaStruct(0x808077DE, 2)] //DE778080
public struct S808077DE
{
    public short PowerCapIndex; // 'powerCapHash' DestinyPowerCapDefinition
}

[SchemaStruct(0x80807905, 2)] //05798080
public struct S80807905
{
    public short Index;
}

[SchemaStruct(0x80807381, 0x30)] //81738080
public struct S80807381
{
    public DynamicArray<S80807386> InvestmentStats;  // "investmentStats" from API
    public DynamicArray<S80807387> Perks;  // 'perks'
}

/// <summary>
/// "investmentStat" from API
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80807386, 0x28)] //86738080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80807386, 0x40)] //86738080
public struct S80807386
{
    public int StatTypeIndex;  // "statTypeHash" from API
    public int Value;  // "value" from API
}

[SchemaStruct(0x80807387, 0x20)] //87738080
public struct S80807387
{
    public int PerkIndex;  // "perkHash" from API
}

[SchemaStruct(0x8080737F, 2)] //7F738080
public struct S8080737F
{
    public short Unk00;
}

[SchemaStruct(0x808073B6, 0x4)] //B6738080
public struct S808073B6
{
    public short LoreEntryIndex;
}

// 'gearset'
[SchemaStruct(0x808073C5, 0x38)] //C5738080
public struct S808073C5
{
    public DynamicArray<S80809026> ItemList;
}

[SchemaStruct(0x80809026, 0x2)] //26908080
public struct S80809026
{
    public short ItemIndex;
}

[SchemaStruct(0x8080B5C5, 0x2)] //C5B58080
public struct S8080B5C5
{
    public int ItemIndex;
}

/// <summary>
/// "translationBlock" from API, "equippingBlock" in D1
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801020, 0x68)] //20108080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80807377, 0x60)] //77738080
public struct S80807377
{
    // D1 has "customDyeExpression" at 0x40 but idk what its used for

    public DynamicArrayUnloaded<S8080737D> Arrangements;  // "arrangements" from API

    [SchemaField(0x50, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S8080737B> CustomDyes;  // "customDyes" from API

    [SchemaField(0x30, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x38, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S8080737B> DefaultDyes;  // "defaultDyes" from API

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S8080737B> LockedDyes;  // "lockedDyes" from API

    [SchemaField(0x60, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x58, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public short WeaponPatternIndex;  // "weaponPatternHash" from API, "weaponSandboxPatternIndex" in D1
}

/// <summary>
/// "arrangement" from API
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080101A, 4)] //1A108080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080737D, 4)] //7D738080
public struct S8080737D
{
    public short ClassHash;  // "classHash" from API
    public short ArtArrangementHash;  // "artArrangementHash" from API, "gearArtArrangementIndex" in D1
}

/// <summary>
/// "lockedDyes" from API
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080101C, 4)] //1C108080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080737B, 4)] //7B738080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080737B, 0x8)] //7B738080
public struct S8080737B // Changed to ints in MoT
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_LATEST, Obsolete = true)]
    public short ChannelIndex;  // "channelHash" from API

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_LATEST, Obsolete = true)]
    public short DyeIndex;  // "dyeHash" from API

    [SchemaField(TigerStrategy.DESTINY2_LATEST)]
    public int ChannelIndexMoT;

    [SchemaField(TigerStrategy.DESTINY2_LATEST)]
    public int DyeIndexMoT;

    public int GetChannelIndex()
    {
        return Strategy.IsLatest() ? ChannelIndexMoT : ChannelIndex;
    }

    public int GetDyeIndex()
    {
        return Strategy.IsLatest() ? DyeIndexMoT : DyeIndex;
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80807374, 0x40)] //74738080
public struct S80807374
{
    public DynamicArray<S80807A49> Unk00;

    [SchemaField(0x20)]
    public DynamicArray<S80807966> Unk20;
    public DynamicArray<S80807966> Unk30;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80807A49, 0x8)] //497A8080
public struct S80807A49
{
    public int Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80807966, 0x8)] //66798080
public struct S80807966
{
    public int Unk00;
    public int Unk04;
}

#endregion

#region Stats
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808054BE, 0x18)] //BE548080
public struct S808054BE
{
    public ulong FileSize;
    public DynamicArrayUnloaded<S808054C4> StatGroupDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808054C4, 0x38)] //C4548080
public struct S808054C4
{
    public TigerHash StatGroupHash;
    public short Unk04;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    [SchemaField(0x10)]
    public DynamicArray<S808054C8> ScaledStats;
    [SchemaField(0x30)]
    public int MaximumValue;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808054C8, 0x18)] //C8548080
public struct S808054C8
{
    public byte StatIndex; // 'statHash'
    public byte DisplayAsNumeric;
    public byte Unk02;
    public byte IsLinear; // not in api, means the value "isnt" interpolated? WYSIWYG
    [SchemaField(0x8)]
    public DynamicArray<S80807A25> DisplayInterpolation;

}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80807A25, 0x8)] //257A8080
public struct S80807A25
{
    public int Value;
    public int Weight;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080586B, 0x18)] //6B588080
public struct S8080586B
{
    public ulong FileSize;
    public DynamicArrayUnloaded<S8080586F> StatDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080586F, 0x24)] //6F588080
public struct S8080586F
{
    public TigerHash StatHash;
    public StringIndexReference StatName;
    public StringIndexReference StatDescription;
    public short StatIconIndex;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808079C9, 0x18)] //C9798080
public struct S808079C9
{
    [SchemaField(0x8)]
    public DynamicArray<S808079CF> PowerCapDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808079CF, 0x8)] //CF798080
public struct S808079CF
{
    public TigerHash PowerCapHash;
    public float PowerCap; // needs multiplied by 10 for some reason?
}
#endregion

#region String Stuff

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808034C7, 0x18)] //C7348080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80805499, 0x18)] //99548080
public struct S80805499
{
    public long FileSize;
    public DynamicArrayUnloaded<S8080549D> StringThings;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808034C7, 0x10)] //C7348080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080549D, 0x20)] //9D548080
public struct S8080549D
{
    public TigerHash ApiHash;

    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public Tag<S8080549F> StringThing;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80803484, 0xB4)] //84348080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080549F, 0x130)] //9F548080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080549F, 0x180)] //9F548080
public struct S8080549F
{
    public long FileSize;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public ResourcePointer Unk38;  // S808054D8

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public ResourcePointer Unk40;  // S808054D7

    [SchemaField(0x58, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Unk58;  // S808054D0

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x60, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public ResourcePointer Unk60;  // S808054CF

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public ResourcePointer Unk78;  // S808054B4

    [SchemaField(0x60, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Obsolete = true)]
    public Tag<S80803EB8> IconContainer;

    [SchemaField(0x70, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Obsolete = true)]
    public Tag<S80803EB8> EmblemContainer;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public int IconIndex;
    //public short FoundryIconIndex; // the banner that appears on foundry weapons (Hakke, veist, etc)
    public int EmblemContainerIndex; // Can be the emblem or foundry container post-TFS

    [SchemaField(0x78, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x80, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringIndexReference ItemName;  // "displayProperties" -> "name"

    [SchemaField(0x80, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x8C, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringIndexReference ItemType;  // "itemTypeDisplayName"

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x94, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringIndexReference ItemDescription; // "displayProperties" -> "description"

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x9C, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringIndexReference ItemDisplaySource; // "displaySource"

    [SchemaField(0x88, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xA4, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringIndexReference ItemFlavourText;  // "flavorText"

    [SchemaField(0xC0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash UnkC8;  // "bucketTypeHash" / "equipmentSlotTypeHash"
    public TigerHash UnkCC;  // DestinySandboxPatternDefinition hash
    public TigerHash UnkD0;  // DestinySandboxPatternDefinition hash
    public TigerHash UnkD4;

    public DestinyTooltipStyle TooltipStyle; // 'tooltipStyle' as fnv hash
    public DestinyUIDisplayStyle DisplayStyle; // 'uiItemDisplayStyle'

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xD8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<S808054B2> TooltipNotifications;
}

[SchemaStruct(0x808054D8, 0x88)] //D8548080
public struct S808054D8
{
    [SchemaField(0x10)]
    public DynamicArray<S808054DC> InsertionRules;
}

[SchemaStruct(0x808054DC, 0x8)] //DC548080
public struct S808054DC
{
    public StringIndexReference FailureMessage;
}

[SchemaStruct(0x808054D7, 0x20)] //D7548080
public struct S808054D7 // 'preview'
{
    public DestinyScreenStyle ScreenStyle; // screenStyle
    public int PreviewVendorIndex; // previewVendorHash
    public StringIndexReference PreviewActionString; // previewActionString
}

[SchemaStruct(0x808054CF, 0x8)] //CF548080
public struct S808054CF // 'details'
{
    public StringIndexReference DetailsActionString;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808054B2, 0x20)] //B2548080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808054B2, 0x28)] //B2548080
public struct S808054B2
{
    [SchemaField(0x10, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_LATEST)]
    public StringIndexReference DisplayString;
    public DestinyUIDisplayStyle DisplayStyle; // No actual strings, fnv (B4437851 = ui_display_style_item_add_on)
}

[SchemaStruct(0x808059F1, 2)] //F1598080
public struct S808059F1
{
    public short Unk00;
}

[SchemaStruct(0x80802359, 0x18)] //59238080
public struct S80802359
{
    [SchemaField(0x10)]
    public short Unk10;
    [SchemaField(0x14)]
    public TigerHash Unk14;
}


/// <summary>
/// Item destruction, includes the term "Dismantle".
/// </summary>
[SchemaStruct(0x808054EF, 0x1C)] //EF548080
public struct S808054EF
{
    public StringIndexReference DestructionTerm;
    // some other terms, integers
}

[SchemaStruct(0x808054CA, 0x18)] //CA548080
public struct S808054CA
{
    [SchemaField(0x1)]
    public byte StatGroupIndex; // TFS Episode 2
}

/// <summary>
/// Item inspection, includes the term "Details".
/// </summary>
[SchemaStruct(0x808054B4, 0x18)] //B4548080
public struct S808054B4
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0xC)]
    public StringIndexReference InspectionTerm;
    public int StatGroupIndex;
}

[SchemaStruct(0x8080542D, 0x18)] //2D548080
public struct S8080542D
{
    public long FileSize;
    public DynamicArrayUnloaded<S80805433> SandboxPerkDefinitionEntries;
}

[SchemaStruct(0x80805433, 0x28)] //33548080
public struct S80805433
{
    public TigerHash SandboxPerkHash;
    public TigerHash Unk04;
    public StringIndexReference SandboxPerkName;
    public StringIndexReference SandboxPerkDescription;
    public int IconIndex;
}

[SchemaStruct(0x808076AA, 0x18)] //AA768080
public struct S808076AA
{
    public long FileSize;
    public DynamicArrayUnloaded<S808076AE> SandboxPerkDefinitionEntries;
}

[SchemaStruct(0x808076AE, 0xC)] //AE768080
public struct S808076AE
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
[SchemaStruct(0x808070F2, 0x18)] //F2708080
public struct S808070F2
{
    public long FileSize;
    public DynamicArrayUnloaded<S80806FED> ArtArrangementHashes;
}

[SchemaStruct(0x80806FED, 4)] //ED6F8080
public struct S80806FED
{
    public TigerHash ArtArrangementHash;
}

#endregion

#region ApiEntities

/// <summary>
/// Entity assignment tag header. The assignment can be accessed via the art arrangement index.
/// The file is massive so I don't auto-parse it.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808034D7, 0x28)] //D7348080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808055CE, 0x28)] //CE558080
public struct S808055CE
{
    public long FileSize;
    public DynamicArrayUnloaded<S808055D4> ArtArrangementEntityAssignments;
    // [DestinyField(FieldType.TablePointer)]
    // public DynamicArray<S808055D8> FinalAssignment;  // this is not needed as the above table has resource pointers
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80803433, 0x18)] //33348080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808055D4, 0x20)] //D4558080
public struct S808055D4
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
    public DynamicArray<S808055D7> MultipleEntityAssignments;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80805D63, 8)] //635D8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808055D7, 8)] //D7558080
public struct S808055D7
{
    public ResourceInTablePointer<S808055D8> EntityAssignmentResource;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808033C1, 0x18)] //C1338080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808055D8, 0x18)] //D8558080
public struct S808055D8
{
    public long Unk00;
    public DynamicArray<S808055DA> EntityAssignments;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808033A3, 4)] //A3338080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808055DA, 4)] //DA558080
public struct S808055DA
{
    public TigerHash EntityAssignmentHash;
}

/// <summary>
/// The "final" assignment map of assignment hash : entity hash
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80803AAA, 0x18)] //AA3A8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80804F43, 0x18)] //434F8080
public struct S80804F43
{
    public long FileSize;
    // This is large but kept as a DynamicArray so we can perform binary searches... todo implement binary search for DynamicArray
    // We could do binary searches... or we could not and transform into a dictionary
    public DynamicArrayUnloaded<S80804F45> EntityArrangementMap;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80803AA9, 8)] //A93A8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80804F45, 8)] //454F8080
public struct S80804F45 : IComparer<S80804F45>
{
    public TigerHash AssignmentHash;
    [NoLoad]
    public Tag<S80806FA3> EntityParent;

    public int Compare(S80804F45 x, S80804F45 y)
    {
        if (x.AssignmentHash.Equals(y.AssignmentHash)) return 0;
        return x.AssignmentHash.CompareTo(y.AssignmentHash);
    }
}

[SchemaStruct(0x80804EA4, 0x38)] //A44E8080
public struct S80804EA4
{
    public long FileSize;
    [SchemaField(0x10, Tag64 = true)]
    public Tag<S8080978C> SandboxPatternAssignmentsTag;
    [SchemaField(0x28, Tag64 = true)]
    public Tag<S80804F43> EntityAssignmentsMap;
}

/// <summary>
/// The assignment map for api entity sandbox patterns, for things like skeletons and audio || OR art dye references
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800341, 0x18)] //41038080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080978C, 0x28)] //8C978080
public struct S8080978C
{
    public long FileSize;
    public DynamicArrayUnloaded<S8080870F> AssignmentBSL;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public DynamicArray<SUInt32> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808005D7, 0x8)] //D7058080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080870F, 0x18)] //0F878080
public struct S8080870F : IComparer<S8080870F>
{
    public TigerHash ApiHash;

    [SchemaField(0x4, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public FileHash EntityRelationHash;  // can be entity or smth else, if SandboxPattern is entity if ArtDyeReference idk

    public int Compare(S8080870F x, S8080870F y)
    {
        if (x.ApiHash.Equals(y.ApiHash)) return 0;
        return x.ApiHash.CompareTo(y.ApiHash);
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080339A, 0x18)] //9A338080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808052AA, 0x18)] //AA528080
public struct S808052AA
{
    public long FileSize;
    public DynamicArrayUnloaded<S808052AE> SandboxPatternGlobalTagId;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808033BC, 0x20)] //BC338080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808052AE, 0x30)] //AE528080
public struct S808052AE
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
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80806FA3, 0x18)] //A36F8080
public struct S80806FA3
{
    public long FileSize;

    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)]
    public FileHash EntityData;  // can be entity, can be audio group for entity
}

#endregion

#region InventoryItem hashmap

[SchemaStruct(0x8080798C, 0x28)] //8C798080
public struct S8080798C
{
    public long FileSize;
    // These tables are just placeholders, instead we transform the bytes into a dict for best performance
    public DynamicArray<S80807996> ExoticHashmap;
    public DynamicArray<S80807996> GeneralHashmap;
}

[SchemaStruct(0x80807996, 8)] //96798080
public struct S80807996
{
    public TigerHash ApiHash;
    public int HashIndex;
}

#endregion

#region InventoryItem Icons

[SchemaStruct(0x80805A01, 0x18)] //015A8080
public struct S80805A01
{
    public long FileSize;
    public DynamicArrayUnloaded<S80805A07> InventoryItemIconsMap;
}

[SchemaStruct(0x80805A07, 0x20)] //075A8080
public struct S80805A07
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10, Tag64 = true), NoLoad]
    public Tag<S80803EB8> IconContainer;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802797, 0x80)] //97278080 // Non-8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80803EB8, 0x80)] //B83E8080
public struct S80803EB8
{
    public long FileSize;
    [SchemaField(0x10)]
    public TigerHash Unk10;
    public Tag<S80803ECF> IconPrimaryContainer;

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)] // Unsure
    [SchemaField(0x18, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<S80803ECF> IconAdContainer; //Eververse item advertisement

    [SchemaField(0x24, TigerStrategy.DESTINY1_RISE_OF_IRON)] // Icon dyemap?
    [SchemaField(0x1C, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<S80803ECF> IconBGOverlayContainer;

    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<S80803ECF> IconBackgroundContainer;

    [SchemaField(0x1C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x24, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<S80803ECF> IconOverlayContainer;

    [SchemaField(0x28, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<S80803ECF> IconSpecialContainer;

    [SchemaField(0x30, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    public Vector4 DyeColorR;
    public Vector4 DyeColorG;
    public Vector4 DyeColorB;
}


[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802070, 0x18)] //70208080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80803ECF, 0x18)] //CF3E8080
public struct S80803ECF
{
    public long FileSize;
    [SchemaField(0x10)]
    public ResourcePointer Unk10;  // cd3e8080, CD298080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808029CD, 0x1C)] //CD298080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80803ECD, 0x20)] //CD3E8080
public struct S80803ECD
{
    public DynamicArrayUnloaded<S80803ED2> Unk00;
}

[SchemaStruct(0x80803ECB, 0x20)] //CB3E8080
public struct S80803ECB
{
    public DynamicArrayUnloaded<S80803ED0> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802478, 0x10)] //78248080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80803ED2, 0x10)] //D23E8080
public struct S80803ED2
{
    public DynamicArrayUnloaded<S80803ED5> TextureList;
}

[SchemaStruct(0x80803ED0, 0x10)] //D03E8080
public struct S80803ED0
{
    public DynamicArrayUnloaded<S80803ED4> TextureList;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802B99, 0x4)] //992B8080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80803ED5, 4)] //D53E8080
public struct S80803ED5
{
    public Texture IconTexture;
}

[SchemaStruct(0x80803ED4, 4)] //D43E8080
public struct S80803ED4
{
    public Texture IconTexture;
}


#endregion

#region Dyes

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80803420, 0x18)] //20348080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808055C2, 0x18)] //C2558080
public struct S808055C2
{
    public long FileSize;
    public DynamicArrayUnloaded<S808055C6> ArtDyeReferences;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808033CC, 4)] //CC338080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808055C6, 8)] //C6558080
public struct S808055C6
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash ArtDyeHash;
    [SchemaField(0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(4, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public TigerHash DyeManifestHash;
}

[SchemaStruct(0x80806CE3, 8)] //E36C8080
public struct S80806CE3
{
    public long FileSize;
    [SchemaField(0x0C)]
    public Dye Dye;
    // same thing + some unknown flags and info
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808017F6, 0x18)] //F6178080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808051F2, 0x18)] //F2518080
public struct SDyeChannels
{
    public long FileSize;
    public DynamicArrayUnloaded<SDyeChannelHash> ChannelHashes;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801821, 4)] //21188080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80804F2C, 4)] //2C4F8080
public struct SDyeChannelHash
{
    public TigerHash ChannelHash;
}


#endregion

#region String container hash + indexmap

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808034CB, 0x18)] //CB348080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80805A09, 0x18)] //095A8080
public struct S80805A09
{
    public long FileSize;
    public DynamicArrayUnloaded<S80805A0E> StringContainerMap;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80803473, 0x18)] //73348080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80805A0E, 0x20)] //0E5A8080
public struct S80805A0E
{
    public TigerHash BankFnvHash;  // some kind of name for the bank

    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true), NoLoad]
    public LocalizedStrings LocalizedStrings;

    [SchemaField(0x18, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public short Index; // Index into 26BA8080 container is LocalizedStrings is null
    public short Unk1A;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080BA26, 0x18)] //26BA8080
public struct S8080BA26
{
    public long FileSize;
    public DynamicArray<S8080BA2C> LocalizedStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080BA2C, 0x20)] //2CBA8080
public struct S8080BA2C
{
    public TigerHash BankFnvHash;  // some kind of name for the bank

    [SchemaField(0x10, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true), NoLoad]
    public LocalizedStrings LocalizedStrings;
}


[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808050CF, 0x18)] //CF508080
public struct S808050CF
{
    public long FileSize;
    public DynamicArrayUnloaded<S808050D3> LoreStringMap;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808050D3, 0x28)] //D3508080
public struct S808050D3
{
    public long Unk00;
    public TigerHash LoreHash;
    public StringIndexReference LoreName;
    public StringIndexReference LoreSubtitle;
    public StringIndexReference LoreDescription;
}

#endregion

#region Socket+Plug Entries
[SchemaStruct(0x808077C0, 0x20)] //C0778080
public struct S808077C0
{
    public DynamicArray<S808077C3> SocketEntries;
    public DynamicArray<S808077C8> IntrinsicSockets;
}

/// <summary>
/// "socketEntries" from API
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808077C3, 0x60)] //C3778080
public struct S808077C3
{
    public short SocketTypeIndex; // 'socketTypeHash' 
    public short Unk02;
    public int Unk04;
    public int SingleInitialItemIndex; // 'singleInitialItemHash'

    [SchemaField(0x14)]
    public short ReusablePlugSetIndex1; // randomizedPlugSetHash -> reusablePlugItems

    [SchemaField(0x30)]
    public short ReusablePlugSetIndex2; // randomizedPlugSetHash -> reusablePlugItems

    [SchemaField(0x50)]
    public DynamicArray<S808077D5> PlugItems; // reusablePlugSetHash -> reusablePlugItems
}

[SchemaStruct(0x808077CD, 0x18)] //CD778080
public struct S808077CD
{
    public long FileSize;
    public DynamicArrayUnloaded<S808077D3> PlugSetDefinitionEntries;
}

[SchemaStruct(0x808077D3, 0x18)] //D3778080
public struct S808077D3
{
    public TigerHash PlugSetHash;
    [SchemaField(0x8)]
    public DynamicArray<S808077D5> ReusablePlugItems;
}

[SchemaStruct(0x808077D5, 0x58)] //D5778080
public struct S808077D5
{
    [SchemaField(0x20)]
    public int PlugInventoryItemIndex;
    //[SchemaField(0x28)]
    //public DynamicArray<S80807A3A>? UnkUnlocks;
}

[SchemaStruct(0x808077C8, 0x8)] //C8778080
public struct S808077C8
{
    public int SocketTypeIndex; // socketTypeHash
    public int PlugItemIndex; // plugItemHash
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808073A1, 0x124)] //A1738080
public struct S808073A1 // 'plug'
{
    public TigerHash PlugCategoryHash;
    [SchemaField(0xE8, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringHash PlugStyle; // 'uiPlugLabel', theres only none (invalid) and masterwork (6048A01E)
}

#endregion

#region Socket Category
[SchemaStruct(0x808076B6, 0x18)] //B6768080
public struct S808076B6
{
    public long FileSize;
    public DynamicArrayUnloaded<S808076BA> SocketTypeEntries;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808076BA, 0x68)] //BA768080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808076BA, 0x70)] //BA768080
public struct S808076BA
{
    public TigerHash SocketTypeHash;
    public short Unk04;
    public short SocketCategoryIndex; // 'socketCategoryHash'
    public int SocketVisiblity; // 'visibility'

    [SchemaField(0x30, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x38, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S808076C5> PlugWhitelists;
}

[SchemaStruct(0x808076C5, 0x8)] //C5768080
public struct S808076C5
{
    public TigerHash PlugCategoryHash;
    public short Unk04;
}

[SchemaStruct(0x80804F59, 0x18)] //594F8080
public struct S80804F59
{
    public long FileSize;
    public DynamicArrayUnloaded<S80804F5D> SocketCategoryEntries;
}

[SchemaStruct(0x80804F5D, 0x18)] //5D4F8080
public struct S80804F5D
{
    public TigerHash SocketCategoryHash;
    public StringIndexReference SocketName;
    public StringIndexReference SocketDescription;
    public DestinySocketCategoryStyle CategoryStyle; // 'uiCategoryStyle'
}
#endregion

#region Collectables

[SchemaStruct(0x80807828, 0x18)] //28788080
public struct S80807828
{
    public long FileSize;
    public DynamicArrayUnloaded<S8080782C> CollectibleDefinitionEntries;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080782C, 0xB0)] //2C788080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080782C, 0xF0)] //2C788080 // EoF 0xC0, Renegades 0xC8, MoT 0xF0
public struct S8080782C
{
    [SchemaField(0x18)]
    public DynamicArray<S808078F7> ParentNodeHashes;
    public TigerHash CollectibleHash;
    public int InventoryItemIndex;

    //[SchemaField(0x30)]
    //public DynamicArrayUnloaded<S80807A3A> UnkUnlock30;
    //[SchemaField(0x60)] // EoF 0x60, Renegades 0x68
    //public DynamicArrayUnloaded<S80807A3A> UnkUnlockClass;
    //public DynamicArrayUnloaded<S80807A3A> Unk70;
}

[SchemaStruct(0x808078F7, 2)] //F7788080
public struct S808078F7
{
    public short ParentNodeHashIndex;
}


[SchemaStruct(0x808059BF, 0x18)] //BF598080
public struct S808059BF
{
    public long FileSize;
    public DynamicArrayUnloaded<S808059C3> CollectibleDefinitionStringEntries;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808059C3, 0x60)] //C3598080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808059C3, 0x68)] //C3598080
public struct S808059C3
{
    public TigerHash CollectibleHash;
    public int IconIndex;
    public StringIndexReference CollectibleName;
    public StringIndexReference CollectibleDescription;
    public StringIndexReference SourceString;
    public StringIndexReference RequirementDescription;
}

#endregion

#region Objectives
// objective definition
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080753C, 0x18)] //3C758080
public struct S8080753C
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S80807540> ObjectiveDefinitionEntries;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80807540, 0xA8)] //40758080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80807540, 0xA8)] //40758080
public struct S80807540
{
    public TigerHash ObjectiveHash;
    [SchemaField(0x14, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public int CompletionValue;
}

// objective definition strings
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080584C, 0x18)] //4C588080
public struct S8080584C
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S80805850> ObjectiveDefinitionStringEntries;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80805850, 0x58)] //50588080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80805850, 0x60)] //50588080
public struct S80805850
{
    public TigerHash ObjectiveHash;
    public short IconIndex;
    [SchemaField(0x18)]
    public StringIndexReference ProgressDescription;
    public byte InProgressValueStyle; // enum DestinyUnlockValueUIStyle ?
    public byte CompletedValueStyle;
    public short LocationIndex; // 'locationHash' DestinyLocationDefinition
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808073B0, 0x28)] //B0738080
public struct S808073B0
{
    public DynamicArray<S80809015> Objectives;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80809015, 0x2)] //15908080
public struct S80809015
{
    public short ObjectiveIndex;
}
#endregion

#region Quest stuff
/// <summary>
/// 'setData' in InventoryItemDefinition
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80807388, 0x30)] //88738080
public struct S80807388 // 0x58 pointer in inv item tag
{
    public DynamicArray<S8080738A> ItemList; // 'itemList'

    [SchemaField(0x18)]
    public TigerHash Type; // 'setType'
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080738A, 0x8)] //8A738080
public struct S8080738A
{
    public int Value; // 'trackingValue'
    public int Index; // 'itemIndex'
}

/// <summary>
/// 'value' in InventoryItemDefinition
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080757C, 0x4C)] //7C758080
public struct S8080757C
{
    public DynamicStruct<SQuestStepReward> Reward1;
    public DynamicStruct<SQuestStepReward> Reward2;
    public DynamicStruct<SQuestStepReward> Reward3;
    public DynamicStruct<SQuestStepReward> Reward4;
    public DynamicStruct<SQuestStepReward> Reward5;
    public DynamicStruct<SQuestStepReward> Reward6;
}

[NonSchemaStruct(TigerStrategy.DESTINY2_LATEST, 0xC)]
public struct SQuestStepReward
{
    public int Unk00;
    public short ItemIndex;
    public short Unk06;
    public int Quantity;
}

// String tag stuff
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808054D0, 0x18)] //D0548080
public struct S808054D0 // 0x58 pointer in string tags
{
    public StringIndexReference QuestLineName; // 'questLineName'
    public StringIndexReference QuestLineDescription; // 'questLineDescription'
    public StringIndexReference QuestStepSummary; // 'questStepSummary'
}

#endregion

#region DestinyPresentationNodeDefinitions
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808078D7, 0x18)] //D7788080
public struct S808078D7
{
    [SchemaField(0x8)]
    public DynamicArray<S808078DB> PresentationNodeDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808078DB, 0xC8)] //DB788080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808078DB, 0xE0)] //DB788080
public struct S808078DB
{
    [SchemaField(0x18)]
    public DynamicArray<S808078F7> ParentNodes;
    [SchemaField(0x2C)]
    public int MaxCategoryRecordScore;
    [SchemaField(0x30)]
    public TigerHash Hash;
    public byte NodeType;
    public byte Scope;
    [SchemaField(0x58, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_LATEST)]
    public short ObjectiveIndex;
    public short CompletionRecordIndex; // completionRecordHash
    [SchemaField(0x70, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    [SchemaField(0x88, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S808078ED> PresentationNodes; // children -> presentationNodes
    public DynamicArray<S808078EA> Collectibles; // children -> collectibles
    public DynamicArray<S808078E7> Records; // children -> records
    // Assuming metrics and craftables are right after as well
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808078ED, 0x20)] //ED788080
public struct S808078ED
{
    public short Unk00; // nodeDisplayPriority? Always 0 in api though
    public short PresentationNodeIndex; // presentationNodeHash
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808078EA, 0x4)] //EA788080
public struct S808078EA
{
    public short Unk00;
    public short CollectableIndex; // Collectable index
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808078E7, 0x6)] //E7788080
public struct S808078E7
{
    public short Unk00;
    public int RecordDefinitionIndex; // RecordDefinition index
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80805803, 0x18)] //03588080
public struct S80805803
{
    [SchemaField(0x8)]
    public DynamicArray<S80805807> PresentationNodeDefinitionStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80805807, 0x2C)] //07588080
public struct S80805807
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
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080711F, 0x18)] //1F718080
public struct S8080711F
{
    [SchemaField(0x8)]
    public DynamicArray<S80806FC1> RecordDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80806FC1, 0xE8)] //C16F8080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80806FC1, 0x118)] //C16F8080
public struct S80806FC1
{
    public int Unk00; // DestinyPresentationNodeType?

    [SchemaField(0x18)]
    public DynamicArray<S808078F7> ParentNodeHashes;

    [SchemaField(0x30)]
    public TigerHash Hash;
    public short LoreIndex;
    public short Unk36;
    public DynamicArray<S80806FC9> Objectives;
    public DynamicArray<S80806FC8> IntervalObjectives;

    [SchemaField(0x64)]
    public int ScoreValue;

    [SchemaField(0xCC)]
    public int GildingTrackingRecordIndex; // 'gildingTrackingRecordHash'
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80806FC9, 0x2)] //C96F8080
public struct S80806FC9
{
    public short ObjectiveIndex;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80806FC8, 0x8)] //C86F8080
public struct S80806FC8
{
    public short ObjectiveIndex;
    public short Unk02; // unlock, unlock value, or unlock expression mapping index...?
    public int ScoreValue; // 'intervalScoreValue'
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80805887, 0x18)] //87588080
public struct S80805887
{
    [SchemaField(0x8)]
    public DynamicArray<S8080588B> RecordDefinitionStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080588B, 0x90)] //8B588080
public struct S8080588B
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
    public DynamicArray<S80805893> RewardItems;
    public DynamicArray<S80805891> IntervalRewardItems;

    // 'titlesByGender'
    [SchemaField(0x80)]
    public StringIndexReference TitleName; // Male
    //public StringIndexReference TitleName; // Female
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80805893, 0x20)] //93588080
public struct S80805893
{
    public int ItemIndex; // InventoryItem index
    public int Quantity;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80805891, 0x10)] //91588080
public struct S80805891
{
    public DynamicArray<S80805893> Rewards;
}
#endregion

#region DestinySeasonDefinition
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80807108, 0x18)] //80807108
public struct S80807108
{
    [SchemaField(0x8)]
    public DynamicArray<S80806FF7> SeasonDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80806FF7, 0xA8)] //F76F8080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80806FF7, 0xB0)] //F76F8080
public struct S80806FF7
{
    public TigerHash SeasonHash;
    public int SeasonNumber;
    //public DynamicArray<S80807A3A> Unk08;
    //public DynamicArray<S8080B3BD> SeasonPassIndexes;

    //[SchemaField(0x20)]
    //public DynamicArray<S80807A3A> Unk20;

    //[SchemaField(0x38)] // No longer valid in EoF
    //public int NumberOfActs;

    //[SchemaField(0x40)] // No longer valid in EoF
    //public long Act1StartTime;
    //public long Act2StartTime;
    //public long Act3StartTime;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080B3BD, 0x20)] //BDB38080
public struct S8080B3BD
{
    public int SeasonPassIndex; // 'seasonPassHash' -> DestinySeasonPassDefinition
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80804F7E, 0x18)] //80804F7E
public struct S80804F7E
{
    [SchemaField(0x8)]
    public DynamicArray<S80804F82> SeasonDefinitionStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80804F82, 0x48)] //824F8080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80804F82, 0x50)] //824F8080
public struct S80804F82
{
    [SchemaField(0x8)]
    public TigerHash SeasonHash;

    [SchemaField(0x28)]
    public int IconIndex;
    public StringIndexReference SeasonName;
    public StringIndexReference SeasonDescription;
    public short Unk34; // index in S80805615??
}
#endregion

#region Trait Definition
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80807900, 0x28)] //80807900
public struct S80807900
{
    [SchemaField(0x8)]
    public DynamicArray<S80807909> Traits;
    // Another table here but its the same as above but unordered with its index where Unk04 would be?
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80807909, 0x8)] //09798080
public struct S80807909
{
    public DestinyTraitID TraitHash;
    public int Unk04; // Sometimes its index, sometimes not
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808057F6, 0x18)] //808057F6
public struct S808057F6
{
    [SchemaField(0x8)]
    public DynamicArray<S808057FA> TraitStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x808057FA, 0x1C)] //FA578080
public struct S808057FA
{
    public DestinyTraitID TraitHash;
    public int IconIndex;
    public StringIndexReference TraitName;
    public StringIndexReference TraitDescription;
    public TigerHash Unk18; // always 'keyword'?
}
#endregion

#region Event/Activity/Seasonal style(?) container stuff
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80805615, 0x18)] //80805615
public struct S80805615
{
    [SchemaField(0x8)]
    public DynamicArray<S8080561B> Entries;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080561B, 0x8)] //1B568080
public struct S8080561B
{
    public TigerHash Unk00;
    public Tag<S80803EA5> Container;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80803EA5, 0x70)] //80803EA5
public struct S80803EA5
{
    [SchemaField(0x8)]
    public TigerHash CodeName;

    [SchemaField(0x10, Tag64 = true)]
    public Tag<S80803EBA> Container;

    [SchemaField(Tag64 = true)]
    public LocalizedStrings Strings;
    public DynamicArray<S80803BB7> ColorSchemes;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80803BB7, 0x20)] //B73B8080
public struct S80803BB7
{
    public TigerHash Type; // primary, secondary, tertiary
    [SchemaField(0x10)]
    public Vector4 Color;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80803EBA, 0x20)] //80803EBA
public struct S80803EBA
{
    [SchemaField(0x8)]
    public DynamicArray<S80803EBE> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80803EBE, 0x70)] //BE3E8080
public struct S80803EBE
{
    public TigerHash Unk00;
    public Tag<S80803ECF> Container;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080B3ED, 0x18)] //8080B3ED
public struct S8080B3ED // DestinyItemFilterDefinitions
{
    [SchemaField(0x8)]
    public DynamicArray<S8080B3C1> Filters;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080B3C1, 0x18)] //C1B38080
public struct S8080B3C1 // DestinyItemFilterDefinitions, currently only FeaturedItems 
{
    public TigerHash FilterHash;
    [SchemaField(0x8)]
    public DynamicArray<S8080B5C5> FilterList;
}
#endregion

#region DestinyEquipableItemSetDefinition

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080B44E, 0x28)] //8080B44E
public struct S8080B44E // DestinyEquipableItemSetDefinition
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S8080B454> ItemSetDefinitions;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080B454, 0x28)] //54B48080
public struct S8080B454
{
    public TigerHash SetHash;
    public int Unk04;

    [SchemaField(0x8)]
    public DynamicArray<S8080B458> SetItems;
    public DynamicArray<S8080B457> SetPerks;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080B458, 0x4)] //58B48080
public struct S8080B458
{
    public int ItemIndex;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080B457, 0x30)] //57B48080
public struct S8080B457
{
    [SchemaField(0x28)]
    public short PerkIndex;
    public short SetCount; // 'requiredSetCount'
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080B2C6, 0x18)] //8080B2C6
public struct S8080B2C6 // DestinyEquipableItemSetDefinition Strings
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S8080B27A> ItemSetDefinitionStrings;
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080B27A, 0x28)] //7AB28080
public struct S8080B27A
{
    public TigerHash SetHash;
    public int IconIndex; // Maybe its actually Unk04 in the main definition?
    public StringIndexReference SetName;
    public StringIndexReference SetDescription;
    //public DynamicArray<S8080B27C> Unk18; // idk, all are zeros
}

#endregion

#region Destiny 1 API stuff

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808017BD, 0x4)] //BD178080
public struct S808017BD
{
    public short TalenGridIndex; // "talentGridHash" from API
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808018C2, 0x18)] //C2188080
public struct S808018C2
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S808017CB> TalentGridEntries;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808017CB, 0x18)] //CB178080
public struct S808017CB
{
    public TigerHash TalentGridHash;
    [SchemaField(0x10)]
    public Tag<S80801963> TalentGrid;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808018C2, 0x38)] //C2188080
public struct S80801963
{
    [SchemaField(0x10)]
    public DynamicArray<S80801728> Nodes;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801728, 0x40)] //28178080
public struct S80801728
{
    public TigerHash NodeHash; // ??
    [SchemaField(0x18)]
    public DynamicArray<S80801758> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801758, 0x90)] //58178080
public struct S80801758
{
    public DynamicArray<S808016DE> Unk00;
    [SchemaField(0x20)]
    public DynamicArray<S808045F1> Unk20;
    public TigerHash Unk30;
    public int Unk34;
    [SchemaField(0x70)]
    public DynamicArray<S80800F94> Unk70;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808016DE, 0x50)] //DE168080
public struct S808016DE
{
    public DynamicArray<S808018E8> Unk00;
    public DynamicArray<S80801787> Unk10;
    public DynamicArray<S80804628> Unk20;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808045F1, 0x2)] //F1458080
public struct S808045F1
{
    public short Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800F94, 0x4)] //940F8080
public struct S80800F94
{
    public short Unk00; // socketTypeHash?
    public short PlugItemIndex; // plugItemHash
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808018E8, 0x10)] //E8188080
public struct S808018E8
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801787, 0x10)] //87178080
public struct S80801787
{
    public int Unk00;
    public float Unk04; // min value?
    public float Unk08; // max value?
    public byte Unk0C; // index?
    public byte Unk0D;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80804628, 0x4)] //28468080
public struct S80804628
{
}


#endregion
