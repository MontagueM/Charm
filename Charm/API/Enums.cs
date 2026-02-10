using System.Collections.Generic;
using System.Windows.Media;
using Arithmic;
using Tiger;
using Tiger.Schema;

namespace Charm;

// TODO: Find where these indexes actually go?
// Would be nice if these stopped changing EVERY UPDATE :)
public static class DestinyDamageType
{
    public static DestinyDamageTypeEnum GetDamageType(int index)
    {
        switch (index)
        {
            case -1:
                return DestinyDamageTypeEnum.None;

            case 1492: // TFS
            case 1959:
                return DestinyDamageTypeEnum.Kinetic;

            case 1493: // TFS
            case 1960:
                return DestinyDamageTypeEnum.Arc;

            case 1494: // TFS
            case 1961:
                return DestinyDamageTypeEnum.Solar;

            case 1495: // TFS
            case 1962:
                return DestinyDamageTypeEnum.Void;

            case 1496: // TFS
            case 1963:
                return DestinyDamageTypeEnum.Stasis;

            case 1497: // TFS
            case 1964:
                return DestinyDamageTypeEnum.Strand;

            default:
                Log.Debug($"Unknown DestinyDamageTypeEnum {index}");
                return DestinyDamageTypeEnum.None;
        }
    }
}

public static class DestinyTierTypeColor
{
    private static readonly Dictionary<DestinyTierType, Color> Colors = new()
    {
        { DestinyTierType.Unknown, FloatToByte(new(0.21961, 0.21961, 0.21961, 1.000000)) },
        { DestinyTierType.Currency, FloatToByte(new(0.21961, 0.21961, 0.21961, 1.000000)) },
        { DestinyTierType.Common, FloatToByte(new(0.759351, 0.731652, 0.700103, 1.000000)) },
        { DestinyTierType.Uncommon, FloatToByte(new(0.201764, 0.418355, 0.244703, 1.000000)) },
        { DestinyTierType.Rare, FloatToByte(new(0.336119, 0.494315, 0.613966, 1.000000)) },
        { DestinyTierType.Legendary, FloatToByte(new(0.310284, 0.212433, 0.388279, 1.000000 )) },
        { DestinyTierType.Exotic, FloatToByte(new(0.803047, 0.676518, 0.212433, 1.000000)) }
    };

    private static readonly Dictionary<DestinyTierType, Color> LabelColors = new()
    {
        { DestinyTierType.Unknown, FloatToByte(new(0.21961, 0.21961, 0.21961, 1.000000)) },
        { DestinyTierType.Currency, FloatToByte(new(0.21961, 0.21961, 0.21961, 1.000000)) },
        { DestinyTierType.Common, Color.FromArgb(255, 194, 187, 179) },
        { DestinyTierType.Uncommon, FloatToByte(new(0.536523, 0.818987, 0.501968, 1.000000)) },
        { DestinyTierType.Rare, FloatToByte(new(0.498140, 0.692234, 0.815000, 1.000000)) },
        { DestinyTierType.Legendary, FloatToByte(new(0.649090, 0.594524, 0.688302, 1.000000)) },
        { DestinyTierType.Exotic, FloatToByte(new(0.903085, 0.838948, 0.306608, 1.000000)) }
    };

    private static readonly Dictionary<DestinyTierType, Color> BodyColors = new()
    {
        { DestinyTierType.Unknown, FloatToByte(new(0.07451, 0.07059, 0.07059, 1.000000)) },
        { DestinyTierType.Currency, FloatToByte(new(0.07451, 0.07059, 0.07059, 1.000000)) },
        { DestinyTierType.Common, FloatToByte(new(0.101446, 0.098100, 0.098100, 1.000000)) },
        { DestinyTierType.Uncommon, FloatToByte(new(0.071704, 0.108167, 0.078231, 1.000000)) },
        { DestinyTierType.Rare, FloatToByte(new(0.094763, 0.121710, 0.145711, 1.000000)) },
        { DestinyTierType.Legendary, FloatToByte(new(0.091436, 0.071704, 0.104802, 1.000000)) },
        { DestinyTierType.Exotic, FloatToByte(new(0.131952, 0.114922, 0.056847, 1.000000)) }
    };

    public static Color GetColor(this DestinyTierType tierType)
    {
        if (Colors.ContainsKey(tierType))
            return Colors[tierType];
        else
            return Colors[DestinyTierType.Unknown];
    }

    public static Color GetLabelColor(this DestinyTierType tierType)
    {
        if (LabelColors.ContainsKey(tierType))
            return LabelColors[tierType];
        else
            return LabelColors[DestinyTierType.Unknown];
    }

    public static Color GetBodyColor(this DestinyTierType tierType)
    {
        if (BodyColors.ContainsKey(tierType))
            return BodyColors[tierType];
        else
            return BodyColors[DestinyTierType.Unknown];
    }

    private static Color FloatToByte(Vector4 vec)
    {
        return Color.FromArgb((byte)(vec.W * 255),
            (byte)(vec.X * 255),
            (byte)(vec.Y * 255),
            (byte)(vec.Z * 255));
    }
}

public enum StatHashes : uint
{
    Accuracy = 1591432999,
    AimAssistance = 1345609583,
    AirborneEffectiveness = 2714457168,
    AmmoCapacity = 925767036,
    AnyEnergyTypeCost = 3578062600,
    ArcCost = 3779394102,
    ArcDamageResistance = 1546607978,
    ArmorEnergyCapacity_16120457 = 16120457,
    ArmorEnergyCapacity_2018193158 = 2018193158,
    ArmorEnergyCapacity_2441327376 = 2441327376,
    ArmorEnergyCapacity_3625423501 = 3625423501,
    ArmorEnergyCapacity_3950461274 = 3950461274,
    AspectEnergyCapacity = 2223994109,
    Attack = 1480404414,
    BlastRadius = 3614673599,
    Boost = 3017642079,
    ChargeRate = 3022301683,
    ChargeTime = 2961396640,
    Defense = 3897883278,
    Discipline = 1735777505,
    DrawTime = 447667954,
    Durability = 360359141,
    FragmentCost = 119204074,
    GhostEnergyCapacity = 237763788,
    GuardEfficiency = 2762071195,
    GuardEndurance = 3736848092,
    GuardResistance = 209426660,
    Handicap = 2341766298,
    Handling = 943549884,
    HeroicResistance = 1546607977,
    Impact = 4043523819,
    Intellect = 144602215,
    InventorySize = 1931675084,
    Magazine = 3871231066,
    Mobility = 2996146975,
    ModCost = 514071887,
    MoveSpeed = 3907551967,
    Power = 1935470627,
    PowerBonus = 3289069874,
    PrecisionDamage = 3597844532,
    Range = 1240592695,
    RecoilDirection = 2715839340,
    Recovery = 1943323491,
    ReloadSpeed = 4188031367,
    Resilience = 392767087,
    RoundsPerMinute = 4284893193,
    ScoreMultiplier = 2733264856,
    ShieldDuration = 1842278586,
    SolarCost = 3344745325,
    SolarDamageResistance = 1546607979,
    Speed = 1501155019,
    Stability = 155624089,
    StasisCost = 998798867,
    Strength = 4244567218,
    SwingSpeed = 2837207746,
    TimeToAimDownSights = 3988418950,
    Velocity = 2523465841,
    VoidCost = 2399985800,
    VoidDamageResistance = 1546607980,
    Zoom = 3555269338,
}
