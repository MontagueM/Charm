namespace Tiger.Schema.Animation;

using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

public class HandConstraintTracks
{
    public sealed class Constraint
    {
        public int RootControl;
        public int EndControl;
        public uint PointName;
        public float[] Weight = Array.Empty<float>(); // per frame, clip +0x38. 0 = off
        public RigTransform[] Offsets = Array.Empty<RigTransform>();
    }

    public readonly List<Constraint> Constraints = new();
    public int FrameCount;

    private static T Key<T>(T[] values, int index) => ClipTime.Key(values, index);

    public (RigTransform Offset, float Weight) Sample(Constraint constraint, double frame, bool loop)
    {
        var (first, second, amount) = ClipTime.Keys(frame, loop, FrameCount);

        RigTransform offset = constraint.Offsets.Length == 0 ? RigTransform.Identity
            : RigTransform.Lerp(Key(constraint.Offsets, first), Key(constraint.Offsets, second), amount);

        float weight = constraint.Weight.Length == 0 ? 1f
            : float.Lerp(Key(constraint.Weight, first), Key(constraint.Weight, second), amount);

        return (offset, Math.Clamp(weight, 0f, 1f));
    }


    internal static float[][] DecodeScalars(S80808B4C block, int frames)
    {
        var channels = block.Unk10.Select(x => x.Unk00).ToList();
        var animated = channels.Where(x => (x & 0x4000) != 0).Select(x => x & 0x3FFF).ToList();
        var result = new float[channels.Count][];

        for (int c = 0; c < channels.Count; c++)
        {
            int k = channels[c] & 0x3FFF;
            float minimum = k < block.Unk40.Count ? block.Unk40[k].Value : 1f;
            float extent = k < block.Unk30.Count ? block.Unk30[k].Value : 0f;

            int stream = (channels[c] & 0x4000) != 0 ? animated.IndexOf(k) : -1;
            if (stream < 0 || block.Unk20.Count < frames * animated.Count)
            {
                result[c] = new[] { minimum };
                continue;
            }

            result[c] = new float[frames];
            for (int f = 0; f < frames; f++)
                result[c][f] = block.Unk20[f * animated.Count + stream].Value / 65535f * extent + minimum;
        }

        return result;
    }

    internal static RigTransform[] Offsets(DecodedStreams streams, int constraint, int frames)
    {
        var result = new RigTransform[Math.Max(frames, 1)];
        for (int f = 0; f < result.Length; f++)
        {
            float scale = constraint < streams.Scales.Length ? Key(streams.Scales[constraint], f) : 1f;
            var r = constraint < streams.Rotations.Length ? Key(streams.Rotations[constraint], f) : new(0, 0, 0, 1);
            var t = constraint < streams.Translations.Length ? Key(streams.Translations[constraint], f) : Vector3.Zero;
            result[f] = new RigTransform(Quaternion.Normalize(new Quaternion(r.X, r.Y, r.Z, r.W)), t, scale);
        }

        return result;
    }
}
