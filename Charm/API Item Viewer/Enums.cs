using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using Arithmic;
using Tiger;

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
}

public enum DestinyTooltipStyle : uint
{
    None = StringHash.InvalidHash32, // C59D1C81
    Build = 3284755031, // 'build'
    Record = 3918064370, // 'record'
    VendorAction = 4278229900 // 'vendor_action'
}

public enum DestinyUIDisplayStyle : uint
{
    None = StringHash.InvalidHash32, // C59D1C81
    Info = 3556713801, // 'ui_display_style_info'
    PerkInfo = 900809780, // 'ui_display_style_perk_info'
    ItemAddon = 1366836148, // 'ui_display_style_item_add_on'
    EnergyMod = 3201739904 // 'ui_display_style_energy_mod'
}


// TODO: Find where these indexes actually go?
public static class DestinyDamageType
{
    public static DestinyDamageTypeEnum GetDamageType(int index)
    {
        switch (index)
        {
            case -1:
                return DestinyDamageTypeEnum.None;
            case 1319:
            case 1373:
            case 1405:
                return DestinyDamageTypeEnum.Kinetic;
            case 1320:
            case 1374:
            case 1406:
                return DestinyDamageTypeEnum.Arc;
            case 1321:
            case 1375:
            case 1407:
                return DestinyDamageTypeEnum.Solar;
            case 1322:
            case 1376:
            case 1408:
                return DestinyDamageTypeEnum.Void;
            case 1323:
            case 1377:
            case 1409:
                return DestinyDamageTypeEnum.Stasis;
            case 1324:
            case 1378:
            case 1410:
                return DestinyDamageTypeEnum.Strand;
            default:
                Log.Warning($"Unknown DestinyDamageTypeEnum {index}");
                return DestinyDamageTypeEnum.None;
        }
    }
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

public static class DestinyTierTypeColor
{
    private static readonly Dictionary<DestinyTierType, Color> Colors = new Dictionary<DestinyTierType, Color>
    {
        { DestinyTierType.Unknown, Color.FromArgb(255, 56, 56, 56) },
        { DestinyTierType.Currency, Color.FromArgb(255, 56, 56, 56) },
        { DestinyTierType.Common, Color.FromArgb(255, 194, 187, 179) },
        { DestinyTierType.Uncommon, Color.FromArgb(255, 51, 107, 62) },
        { DestinyTierType.Rare, Color.FromArgb(255, 85, 125, 155) },
        { DestinyTierType.Legendary, Color.FromArgb(255, 79, 55, 99) },
        { DestinyTierType.Exotic, Color.FromArgb(255, 203, 171, 54) }
    };

    public static Color GetColor(this DestinyTierType tierType)
    {
        if (Colors.ContainsKey(tierType))
            return Colors[tierType];
        else
            return Colors[DestinyTierType.Unknown];
    }
}
