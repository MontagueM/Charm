using System.Reflection;

namespace Tiger;

public abstract class Subsystem
{
    protected internal abstract bool Initialise();
}

public abstract class Subsystem<T> : Subsystem where T : Subsystem
{
    public static T Get()
    {
        return CharmInstance.GetSubsystem<T>();
    }
}
