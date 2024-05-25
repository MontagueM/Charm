using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

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

// TODO: Find where these indexes actually go?
public enum DestinyDamageType : int
{
    [Description("Kinetic")]
    Kinetic = 1319,
    [Description(" Arc")]
    Arc = 1320,
    [Description(" Solar")]
    Solar = 1321,
    [Description(" Void")]
    Void = 1322,
    [Description(" Stasis")]
    Stasis = 1323,
    [Description(" Strand")]
    Strand = 1324
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
        { DestinyTierType.Unknown, Color.FromArgb(255, 66, 66, 66) },
        { DestinyTierType.Currency, Color.FromArgb(255, 195, 188, 180) },
        { DestinyTierType.Common, Color.FromArgb(255, 66, 66, 66) },
        { DestinyTierType.Uncommon, Color.FromArgb(255, 55, 113, 67) },
        { DestinyTierType.Rare, Color.FromArgb(255, 80, 118, 164) },
        { DestinyTierType.Legendary, Color.FromArgb(255, 82, 47, 100) },
        { DestinyTierType.Exotic, Color.FromArgb(255, 206, 174, 51) }
    };

    public static Color GetColor(this DestinyTierType tierType)
    {
        if (Colors.ContainsKey(tierType))
            return Colors[tierType];
        else
            throw new ArgumentException("Invalid DestinyTierType");
    }
}
