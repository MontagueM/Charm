using System.Collections.Concurrent;
using Newtonsoft.Json;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;

namespace Tiger.Exporters;

// TODO: Clean this up
public class MetadataExporter : AbstractExporter
{
    public override void Export(Exporter.ExportEventArgs args)
    {
        Parallel.ForEach(args.Scenes, scene =>
        {
            MetadataScene metadataScene = new(scene);
            metadataScene.WriteToFile(args);
        });
    }
}

class MetadataScene
{
    private readonly ConcurrentDictionary<string, dynamic> _config = new();
    private readonly ExportType _exportType;

    public MetadataScene(ExporterScene scene)
    {
        ConcurrentDictionary<string, JsonMaterial> mats = new();
        _config.TryAdd("Materials", mats);
        ConcurrentDictionary<string, Dictionary<string, string>> parts = new();
        _config.TryAdd("Parts", parts);
        ConcurrentDictionary<string, ConcurrentBag<JsonInstance>> instances = new();
        _config.TryAdd("Instances", instances);
        ConcurrentDictionary<string, ConcurrentBag<JsonCubemap>> cubemaps = new();
        _config.TryAdd("Cubemaps", cubemaps);
        ConcurrentDictionary<string, ConcurrentBag<JsonLight>> pointLights = new();
        _config.TryAdd("Lights", pointLights);
        ConcurrentDictionary<string, ConcurrentBag<JsonDecal>> decals = new ConcurrentDictionary<string, ConcurrentBag<JsonDecal>>();
        _config.TryAdd("Decals", decals);
        ConcurrentDictionary<string, ConcurrentBag<string>> terrainDyemaps = new ConcurrentDictionary<string, ConcurrentBag<string>>();
        _config.TryAdd("TerrainDyemaps", terrainDyemaps);
        ConcurrentDictionary<string, string> atmosphere = new();
        _config.TryAdd("Atmosphere", atmosphere);


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

        foreach (SMapCubemapResource cubemap in scene.Cubemaps)
        {
            AddCubemap(cubemap.CubemapName != null ? cubemap.CubemapName.Value : $"Cubemap_{cubemap.CubemapTexture?.Hash}",
                cubemap.CubemapSize.ToVec3(),
                cubemap.CubemapRotation,
                cubemap.CubemapPosition.ToVec3(),
                cubemap.CubemapTexture != null ? cubemap.CubemapTexture.Hash : "");
        }

        foreach (var mapLight in scene.MapLights)
        {
            AddLight(mapLight);
        }

        foreach (SMapDecalsResource decal in scene.Decals)
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
                            var boxCorners = decal.MapDecals.TagData.DecalProjectionBounds.TagData.InstanceBounds.ElementAt(decal.MapDecals.TagData.DecalProjectionBounds.GetReader(), i);

                            AddDecal(boxCorners.Unk24.ToString(), item.Material.FileHash, location, boxCorners.Corner1, boxCorners.Corner2);
                            AddMaterial(item.Material);
                        }
                    }
                }
            }

        }

        foreach (var dyemaps in scene.TerrainDyemaps)
        {
            foreach (var dyemap in dyemaps.Value)
                AddTerrainDyemap(dyemaps.Key, dyemap);
        }

        foreach (var atmos in scene.Atmosphere)
        {
            AddAtmosphere(atmos);
        }
    }

    public void AddMaterial(IMaterial material)
    {
        if (!material.FileHash.IsValid())
            return;

        var matInfo = new JsonMaterial
        {
            BackfaceCulling = material.Unk0C == 0,
            UsedScopes = material.EnumerateScopes().Select(x => x.ToString()).ToList(),
            Textures = new Dictionary<string, Dictionary<int, TexInfo>>()
        };

        if (!_config["Materials"].TryAdd(material.FileHash, matInfo))
            return;

        Dictionary<int, TexInfo> vstex = new();
        matInfo.Textures.Add("VS", vstex);
        foreach (var vst in material.EnumerateVSTextures())
        {
            if (vst.Texture != null)
                vstex.Add((int)vst.TextureIndex, new TexInfo { Hash = vst.Texture.Hash, SRGB = vst.Texture.IsSrgb(), Dimension = EnumExtensions.GetEnumDescription(vst.Texture.GetDimension()) });
        }

        Dictionary<int, TexInfo> pstex = new();
        matInfo.Textures.Add("PS", pstex);
        foreach (var pst in material.EnumeratePSTextures())
        {
            if (pst.Texture != null)
                pstex.Add((int)pst.TextureIndex, new TexInfo { Hash = pst.Texture.Hash, SRGB = pst.Texture.IsSrgb(), Dimension = EnumExtensions.GetEnumDescription(pst.Texture.GetDimension()) });
        }
    }

    public void AddTextureToMaterial(string material, int index, Texture texture)
    {
        if (!_config["Materials"].ContainsKey(material))
        {
            var matInfo = new JsonMaterial { BackfaceCulling = true, Textures = new Dictionary<string, Dictionary<int, TexInfo>>() };

            Dictionary<int, TexInfo> pstex = new();
            matInfo.Textures.Add("PS", pstex);
            _config["Materials"][material] = matInfo;
        }
        _config["Materials"][material].Textures["PS"].TryAdd(index, new TexInfo { Hash = texture.Hash, SRGB = texture.IsSrgb(), Dimension = EnumExtensions.GetEnumDescription(texture.GetDimension()) });
    }

    public void AddPart(ExporterPart part, string partName)
    {
        if (!_config["Parts"].ContainsKey(part.SubName))
        {
            _config["Parts"][part.SubName] = new Dictionary<string, string>();
        }

        _config["Parts"][part.SubName].TryAdd(partName, part.Material?.FileHash ?? "");
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

    public void AddInstanced(string meshHash, List<Transform> transforms)
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

    public void AddCubemap(string name, Vector3 scale, Vector4 quatRotation, Vector3 translation, string texHash)
    {
        if (!_config["Cubemaps"].ContainsKey(name))
        {
            _config["Cubemaps"][name] = new ConcurrentBag<JsonCubemap>();
        }
        _config["Cubemaps"][name].Add(new JsonCubemap
        {
            Translation = new[] { translation.X, translation.Y, translation.Z },
            Rotation = new[] { quatRotation.X, quatRotation.Y, quatRotation.Z, quatRotation.W },
            Scale = new[] { scale.X, scale.Y, scale.Z },
            Texture = texHash
        });
    }

    public void AddLight(Lights.LightData light)
    {
        if (!_config["Lights"].ContainsKey($"{light.LightType}_{light.Hash}"))
            _config["Lights"][$"{light.LightType}_{light.Hash}"] = new ConcurrentBag<JsonLight>();

        _config["Lights"][$"{light.LightType}_{light.Hash}"].Add(new JsonLight
        {
            Type = light.LightType.ToString(),
            Translation = new[] { light.Transform.Position.X, light.Transform.Position.Y, light.Transform.Position.Z },
            Rotation = new[] { light.Transform.Quaternion.X, light.Transform.Quaternion.Y, light.Transform.Quaternion.Z, light.Transform.Quaternion.W },
            Size = new[] { light.Size.X, light.Size.Y },
            Color = new[] { light.Color.X, light.Color.Y, light.Color.Z },
            Range = light.Range,
            Attenuation = light.Attenuation,
            Cookie = light.Cookie ?? ""
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

    public void AddTerrainDyemap(string modelHash, FileHash dyemapHash)
    {
        if (!_config["TerrainDyemaps"].ContainsKey(modelHash))
        {
            _config["TerrainDyemaps"][modelHash] = new ConcurrentBag<string>();
        }
        _config["TerrainDyemaps"][modelHash].Add(dyemapHash);
    }

    public void AddAtmosphere(SMapAtmosphere atmosphere)
    {
        _config["Atmosphere"].TryAdd("Texture0", $"{atmosphere.Texture0?.Hash}");
        _config["Atmosphere"].TryAdd("TextureUnk0", $"{atmosphere.TextureUnk0?.Hash}");
        _config["Atmosphere"].TryAdd("Texture1", $"{atmosphere.Texture1?.Hash}");
        _config["Atmosphere"].TryAdd("TextureUnk1", $"{atmosphere.TextureUnk1?.Hash}");
        _config["Atmosphere"].TryAdd("Texture2", $"{atmosphere.Texture2?.Hash}");
    }

    public void WriteToFile(Exporter.ExportEventArgs args)
    {
        string path = args.OutputDirectory;

        if (_config["Lights"].Count == 0
            && _config["Materials"].Count == 0
            && _config["Cubemaps"].Count == 0
            && _config["Instances"].Count == 0
            && _config["Parts"].Count == 0
            && _config["Decals"].Count == 0
            && _config["Atmosphere"].Count == 0
            && _exportType is not ExportType.EntityPoints)
            return; //Dont export if theres nothing in the cfg (this is kind of a mess though)

        if (!args.AggregateOutput)
        {
            if (_exportType is ExportType.Static or ExportType.Entity or ExportType.API or ExportType.D1API)
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
        }

        // Are these needed anymore?
        // If theres only 1 part, we need to rename it + the instance to the name of the mesh (unreal imports to fbx name if only 1 mesh inside)
        //if (_config["Parts"].Count == 1)
        //{
        //    var part = _config["Parts"][_config["Parts"].Keys[0]];
        //    //I'm not sure what to do if it's 0, so I guess I'll leave that to fix it in the future if something breakes.
        //    if (_config["Instances"].Count != 0)
        //    {
        //        var instance = _config["Instances"][_config["Instances"].Keys[0]];
        //        _config["Instances"] = new ConcurrentDictionary<string, ConcurrentBag<JsonInstance>>();
        //        _config["Instances"][_config["MeshName"]] = instance;
        //    }
        //    _config["Parts"] = new ConcurrentDictionary<string, string>();
        //    _config["Parts"][_config["MeshName"]] = part;
        //}


        ////this just sorts the "instances" part of the cfg so its ordered by scale
        ////makes it easier for instancing models in Hammer/S&Box
        //var sortedDict = new ConcurrentDictionary<string, ConcurrentBag<JsonInstance>>();

        //foreach (var keyValuePair in (ConcurrentDictionary<string, ConcurrentBag<JsonInstance>>)_config["Instances"])
        //{
        //    var array = keyValuePair.Value;
        //    var sortedArray = array.OrderBy(x => x.Scale[0]);

        //    var sortedBag = new ConcurrentBag<JsonInstance>(sortedArray);
        //    sortedDict.TryAdd(keyValuePair.Key, sortedBag);
        //}
        //_config["Instances"] = sortedDict;

        string s = JsonConvert.SerializeObject(_config, Formatting.Indented);
        if (_config.ContainsKey("MeshName"))
            File.WriteAllText($"{path}/{_config["MeshName"]}_info.cfg", s);
        else
            File.WriteAllText($"{path}/info.cfg", s);
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
        public string Texture;
    }

    private struct JsonLight
    {
        public string Type;
        public float[] Translation;
        public float[] Rotation;
        public float[] Size;
        public float[] Color;
        public float Range;
        public float Attenuation;
        public string Cookie;
    }
    private struct JsonDecal
    {
        public string Material;
        public float[] Origin;
        public float Scale;
        public float[] Corner1;
        public float[] Corner2;
    }

    private struct JsonMaterial
    {
        public bool BackfaceCulling;
        public List<string> UsedScopes;
        public Dictionary<string, Dictionary<int, TexInfo>> Textures;
    }
}

public struct TexInfo
{
    public string Hash { get; set; }
    public string Dimension { get; set; }
    public bool SRGB { get; set; }
}
