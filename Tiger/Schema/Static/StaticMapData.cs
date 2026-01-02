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
        List<SA6488080> staticEntries = CollapseStaticTables();
        for (int i = 0; i < staticEntries.Count; i++)
        {
            Tag<S901A8080> entry = staticEntries[i].Entry;

            for (int j = 0; j < entry.TagData.StaticInfoTable.Count; j++)
            {
                S861B8080 infoEntry = entry.TagData.StaticInfoTable[j];
                SStaticMeshData_D1 staticEntry = entry.TagData.StaticMesh[infoEntry.StaticIndex];
                if (staticEntry.DetailLevel is 0 or 1 or 2 or 3 or 10)
                {
                    SAF1A8080 materialEntry = entry.TagData.MaterialTable[infoEntry.MaterialIndex];
                    // Material is (probably) used for depth pass, so ignore this mesh
                    if (materialEntry.Material.TagData.Unk08 != 1)
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
        List<InstanceTransform> instances = ParseTransforms();
        Dictionary<FileHash, List<MeshInfo>> statics = GetStatics();

        Parallel.ForEach(statics, mesh =>
        {
            List<StaticPart> parts = Load(mesh.Value, instances);
            scene.AddStatic(mesh.Key, parts);
            foreach (StaticPart part in parts)
            {
                if (part.Material == null)
                    continue;

                scene.Materials.Add(new ExportMaterial(part.Material));
            }
        });

        // I think this is working the way it should, but i feel like this isnt the right way..
        foreach ((FileHash mesh, List<MeshInfo> info) in statics.DistinctBy(x => x.Key))
        {
            foreach (MeshInfo instance in info.DistinctBy(x => x.TransformIndex))
            {
                scene.AddStaticInstancesToMesh(mesh, instances.Skip(instance.TransformIndex).Take(instance.InstanceCount).ToList());
            }
        }
    }

    // Static part loading will have to be done here since the statics aren't a seperate tag to build a class off of
    public List<StaticPart> Load(List<MeshInfo> meshInfo, List<InstanceTransform> instances)
    {
        List<StaticPart> parts = new();
        foreach (MeshInfo mesh in meshInfo.DistinctBy(x => x.Data))
        {
            StaticPart part = new(mesh.Data);
            part.VertexLayoutIndex = mesh.VertexLayoutIndex;
            part.Material = mesh.Material;
            part.GetAllData(mesh.Data);

            // Why in the world Bungie would store UV transforms in here is beyond me
            Vector4 texcoordTransform = instances[mesh.TransformIndex].UVTransform;
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
    public List<SA6488080> CollapseStaticTables()
    {
        List<SA6488080> collapsed = _tag.Statics1.ToList();
        collapsed.AddRange(_tag.Statics2.ToList());
        collapsed.AddRange(_tag.Statics3.ToList());
        collapsed.AddRange(_tag.Statics4.ToList());

        return collapsed;
    }

    // https://github.com/MontagueM/MontevenDynamicExtractor/blob/d1/d1map.cpp#L273
    public List<InstanceTransform> ParseTransforms()
    {
        List<Matrix4x4> a = ParseInstances();
        List<InstanceTransform> transforms = new();
        for (int i = 0; i < a.Count; i++)
        {
            InstanceTransform transform = new();
            Matrix4x4 b = a[i];

            System.Numerics.Matrix4x4 matrix = b.ToSys();

            matrix = System.Numerics.Matrix4x4.Transpose(matrix);
            System.Numerics.Vector3 translation = new();
            Quaternion rotation = new();
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

        TigerReader reader = new TigerFile(_tag.InstanceTransforms).GetReader();
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
        public Material Material;
        public int VertexLayoutIndex;
        public SStaticMeshData_D1 Data;
    }
}

public class StaticMapData : Tag<SStaticMapData>
{
    public StaticMapData(FileHash hash) : base(hash)
    {
    }

    //public void LoadArrangedIntoExporterScene()
    //{
    //    ExporterScene scene = Exporter.Get().CreateScene(Hash, ExportType.Map);
    //    Parallel.ForEach(_tag.InstanceCounts, c =>
    //    {
    //        var s = _tag.Statics[c.StaticIndex].Static;
    //        var parts = s.Load(ExportDetailLevel.MostDetailed);
    //        scene.AddStaticInstancesAndParts(s.Hash, parts, _tag.Instances.Skip(c.InstanceOffset).Take(c.InstanceCount));
    //    });
    //}

    public void LoadDecalsIntoExporterScene(ExporterScene scene)
    {
        foreach (SBA048080 decal in _tag.Decals)
        {
            Debug.Assert(decal.Transforms.Count == 1 && decal.Models.Count == 1);

            Matrix4x4 transform = decal.Transforms[0].Transform;
            SA5438080 model = decal.Models[0];

            //System.Numerics.Matrix4x4 matrix = transform.ToSys();

            //System.Numerics.Vector3 translation = new();
            //Quaternion rotation = new Quaternion();
            //System.Numerics.Vector3 scale = new();
            //System.Numerics.Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);

            //scene.AddMapModel(model.Model,
            //new Tiger.Schema.Vector4(translation.X, translation.Y, translation.Z, 1.0f),
            //new Tiger.Schema.Vector4(rotation.X, rotation.Y, rotation.Z, rotation.W),
            //new Tiger.Schema.Vector3(scale.X, scale.Y, scale.Z), true);

            Matrix4x4 matrix = transform;

            Vector3 scale = new();
            Vector4 trans = new();
            Vector4 quat = new();
            matrix.Decompose(out trans, out quat, out scale);

            scene.AddMapModel(model.Model, new Transform
            {
                Position = trans.ToVec3(),
                Rotation = Vector4.QuaternionToEulerAngles(quat),
                Quaternion = quat,
                Scale = scale,
            }, true);

            foreach (DynamicMeshPart part in model.Model.Load(ExportDetailLevel.MostDetailed, null, true))
            {
                if (part.Material == null) continue;
                scene.Materials.Add(new ExportMaterial(part.Material));
            }
        }
    }

    public void LoadIntoExporterScene(ExporterScene scene)
    {
        if (Strategy.IsD1())
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
                List<StaticPart> parts = s.Static.Load(ExportDetailLevel.MostDetailed);
                scene.AddStatic(s.Static.Hash, parts);
                s.Static.SaveMaterialsFromParts(scene, parts);
            });

            foreach (SStaticMeshInstanceMap c in _tag.InstanceCounts)
            {
                StaticMesh model = _tag.Statics[c.StaticIndex].Static;
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
    public DynamicArray<SBA048080> Decals; // Transparent/Decal meshes for ROI

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
    public DynamicArray<SUInt32> Unk50;

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
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "1E898080", 0x6C)]
public struct SBubbleParent
{
    public long FileSize;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Tag64 = true)] // Changed to Tag64 in Heresy
    public Tag<SBubbleDefinition> ChildMapReference;

    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public StringHash MapName;
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
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Tag64 = true)]
    public Tag<SMapContainer> MapContainer;
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
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true), NoLoad]
    public Entity.Entity Entity;

    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public MapTransform Transfrom;

    [SchemaField(0x68)]
    public uint Unk68;

    [SchemaField(0x80, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public ulong WorldID;

    [SchemaField(0x88, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public ResourcePointer DataResource;
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
    [SchemaField(0x8)]
    public StaticMapData StaticMap;
}

/// <summary>
/// Unk data resource.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "F21A8080", 0x90)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "DC718080", 0x90)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "A16D8080", 0x80)]
public struct SA16D8080
{
    public ulong FileSize;
    [SchemaField(0x30)]
    public DynamicArray<SUInt8> Bytecode;
    public DynamicArray<Vec4> Buffer1; // bytecode constants?
    [SchemaField(0x60)]
    public DynamicArray<Vec4> Buffer2;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "E2078080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "73968080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "B3938080", 0x30)]
public struct SB3938080
{
    //Bounds
    public Vector4 Unk00;
    public Vector4 Unk10;
}

// /// <summary>
// /// Boss entity data resource?
// /// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "19808080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "19808080", 0x54)]
public struct S19808080
{
    [SchemaField(0x24)]
    public StringHash EntityName;
}

// 501A8080 in D1, uses 16 2D textures instead of the 16-depth 3D texture D2 uses
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "501A8080", 0x98)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "86708080", 0xF0)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "C16B8080", 0x130)]
public struct SMapAtmosphere
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON, ArraySizeConst = 16), NoLoad]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public Texture[] D1Lookup;


    // 0 and 1 used in...
    // sky_lookup_generate_near/far, result used in 'Sky' and set to T11 and T13 (transparent scope)
    // full_hemisphere_sky_color_generate,
    // hemisphere_sky_color_generate,
    // water_sky_color_generate,
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x90, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x90, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Texture Lookup0;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Texture Lookup1;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Texture Lookup2;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Texture Lookup3;

    [SchemaField(0x4C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x98, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xD0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Texture Lookup4; // used in atmo_depth_angle_density_lookup_generate, result set to T15 (transparent scope)

    public FileHash UnkD4; // Lookup4 but in RGBA byte form, for some reason

    // Actually 16 floats, some type of ramp? Usually starts at around 0.2 and ends around 0.9
    public Vector4 UnkD8;
    public Vector4 UnkE8;
    public Vector4 UnkF8;
    public Vector4 Unk108;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "131B8080", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "616F8080", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "716A8080", 0x28)]
public struct S716A8080
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Tag<S746A8080> Unk10;
    public float Unk14; // always 3600? (one hour as seconds)
    public float Unk18; // some kind of multiplier maybe?
    public FileHash Unk1C; // Lens dirt or something
    public FileHash Unk20; // Lens dirt or something
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "061B8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "646F8080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "746A8080", 0x20)]
public struct S746A8080
{
    [SchemaField(0x0, TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Vector4 Unk00;

    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Tag<SC88A8080> Unk10;
    public Tag<SC88A8080> Unk14;
    public Tag<SC88A8080> Unk18;
    public Tag<SC88A8080> Unk1C;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "BF078080", 0x48)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "F38E8080", 0x48)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "C88A8080", 0x48)]
public struct SC88A8080
{
    [SchemaField(0x8)]
    public int Unk08; // always 1800? (1/2 hour as seconds)
    public float Unk0C; // always 108000?

    // Theres actually a relative pointer here but its always(?) 498B8080 so it doesnt matter

    [SchemaField(0x30)]
    public DynamicArrayUnloaded<Vec4> Unk30; // Global Channel 102? Some type of sun/light rotation
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "406A8080", 0x18)]
public struct SStaticAOResource
{
    [SchemaField(0x10)]
    public FileHash MapAO;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "196D8080", 0x78)]
public struct SStaticAmbientOcclusion
{
    [SchemaField(0x8)]
    public DynamicStruct<SAmbientOcclusionBuffer> AO_1;
    public DynamicStruct<SAmbientOcclusionBuffer> AO_2;
    public DynamicStruct<SAmbientOcclusionBuffer> AO_3;
}

[NonSchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x18)]
public struct SAmbientOcclusionBuffer
{
    public VertexBuffer Buffer;
    [SchemaField(0x8)]
    public DynamicArray<SStaticAmbientOcclusionMappings> Mappings;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "216D8080", 0x20)]
public struct SStaticAmbientOcclusionMappings
{
    public ulong Identifier;
    public uint Offset;
}
#endregion

#region Destiny 1 specific structs
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "751B8080", 0xD8)]
public struct SStaticMapData_D1
{
    public long FileSize;
    public int Unk08;
    [SchemaField(0x10)]
    public DynamicArray<SE71A8080> Unk10;
    public int InstanceCounts; // Total instances
    public FileHash InstanceTransforms; // Ref FFFFFFFF, Matrix4x4s
    public TigerHash Unk28;

    [SchemaField(0x38)]
    public DynamicArray<SA6488080> Statics1;  // Is this one just for depth purposes? I've only ever seen materials with just vertex shaders
    [SchemaField(0x50)]
    public DynamicArray<SA6488080> Statics2;
    [SchemaField(0x68)]
    public DynamicArray<SA6488080> Statics3;
    [SchemaField(0x80)]
    public DynamicArray<SA6488080> Statics4;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "E71A8080", 0x70)]
public struct SE71A8080 // ????
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
public struct SA6488080
{
    public Tag<S901A8080> Entry;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "901A8080", 0x38)]
public struct S901A8080
{
    public long FileSize;
    public DynamicArray<SAF1A8080> MaterialTable;
    public DynamicArray<SStaticMeshData_D1> StaticMesh;
    public DynamicArray<S861B8080> StaticInfoTable;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "AF1A8080", 0x8)]
public struct SAF1A8080
{
    public int VertexLayoutIndex;
    public Material Material;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "861B8080", 0x18)]
public struct S861B8080
{
    public short InstanceCount; // Instance count for this static
    [SchemaField(0x4)]
    public short MaterialIndex; // Index in MaterialTable
    [SchemaField(0x8)]
    public short StaticIndex; // Index in StaticMesh table
    public short TransformIndex; // Index in InstanceTransforms file
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "BA048080", 0x80)]
public struct SBA048080
{
    public long Size; // Just the size of the entry I think
    public DynamicArray<S75018080> Transforms;
    public DynamicArray<SC1018080> Unk18; // Similar to the location from Transforms but slightly different
    public DynamicArray<SA5438080> Models;
    [SchemaField(0x50)]
    public Vector4 Unk50; // Bounding box?
    public Vector4 Unk60;
    public Vector4 Unk70;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "75018080", 0x40)]
public struct S75018080
{
    // Matrix4x4
    public Matrix4x4 Transform;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C1018080", 0x10)]
public struct SC1018080
{
    public Vector4 Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "A5438080", 0x4)]
public struct SA5438080
{
    public EntityModel Model;
}

#endregion
