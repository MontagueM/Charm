using Tiger.Schema.Entity;

namespace Tiger.Schema.Animation;

using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

// rotation, translation and uniform scale, same as how the rig stores bones and control values
public struct RigTransform
{
    public Quaternion Rotation;
    public Vector3 Translation;
    public float Scale;

    public RigTransform(Quaternion rotation, Vector3 translation, float scale)
    {
        Rotation = rotation;
        Translation = translation;
        Scale = scale;
    }

    public static RigTransform Identity => new(Quaternion.Identity, Vector3.Zero, 1);

    public static RigTransform FromObjectSpace(ObjectSpaceTransform t)
    {
        Quaternion rotation = new(t.QuaternionRotation.X, t.QuaternionRotation.Y, t.QuaternionRotation.Z, t.QuaternionRotation.W);
        return new RigTransform(Quaternion.Normalize(rotation), new Vector3(t.Translation.X, t.Translation.Y, t.Translation.Z), t.Scale);
    }

    // child in the space of parent
    public static RigTransform Compose(RigTransform parent, RigTransform child)
    {
        var rotation = Quaternion.Normalize(parent.Rotation * child.Rotation);
        var translation = Vector3.Transform(child.Translation, parent.Rotation) * parent.Scale + parent.Translation;
        return new RigTransform(rotation, translation, parent.Scale * child.Scale);
    }

    public RigTransform Inverse()
    {
        var rotation = Quaternion.Conjugate(Quaternion.Normalize(Rotation));
        return new RigTransform(rotation, -Vector3.Transform(Translation, rotation) / Scale, 1 / Scale);
    }

    public static RigTransform Lerp(RigTransform a, RigTransform b, float amount)
    {
        return new RigTransform(Nlerp(a.Rotation, b.Rotation, amount), Vector3.Lerp(a.Translation, b.Translation, amount), a.Scale + (b.Scale - a.Scale) * amount);
    }

    public static Quaternion Nlerp(Quaternion a, Quaternion b, float amount) => Quaternion.Lerp(a, b, amount);
}

public static class RigMath
{
    public static Vector3 NormalizeOrZero(Vector3 value)
    {
        float length = value.Length();
        return length > 0 ? value / length : Vector3.Zero;
    }

    // shortest rotation from one unit vector to another, pole picks the axis when theyre opposite
    public static Quaternion FromTo(Vector3 from, Vector3 to, Vector3 pole)
    {
        if (Vector3.Dot(from, to) < -0.9999f)
        {
            var side = Vector3.Normalize(Vector3.Cross(from, pole));
            var axis = Vector3.Normalize(Vector3.Cross(from, side));
            return new Quaternion(axis, 0);
        }

        var half = Vector3.Normalize(from + to);
        return Quaternion.Normalize(new Quaternion(Vector3.Cross(from, half), Vector3.Dot(from, half)));
    }

    // turns +X to the direction, then the given axis toward the reference
    public static Quaternion LookRotation(Vector3 direction, Vector3 reference, Vector3 axis)
    {
        var x = NormalizeOrZero(direction);
        var side = NormalizeOrZero(Vector3.Cross(Vector3.Cross(direction, reference), direction));
        var first = FromTo(Vector3.UnitX, x, side);
        var second = FromTo(Vector3.Transform(axis, first), side, x);
        return Quaternion.Normalize(second * first);
    }
}
