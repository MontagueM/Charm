using System.Text;
using Field.Entities;

namespace Field.Models;

public class Source2Handler
{
	public static bool source2Shaders = FieldConfigHandler.GetS2ShaderExportEnabled();
	public static bool source2Models = FieldConfigHandler.GetS2VMDLExportEnabled();
	public static bool source2Materials = FieldConfigHandler.GetS2VMATExportEnabled();

	public static void SaveStaticVMDL(string savePath, string staticMeshName, List<Part> staticMesh)
	{
		if (!File.Exists($"{savePath}/{staticMeshName}.vmdl"))
		{
			//Source 2 shit
			File.Copy("template.vmdl", $"{savePath}/{staticMeshName}.vmdl", true);
			string text = File.ReadAllText($"{savePath}/{staticMeshName}.vmdl");

			StringBuilder mats = new StringBuilder();

			int i = 0;
			foreach (Part staticpart in staticMesh)
			{
				mats.AppendLine("{");
				mats.AppendLine($"    from = \"{staticpart.Material.Hash}.vmat\"");
				mats.AppendLine($"    to = \"materials/{staticpart.Material.Hash}.vmat\"");
				mats.AppendLine("},\n");
				i++;
			}

			text = text.Replace("%MATERIALS%", mats.ToString());
			text = text.Replace("%FILENAME%", $"models/{staticMeshName}.fbx");
			text = text.Replace("%MESHNAME%", staticMeshName);

			File.WriteAllText($"{savePath}/{staticMeshName}.vmdl", text);
		}
	}

	public static void SaveEntityVMDL(string savePath, Entity entity)
	{
		if (!File.Exists($"{savePath}/{entity.Hash}.vmdl"))
		{
			File.Copy("template.vmdl", $"{savePath}/{entity.Hash}.vmdl", true);
			string text = File.ReadAllText($"{savePath}/{entity.Hash}.vmdl");

			StringBuilder mats = new StringBuilder();

			int i = 0;
			foreach (var part in entity.Load(ELOD.MostDetail, true))
			{
				if (part.Material == null)
					continue;

				if (part.Material.Header.PSTextures.Count == 0)
					continue;

				mats.AppendLine("{");
				mats.AppendLine($"    from = \"{part.Material.Hash}.vmat\"");
				mats.AppendLine($"    to = \"materials/{part.Material.Hash}.vmat\"");
				mats.AppendLine("},\n");
				i++;
			}

			text = text.Replace("%MATERIALS%", mats.ToString());
			text = text.Replace("%FILENAME%", $"models/{entity.Hash}.fbx");
			text = text.Replace("%MESHNAME%", entity.Hash);

			File.WriteAllText($"{savePath}/{entity.Hash}.vmdl", text);
		}
	}

	public static void SaveTerrainVMDL(string savePath, string hash, List<Part> parts, D2Class_816C8080 terrainHeader)
	{
		if (File.Exists($"{savePath}/Statics/{hash}_Terrain.vmdl"))
		{
			string text = File.ReadAllText($"{savePath}/Statics/{hash}_Terrain.vmdl");

			StringBuilder mats = new StringBuilder();

			int i = 0;
			foreach (var staticpart in parts)
			{
				if (terrainHeader.MeshGroups[staticpart.GroupIndex].Dyemap != null)
				{
					mats.AppendLine("{");
					mats.AppendLine($"    from = \"{staticpart.Material.Hash}.vmat\"");
					mats.AppendLine($"    to = \"materials/{staticpart.Material.Hash}.vmat\"");
					mats.AppendLine("},\n");
					i++;
				}
			}

			text = text.Replace("%MATERIALS%", mats.ToString());
			text = text.Replace("%FILENAME%", $"models/{hash}_Terrain.fbx");
			text = text.Replace("%MESHNAME%", hash);

			File.WriteAllText($"{savePath}/Statics/{hash}_Terrain.vmdl", text);

		}
	}

	public static void SaveVMAT(string savePath, string hash, D2Class_AA6D8080 materialHeader)
	{
		StringBuilder vmat = new StringBuilder();
		vmat.AppendLine("Layer0 \n{");

		//If the shader doesnt exist, just use the default complex.shader
		if (!File.Exists($"{savePath}/Source2/PS_{hash}.shader"))
		{
			vmat.AppendLine($"  shader \"complex.shader\"");

			//Use just the first texture for the diffuse
			if (materialHeader.PSTextures.Count > 0)
			{
				vmat.AppendLine($"  TextureColor \"materials/Textures/{materialHeader.PSTextures[0].Texture.Hash}.png\"");
			}
		}
		else
		{
			vmat.AppendLine($"  shader \"ps_{hash}.shader\"");
			vmat.AppendLine("   F_ALPHA_TEST 1");
		}

		foreach (var e in materialHeader.PSTextures)
		{
			if (e.Texture == null)
			{
				continue;
			}

			vmat.AppendLine($"  TextureT{e.TextureIndex} \"materials/Textures/{e.Texture.Hash}.png\"");
		}

		vmat.AppendLine($"Attributes\r\n\t{{\r\n\t\tDebug_Diffuse \"false\"\r\n\t\tDebug_Rough \"false\"\r\n\t\tDebug_Metal \"false\"\r\n\t\tDebug_Normal \"false\"\r\n\t\tDebug_AO \"false\"\r\n\t\tDebug_Emit \"false\"\r\n\t\tDebug_Alpha \"false\"\r\n\t}}");
		vmat.AppendLine("}");

		if (!File.Exists($"{savePath}/Source2/materials/{hash}.vmat"))
		{
			try
			{
				File.WriteAllText($"{savePath}/Source2/materials/{hash}.vmat", vmat.ToString());
			}
			catch (IOException)
			{
			}
		}
	}
}
