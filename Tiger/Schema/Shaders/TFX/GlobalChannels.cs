using Tiger.Schema;

namespace Tiger;

public static class GlobalChannels
{
    //public static Dictionary<TigerHash, Vector4> Channels = new();

    public static Vector4 GetDefault(int index)
    {
        return Globals.Get().GlobalChannelDefaults.ElementAt(index).Value;
    }

    public static Vector4 GetDefault(TigerHash hash)
    {
        return Globals.Get().GlobalChannelDefaults[hash];
    }

    public static void Set(int index, Vector4 vec)
    {
        var key = Globals.Get().GlobalChannelDefaults.Keys.ElementAt(index);
        Globals.Get().GlobalChannelDefaults[key] = vec;
    }

    public static void Set(TigerHash hash, Vector4 vec)
    {
        Globals.Get().GlobalChannelDefaults[hash] = vec;
    }

    public static void RestoreDefaults()
    {
        Globals.Get().GlobalChannelDefaults.Clear();
        Globals.Get().FillGlobalChannelDefaults();
    }

    public static Dictionary<uint, string> KnownChannelNames = new()
    {
        { 1050803134, "skybox_sun_intensity"},
        { 1055574819, "layered_fog_density"},
        { 1061091340, "cubemap_sky_intensity"},
        { 1067185869, "skybox_down_ambient_intensity"},
        { 1252114165, "sun_glow_shape"},
        { 1294091604, "fx_weather_01"},
        { 1294091607, "fx_weather_02"},
        { 1302805671, "sky_sun_glow_intensity"},
        { 1305631816, "cubemap_relighting_sky_intensity"},
        { 1337804320, "cubemap_bounce_scale"},
        { 1383168257, "up_ambient_intensity"},
        { 1459668224, "sun_tunnel_hash7"},
        { 1459668225, "sun_tunnel_hash6"},
        { 1459668226, "sun_tunnel_hash5"},
        { 1459668227, "sun_tunnel_hash4"},
        { 1459668228, "sun_tunnel_hash3"},
        { 1459668229, "sun_tunnel_hash2"},
        { 1459668230, "sun_tunnel_hash1"},
        { 1459668231, "sun_tunnel_hash0"},
        { 1549245946, "sun_light_direction"},
        { 1597736081, "sky_snapshot_intensity"},
        { 1614739564, "down_ambient_color"},
        { 1633552291, "sun_intensity"},
        { 1686806703, "global_weather_state"},
        { 1847767646, "sun_ambient_direction"},
        { 1942337203, "global_cubemap_diffuse_intensity"},
        { 1962697412, "down_ambient_sharpness"},
        { 2086794555, "sun_track_direction"},
        { 2088734839, "up_ambient_color"},
        { 2121305497, "sun_or_moon"},
        { 2123557681, "layered_fog_falloff"},
        { 2423543701, "sun_color"},
        { 2497244380, "fog_height_falloff"},
        { 2555718632, "dc_intensity"},
        { 2556025585, "sun_glow_intensity"},
        { 2578388673, "flashlight_intensity"},
        { 2663884264, "fog_decay_color"},
        { 2672605071, "layered_fog_start_height"},
        { 2781995415, "autoexposure_adjust_speed"},
        { 2787279257, "fog_density_interior"},
        { 2961144874, "autoexposure_max_stops"},
        { 3013228440, "fx_cinematics_03"},
        { 3013228441, "fx_cinematics_02"},
        { 3013228442, "fx_cinematics_01"},
        { 3013228445, "fx_cinematics_06"},
        { 3013228446, "fx_cinematics_05"},
        { 3013228447, "fx_cinematics_04"},
        { 3056632075, "fog_decay_scale"},
        { 3066944844, "global_weather_wind_direction"},
        { 3173810152, "fog_start_height"},
        { 3187940295, "reticle_utilize_filtering"},
        { 3231367590, "sun_atmosphere_direction"},
        { 3347321793, "sun_direct_intensity"},
        { 3437944730, "sun_shadow_intensity"},
        { 3439522578, "down_ambient_intensity"},
        { 3441842840, "skybox_sun_color"},
        { 3454161171, "skybox_down_ambient_color"},
        { 3479309780, "skybox_up_ambient_color"},
        { 3564389, "autoexposure_bias"},
        { 357608460, "global_cubemap_intensity"},
        { 3626505107, "sky_sun_glow_shape"},
        { 3651328163, "sky_color_override"},
        { 3785521083, "autoexposure_max_approach_speed"},
        { 3800300512, "sky_snapshot_rotation"},
        { 3828643774, "dc_color"},
        { 3862473451, "global_ambient_intensity"},
        { 3895284791, "flashlight_color"},
        { 3924713116, "sun_transmission_color"},
        { 3968759000, "autoexposure_min_stops"},
        { 3981160586, "fog_density"},
        { 4033942488, "fx_channel_02"},
        { 4033942489, "fx_channel_03"},
        { 4033942491, "fx_channel_01"},
        { 4033942492, "fx_channel_06"},
        { 4033942494, "fx_channel_04"},
        { 4033942495, "fx_channel_05"},
        { 4060811251, "up_ambient_sharpness"},
        { 4074476269, "screenspace_reflection_intensity"},
        { 4081239241, "global_cubemap_down_color"},
        { 4118825714, "global_weather_wind_speed"},
        { 413164828, "autoexposure_zero_bias"},
        { 4264009506, "sun_transmission_intensity"},
        { 90179527, "sun_glow_color"},
        { 911706265, "autoexposure_frame_delay"},
        { 954006746, "skybox_up_ambient_intensity"},
        { 2658574034, "fog_density_lookup_start"},
        { 1241234657, "fog_density_lookup_end"},
        { 1338215099, "ao_ambient_weight"},
        { 61911189, "ao_sun_weight"},
        { 4222214337, "character_lighting_scale_ao"},
        { 2093588672, "character_lighting_scale_emissive"},
        { 3076743815, "reticle_location"},
        { 3638890901, "sky_cloud_lighting_front"},
        { 4284370545, "sky_cloud_lighting_back"},
        { 2404723577, "cubemap_relighting_sun_intensity"},
        { 2243399393, "sky_snapshot_weather_amount"},
        { 743670141, "sky_colour_tint_01"},
        { 743670142, "sky_colour_tint_02"},
        { 743670143, "sky_colour_tint_03"},
        { 743670136, "sky_colour_tint_04"},
        { 743670137, "sky_colour_tint_05"},
        { 743670138, "sky_colour_tint_06"},
        { 743670139, "sky_colour_tint_07"},
        { 743670132, "sky_colour_tint_08"},
        { 743670133, "sky_colour_tint_09"},
        { 760447727, "sky_colour_tint_10"},
        { 760447726, "sky_colour_tint_11"},
        { 760447725, "sky_colour_tint_12"},
        { 760447724, "sky_colour_tint_13"},
        { 760447723, "sky_colour_tint_14"},
        { 760447722, "sky_colour_tint_15"},
        { 760447721, "sky_colour_tint_16"},
        { 760447720, "sky_colour_tint_17"},
        { 760447719, "sky_colour_tint_18"},
        { 760447718, "sky_colour_tint_19"},
        { 777225282, "sky_colour_tint_20"},
        { 777225283, "sky_colour_tint_21"},
        { 777225280, "sky_colour_tint_22"},
        { 777225281, "sky_colour_tint_23"},
        { 777225286, "sky_colour_tint_24"},
        { 777225287, "sky_colour_tint_25"},
        { 777225284, "sky_colour_tint_26"},
        { 777225285, "sky_colour_tint_27"},
        { 777225290, "sky_colour_tint_28"},
        { 777225291, "sky_colour_tint_29"},
        { 794002997, "sky_colour_tint_30"},
        { 794002996, "sky_colour_tint_31"},
        { 794002999, "sky_colour_tint_32"},
        { 1150719031, "cubemap_exterior_ground_intensity"},
        { 3781913451, "skybox_lighting_start_distance"},
        { 3421710210, "skybox_lighting_transition_distance"},
    };
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800507, 0x68)] //07058080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808093C4, 0x78)] //C4938080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808091D1, 0x68)] //D1918080
public struct S808091D1
{
    [SchemaField(0x0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public TigerHash Unk00; // Assuming name
    public int Unk04;

    [SchemaField(0x14, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x24, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x14, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public float Unk14;
    public float Unk18;
    public int Unk1C;

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x3C, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x24, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public int ChannelIndex;

    [SchemaField(0x28, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<SUInt8> UnkBytecode;
    public DynamicArray<Vec4> Values;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800788, 0x30)] //88078080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808084DF, 0x30)] //DF848080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080816F, 0x38)] //6F818080
public struct S8080816F
{
    [SchemaField(0x28, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public TigerHash ID;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808091CF, 0xF8)] //CF918080
public struct S808091CF
{
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<S80806BB7> Unk28;

    // Theres bytecode here also but idk what its used for
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806BB7, 0x50)] //80806BB7
public struct S80806BB7
{
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Texture LUT;
    public Texture Unk2C;
    public Texture Unk30;
}

//[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808093D3, 0x50)] //D3938080
//[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808091DF, 0x38)] //DF918080
//[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x808091DD, 0x38)] //DD918080
//public struct S808091DD
//{
//    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
//    [SchemaField(0x0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
//    public TigerHash Unk00; // Assuming name

//    [SchemaField(0x38, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
//    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
//    public DynamicArray<S808094E9> Unk20;
//}

//[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808093FB, 0x4)] //FB938080
//[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x808094E9, 0x4)] //E9948080
//public struct S808094E9
//{
//    public short Unk00;
//    public short Unk02;
//}
