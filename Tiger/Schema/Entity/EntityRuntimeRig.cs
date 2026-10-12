namespace Tiger.Schema.Entity;


public class EntityRuntimeRig : EntityComponent
{
    public List<S80808C5F> Components;
    public List<S80808C56> Constraints;   // one per control
    public int ControlTransformCount;
    public List<S80809F4F> ControlDefaults = new();
    public List<short> IkControls = new();
    public List<short> BoneToControl;
    public List<short> ControlToBone;

    public List<(HashSet<uint>, float)> RetargetProfiles = new();

    public bool RigLoaded = false;
    public EntityRuntimeRig(FileHash resource) : base(resource)
    {
    }

    public void LoadRig()
    {
        if (RigLoaded)
            return;

        var definition = (S80808B5F)GetUnk18();
        Components = definition.RigComponents;
        Constraints = definition.ControlsRelations;
        ControlTransformCount = definition.ControlsTransforms.Count;
        ControlDefaults = definition.ControlsTransforms;
        IkControls = definition.IKControls.Select(x => x.Value).ToList();
        BoneToControl = definition.BoneToControl.Select(x => x.Value).ToList();
        ControlToBone = definition.ControlToBone.Select(x => x.Value).ToList();

        // not every rig has profiles
        if (definition.Profiles != null)
        {
            foreach (S80808C53 profile in definition.Profiles.TagData.Profiles)
            {
                var keys = profile.Names.Select(x => x.StringHash.Hash32).ToHashSet();
                RetargetProfiles.Add((keys, profile.TranslationScale));
            }
        }

        RigLoaded = true;
    }
}
