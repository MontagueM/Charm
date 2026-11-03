using Tiger.Schema.Entity;

namespace Tiger.Schema.Animation;

using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

public static class HandIk
{
    public readonly record struct Source(HandConstraintTracks? Constraints, ClipChannels Clip, double Frame, bool Loop, float Weight);

    public static Dictionary<uint, (int Bone, RigTransform Point)> Points(IReadOnlyList<S808081A9>? weaponPoints, int firstBone, int weaponBoneCount)
    {
        Dictionary<uint, (int Bone, RigTransform Point)> points = new();
        foreach (S808081A9 point in weaponPoints ?? Array.Empty<S808081A9>())
        {
            if (point.Bone < 0 || point.Bone >= weaponBoneCount)
                continue;

            var rotation = new Quaternion(point.Rotation.X, point.Rotation.Y, point.Rotation.Z, point.Rotation.W);
            var translation = new Vector3(point.Translation.X, point.Translation.Y, point.Translation.Z);
            points.TryAdd(point.Name.Hash32, (firstBone + point.Bone, new RigTransform(Quaternion.Normalize(rotation), translation, 1)));
        }

        return points;
    }

    public static void Apply(RigProgram program, RigTransform[] pose, IReadOnlyList<Source> sources, IReadOnlyDictionary<uint, (int Bone, RigTransform Point)> points, int keep)
    {
        SortedDictionary<int, (RigTransform Target, float Weight, bool Used)> hands = new();
        foreach (var source in sources)
        {
            if (source.Constraints is not { } constraints)
                continue;

            bool additive = source.Clip.Mode == LayerType.Additive;
            foreach (var constraint in constraints.Constraints)
            {
                int end = program.ControlBone(constraint.EndControl);
                if (end < 0 || !source.Clip.HasChannel(constraint.EndControl))
                    continue;

                var (offset, constraintWeight) = constraints.Sample(constraint, source.Frame, source.Loop);
                (RigTransform Target, float Weight, bool Used) hand = hands.TryGetValue(end, out var known) ? known : (pose[end], 1f, false);

                if (additive)
                {
                    // the offset is a delta to the hand a base constraint placed, without one the clip already has the hand where it goes
                    if (!hand.Used)
                        continue;

                    hand.Target = RigTransform.Compose(hand.Target, RigTransform.Lerp(RigTransform.Identity, offset, source.Weight));
                }
                else
                {
                    if (!points.TryGetValue(constraint.PointName, out var named))
                        continue;

                    var target = RigTransform.Compose(RigTransform.Compose(pose[named.Bone], named.Point), offset);
                    hand.Target = RigTransform.Lerp(hand.Target, target, source.Weight);
                }

                hand.Weight = float.Lerp(hand.Weight, constraintWeight, source.Weight);
                hand.Used = true;
                hands[end] = hand;
            }
        }

        foreach (var (end, hand) in hands)
        {
            if (!hand.Used || hand.Weight <= 0f)
                continue;

            var current = pose[end];
            var blended = new RigTransform(
                Quaternion.Normalize(Quaternion.Slerp(current.Rotation, hand.Target.Rotation, hand.Weight)),
                Vector3.Lerp(current.Translation, hand.Target.Translation, hand.Weight),
                current.Scale);

            program.SolveChain(end, blended, pose, keep);
        }
    }
}

public sealed record HandIkSetup(IReadOnlyDictionary<uint, (int Bone, RigTransform Point)> Points, int WeaponSocket)
{
    public static HandIkSetup? Create(EntitySkeleton rigSkeleton, EntitySkeleton weaponSkeleton, IReadOnlyList<S808081A9>? weaponPoints)
    {
        if (weaponPoints == null || weaponPoints.Count == 0)
            return null;

        int first = AnimationSkinning.WeaponFirstBone(rigSkeleton, weaponSkeleton);
        var points = HandIk.Points(weaponPoints, first, weaponSkeleton.BoneNodes.Count);
        return points.Count == 0 ? null : new HandIkSetup(points, rigSkeleton.BoneNodes[first].ParentNodeIndex);
    }
}
