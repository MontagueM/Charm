using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text.Json;
using Field;
using Field.Entities;
using Field.General;
using Field.Investment;

namespace Field;

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
		string fileName1 = "fnv_charm.json";
		string fileName2 = "fnv_dict.json";
		string jsonData1 = File.ReadAllText(fileName1);
		string jsonData2 = File.ReadAllText(fileName2);

		_fnvMap = JsonSerializer.Deserialize<ConcurrentDictionary<uint, string>>(jsonData1);
		foreach (var customString in _customStrings)
		{
			_fnvMap.TryAdd(Fnv(customString), customString);
		}

		foreach (var (key, value) in JsonSerializer.Deserialize<ConcurrentDictionary<uint, string>>(jsonData2))
		{
			_fnvMap.TryAdd(key, value);
		}

		CacheNamesFromTags();

		string updatedJsonData2 = JsonSerializer.Serialize(_fnvMap, new JsonSerializerOptions
		{
			WriteIndented = true,
		});

		// write updated data back to file with each entry on a new line
		File.WriteAllText(fileName2, updatedJsonData2.Replace("\r\n", "\n").Replace("\n", Environment.NewLine));

		//foreach (var (key, value) in _fnvMap)
		//{
		//	Console.WriteLine($"{key} {value}");
		//}
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

	/// <summary>
	/// We can:
	/// 1. add the dev strings to the fnv map
	/// 2. add names to the TagHash so we can get some names
	///
	/// to add to TagHash we cheat a bit by adding the tag hash to _fnvMap instead of the actual fnv hash
	/// </summary>
	private static void CacheNamesFromTags()
	{
		Cache085D8080();
	}

	private static void Cache085D8080()
	{
		//var dgtbVals = PackageHandler.GetAllEntriesOfReference(0x010a, 0x80808930);
		//Parallel.ForEach(dgtbVals, val =>
		//{
		//	Tag<D2Class_30898080> dgtb = PackageHandler.GetTag<D2Class_30898080>(val);
		//	foreach (var entry in dgtb.Header.Unk18)
		//	{
		//		if (entry.Tag == null)
		//			continue;
		//		Console.WriteLine(entry.TagPath);
		//		Console.WriteLine(entry.TagNote);
		//	}
		//});

		var vals = PackageHandler.GetAllTagsWithReference(0x80805d08);
		PackageHandler.CacheHashDataList(vals.Select(x => x.Hash).ToArray());
		foreach (var tagHash in vals)
		{
			Tag<D2Class_085D8080> tag = PackageHandler.GetTag<D2Class_085D8080>(tagHash);
			Parallel.ForEach(tag.Header.Unk10, entry =>
			{
				if (entry.Tag == null)
					return;

				if (entry.TagName.Contains("localized_strings.tft"))
				{
					_fnvMap.TryAdd(entry.Tag.Hash.Hash, entry.TagName);
				}
				else if (entry.TagName.Contains("master_strings.tft"))
				{
					_fnvMap.TryAdd(entry.Tag.Hash.Hash, entry.TagName);

					Tag<D2Class_F27B8080> tag = PackageHandler.GetTag<D2Class_F27B8080>(entry.Tag.Hash);
					_fnvMap.TryAdd(tag.Header.StringContainer.Hash.Hash, entry.TagName);
				}
				else if (entry.TagName.Contains("ui_icon.tft"))
				{
					// We want to label both this tag + the texture below it
					_fnvMap.TryAdd(entry.Tag.Hash.Hash, entry.TagName);

					Tag<D2Class_B83E8080> textureTag = PackageHandler.GetTag<D2Class_B83E8080>(entry.Tag.Hash);
					var tex = ((D2Class_CD3E8080)textureTag.Header.IconPrimaryContainer.Header.Unk10).Unk00[0].TextureList[0].IconTexture;
					//var tex = textureTag.Header.IconPrimaryContainer;
					_fnvMap.TryAdd(tex.Hash.Hash, entry.TagName);
				}
				else
				{
					throw new NotImplementedException();
				}
			});
		}
	}
}

[StructLayout(LayoutKind.Sequential, Size = 0xE8)]
public struct D2Class_0A5D8080
{
	// public DestinyHash Unk00;
	// public DestinyHash Unk04;
	// public DestinyHash Unk08;
	// public uint Unk0C;
	[DestinyOffset(0x10), DestinyField(FieldType.RelativePointer)]
	public string TagName;
	[DestinyField(FieldType.TagHash64)]
	public Tag Tag;
	public DestinyHash Unk28;
	public DestinyHash Unk2C;
	[DestinyOffset(0x38), DestinyField(FieldType.TagHash64)]
	public Tag Unk38;

}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_085D8080
{
	public long FileSize;
	public DestinyHash TagName;  // usually the name of the localized string
	[DestinyOffset(0x10), DestinyField(FieldType.TablePointer)]
	public List<D2Class_0A5D8080> Unk10;
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_F27B8080
{
	public long FileSize;
	[DestinyField(FieldType.TagHash)]
	public Tag StringContainer;
	// unk list here
}

