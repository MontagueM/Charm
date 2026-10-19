using Tiger.Schema.Entity;

namespace Tiger.Schema.Animation;

using Vector3 = System.Numerics.Vector3;

// for the custom renderer
public class SkinningFrames
{
    public const int FloatsPerBone = 12;
    public int BoneCount => _inverseBind.Length;

    public int FrameCount;
    public double FrameRate;
    public double Duration => FrameRate > 0 && FrameCount > 1 ? (FrameCount - 1) / FrameRate : 0;

    public LayerType LayerType => _clip.Mode;
    public bool IsReplacement => _clip.Mode == LayerType.Replacement;
    public bool IsAdditive => _clip.Mode == LayerType.Additive;

    private RigProgram _program;
    private ClipChannels _clip;
    private Mat4[] _inverseBind;

    public int FirstBone;
    public RigTransform? Anchor;

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

    public static void Blend(SkinningFrames baseClip, double baseFrame, SkinningFrames? layer, double layerFrame, float weight, bool loop, float[] destination)
    {
        RigProgram program = baseClip._program;
        int count = program.BoneCount;
        if (baseClip.FrameCount == 0 || destination.Length < baseClip.BoneCount * FloatsPerBone)
            return;

        var values = baseClip._values;
        program.CopyDefaults(values);
        baseClip._clip.Apply(baseFrame, loop, values, 1f);

        weight = Math.Clamp(weight, 0f, 1f);
        if (layer != null && weight > 0f && layer.FrameCount > 0 && layer._program.ControlCount == program.ControlCount && layer._program.BoneCount == count)
            layer._clip.Apply(layerFrame, loop, values, weight);

        var pose = baseClip._pose;
        program.Evaluate(values, pose);

        int outputs = baseClip.BoneCount;
        for (int i = 0; i < outputs; i++)
        {
            var t = pose[baseClip.FirstBone + i];
            if (baseClip.Anchor is RigTransform anchor)
                t = RigTransform.Compose(anchor, t);

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
    public static SkinningFrames Build(Animation animation, EntityRuntimeRig rig, EntitySkeleton skeleton, double frameRate = 30)
    {
        RigProgram program = new(rig, skeleton);
        ClipChannels clip = new(animation, AnimationDecoder.Decode(animation), program);

        var bones = skeleton.BoneNodes;
        Mat4[] inverseBind = new Mat4[bones.Count];
        for (int i = 0; i < bones.Count; i++)
            inverseBind[i] = Mat4.FromTransform(bones[i].DefaultInverseObjectSpaceTransform);

        SkinningFrames frames = new(program, clip, inverseBind, frameRate);
        frames.Sample(0, false, new float[frames.BoneCount * SkinningFrames.FloatsPerBone]);
        return frames;
    }

    // the weapons bones are the last ones of the first person skeleton, returns the first of them
    public static int WeaponFirstBone(EntitySkeleton rigSkeleton, EntitySkeleton weaponSkeleton)
    {
        var rigBones = rigSkeleton.BoneNodes;
        var weaponBones = weaponSkeleton.BoneNodes;
        int first = rigBones.Count - weaponBones.Count;
        if (weaponBones.Count == 0 || first < 1)
            throw new Exception($"Skeleton has {rigBones.Count} bones, weapon has {weaponBones.Count}.");

        for (int i = 0; i < weaponBones.Count; i++)
        {
            var rigBone = rigBones[first + i];
            int parent = weaponBones[i].ParentNodeIndex;
            bool parentMatches = parent < 0 ? rigBone.ParentNodeIndex >= 0 && rigBone.ParentNodeIndex < first : rigBone.ParentNodeIndex == first + parent;
            if (rigBone.Hash.Hash32 != weaponBones[i].Hash.Hash32 || !parentMatches)
                throw new Exception($"Weapon bone {i} ({weaponBones[i].Hash}) doesnt match first person bone {first + i} ({rigBone.Hash})");
        }

        return first;
    }

    public static SkinningFrames BuildWeapon(Animation animation, EntityRuntimeRig rig, EntitySkeleton rigSkeleton, EntitySkeleton weaponSkeleton, double frameRate = 30)
    {
        var weaponBones = weaponSkeleton.BoneNodes;
        int first = WeaponFirstBone(rigSkeleton, weaponSkeleton);

        RigProgram program = new(rig, rigSkeleton);
        ClipChannels clip = new(animation, AnimationDecoder.Decode(animation), program);

        Mat4[] inverseBind = new Mat4[weaponBones.Count];
        for (int i = 0; i < weaponBones.Count; i++)
            inverseBind[i] = Mat4.FromTransform(weaponBones[i].DefaultInverseObjectSpaceTransform);

        SkinningFrames frames = new(program, clip, inverseBind, frameRate) { FirstBone = first };

        var values = new RigTransform[program.ControlCount];
        var pose = new RigTransform[program.BoneCount];
        program.CopyDefaults(values);
        clip.Apply(0, false, values, 1f);
        program.Evaluate(values, pose);
        frames.Anchor = RigTransform.Compose(RigTransform.FromObjectSpace(weaponBones[0].DefaultObjectSpaceTransform), pose[first].Inverse());

        frames.Sample(0, false, new float[frames.BoneCount * SkinningFrames.FloatsPerBone]);
        return frames;
    }
}
