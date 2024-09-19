using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Tiger.Exporters;
using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;

namespace Tiger.Schema;

public class Map : Tag<SMapContainer>
{
    public Map(FileHash fileHash) : base(fileHash)
    {
    }
}

public class StaticMapData_D1 : Tag<SStaticMapData_D1>
{
    public StaticMapData_D1(FileHash hash) : base(hash)
    {
    }

    // Statics in D1 aren't there own tag, the data for them is just shoved into a table, so the 'Hash' that we will
    // assign to them will just be their Vertices0 hash.
    // Static tables will have multiple duplicate meshes since they are baked into the map.
    // Each static can have multiple parts that use the same Vertices0 data, so instead of filtering out duplicate hashes,
    // we will filter out duplicate entries that have the same hash and the same IndexOffset, that should (in theory) remove all dupes.
    public Dictionary<FileHash, List<MeshInfo>> GetStatics()
    {
        Dictionary<FileHash, List<MeshInfo>> statics = new();
        var staticEntries = CollapseStaticTables();
        for (int i = 0; i < staticEntries.Count; i++)
        {
            var entry = staticEntries[i].Entry;

            for (int j = 0; j < entry.TagData.StaticInfoTable.Count; j++)
            {
                var infoEntry = entry.TagData.StaticInfoTable[j];
                var staticEntry = entry.TagData.StaticMesh[infoEntry.StaticIndex];
                if (staticEntry.DetailLevel == 0 || staticEntry.DetailLevel == 1 || staticEntry.DetailLevel == 2 || staticEntry.DetailLevel == 3 || staticEntry.DetailLevel == 10)
                {
                    var materialEntry = entry.TagData.MaterialTable[infoEntry.MaterialIndex];
                    // Material is (probably) used for depth pass, so ignore this mesh
                    if (materialEntry.Material.Unk08 != 1)
                        continue;

                    if (!statics.ContainsKey(staticEntry.Vertices0.Hash))
                        statics[staticEntry.Vertices0.Hash] = new();

                    MeshInfo meshInfo = new()
                    {
                        InstanceCount = infoEntry.InstanceCount,
                        TransformIndex = infoEntry.TransformIndex,
                        MaterialIndex = infoEntry.MaterialIndex,
                        Material = materialEntry.Material,
                        VertexLayoutIndex = materialEntry.VertexLayoutIndex,
                        Data = staticEntry
                    };
                    statics[staticEntry.Vertices0.Hash].Add(meshInfo);
                    //Console.WriteLine($"{staticEntry.Vertices0.Hash}: {staticEntry.IndexCount} {staticEntry.IndexOffset}");
                }
            }
        }

        return statics;
    }

    public void LoadIntoExporterScene(ExporterScene scene)
    {
        var instances = ParseTransforms();
        var statics = GetStatics();

        Parallel.ForEach(statics, mesh =>
        {
            var parts = Load(mesh.Value, instances);
            scene.AddStatic(mesh.Key, parts);
            foreach (var part in parts)
            {
                if (part.Material == null)
                    continue;

                scene.Materials.Add(new ExportMaterial(part.Material));
            }
        });

        // I think this is working the way it should, but i feel like this isnt the right way..
        foreach (var (mesh, info) in statics.DistinctBy(x => x.Key))
        {
            foreach (var instance in info.DistinctBy(x => x.TransformIndex))
            {
                scene.AddStaticInstancesToMesh(mesh, instances.Skip(instance.TransformIndex).Take(instance.InstanceCount).ToList());
            }
        }
    }

    // Static part loading will have to be done here since the statics aren't a seperate tag to build a class off of
    public List<StaticPart> Load(List<MeshInfo> meshInfo, List<InstanceTransform> instances)
    {
        List<StaticPart> parts = new();
        foreach (var mesh in meshInfo.DistinctBy(x => x.Data))
        {
            StaticPart part = new StaticPart(mesh.Data);
            part.VertexLayoutIndex = mesh.VertexLayoutIndex;
            part.Material = mesh.Material;
            part.GetAllData(mesh.Data);

            // Why in the world Bungie would store UV transforms in here is beyond me
            var texcoordTransform = instances[mesh.TransformIndex].UVTransform;
            for (int i = 0; i < part.VertexTexcoords0.Count; i++)
            {
                part.VertexTexcoords0[i] = new Vector2(
                    part.VertexTexcoords0[i].X * texcoordTransform.X + texcoordTransform.Y,
                    part.VertexTexcoords0[i].Y * -texcoordTransform.X + 1 - texcoordTransform.Z
                );
            }

            parts.Add(part);
        }
        return parts;
    }

    // Statics1 seems to just be depth only meshes so I don't think it needs to be added, but ill do it just in case,
    // they should get filtered out anyways.
    public List<D1Class_A6488080> CollapseStaticTables()
    {
        List<D1Class_A6488080> collapsed = _tag.Statics1.ToList();
        collapsed.AddRange(_tag.Statics2.ToList());
        collapsed.AddRange(_tag.Statics3.ToList());
        collapsed.AddRange(_tag.Statics4.ToList());

        return collapsed;
    }

    // https://github.com/MontagueM/MontevenDynamicExtractor/blob/d1/d1map.cpp#L273
    public List<InstanceTransform> ParseTransforms()
    {
        var a = ParseInstances();
        List<InstanceTransform> transforms = new();
        for (int i = 0; i < a.Count; i++)
        {
            InstanceTransform transform = new();
            var b = a[i];

            System.Numerics.Matrix4x4 matrix = b.ToSys();

            matrix = System.Numerics.Matrix4x4.Transpose(matrix);
            System.Numerics.Vector3 translation = new();
            Quaternion rotation = new Quaternion();
            System.Numerics.Vector3 scale = new();
            System.Numerics.Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);

            transform.Translation = new(translation.X, translation.Y, translation.Z, 0);
            transform.Rotation = new(rotation.X, rotation.Y, rotation.Z, rotation.W);
            transform.Scale = new(scale.X, scale.Y, scale.Z);
            // X = scale
            // Y, Z = TranslateX/Y
            transform.UVTransform = a[i].W_Axis;

            transforms.Add(transform);
        }

        return transforms;
    }

    private List<Matrix4x4> ParseInstances()
    {
        byte[] instances = PackageResourcer.Get().GetFileData(_tag.InstanceTransforms);
        List<Matrix4x4> instanceTransforms = new();
        int blockSize = Marshal.SizeOf<Matrix4x4>();

        var reader = new TigerFile(_tag.InstanceTransforms).GetReader();
        for (int i = 0; i < _tag.InstanceCounts; i++)
        {
            Matrix4x4 instance = reader.ReadBytes(blockSize).ToType<Matrix4x4>();
            instanceTransforms.Add(instance);
        }

        return instanceTransforms;
    }

    public struct InstanceTransform
    {
        public Vector4 Translation;
        public Vector4 Rotation;
        public Vector4 Scale;
        public Vector4 UVTransform;
    }

    public struct MeshInfo
    {
        public short InstanceCount; // Instance count for this static
        public short TransformIndex; // Index in InstanceTransforms file
        public short MaterialIndex;
        public IMaterial Material;
        public int VertexLayoutIndex;
        public SStaticMeshData_D1 Data;
    }
}

public class StaticMapData : Tag<SStaticMapData>
{
    public StaticMapData(FileHash hash) : base(hash)
    {
    }

    public void LoadArrangedIntoExporterScene()
    {
        ExporterScene scene = Exporter.Get().CreateScene(Hash, ExportType.Map);
        Parallel.ForEach(_tag.InstanceCounts, c =>
        {
            var s = _tag.Statics[c.StaticIndex].Static;
            var parts = s.Load(ExportDetailLevel.MostDetailed);
            scene.AddStaticInstancesAndParts(s.Hash, parts, _tag.Instances.Skip(c.InstanceOffset).Take(c.InstanceCount));
        });
    }

    public void LoadDecalsIntoExporterScene(ExporterScene scene)
    {
        foreach (var decal in _tag.Decals)
        {
            Debug.Assert(decal.Transforms.Count == 1 && decal.Models.Count == 1);

            var transform = decal.Transforms[0].Transform;
            var model = decal.Models[0];

            System.Numerics.Matrix4x4 matrix = transform.ToSys();

            //matrix = Matrix4x4.Transpose(matrix);
            System.Numerics.Vector3 translation = new();
            Quaternion rotation = new Quaternion();
            System.Numerics.Vector3 scale = new();
            System.Numerics.Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);

            scene.AddMapModel(model.Model,
            new Tiger.Schema.Vector4(translation.X, translation.Y, translation.Z, 1.0f),
            new Tiger.Schema.Vector4(rotation.X, rotation.Y, rotation.Z, rotation.W),
            new Tiger.Schema.Vector3(scale.X, scale.Y, scale.Z), true);

            foreach (DynamicMeshPart part in model.Model.Load(ExportDetailLevel.MostDetailed, null, true))
            {
                if (part.Material == null) continue;
                scene.Materials.Add(new ExportMaterial(part.Material));
            }
        }
    }

    public void LoadIntoExporterScene(ExporterScene scene)
    {
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            if (_tag.D1StaticMapData is not null)
                _tag.D1StaticMapData.LoadIntoExporterScene(scene);
        }
        else
        {
            List<SStaticMeshHash> extractedStatics = _tag.Statics.DistinctBy(x => x.Static.Hash).ToList();

            // todo this loads statics twice
            Parallel.ForEach(extractedStatics, s =>
            {
                var parts = s.Static.Load(ExportDetailLevel.MostDetailed);
                scene.AddStatic(s.Static.Hash, parts);
                s.Static.SaveMaterialsFromParts(scene, parts);
            });

            foreach (var c in _tag.InstanceCounts)
            {
                var model = _tag.Statics[c.StaticIndex].Static;
                scene.AddStaticInstancesToMesh(model.Hash, _tag.Instances.Skip(c.InstanceOffset).Take(c.InstanceCount).ToList());
            }
        }

    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "B4088080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "6D968080", 0xA0)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "AD938080", 0xA0)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "AD938080", 0xC0)]
public struct SStaticMapData
{
    public long FileSize;

    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public DynamicArray<D1Class_BA048080> Decals; // Transparent/Decal meshes for ROI

    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    public Tag<SOcclusionBounds> ModelOcclusionBounds;

    [SchemaField(0x30, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public StaticMapData_D1 D1StaticMapData; // Contains the actual static map data in ROI

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicArray<SStaticMeshInstanceTransform> Instances;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicArray<SUnknownUInt> Unk50;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x58, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<SStaticMeshHash> Statics;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicArray<SStaticMeshInstanceMap> InstanceCounts;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x98, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk98;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x80, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xA0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Vector4 UnkA0; // likely a bound corner
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Vector4 UnkB0; // likely the other bound corner
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "0B008080", 0x04)]
public struct SUnknownUInt
{
    public uint Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "83058080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "71968080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B1938080", 0x18)]
public struct SOcclusionBounds
{
    public long FileSize;
    public DynamicArrayUnloaded<SMeshInstanceOcclusionBounds> InstanceBounds;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "E2078080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "73968080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B3938080", 0x30)]
public struct SMeshInstanceOcclusionBounds
{
    public Vector4 Corner1;
    public Vector4 Corner2;
    public TigerHash Unk20;
    public TigerHash Unk24;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "A3718080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "406D8080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "406D8080", 0x40)]
public struct SStaticMeshInstanceTransform
{
    public Vector4 Rotation;
    public Vector3 Position;
    public Vector3 Scale;  // Only X is used as a global scale
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "7D968080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "BD938080", 0x4)]
public struct SStaticMeshHash
{
    public StaticMesh Static;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "90718080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "286D8080", 0x8)]
public struct SStaticMeshInstanceMap
{
    public short InstanceCount;
    public short InstanceOffset;
    public short StaticIndex;
    public short Unk06;
}

#region Parent/other structures for maps


/// <summary>
/// The very top reference for all map-related things.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "AE7D8080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "1E898080", 0x60)]
public struct SBubbleParent
{
    public long FileSize;
    public Tag<SBubbleDefinition> ChildMapReference;
    [SchemaField(0x10)] // todo i dont get this
    public StringHash MapName;
    public int Unk1C;
    [SchemaField(0x40)]
    public DynamicArray<D2Class_C9968080> Unk40;
    [Tag64, SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag Unk50;  // some kind of parent thing, very strange weird idk
}

/// <summary>
/// Basically same table as in the child tag, but in a weird format. Never understood what its for.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "44968080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "C9968080", 0x10)]
public struct D2Class_C9968080
{
    [Tag64]
    public Tag Unk00;
}

/// <summary>
/// The one below the top reference, actually contains useful information.
/// First of MapResources is what I call "ambient entities", second is always the static map.
/// </summary>

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "E0918080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "E0918080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "01878080", 0x60)]
public struct SBubbleDefinition
{
    public long FileSize;
    public DynamicArray<SMapContainerEntry> MapResources;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "67078080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "C1848080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "03878080", 0x10)]
public struct SMapContainerEntry
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public Tag<SMapContainer> MapContainerROI;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601), Tag64]
    public Tag<SMapContainer> MapContainer;

    public Tag<SMapContainer> GetMapContainer()
    {
        if (MapContainerROI != null)
            return MapContainerROI;
        else
            return MapContainer;
    }
}

/// <summary>
/// A map resource, contains data used to make a map.
/// This is quite similar to EntityResource, but with more children.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "548A8080", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "548A8080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "07878080", 0x38)]
public struct SMapContainer
{
    public long FileSize;
    public long Unk08;
    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicArray<SMapDataTableEntry> MapDataTables;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "09418080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "B08B8080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "09878080", 4)]
public struct SMapDataTableEntry
{
    public Tag<SMapDataTable> MapDataTable;
}

/// <summary>
/// A map data table, containing data entries.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "A2098080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "D6998080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "83988080", 0x18)]
public struct SMapDataTable
{
    public long FileSize;
    public DynamicArray<SMapDataEntry> DataEntries;
}


/// <summary>
/// A data entry. Can be static maps, entities, etc. with a defined world transform.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "06048080", 0x90)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "D8998080", 0x90)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "85988080", 0x90)]
public struct SMapDataEntry
{
    // for now we'll process both entities as if they were the same
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true), NoLoad]
    public Entity.Entity EntitySK;
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Vector4 Rotation;
    public Vector4 Translation;
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402), Tag64, NoLoad]
    public Entity.Entity EntityWQ;
    [SchemaField(0x68)]
    public uint Unk68;
    [SchemaField(0x80, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public ulong WorldID;
    [SchemaField(0x88, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public ResourcePointer DataResource;

    public FileHash GetEntityHash()
    {
        if (EntitySK != null)
            return EntitySK.Hash;
        else
            return EntityWQ.Hash;
    }
}

/// <summary>
/// Data resource containing a static map.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "EA1A8080", 0x14)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "B3718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "C96C8080", 0x18)]
public struct SMapDataResource
{
    [SchemaField(0x8, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public TigerHash Unk08;
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public Tag<SStaticMapParent> StaticMapParent;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C61A8080", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "F46E8080", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "0D6A8080", 0x30)]
public struct SStaticMapParent
{
    // no filesize
    [SchemaField(0x8)]
    public StaticMapData StaticMap;  // could make it StaticMapData but dont want it to load it, could have a NoLoad option
    [SchemaField(0x24, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x2C, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public TigerHash Unk2C;
}

/// <summary>
/// Light Lens Flares
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "BF6C8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B5678080", 0x1C)]
public struct D2Class_B5678080
{
    //[SchemaField(0x10)]
    //public Tag Unk10; // D2Class_786A8080
}

/// <summary>
/// Unk data resource.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "686F8080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "786A8080", 0x38)]
public struct D2Class_786A8080
{
    public ulong FileSize;
    [SchemaField(0x18)]
    public Tag<D2Class_A16D8080> Unk10;
    [SchemaField(0x20)]
    public DynamicArray<D2Class_7D6A8080> Unk20;
    public TigerHash Unk30;
}

/// <summary>
/// Unk data resource.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "6D6F8080", 0xC)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "7D6A8080", 0xC)]
public struct D2Class_7D6A8080
{
    public IMaterial Unk00;
    public Tag<D2Class_A16D8080> Unk04;
    public int Unk08;
}

/// <summary>
/// Map Light
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "EA1B8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "5A6F8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "636A8080", 0x18)]
public struct SMapLightResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public Lights Lights;
}

/// <summary>
/// Map Shadowing Light (Casts shadows)
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C71B8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "33718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "5E6C8080", 0x20)]
public struct SMapShadowingLightResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public ShadowingLights ShadowingLight;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D91B8080", 0xB0)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "40718080", 0xC0)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "716C8080", 0x110)]
public struct SMapShadowingLight
{
    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x60, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Matrix4x4 LightToWorld;
    public Vector4 Distance; // Unsure but only W is used?

    [SchemaField(0x80, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x80, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xC0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public float FarPlane;
    public float HalfFOV; // * 2, radians->degrees

    // Not really a point in even loading these
    [SchemaField(0x90, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xD0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public FileHash Shading; // For some reason using IMaterial breaks tag reading....?
    //public IMaterial Shading_Shadowing;
    //public IMaterial Volumetric;
    //public IMaterial Volumetric_Shadowing;
    //public IMaterial Lightprobe;
    //public IMaterial Lightprobe_Shadowing;

    [SchemaField(0x98, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xA0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xE8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<D2Class_A16D8080> BufferData;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xEC, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<D2Class_A16D8080> BufferData2;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "5B1A8080", 0x60)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "3A718080", 0x60)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "656C8080", 0x60)]
public struct D2Class_656C8080
{
    [SchemaField(0x10)]
    public Vector4 Unk10;
    public Vector4 Unk20;
    public DynamicArrayUnloaded<D2Class_706C8080> LightData;
    public DynamicArrayUnloaded<D2Class_4F9F8080> Transforms;
    [SchemaField(0x54, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x58, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Tag<SOcclusionBounds> Bounds;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "2F1C8080", 0x90)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "3E718080", 0xA0)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "706C8080", 0xF0)]
public struct D2Class_706C8080
{
    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x60, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Matrix4x4 LightToWorld;
    // Techniques between

    [SchemaField(0x80, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xC4, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public FileHash Shading; // For some reason using IMaterial breaks tag reading....?

    [SchemaField(0x84, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x88, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xCC, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0xD0, TigerStrategy.DESTINY2_LATEST)]
    public Tag<D2Class_A16D8080> BufferData;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xD0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0xD4, TigerStrategy.DESTINY2_LATEST)]
    public Tag<D2Class_A16D8080> BufferData2;
}

/// <summary>
/// Unk data resource.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "F21A8080", 0x90)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "DC718080", 0x90)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "A16D8080", 0x80)]
public struct D2Class_A16D8080
{
    public ulong FileSize;
    [SchemaField(0x30)]
    public DynamicArray<D2Class_09008080> Bytecode;
    public DynamicArray<Vec4> Buffer1; // bytecode constants?
    [SchemaField(0x60)]
    public DynamicArray<Vec4> Buffer2;
}

/// </summary>
/// Background entities/skybox resource
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "DA1B8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "916F8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "A36A8080", 0x18)]
public struct SMapSkyEntResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public Tag<SMapSkyEntities> SkyEntities;  // A76A8080
}

/// <summary>
/// Background entities/skybox
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "1F1C8080", 0x68)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "956F8080", 0x68)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "A76A8080", 0x60)]
public struct SMapSkyEntities
{
    public long FileSize;
    public DynamicArray<D2Class_A96A8080> Entries;
    //public DynamicArray<D2Class_B3938080> Unk18;
    //public DynamicArray<D2Class_07008080> Unk28;
    [SchemaField(0x40)]
    public Vector4 Unk40;
    public Vector4 Unk50;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "1E1C8080", 0x80)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "976F8080", 0x80)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "A96A8080", 0x90)]
public struct D2Class_A96A8080
{
    public Matrix4x4 Transform;
    public AABB Bounds;
    public Tag<D2Class_AE6A8080> Model;
    [SchemaField(0x70, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public int Unk70; // if 5, skip the model?? 
}

/// </summary>
/// Map Decals Resource
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "701A8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "626E8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "55698080", 0x18)]
public struct SMapDecalsResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public Tag<SMapDecals> MapDecals;
}

/// <summary>
/// Map Decals
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "401B8080", 0x68)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "686E8080", 0x78)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "5B698080", 0x78)]
public struct SMapDecals
{
    public ulong FileSize;
    public DynamicArray<D2Class_63698080> DecalResources;
    [SchemaField(0x18)]
    public DynamicArray<D2Class_64698080> Locations;
    [SchemaField(0x28)]
    public Tag Unk28;
    public Tag Unk2C;
    [SchemaField(0x38)]
    public Tag<SOcclusionBounds> DecalProjectionBounds;
    [SchemaField(0x40)]
    public Vector4 Unk40; //some type of bounds
    public Vector4 Unk50;
    public TigerHash Unk60;
}

/// <summary>
/// Decal resources
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "831A8080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "6C6E8080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "63698080", 0x8)]
public struct D2Class_63698080
{
    public IMaterial Material;
    public short StartIndex;
    public short Count; //Number of entries to read
}

/// <summary>
/// Decal Location
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "531A8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "6D6E8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "64698080", 0x10)]
public struct D2Class_64698080
{
    public Vector4 Location;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "3A1B8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "9B6F8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "AE6A8080", 0x10)]
public struct D2Class_AE6A8080
{
    public long FileSize;
    public EntityModel Model;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "E2078080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "73968080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B3938080", 0x30)]
public struct D2Class_B3938080
{
    //Bounds
    public Vector4 Unk00;
    public Vector4 Unk10;
}


// /// <summary>
// /// Boss entity data resource?
// /// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "19808080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "19808080", 0x54)]
public struct D2Class_19808080
{
    // todo rest of this
    // [DestinyField(FieldType.ResourcePointer)]
    // public dynamic? Unk00;
    [SchemaField(0x24)]
    public StringHash EntityName;
}

/// <summary>
/// Usually a flat plane for screen-space reflected water
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "7E1A8080", 0x60)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "E06D8080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "D4688080", 0x70)]
public struct SMapWaterDecal
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public EntityModel Model;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "C16B8080", 0x130)]
public struct SMapAtmosphere
{
    [SchemaField(0x90), Tag64]
    public Texture Texture0; // 
    [Tag64]
    public Texture TextureUnk0; //  
    [Tag64]
    public Texture Texture1; // 
    [Tag64]
    public Texture TextureUnk1; // 
    public Texture Texture2; // 
}

// /// <summary>
// /// Unk data resource, maybe lights for entities?
// /// </summary>
// [SchemaStruct("636A8080", 0x18)]
// public struct D2Class_636A8080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // D2Class_656C8080, might be related to lights for entities?
// }
//
//
// /// <summary>
// /// Audio data resource.
// /// </summary>
// [SchemaStruct("6F668080", 0x30)]
// public struct D2Class_6F668080
// {
//     [SchemaField(0x10), DestinyField(FieldType.TagHash64)]
//     public Tag AudioContainer;  // 38978080 audio container
// }
//
// /// <summary>
// /// Spatial audio data resource, contains a list of positions to play an audio container.
// /// </summary>
// [SchemaStruct("6D668080", 0x48)]
// public struct D2Class_6D668080
// {
//     [SchemaField(0x10), DestinyField(FieldType.TagHash64)]
//     public Tag AudioContainer;  // 38978080 audio container
//     [SchemaField(0x30), DestinyField(FieldType.TablePointer)]
//     public List<D2Class_94008080> AudioPositions;
//     public float Unk40;
// }
//
// [SchemaStruct("94008080", 0x10)]
// public struct D2Class_94008080
// {
//     public Vector4 Translation;
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("B58C8080", 0x18)]
// public struct D2Class_B58C8080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // B78C8080
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("55698080", 0x18)]
// public struct D2Class_55698080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // 5B698080, lights/volumes/smth maybe cubemaps idk
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("7B918080", 0x18)]
// public struct D2Class_7B918080
// {
//     [DestinyField(FieldType.RelativePointer)]
//     public dynamic? Unk00;
// }
//
// /// <summary>
// /// Havok volume data resource.
// /// </summary>
// [SchemaStruct("21918080", 0x20)]
// public struct D2Class_21918080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag HavokVolume;  // type 27 subtype 0
//     public TigerHash Unk14;
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("C0858080", 0x18)]
// public struct D2Class_C0858080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // C2858080
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("C26A8080", 0x18)]
// public struct D2Class_C26A8080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // C46A8080
// }
//
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("222B8080", 0x18)]
// public struct D2Class_222B8080
// {
//     [SchemaField(0x10)]
//     public TigerHash Unk10;
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("04868080", 0x18)]
// public struct D2Class_04868080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // 24878080, smth related to havok volumes
// }


#endregion

#region Destiny 1 specific structs
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "751B8080", 0xD8)]
public struct SStaticMapData_D1
{
    public long FileSize;
    public int Unk08;
    [SchemaField(0x10)]
    public DynamicArray<D1Class_E71A8080> Unk10;
    public int InstanceCounts; // Total instances
    public FileHash InstanceTransforms; // Ref FFFFFFFF, Matrix4x4s
    public TigerHash Unk28;

    [SchemaField(0x38)]
    public DynamicArray<D1Class_A6488080> Statics1;  // Is this one just for depth purposes? I've only ever seen materials with just vertex shaders
    [SchemaField(0x50)]
    public DynamicArray<D1Class_A6488080> Statics2;
    [SchemaField(0x68)]
    public DynamicArray<D1Class_A6488080> Statics3;
    [SchemaField(0x80)]
    public DynamicArray<D1Class_A6488080> Statics4;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "E71A8080", 0x70)]
public struct D1Class_E71A8080 // ????
{
    public Vector4 Unk00;
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    public Vector4 Unk40;
    public Vector4 Unk50;
    public Vector4 Unk60;
    public Vector4 Unk70;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "A6488080", 0x4)]
public struct D1Class_A6488080
{
    public Tag<D1Class_901A8080> Entry;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "901A8080", 0x38)]
public struct D1Class_901A8080
{
    public long FileSize;
    public DynamicArray<D1Class_AF1A8080> MaterialTable;
    public DynamicArray<SStaticMeshData_D1> StaticMesh;
    public DynamicArray<D1Class_861B8080> StaticInfoTable;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "AF1A8080", 0x8)]
public struct D1Class_AF1A8080
{
    public int VertexLayoutIndex;
    public IMaterial Material;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "861B8080", 0x18)]
public struct D1Class_861B8080
{
    public short InstanceCount; // Instance count for this static
    [SchemaField(0x4)]
    public short MaterialIndex; // Index in MaterialTable
    [SchemaField(0x8)]
    public short StaticIndex; // Index in StaticMesh table
    public short TransformIndex; // Index in InstanceTransforms file
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "BA048080", 0x80)]
public struct D1Class_BA048080
{
    public long Size; // Just the size of the entry I think
    public DynamicArray<D1Class_75018080> Transforms;
    public DynamicArray<D1Class_C1018080> Unk18; // Similar to the location from Transforms but slightly different
    public DynamicArray<D1Class_A5438080> Models;
    [SchemaField(0x50)]
    public Vector4 Unk50; // Bounding box?
    public Vector4 Unk60;
    public Vector4 Unk70;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "75018080", 0x40)]
public struct D1Class_75018080
{
    // Matrix4x4
    public Matrix4x4 Transform;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C1018080", 0x10)]
public struct D1Class_C1018080
{
    public Vector4 Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "A5438080", 0x4)]
public struct D1Class_A5438080
{
    public EntityModel Model;
}

#endregion
