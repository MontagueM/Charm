using System.Collections.Concurrent;
using System.Text.Json;
using Field.Models;
using Field.Statics;
using Field.Textures;
using Newtonsoft.Json;

namespace Field.General;

public class InfoConfigHandler
{
    public bool bOpen = false;
    private ConcurrentDictionary<string, dynamic> _config = new ConcurrentDictionary<string, dynamic>();
    private Dictionary<string, dynamic> _matTexs = new Dictionary<string, dynamic>();

    public InfoConfigHandler()
    {
        ConcurrentDictionary<string, Dictionary<string, Dictionary<int, TexInfo>>> mats = new ConcurrentDictionary<string, Dictionary<string, Dictionary<int, TexInfo>>>();
        _config.TryAdd("Materials", mats);
        ConcurrentDictionary<string, string> parts = new ConcurrentDictionary<string, string>();
        _config.TryAdd("Parts", parts);
        ConcurrentDictionary<string, ConcurrentBag<JsonInstance>> instances = new ConcurrentDictionary<string, ConcurrentBag<JsonInstance>>();
        _config.TryAdd("Instances", instances);
        bOpen = true;
    }

    public void Dispose()
    {
        if (bOpen)
        {
            _config.Clear();
            bOpen = false; 
        }
    }

    public void AddMaterial(Material material)
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
            if (vst.Texture != null)
            {
                vstex.Add((int)vst.TextureIndex, new TexInfo {Hash = vst.Texture.Hash, SRGB = vst.Texture.IsSrgb() });
            }
        }
        Dictionary<int, TexInfo> pstex = new Dictionary<int, TexInfo>();
        List<string> pstexhash = new List<string>();
        textures.Add("PS", pstex);
        foreach (var pst in material.Header.PSTextures)
        {
            if (pst.Texture != null)
            {
                pstexhash.Add(pst.Texture.Hash + " " + pst.Texture.IsSrgb());
                pstex.Add((int)pst.TextureIndex, new TexInfo {Hash = pst.Texture.Hash, SRGB = pst.Texture.IsSrgb() });
            }
        }
        _matTexs.TryAdd(material.Hash, pstexhash); //No reason to add VS textures really
    }
    
    public void AddPart(Part part, string partName)
    {
        _config["Parts"].TryAdd(partName, part.Material.Hash.GetHashString());
    }

    public void SetMeshName(string meshName)
    {
        _config["MeshName"] = meshName;
    }

    public void SetUnrealInteropPath(string interopPath)
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

    public void AddStaticInstances(List<D2Class_406D8080> instances, string staticMesh)
    {
        ConcurrentBag<JsonInstance> jsonInstances = new ConcurrentBag<JsonInstance>();
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
            foreach (var jsonInstance in jsonInstances)
            {
                _config["Instances"][staticMesh].Add(jsonInstance);
            }
        }
        else
        {
            _config["Instances"].TryAdd(staticMesh, jsonInstances);
        }
    }
    
    public void WriteToFile(string path)
    {
        // If theres only 1 part, we need to rename it + the instance to the name of the mesh (unreal imports to fbx name if only 1 mesh inside)
        if (_config["Parts"].Count == 1)
        {
            var part = _config["Parts"][_config["Parts"].Keys[0]];
            //I'm not sure what to do if it's 0, so I guess I'll leave that to fix it in the future if something breakes.
            if (_config["Instances"].Count != 0)
            {
                var instance = _config["Instances"][_config["Instances"].Keys[0]];
                _config["Instances"] = new ConcurrentDictionary<string, ConcurrentBag<JsonInstance>>();
                _config["Instances"][_config["MeshName"]] = instance;
            }
            _config["Parts"] = new ConcurrentDictionary<string, string>();
            _config["Parts"][_config["MeshName"]] = part;
        }
        
        string s = JsonConvert.SerializeObject(_config, Formatting.Indented);
        if (_config.ContainsKey("MeshName"))
        {
            File.WriteAllText($"{path}/{_config["MeshName"]}_info.cfg", s);
        }
        else
        {
            File.WriteAllText($"{path}/info.cfg", s);
        }

        //TODO: Check if "Blender export" is checked in the settings
        string m = JsonConvert.SerializeObject(_matTexs, Formatting.Indented);
        string blenderPath = $"{path}/{_config["MeshName"]}_BlenderMats.json";
        File.WriteAllText(blenderPath, m);
             
        Dispose();
    }
}

public struct TexInfo
{
    public string Hash  { get; set; }
    public bool SRGB  { get; set; }
}