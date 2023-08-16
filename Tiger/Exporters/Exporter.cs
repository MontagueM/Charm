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

    public ExporterScene CreateScene(string name)
    {
        var scene = new ExporterScene { Name = name };
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
    public List<ExporterMesh> StaticMeshes = new();
    public List<ExporterMesh> EntityMeshes = new();
    public List<ExporterInstance<ExporterMesh>> StaticMeshInstances = new();

    public void AddStatic(string name, List<StaticPart> parts)
    {
        ExporterMesh mesh = new();
        for (int i = 0; i < parts.Count; i++)
        {
            StaticPart part = parts[i];
            mesh.AddPart(name, part, i);
        }
        StaticMeshes.Add(mesh);
    }

    public void AddStaticInstances(string name, List<StaticPart> parts, IEnumerable<SStaticMeshInstanceTransform> instances)
    {
        ExporterInstance<ExporterMesh> mesh = new();

        for (int i = 0; i < parts.Count; i++)
        {
            StaticPart part = parts[i];
            mesh.Object.AddPart(name, part, i);
        }

        mesh.AddTransforms(instances);

        StaticMeshInstances.Add(mesh);
    }
}

public class ExporterMesh
{
    public List<ExporterPart> Parts { get; } = new();

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

/// <summary>
/// A wrapper for any exporter type that can be instanced.
/// Side effect is can also be used to place objects in the world at not-origin.
/// </summary>
public class ExporterInstance<T> where T : class
{
    public T Object { get; }
    public List<Transform> Transforms { get; set; } = new();

    public ExporterInstance()
    {
        Object = Activator.CreateInstance<T>();
    }

    public void AddTransforms(IEnumerable<SStaticMeshInstanceTransform> transforms)
    {
        Transforms.AddRange(transforms.Select(t => new Transform
        {
            Position = t.Position,
            Rotation = Vector4.QuaternionToEulerAngles(t.Rotation),
            Scale = t.Scale
        }));
    }
}

public struct Transform
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }
}
