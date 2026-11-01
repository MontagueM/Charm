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

    public bool UseHandConstraints;
    public int FirstBone;
    public RigTransform? Anchor;
    public bool AnchorToRig;
    private RigTransform? ActiveAnchor => AnchorToRig ? null : Anchor;

    public IReadOnlyList<(SkinningFrames Frames, WeaponLayerClip Layer)>? TrackLayers;
    public bool UseTrackLayers;

    public SkinningFrames? FadeFrom;
    public double FadeFromFrame;
    public float FadeAmount = 1f;
    public HandConstraintTracks? HandConstraints;
    public Dictionary<uint, (int Bone, RigTransform Point)> WeaponPoints = new();
    public int WeaponSocket = -1;

    public int RigBoneCount => _program.BoneCount;
    public int RigParent(int bone) => _program.ParentOf(bone);

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

        if (baseClip.UseTrackLayers)
            baseClip.ApplyTrackLayers(baseFrame, values);

        if (baseClip.FadeFrom is SkinningFrames from && !ReferenceEquals(from, baseClip) && baseClip.FadeAmount < 1f && from._program.ControlCount == program.ControlCount)
        {
            var previous = from._values;
            program.CopyDefaults(previous);

            from._clip.Apply(baseClip.FadeFromFrame, false, previous, 1f);

            if (baseClip.UseTrackLayers)
                from.ApplyTrackLayers(baseClip.FadeFromFrame, previous);

            float amount = Math.Clamp(baseClip.FadeAmount, 0f, 1f);

            for (int i = 0; i < program.ControlCount; i++)
                values[i] = RigTransform.Lerp(previous[i], values[i], amount);
        }

        weight = Math.Clamp(weight, 0f, 1f);

        if (layer != null && weight > 0f && layer.FrameCount > 0 && layer._program.ControlCount == program.ControlCount && layer._program.BoneCount == count)
            layer._clip.Apply(layerFrame, loop, values, weight);

        var pose = baseClip._pose;
        program.Evaluate(values, pose);

        if (baseClip.UseHandConstraints)
            baseClip.ApplyHandConstraints(baseFrame, loop, pose, weight > 0f ? layer : null, layerFrame, weight);

        int outputs = baseClip.BoneCount;
        for (int i = 0; i < outputs; i++)
        {
            var t = pose[baseClip.FirstBone + i];
            if (baseClip.ActiveAnchor is RigTransform anchor)
                t = RigTransform.Compose(anchor, t);

            WriteSkin(t, baseClip._inverseBind[i], destination, i * FloatsPerBone);
        }
    }

    public static void WriteSkin(RigTransform pose, Mat4 inverseBind, float[] destination, int offset)
    {
        var rotation = new GlmQuat(pose.Rotation.W, pose.Rotation.X, pose.Rotation.Y, pose.Rotation.Z);
        var skin = Mat4.Mul(Mat4.Compose(pose.Translation, rotation, new Vector3(pose.Scale)), inverseBind);

        int o = offset;
        destination[o + 0] = skin.C0.X; destination[o + 1] = skin.C1.X; destination[o + 2] = skin.C2.X; destination[o + 3] = skin.C3.X;
        destination[o + 4] = skin.C0.Y; destination[o + 5] = skin.C1.Y; destination[o + 6] = skin.C2.Y; destination[o + 7] = skin.C3.Y;
        destination[o + 8] = skin.C0.Z; destination[o + 9] = skin.C1.Z; destination[o + 10] = skin.C2.Z; destination[o + 11] = skin.C3.Z;
    }

    private void ApplyTrackLayers(double frame, RigTransform[] values)
    {
        foreach (var (track, trackFrame, loops) in TrackLayerFrames(frame))
            track._clip.Apply(trackFrame, loops, values, 1f);
    }

    private IEnumerable<(SkinningFrames Track, double Frame, bool Loops)> TrackLayerFrames(double frame)
    {
        if (TrackLayers is not { } tracks)
            yield break;

        if (FrameCount > 1)
            frame = Math.Clamp(frame, 0, FrameCount - 1);

        foreach (var (track, layer) in tracks)
        {
            if (track.FrameCount > 0 && track._program.ControlCount == _program.ControlCount)
                yield return (track, layer.FrameFor(frame, FrameCount, track.FrameCount), layer.Loops);
        }
    }


    public Vector3 RigBonePosition(int bone) => RigBoneTransform(bone).Translation;

    public RigTransform RigBoneTransform(int bone)
    {
        var t = _pose[bone];
        if (ActiveAnchor is RigTransform anchor)
            t = RigTransform.Compose(anchor, t);

        return t;
    }

    private void ApplyHandConstraints(double frame, bool loop, RigTransform[] pose, SkinningFrames? layer, double layerFrame, float layerWeight)
    {
        List<HandIk.Source> sources = new() { new(HandConstraints, _clip, frame, loop, 1f) };
        if (UseTrackLayers)
        {
            foreach (var (track, trackFrame, loops) in TrackLayerFrames(frame))
                sources.Add(new(track.HandConstraints, track._clip, trackFrame, loops, 1f));
        }

        if (layer != null && layerWeight > 0f && layer._program.ControlCount == _program.ControlCount)
            sources.Add(new(layer.HandConstraints, layer._clip, layerFrame, loop, layerWeight));

        HandIk.Apply(_program, pose, sources, WeaponPoints, WeaponSocket);
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

    public static SkinningFrames BuildWeapon(Animation animation, EntityRuntimeRig rig, EntitySkeleton rigSkeleton, EntitySkeleton weaponSkeleton,
        IReadOnlyList<S808081A9>? weaponPoints = null, double frameRate = 30)
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

        frames.WeaponSocket = rigSkeleton.BoneNodes[first].ParentNodeIndex;
        frames.WeaponPoints = HandIk.Points(weaponPoints, first, weaponBones.Count);

        frames.HandConstraints = animation.TryReadHandConstraints();

        frames.Sample(0, false, new float[frames.BoneCount * SkinningFrames.FloatsPerBone]);
        return frames;
    }
}
