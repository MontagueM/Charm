namespace Tiger.Schema.Animation;

using Arithmic;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

public struct ChannelTrack
{
    public int Control;
    public float[]? Scales;
    public Quaternion[]? Rotations;
    public Vector3[]? Translations;
}

// a decoded clip bound to a rig, channels moved onto rig controls and scaled to the rigs size
public class ClipChannels
{
    public int FrameCount;
    public LayerType Mode;
    private List<ChannelTrack> _tracks = new();
    private HashSet<int> _controls = new();

    public bool HasChannel(int control) => _controls.Contains(control);

    public ClipChannels(Animation clip, BoneTracks[] decoded, RigProgram program)
    {
        if (program.CompatibleControlCount(clip) == 0)
        {
            //Debug.Assert(false);
            Log.Error($"{clip.Hash}: Clip and rig have no matching control groups. Can not play animation.");
        }

        int[] map = program.ChannelMap(clip);
        float scale = program.TranslationScale(clip);

        for (int i = 0; i < decoded.Length; i++)
        {
            int control = i < map.Length ? map[i] : -1;
            var tracks = decoded[i];
            if (control < 0 || (tracks.Scales == null && tracks.Rotations == null && tracks.Translations == null))
                continue;

            Vector3[]? translations = tracks.Translations;
            if (translations != null && scale != 1f)
                translations = translations.Select(x => x * scale).ToArray();

            _tracks.Add(new ChannelTrack
            {
                Control = control,
                Scales = tracks.Scales,
                Rotations = tracks.Rotations?.Select(x => new Quaternion(x.X, x.Y, x.Z, x.W)).ToArray(),
                Translations = translations
            });
        }

        FrameCount = clip.FrameCount;
        Mode = clip.LayerType;
        _controls = _tracks.Select(x => x.Control).ToHashSet();
    }

    private (int First, int Second, float Amount) Keys(double frame, bool loop) => ClipTime.Keys(frame, loop, FrameCount);

    // additive clips offset the values, base and replacement clips move them toward the clip by weight.
    // anything the clip doesnt have a channel for is left alone
    public void Apply(double frame, bool loop, RigTransform[] values, float weight)
    {
        if (FrameCount <= 0 || weight <= 0f)
            return;

        var (first, second, amount) = Keys(frame, loop);
        bool additive = Mode == LayerType.Additive;

        foreach (ChannelTrack track in _tracks)
        {
            var value = values[track.Control];

            if (track.Scales != null && track.Scales.Length > 0)
            {
                float a = Key(track.Scales, first);
                float s = a + (Key(track.Scales, second) - a) * amount;

                if (additive)
                    value.Scale *= 1 + weight * (s - 1);
                else
                    value.Scale += (s - value.Scale) * weight;
            }

            if (track.Rotations != null && track.Rotations.Length > 0)
            {
                var r = RigTransform.Nlerp(Key(track.Rotations, first), Key(track.Rotations, second), amount);

                if (additive)
                    value.Rotation = Quaternion.Normalize(value.Rotation * RigTransform.Nlerp(Quaternion.Identity, r, weight));
                else
                    value.Rotation = RigTransform.Nlerp(value.Rotation, r, weight);
            }

            if (track.Translations != null && track.Translations.Length > 0)
            {
                var t = Vector3.Lerp(Key(track.Translations, first), Key(track.Translations, second), amount);

                if (additive)
                    value.Translation += t * weight;
                else
                    value.Translation += (t - value.Translation) * weight;
            }

            values[track.Control] = value;
        }
    }

    private static T Key<T>(T[] keys, int frame) => ClipTime.Key(keys, frame);
}

/// <summary>
/// Clip timing shared by clip channels and hand constraint tracks
/// </summary>
internal static class ClipTime
{
    public static (int First, int Second, float Amount) Keys(double frame, bool loop, int frameCount)
    {
        int last = Math.Max(frameCount - 1, 0);
        double time;

        if (loop && last > 0)
        {
            time = frame % last;
            if (time < 0)
                time += last;
        }
        else
        {
            time = Math.Clamp(frame, 0, last);
        }

        int first = Math.Clamp((int)time, 0, last);
        return (first, Math.Min(first + 1, last), (float)Math.Clamp(time - first, 0, 1));
    }

    public static T Key<T>(T[] keys, int frame) => keys[Math.Min(frame, keys.Length - 1)];
}
