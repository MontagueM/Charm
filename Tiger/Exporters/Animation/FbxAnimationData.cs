using Tiger.Schema.Entity;

namespace Tiger.Schema.Animation;

using Vector3 = System.Numerics.Vector3;

public sealed class FbxBoneCurves
{
    public uint BoneHash;
    public float[] TranslationX = Array.Empty<float>();
    public float[] TranslationY = Array.Empty<float>();
    public float[] TranslationZ = Array.Empty<float>();
    public float[] RotationX = Array.Empty<float>();
    public float[] RotationY = Array.Empty<float>();
    public float[] RotationZ = Array.Empty<float>();
    public float[] ScaleX = Array.Empty<float>();
    public float[] ScaleY = Array.Empty<float>();
    public float[] ScaleZ = Array.Empty<float>();
}

public sealed class FbxAnimationData
{
    public string Name = "";
    public int FrameCount;
    public double FrameRate = 30;
    public LayerType LayerType;
    public FbxBoneCurves[] Bones = Array.Empty<FbxBoneCurves>();
}

public static class FbxAnimationBuilder
{
    public static FbxAnimationData Build(string name, BoneAnimation[] objectSpace, EntitySkeleton skeleton, LayerType layerType, int frameCount, double frameRate = 30)
    {
        List<BoneNode> nodes = skeleton.BoneNodes;
        int n = nodes.Count;
        if (objectSpace.Length != n)
            throw new InvalidDataException($"The animation has {objectSpace.Length} bones but the skeleton has {n}");

        var data = new FbxAnimationData { Name = name, FrameCount = frameCount, FrameRate = frameRate, LayerType = layerType, Bones = new FbxBoneCurves[n] };
        for (int b = 0; b < n; b++)
        {
            data.Bones[b] = new FbxBoneCurves
            {
                BoneHash = nodes[b].Hash.Hash32,
                TranslationX = new float[frameCount],
                TranslationY = new float[frameCount],
                TranslationZ = new float[frameCount],
                RotationX = new float[frameCount],
                RotationY = new float[frameCount],
                RotationZ = new float[frameCount],
                ScaleX = new float[frameCount],
                ScaleY = new float[frameCount],
                ScaleZ = new float[frameCount],
            };
        }

        var inverseBindRotation = new GlmQuat[n];
        for (int b = 0; b < n; b++)
        {
            var r = nodes[b].DefaultObjectSpaceTransform.QuaternionRotation;
            inverseBindRotation[b] = new GlmQuat(r.W, r.X, r.Y, r.Z).Normalized().Inverse();
        }

        var global = new Mat4[n];
        var previousAngles = new Vector3?[n];

        for (int f = 0; f < frameCount; f++)
        {
            for (int b = 0; b < n; b++)
            {
                BoneAnimation bone = objectSpace[b];
                int parent = nodes[b].ParentNodeIndex;
                if (parent >= b)
                    throw new InvalidDataException($"Bone {b} has parent {parent}, which comes later in the skeleton");

                Vector3 translation = bone.Translations[f], scale = bone.Scales[f];
                System.Numerics.Vector4 rp = bone.Rotations[f];
                var animated = new GlmQuat(rp.W, rp.X, rp.Y, rp.Z);

                global[b] = Mat4.Compose(translation, animated * inverseBindRotation[b], scale);

                Mat4 local = parent >= 0 ? Mat4.Mul(global[parent].Inverse(), global[b]) : global[b];
                local.Decompose(out Vector3 s, out GlmQuat r, out Vector3 t);

                Vector3 angles = EulerXyz.FromQuaternion(r, previousAngles[b]);
                previousAngles[b] = angles;

                FbxBoneCurves c = data.Bones[b];
                c.TranslationX[f] = t.X; c.TranslationY[f] = t.Y; c.TranslationZ[f] = t.Z;
                c.RotationX[f] = angles.X; c.RotationY[f] = angles.Y; c.RotationZ[f] = angles.Z;
                c.ScaleX[f] = s.X; c.ScaleY[f] = s.Y; c.ScaleZ[f] = s.Z;
            }
        }

        return data;
    }
}

/// <summary>Quaternion to Euler angles for FBX's default XYZ rotation order (R = Rz * Ry * Rx).</summary>
internal static class EulerXyz
{
    public static Vector3 FromQuaternion(GlmQuat q, Vector3? previous)
    {
        q = q.Normalized();
        double r00 = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        double r10 = 2 * (q.X * q.Y + q.W * q.Z);
        double r20 = 2 * (q.X * q.Z - q.W * q.Y);
        double r21 = 2 * (q.Y * q.Z + q.W * q.X);
        double r22 = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        double r11 = 1 - 2 * (q.X * q.X + q.Z * q.Z);
        double r12 = 2 * (q.Y * q.Z - q.W * q.X);

        double sinY = Math.Clamp(-r20, -1.0, 1.0);

        // z comes from the first column. Near gimbal lock (pitch of +-90 degrees) that column is mostly rounding noise, but
        // the noise only matters multiplied by cos(pitch), so a poor z is harmless as long as x is solved against it.
        // Below the cutoff z is carried over from the previous frame, which also keeps the curve smooth there.
        double cosY = Math.Sqrt(r21 * r21 + r22 * r22);
        double y = Math.Atan2(sinY, cosY); // asin would lose half its precision near +-90 degrees
        double z = cosY > 1e-6 ? Math.Atan2(r10, r00)
                 : previous.HasValue ? previous.Value.Z * Math.PI / 180.0 : 0.0;

        // x from the second row of Rz(-z) * R, which is (0, cos x, -sin x) and well conditioned at any pitch
        double r01 = 2 * (q.X * q.Y - q.W * q.Z);
        double r02 = 2 * (q.X * q.Z + q.W * q.Y);
        double cz = Math.Cos(z), sz = Math.Sin(z);
        double x = Math.Atan2(-(-sz * r02 + cz * r12), -sz * r01 + cz * r11);

        const double Deg = 180.0 / Math.PI;
        var first = new double[] { x * Deg, y * Deg, z * Deg };
        if (previous == null)
            return new Vector3((float)first[0], (float)first[1], (float)first[2]);

        var second = new double[] { first[0] + 180, 180 - first[1], first[2] + 180 };
        double[] prev = { previous.Value.X, previous.Value.Y, previous.Value.Z };

        double[] best = Unwrap(first, prev), alt = Unwrap(second, prev);
        double costBest = 0, costAlt = 0;
        for (int i = 0; i < 3; i++)
        {
            costBest += Math.Abs(best[i] - prev[i]);
            costAlt += Math.Abs(alt[i] - prev[i]);
        }
        if (costAlt < costBest)
            best = alt;

        return new Vector3((float)best[0], (float)best[1], (float)best[2]);
    }

    private static double[] Unwrap(double[] angles, double[] previous)
    {
        var result = new double[3];
        for (int i = 0; i < 3; i++)
            result[i] = angles[i] + 360.0 * Math.Round((previous[i] - angles[i]) / 360.0);
        return result;
    }

    public static GlmQuat ToQuaternion(Vector3 degrees)
    {
        const float Rad = (float)(Math.PI / 180.0);
        return GlmQuat.AngleAxis(degrees.Z * Rad, new Vector3(0, 0, 1))
             * GlmQuat.AngleAxis(degrees.Y * Rad, new Vector3(0, 1, 0))
             * GlmQuat.AngleAxis(degrees.X * Rad, new Vector3(1, 0, 0));
    }
}
