using System.Collections.Concurrent;
using System.Text.Json;
using Field.Models;
using Field.Statics;
using Newtonsoft.Json;

namespace Field.General;

public class InfoConfigHandler
{
    public bool bOpen = false;
    private ConcurrentDictionary<string, dynamic> _config = new ConcurrentDictionary<string, dynamic>();

    public InfoConfigHandler()
    {
        ConcurrentDictionary<string, dynamic> mats = new ConcurrentDictionary<string, dynamic>();
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

        // Vertex shader
        Dictionary<int, TexInfo> vsTextures = new Dictionary<int, TexInfo>();
        foreach (var vst in material.Header.VSTextures)
        {
            if (vst.Texture != null)
            {
                vsTextures.Add((int)vst.TextureIndex, new TexInfo {Hash = vst.Texture.Hash, SRGB = vst.Texture.IsSrgb() });
            }
        }
        Dictionary<int, List<JsonVector4>> vsConstantBuffers = new Dictionary<int, List<JsonVector4>>();
        // vsConstantBuffers.Add(material.Header.Unk90);  bytes
        if (material.Header.VSVector4Container.IsValid())
        {
            var vsContainerData = Material.GetDataFromContainer(material.Header.VSVector4Container).Select(x => new JsonVector4(x)).ToList();
            vsConstantBuffers.Add(vsContainerData.Count, vsContainerData);
        }
        if (material.Header.UnkA0.Count > 0 && !vsConstantBuffers.ContainsKey(material.Header.UnkA0.Count))
            vsConstantBuffers.Add(material.Header.UnkA0.Count, material.Header.UnkA0.Select(x => new JsonVector4(x.Unk00)).ToList());
        if (material.Header.UnkC0.Count > 0 && !vsConstantBuffers.ContainsKey(material.Header.UnkC0.Count))
            vsConstantBuffers.Add(material.Header.UnkC0.Count, material.Header.UnkC0.Select(x => new JsonVector4(x.Unk00)).ToList());



            // Pixel shader
        Dictionary<int, TexInfo> psTextures = new Dictionary<int, TexInfo>();
        foreach (var pst in material.Header.PSTextures)
        {
            if (pst.Texture != null)
            {
                psTextures.Add((int)pst.TextureIndex, new TexInfo {Hash = pst.Texture.Hash, SRGB = pst.Texture.IsSrgb() });
            }
        }
        Dictionary<int, List<JsonVector4>> psConstantBuffers = new Dictionary<int, List<JsonVector4>>();
        if (material.Header.PSVector4Container.IsValid())
        {
            var psContainerData = Material.GetDataFromContainer(material.Header.PSVector4Container).Select(x => new JsonVector4(x)).ToList();
            psConstantBuffers.Add(psContainerData.Count, psContainerData);
        }
        if (material.Header.Unk2E0.Count > 0 && !vsConstantBuffers.ContainsKey(material.Header.Unk2E0.Count))
            vsConstantBuffers.Add(material.Header.Unk2E0.Count, material.Header.Unk2E0.Select(x => new JsonVector4(x.Unk00)).ToList());
        if (material.Header.Unk300.Count > 0 && !psConstantBuffers.ContainsKey(material.Header.Unk300.Count))
            psConstantBuffers.Add(material.Header.Unk300.Count, material.Header.Unk300.Select(x => new JsonVector4(x.Unk00)).ToList());

        
        // Add to config
        
        Dictionary<string, dynamic> template = new Dictionary<string, dynamic>
        {
            {"VS", new Dictionary<string, dynamic>
                {
                    {"Textures", vsTextures},
                    {"ConstantBuffers", vsConstantBuffers}
                }
            },
            {"PS", new Dictionary<string, dynamic>
                {
                    {"Textures", psTextures},
                    {"ConstantBuffers", psConstantBuffers}
                }
            },
        };
        _config["Materials"].TryAdd(material.Hash, template);
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

    public void AddInstance(string modelHash, float scale, Vector4 quatRotation, Vector3 translation)
    {
        if (!_config["Instances"].ContainsKey(modelHash))
        {
            _config["Instances"][modelHash] = new ConcurrentBag<JsonInstance>();
        }
        _config["Instances"][modelHash].Add(new JsonInstance
        {
            Translation = new [] { translation.X, translation.Y, translation.Z },
            Rotation = new [] { quatRotation.X, quatRotation.Y, quatRotation.Z, quatRotation.W },
            Scale = scale
        });
    }

    public void AddStaticInstances(List<D2Class_406D8080> instances, string staticMesh)
    {
        foreach (var instance in instances)
        {
            AddInstance(staticMesh, instance.Scale.X, instance.Rotation, instance.Position);
        }
    }
    
    public void AddCustomTexture(string material, int index, TextureHeader texture)
    {
        if (!_config["Materials"].ContainsKey(material))
        {
            var textures = new Dictionary<string, Dictionary<int, TexInfo>>();
            textures.Add("PS",  new Dictionary<int, TexInfo>());
            _config["Materials"][material] = textures;
        }
        _config["Materials"][material]["PS"].TryAdd(index, new TexInfo { Hash = texture.Hash, SRGB = texture.IsSrgb()});
    }
    
    public void WriteToFile(string path)
    {
        List<string> s2 = new List<string>();
        foreach (var material in _config["Materials"])
        {
            s2.Add($"ps_{material.Key}.vfx");
        }
        if(!Directory.Exists($"{path}/Shaders/Source2/"))
            Directory.CreateDirectory($"{path}/Shaders/Source2/");
        
        File.WriteAllLines($"{path}/Shaders/Source2/_S2BuildList.txt", s2);

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
        Dispose();
    }
}

public struct TexInfo
{
    public string Hash  { get; set; }
    public bool SRGB  { get; set; }
}


public struct JsonVector4
{
    public JsonVector4(Vector4 inVec4)
    {
        X = inVec4.X;
        Y = inVec4.Y;
        Z = inVec4.Z;
        W = inVec4.W;
    }

    public float X  { get; set; }
    public float Y  { get; set; }
    public float Z  { get; set; }
    public float W  { get; set; }
}
