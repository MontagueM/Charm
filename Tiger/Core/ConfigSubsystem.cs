using Arithmic;
using Newtonsoft.Json;
using Tiger.Exporters;

// using MessageBox = System.Windows.Forms.MessageBox;

namespace Tiger;

public struct Settings
{
    public CommonSettings Common;
    public UnrealSettings Unreal;
    public Source2Settings Source2;
}

public class CommonSettings
{
    public Dictionary<TigerStrategy, string> PackagesPath { get; set; } = new Dictionary<TigerStrategy, string>();
    public TigerStrategy CurrentStrategy { get; set; } = TigerStrategy.NONE;
    public string ExportPath { get; set; } = "";
    public bool SingleFolderMapAssetsEnabled { get; set; } = false;
    public bool IndividualStaticsEnabled { get; set; } = false;
    public TextureExportFormat OutputTextureFormat { get; set; } = TextureExportFormat.PNG;
    public bool AnimatedBackground { get; set; } = true;
    public bool MotionEffects { get; set; } = true;
    public bool HolofoilShader { get; set; } = true;
    public bool CustomRenderer { get; set; } = false;
    public bool SaveShaderHLSL { get; set; } = false;
    public bool SaveEquirectCubemaps { get; set; } = false;

    public bool AcceptedAgreementV320 { get; set; } = false;
}

// [ConfigSubsystem]
public class UnrealSettings
{
    public bool UnrealInteropEnabled { get; set; } = false;
    public string UnrealInteropPath { get; set; } = "";
}

// [ConfigSubsystem]
public class Source2Settings
{
    public bool Source2ExportsEnabled { get; set; } = false;
    //public string Source2Path { get; set; } = "";
}

// class TypeExtensions
// {
//     public bool HasAttributeOfType<T>(this Type type) where T : Attribute
//     {
//         return type.GetCustomAttributes<T>(true).Any();
//     }
// }

public class ConfigSubsystem : Subsystem<ConfigSubsystem>
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

    #region General
    public bool GetAcceptedAgreement()
    {
        return _settings.Common.AcceptedAgreementV320;
    }

    public void SetAcceptedAgreement(bool b)
    {
        _settings.Common.AcceptedAgreementV320 = b;
        Save();
    }
    #endregion

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
        if (path == "" || !Strategy.CheckValidPackagesDirectory(strategy, path))
            return false;

        if (_settings.Common.PackagesPath.ContainsKey(strategy))
            _settings.Common.PackagesPath.Remove(strategy);

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

    #region source2Path

    //public string GetSource2Path()
    //{
    //    return _settings.Source2.Source2Path;
    //}

    //public bool TrySetSource2Path(string path)
    //{
    //    if (path == "")
    //    {
    //        return false;
    //    }

    //    if (!path.EndsWith("win64"))
    //    {
    //        return false;
    //    }

    //    _settings.Source2.Source2Path = path;

    //    Save();
    //    return true;
    //}

    #endregion

    #region source2ExportsEnabled

    public void SetSBoxExportEnabled(bool bS2ExportEnabled)
    {
        _settings.Source2.Source2ExportsEnabled = bS2ExportEnabled;
        Save();
    }

    public bool GetSBoxExportEnabled()
    {
        return _settings.Source2.Source2ExportsEnabled;
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
        if (_settings.Unreal == null)
        {
            return false;
        }
        return _settings.Unreal.UnrealInteropEnabled;
    }

    #endregion

    #region singleFolderMapsEnabled

    public void SetSingleFolderMapAssetsEnabled(bool bSingleFolderMapAssetsEnabled)
    {
        _settings.Common.SingleFolderMapAssetsEnabled = bSingleFolderMapAssetsEnabled;
        Save();
    }

    public bool GetSingleFolderMapAssetsEnabled()
    {
        return _settings.Common.SingleFolderMapAssetsEnabled;
    }

    #endregion

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

    public void SetAnimatedBackground(bool bg)
    {
        _settings.Common.AnimatedBackground = bg;
        Save();
    }

    public bool GetAnimatedBackground()
    {
        return _settings.Common.AnimatedBackground;
    }

    public void SetMotionEffects(bool b)
    {
        _settings.Common.MotionEffects = b;
        Save();
    }

    public bool GetMotionEffects()
    {
        return _settings.Common.MotionEffects;
    }

    public void SetHolofoilShader(bool b)
    {
        _settings.Common.HolofoilShader = b;
        Save();
    }

    public bool GetHolofoilShader()
    {
        return _settings.Common.HolofoilShader;
    }

    public void SetCustomRenderer(bool b)
    {
        _settings.Common.CustomRenderer = b;
        Save();
    }

    public bool GetCustomRenderer()
    {
        return _settings.Common.CustomRenderer;
    }

    public void SetSaveShaderHLSL(bool val)
    {
        if (_settings.Source2.Source2ExportsEnabled)
            val = true;

        _settings.Common.SaveShaderHLSL = val;
        Save();
    }

    public bool GetSaveShaderHLSL()
    {
        return _settings.Common.SaveShaderHLSL;
    }

    public void SetExportEquirectCubemaps(bool val)
    {
        _settings.Common.SaveEquirectCubemaps = val;
        Save();
    }

    public bool GetExportEquirectCubemaps()
    {
        return _settings.Common.SaveEquirectCubemaps;
    }

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
            Log.Error($"Failed to load config file {_configFilePath}: {e.Message}");
        }
        catch (JsonReaderException e)
        {
            Log.Error($"Failed to load config file {_configFilePath}: {e.Message}");
        }

        if (_settings.Common == null)
        {
            _settings.Common = new CommonSettings();
            _settings.Unreal = new UnrealSettings();
            _settings.Source2 = new Source2Settings();
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

        if (TigerInstance.Args.GetArgValue("strategy", out string strategyName))
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
        if (TigerInstance.Args.GetArgValue("config", out string configPath))
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
