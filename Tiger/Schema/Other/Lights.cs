using Tiger.Exporters;

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

            // This sucks and is stupid
            //Vector4 color = data.BufferData.TagData.Buffer1.Where(x => (x.Vec.X != 0 && x.Vec.Y != 0 && x.Vec.Z != 0)).Count() != 0 ?
            //    data.BufferData.TagData.Buffer1.Where(x => (x.Vec.X != 0 && x.Vec.Y != 0 && x.Vec.Z != 0)).FirstOrDefault().Vec :
            //    data.BufferData.TagData.Buffer2.Where(x => (x.Vec.X != 0 && x.Vec.Y != 0 && x.Vec.Z != 0)).FirstOrDefault().Vec;

            List<Vec4> possibleColors = bufferData.TagData.Buffer1.ToList();
            possibleColors.AddRange(bufferData.TagData.Buffer2.ToList());

            // This also isn't great but it's slightly better...?
            Vector4 color = possibleColors.Count == 0 ? Vector4.Zero : possibleColors.MaxBy(v => v.Vec.Magnitude).Vec;

            LightType lightType = LightType.Point;
            if (MathF.Abs(data.LightToWorld.X_Axis.X) == 0.0)
                lightType = LightType.Spot;
            else if (MathF.Abs(data.LightToWorld.X_Axis.X) != MathF.Abs(data.LightToWorld.Y_Axis.Y) || MathF.Abs(data.LightToWorld.Y_Axis.Y) != MathF.Abs(data.LightToWorld.Z_Axis.Z))
                lightType = LightType.Line;

            Vector3 size = GetSize(data.LightToWorld, lightType, $"{lightType}_{data.BufferData.Hash}_{i}");
            var bounds = _tag.Bounds.TagData.InstanceBounds.ElementAt(_tag.Bounds.GetReader(), i);
            var transforms = _tag.Transforms.ElementAt(reader, i);
            LightData lightData = new()
            {
                Hash = bufferData.Hash,
                LightType = lightType,
                Color = color,
                Size = new(size.X, size.Y),
                Range = size.Z, //data.Distance.W, //bounds.Corner2.X - bounds.Corner1.X, // Not right
                Transform = new()
                {
                    Position = transforms.Translation.ToVec3(),
                    Quaternion = transforms.Rotation
                }
            };

            scene.AddMapLight(lightData);

            bufferData.Dump($"{savePath}/Shaders/Source2/Lights");

            //TfxBytecodeInterpreter bytecode = new(TfxBytecodeOp.ParseAll(bufferData.TagData.Bytecode));
            //var bytecode_hlsl = bytecode.Evaluate(bufferData.TagData.Buffer1, true);
        }
    }

    public Vector3 GetSize(Matrix4x4 matrix, LightType lightType, string a)
    {
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
        //File.WriteAllText($"C:\\Users\\Michael\\Desktop\\test\\{a}.obj", sb.ToString());

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
        public float Range;
    }

    public enum LightType
    {
        Point,
        Spot,
        Line,
        Shadowing
    }
}
