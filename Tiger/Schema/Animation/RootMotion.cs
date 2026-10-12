namespace Tiger.Schema.Animation;

using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

public class RootMotion
{
    public byte ExtractionMode; // 0 none, 1 planar translation, 2 planar translation + turning, 3 translation + turning
    public RigTransform CycleTransform;
    public int FrameCount => _rotations.Length;

    private Quaternion[] _rotations;
    private Vector3[] _translations;

    public RootMotion(byte extractionMode, RigTransform cycle, Vector4[] rotations, Vector3[] translations)
    {
        ExtractionMode = extractionMode;
        CycleTransform = cycle;
        _rotations = rotations.Select(x => new Quaternion(x.X, x.Y, x.Z, x.W)).ToArray();
        _translations = translations;
    }

    public RigTransform Sample(double frame, bool loop = false)
    {
        int last = FrameCount - 1;
        if (last < 0)
            return RigTransform.Identity;

        long cycles = 0;
        if (loop && last > 0)
        {
            var quotient = Math.Floor(frame / last);
            cycles = (long)quotient;
            frame -= quotient * last;
        }
        else
        {
            frame = Math.Clamp(frame, 0, last);
        }

        int first = Math.Min((int)frame, last);
        int second = Math.Min(first + 1, last);
        float amount = (float)(frame - first);

        var rotation = RigTransform.Nlerp(_rotations[first], _rotations[second], amount);
        var translation = Vector3.Lerp(_translations[first], _translations[second], amount);
        RigTransform sampled = new(rotation, translation, 1);

        if (cycles == 0)
            return sampled;

        return RigTransform.Compose(Repeat(CycleTransform, cycles), sampled);
    }

    // the motion from one frame to another, in the space of the first
    public RigTransform Delta(double fromFrame, double toFrame, bool loop = false)
    {
        return RigTransform.Compose(Sample(fromFrame, loop).Inverse(), Sample(toFrame, loop));
    }

    // composes the transform with itself count times, negative when looping backwards
    private static RigTransform Repeat(RigTransform value, long count)
    {
        if (count < 0)
        {
            value = value.Inverse();
            count = -count;
        }

        var result = RigTransform.Identity;
        while (count != 0)
        {
            if ((count & 1) != 0)
                result = RigTransform.Compose(result, value);

            count >>= 1;
            if (count != 0)
                value = RigTransform.Compose(value, value);
        }

        return result;
    }
}
