using Internal.Fbx;

namespace Tiger.Schema.Entity;

public class EntitySkeleton : EntityComponent
{
    private List<BoneNode>? _cachedBoneNodes;

    public EntitySkeleton(FileHash resource) : base(resource)
    {
    }

    public List<BoneNode> GetBoneNodes()
    {
        if (_cachedBoneNodes != null)
            return _cachedBoneNodes;

        using TigerReader reader = GetReader();
        var nodes = new List<BoneNode>();

        dynamic? resource = _tag.Unk18.GetValue(reader);
        if (resource is S808081DE skelInfo)
        {
            for (int i = 0; i < skelInfo.NodeHierarchy.Count; i++)
            {
                BoneNode node = new();
                node.Index = i;
                node.ParentNodeIndex = skelInfo.NodeHierarchy[reader, i].ParentNodeIndex;
                node.FirstChildNodeIndex = skelInfo.NodeHierarchy[reader, i].FirstChildNodeIndex;
                node.NextSiblingNodeIndex = skelInfo.NodeHierarchy[reader, i].NextSiblingNodeIndex;

                node.Hash = skelInfo.NodeHierarchy[reader, i].NodeHash;
                node.HashString = node.Hash.Reverse();
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
        else if (resource is S808081D6 weirdSkeleInfo)
        {
            for (int i = 0; i < weirdSkeleInfo.NodeHierarchy.Count; i++)
            {
                BoneNode node = new();
                node.Index = i;
                node.ParentNodeIndex = weirdSkeleInfo.NodeHierarchy[reader, i].ParentNodeIndex;
                node.FirstChildNodeIndex = weirdSkeleInfo.NodeHierarchy[reader, i].FirstChildNodeIndex;
                node.NextSiblingNodeIndex = weirdSkeleInfo.NodeHierarchy[reader, i].NextSiblingNodeIndex;

                node.Hash = weirdSkeleInfo.NodeHierarchy[reader, i].NodeHash;
                node.HashString = node.Hash.Reverse();
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

        _cachedBoneNodes = nodes;
        return _cachedBoneNodes;
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
    public int Index { get; set; }
    public int ParentNodeIndex;
    public int FirstChildNodeIndex;
    public int NextSiblingNodeIndex;
    public TigerHash Hash;
    public string HashString; // Gets set as flipped hash for compatibility with the blender importers rename bones function
    public FbxNode Node;
}

public struct ExportBoneNode
{
    public ObjectSpaceTransform DefaultObjectSpaceTransform { get; set; }
    public ObjectSpaceTransform DefaultInverseObjectSpaceTransform { get; set; }
    public int Index { get; set; }
    public int ParentNodeIndex { get; set; }
    public int FirstChildNodeIndex;
    public int NextSiblingNodeIndex;
    public string Hash { get; set; }
}
