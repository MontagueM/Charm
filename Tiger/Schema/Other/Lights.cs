using Tiger.Exporters;
using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class Lights : Tag<SMapLights>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.ChunkedLights;
    public Lights(FileHash hash) : base(hash)
    {
    }

    public void LoadIntoExporter()
    {
        using TigerReader reader = GetReader();
        for (int i = 0; i < _tag.LightData.Count; i++)
        {
            SMapLightCollection data = _tag.LightData.ElementAt(reader, i);

            Tag<S80806DA1>? bufferData = (Strategy.CurrentStrategy < TigerStrategy.DESTINY2_BEYONDLIGHT_3402 || data.BufferData2 is null) ? data.BufferData : data.BufferData2;
            if (bufferData is null)
                continue;

            Vector4 color = GetColor(bufferData);
            LightType lightType = LightType.Point;

            if (MathF.Abs(data.LightToWorld.X_Axis.X) == 0.0)
                lightType = LightType.Spot;
            else if (MathF.Abs(data.LightToWorld.X_Axis.X) != MathF.Abs(data.LightToWorld.Y_Axis.Y) || MathF.Abs(data.LightToWorld.Y_Axis.Y) != MathF.Abs(data.LightToWorld.Z_Axis.Z))
                lightType = LightType.Line;

            Texture cookie = null;
            if (lightType == LightType.Spot)
            {
                if (data.Shading is null)
                    continue;

                if (data.Shading.Pixel.EnumerateTextures().Any())
                {
                    cookie = data.Shading.Pixel.EnumerateTextures().First().Texture;
                }
            }

            Vector3 size = GetSize(data.LightToWorld, lightType, $"{lightType}_{data.BufferData.Hash}_{i}");
            SMeshInstanceOcclusionBounds bounds = _tag.Bounds.TagData.InstanceBounds.ElementAt(_tag.Bounds.GetReader(), i);
            S80809F4F transforms = _tag.Transforms.ElementAt(reader, i);
            LightData lightData = new()
            {
                Hash = bufferData.Hash,
                Material = data.Shading.Hash,
                LightType = lightType,
                Color = color,
                Size = size,
                Attenuation = 1, // Don't know
                Transform = new()
                {
                    Position = transforms.Translation.ToVec3(),
                    Quaternion = transforms.Rotation
                },
                Cookie = cookie != null ? cookie.Hash : null,
                Bytecode = bufferData.TagData.Bytecode.Select(x => x.Value).ToArray(),
                BytecodeConstants = bufferData.TagData.Buffer1.Select(x => x.Vec).ToArray()
            };

            Exporter.Get().GetGlobalScene().AddToGlobalScene(lightData);
        }
    }

    public Vector4 GetColor(Tag<S80806DA1> data)
    {
        //Console.WriteLine($"{data.TagData.Buffer2[0].Vec} : {data.TagData.Buffer2[1].Vec} : {data.TagData.Buffer2.Count(x => x.Vec.Magnitude != 0)}");
        if ((Strategy.IsD1() || Strategy.IsPreBL()) && data.TagData.Buffer2.Count != 0 && !data.TagData.Buffer2[2].Vec.IsZero())
        {
            return data.TagData.Buffer2[2].Vec; // Always color in D1?
        }

        if (data.TagData.Bytecode.Count != 0)
        {
            return data.TagData.Buffer1.Find(x => x.Vec != Vector4.Zero).Vec;
        }
        else if (data.TagData.Buffer2.Count(x => x.Vec.Magnitude != 0) == 2)
        {
            var sorted = data.TagData.Buffer2.OrderByDescending(v => v.Vec.Magnitude).ToList();
            return sorted[0].Vec; //* sorted[1].Vec;
        }
        else
        {
            List<Vec4> possibleColors = data.TagData.Buffer1.ToList();
            possibleColors.AddRange(data.TagData.Buffer2.ToList());
            return possibleColors.Count == 0 ? Vector4.Zero : possibleColors.MaxBy(v => v.Vec.Magnitude).Vec;
        }
    }

    public Vector3 GetSize(Matrix4x4 matrix, LightType lightType, string a)
    {
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

        //for (int i = 0; i < cubePoints.Length; i++)
        //{
        //    Vector4 p = Vector4.Transform(new Vector4(cubePoints[i].X, cubePoints[i].Y, cubePoints[i].Z, 1.0f), matrix);
        //    float w = -p.W;
        //    Vector4 auvar58 = p / p.W;
        //    float auvar50 = Math.Abs(w);

        //    cubePoints[i] = (auvar50 >= 0.0001f) ? new Vector3(auvar58.X, auvar58.Y, auvar58.Z) : new Vector3(0, 0, 1);
        //    Console.WriteLine($"{cubePoints[i]}");
        //}

        //System.Numerics.Vector3 min = new System.Numerics.Vector3(float.MaxValue);
        //System.Numerics.Vector3 max = new System.Numerics.Vector3(float.MinValue);

        //foreach (var point in cubePoints)
        //{
        //    min = System.Numerics.Vector3.Min(min, point.ToSys());
        //    max = System.Numerics.Vector3.Max(max, point.ToSys());
        //}

        //Console.WriteLine($"{Hash}: Min: {min}, Max: {max}");

        switch (lightType)
        {
            case LightType.Spot:
                // Dimensions of the pyramid
                float baseWH = cubePoints[1].Y * 2f; // Width of the base
                float coneHeight = cubePoints[1].X - cubePoints[0].X;
                float radianFOV = MathF.Atan((baseWH / 2) / coneHeight) * 2;
                return new(radianFOV, coneHeight, coneHeight);
            case LightType.Line:
                return cubePoints[0];
            default:
                return cubePoints[0];
        }

    }

    public struct LightData
    {
        public FileHash Hash;
        public FileHash Material;
        public LightType LightType;
        public Transform Transform;
        public Vector3 Size;
        public Vector4 Color;
        public FileHash Cookie;
        public float Attenuation;
        public byte[] Bytecode;
        public Vector4[] BytecodeConstants;
    }

    public enum LightType
    {
        Point,
        Spot,
        Line,
        Shadowing
    }
}

/// <summary>
/// Map Light
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801BEA, 0x10)] //EA1B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806F5A, 0x18)] //5A6F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806A63, 0x18)] //636A8080
public struct SMapLightResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public Lights Lights;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A5B, 0x60)] //5B1A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080713A, 0x60)] //3A718080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806C65, 0x60)] //656C8080
public struct SMapLights
{
    [SchemaField(0x10)]
    public Vector4 Unk10;
    public Vector4 Unk20;
    public DynamicArrayUnloaded<SMapLightCollection> LightData;
    public DynamicArrayUnloaded<S80809F4F> Transforms;
    [SchemaField(0x54, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x58, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public Tag<SOcclusionBounds> Bounds;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801C2F, 0x90)] //2F1C8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080713E, 0xA0)] //3E718080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806C70, 0xF0)] //706C8080
public struct SMapLightCollection
{
    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x60, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Matrix4x4 LightToWorld;
    // Techniques between

    [SchemaField(0x80, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xC0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0xC4, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Material Shading;

    [SchemaField(0x84, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x88, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xCC, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0xD0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<S80806DA1> BufferData;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0xD0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0xD4, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Tag<S80806DA1> BufferData2;
}
