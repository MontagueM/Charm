using Tiger.Schema.Entity;

namespace Tiger.Schema.Animation;

using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

public struct FrameReference
{
    public SpaceDef Space;
    public SpaceDef Nested;
    public float AimWeight;
    public short AimTarget;
    public sbyte AimSource;
}

public class RigProgram
{
    private const int MaxChainDepth = 16;
    private const float MaximumReach = 0.98f;
    private const float MinimumReach = 1.02f;
    private const float MinimumSineSquared = 1e-8f;

    public int BoneCount;
    public int ControlCount;

    public Vector3 AimDirection = Vector3.UnitX;
    public Vector3 LookDirection = Vector3.UnitX;

    private RigTransform[] _bind;
    private RigTransform[] _localBind;
    private int[] _parents;
    private int[] _firstChild;
    private int[] _nextSibling;
    private bool[] _controlled;
    private S80808C56[] _controls;
    private RigTransform[] _defaults;
    private int[] _ikControls;
    private uint[] _groupHashes;
    private int[] _groupCounts;
    private List<(HashSet<uint>, float)> _retargets;

    private RigTransform[] _original;
    private HashSet<int> _chain = new();
    private Stack<int> _pending = new();

    public RigProgram(EntityRuntimeRig rig, EntitySkeleton skeleton)
    {
        rig.LoadRig();
        var bones = skeleton.BoneNodes;
        BoneCount = bones.Count;
        ControlCount = rig.Constraints.Count;

        _bind = new RigTransform[BoneCount];
        _localBind = new RigTransform[BoneCount];
        _parents = new int[BoneCount];
        _firstChild = new int[BoneCount];
        _nextSibling = new int[BoneCount];
        for (int i = 0; i < BoneCount; i++)
        {
            _bind[i] = RigTransform.FromObjectSpace(bones[i].DefaultObjectSpaceTransform);
            _parents[i] = bones[i].ParentNodeIndex;
            _firstChild[i] = bones[i].FirstChildNodeIndex;
            _nextSibling[i] = bones[i].NextSiblingNodeIndex;

            if (_parents[i] >= i)
                throw new Exception($"Bone {i} has parent {_parents[i]}, which comes later in the skeleton");
        }

        for (int i = 0; i < BoneCount; i++)
        {
            int parent = _parents[i];
            _localBind[i] = parent < 0 ? _bind[i] : RigTransform.Compose(_bind[parent].Inverse(), _bind[i]);
        }

        if (rig.ControlDefaults.Count != ControlCount)
            throw new Exception($"Rig has {ControlCount} controls but {rig.ControlDefaults.Count} default values");

        _controls = rig.Constraints.ToArray();
        _defaults = new RigTransform[ControlCount];
        _controlled = new bool[BoneCount];
        for (int i = 0; i < ControlCount; i++)
        {
            var control = _controls[i];
            if (control.BoneIndex < 0 || control.BoneIndex >= BoneCount)
                throw new Exception($"Control {i} drives bone {control.BoneIndex}, outside the {BoneCount} bone skeleton");

            _controlled[control.BoneIndex] = true;

            var value = rig.ControlDefaults[i];
            var rotation = new Quaternion(value.Rotation.X, value.Rotation.Y, value.Rotation.Z, value.Rotation.W);
            var translation = new Vector3(value.Translation.X, value.Translation.Y, value.Translation.Z);
            var scale = value.Translation.W > 1e-4f ? value.Translation.W : 1f;
            _defaults[i] = new RigTransform(Quaternion.Normalize(rotation), translation, scale);
        }

        _ikControls = rig.IkControls.Select(x => (int)x).ToArray();
        if (_ikControls.Any(x => x < 0 || x >= ControlCount) || _ikControls.Distinct().Count() != _ikControls.Length)
            throw new Exception("Rig IK list has invalid control indices");

        _groupHashes = rig.Components.Select(x => x.Unk00.Hash32).ToArray();
        _groupCounts = rig.Components.Select(x => x.Unk04).ToArray();

        if (_groupCounts.Any(x => x < 0) || _groupCounts.Sum() != ControlCount)
            throw new Exception("Rig control groups dont add up to its control count");

        _retargets = rig.RetargetProfiles;
        _original = new RigTransform[BoneCount];
    }

    public void CopyDefaults(RigTransform[] values) => Array.Copy(_defaults, values, ControlCount);

    public void WriteBind(RigTransform[] output) => Array.Copy(_bind, output, BoneCount);

    #region Clip binding
    public int CompatibleControlCount(Animation clip)
    {
        int count = 0;
        int groups = Math.Min(clip.RigComponents.Count, _groupHashes.Length);
        for (int i = 0; i < groups; i++)
        {
            if (clip.RigComponents[i].Unk00.Hash32 != _groupHashes[i])
                break;

            count += clip.RigComponents[i].Unk04;
        }

        return Math.Min(count, ControlCount);
    }

    public int[] ChannelMap(Animation clip)
    {
        var lead = CompatibleControlCount(clip);
        var map = Enumerable.Range(0, clip.RigControlCount).Select(x => x < lead ? x : -1).ToArray();

        int groups = 0;
        while (groups < Math.Min(clip.RigComponents.Count, _groupHashes.Length) && clip.RigComponents[groups].Unk00.Hash32 == _groupHashes[groups])
            groups++;

        var trailing = clip.RigComponents.Skip(groups).Where(x => x.Unk04 != 0).ToList();
        var names = _groupHashes.ToHashSet();

        if (lead == 0 || trailing.Count == 0 || trailing.Any(x => names.Contains(x.Unk00.Hash32)) || trailing.Sum(x => x.Unk04) != ControlCount - lead)
            return map;

        var remaining = Enumerable.Range(lead, ControlCount - lead).OrderBy(x => _controls[x].BoneIndex).ThenBy(x => x).ToArray();
        for (int i = 0; i < remaining.Length && lead + i < map.Length; i++)
        {
            map[lead + i] = remaining[i];
        }

        return map;
    }

    public float TranslationScale(Animation clip)
    {
        foreach (var items in _retargets)
        {
            var keys = items.Item1;
            var scale = items.Item2;
            if (clip.RetargetKeys.Any(keys.Contains))
                return scale > 0 && float.IsFinite(scale) ? scale : 1f;
        }

        return 1f;
    }
    #endregion

    #region Evaluation
    public void Evaluate(RigTransform[] values, RigTransform[] output)
    {
        Array.Copy(_bind, output, BoneCount);

        for (int i = 0; i < ControlCount; i++)
        {
            var control = _controls[i];
            var value = values[i];
            var bone = control.BoneIndex;
            var reference = RotationReference(control);
            var frame = Frame(reference, bone, output);

            switch (control.ConstraintType)
            {
                case ConstraintType.PARENT:
                    output[bone] = RigTransform.Compose(frame, value);
                    break;

                case ConstraintType.POINT_AND_ORIENT:
                    var rotation = Quaternion.Normalize(frame.Rotation * value.Rotation);
                    var translation = Translation(TranslationReference(control), bone, output) + value.Translation;
                    output[bone] = new RigTransform(rotation, translation, 1);
                    break;

                default:
                    throw new NotImplementedException($"Constraint type {control.ConstraintType} not implemented");
            }

            switch (control.IkFunctionType)
            {
                case IkFunctionType.IK_3:
                    int parent = _parents[bone];
                    if (parent < 0)
                        throw new Exception($"Control {i} has an IK chain but bone {bone} has no parent");

                    if (_parents[parent] >= 0)
                        output[parent] = RigTransform.Compose(output[_parents[parent]], _localBind[parent]);
                    break;

                case IkFunctionType.RFK:
                    DistributeChain(control, output);
                    break;

                case IkFunctionType.NONE:
                    break;

                default:
                    throw new NotImplementedException($"IK function type {control.IkFunctionType} not implemented");
            }
        }

        for (int i = 0; i < BoneCount; i++)
        {
            if (!_controlled[i] && _parents[i] >= 0)
                output[i] = RigTransform.Compose(output[_parents[i]], _localBind[i]);
        }

        SolveIk(output);
    }

    private static FrameReference RotationReference(S80808C56 control)
    {
        return new FrameReference
        {
            Space = control.RotationSpace0,
            Nested = control.RotationSpace1,
            AimWeight = control.AimWeight,
            AimTarget = control.AimTarget,
            AimSource = control.AimSource
        };
    }

    private static FrameReference TranslationReference(S80808C56 control)
    {
        return new FrameReference
        {
            Space = control.TranslationSpace0,
            Nested = control.TranslationSpace1,
            AimWeight = control.AimWeight2,
            AimTarget = control.AimTarget2,
            AimSource = control.AimSource2
        };
    }

    private static RigTransform Current(int bone, RigTransform[] output) => bone < 0 ? RigTransform.Identity : output[bone];

    private RigTransform Frame(FrameReference reference, int target, RigTransform[] output)
    {
        if (reference.Space.Type != SpaceType.AIM)
            return SpaceFrame(reference.Space, target, output);

        return new RigTransform(AimRotation(reference, target, output), SpaceTranslation(reference.Nested, target, output), 1);
    }

    private RigTransform SpaceFrame(SpaceDef space, int target, RigTransform[] output)
    {
        int bone = space.BoneIndex0;
        switch (space.Type)
        {
            case SpaceType.RELATIVE:
            case SpaceType.UNK_3:
                return Current(bone, output);

            case SpaceType.PARENT:
                if (bone < 0)
                    return _bind[target];

                return RigTransform.Compose(output[bone], RigTransform.Compose(_bind[bone].Inverse(), _bind[target]));

            case SpaceType.OBJECT:
                return output[0];

            case SpaceType.BIND:
                return RigTransform.Compose(output[0], _bind[target]);

            case SpaceType.AVERAGE:
                return RigTransform.Lerp(Current(bone, output), Current(space.BoneIndex1, output), 0.5f);

            default:
                throw new NotImplementedException($"Space type {space.Type} not implemented");
        }
    }

    private Vector3 Translation(FrameReference reference, int target, RigTransform[] output)
    {
        if (reference.Space.Type == SpaceType.AIM)
            return SpaceTranslation(reference.Nested, target, output);

        return SpaceTranslation(reference.Space, target, output);
    }

    private Vector3 SpaceTranslation(SpaceDef space, int target, RigTransform[] output)
    {
        if (space.Type != SpaceType.PARENT)
            return SpaceFrame(space, target, output).Translation;

        int bone = space.BoneIndex0;
        if (bone < 0)
            return _bind[target].Translation;

        var relative = RigTransform.Compose(_bind[bone].Inverse(), _bind[target]);
        var current = output[bone];
        return Vector3.Transform(relative.Translation, current.Rotation) + current.Translation;
    }

    private Quaternion AimRotation(FrameReference reference, int target, RigTransform[] output)
    {
        float weight = Math.Clamp(reference.AimWeight, 0, 1);

        Vector3 source;
        if (reference.AimSource == 0)
            source = AimDirection;
        else if (reference.AimSource == 1)
            source = LookDirection;
        else
            source = RigMath.NormalizeOrZero(Current(reference.AimTarget, output).Translation - output[target].Translation);

        var direction = RigMath.NormalizeOrZero(Vector3.UnitX + (source - Vector3.UnitX) * weight);
        return RigMath.FromTo(Vector3.UnitX, direction, Vector3.UnitZ);
    }

    private void DistributeChain(S80808C56 control, RigTransform[] output)
    {
        int start = control.IkChainStartBoneIndex;
        List<int> chain = new();

        int bone = _parents[control.BoneIndex];
        while (bone != start && bone >= 0)
        {
            if (chain.Count == MaxChainDepth)
                throw new Exception($"Chain of bone {control.BoneIndex} is deeper than {MaxChainDepth} bones");

            chain.Add(bone);
            bone = _parents[bone];
        }

        var relative = RigTransform.Compose(output[start].Inverse(), output[control.BoneIndex]);
        var inverseBind = _bind[start].Inverse();
        int step = 1;
        for (int i = chain.Count - 1; i >= 0; i--)
        {
            float amount = (float)step / (chain.Count + 1);
            var relativeBind = RigTransform.Compose(inverseBind, _bind[chain[i]]);
            var distributed = RigTransform.Lerp(RigTransform.Identity, relative, amount);
            output[chain[i]] = RigTransform.Compose(output[start], RigTransform.Lerp(relativeBind, distributed, amount));
            step++;
        }
    }
    #endregion

    #region IK
    private void SolveIk(RigTransform[] output)
    {
        if (_ikControls.Length == 0)
            return;

        Array.Copy(output, _original, BoneCount);
        foreach (int index in _ikControls)
        {
            int end = _controls[index].BoneIndex;
            int middle = _parents[end];
            int root = middle < 0 ? -1 : _parents[middle];

            RigTransform target = _original[end];
            output[middle] = RigTransform.Compose(_original[root], _localBind[middle]);
            output[end] = RigTransform.Compose(output[middle], _localBind[end]);

            RigTransform rootValue = output[root];
            SolveInPlane(target, ref rootValue, ref output[middle], ref output[end]);
            if (root != 0)
                output[root] = rootValue;

            _chain.Clear();
            _chain.Add(root);
            _chain.Add(middle);
            _chain.Add(end);
            foreach (int joint in _chain)
            {
                RigTransform delta = RigTransform.Compose(output[joint], _original[joint].Inverse());
                CarryChildren(joint, delta, output);
            }
        }
    }

    private void CarryChildren(int joint, RigTransform delta, RigTransform[] output)
    {
        _pending.Clear();
        if (_firstChild[joint] >= 0)
            _pending.Push(_firstChild[joint]);

        while (_pending.Count > 0)
        {
            int bone = _pending.Pop();

            if (_nextSibling[bone] >= 0)
                _pending.Push(_nextSibling[bone]);

            if (_chain.Contains(bone))
                continue;

            output[bone] = RigTransform.Compose(delta, _original[bone]);

            if (_firstChild[bone] >= 0)
                _pending.Push(_firstChild[bone]);
        }
    }

    private static void SolveInPlane(RigTransform target, ref RigTransform root, ref RigTransform middle, ref RigTransform end)
    {
        var start = root.Translation;
        var upper = middle.Translation - start;
        var lower = end.Translation - middle.Translation;
        float upperLength = upper.Length();
        float lowerLength = lower.Length();
        if (upperLength < 0.0001f || lowerLength < 0.0001f)
            return;

        var rootAim = AimFrame(root.Rotation, upper);
        var middleAim = AimFrame(middle.Rotation, lower);
        var aim = target.Translation - start;

        var maximum = MaximumReach * (upperLength + lowerLength);
        var minimum = MathF.Min(maximum, MinimumReach * MathF.Abs(upperLength - lowerLength));
        var distance = Math.Clamp(aim.Length(), minimum, maximum);
        var wrist = start + RigMath.NormalizeOrZero(aim) * distance;

        var denominator = 2 * upperLength * distance;
        var cosine = denominator <= 0.0001f ? 0 : (upperLength * upperLength + distance * distance - lowerLength * lowerLength) / denominator;
        var sine = MathF.Sqrt(MathF.Max(1 - cosine * cosine, MinimumSineSquared));

        var plane = RigMath.LookRotation(aim, upper, Vector3.UnitY);
        var elbow = start + Vector3.Transform(new Vector3(upperLength * cosine, upperLength * sine, 0), plane);

        root = new RigTransform(Reaim(rootAim, root.Rotation, elbow - start), start, 1);
        middle = new RigTransform(Reaim(middleAim, middle.Rotation, wrist - elbow), elbow, 1);
        end = new RigTransform(target.Rotation, wrist, 1);
    }

    private static Quaternion AimFrame(Quaternion bone, Vector3 segment)
    {
        var axis = RigMath.NormalizeOrZero(Vector3.Transform(Vector3.UnitX, bone));
        if (Vector3.Dot(RigMath.NormalizeOrZero(segment), axis) >= 1)
            return bone;

        return RigMath.LookRotation(segment, Vector3.Transform(Vector3.UnitZ, bone), Vector3.UnitZ);
    }

    private static Quaternion Reaim(Quaternion aim, Quaternion bone, Vector3 direction)
    {
        var turned = RigMath.LookRotation(direction, Vector3.Transform(Vector3.UnitZ, aim), Vector3.UnitZ);
        return Quaternion.Normalize(turned * Quaternion.Conjugate(aim) * bone);
    }
    #endregion
}
