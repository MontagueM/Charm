namespace Tiger.Schema.Investment;

public enum EmoteKind
{
    Simple,
    Looping,
    Multiplayer,
}

public class EmoteParticipant
{
    public Animation.Animation? Idle;
    public Animation.Animation? Loop;
    public MapTransform Transform;
}

public class EmoteAnimations
{
    public EmoteKind Kind;
    public DestinyGenderDefinition Gender = DestinyGenderDefinition.None;
    public Animation.Animation? Intro;
    public Animation.Animation? Loop;
    public Animation.Animation? Outro;
    public List<EmoteParticipant> Participants = new();

    public static bool IsValid(Animation.Animation? animation) => animation != null && animation.Hash.IsValid();

    public bool HasIntroLoopOutro => Kind == EmoteKind.Looping && IsValid(Loop);

    public List<Animation.Animation> GetAnimations()
    {
        List<Animation.Animation> anims = Kind switch
        {
            EmoteKind.Simple => new() { Loop },
            _ => new() { Intro, Loop, Outro },
        };

        foreach (var participant in Participants)
        {
            anims.Add(participant.Idle);
            anims.Add(participant.Loop);
        }

        return anims;
    }
}
