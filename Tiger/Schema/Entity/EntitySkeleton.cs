
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
        if (resource is D2Class_DE818080)
        {
            D2Class_DE818080 skelInfo = (D2Class_DE818080)_tag.Unk18.GetValue(reader);
            for (int i = 0; i < skelInfo.NodeHierarchy.Count; i++)
            {
                BoneNode node = new();
                node.ParentNodeIndex = skelInfo.NodeHierarchy[reader, i].ParentNodeIndex;
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
        else if (resource is D2Class_D6818080)
        {
            D2Class_D6818080 skelInfo = (D2Class_D6818080)_tag.Unk18.GetValue(reader);
            for (int i = 0; i < skelInfo.NodeHierarchy.Count; i++)
            {
                BoneNode node = new();
                node.ParentNodeIndex = skelInfo.NodeHierarchy[reader, i].ParentNodeIndex;
                node.Hash = skelInfo.NodeHierarchy[reader, i].NodeHash;
                node.DefaultInverseObjectSpaceTransform = new ObjectSpaceTransform
                {
                    QuaternionRotation = skelInfo.DefaultInverseObjectSpaceTransforms[reader, i].Rotation,
                    Translation = skelInfo.DefaultInverseObjectSpaceTransforms[reader, i].Translation.ToVec3(),
                    Scale = skelInfo.DefaultInverseObjectSpaceTransforms[reader, i].Translation.W
                };
                // no DOST, so calculate inverse DIOST
                Vector4 inverseRotation = skelInfo.DefaultInverseObjectSpaceTransforms[reader, i].Rotation;
                inverseRotation.W = -inverseRotation.W;

                Vector4 inverseTranslation = skelInfo.DefaultInverseObjectSpaceTransforms[reader, i].Translation;
                inverseTranslation = Vector4.QuaternionMultiply(inverseRotation, inverseTranslation);
                inverseTranslation = Vector4.QuaternionMultiply(inverseTranslation, skelInfo.DefaultInverseObjectSpaceTransforms[reader, i].Rotation);
                node.DefaultObjectSpaceTransform = new ObjectSpaceTransform
                {
                    QuaternionRotation = inverseRotation,
                    Translation = inverseTranslation.ToVec3(),
                    Scale = skelInfo.DefaultInverseObjectSpaceTransforms[reader, i].Translation.W
                };
                nodes.Add(node);
            }
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
    public int ParentNodeIndex;
    public TigerHash Hash;
    public FbxNode Node;
}
