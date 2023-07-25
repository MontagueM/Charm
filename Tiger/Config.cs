using System.Reflection;
using Newtonsoft.Json;

namespace Tiger;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigFieldAttribute : Attribute
{
    public string FieldName { get; private set; }

    public ConfigFieldAttribute(string fieldName)
    {
        FieldName = fieldName;
    }
}

// public class ConfigSubsystem : Subsystem
// {
//     private string _configFilePath = "./config.json";
//
//     public ConfigSubsystem()
//     {
//     }
//
//     public ConfigSubsystem(string overrideConfigFilePath)
//     {
//         _configFilePath = overrideConfigFilePath;
//     }
//
//     public void SetConfigFilePath(string configPath)
//     {
//         _configFilePath = configPath;
//         Initialise();
//     }
//
//     public string GetConfigFilePath()
//     {
//         return _configFilePath;
//     }
//
//     protected internal override bool Initialise()
//     {
//         if (CharmInstance.Args.GetArgValue("config", out string configPath))
//         {
//             _configFilePath = configPath;
//         }
//
//         if (ConfigFileExists())
//         {
//             bool successfullyLoadedConfig = LoadConfig();
//             if (successfullyLoadedConfig)
//             {
//                 return true;
//             }
//         }
//
//         return WriteConfig();
//     }
//
//     private bool ConfigFileExists()
//     {
//         return File.Exists(_configFilePath);
//     }
//
//     private bool LoadConfig()
//     {
//         Dictionary<string, dynamic?>? deserializedSettings;
//         try
//         {
//             deserializedSettings =
//                 JsonConvert.DeserializeObject<Dictionary<string, dynamic?>>(File.ReadAllText(_configFilePath));
//         }
//         catch (JsonSerializationException e)
//         {
//             throw new JsonSerializationException($"Failed to load config file {_configFilePath}: {e.Message}", e);
//         }
//         catch (JsonReaderException e)
//         {
//             throw new JsonReaderException($"Failed to load config file {_configFilePath}: {e.Message}", e);
//         }
//
//         if (deserializedSettings is null)
//         {
//             return false;
//         }
//
//         bool configIsMissingField = false;
//         foreach (var field in GetConfigFields())
//         {
//             configIsMissingField |= SetFieldValueFromConfig(field.Key, field.Value, deserializedSettings);
//         }
//
//         if (configIsMissingField)
//         {
//             return WriteConfig();
//         }
//
//         return true;
//     }
//
//     private Dictionary<string, FieldInfo> GetConfigFields()
//     {
//         var fields = AppDomain.CurrentDomain.GetAssemblies()
//             .SelectMany(a => a.GetTypes())
//             .SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
//             .Where(f => Attribute.IsDefined(f, typeof(ConfigFieldAttribute)));
//
//         // Display warnings about fields that are 1. not static or 2. readonly
//         foreach (var field in fields)
//         {
//             if (!field.IsStatic)
//             {
//                 Console.WriteLine($"WARNING: Config field {field.Name} is not static. It will not be loaded from the config.");
//             }
//             else if (field.IsInitOnly)
//             {
//                 Console.WriteLine($"WARNING: Config field {field.Name} is readonly. It will not be loaded from the config.");
//             }
//         }
//
//         // Strip out fields that are not static or readonly
//         fields = fields.Where(f => f.IsStatic && !f.IsInitOnly);
//
//         return GetConfigFieldsDictionary(fields);
//     }
//
//     private Dictionary<string, FieldInfo> GetConfigFieldsDictionary(IEnumerable<FieldInfo> fields)
//     {
//         var configFields = new Dictionary<string, FieldInfo>();
//         foreach (var field in fields)
//         {
//             string fieldName = GetFieldName(field);
//             if (configFields.ContainsKey(fieldName))
//             {
//                 throw new Exception($"Config field {fieldName} is defined multiple times.");
//             }
//             configFields[fieldName] = field;
//         }
//
//         return configFields;
//     }
//
//     private bool SetFieldValueFromConfig(string fieldName, FieldInfo field, Dictionary<string, dynamic?> deserializedSettings)
//     {
//         if (deserializedSettings.TryGetValue(fieldName, out dynamic? configField))
//         {
//             if (configField is Newtonsoft.Json.Linq.JObject)
//             {
//                 field.SetValue(null, ((Newtonsoft.Json.Linq.JObject)configField).ToObject(field.FieldType));
//             }
//             else
//             {
//                 field.SetValue(null, ((Newtonsoft.Json.Linq.JValue)configField).ToObject(field.FieldType));
//             }
//
//             return true;
//         }
//
//         return false;
//     }
//
//     private string GetFieldName(FieldInfo field)
//     {
//         return ((ConfigFieldAttribute)field.GetCustomAttributes(typeof(ConfigFieldAttribute)).First()).FieldName;
//     }
//
//     private bool WriteConfig()
//     {
//         var fields = GetConfigFields();
//
//         var deserializedSettings = new Dictionary<string, dynamic?>();
//         foreach (var (fieldName, fieldInfo) in fields)
//         {
//             deserializedSettings[fieldName] = fieldInfo.GetValue(null);
//         }
//
//         string serializedSettings = JsonConvert.SerializeObject(deserializedSettings, Formatting.Indented);
//         File.WriteAllText(_configFilePath, serializedSettings);
//
//         return ConfigFileExists();
//     }
// }
