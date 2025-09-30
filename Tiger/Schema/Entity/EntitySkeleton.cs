
using Internal.Fbx;

namespace Tiger.Schema.Entity;

public class EntitySkeleton : EntityResource
{
    public EntitySkeleton(FileHash resource) : base(resource)
    {
    }

    public List<BoneNode> GetBoneNodes()
    {
        using TigerReader reader = GetReader();
        var nodes = new List<BoneNode>();

        dynamic? resource = _tag.Unk18.GetValue(reader);
        if (resource is SDE818080 skelInfo)
        {
            for (int i = 0; i < skelInfo.NodeHierarchy.Count; i++)
            {
                BoneNode node = new();
                node.ParentNodeIndex = skelInfo.NodeHierarchy[reader, i].ParentNodeIndex;
                node.Index = i;
                node.Hash = skelInfo.NodeHierarchy[reader, i].NodeHash;
                node.DefaultObjectSpaceTransform = new ObjectSpaceTransform
                {
                    QuaternionRotation = skelInfo.DefaultObjectSpaceTransforms[reader, i].Rotation,
                    Translation = skelInfo.DefaultObjectSpaceTransforms[reader, i].Translation.ToVec3(),
                    Scale = skelInfo.DefaultObjectSpaceTransforms[reader, i].Translation.W
                };
                node.DefaultInverseObjectSpaceTransform = new ObjectSpaceTransform
                {
                    QuaternionRotation = skelInfo.DefaultInverseObjectSpaceTransforms[reader, i].Rotation,
                    Translation = skelInfo.DefaultInverseObjectSpaceTransforms[reader, i].Translation.ToVec3(),
                    Scale = skelInfo.DefaultInverseObjectSpaceTransforms[reader, i].Translation.W
                };
                nodes.Add(node);
            }
        }
        else if (resource is SD6818080 weirdSkeleInfo)
        {
            for (int i = 0; i < weirdSkeleInfo.NodeHierarchy.Count; i++)
            {
                BoneNode node = new();
                node.Index = i;
                node.ParentNodeIndex = weirdSkeleInfo.NodeHierarchy[reader, i].ParentNodeIndex;
                node.Hash = weirdSkeleInfo.NodeHierarchy[reader, i].NodeHash;
                node.DefaultInverseObjectSpaceTransform = new ObjectSpaceTransform
                {
                    QuaternionRotation = weirdSkeleInfo.DefaultInverseObjectSpaceTransforms[reader, i].Rotation,
                    Translation = weirdSkeleInfo.DefaultInverseObjectSpaceTransforms[reader, i].Translation.ToVec3(),
                    Scale = weirdSkeleInfo.DefaultInverseObjectSpaceTransforms[reader, i].Translation.W
                };
                // no DOST, so calculate inverse DIOST
                Vector4 inverseRotation = weirdSkeleInfo.DefaultInverseObjectSpaceTransforms[reader, i].Rotation;
                inverseRotation.W = -inverseRotation.W;

                Vector4 inverseTranslation = weirdSkeleInfo.DefaultInverseObjectSpaceTransforms[reader, i].Translation;
                inverseTranslation = Vector4.QuaternionMultiply(inverseRotation, inverseTranslation);
                inverseTranslation = Vector4.QuaternionMultiply(inverseTranslation, weirdSkeleInfo.DefaultInverseObjectSpaceTransforms[reader, i].Rotation);
                node.DefaultObjectSpaceTransform = new ObjectSpaceTransform
                {
                    QuaternionRotation = inverseRotation,
                    Translation = inverseTranslation.ToVec3(),
                    Scale = weirdSkeleInfo.DefaultInverseObjectSpaceTransforms[reader, i].Translation.W
                };
                nodes.Add(node);
            }
        }

        foreach (var node in nodes)
        {
            //Console.WriteLine($"{node.Hash}: Index {node.Index}, Parent {node.ParentNodeIndex}");
            //Console.WriteLine($"\t{node.DefaultObjectSpaceTransform.Translation} : {node.DefaultObjectSpaceTransform.QuaternionRotation}");
        }

        return nodes;
    }
}

public struct ObjectSpaceTransform
{
    public Vector4 QuaternionRotation;
    public Vector3 Translation;
    public float Scale;
}
public struct BoneNode
{
    public ObjectSpaceTransform DefaultObjectSpaceTransform;
    public ObjectSpaceTransform DefaultInverseObjectSpaceTransform;
    public int Index;
    public int ParentNodeIndex;
    public TigerHash Hash;
    public FbxNode Node;
}
