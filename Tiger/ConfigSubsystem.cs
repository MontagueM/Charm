using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Arithmic;
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
    public BlenderSettings Blender;
    public SBoxSettings SBox;
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
public class BlenderSettings
{
    public bool BlenderInteropEnabled { get; set; } = false;
}

// [ConfigSubsystem]
public class SBoxSettings
{
    public bool SBoxShaderExportsEnabled { get; set; } = false;
    public bool SBoxMaterialExportsEnabled { get; set; } = false;
    public bool SBoxModelExportsEnabled { get; set; } = false;
    public string SBoxToolsPath { get; set; } = "";
    public string SBoxContentPath { get; set; } = "";
}

public class ConfigSubsystem : Subsystem<ConfigSubsystem>
{

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

#pragma warning disable S1144 // Unused private types or members should be removed
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

    #region S&Box

    public string GetSBoxToolsPath()
    {
        return _settings.SBox.SBoxToolsPath;
    }

    public bool TrySetSBoxToolsPath(string path)
    {
        if (path == "")
            return false;

        if (!path.EndsWith("win64"))
            return false;

        _settings.SBox.SBoxToolsPath = path;

        Save();
        return true;
    }

    public string GetSBoxContentPath()
    {
        return _settings.SBox.SBoxContentPath;
    }

    public bool TrySetSBoxContentPath(string path)
    {
        if (path == "")
            return false;

        _settings.SBox.SBoxContentPath = path;

        Save();
        return true;
    }

    public void SetSBoxShaderExportEnabled(bool bS2ShaderExportEnabled)
    {
        _settings.SBox.SBoxShaderExportsEnabled = bS2ShaderExportEnabled;
        Save();
    }

    public bool GetSBoxShaderExportEnabled()
    {
        return _settings.SBox.SBoxShaderExportsEnabled;
    }

    public void SetSBoxMaterialExportEnabled(bool bS2VMATExportEnabled)
    {
        _settings.SBox.SBoxMaterialExportsEnabled = bS2VMATExportEnabled;
        Save();
    }

    public bool GetSBoxMaterialExportEnabled()
    {
        return _settings.SBox.SBoxMaterialExportsEnabled;
    }

    public void SetSBoxModelExportEnabled(bool bS2VMDLExportEnabled)
    {
        _settings.SBox.SBoxModelExportsEnabled = bS2VMDLExportEnabled;
        Save();
    }

    public bool GetSBoxModelExportEnabled()
    {
        return _settings.SBox.SBoxModelExportsEnabled;
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
            Log.Error($"Failed to load config file {_configFilePath}: {e.Message}");
        }
        catch (JsonReaderException e)
        {
            Log.Error($"Failed to load config file {_configFilePath}: {e.Message}");
        }

        if (_settings.Common == null)
        {
            _settings.Common = new CommonSettings();
            _settings.Blender = new BlenderSettings();
            _settings.SBox = new SBoxSettings();
            WriteConfig();
        }

        // todo make validation generic, lots of nice ways to do this in .net 8
        if (!Enum.IsDefined(typeof(TigerStrategy), _settings.Common.CurrentStrategy))
        {
            _settings.Common.CurrentStrategy = TigerStrategy.NONE;
            WriteConfig();
        }

        foreach ((TigerStrategy strategy, string packagesPath) in _settings.Common.PackagesPath)
        {
            Strategy.AddNewStrategy(strategy, packagesPath, false);
        }

        if (CharmInstance.Args.GetArgValue("strategy", out string strategyName))
        {
            Strategy.SetStrategy(strategyName);
        }
        else
        {
            Strategy.SetStrategy(_settings.Common.CurrentStrategy);
        }

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
