﻿using System.Collections.Concurrent;
using Arithmic;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;

namespace Tiger.Exporters;

public class Exporter : Subsystem<Exporter>
{
    private List<ExporterScene> _scenes = new();

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
    }
}


public class ExporterScene
{
    public string Name { get; set; }
    public ExportType Type { get; set; }
    public ConcurrentBag<ExporterMesh> StaticMeshes = new();
    public ConcurrentBag<ExporterMesh> EntityMeshes = new();
    public ConcurrentDictionary<FileHash, List<Transform>> StaticMeshInstances = new();
    public ConcurrentBag<MaterialTexture> ExternalMaterialTextures = new();
    public ConcurrentDictionary<string, SMapDataEntry> EntityPoints = new();
    public ConcurrentBag<CubemapResource> Cubemaps = new();

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

    public void AddStaticInstancesAndParts(FileHash meshHash, List<StaticPart> parts, IEnumerable<SStaticMeshInstanceTransform> instances)
    {
        ExporterMesh mesh = new(meshHash);
        for (int i = 0; i < parts.Count; i++)
        {
            StaticPart part = parts[i];
            mesh.AddPart(meshHash, part, i);
        }
        StaticMeshes.Add(mesh);

        StaticMeshInstances.TryAdd(meshHash, InstancesToTransforms(instances));
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

    private static List<Transform> InstancesToTransforms(IEnumerable<SStaticMeshInstanceTransform> instances)
    {
        return instances.Select(t => new Transform
        {
            Position = t.Position,
            Rotation = Vector4.QuaternionToEulerAngles(t.Rotation),
            Quaternion = t.Rotation,
            Scale = t.Scale
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
        ExternalMaterialTextures.Add(new MaterialTexture { Material = material, Index = index, Texture = texture});
    }

    public void AddEntityPoints(SMapDataEntry points, string meshName)
    {
        Entity entity = FileResourcer.Get().GetFile<Entity>(points.GetEntityHash());
        entity.Load();
        if (entity.Model != null)
        {
            meshName += "_Model";
            // dynamicHandler.AddEntityToScene(entity, entity.Model.Load(ExportDetailLevel.MostDetailed, points.Entity.ModelParentResource), ExportDetailLevel.MostDetailed);
        }

        EntityPoints.TryAdd(meshName, points);
    }

    public void AddCubemap(CubemapResource cubemap)
    {
        Cubemaps.Add(cubemap);
    }
}

public class ExporterMesh
{
    public FileHash Hash { get; set; }
    public List<ExporterPart> Parts { get; } = new();

    public ExporterMesh(FileHash hash)
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
    StaticInMap
}
