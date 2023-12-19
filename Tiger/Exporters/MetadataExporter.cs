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
        ConcurrentDictionary<string, ConcurrentBag<string>> terrainDyemaps = new ConcurrentDictionary<string, ConcurrentBag<string>>();
        _config.TryAdd("TerrainDyemaps", terrainDyemaps);

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
            AddCubemap(cubemap.CubemapName,
                cubemap.CubemapSize.ToVec3(),
                cubemap.CubemapRotation,
                cubemap.CubemapPosition.ToVec3(),
                cubemap.CubemapTexture != null ? cubemap.CubemapTexture.Hash : "");
        }
        foreach (var mapLight in scene.MapLights)
        {
            for (int i = 0; i < mapLight.Unk10.TagData.Unk30.Count; i++)
            {
                var data = Strategy.CurrentStrategy == TigerStrategy.DESTINY2_SHADOWKEEP_2601 ? mapLight.Unk10.TagData.Unk30[i].UnkCC : mapLight.Unk10.TagData.Unk30[i].UnkD0;
                if (data is null)
                    continue;
                AddLight(
                    data.Hash,
                    "Point",
                    mapLight.Unk10.TagData.Unk40[i].Translation,
                    mapLight.Unk10.TagData.Unk40[i].Rotation,
                    new Vector2(1,1), //new Vector2(mapLight.Unk10.TagData.Unk30[i].UnkA0.W, mapLight.Unk10.TagData.Unk30[i].UnkB0.W), //Not right
                    (data.TagData.Unk40.Count > 0 ? data.TagData.Unk40[0].Vec : data.TagData.Unk60[0].Vec));
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
        foreach (var mapLight in scene.MapSpotLights)
        {
            foreach(var entry in mapLight.Value)
            {
                AddLight(
                    mapLight.Key,
                    "Spot",
                    new Vector4(entry.Position.X, entry.Position.Y, entry.Position.Z, 1),
                    entry.Quaternion,
                    new Vector2(1.0, 1.0),
                    new Vector4(1.0,1.0,1.0,1.0));
            }
        }

        foreach(var dyemaps in scene.TerrainDyemaps)
        {
            foreach(var dyemap in dyemaps.Value)
                AddTerrainDyemap(dyemaps.Key, dyemap);
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

    public void AddTerrainDyemap(string modelHash, FileHash dyemapHash)
    {
        if (!_config["TerrainDyemaps"].ContainsKey(modelHash))
        {
            _config["TerrainDyemaps"][modelHash] = new ConcurrentBag<string>();
        }
        _config["TerrainDyemaps"][modelHash].Add(dyemapHash);
    }

    public void WriteToFile(string path)
    {
        if (_config["Lights"].Count == 0
            && _config["Materials"].Count == 0
            && _config["Cubemaps"].Count == 0
            && _config["Instances"].Count == 0
            && _config["Parts"].Count == 0
            && _config["Decals"].Count == 0
            && _exportType is not ExportType.EntityPoints)
            return; //Dont export if theres nothing in the cfg (this is kind of a mess though)

        if (_exportType is ExportType.Static or ExportType.Entity or ExportType.API)
        {
            path = Path.Join(path, _config["MeshName"]);
        }
        else if (_exportType is ExportType.Map or ExportType.Terrain or ExportType.EntityPoints or ExportType.MapResource)
        {
            path = Path.Join(path, "Maps");
        }
        else if (_exportType is ExportType.StaticInMap or ExportType.EntityInMap)
        {
            return;
        }

        //// If theres only 1 part, we need to rename it + the instance to the name of the mesh (unreal imports to fbx name if only 1 mesh inside)
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


        //this just sorts the "instances" part of the cfg so its ordered by scale
        //makes it easier for instancing models in Hammer/S&Box
        var sortedDict = new ConcurrentDictionary<string, ConcurrentBag<JsonInstance>>();

        foreach (var keyValuePair in (ConcurrentDictionary<string, ConcurrentBag<JsonInstance>>)_config["Instances"])
        {
            var array = keyValuePair.Value;
            var sortedArray = array.OrderBy(x => x.Scale[0]);

            var sortedBag = new ConcurrentBag<JsonInstance>(sortedArray);
            sortedDict.TryAdd(keyValuePair.Key, sortedBag);
        }
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
        public string Texture;
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
