using System.Collections.Concurrent;
using System.Text.Json;

namespace Field.General;

public class FnvHandler
{
    private static ConcurrentDictionary<uint, string> _fnvMap = new ConcurrentDictionary<uint, string>();

    private static List<string> _customStrings = new List<string>
    {
        // general
        "linear_velocity",
        "linear_acceleration",
        "length",
        "max_speed",
        "fp_iron_sight_fraction",
        "clamp",
        "look_yaw_pitch",
        "jump_directional_weights",
        "base_layer_playback_ratio",
        "throttle",
        "throttle_side_forward",
        "angular_velocity",
        "gender",
        "aim_vector",
        "aim_yaw_pitch",
        "velocity_ratio",
        "game_time_seconds",
        "linear_speed",
        "weapon_firing",
        "rotating_shield_orientation",
        "biped_base",
        "fallen_base",
        "fallen_base_with_fingers",
        "_auto_base_skeleton",
        "base",
        "layer",
        "Weight",
        "slice-rate",
        // control rig
        "left_foot",
        "right_foot",
        "right_thigh",
        "pelvis",
        "left_thigh",
        "spine_base",
        "spine_1",
        "spine_2",
        "spine_3",
        "pedestal",
        "sternum",
        "right_grip",
        "right_hand",
        "left_grip",
        "left_hand",
        "left_elbow",
        "left_shoulder",
        "right_elbow",
        "right_shoulder",
        "left_clavicle",
        "right_clavicle",
    };
    
    public static void Initialise()
    {
        string fileName = "fnv_charm.json";
        string jsonData = System.IO.File.ReadAllText(fileName);
        _fnvMap = JsonSerializer.Deserialize<ConcurrentDictionary<uint, string>>(jsonData);
        foreach (var customString in _customStrings)
        {
            _fnvMap.TryAdd(Fnv(customString), customString);
        }
    }

    public static string GetStringFromHash(uint fnvHash)
    {
        if (_fnvMap.ContainsKey(fnvHash))
        {
            return _fnvMap[fnvHash];
        }

        return "";
    }

    public static uint Fnv(string fnvString)
    {
        uint value = 0x811c9dc5;
        for (var i = 0; i < fnvString.Length; i++)
        {
            value *= 0x01000193;
            value ^= fnvString[i];
        }
        return value;
    }
}