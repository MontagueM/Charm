using Tiger.Schema.Entity;

namespace Tiger.Schema.Animation;

using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

public class BoneAnimation
{
    public uint BoneHash;
    public Vector3[] Scales = Array.Empty<Vector3>();
    public Vector4[] Rotations = Array.Empty<Vector4>();
    public Vector3[] Translations = Array.Empty<Vector3>();
}

public enum RetargetMode
{
    Auto,
    Absolute,
    RestRelative, // added onto the rigs defaults
}

public static class RigRetarget
{
    // evaluates every frame of the clip, object space tracks per skeleton bone
    public static BoneAnimation[] Retarget(Animation clip, BoneTracks[] controls, EntitySkeleton skeleton, EntityRuntimeRig rig, RetargetMode mode = RetargetMode.Auto)
    {
        RigProgram program = new(rig, skeleton);
        ClipChannels channels = new(clip, controls, program, mode);

        int frames = clip.FrameCount;
        var bones = skeleton.BoneNodes;
        var result = NewTracks(bones, frames);

        RigTransform[] values = new RigTransform[program.ControlCount];
        RigTransform[] pose = new RigTransform[program.BoneCount];
        for (int frame = 0; frame < frames; frame++)
        {
            program.CopyDefaults(values);
            channels.Apply(frame, false, values, 1f);
            program.Evaluate(values, pose);

            for (int i = 0; i < program.BoneCount; i++)
                SetFrame(result[i], frame, pose[i]);
        }

        return result;
    }

    public static BoneAnimation[] ToLocalSpace(Animation clip, BoneAnimation[] objectSpace, EntitySkeleton skeleton)
    {
        int frames = clip.FrameCount;
        var bones = skeleton.BoneNodes;
        var result = NewTracks(bones, frames);

        RigTransform[] transforms = new RigTransform[bones.Count];
        for (int frame = 0; frame < frames; frame++)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                var rotation = objectSpace[i].Rotations[frame];
                transforms[i] = new RigTransform(
                    new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W),
                    objectSpace[i].Translations[frame],
                    objectSpace[i].Scales[frame].X);

                int parent = bones[i].ParentNodeIndex;
                if (parent >= i)
                    throw new Exception($"Bone {i} has parent {parent}, which comes later in the skeleton");

                var local = parent > -1 ? RigTransform.Compose(transforms[parent].Inverse(), transforms[i]) : transforms[i];
                SetFrame(result[i], frame, local);
            }
        }

        return result;
    }

    private static BoneAnimation[] NewTracks(List<BoneNode> bones, int frames)
    {
        BoneAnimation[] tracks = new BoneAnimation[bones.Count];
        for (int i = 0; i < bones.Count; i++)
        {
            tracks[i] = new BoneAnimation
            {
                BoneHash = bones[i].Hash.Hash32,
                Scales = new Vector3[frames],
                Rotations = new Vector4[frames],
                Translations = new Vector3[frames],
            };
        }

        return tracks;
    }

    private static void SetFrame(BoneAnimation track, int frame, RigTransform transform)
    {
        track.Scales[frame] = new Vector3(transform.Scale);
        track.Rotations[frame] = new Vector4(transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z, transform.Rotation.W);
        track.Translations[frame] = transform.Translation;
    }
}

// glm style quaternion, (w, x, y, z) and hamilton product
public struct GlmQuat
{
    public float W;
    public float X;
    public float Y;
    public float Z;

    public GlmQuat(float w, float x, float y, float z)
    {
        W = w;
        X = x;
        Y = y;
        Z = z;
    }

    public static GlmQuat Identity => new(1, 0, 0, 0);

    public static GlmQuat operator *(GlmQuat a, GlmQuat b) => new(
        a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
        a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
        a.W * b.Y + a.Y * b.W + a.Z * b.X - a.X * b.Z,
        a.W * b.Z + a.Z * b.W + a.X * b.Y - a.Y * b.X);

    public Vector3 Rotate(Vector3 v)
    {
        Vector3 qv = new(X, Y, Z);
        Vector3 uv = Vector3.Cross(qv, v);
        Vector3 uuv = Vector3.Cross(qv, uv);
        return v + (uv * W + uuv) * 2f;
    }

    public GlmQuat Inverse()
    {
        float inv = 1f / (W * W + X * X + Y * Y + Z * Z);
        return new GlmQuat(W * inv, -X * inv, -Y * inv, -Z * inv);
    }

    public static GlmQuat AngleAxis(float angle, Vector3 axis)
    {
        float s = MathF.Sin(angle * 0.5f);
        return new GlmQuat(MathF.Cos(angle * 0.5f), axis.X * s, axis.Y * s, axis.Z * s);
    }

    public GlmQuat Normalized()
    {
        float inv = 1f / MathF.Sqrt(W * W + X * X + Y * Y + Z * Z);
        return new GlmQuat(W * inv, X * inv, Y * inv, Z * inv);
    }
}

// column major 4x4 matrix, same as glm
public struct Mat4
{
    public Vector4 C0;
    public Vector4 C1;
    public Vector4 C2;
    public Vector4 C3;

    public static Mat4 Identity => new()
    {
        C0 = new Vector4(1, 0, 0, 0),
        C1 = new Vector4(0, 1, 0, 0),
        C2 = new Vector4(0, 0, 1, 0),
        C3 = new Vector4(0, 0, 0, 1),
    };

    public Vector3 Translation => new(C3.X, C3.Y, C3.Z);

    private Vector4 Apply(Vector4 v) => C0 * v.X + C1 * v.Y + C2 * v.Z + C3 * v.W;

    public static Mat4 Mul(Mat4 a, Mat4 b) => new()
    {
        C0 = a.Apply(b.C0),
        C1 = a.Apply(b.C1),
        C2 = a.Apply(b.C2),
        C3 = a.Apply(b.C3),
    };

    public static Mat4 Compose(Vector3 translation, GlmQuat r, Vector3 scale)
    {
        r = r.Normalized();
        float xx = r.X * r.X;
        float yy = r.Y * r.Y;
        float zz = r.Z * r.Z;
        float xz = r.X * r.Z;
        float xy = r.X * r.Y;
        float yz = r.Y * r.Z;
        float wx = r.W * r.X;
        float wy = r.W * r.Y;
        float wz = r.W * r.Z;

        return new Mat4
        {
            C0 = new Vector4(1 - 2 * (yy + zz), 2 * (xy + wz), 2 * (xz - wy), 0) * scale.X,
            C1 = new Vector4(2 * (xy - wz), 1 - 2 * (xx + zz), 2 * (yz + wx), 0) * scale.Y,
            C2 = new Vector4(2 * (xz + wy), 2 * (yz - wx), 1 - 2 * (xx + yy), 0) * scale.Z,
            C3 = new Vector4(translation, 1),
        };
    }

    public static Mat4 FromTransform(ObjectSpaceTransform t)
    {
        Vector3 translation = new(t.Translation.X, t.Translation.Y, t.Translation.Z);
        GlmQuat rotation = new(t.QuaternionRotation.W, t.QuaternionRotation.X, t.QuaternionRotation.Y, t.QuaternionRotation.Z);
        return Compose(translation, rotation, new Vector3(t.Scale));
    }

    public Vector3 GetScale() => new(
        new Vector3(C0.X, C0.Y, C0.Z).Length(),
        new Vector3(C1.X, C1.Y, C1.Z).Length(),
        new Vector3(C2.X, C2.Y, C2.Z).Length());

    // glm::quat_cast on the rotation part with scale removed
    public GlmQuat GetQuat()
    {
        Vector3 s = GetScale();
        float sx = s.X > 0.00001f ? s.X : 1f;
        float sy = s.Y > 0.00001f ? s.Y : 1f;
        float sz = s.Z > 0.00001f ? s.Z : 1f;

        // m[column][row]
        float m00 = C0.X / sx, m01 = C0.Y / sx, m02 = C0.Z / sx;
        float m10 = C1.X / sy, m11 = C1.Y / sy, m12 = C1.Z / sy;
        float m20 = C2.X / sz, m21 = C2.Y / sz, m22 = C2.Z / sz;

        float fourX = m00 - m11 - m22;
        float fourY = m11 - m00 - m22;
        float fourZ = m22 - m00 - m11;
        float fourW = m00 + m11 + m22;

        int biggest = 0;
        float fourBiggest = fourW;
        if (fourX > fourBiggest)
        {
            fourBiggest = fourX;
            biggest = 1;
        }
        if (fourY > fourBiggest)
        {
            fourBiggest = fourY;
            biggest = 2;
        }
        if (fourZ > fourBiggest)
        {
            fourBiggest = fourZ;
            biggest = 3;
        }

        float biggestVal = MathF.Sqrt(fourBiggest + 1f) * 0.5f;
        float mult = 0.25f / biggestVal;

        GlmQuat q = biggest switch
        {
            0 => new GlmQuat(biggestVal, (m12 - m21) * mult, (m20 - m02) * mult, (m01 - m10) * mult),
            1 => new GlmQuat((m12 - m21) * mult, biggestVal, (m01 + m10) * mult, (m20 + m02) * mult),
            2 => new GlmQuat((m20 - m02) * mult, (m01 + m10) * mult, biggestVal, (m12 + m21) * mult),
            _ => new GlmQuat((m01 - m10) * mult, (m20 + m02) * mult, (m12 + m21) * mult, biggestVal),
        };
        return q.Normalized();
    }

    public void Decompose(out Vector3 scale, out GlmQuat rotation, out Vector3 translation)
    {
        scale = GetScale();
        rotation = GetQuat();
        translation = Translation;
    }

    public Mat4 Inverse()
    {
        Matrix4x4 m = new(
            C0.X, C0.Y, C0.Z, C0.W,
            C1.X, C1.Y, C1.Z, C1.W,
            C2.X, C2.Y, C2.Z, C2.W,
            C3.X, C3.Y, C3.Z, C3.W);

        if (!Matrix4x4.Invert(m, out Matrix4x4 inv))
            throw new Exception("Matrix is not invertible");

        return new Mat4
        {
            C0 = new Vector4(inv.M11, inv.M12, inv.M13, inv.M14),
            C1 = new Vector4(inv.M21, inv.M22, inv.M23, inv.M24),
            C2 = new Vector4(inv.M31, inv.M32, inv.M33, inv.M34),
            C3 = new Vector4(inv.M41, inv.M42, inv.M43, inv.M44),
        };
    }
}
