using Tiger.Exporters;

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

        // This sucks and is stupid
        //var color = data.TagData.Buffer1.Where(x => (x.Vec.X != 0 && x.Vec.Y != 0 && x.Vec.Z != 0 && x.Vec.W != 0)).Count() != 0 ?
        //    data.TagData.Buffer1.Where(x => (x.Vec.X != 0 && x.Vec.Y != 0 && x.Vec.Z != 0 && x.Vec.W != 0)).FirstOrDefault().Vec :
        //    data.TagData.Buffer2.Where(x => (x.Vec.X != 0 && x.Vec.Y != 0 && x.Vec.Z != 0 && x.Vec.W != 0)).FirstOrDefault().Vec;

        List<Vec4> possibleColors = data.TagData.Buffer1.ToList();
        possibleColors.AddRange(data.TagData.Buffer2.ToList());

        Vector4 color = possibleColors.Count == 0 ? Vector4.Zero : possibleColors.MaxBy(v => v.Vec.Magnitude).Vec;
        Vector2 size = GetSize();

        Lights.LightData lightData = new()
        {
            Hash = data.Hash,
            LightType = Lights.LightType.Shadowing,
            Color = color,
            Size = new Vector2(_tag.HalfFOV * 2.0f, 1f),
            Range = size.Y, //_tag.Distance.W,
            Transform = new()
            {
                Position = mapEntry.Translation.ToVec3(),
                Quaternion = mapEntry.Rotation
            }
        };

        //Console.WriteLine($"{Hash}");
        //Console.WriteLine($"FOV: {_tag.HalfFOV * 2.0f} (Calculated {size.X})");
        //Console.WriteLine($"Distance: {_tag.Distance.W} (Calculated {size.Y})\n");

        scene.AddMapLight(lightData);
        data.Dump($"{savePath}/Shaders/Source2/Lights");

        //TfxBytecodeInterpreter bytecode = new(TfxBytecodeOp.ParseAll(data.TagData.Bytecode));
        //var bytecode_hlsl = bytecode.Evaluate(data.TagData.Buffer2, true);
    }

    public Vector2 GetSize()
    {
        Matrix4x4 matrix = _tag.LightToWorld;
        //StringBuilder sb = new();
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
            //sb.AppendLine($"v {cubePoints[i].X} {cubePoints[i].Y} {cubePoints[i].Z}");
        }
        //File.WriteAllText($"C:\\Users\\Michael\\Desktop\\test\\{Hash}.obj", sb.ToString());

        float baseWH = cubePoints[1].Y * 2f; // Width of the base
        float coneHeight = cubePoints[1].X - cubePoints[0].X;
        float radianFOV = MathF.Atan((baseWH / 2) / coneHeight) * 2;
        return new(radianFOV, coneHeight);
    }
}
