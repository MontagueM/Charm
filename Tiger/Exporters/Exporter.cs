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
    private GlobalExporterScene? _globalScene = null;

    public class ExportEventArgs : EventArgs
    {
        public List<ExporterScene> Scenes { get; }
        public string OutputDirectory { get; set; }
        public bool AggregateOutput { get; set; }

        public ExportEventArgs(List<ExporterScene> scenes, string outputDirectory, bool aggregateOutput)
        {
            Scenes = scenes;
            OutputDirectory = outputDirectory;
            AggregateOutput = aggregateOutput;
        }
    }

    public delegate void ExportEventHandler(ExportEventArgs e);
    public static event ExportEventHandler ExportEvent = delegate { };


    protected internal override bool Initialise()
    {
        return true;
    }

    public ExporterScene CreateScene(string name, ExportType type, DataExportType dataType = DataExportType.Individual)
    {
        var scene = new ExporterScene { Name = name, Type = type, DataType = dataType };
        _scenes.Add(scene);
        return scene;
    }

    /// <summary>
    /// Gets or creates a Global Exporter scene. 
    /// A Global scene can be used to store anything that needs to be accessed across other scenes.
    /// Map Vertex AO for Terrain being one example. (Map AO and Terrain are stored in different data tables)
    /// The Global Scene gets reset with each full export, and anything added to can be exported if implemented in GlobalExporter
    /// </summary>
    /// <returns>The Global Scene</returns>
    public GlobalExporterScene GetOrCreateGlobalScene()
    {
        if (_globalScene == null)
            _globalScene = new GlobalExporterScene();

        return _globalScene;
    }

    /// <summary>
    /// Only returns the Global Scene instead of creating then returning
    /// </summary>
    /// <returns>The Global Scene if it exists, null if not</returns>
    public GlobalExporterScene? GetGlobalScene()
    {
        if (_globalScene == null)
            return null;

        return _globalScene;
    }

    public void Reset()
    {
        _scenes.Clear();
        _globalScene?.Clear();
        _globalScene = null;
    }

    /// <summary>
    /// We let each exporter handle its exporting itself, e.g.
    /// exporting info to json can be done threaded but FBX cannot
    /// </summary>
    public void Export(string? outputDirectory = null)
    {
        bool aggregateOutput = outputDirectory is not null;
        if (outputDirectory is null)
            outputDirectory = TigerInstance.GetSubsystem<ConfigSubsystem>().GetExportSavePath();

        ExportEvent(new ExportEventArgs(_scenes, outputDirectory, aggregateOutput));
        Reset();
    }
}

public class GlobalExporterScene : ExporterScene
{
    private ConcurrentBag<dynamic> _objects = new();

    public GlobalExporterScene()
    {
        Name = "Global";
        Type = ExportType.Global;
    }

    /// <summary>
    /// Adds an item to the Global Scene's objects.
    /// </summary>
    /// <param name="item">The dynamic item to add.</param>
    /// <param name="isUnique">Should only one of this item exist?</param>
    public void AddToGlobalScene(dynamic item, bool isUnique = false)
    {
        if (_objects == null)
            _objects = new ConcurrentBag<dynamic>();

        // Check if an item of the same type already exists (if there should only be one)
        dynamic type = item.GetType();
        if (isUnique && _objects.Any(existing => existing.GetType() == type))
            throw new InvalidOperationException($"A unique item of type {type.Name} already exists in the Global Scene.");

        _objects.Add(item);
    }

    /// <summary>
    /// Attempts to get an item of a specific type from the Global Scene's objects.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="item">The retrieved item if found.</param>
    /// <returns>True if an item of the specified type was found; otherwise, false.</returns>
    public bool TryGetItem<T>(out T item)
    {
        item = default;

        foreach (dynamic existing in _objects)
        {
            if (existing is T)
            {
                item = (T)existing; // Safely cast to T
                return true;
            }
        }

        return false;
    }

    public IEnumerable<T> GetAllOfType<T>()
    {
        return _objects.OfType<T>();
    }

    public bool Any<T>()
    {
        return _objects.Any(item => item is T);
    }

    public void Clear()
    {
        _objects.Clear();
    }
}

public class ExporterScene
{
    public string Name { get; set; }
    public DataExportType DataType { get; set; }
    public ExportType Type { get; set; }

    public ConcurrentBag<ExporterMesh> TerrainMeshes = new();
    public ConcurrentBag<ExporterMesh> StaticMeshes = new();
    public ConcurrentBag<ExporterEntity> Entities = new();
    public ConcurrentDictionary<string, List<Transform>> StaticMeshInstances = new();
    public ConcurrentDictionary<string, List<Transform>> ArrangedStaticMeshInstances = new();
    public ConcurrentDictionary<string, List<Transform>> EntityInstances = new();
    public ConcurrentBag<SMapDataEntry> EntityPoints = new();
    public ConcurrentHashSet<Texture> ExternalTextures = new();
    public ConcurrentHashSet<ExportMaterial> Materials = new();
    public ConcurrentDictionary<string, List<FileHash>> TerrainDyemaps = new();
    private ConcurrentDictionary<string, bool> _addedEntities = new();

    public void AddStatic(string meshHash, List<StaticPart> parts)
    {
        ExporterMesh mesh = new(meshHash);
        for (int i = 0; i < parts.Count; i++)
        {
            StaticPart part = parts[i];
            mesh.AddPart(meshHash, part, i);
        }
        StaticMeshes.Add(mesh);
    }

    public void AddStaticInstancesAndParts(string meshHash, List<StaticPart> parts, IEnumerable<SStaticMeshInstanceTransform> instances)
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

    public void AddStaticInstancesToMesh(string modelHash, IEnumerable<SStaticMeshInstanceTransform> instances)
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
    public void AddStaticInstancesToMesh(string modelHash, IEnumerable<InstanceTransform> instances)
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

    public void AddStaticInstance(FileHash modelHash, Vector3 scale, Vector4 quatRotation, Vector3 translation)
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
            Scale = scale
        });
    }

    public void AddEntityPoints(SMapDataEntry points)
    {
        EntityPoints.Add(points);
    }

    public void AddEntity(FileHash entityHash, List<DynamicMeshPart> parts, List<BoneNode> boneNodes, DestinyGenderDefinition gender = DestinyGenderDefinition.None)
    {
        ExporterMesh mesh = new(entityHash);
        string name = $"{entityHash}" + (gender == DestinyGenderDefinition.None ? "" : $"_{gender}");
        for (int i = 0; i < parts.Count; i++)
        {
            DynamicMeshPart part = parts[i];
            if (part.Material == null)
                continue;

            mesh.AddPart(name, part, i);
        }
        Entities.Add(new ExporterEntity { Mesh = mesh, BoneNodes = boneNodes });
    }

    public void AddMapEntity(SMapDataEntry entry, Transform? transform = null)
    {
        List<Entity> ents = new() { entry.Entity };
        ents.AddRange(entry.Entity.GetEntityChildren());
        foreach (var ent in ents)
        {
            if (_addedEntities.TryAdd(ent.Hash, true)) // Dont want duplicate entities being added
            {
                ExporterMesh mesh = new(ent.Hash);
                List<DynamicMeshPart> parts = ent.Load(ExportDetailLevel.MostDetailed);

                for (int i = 0; i < parts.Count; i++)
                {
                    DynamicMeshPart part = parts[i];
                    if (part.Material == null)
                        continue;

                    mesh.AddPart(ent.Hash, part, i);
                }
                Entities.Add(new ExporterEntity { Mesh = mesh, BoneNodes = ent.Skeleton?.GetBoneNodes() });
            }

            EntityInstances.TryAdd(ent.Hash, new());
            if (transform is null)
            {
                transform = new Transform
                {
                    Position = entry.Transfrom.Translation.ToVec3(),
                    Rotation = Vector4.QuaternionToEulerAngles(entry.Transfrom.Rotation),
                    Quaternion = entry.Transfrom.Rotation,
                    Scale = new Vector3(entry.Transfrom.Translation.W, entry.Transfrom.Translation.W, entry.Transfrom.Translation.W)
                };
            }
            EntityInstances[ent.Hash].Add((Transform)transform);
        }
    }

    public void AddMapModel(EntityModel model, Transform transform, bool transparentsOnly = false)
    {
        ExporterMesh mesh = new(model.Hash);

        if (_addedEntities.TryAdd(model.Hash, true)) // Dont want duplicate entities being added
        {
            List<DynamicMeshPart> parts = model.Load(ExportDetailLevel.MostDetailed, null, transparentsOnly);
            for (int i = 0; i < parts.Count; i++)
            {
                DynamicMeshPart part = parts[i];
                mesh.AddPart(model.Hash, part, i);
            }
            Entities.Add(new ExporterEntity { Mesh = mesh, BoneNodes = null });
        }

        EntityInstances.TryAdd(model.Hash, new());
        EntityInstances[model.Hash].Add(transform);
    }

    public void AddModel(EntityModel model)
    {
        ExporterMesh mesh = new(model.Hash);
        List<DynamicMeshPart> parts = model.Load(ExportDetailLevel.MostDetailed, null);
        for (int i = 0; i < parts.Count; i++)
        {
            DynamicMeshPart part = parts[i];
            mesh.AddPart(model.Hash, part, i);
        }
        Entities.Add(new ExporterEntity { Mesh = mesh, BoneNodes = null });
    }

    public void AddTerrainDyemap(FileHash modelHash, FileHash dyemapHash)
    {
        if (!TerrainDyemaps.ContainsKey(modelHash))
        {
            TerrainDyemaps.TryAdd(modelHash, new());
        }
        TerrainDyemaps[modelHash].Add(dyemapHash);
    }

    public void AddMapModelParts(string name, List<DynamicMeshPart> parts, Transform transform)
    {
        ExporterMesh mesh = new(name);

        if (_addedEntities.TryAdd(name, true)) // Dont want duplicate entities being added
        {
            for (int i = 0; i < parts.Count; i++)
            {
                DynamicMeshPart part = parts[i];
                mesh.AddPart(name, part, i);
            }
            Entities.Add(new ExporterEntity { Mesh = mesh, BoneNodes = null });
        }

        EntityInstances.TryAdd(name, new());
        EntityInstances[name].Add(transform);
    }

    public void AddMapModelParts(string name, List<MeshPart> parts, Transform transform)
    {
        ExporterMesh mesh = new(name);
        for (int i = 0; i < parts.Count; i++)
        {
            MeshPart part = parts[i];
            mesh.AddPart(name, part, i);
        }
        StaticMeshes.Add(mesh);

        if (!StaticMeshInstances.ContainsKey(name))
            StaticMeshInstances.TryAdd(name, new());

        StaticMeshInstances[name].Add(transform);
    }

    public void AddTerrain(string meshHash, List<StaticPart> parts, ulong? id = null, int index = 0)
    {
        ExporterMesh mesh = new(meshHash, id, index);
        for (int i = 0; i < parts.Count; i++)
        {
            StaticPart part = parts[i];
            mesh.AddPart(meshHash, part, i);
        }
        TerrainMeshes.Add(mesh);
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

    // These are currently only used on Terrain, might be useful later for other things?
    public ulong? ID { get; set; }
    public int Index { get; set; } = 0;

    public ExporterMesh(FileHash hash, ulong? id = null, int index = 0)
    {
        Hash = hash;
        ID = id;
        Index = index;
    }
    public ExporterMesh(string hash, ulong? id = null, int index = 0)
    {
        Hash = hash;
        ID = id;
        Index = index;
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
    public readonly string SubName;
    public readonly string Name;
    public readonly int Index;

    public Material? Material { get; set; }

    public ExporterPart(string name, MeshPart meshPart, int index)
    {
        MeshPart = meshPart;
        SubName = name;
        Name = $"{name}_Group{meshPart.GroupIndex}_Index{meshPart.Index}_{index}_{meshPart.LodCategory}";
        Index = index;
    }
}

public struct ExportMaterial
{
    public readonly Material Material;
    public readonly bool IsTerrain;

    public ExportMaterial(Material material, bool isTerrain = false)
    {
        Material = material;
        IsTerrain = isTerrain;
    }

    public override int GetHashCode()
    {
        return (int)Material.Hash.Hash32;
    }

    public override bool Equals(object? obj)
    {
        return obj is ExportMaterial material && material.Material.Hash == Material.Hash;
    }
}

public struct Transform
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector4 Quaternion { get; set; }
    public Vector3 Scale { get; set; }
    public float Order { get; set; }
}

public struct MaterialTexture
{
    public string Material;
    public int Index;
    public Texture Texture;
}

// Used for metadata exporter
public enum DataExportType
{
    Individual,
    Map
}

public enum ExportType
{
    Map,
    Global,
    Statics,
    Entities,
    Terrain,
    SkyObjects,
    RoadDecals,
    Decorators,
    WaterDecals,

    EntityPoints,
    API,
    D1API
}
