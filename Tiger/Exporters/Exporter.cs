using System.Collections.Concurrent;
using ConcurrentCollections;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;
using static Tiger.Schema.StaticMapData_D1;

namespace Tiger.Exporters;

public class Exporter : Subsystem<Exporter>
{
    private readonly List<ExporterScene> _scenes = new();

    public class ExportEventArgs : EventArgs
    {
        public List<ExporterScene> Scenes { get; }
        public string OutputDirectory { get; set; }

        public ExportEventArgs(List<ExporterScene> scenes, string outputDirectory)
        {
            Scenes = scenes;
            OutputDirectory = outputDirectory;
        }
    }

    public delegate void ExportEventHandler(ExportEventArgs e);
    public static event ExportEventHandler ExportEvent = delegate { };


    protected internal override bool Initialise()
    {
        return true;
    }

    public ExporterScene CreateScene(string name, ExportType type)
    {
        var scene = new ExporterScene { Name = name, Type = type };
        _scenes.Add(scene);
        return scene;
    }

    public void Reset()
    {
        _scenes.Clear();
    }

    /// <summary>
    /// We let each exporter handle its exporting itself, e.g.
    /// exporting info to json can be done threaded but FBX cannot
    /// </summary>
    public void Export()
    {
        string outputDirectory = CharmInstance.GetSubsystem<ConfigSubsystem>().GetExportSavePath();
        ExportEvent(new ExportEventArgs(_scenes, outputDirectory));
        Reset();
    }
}

public struct ExportMaterial
{
    public readonly IMaterial Material;
    public readonly MaterialType Type;
    public readonly bool IsTerrain;

    public ExportMaterial(IMaterial material, MaterialType type, bool isTerrain = false)
    {
        Material = material;
        IsTerrain = isTerrain;
        Type = type;
    }

    public override int GetHashCode()
    {
        return (int)Material.FileHash.Hash32;
    }

    public override bool Equals(object? obj)
    {
        return obj is ExportMaterial material && material.Material.FileHash == Material.FileHash;
    }
}

public class ExporterScene
{
    public string Name { get; set; }
    public ExportType Type { get; set; }
    public ConcurrentBag<ExporterMesh> StaticMeshes = new();
    public ConcurrentBag<ExporterMesh> TerrainMeshes = new();
    public ConcurrentBag<ExporterEntity> Entities = new();
    public ConcurrentDictionary<FileHash, List<Transform>> StaticMeshInstances = new();
    public ConcurrentDictionary<FileHash, List<Transform>> ArrangedStaticMeshInstances = new();
    public ConcurrentDictionary<FileHash, List<Transform>> EntityInstances = new();
    public ConcurrentBag<MaterialTexture> ExternalMaterialTextures = new();
    public ConcurrentBag<SMapDataEntry> EntityPoints = new();
    public ConcurrentDictionary<CubemapResource, Transform> Cubemaps = new();
    public ConcurrentBag<SMapLightResource> MapLights = new();
    public ConcurrentDictionary<FileHash, List<Transform>> MapSpotLights = new();
    public ConcurrentBag<SMapDecalsResource> Decals = new();
    private ConcurrentBag<FileHash> _addedEntities = new();
    public ConcurrentHashSet<Texture> Textures = new();
    public ConcurrentHashSet<ExportMaterial> Materials = new();
    public ConcurrentDictionary<FileHash, List<FileHash>> TerrainDyemaps = new();

    public void AddStatic(FileHash meshHash, List<StaticPart> parts)
    {
        ExporterMesh mesh = new(meshHash);
        for (int i = 0; i < parts.Count; i++)
        {
            StaticPart part = parts[i];
            mesh.AddPart(meshHash, part, i);
        }
        StaticMeshes.Add(mesh);
    }

    public void AddTerrain(string meshHash, List<StaticPart> parts)
    {
        ExporterMesh mesh = new(meshHash);
        for (int i = 0; i < parts.Count; i++)
        {
            StaticPart part = parts[i];
            mesh.AddPart(meshHash, part, i);
        }
        TerrainMeshes.Add(mesh);
    }

    public void AddStaticInstancesAndParts(FileHash meshHash, List<StaticPart> parts, IEnumerable<SStaticMeshInstanceTransform> instances)
    {
        ExporterMesh mesh = new(meshHash);
        for (int i = 0; i < parts.Count; i++)
        {
            StaticPart part = parts[i];
            mesh.AddPart(meshHash, part, i);
        }
        StaticMeshes.Add(mesh);

        ArrangedStaticMeshInstances.TryAdd(meshHash, InstancesToTransforms(instances));
    }

    public void AddStaticInstancesToMesh(FileHash modelHash, IEnumerable<SStaticMeshInstanceTransform> instances)
    {
        if (!StaticMeshInstances.ContainsKey(modelHash))
        {
            StaticMeshInstances.TryAdd(modelHash, InstancesToTransforms(instances));
        }
        else
        {
            foreach (Transform transform in InstancesToTransforms(instances))
            {
                StaticMeshInstances[modelHash].Add(transform);
            }
        }
    }

    // D1
    public void AddStaticInstancesToMesh(FileHash modelHash, IEnumerable<InstanceTransform> instances)
    {
        if (!StaticMeshInstances.ContainsKey(modelHash))
        {
            StaticMeshInstances.TryAdd(modelHash, InstancesToTransforms(instances));
        }
        else
        {
            foreach (Transform transform in InstancesToTransforms(instances))
            {
                StaticMeshInstances[modelHash].Add(transform);
            }
        }
    }

    private static List<Transform> InstancesToTransforms(IEnumerable<SStaticMeshInstanceTransform> instances)
    {
        return instances.Select(t => new Transform
        {
            Position = t.Position,
            Rotation = Vector4.QuaternionToEulerAngles(t.Rotation),
            Quaternion = t.Rotation,
            Scale = new Vector3(t.Scale.X, t.Scale.X, t.Scale.X)
        }).ToList();
    }

    // D1
    private static List<Transform> InstancesToTransforms(IEnumerable<InstanceTransform> instances)
    {
        return instances.Select(t => new Transform
        {
            Position = t.Translation.ToVec3(),
            Rotation = Vector4.QuaternionToEulerAngles(t.Rotation),
            Quaternion = t.Rotation,
            Scale = new Vector3(t.Scale.X, t.Scale.X, t.Scale.X)
        }).ToList();
    }

    public void AddStaticInstance(FileHash modelHash, float scale, Vector4 quatRotation, Vector3 translation)
    {
        if (!StaticMeshInstances.ContainsKey(modelHash))
        {
            StaticMeshInstances.TryAdd(modelHash, new());
        }

        StaticMeshInstances[modelHash].Add(new Transform
        {
            Position = translation,
            Rotation = Vector4.QuaternionToEulerAngles(quatRotation),
            Quaternion = quatRotation,
            Scale = new Vector3(scale, scale, scale)
        });
    }

    public void AddTextureToMaterial(string material, int index, Texture texture)
    {
        ExternalMaterialTextures.Add(new MaterialTexture { Material = material, Index = index, Texture = texture });
    }

    public void AddEntityPoints(SMapDataEntry points)
    {
        EntityPoints.Add(points);
    }

    public void AddEntity(FileHash entityHash, List<DynamicMeshPart> parts, List<BoneNode> boneNodes)
    {
        ExporterMesh mesh = new(entityHash);
        for (int i = 0; i < parts.Count; i++)
        {
            DynamicMeshPart part = parts[i];
            if (part.Material == null)
                continue;

            //No longer needed because of EntityModel.cs line 107?
            //if (part.Material.EnumeratePSTextures().Any()) //Dont know if this will 100% "fix" the duplicate meshs that come with entities
            //{
            //    mesh.AddPart(entityHash, part, i);
            //}
            mesh.AddPart(entityHash, part, i);
        }
        Entities.Add(new ExporterEntity { Mesh = mesh, BoneNodes = boneNodes });
    }

    public void AddMapEntity(SMapDataEntry dynamicResource, Entity entity, Transform? transform = null)
    {
        if (!_addedEntities.Contains(entity.Hash)) //Dont want duplicate entities being added
        {
            ExporterMesh mesh = new(entity.Hash);

            _addedEntities.Add(entity.Hash);
            var parts = entity.Model.Load(ExportDetailLevel.MostDetailed, entity.ModelParentResource);
            for (int i = 0; i < parts.Count; i++)
            {
                DynamicMeshPart part = parts[i];
                if (part.Material == null)
                    continue;

                Materials.Add(new ExportMaterial(part.Material, MaterialType.Opaque));
                mesh.AddPart(entity.Hash, part, i);
            }
            Entities.Add(new ExporterEntity { Mesh = mesh, BoneNodes = entity.Skeleton?.GetBoneNodes() });
        }

        if (!EntityInstances.ContainsKey(entity.Hash))
        {
            EntityInstances.TryAdd(entity.Hash, new());
        }

        EntityInstances[entity.Hash].Add(new Transform
        {
            transform = new Transform
            {
                Position = dynamicResource.Translation.ToVec3(),
                Rotation = Vector4.QuaternionToEulerAngles(dynamicResource.Rotation),
                Quaternion = dynamicResource.Rotation,
                Scale = new Vector3(dynamicResource.Translation.W, dynamicResource.Translation.W, dynamicResource.Translation.W)
            };
        }
        EntityInstances[dynamicResource.GetEntityHash()].Add((Transform)transform);
    }

    public void AddMapModel(EntityModel model, Vector4 translation, Vector4 rotation, Vector3 scale, bool transparentsOnly = false)
    {
        ExporterMesh mesh = new(model.Hash);
        if (!_addedEntities.Contains(model.Hash)) //Dont want duplicate entities being added
        {
            _addedEntities.Add(model.Hash);
            var parts = model.Load(ExportDetailLevel.MostDetailed, null, transparentsOnly);
            for (int i = 0; i < parts.Count; i++)
            {
                DynamicMeshPart part = parts[i];

                if (part.Material == null)
                    continue;

                mesh.AddPart(model.Hash, part, i);
            }
            Entities.Add(new ExporterEntity { Mesh = mesh, BoneNodes = null });
        }

        if (!EntityInstances.ContainsKey(model.Hash))
        {
            EntityInstances.TryAdd(model.Hash, new());
        }

        EntityInstances[model.Hash].Add(new Transform
        {
            Position = translation.ToVec3(),
            Rotation = Vector4.QuaternionToEulerAngles(rotation),
            Quaternion = rotation,
            Scale = scale
        });
    }

    public void AddModel(EntityModel model)
    {
        ExporterMesh mesh = new(model.Hash);
        var parts = model.Load(ExportDetailLevel.MostDetailed, null);
        for (int i = 0; i < parts.Count; i++)
        {
            DynamicMeshPart part = parts[i];
            mesh.AddPart(model.Hash, part, i);
        }
        Entities.Add(new ExporterEntity { Mesh = mesh, BoneNodes = null });
    }

    public void AddCubemap(SMapDataEntry entry, CubemapResource cubemap)
    {
        Cubemaps.TryAdd(cubemap, new Transform
        {
            Position = entry.Translation.ToVec3(),
            Rotation = Vector4.QuaternionToEulerAngles(entry.Rotation),
            Quaternion = entry.Rotation,
            Scale = Vector3.One
        });
    }

    public void AddMapLight(SMapLightResource mapLight) //Point
    {
        MapLights.Add(mapLight);
    }
    public void AddMapSpotLight(SMapDataEntry spotLightEntry, SMapShadowingLightResource spotLightResource) //Spot
    {
        if (!MapSpotLights.ContainsKey(spotLightResource.Unk10.Hash))
        {
            MapSpotLights.TryAdd(spotLightResource.Unk10.Hash, new());
        }

        MapSpotLights[spotLightResource.Unk10.Hash].Add(new Transform
        {
            Position = spotLightEntry.Translation.ToVec3(),
            Rotation = Vector4.QuaternionToEulerAngles(spotLightEntry.Rotation),
            Quaternion = spotLightEntry.Rotation,
            Scale = new Vector3(spotLightEntry.Translation.W, spotLightEntry.Translation.W, spotLightEntry.Translation.W)
        });

    }
    public void AddDecals(SMapDecalsResource decal)
    {
        Decals.Add(decal);
    }

    public void AddTerrainDyemap(FileHash modelHash, FileHash dyemapHash)
    {
        if (!TerrainDyemaps.ContainsKey(modelHash))
        {
            TerrainDyemaps.TryAdd(modelHash, new());
        }
        TerrainDyemaps[modelHash].Add(dyemapHash);
    }
}

public class ExporterEntity
{
    public ExporterMesh Mesh { get; set; }
    public List<BoneNode> BoneNodes { get; set; } = new();
}

public class ExporterMesh
{
    public string Hash { get; set; }
    public List<ExporterPart> Parts { get; } = new();

    public ExporterMesh(string hash)
    {
        Hash = hash;
    }

    public void AddPart(string name, MeshPart part, int index)
    {
        ExporterPart exporterPart = new(name, part, index);
        exporterPart.Material = part.Material;
        Parts.Add(exporterPart);
    }
}

public class ExporterPart
{
    public readonly MeshPart MeshPart;
    public readonly string Name;
    public readonly int Index;

    public IMaterial? Material { get; set; }

    public ExporterPart(string name, MeshPart meshPart, int index)
    {
        MeshPart = meshPart;
        Name = $"{name}_Group{meshPart.GroupIndex}_Index{meshPart.Index}_{index}_{meshPart.LodCategory}";
        Index = index;
    }
}

// /// <summary>
// /// A wrapper for any exporter type that can be instanced.
// /// Side effect is can also be used to place objects in the world at not-origin.
// /// </summary>
// public class ExporterInstanced<T> where T : class
// {
//     public T Object { get; }
//     public List<Transform> Transforms { get; set; } = new();
//
//     public ExporterInstanced(FileHash hash)
//     {
//         Object = (T)Activator.CreateInstance(typeof(T), new object[]{hash});
//     }
//
//     public void AddTransforms(IEnumerable<SStaticMeshInstanceTransform> transforms)
//     {
//         Transforms.AddRange(transforms.Select(t => new Transform
//         {
//             Position = t.Position,
//             Rotation = Vector4.QuaternionToEulerAngles(t.Rotation),
//             Quaternion = t.Rotation,
//             Scale = t.Scale
//         }));
//     }
// }

public struct Transform
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector4 Quaternion { get; set; }
    public Vector3 Scale { get; set; }
}

public struct MaterialTexture
{
    public string Material;
    public int Index;
    public Texture Texture;
}

public enum ExportType
{
    Static,
    Entity,
    Map,
    Terrain,
    EntityPoints,
    StaticInMap,
    EntityInMap,
    API,
    D1API,
    MapResource
}
