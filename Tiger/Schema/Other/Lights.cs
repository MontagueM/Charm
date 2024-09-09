using Tiger.Exporters;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class Lights : Tag<D2Class_656C8080>
{
    public Lights(FileHash hash) : base(hash)
    {
    }

    public void LoadIntoExporter(ExporterScene scene, string savePath)
    {
        using TigerReader reader = GetReader();
        for (int i = 0; i < _tag.LightData.Count; i++)
        {
            var data = _tag.LightData.ElementAt(reader, i);

            var bufferData = (Strategy.CurrentStrategy < TigerStrategy.DESTINY2_BEYONDLIGHT_3402 || data.BufferData2 is null) ? data.BufferData : data.BufferData2;
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
                IMaterial shading = FileResourcer.Get().GetFileInterface<IMaterial>(data.Shading);
                if (shading.EnumeratePSTextures().Any())
                {
                    cookie = shading.EnumeratePSTextures().First().Texture;
                    cookie.SavetoFile($"{savePath}/Textures/{cookie.Hash}");
                    if (ConfigSubsystem.Get().GetS2ShaderExportEnabled())
                        Source2Handler.SaveVTEX(cookie, $"{savePath}/Textures");
                }
            }

            Vector3 size = GetSize(data.LightToWorld, lightType, $"{lightType}_{data.BufferData.Hash}_{i}");
            var bounds = _tag.Bounds.TagData.InstanceBounds.ElementAt(_tag.Bounds.GetReader(), i);
            var transforms = _tag.Transforms.ElementAt(reader, i);
            LightData lightData = new()
            {
                Hash = bufferData.Hash,
                LightType = lightType,
                Color = color,
                Size = new(size.X, size.Y),
                Range = size.Z,
                Attenuation = data.BufferData.TagData.Buffer2[1].Vec.W, // Completely unsure, just testing
                Transform = new()
                {
                    Position = transforms.Translation.ToVec3(),
                    Quaternion = transforms.Rotation
                },
                Cookie = cookie != null ? cookie.Hash : null
            };

            scene.AddMapLight(lightData);

            if (ConfigSubsystem.Get().GetS2ShaderExportEnabled())
                bufferData.Dump($"{savePath}/Shaders/Source2/Lights");
        }
    }

    public Vector4 GetColor(Tag<D2Class_A16D8080> data)
    {
        //Console.WriteLine($"{data.TagData.Buffer2[0].Vec} : {data.TagData.Buffer2[1].Vec} : {data.TagData.Buffer2.Count(x => x.Vec.Magnitude != 0)}");
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
            new Vector3(-1f, -1f, -1f),
            new Vector3(-1f, -1f, 1f),
            new Vector3(-1f, 1f, -1f),
            new Vector3(-1f, 1f, 1f),
            new Vector3(1f, -1f, -1f),
            new Vector3(1f, -1f, 1f),
            new Vector3(1f, 1f, -1f),
            new Vector3(1f, 1f, 1f)
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
            var b = (matrix.W_Axis + r0);

            cubePoints[i] = (b / new Vector4(b.W)).ToVec3();
        }

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
        public LightType LightType;
        public Transform Transform;
        public Vector2 Size;
        public Vector4 Color;
        public FileHash Cookie;
        public float Range;
        public float Attenuation;
    }

    public enum LightType
    {
        Point,
        Spot,
        Line,
        Shadowing
    }
}
