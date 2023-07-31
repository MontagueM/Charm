using System.Reflection;
using Arithmic;

namespace Tiger;

// Is alive for the entire duration of the program
public static class CharmInstance
{
    private static Dictionary<string, Subsystem> _subsystems = new();

    public static CharmArgs Args { get; set; } = new CharmArgs();

    public static bool HasSubsystem<T>() where T : Subsystem
    {
        return HasSubsystem(typeof(T));
    }

    public static bool HasSubsystem(Type type)
    {
        return _subsystems.ContainsKey(type.Name);
    }

    public static T GetSubsystem<T>() where T : Subsystem
    {
        return (T)GetSubsystem(typeof(T));
    }

    public static dynamic? GetSubsystem(Type type)
    {
        bool found = _subsystems.TryGetValue(type.Name, out Subsystem? subsystem);
        if (!found)
        {
            subsystem = CreateSubsystem(type);
            if (subsystem != null)
            {
                // todo reconcile subsystem initialisation
                // InitialiseSubsystem(subsystem);
                _subsystems.Add(type.Name, subsystem);
            }
        }
        return subsystem;
    }

    private static Subsystem? CreateSubsystem(Type type)
    {
        if (type.IsSubclassOf(typeof(Subsystem)))
        {
            return (Subsystem?)Activator.CreateInstance(type);
        }

        return null;
    }

    public static void InitialiseSubsystems()
    {
        _subsystems = GetAllSubsystems();
        Log.Info($"All subsystems found: {string.Join(", ", _subsystems.Keys)}");
        _subsystems.Values.ToList().ForEach(InitialiseSubsystem);
    }

    private static void InitialiseSubsystem(Subsystem subsystem)
    {
        Log.Info($"Initialising subsystem {subsystem.GetType().Name}");
        bool initialisedSuccess = subsystem.Initialise();
        if (!initialisedSuccess)
        {
            _subsystems.Remove(subsystem.GetType().Name);
            throw new Exception($"Failed to initialise subsystem {subsystem.GetType().Name}");
        }
        Log.Info($"Initialised subsystem {subsystem.GetType().Name}");
    }

    private static Dictionary<string, Subsystem> GetAllSubsystems()
    {
        var subsystems = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t => typeof(Subsystem).IsAssignableFrom(t) &&
!t.Attributes.HasFlag(TypeAttributes.Interface) && !t.IsAbstract);
        return subsystems.ToDictionary(type => type.Name, type => (Subsystem)GetSubsystem(type));
    }

    public static void ClearSubsystems()
    {
        _subsystems.Clear();
    }
}
