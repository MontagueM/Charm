using System.Collections.Concurrent;
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
        ConcurrentDictionary<string, ConcurrentBag<JsonLight>> pointLights = new();
        _config.TryAdd("Lights", pointLights);
        ConcurrentDictionary<string, ConcurrentBag<JsonDecal>> decals = new ConcurrentDictionary<string, ConcurrentBag<JsonDecal>>();
        _config.TryAdd("Decals", decals);

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
        foreach (var meshInstanced in scene.EntityInstances)
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
        foreach (var mapLight in scene.MapLights)
        {
            for (int i = 0; i < mapLight.Unk10.TagData.Unk30.Count; i++)
            {
               AddLight(
                    mapLight.Unk10.TagData.Unk30[i].UnkD0.Hash,
                    "Point",
                    mapLight.Unk10.TagData.Unk40[i].Translation,
                    mapLight.Unk10.TagData.Unk40[i].Rotation,
                    new Vector2(mapLight.Unk10.TagData.Unk30[i].UnkA0.W, mapLight.Unk10.TagData.Unk30[i].UnkB0.W), //Not right
                    (mapLight.Unk10.TagData.Unk30[i].UnkD0.TagData.Unk40.Count > 0 ? mapLight.Unk10.TagData.Unk30[i].UnkD0.TagData.Unk40[0].Vec : mapLight.Unk10.TagData.Unk30[i].UnkD0.TagData.Unk60[0].Vec));
            }
        }
        foreach(SMapDecalsResource decal in scene.Decals)
        {
            if (decal.MapDecals is not null)
            {
                foreach (var item in decal.MapDecals.TagData.DecalResources)
                {
                    // Check if the index is within the bounds of the second list
                    if (item.StartIndex >= 0 && item.StartIndex < decal.MapDecals.TagData.Locations.Count)
                    {
                        // Loop through the second list based on the given parameters
                        for (int i = item.StartIndex; i < item.StartIndex + item.Count && i < decal.MapDecals.TagData.Locations.Count; i++)
                        {
                            var location = decal.MapDecals.TagData.Locations[i].Location;
                            var boxCorners = decal.MapDecals.TagData.DecalProjectionBounds.TagData.InstanceBounds[i];

                            AddDecal(boxCorners.Unk24.ToString(), item.Material.FileHash, location, boxCorners.Corner1, boxCorners.Corner2);
                            AddMaterial(item.Material);
                        }
                    }
                }
            }

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
                Scale = new[] { transform.Scale.X, transform.Scale.Y, transform.Scale.Z }
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

    public void AddLight(string name, string type, Vector4 translation, Vector4 quatRotation, Vector2 size, Vector4 color)
    {
        //Idk how color/intensity is handled, so if its above 1 just bring it down
        float R = color.X > 1 ? color.X / 100 : color.X;
        float G = color.Y > 1 ? color.Y / 100 : color.Y;
        float B = color.Z > 1 ? color.Z / 100 : color.Z;

        if (!_config["Lights"].ContainsKey(name))
        {
            _config["Lights"][name] = new ConcurrentBag<JsonLight>();
        }
        _config["Lights"][name].Add(new JsonLight
        {
            Type = type,
            Translation = new[] { translation.X, translation.Y, translation.Z },
            Rotation = new[] { quatRotation.X, quatRotation.Y, quatRotation.Z, quatRotation.W },
            Size = new[] { size.X, size.Y },
            Color = new[] { R, G, B }
        });
    }

    public void AddDecal(string boxhash, string materialName, Vector4 origin, Vector4 corner1, Vector4 corner2)
    {
        if (!_config["Decals"].ContainsKey(boxhash))
        {
            _config["Decals"][boxhash] = new ConcurrentBag<JsonDecal>();
        }
        _config["Decals"][boxhash].Add(new JsonDecal
        {
            Material = materialName,
            Origin = new[] { origin.X, origin.Y, origin.Z },
            Scale = origin.W,
            Corner1 = new[] { corner1.X, corner1.Y, corner1.Z },
            Corner2 = new[] { corner2.X, corner2.Y, corner2.Z }
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
        else if (_exportType is ExportType.StaticInMap or ExportType.EntityInMap)
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
            var sortedArray = array.OrderBy(x => x.Scale[0]);

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
        public float[] Scale;
    }

    private struct JsonCubemap
    {
        public float[] Translation;
        public float[] Rotation;
        public float[] Scale;
    }

    private struct JsonLight
    {
        public string Type;
        public float[] Translation;
        public float[] Rotation;
        public float[] Size;
        public float[] Color;
    }
    private struct JsonDecal
    {
        public string Material;
        public float[] Origin;
        public float Scale;
        public float[] Corner1;
        public float[] Corner2;
    }
}

public struct TexInfo
{
    public string Hash { get; set; }
    public bool SRGB { get; set; }
}
