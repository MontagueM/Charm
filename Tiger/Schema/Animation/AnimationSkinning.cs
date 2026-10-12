using Tiger.Schema.Entity;

namespace Tiger.Schema.Animation;

using Vector3 = System.Numerics.Vector3;

// for the custom renderer
public class SkinningFrames
{
    public const int FloatsPerBone = 12;
    public int BoneCount => _program.BoneCount;

    public int FrameCount;
    public double FrameRate;
    public double Duration => FrameRate > 0 && FrameCount > 1 ? (FrameCount - 1) / FrameRate : 0;

    public LayerType LayerType => _clip.Mode;
    public bool IsReplacement => _clip.Mode == LayerType.Replacement;
    public bool IsAdditive => _clip.Mode == LayerType.Additive;

    private RigProgram _program;
    private ClipChannels _clip;
    private Mat4[] _inverseBind;

    private RigTransform[] _values;
    private RigTransform[] _pose;

    public SkinningFrames(RigProgram program, ClipChannels clip, Mat4[] inverseBind, double frameRate)
    {
        _program = program;
        _clip = clip;
        _inverseBind = inverseBind;
        FrameCount = clip.FrameCount;
        FrameRate = frameRate;
        _values = new RigTransform[program.ControlCount];
        _pose = new RigTransform[program.BoneCount];
    }

    public void Sample(double frame, bool loop, float[] destination) => Blend(this, frame, null, 0, 0f, loop, destination);

    // the base clip goes over the rigs default values, the layer goes over that, then the rig runs once
    public static void Blend(SkinningFrames baseClip, double baseFrame, SkinningFrames? layer, double layerFrame, float weight, bool loop, float[] destination)
    {
        RigProgram program = baseClip._program;
        int count = program.BoneCount;
        if (baseClip.FrameCount == 0 || destination.Length < count * FloatsPerBone)
            return;

        var values = baseClip._values;
        program.CopyDefaults(values);
        baseClip._clip.Apply(baseFrame, loop, values, 1f);

        weight = Math.Clamp(weight, 0f, 1f);
        if (layer != null && weight > 0f && layer.FrameCount > 0 && layer._program.ControlCount == program.ControlCount && layer.BoneCount == count)
            layer._clip.Apply(layerFrame, loop, values, weight);

        var pose = baseClip._pose;
        program.Evaluate(values, pose);

        for (int i = 0; i < count; i++)
        {
            var t = pose[i];
            var rotation = new GlmQuat(t.Rotation.W, t.Rotation.X, t.Rotation.Y, t.Rotation.Z);
            var skin = Mat4.Mul(Mat4.Compose(t.Translation, rotation, new Vector3(t.Scale)), baseClip._inverseBind[i]);

            int o = i * FloatsPerBone;
            destination[o + 0] = skin.C0.X; destination[o + 1] = skin.C1.X; destination[o + 2] = skin.C2.X; destination[o + 3] = skin.C3.X;
            destination[o + 4] = skin.C0.Y; destination[o + 5] = skin.C1.Y; destination[o + 6] = skin.C2.Y; destination[o + 7] = skin.C3.Y;
            destination[o + 8] = skin.C0.Z; destination[o + 9] = skin.C1.Z; destination[o + 10] = skin.C2.Z; destination[o + 11] = skin.C3.Z;
        }
    }
}

public static class AnimationSkinning
{
    public static SkinningFrames Build(Animation animation, EntityRuntimeRig rig, EntitySkeleton skeleton, double frameRate = 30, RetargetMode mode = RetargetMode.Auto)
    {
        RigProgram program = new(rig, skeleton);
        ClipChannels clip = new(animation, AnimationDecoder.Decode(animation), program, mode);

        var bones = skeleton.BoneNodes;
        Mat4[] inverseBind = new Mat4[bones.Count];
        for (int i = 0; i < bones.Count; i++)
            inverseBind[i] = Mat4.FromTransform(bones[i].DefaultInverseObjectSpaceTransform);

        SkinningFrames frames = new(program, clip, inverseBind, frameRate);

        // run it once now so a broken clip fails here and not on the render thread
        frames.Sample(0, false, new float[frames.BoneCount * SkinningFrames.FloatsPerBone]);
        return frames;
    }
}
