using Tiger.Exporters;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class ShadowingLights : Tag<SMapShadowingLight>
{
    public ShadowingLights(FileHash hash) : base(hash)
    {
    }

    public void LoadIntoExporter(ExporterScene scene, SMapDataEntry mapEntry, string savePath)
    {
        var data = (Strategy.CurrentStrategy < TigerStrategy.DESTINY2_BEYONDLIGHT_3402 || _tag.BufferData2 is null) ? _tag.BufferData : _tag.BufferData2;
        if (data is null)
            return;

        List<Vec4> possibleColors = data.TagData.Buffer1.ToList();
        possibleColors.AddRange(data.TagData.Buffer2.ToList());

        Vector4 color = GetColor(data);
        Vector2 size = GetSize();
        Texture cookie = null;

        IMaterial shading = FileResourcer.Get().GetFileInterface<IMaterial>(_tag.Shading);
        if (shading.EnumeratePSTextures().Any())
        {
            cookie = shading.EnumeratePSTextures().First().Texture;
            cookie.SavetoFile($"{savePath}/Textures/{cookie.Hash}");
            if (ConfigSubsystem.Get().GetS2ShaderExportEnabled())
                Source2Handler.SaveVTEX(cookie, $"{savePath}/Textures");
        }

        Lights.LightData lightData = new()
        {
            Hash = data.Hash,
            LightType = Lights.LightType.Shadowing,
            Color = color,
            Size = new Vector2(_tag.HalfFOV * 2.0f, 1f),
            Range = size.Y,
            Attenuation = _tag.BufferData.TagData.Buffer2[1].Vec.W,
            Transform = new()
            {
                Position = mapEntry.Translation.ToVec3(),
                Quaternion = mapEntry.Rotation
            },
            Cookie = cookie != null ? cookie.Hash : null
        };

        scene.AddMapLight(lightData);

        if (ConfigSubsystem.Get().GetS2ShaderExportEnabled())
            data.Dump($"{savePath}/Shaders/Source2/Lights");
    }

    public Vector4 GetColor(Tag<D2Class_A16D8080> data)
    {
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

        float baseWH = cubePoints[1].Y * 2f; // Width of the base
        float coneHeight = cubePoints[1].X - cubePoints[0].X;
        float radianFOV = MathF.Atan((baseWH / 2) / coneHeight) * 2;
        return new(radianFOV, coneHeight);
    }
}
