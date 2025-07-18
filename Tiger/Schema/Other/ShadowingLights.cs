using Tiger.Exporters;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class ExpensiveLight : Tag<SExpensiveLight>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.DeferredLights;
    public MapTransform Transfrom { get; set; }
    public ExpensiveLight(FileHash hash) : base(hash)
    {
    }

    public void LoadIntoExporter()
    {
        Tag<SA16D8080>? data = (Strategy.CurrentStrategy < TigerStrategy.DESTINY2_BEYONDLIGHT_3402 || _tag.BufferData2 is null) ? _tag.BufferData : _tag.BufferData2;
        if (data is null)
            return;

        List<Vec4> possibleColors = data.TagData.Buffer1.ToList();
        possibleColors.AddRange(data.TagData.Buffer2.ToList());

        Vector4 color = GetColor(data);
        Vector2 size = GetSize();
        Texture cookie = null;

        Material shading = FileResourcer.Get().GetFile<Material>(_tag.Shading);
        if (shading.Pixel.EnumerateTextures().Any())
        {
            cookie = shading.Pixel.EnumerateTextures().First().Texture;
        }

        Lights.LightData lightData = new()
        {
            Hash = data.Hash,
            Material = shading.Hash,
            LightType = Lights.LightType.Shadowing,
            Color = color,
            Size = new Vector3(_tag.HalfFOV * 2.0f, size.Y, 1f),
            Attenuation = 1, // Don't know
            Transform = new()
            {
                Position = Transfrom.Translation.ToVec3(),
                Quaternion = Transfrom.Rotation
            },
            Cookie = cookie != null ? cookie.Hash : null,
            Bytecode = data.TagData.Bytecode.Select(x => x.Value).ToArray(),
            BytecodeConstants = data.TagData.Buffer1.Select(x => x.Vec).ToArray()
        };

        Exporter.Get().GetGlobalScene().AddToGlobalScene(lightData);
    }

    public Vector4 GetColor(Tag<SA16D8080> data)
    {
        if (Strategy.IsD1() && data.TagData.Buffer2.Count != 0 && !data.TagData.Buffer2[2].Vec.IsZero())
        {
            return data.TagData.Buffer2[2].Vec; // Almost always color in D1?
        }

        if (data.TagData.Bytecode.Count != 0)
        {
            return data.TagData.Buffer1.Find(x => x.Vec != Vector4.Zero).Vec;
        }
        else if (data.TagData.Buffer2.Count(x => x.Vec.Magnitude != 0) == 2)
        {
            var sorted = data.TagData.Buffer2.OrderByDescending(v => v.Vec.Magnitude).ToList();
            return sorted[0].Vec;// * sorted[1].Vec;
        }
        else
        {
            List<Vec4> possibleColors = data.TagData.Buffer1.ToList();
            possibleColors.AddRange(data.TagData.Buffer2.ToList());
            return possibleColors.Count == 0 ? Vector4.Zero : possibleColors.MaxBy(v => v.Vec.Magnitude).Vec;
        }
    }

    public Vector2 GetSize()
    {
        Matrix4x4 matrix = _tag.LightToWorld;

        // 2x2x2 Cube
        Vector3[] cubePoints = new Vector3[] {
            new(-1f, -1f, -1f),
            new(-1f, -1f, 1f),
            new(-1f, 1f, -1f),
            new(-1f, 1f, 1f),
            new(1f, -1f, -1f),
            new(1f, -1f, 1f),
            new(1f, 1f, -1f),
            new(1f, 1f, 1f)
        };

        for (int i = 0; i < cubePoints.Length; i++)
        {
            Vector4 r0;

            //r0.xyzw = cb0[19].xyzw * v0.yyyy;
            r0 = matrix.Y_Axis * new Vector4(cubePoints[i].Y);

            //r0.xyzw = cb0[18].xyzw * v0.xxxx + r0.xyzw;
            r0 = matrix.X_Axis * new Vector4(cubePoints[i].X) + r0;

            //r0.xyzw = cb0[20].xyzw * v0.zzzz + r0.xyzw;
            r0 = matrix.Z_Axis * new Vector4(cubePoints[i].Z) + r0;

            //o0.xyzw = cb0[21].xyzw + r0.xyzw;
            Vector4 b = (matrix.W_Axis + r0);

            cubePoints[i] = (b / new Vector4(b.W)).ToVec3();
        }

        float baseWH = cubePoints[1].Y * 2f; // Width of the base
        float coneHeight = cubePoints[1].X - cubePoints[0].X;
        float radianFOV = MathF.Atan((baseWH / 2) / coneHeight) * 2;
        return new(radianFOV, coneHeight);
    }
}

/// <summary>
/// Map Shadowing Light (Casts shadows)
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C71B8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "33718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "5E6C8080", 0x20)]
public struct SExpensiveLightResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public ExpensiveLight ShadowingLight;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D91B8080", 0xB0)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "40718080", 0xC0)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "716C8080", 0x110)]
public struct SExpensiveLight
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
    public FileHash Shading; // For some reason using Material breaks tag reading....?
    //public Material Shading_Shadowing;
    //public Material Volumetric;
    //public Material Volumetric_Shadowing;
    //public Material Lightprobe;
    //public Material Lightprobe_Shadowing;

    [SchemaField(0x98, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xA0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xE8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<SA16D8080> BufferData;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xEC, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<SA16D8080> BufferData2;
}
