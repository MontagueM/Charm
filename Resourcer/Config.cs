using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;

namespace Resourcer;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigPropertyAttribute : Attribute
{
    
}

public abstract class CharmSubsystem
{
    private static Dictionary<string, CharmSubsystem> _subsystems = new();

    public abstract bool Initialise();

    public static void InitialiseSubsystems()
    {
        _subsystems = GetAllSubsystems();
        _subsystems.Values.ToList().ForEach(s => s.Initialise());
    }
    
    public static T GetSubsystem<T>() where T : CharmSubsystem
    {
        _subsystems.TryGetValue(typeof(T).Name, out CharmSubsystem subsystem);
        return (T)subsystem;
    }

    private static Dictionary<string, CharmSubsystem> GetAllSubsystems()
    {
        var subsystems = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t => typeof(CharmSubsystem).IsAssignableFrom(t) &&
                        t.Attributes.HasFlag(TypeAttributes.Interface) == false && t.IsAbstract == false);
        return subsystems.ToDictionary(type => type.Name, type => (CharmSubsystem)Activator.CreateInstance(type));
    }
}

public class Config : CharmSubsystem
{
    private static readonly string configFileName = "config.json";
    
    // Defaults to directory of the executable
    public static string ConfigFileDirectory { get; set; } = "./";
    
    public static string ConfigFilePath => Path.Combine(ConfigFileDirectory, configFileName);

    public class FieldWatcher : INotifyPropertyChanged
    {
        private object targetObject;
        private FieldInfo fieldInfo;
        private Action<object, object> callback;

        public event PropertyChangedEventHandler PropertyChanged;

        public FieldWatcher(object targetObject, FieldInfo fieldInfo, Action<object, object> callback)
        {
            this.targetObject = targetObject;
            this.fieldInfo = fieldInfo;
            this.callback = callback;

            // Subscribe to the PropertyChanged event for the target object
            if (targetObject is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == fieldInfo.Name)
            {
                // Call the callback whenever the field is modified
                callback(targetObject, fieldInfo.GetValue(targetObject));

                // Raise the PropertyChanged event to notify any subscribers that the property has changed
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(fieldInfo.Name));
            }
        }
    }
    
    public override bool Initialise()
    {
        if (ConfigFileExists())
        {
            bool successfullyLoadedConfig = LoadConfig();
            if (successfullyLoadedConfig)
            {
                return true;
            }
        }

        return CreateNewConfig();
    }
    
    private static bool ConfigFileExists()
    {
        return File.Exists(ConfigFilePath);
    }

    private static bool LoadConfig()
    {
        Dictionary<string, string>? deserializedSettings = JsonConvert.DeserializeObject<Dictionary<string, string?>>(File.ReadAllText(ConfigFilePath));
        if (deserializedSettings is null)
        {
            return false;
        }

        var fields = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            .Where(f => Attribute.IsDefined(f, typeof(ConfigPropertyAttribute)));

        foreach (var field in fields)
        {
            // set field value to the value in the config
            if (deserializedSettings[field.Name] is { } configField)
            {
                // convert the object into the field type
                bool converted = TryConvertConfigStringToValue(configField, field.FieldType, out object? configValue);
                if (converted)
                {
                    field.SetValue(null, configValue);
                }
            }
        }
        
        return true;
    }

    private static bool CreateNewConfig()
    {
        var fields = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            .Where(f => Attribute.IsDefined(f, typeof(ConfigPropertyAttribute)));

        var deserializedSettings = new Dictionary<string, string>();
        foreach (var field in fields)
        {
            deserializedSettings[field.Name] = ConvertConfigValueToString(field.GetValue(null));
        }
        
        string serializedSettings = JsonConvert.SerializeObject(deserializedSettings, Formatting.Indented);
        File.WriteAllText(ConfigFilePath, serializedSettings);
        
        return ConfigFileExists();
    }

    private static string ConvertConfigValueToString(object? value)
    {
        if (value is null)
        {
            return "";
        }

        switch (value.GetType())
        {
            case {IsEnum: true}:
                return ConvertEnumToString((Enum)value);
        }

        return value.ToString();
    }
    
    private static bool TryConvertConfigStringToValue(string stringValue, Type outType, out object? value)
    {
        value = default;
        if (stringValue is null)
        {
            return false;
        }

        if (outType.IsEnum)
        {
            value = ConvertStringToEnum(stringValue, outType);
        }
        else
        {
            value = Convert.ChangeType(stringValue, outType);
        }

        return true;
    }
    
    public static string ConvertEnumToString<T>(T enumValue) where T : Enum
    {
        return enumValue.ToString();
    }
    
    public static object ConvertStringToEnum(string enumString, Type type)
    {
        return Enum.Parse(type, enumString);
    }
}

