using System.ComponentModel;

namespace Tiger;

public enum PrimitiveType  // name comes from bungie
{
    Triangles = 3,
    TriangleStrip = 5,
}

public enum ELodCategory : byte
{
    MainGeom0 = 0, // main geometry lod0
    GripStock0 = 1,  // grip/stock lod0
    Stickers0 = 2,  // stickers lod0
    InternalGeom0 = 3,  // internal geom lod0
    LowPolyGeom1 = 4,  // low poly geom lod1
    LowPolyGeom2 = 7,  // low poly geom lod2
    GripStockScope2 = 8,  // grip/stock/scope lod2
    LowPolyGeom3 = 9,  // low poly geom lod3
    Detail0 = 10 // detail lod0
}

public enum TigerLanguage
{
    English = 1,
    French = 2,
    Italian = 3,
    German = 4,
    Spanish = 5,
    Japanese = 6,
    Portuguese = 7,
    Russian = 8,
    Polish = 9,
    Simplified_Chinese = 10,
    Traditional_Chinese = 11,
    Latin_American_Spanish = 12,
    Korean = 13,
}

public enum DestinyGenderDefinition
{
    Masculine = 0,
    Feminine = 1,
    None = 2
}

// https://bungie-net.github.io/multi/schema_Destiny-Definitions-Sockets-DestinySocketCategoryDefinition.html#schema_Destiny-Definitions-Sockets-DestinySocketCategoryDefinition
// https://bungie-net.github.io/multi/schema_Destiny-DestinySocketCategoryStyle.html#schema_Destiny-DestinySocketCategoryStyle
public enum DestinySocketCategoryStyle : uint
{
    Unknown = 0, // 0
    Reusable = 2656457638, // 1
    Consumable = 1469714392, // 2
                             // where Intrinsic? Replaced with LargePerk? // 4
    Unlockable = 1762428417, // 3
    EnergyMeter = 750616615, // 5
    LargePerk = 2251952357, // 6
    Abilities = 1901312945, // 7
    Supers = 497024337, // 8

    ArmorPerkSet = 3991564788, // 9, not real in game, using for armor set bonuses
}

public enum DestinyTooltipStyle : uint
{
    None = StringHash.InvalidHash32, // C59D1C81
    Build = 3284755031, // 'build'
    Record = 3918064370, // 'record'
    VendorAction = 4278229900, // 'vendor_action'
    Package = 1905831191, // 'package'
    Bounty = 1345459588, // 'bounty'
    Quest = 1801258597, // 'quest'
    Emblem = 4274335291, // 'emblem'
}

public enum DestinyUIDisplayStyle : uint
{
    None = StringHash.InvalidHash32, // C59D1C81
    Info = 3556713801, // 'ui_display_style_info'
    PerkInfo = 900809780, // 'ui_display_style_perk_info'
    ItemAddon = 1366836148, // 'ui_display_style_item_add_on'
    EnergyMod = 3201739904, // 'ui_display_style_energy_mod'
    Crafting = 2902805631, // 'ui_display_style_crafting'
    Warning = 3475342179, // 'ui_display_style_warning'
    Box = 2744312160, // 'ui_display_style_box'
    SetContainer = 262729839, // 'ui_display_style_set_container'
    IntrinsicPlug = 2065752925, // 'ui_display_style_intrinsic_plug'
    Engram = 2688883665, // 'ui_display_style_engram'
    Token = 4060663772, // 'ui_display_style_token'
    Infuse = 1494624843, // 'ui_display_style_infuse'
    Memory = 1497864296, // 'ui_display_style_memory'
    Deepsight = 2609099574,
}

public enum DestinyScreenStyle : uint
{
    None = StringHash.InvalidHash32, // C59D1C81
    Emblem = 3797307284, // 'screen_style_emblem'
    Sockets = 1726057944, // 'screen_style_sockets'
    Vendor = 1509794344, // 'screen_style_vendor'
    Pursuit = 347107188, // 'screen_style_pursuit'
    SeasonalArtifact = 3129355947, // 'screen_style_seasonal_artifact'
    SeasonalArtifactMemorialized = 1186261878, // 'screen_style_seasonal_artifact_memorialized'
    Builds = 2050070793, // 'screen_style_builds'
    DestinationMods = 2085300474, // 'screen_style_destination_mods'
    LoreOnly = 788037671, // 'screen_style_lore_only'
    NewLightSkip = 2869100985, // 'screen_style_new_light_skip'
    Potions = 4095008086, // 'screen_style_potions'
    Emote = 1177935158, // 'screen_style_emote'
}

// https://bungie-net.github.io/multi/schema_Destiny-DestinyPresentationDisplayStyle.html#schema_Destiny-DestinyPresentationDisplayStyle
public enum DestinyPresentationDisplayStyle : uint
{
    None = StringHash.InvalidHash32, // C59D1C81
    Category = 2709938145,
    Badge = 2706248602,
    Medals = 2428427449,
    Collectible = 3356756485,
    Record = 3918064370,
    SeasonalTriumph = 2299886033,
    GuardianRank = 4244259185,
    CategoryCollectibles = 1420736209,
    CategoryCurrencies = 389668453,
    CategoryEmblems = 1365480833,
    CategoryEmotes = 3746094335,
    CategoryEngrams = 1943645307,
    CategoryFinishers = 542174403,
    CategoryGhosts = 1151876704,
    CategoryMisc = 1664305868,
    CategoryMods = 1547156651,
    CategoryOrnaments = 71601523,
    CategoryShaders = 627000902,
    CategoryShips = 1751672029,
    CategorySpawnfx = 1570941439,
    CategoryUpgradeMaterials = 2544811075,
}

// https://bungie-net.github.io/multi/schema_Destiny-DestinyPresentationScreenStyle.html#schema_Destiny-DestinyPresentationScreenStyle
public enum DestinyPresentationScreenStyle : uint
{
    Default = StringHash.InvalidHash32, // C59D1C81
    CategorySets = 1356519327,
    Badge = 2706248602,
}

public enum DestinyTraitID : uint
{
    // Not in game, instead defined for custom assignment
    [Description("Other")]
    item_other = 0,
    [Description("Mask")]
    item_mask = 1,
    [Description("Seasonal Artifact")]
    item_seasonal_artifact = 2,
    item_quest_all = 3,

    // Defined in game
    [Description("Black Armory")]
    activities_black_armory = 2944045106,
    [Description("Gambit")]
    activities_gambit = 853784306,
    [Description("Iron Banner")]
    activities_iron_banner = 2716563063,
    [Description("Gambit Prime")]
    activities_mamba = 1781288324,
    [Description("Trials of Osiris")]
    activities_trials = 3439101959,
    [Description("The Crucible")]
    faction_crucible = 2951764300,
    [Description("Dead Orbit")]
    faction_dead_orbit = 3331226384,
    [Description("Future War Cult")]
    faction_future_war_cult = 1345630660,
    [Description("New Monarchy")]
    faction_new_monarchy = 1221030001,
    [Description("Vanguard")]
    faction_vanguard = 3359893241,
    [Description("Daito")]
    foundry_daito = 1866367371,
    [Description("Field Forged")]
    foundry_field_forged = 3475344486,
    [Description("FOTC")]
    foundry_fotc = 2217328812,
    [Description("Hakke")]
    foundry_hakke = 2210483526,
    [Description("Omolon")]
    foundry_omolon = 192828432,
    [Description("Suros")]
    foundry_suros = 3690635686,
    [Description("Tex Mechanica")]
    foundry_tex_mechanica = 1821231131,
    [Description("Veist")]
    foundry_veist = 963390771,

    inventory_filtering_bounty = 201433599,
    inventory_filtering_quest = 1861210184,
    inventory_filtering_quest_featured = 3034243664,

    [Description("Arm Armor")]
    item_armor_arms = 1851377542,
    [Description("Chest Armor")]
    item_armor_chest = 374319058,
    [Description("Class Item")]
    item_armor_class = 3367459877,
    [Description("Head Armor")]
    item_armor_head = 1075323345,
    [Description("Leg Armor")]
    item_armor_legs = 1968436740,

    item_aura = 3553898659,
    item_boost = 1030789163,
    item_bounty = 2443101659,
    item_consumable = 2062186907,
    item_currency = 3906525419,

    [Description("Emblem")]
    item_emblem = 2455696884,

    item_emote = 888082966,
    item_engram = 2893978702,
    item_exotic_catalyst = 4036726046,
    item_finisher = 2582082890,

    [Description("Ghost Shell")]
    item_ghost = 2570676179,
    item_ghost_hologram = 4118304139,

    [Description("Armor Ornament")]
    item_ornament_armor = 3477257717,
    [Description("Weapon Ornament")]
    item_ornament_weapon = 3828004164,

    item_package = 151064318,
    item_plug_aspect = 577926988,
    item_plug_fragment = 2833630124,

    item_quest_annual_v460 = 2908763903,
    item_quest_annual_v500 = 2774395792,
    item_quest_annual_v600 = 929402123,
    item_quest_annual_v700 = 2976021378,
    item_quest_annual_v800 = 3011401061,
    item_quest_annual_v900 = 763053052,
    item_quest_campaign = 2973844452,
    [Description("The Edge of Fate Quest")]
    item_quest_current_release = 2878306895,
    [Description("Seasonal Event Quest")]
    item_quest_event = 1056186694,
    [Description("Exotic Quest")]
    item_quest_exotic = 370766376,
    item_quest_frontier_apollo = 2799343944,
    [Description("New Light Quest")]
    item_quest_new_light = 520867389,
    item_quest_onramp = 170945933,
    [Description("Legacy Quest")]
    item_quest_past = 2387836362,
    [Description("Playlist Quest")]
    item_quest_playlists = 500105683,
    [Description("Seasonal Quest")]
    item_quest_seasonal = 3671004794,
    item_quest_seasonal_season24 = 3904180889,
    item_quest_seasonal_season25 = 3904180888,
    item_quest_seasonal_season26 = 3904180891,

    [Description("Shader")]
    item_shader = 2652561225,
    [Description("Ship")]
    item_ship = 3607584152,

    item_spawnfx = 856705125,
    item_subclass_dark = 3224025418,
    item_subclass_light = 482679394,
    item_subclass_prism = 3820193993,

    [Description("Sparrow")]
    item_vehicle = 3977049418,
    [Description("Auto Rifle")]
    item_weapon_auto_rifle = 2729780558,
    [Description("Combat Bow")]
    item_weapon_bow = 195373008,
    [Description("Fusion Rifle")]
    item_weapon_fusion_rifle = 2891203715,
    [Description("Glaive")]
    item_weapon_glaive = 888940472,
    [Description("Grenade Launcher")]
    item_weapon_grenade_launcher = 130863397,
    [Description("Hand Cannon")]
    item_weapon_hand_cannon = 3602983853,
    [Description("Linear Fusion Rifle")]
    item_weapon_linear_fusion_rifle = 2100142349,
    [Description("Machine Gun")]
    item_weapon_machinegun = 1143070403,
    [Description("Pulse Rifle")]
    item_weapon_pulse_rifle = 1648572040,
    [Description("Rocket Launcher")]
    item_weapon_rocket_launcher = 3925016055,
    [Description("Scout Rifle")]
    item_weapon_scout_rifle = 12026609,
    [Description("Shotgun")]
    item_weapon_shotgun = 2114179114,
    [Description("Sidearm")]
    item_weapon_sidearm = 2034403781,
    [Description("Sniper Rifle")]
    item_weapon_sniper_rifle = 3300229618,
    [Description("Submachine Gun")]
    item_weapon_submachinegun = 2659552777,
    [Description("Sword")]
    item_weapon_sword = 1531673855,
    [Description("Trace Rifle")]
    item_weapon_trace_rifle = 446244952,

    keywords_buffs_arc_ionic_trace = 3824458961,
    keywords_buffs_arc_static_charge = 2935077680,
    keywords_buffs_arc_supercharged = 3291013836,
    keywords_buffs_prism_dark_buffs = 1891050213,
    keywords_buffs_prism_dark_debuffs = 1514833946,
    keywords_buffs_prism_light_buffs = 2713325501,
    keywords_buffs_prism_light_debuffs = 3023190802,
    keywords_buffs_prism_transcendence = 345967499,
    keywords_buffs_solar_cure = 3263723277,
    keywords_buffs_solar_empower = 157469667,
    keywords_buffs_solar_flare_bauble = 37177486,
    keywords_buffs_solar_restoration = 3488482714,
    keywords_buffs_stasis_crystal = 3385340084,
    keywords_buffs_stasis_frost_armor = 106947924,
    keywords_buffs_stasis_shard = 4043161234,
    keywords_buffs_strand_body_armor = 3173573497,
    keywords_buffs_strand_tangle = 1577394840,
    keywords_buffs_strand_threadling = 2724747993,
    keywords_buffs_void_breach_bauble = 3328352616,
    keywords_buffs_void_devour = 3078132110,
    keywords_buffs_void_invisibility = 655301426,
    keywords_buffs_void_overshield = 2485406866,
    keywords_debuffs_arc_blind = 500183315,
    keywords_debuffs_arc_jolt = 3221118171,
    keywords_debuffs_solar_detonation = 3268862716,
    keywords_debuffs_solar_scorch = 1096356879,
    keywords_debuffs_stasis_freeze = 2968599152,
    keywords_debuffs_stasis_shatter = 37938188,
    keywords_debuffs_stasis_slow = 4239423954,
    keywords_debuffs_strand_infest = 945613349,
    keywords_debuffs_strand_sever = 2519102437,
    keywords_debuffs_strand_suspend = 2679722414,
    keywords_debuffs_void_suppression = 2578642829,
    keywords_debuffs_void_volatile = 4105407564,
    keywords_debuffs_void_weaken = 3336638905,
    mamba_role_collector = 3791840693,
    mamba_role_defender = 2712954769,
    mamba_role_invader = 3090596947,
    mamba_role_killer = 3460933757,

    [Description("Vanilla")]
    releases_v300_annual = 2677200345,
    [Description("Curse of Osiris")]
    releases_v310_season = 3750900718,
    [Description("Warmind")]
    releases_v320_season = 3990406773,
    [Description("Solstice (Year 1)")]
    releases_v350_season = 977620370,
    [Description("Forsaken")]
    releases_v400_annual = 1385893620,
    [Description("Festival (Year 1)")]
    releases_v400_season = 1416106830,
    [Description("Black Armory")]
    releases_v410_season = 3619103539,
    [Description("Drifter")]
    releases_v420_season = 117031016,
    [Description("Opulence")]
    releases_v450_season = 1357347767,
    [Description("Shadowkeep")]
    releases_v460_season = 1160263324,
    [Description("Dawn")]
    releases_v470_season = 2326993577,
    [Description("Worthy")]
    releases_v480_season = 1573004294,
    [Description("Arrivals")]
    releases_v490_season = 2405803211,
    [Description("Beyond Light")]
    releases_v500_annual = 2184280643,
    [Description("Hunt")]
    releases_v500_season = 2752740613,
    [Description("Chosen")]
    releases_v510_season = 3361847320,
    [Description("Splicer")]
    releases_v520_season = 4020167523,
    [Description("Lost")]
    releases_v530_season = 3353022846,
    [Description("30th Anniversary")]
    releases_v540_season = 2656809369,
    [Description("Witch Queen")]
    releases_v600_annual = 823756278,
    [Description("Risen")]
    releases_v600_season = 3596220576,
    [Description("Haunted")]
    releases_v610_season = 2868778669,
    [Description("Plunder")]
    releases_v620_season = 2572971238,
    [Description("Worthy")]
    releases_v630_season = 2208921643,
    [Description("Lightfall")]
    releases_v700_annual = 2606653893,
    [Description("Defiance")]
    releases_v700_season = 3833926855,
    [Description("Deep")]
    releases_v710_season = 661041410,
    [Description("Witch")]
    releases_v720_season = 687504889,
    [Description("Wish")]
    releases_v730_season = 866931116,
    [Description("The Final Shape")]
    releases_v800_annual = 2906302736,
    [Description("Echoes")]
    releases_v800_season = 1348188306,
    [Description("Revenant")]
    releases_v810_season = 4062709591,
    [Description("Heresy")]
    releases_v820_season = 3870807100,
    [Description("Reclamation")]
    releases_v900_core = 1858131755,
    [Description("Edge of Fate")]
    releases_v900_dlc = 2725534325,
    releases_v910 = 753559279,
    [Description("Ash and Iron")]
    releases_v910_core = 2052231686
}


public enum DestinyDamageTypeEnum : int
{
    None = -1,
    [Description("Kinetic")]
    Kinetic,
    [Description(" Arc")]
    Arc,
    [Description(" Solar")]
    Solar,
    [Description(" Void")]
    Void,
    [Description(" Stasis")]
    Stasis,
    [Description(" Strand")]
    Strand
}

public enum DestinyTierType
{
    Unknown = -1,
    Currency = 0,
    Common = 1, // Basic
    Uncommon = 2, // Common
    Rare = 3,
    Legendary = 4, // Superior
    Exotic = 5,
}

// https://bungie-net.github.io/multi/schema_Destiny-DestinyUnlockValueUIStyle.html#schema_Destiny-DestinyUnlockValueUIStyle
// Pls update your api docs bungo, most dont match up
public enum DestinyUnlockValueUIStyle
{
    Automatic = 0,
    Checkbox = 1,
    DateTime = 2,
    Fraction = 3,
    Integer = 5,
    Percentage = 6,
    TimeDuration = 7,
    GreenPips = 9,
    RedPips = 10,
    Hidden = 11,
    RawFloat = 13,
}

