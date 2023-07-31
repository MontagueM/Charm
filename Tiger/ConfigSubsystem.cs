using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Newtonsoft.Json;
// using MessageBox = System.Windows.Forms.MessageBox;
using Tiger;
using Tiger.Schema;

namespace Tiger;

// public class ConfigAttribute : Attribute
// {
//
// }
//
// [ConfigSubsystem]

public struct Settings
{
    public CommonSettings Common;
    public UnrealSettings Unreal;
    public BlenderSettings Blender;
    public Source2Settings Source2;
}

public class CommonSettings
{
    public Dictionary<TigerStrategy, string> PackagesPath { get; set; } = new Dictionary<TigerStrategy, string>();
    public TigerStrategy CurrentStrategy { get; set; } = TigerStrategy.NONE;
    public string ExportPath { get; set; } = "";
    public bool SingleFolderMapsEnabled { get; set; } = true;
    public bool IndividualStaticsEnabled { get; set; } = true;
    public TextureExportFormat OutputTextureFormat { get; set; } = TextureExportFormat.DDS_BGRA_UNCOMP_DX10;
}

// [ConfigSubsystem]
public class UnrealSettings
{
    public bool UnrealInteropEnabled { get; set; } = false;
    public string UnrealInteropPath { get; set; } = "";
}

// [ConfigSubsystem]
public class BlenderSettings
{
    public bool BlenderInteropEnabled { get; set; } = false;
}

// [ConfigSubsystem]
public class Source2Settings
{
    public bool Source2VShaderExportsEnabled { get; set; } = false;
    public bool Source2VMATExportsEnabled { get; set; } = false;
    public bool Source2VMDLExportsEnabled { get; set; } = false;
    public string Source2Path { get; set; } = "";
}

// class TypeExtensions
// {
//     public bool HasAttributeOfType<T>(this Type type) where T : Attribute
//     {
//         return type.GetCustomAttributes<T>(true).Any();
//     }
// }

public class ConfigSubsystem : Subsystem
{
    // private Configuration _config =
        // ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.ExecutablePath);

    // private Dictionary<Type, dynamic?> _settings = new();


    // protected override bool Initialise()
    // {
    //     // FillSettingsCache();
    //     return true;
    // }

    // private void FillSettingsCache()
    // {
    //     HashSet<Type> allSettings = AppDomain.CurrentDomain.GetAssemblies()
    //         .SelectMany(a => a.GetTypes())
    //         .Where(t => t.HasAttributeOfType<ConfigAttribute>())
    //         .ToHashSet();
    //
    //     foreach (Type settingType in allSettings)
    //     {
    //         dynamic? settings = Activator.CreateInstance(settingType);
    //         _settings.Common.Common.Add(settingType, settings);
    //     }
    // }

    // public T? GetSettings<T>() where T : struct
    // {
    //     if (_settings.Common.Common.TryGetValue(typeof(T), out dynamic? settings))
    //     {
    //         return (T) settings;
    //     }
    //
    //     return null;
    // }

    #region packagesPath

    public string GetPackagesPath(TigerStrategy strategy)
    {
        if (!_settings.Common.PackagesPath.ContainsKey(strategy))
        {
            return "";
        }

        return _settings.Common.PackagesPath[strategy];
    }

    public bool TrySetPackagePath(string path, TigerStrategy strategy)
    {
        if (path == "")
        {
            return false;
        }

        // Verify this is a valid path by checking to see if a .pkg file is inside
        string[] files = Directory.GetFiles(path, "*.pkg", SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
        {
            return false;
        }

        if (_settings.Common.PackagesPath.ContainsKey(strategy))
        {
            _settings.Common.PackagesPath.Remove(strategy);
        }
        _settings.Common.PackagesPath.Add(strategy, path);

        Save();
        return true;
    }

    #endregion

    #region strategy

    public void SetCurrentStrategy(TigerStrategy strategy)
    {
        _settings.Common.CurrentStrategy = strategy;
        Save();
    }

    public TigerStrategy GetCurrentStrategy()
    {
        return _settings.Common.CurrentStrategy;
    }

    private TigerStrategy FindEnumValueStrategy(string description)
    {
        for (int i = 0; i < typeof(TigerStrategy).GetFields().Length - 1; i++)
        {
            if (((TigerStrategy)i).ToString() == description)
            {
                return (TigerStrategy)i;
            }
        }

        return TigerStrategy.NONE;
    }

    #endregion

    #region source2Path

    public string GetSource2Path()
    {
        return _settings.Source2.Source2Path;
    }

    public bool TrySetSource2Path(string path)
    {
        if (path == "")
        {
            return false;
        }

        if (!path.EndsWith("win64"))
        {
            return false;
        }

        _settings.Source2.Source2Path = path;

        Save();
        return true;
    }

    #endregion

    #region source2ExportsEnabled

    public void SetS2ShaderExportEnabled(bool bS2ShaderExportEnabled)
    {
        _settings.Source2.Source2VShaderExportsEnabled = bS2ShaderExportEnabled;
        Save();
    }

    public bool GetS2ShaderExportEnabled()
    {
        return _settings.Source2.Source2VShaderExportsEnabled;
    }

    //
    public void SetS2VMATExportEnabled(bool bS2VMATExportEnabled)
    {
        _settings.Source2.Source2VMATExportsEnabled = bS2VMATExportEnabled;
        Save();
    }

    public bool GetS2VMATExportEnabled()
    {
        return _settings.Source2.Source2VMATExportsEnabled;
    }

    public void SetS2VMDLExportEnabled(bool bS2VMDLExportEnabled)
    {
        _settings.Source2.Source2VMDLExportsEnabled = bS2VMDLExportEnabled;
        Save();
    }

    public bool GetS2VMDLExportEnabled()
    {
        return _settings.Source2.Source2VMDLExportsEnabled;
    }

    #endregion


    #region exportSavePath

    public string GetExportSavePath()
    {
        return _settings.Common.ExportPath;
    }

    public bool TrySetExportSavePath(string path)
    {
        if (path == "")
        {
            return false;
        }

        _settings.Common.ExportPath = path;

        Save();
        return true;
    }

    #endregion

    #region unrealInteropPath

    public bool TrySetUnrealInteropPath(string interopPath)
    {
        if (!interopPath.Contains("Content"))
        {
            SetUnrealInteropEnabled(false);
            return false;
        }

        _settings.Unreal.UnrealInteropPath = interopPath;
        SetUnrealInteropEnabled(true);

        Save();
        return true;
    }

    public string GetUnrealInteropPath()
    {
        return _settings.Unreal.UnrealInteropPath;
    }

    #endregion

    #region unrealInteropEnabled

    public void SetUnrealInteropEnabled(bool bUnrealInteropEnabled)
    {
        _settings.Unreal.UnrealInteropEnabled = bUnrealInteropEnabled;
        Save();
    }

    public bool GetUnrealInteropEnabled()
    {
        return _settings.Unreal.UnrealInteropEnabled;
    }

    #endregion

    #region blenderInteropEnabled

    public void SetBlenderInteropEnabled(bool bBlenderInteropEnabled)
    {
        _settings.Blender.BlenderInteropEnabled = bBlenderInteropEnabled;
        Save();
    }

    public bool GetBlenderInteropEnabled()
    {
        return _settings.Blender.BlenderInteropEnabled;
    }

    #endregion

    #region singleFolderMapsEnabled

    public void SetSingleFolderMapsEnabled(bool bSingleFolderMapsEnabled)
    {
        _settings.Common.SingleFolderMapsEnabled = bSingleFolderMapsEnabled;
        Save();
    }

    public bool GetSingleFolderMapsEnabled()
    {
        return _settings.Common.SingleFolderMapsEnabled;
    }

    #endregion

    public void SetIndvidualStaticsEnabled(bool bIndvidualStaticsEnabled)
    {
        _settings.Common.IndividualStaticsEnabled = bIndvidualStaticsEnabled;
        Save();
    }

    public bool GetIndvidualStaticsEnabled()
    {
        return _settings.Common.IndividualStaticsEnabled;
    }

    #region outputTextureFormat

    public void SetOutputTextureFormat(TextureExportFormat outputTextureFormat)
    {
        _settings.Common.OutputTextureFormat = outputTextureFormat;
        Save();
    }

    public TextureExportFormat GetOutputTextureFormat()
    {
        return _settings.Common.OutputTextureFormat;
    }

    private TextureExportFormat FindEnumValue(string description)
    {
        for (int i = 0; i < typeof(TextureExportFormat).GetFields().Length - 1; i++)
        {
            if (((TextureExportFormat)i).ToString() == description)
            {
                return (TextureExportFormat)i;
            }
        }

        return TextureExportFormat.DDS_BGRA_UNCOMP_DX10;
    }

    #endregion

    private string _configFilePath = "./config.json";
    // private Dictionary<string, dynamic?> _settings;
    private Settings _settings;

    public ConfigSubsystem()
    {
    }

    public ConfigSubsystem(string overrideConfigFilePath)
    {
        _configFilePath = overrideConfigFilePath;
    }

    public void SetConfigFilePath(string configPath)
    {
        _configFilePath = configPath;
        Initialise();
    }

    public string GetConfigFilePath()
    {
        return _configFilePath;
    }

    private bool LoadConfig()
     {
         try
         {
             _settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_configFilePath));
         }
         catch (JsonSerializationException e)
         {
             throw new JsonSerializationException($"Failed to load config file {_configFilePath}: {e.Message}", e);
         }
         catch (JsonReaderException e)
         {
             throw new JsonReaderException($"Failed to load config file {_configFilePath}: {e.Message}", e);
         }

         // bool configIsMissingField = false;
         // foreach (var field in GetConfigFields())
         // {
         //     configIsMissingField |= SetFieldValueFromConfig(field.Key, field, deserializedSettings);
         // }
         //
         // if (configIsMissingField)
         // {
         //     return WriteConfig();
         // }
         if (_settings.Common == null)
         {
             _settings.Common = new CommonSettings();
             _settings.Blender = new BlenderSettings();
            _settings.Unreal = new UnrealSettings();
            _settings.Source2 = new Source2Settings();
            WriteConfig();
         }

         foreach ((TigerStrategy strategy, string packagesPath) in _settings.Common.PackagesPath)
         {
             Strategy.AddNewStrategy(strategy, packagesPath, false);
         }
         Strategy.SetStrategy(_settings.Common.CurrentStrategy);

         return true;
     }

    private bool WriteConfig()
     {
         string serializedSettings = JsonConvert.SerializeObject(_settings, Formatting.Indented);
         File.WriteAllText(_configFilePath, serializedSettings);

         return ConfigFileExists();
     }

    private bool ConfigFileExists()
     {
         return File.Exists(_configFilePath);
     }

    private void Save()
    {
        WriteConfig();
        // _config.Save(ConfigurationSaveMode.Modified);
        // ConfigurationManager.RefreshSection("appSettings");
    }

    protected internal override bool Initialise()
    {
        if (CharmInstance.Args.GetArgValue("config", out string configPath))
        {
             _configFilePath = configPath;
        }

        if (ConfigFileExists())
        {
            bool successfullyLoadedConfig = LoadConfig();
            if (successfullyLoadedConfig)
            {
             return true;
            }
        }
        else
        {
            WriteConfig();
            bool successfullyLoadedConfig = LoadConfig();
            if (successfullyLoadedConfig)
            {
                return true;
            }
        }

        return false;
    }
}
