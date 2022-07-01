using System.Collections.Concurrent;
using System.Text.Json;
using Field.Models;
using Field.Statics;
using Field.Textures;
using Newtonsoft.Json;

namespace Field.General;

public class InfoConfigHandler
{
    public static bool bOpen = false;
    private static Dictionary<string, dynamic> _config = new Dictionary<string, dynamic>();

    public static void MakeFile()
    {
        ConcurrentDictionary<string, Dictionary<string, Dictionary<int, TexInfo>>> mats = new ConcurrentDictionary<string, Dictionary<string, Dictionary<int, TexInfo>>>();
        _config.Add("Materials", mats);
        Dictionary<string, string> parts = new Dictionary<string, string>();
        _config.Add("Parts", parts);
        ConcurrentDictionary<string, List<JsonInstance>> instances = new ConcurrentDictionary<string, List<JsonInstance>>();
        _config.Add("Instances", instances);
        bOpen = true;
    }

    public static void AddMaterial(Material material)
    {
        if (!material.Hash.IsValid())
        {
            return;
        }
        Dictionary<string, Dictionary<int, TexInfo>> textures = new Dictionary<string, Dictionary<int, TexInfo>>();
        if (!_config["Materials"].TryAdd(material.Hash, textures))
        {
            return;
        }
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
        _config["Parts"].Add(partName, part.Material.Hash.GetHashString());
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

    private struct JsonInstance
    {
        public float[] Translation;
        public float[] Rotation;
        public float Scale;
    }

    public static void AddStaticInstances(List<D2Class_406D8080> instances, string staticMesh)
    {
        List<JsonInstance> jsonInstances = new List<JsonInstance>(instances.Count);
        foreach (var instance in instances)
        {
            jsonInstances.Add(new JsonInstance
            {
                Translation = new [] {instance.Position.X, instance.Position.Y, instance.Position.Z},
                Rotation = new [] {instance.Rotation.X, instance.Rotation.Y, instance.Rotation.Z, instance.Rotation.W},
                Scale = instance.Scale.X,
            });
        }
        if (_config["Instances"].ContainsKey(staticMesh))
        {
            _config["Instances"][staticMesh].AddRange(jsonInstances);
        }
        else
        {
            _config["Instances"].TryAdd(staticMesh, jsonInstances);
        }
    }
    
    public static void WriteToFile(string path)
    {
        string s = JsonConvert.SerializeObject(_config, Formatting.Indented);
        File.WriteAllText($"{path}/info.cfg", s);
        _config.Clear();
        bOpen = false;
    }
}

public struct TexInfo
{
    public string Hash  { get; set; }
    public bool SRGB  { get; set; }
}