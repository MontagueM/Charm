using System.Reflection;

namespace Resourcer;

// Is alive for the entire duration of the program
public class CharmInstance
{
    private static Dictionary<string, CharmSubsystem> _subsystems = new();

    public static bool HasSubsystem<T>() where T : CharmSubsystem
    {
        return HasSubsystem(typeof(T));
    }
    
    public static bool HasSubsystem(Type type)
    {
        return _subsystems.ContainsKey(type.Name);
    }
    
    public static T GetSubsystem<T>() where T : CharmSubsystem
    {
        return (T)GetSubsystem(typeof(T));
    }
    
    public static dynamic? GetSubsystem(Type type)
    {
        bool found = _subsystems.TryGetValue(type.Name, out CharmSubsystem? subsystem);
        if (!found)
        {
            subsystem = CreateSubsystem(type);
            if (subsystem != null)
            {
                InitialiseSubsystem(subsystem);
                _subsystems.Add(type.Name, subsystem);
            }
        }
        return subsystem;
    }

    private static CharmSubsystem? CreateSubsystem<T>() where T : CharmSubsystem
    {
        return CreateSubsystem(typeof(T));
    }
    
    private static CharmSubsystem? CreateSubsystem(Type type)
    {
        if (type.IsSubclassOf(typeof(CharmSubsystem)))
        {
            return (CharmSubsystem?) Activator.CreateInstance(type);
        }

        return null;
    }

    public static void InitialiseSubsystems()
    {
        _subsystems = GetAllSubsystems();
        _subsystems.Values.ToList().ForEach(InitialiseSubsystem);
    }
    
    private static void InitialiseSubsystem(CharmSubsystem subsystem)
    {
        bool initialisedSuccess = subsystem.Initialise();
        if (initialisedSuccess == false)
        {
            _subsystems.Remove(subsystem.GetType().Name);
            throw new Exception($"Failed to initialise subsystem {subsystem.GetType().Name}");
        }
    }

    private static Dictionary<string, CharmSubsystem> GetAllSubsystems()
    {
        var subsystems = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t => typeof(CharmSubsystem).IsAssignableFrom(t) &&
                        t.Attributes.HasFlag(TypeAttributes.Interface) == false && t.IsAbstract == false);
        return subsystems.ToDictionary(type => type.Name, type => (CharmSubsystem)GetSubsystem(type));
    }

    public static void ClearSubsystems()
    {
        _subsystems.Clear();
    }
}