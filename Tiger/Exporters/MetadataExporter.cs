﻿using System.Collections.Concurrent;
using Newtonsoft.Json;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;

namespace Tiger.Exporters;

public class MetadataExporter : AbstractExporter
{

    public override void Export(Exporter.ExportEventArgs args)
    {
        Parallel.ForEach(args.Scenes, scene =>
        {
            MetadataScene metadataScene = new(scene);
            metadataScene.WriteToFile(args.OutputDirectory);
        });
    }
}

class MetadataScene
{
    private readonly ConcurrentDictionary<string, dynamic> _config = new();
    private readonly ExportType _exportType;

    public MetadataScene(ExporterScene scene)
    {
        ConcurrentDictionary<string, Dictionary<string, Dictionary<int, TexInfo>>> mats = new();
        _config.TryAdd("Materials", mats);
        ConcurrentDictionary<string, string> parts = new();
        _config.TryAdd("Parts", parts);
        ConcurrentDictionary<string, ConcurrentBag<JsonInstance>> instances = new();
        _config.TryAdd("Instances", instances);
        ConcurrentDictionary<string, ConcurrentBag<JsonCubemap>> cubemaps = new();
        _config.TryAdd("Cubemaps", cubemaps);

        if (ConfigSubsystem.Get().GetUnrealInteropEnabled())
        {
            SetUnrealInteropPath(ConfigSubsystem.Get().GetUnrealInteropPath());
        }

        SetType(scene.Type.ToString());
        _exportType = scene.Type;
        SetMeshName(scene.Name);

        foreach (var mesh in scene.StaticMeshes)
        {
            foreach (var part in mesh.Parts)
            {
                if (part.Material != null)
                {
                    AddMaterial(part.Material);
                }
                AddPart(part, part.Name);
            }
        }

        foreach (var meshInstanced in scene.StaticMeshInstances)
        {
            AddInstanced(meshInstanced.Key, meshInstanced.Value);
        }

        foreach (ExporterEntity entityMesh in scene.Entities)
        {
            foreach (var part in entityMesh.Mesh.Parts)
            {
                if (part.Material != null)
                {
                    AddMaterial(part.Material);
                }
                AddPart(part, part.Name);
            }
        }

        foreach (MaterialTexture texture in scene.ExternalMaterialTextures)
        {
            AddTextureToMaterial(texture.Material, texture.Index, texture.Texture);
        }

        foreach (CubemapResource cubemap in scene.Cubemaps)
        {
            AddCubemap(cubemap.CubemapName, cubemap.CubemapSize.ToVec3(), cubemap.CubemapRotation, cubemap.CubemapPosition.ToVec3());
        }
    }

    public void AddMaterial(IMaterial material)
    {
        if (!material.FileHash.IsValid())
        {
            return;
        }
        Dictionary<string, Dictionary<int, TexInfo>> textures = new();
        if (!_config["Materials"].TryAdd(material.FileHash, textures))
        {
            return;
        }
        Dictionary<int, TexInfo> vstex = new();
        textures.Add("VS", vstex);
        foreach (var vst in material.EnumerateVSTextures())
        {
            if (vst.Texture != null)
            {
                vstex.Add((int)vst.TextureIndex, new TexInfo { Hash = vst.Texture.Hash, SRGB = vst.Texture.IsSrgb() });
            }
        }
        Dictionary<int, TexInfo> pstex = new();
        textures.Add("PS", pstex);
        foreach (var pst in material.EnumeratePSTextures())
        {
            if (pst.Texture != null)
            {
                pstex.Add((int)pst.TextureIndex, new TexInfo { Hash = pst.Texture.Hash, SRGB = pst.Texture.IsSrgb() });
            }
        }
    }

    public void AddPart(ExporterPart part, string partName)
    {
        _config["Parts"].TryAdd(partName, part.Material?.FileHash ?? "");
    }

    public void SetType(string type)
    {
        _config["Type"] = type;
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

    public void AddInstanced(FileHash meshHash, List<Transform> transforms)
    {
        if (!_config["Instances"].ContainsKey(meshHash))
        {
            _config["Instances"][meshHash] = new ConcurrentBag<JsonInstance>();
        }
        foreach (Transform transform in transforms)
        {
            _config["Instances"][meshHash].Add(new JsonInstance
            {
                Translation = new[] { transform.Position.X, transform.Position.Y, transform.Position.Z },
                Rotation = new[] { transform.Quaternion.X, transform.Quaternion.Y, transform.Quaternion.Z, transform.Quaternion.W },
                Scale = transform.Scale.X
            });
        }
    }

    public void AddTextureToMaterial(string material, int index, Texture texture)
    {
        if (!_config["Materials"].ContainsKey(material))
        {
            var textures = new Dictionary<string, Dictionary<int, TexInfo>>();
            textures.Add("PS", new Dictionary<int, TexInfo>());
            _config["Materials"][material] = textures;
        }
        _config["Materials"][material]["PS"].TryAdd(index, new TexInfo { Hash = texture.Hash, SRGB = texture.IsSrgb() });
    }

    public void AddCubemap(string name, Vector3 scale, Vector4 quatRotation, Vector3 translation)
    {
        if (!_config["Cubemaps"].ContainsKey(name))
        {
            _config["Cubemaps"][name] = new ConcurrentBag<JsonCubemap>();
        }
        _config["Cubemaps"][name].Add(new JsonCubemap
        {
            Translation = new[] { translation.X, translation.Y, translation.Z },
            Rotation = new[] { quatRotation.X, quatRotation.Y, quatRotation.Z, quatRotation.W },
            Scale = new[] { scale.X, scale.Y, scale.Z }
        });
    }

    public void WriteToFile(string path)
    {
        if (_exportType is ExportType.Static or ExportType.Entity)
        {
            path = Path.Join(path, _config["MeshName"]);
        }
        else if (_exportType is ExportType.Map or ExportType.Terrain or ExportType.EntityPoints)
        {
            path = Path.Join(path, "Maps");
        }
        else if (_exportType is ExportType.StaticInMap)
        {
            return;
        }

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
    }

    private struct JsonInstance
    {
        public float[] Translation;
        public float[] Rotation;  // Quaternion
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
