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
        List<S808048A6> staticEntries = CollapseStaticTables();
        for (int i = 0; i < staticEntries.Count; i++)
        {
            Tag<S80801A90> entry = staticEntries[i].Entry;

            for (int j = 0; j < entry.TagData.StaticInfoTable.Count; j++)
            {
                S80801B86 infoEntry = entry.TagData.StaticInfoTable[j];
                SStaticMeshData_D1 staticEntry = entry.TagData.StaticMesh[infoEntry.StaticIndex];
                if (staticEntry.DetailLevel is 0 or 1 or 2 or 3 or 10)
                {
                    S80801AAF materialEntry = entry.TagData.MaterialTable[infoEntry.MaterialIndex];
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
    public List<S808048A6> CollapseStaticTables()
    {
        List<S808048A6> collapsed = _tag.Statics1.ToList();
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
        foreach (S808004BA decal in _tag.Decals)
        {
            Debug.Assert(decal.Transforms.Count == 1 && decal.Models.Count == 1);

            Matrix4x4 transform = decal.Transforms[0].Transform;
            S808043A5 model = decal.Models[0];

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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808008B4, 0x38)] //B4088080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080966D, 0xA0)] //6D968080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808093AD, 0xA0)] //AD938080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x808093AD, 0xC0)] //AD938080
public struct SStaticMapData
{
    public long FileSize;

    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public DynamicArray<S808004BA> Decals; // Transparent/Decal meshes for ROI

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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800583, 0x18)] //83058080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809671, 0x18)] //71968080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x808093B1, 0x18)] //B1938080
public struct SOcclusionBounds
{
    public long FileSize;
    public DynamicArrayUnloaded<SMeshInstanceOcclusionBounds> InstanceBounds;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808007E2, 0x30)] //E2078080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809673, 0x30)] //73968080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x808093B3, 0x30)] //B3938080
public struct SMeshInstanceOcclusionBounds
{
    public Vector4 Corner1;
    public Vector4 Corner2;
    public TigerHash Unk20;
    public TigerHash Unk24;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808071A3, 0x30)] //A3718080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D40, 0x30)] //406D8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80806D40, 0x40)] //406D8080
public struct SStaticMeshInstanceTransform
{
    public Vector4 Rotation;
    public Vector3 Position;
    public Vector3 Scale;  // Only X is used as a global scale
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080967D, 0x4)] //7D968080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808093BD, 0x4)] //BD938080
public struct SStaticMeshHash
{
    public StaticMesh Static;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80807190, 0x8)] //90718080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D28, 0x8)] //286D8080
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
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80807DAE, 0x50)] //AE7D8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080891E, 0x60)] //1E898080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x8080891E, 0x6C)] //1E898080
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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808091E0, 0x18)] //E0918080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808091E0, 0x18)] //E0918080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808701, 0x60)] //01878080
public struct SBubbleDefinition
{
    public long FileSize;
    public DynamicArray<SMapContainerEntry> MapResources;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800767, 0x4)] //67078080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808084C1, 0x10)] //C1848080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808703, 0x10)] //03878080
public struct SMapContainerEntry
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Tag64 = true)]
    public Tag<SMapContainer> MapContainer;
}

/// <summary>
/// A map resource, contains data used to make a map.
/// This is quite similar to EntityComponent, but with more children.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80808A54, 0x28)] //548A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808A54, 0x38)] //548A8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808707, 0x38)] //07878080
public struct SMapContainer
{
    public long FileSize;
    public long Unk08;
    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicArray<SMapDataTableEntry> MapDataTables;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80804109, 4)] //09418080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808BB0, 4)] //B08B8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808709, 4)] //09878080
public struct SMapDataTableEntry
{
    public Tag<SMapDataTable> MapDataTable;
}

/// <summary>
/// A map data table, containing data entries.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808009A2, 0x18)] //A2098080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808099D6, 0x18)] //D6998080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809883, 0x18)] //83988080
public struct SMapDataTable
{
    public long FileSize;
    public DynamicArray<SMapDataEntry> DataEntries;
}


/// <summary>
/// A data entry. Can be static maps, entities, etc. with a defined world transform.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800406, 0x90)] //06048080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808099D8, 0x90)] //D8998080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80809885, 0x90)] //85988080
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
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801AEA, 0x14)] //EA1A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808071B3, 0x18)] //B3718080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806CC9, 0x18)] //C96C8080
public struct SStaticMapDataResource
{
    [SchemaField(0x8, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public TigerHash Unk08;

    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public Tag<SStaticMapParent> StaticMapParent;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801AC6, 0x28)] //C61A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806EF4, 0x28)] //F46E8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806A0D, 0x30)] //0D6A8080
public struct SStaticMapParent
{
    [SchemaField(0x8)]
    public StaticMapData StaticMap;
}

/// <summary>
/// Unk data resource.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801AF2, 0x90)] //F21A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808071DC, 0x90)] //DC718080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806DA1, 0x80)] //A16D8080
public struct S80806DA1
{
    public ulong FileSize;
    [SchemaField(0x30)]
    public DynamicArray<SUInt8> Bytecode;
    public DynamicArray<Vec4> Buffer1; // bytecode constants?
    [SchemaField(0x60)]
    public DynamicArray<Vec4> Buffer2;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808007E2, 0x30)] //E2078080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809673, 0x30)] //73968080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808093B3, 0x30)] //B3938080
public struct S808093B3
{
    //Bounds
    public Vector4 Unk00;
    public Vector4 Unk10;
}

// /// <summary>
// /// Boss entity data resource?
// /// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808019, 0x50)] //19808080
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, 0x80808019, 0x54)] //19808080
public struct S80808019
{
    [SchemaField(0x24)]
    public StringHash EntityName;
}

// 501A8080 in D1, uses 16 2D textures instead of the 16-depth 3D texture D2 uses
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A50, 0x98)] //501A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80807086, 0xF0)] //86708080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806BC1, 0x130)] //C16B8080
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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801B13, 0x28)] //131B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806F61, 0x28)] //616F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806A71, 0x28)] //716A8080
public struct S80806A71
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Tag<S80806A74> Unk10;
    public float Unk14; // always 3600? (one hour as seconds)
    public float Unk18; // some kind of multiplier maybe?
    public FileHash Unk1C; // Lens dirt or something
    public FileHash Unk20; // Lens dirt or something
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801B06, 0x18)] //061B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806F64, 0x20)] //646F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806A74, 0x20)] //746A8080
public struct S80806A74
{
    [SchemaField(0x0, TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Vector4 Unk00;

    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Tag<S80808AC8> Unk10;
    public Tag<S80808AC8> Unk14;
    public Tag<S80808AC8> Unk18;
    public Tag<S80808AC8> Unk1C;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808007BF, 0x48)] //BF078080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808EF3, 0x48)] //F38E8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808AC8, 0x48)] //C88A8080
public struct S80808AC8
{
    [SchemaField(0x8)]
    public int Unk08; // always 1800? (1/2 hour as seconds)
    public float Unk0C; // always 108000?

    // Theres actually a relative pointer here but its always(?) 498B8080 so it doesnt matter

    [SchemaField(0x30)]
    public DynamicArrayUnloaded<Vec4> Unk30; // Global Channel 102? Some type of sun/light rotation
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806A40, 0x18)] //406A8080
public struct SStaticAOResource
{
    [SchemaField(0x10)]
    public FileHash MapAO;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D19, 0x78)] //196D8080
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

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806D21, 0x20)] //216D8080
public struct SStaticAmbientOcclusionMappings
{
    public ulong Identifier;
    public uint Offset;
}
#endregion

#region Destiny 1 specific structs
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801B75, 0xD8)] //751B8080
public struct SStaticMapData_D1
{
    public long FileSize;
    public int Unk08;
    [SchemaField(0x10)]
    public DynamicArray<S80801AE7> Unk10;
    public int InstanceCounts; // Total instances
    public FileHash InstanceTransforms; // Ref FFFFFFFF, Matrix4x4s
    public TigerHash Unk28;

    [SchemaField(0x38)]
    public DynamicArray<S808048A6> Statics1;  // Is this one just for depth purposes? I've only ever seen materials with just vertex shaders
    [SchemaField(0x50)]
    public DynamicArray<S808048A6> Statics2;
    [SchemaField(0x68)]
    public DynamicArray<S808048A6> Statics3;
    [SchemaField(0x80)]
    public DynamicArray<S808048A6> Statics4;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801AE7, 0x70)] //E71A8080
public struct S80801AE7 // ????
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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808048A6, 0x4)] //A6488080
public struct S808048A6
{
    public Tag<S80801A90> Entry;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A90, 0x38)] //901A8080
public struct S80801A90
{
    public long FileSize;
    public DynamicArray<S80801AAF> MaterialTable;
    public DynamicArray<SStaticMeshData_D1> StaticMesh;
    public DynamicArray<S80801B86> StaticInfoTable;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801AAF, 0x8)] //AF1A8080
public struct S80801AAF
{
    public int VertexLayoutIndex;
    public Material Material;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801B86, 0x18)] //861B8080
public struct S80801B86
{
    public short InstanceCount; // Instance count for this static
    [SchemaField(0x4)]
    public short MaterialIndex; // Index in MaterialTable
    [SchemaField(0x8)]
    public short StaticIndex; // Index in StaticMesh table
    public short TransformIndex; // Index in InstanceTransforms file
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808004BA, 0x80)] //BA048080
public struct S808004BA
{
    public long Size; // Just the size of the entry I think
    public DynamicArray<S80800175> Transforms;
    public DynamicArray<S808001C1> Unk18; // Similar to the location from Transforms but slightly different
    public DynamicArray<S808043A5> Models;
    [SchemaField(0x50)]
    public Vector4 Unk50; // Bounding box?
    public Vector4 Unk60;
    public Vector4 Unk70;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800175, 0x40)] //75018080
public struct S80800175
{
    // Matrix4x4
    public Matrix4x4 Transform;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808001C1, 0x10)] //C1018080
public struct S808001C1
{
    public Vector4 Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808043A5, 0x4)] //A5438080
public struct S808043A5
{
    public EntityModel Model;
}

#endregion
