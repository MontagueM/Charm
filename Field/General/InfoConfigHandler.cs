using System.Text.Json;
using Field.Models;
using Field.Textures;

namespace Field.General;

public class InfoConfigHandler
{
    public static bool bOpen = false;
    private static Dictionary<string, dynamic> _config = new Dictionary<string, dynamic>();

    public static void MakeFile()
    {
        Dictionary<string, Dictionary<string, Dictionary<int, string>>> mats = new Dictionary<string, Dictionary<string, Dictionary<int, string>>>();
        _config.Add("Materials", mats);
        Dictionary<string, string> parts = new Dictionary<string, string>();
        _config.Add("Parts", parts);
        bOpen = true;
    }

    public static void AddMaterial(Material material)
    {
        Dictionary<string, Dictionary<int, string>> textures = new Dictionary<string, Dictionary<int, string>>();
        if (_config["Materials"].ContainsKey(material.Hash))
        {
            return;
        }
        _config["Materials"].Add(material.Hash, textures);
        Dictionary<int, string> vstex = new Dictionary<int, string>();
        textures.Add("VS", vstex);
        foreach (var vst in material.Header.VSTextures)
        {
            vstex.Add((int)vst.TextureIndex, vst.Texture.Hash);
        }
        Dictionary<int, string> pstex = new Dictionary<int, string>();
        textures.Add("PS", pstex);
        foreach (var pst in material.Header.PSTextures)
        {
            pstex.Add((int)pst.TextureIndex, pst.Texture.Hash);
        }
    }
    
    public static void AddPart(Part part, string partName)
    {
        _config["Parts"].Add(partName, part.Material.Hash);
    }

    public static void SetMeshName(string meshName)
    {
        _config["MeshName"] = meshName;
    }

    public static void WriteToFile(string path)
    {
        MemoryStream ms = new MemoryStream();
        JsonSerializer.Serialize(ms, _config, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllBytes($"{path}/info.cfg", ms.ToArray());
        _config.Clear();
        bOpen = false;
    }
}