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
        Dictionary<string, Dictionary<string, Dictionary<int, TexInfo>>> mats = new Dictionary<string, Dictionary<string, Dictionary<int, TexInfo>>>();
        _config.Add("Materials", mats);
        Dictionary<string, string> parts = new Dictionary<string, string>();
        _config.Add("Parts", parts);
        bOpen = true;
    }

    public static void AddMaterial(Material material)
    {
        Dictionary<string, Dictionary<int, TexInfo>> textures = new Dictionary<string, Dictionary<int, TexInfo>>();
        if (_config["Materials"].ContainsKey(material.Hash))
        {
            return;
        }
        _config["Materials"].Add(material.Hash, textures);
        Dictionary<int, TexInfo> vstex = new Dictionary<int, TexInfo>();
        textures.Add("VS", vstex);
        foreach (var vst in material.Header.VSTextures)
        {
            vstex.Add((int)vst.TextureIndex, new TexInfo {Hash = vst.Texture.Hash, SRGB = vst.Texture.IsSrgb() });
        }
        Dictionary<int, TexInfo> pstex = new Dictionary<int, TexInfo>();
        textures.Add("PS", pstex);
        foreach (var pst in material.Header.PSTextures)
        {
            pstex.Add((int)pst.TextureIndex, new TexInfo {Hash = pst.Texture.Hash, SRGB = pst.Texture.IsSrgb() });
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

    public static void SetUnrealInteropPath(string interopPath)
    {
        _config["UnrealInteropPath"] = new string(interopPath.Split("\\Content").Last().ToArray()).TrimStart('\\');
        if (_config["UnrealInteropPath"] == "")
        {
            _config["UnrealInteropPath"] = "Content";
        }
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

public struct TexInfo
{
    public DestinyHash Hash  { get; set; }
    public bool SRGB  { get; set; }
}