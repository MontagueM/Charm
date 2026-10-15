namespace Tiger.Schema.Entity;

public class EntityPhysicsModelParent : EntityComponent
{
    private EntityMaterialVariants _materialVariants;
    public EntityMaterialVariants MaterialVariants
    {
        get
        {
            if (_materialVariants is not null)
                return _materialVariants;

            using TigerReader reader = GetReader();
            _materialVariants = new EntityMaterialVariants(reader, Reader);
            return _materialVariants;
        }
    }

    public EntityPhysicsModelParent(FileHash resource) : base(resource)
    {
    }

    public S80806D6C Reader => ((S80806D6C)TagData.Unk18.GetValue(GetReader()));

    public EntityModel GetModel()
    {
        return Reader.PhysicsModel;
    }

    public AABB GetBoundingBox()
    {
        return Reader.BoundingBox;
    }
}
