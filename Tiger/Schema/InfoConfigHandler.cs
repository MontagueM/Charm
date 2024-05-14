using System.Collections.Concurrent;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class InfoConfigHandler
{
    public bool bOpen = false;
    private readonly ConcurrentDictionary<string, dynamic> _config = new ConcurrentDictionary<string, dynamic>();

    public InfoConfigHandler()
    {
        ConcurrentDictionary<string, Dictionary<string, Dictionary<int, TexInfo>>> mats = new ConcurrentDictionary<string, Dictionary<string, Dictionary<int, TexInfo>>>();
        _config.TryAdd("Materials", mats);
        ConcurrentDictionary<string, string> parts = new ConcurrentDictionary<string, string>();
        _config.TryAdd("Parts", parts);
        ConcurrentDictionary<string, ConcurrentBag<JsonInstance>> instances = new ConcurrentDictionary<string, ConcurrentBag<JsonInstance>>();
        _config.TryAdd("Instances", instances);
        ConcurrentDictionary<string, ConcurrentBag<JsonCubemap>> cubemaps = new ConcurrentDictionary<string, ConcurrentBag<JsonCubemap>>();
        _config.TryAdd("Cubemaps", cubemaps);
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

    public void AddMaterial(IMaterial material)
    {
        if (!material.FileHash.IsValid())
        {
            return;
        }
        Dictionary<string, Dictionary<int, TexInfo>> textures = new Dictionary<string, Dictionary<int, TexInfo>>();
        if (!_config["Materials"].TryAdd(material.FileHash, textures))
        {
            return;
        }
        Dictionary<int, TexInfo> vstex = new Dictionary<int, TexInfo>();
        textures.Add("VS", vstex);
        foreach (var vst in material.EnumerateVSTextures())
        {
            if (vst.Texture != null)
            {
                vstex.Add((int)vst.TextureIndex, new TexInfo { Hash = vst.Texture.Hash, SRGB = vst.Texture.IsSrgb() });
            }
        }
        Dictionary<int, TexInfo> pstex = new Dictionary<int, TexInfo>();
        textures.Add("PS", pstex);
        foreach (var pst in material.EnumeratePSTextures())
        {
            if (pst.Texture != null)
            {
                pstex.Add((int)pst.TextureIndex, new TexInfo { Hash = pst.Texture.Hash, SRGB = pst.Texture.IsSrgb() });
            }
        }
    }

    public void AddPart(MeshPart part, string partName)
    {
        _config["Parts"].TryAdd(partName, part.Material.FileHash);
    }

    public void AddType(string type)
    {
        _config["Type"] = type;
    }

    public void SetMeshName(string meshName)
    {
        _config["MeshName"] = meshName;
    }

    public void AddInstance(string modelHash, float scale, Vector4 quatRotation, Vector3 translation)
    {
        if (!_config["Instances"].ContainsKey(modelHash))
        {
            _config["Instances"][modelHash] = new ConcurrentBag<JsonInstance>();
        }
        _config["Instances"][modelHash].Add(new JsonInstance
        {
            Translation = new[] { translation.X, translation.Y, translation.Z },
            Rotation = new[] { quatRotation.X, quatRotation.Y, quatRotation.Z, quatRotation.W },
            Scale = scale
        });
    }

    public void AddCustomTexture(string material, int index, Texture texture)
    {
        if (!_config["Materials"].ContainsKey(material))
        {
            var textures = new Dictionary<string, Dictionary<int, TexInfo>>();
            textures.Add("PS", new Dictionary<int, TexInfo>());
            _config["Materials"][material] = textures;
        }
        _config["Materials"][material]["PS"].TryAdd(index, new TexInfo { Hash = texture.Hash, SRGB = texture.IsSrgb() });
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


        //im not smart enough to have done this, so i made an ai do it lol
        //this just sorts the "instances" part of the cfg so its ordered by scale
        //makes it easier for instancing models in Hammer/S&Box

        var sortedDict = new ConcurrentDictionary<string, ConcurrentBag<JsonInstance>>();

        // Use LINQ's OrderBy method to sort the values in each array
        // based on the "Scale" key. The lambda expression specifies that
        // the "Scale" property should be used as the key for the order.
        foreach (var keyValuePair in (ConcurrentDictionary<string, ConcurrentBag<JsonInstance>>)_config["Instances"])
        {
            var array = keyValuePair.Value;
            var sortedArray = array.OrderBy(x => x.Scale);

            // Convert the sorted array to a ConcurrentBag
            var sortedBag = new ConcurrentBag<JsonInstance>(sortedArray);

            // Add the sorted bag to the dictionary
            sortedDict.TryAdd(keyValuePair.Key, sortedBag);
        }

        // Finally, update the _config["Instances"] object with the sorted values
        _config["Instances"] = sortedDict;


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

    private struct JsonInstance
    {
        public float[] Translation;
        public float[] Rotation;
        public float Scale;
    }

    private struct JsonCubemap
    {
        public float[] Translation;
        public float[] Rotation;
        public float[] Scale;
    }
}


public struct TexInfo
{
    public string Hash { get; set; }
    public bool SRGB { get; set; }
}
