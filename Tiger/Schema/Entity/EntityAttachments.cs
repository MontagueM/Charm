namespace Tiger.Schema.Entity;

public class EntityAttachments : EntityComponent
{
    public EntityAttachments(FileHash resource) : base(resource)
    {
    }

    //public List<AttachmentNode> GetAttachmentNodes()
    //{
    //    using TigerReader reader = GetReader();
    //    var nodes = new List<AttachmentNode>();

    //    if (_tag.Unk18.GetValue(reader) is D2Class_9D818080 attachments)
    //    {
    //        for (int i = 0; i < attachments.Attachments.Count; i++)
    //        {
    //            AttachmentNode node = new();
    //            var attachment = attachments.Attachments[reader, i];
    //            node.Hash = attachment.AttachmentName;
    //            node.ParentNodeIndex = attachment.Unk24;
    //            node.DefaultObjectSpaceTransform.Translation = attachment.Location.ToVec3();
    //            node.DefaultObjectSpaceTransform.QuaternionRotation = attachment.Rotation;
    //            node.DefaultObjectSpaceTransform.Scale = attachment.Location.W;
    //            nodes.Add(node);

    //            Console.WriteLine($"{Hash}: Attachment node {node.Hash}");
    //        }
    //    }
    //    return nodes;
    //}

}

//public struct AttachmentNode
//{
//    public ObjectSpaceTransform DefaultObjectSpaceTransform;
//    public int ParentNodeIndex;
//    public TigerHash Hash;
//    public FbxNode Node;
//}
