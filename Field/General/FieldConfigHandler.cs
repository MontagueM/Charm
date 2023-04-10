using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows;
using Field;

namespace Field;

public class FieldConfigHandler
{
    private static Configuration _config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.ExecutablePath);

    // Todo convert these into general functions, eg GetBool(...) or GetPath(...) SetPath(...) etc, way cleaner
    public static bool DoesPathKeyExist(string key)
    {
        return _config.AppSettings.Settings[key] != null;
    }

	public static void Refresh()
	{
		ConfigurationManager.RefreshSection("appSettings");
	}

	#region packagesPath

	public static string GetPackagesPath()
    {
        if (_config.AppSettings.Settings["packagesPath"] == null)
        {
            return "";
        }
        return _config.AppSettings.Settings["packagesPath"].Value;
    }

    #endregion
    
    #region source2Path
        
    public static string GetSource2Path()
    {
        if (_config.AppSettings.Settings["source2Path"] == null)
        {
            return "";
        }
        return _config.AppSettings.Settings["source2Path"].Value;
    }
    #endregion

    #region source2ExportsEnabled
    public static bool GetS2ShaderExportEnabled()
    {
        if (_config.AppSettings.Settings["s2ShaderExportEnabled"] == null)
        {
            return false;
        }
        return _config.AppSettings.Settings["s2ShaderExportEnabled"].Value == "True";
    }

    public static bool GetS2VMATExportEnabled()
    {
        if (_config.AppSettings.Settings["s2VMATExportEnabled"] == null)
        {
            return false;
        }
        return _config.AppSettings.Settings["s2VMATExportEnabled"].Value == "True";
    }

    public static bool GetS2VMDLExportEnabled()
    {
        if (_config.AppSettings.Settings["s2VMDLExportEnabled"] == null)
        {
            return false;
        }
        return _config.AppSettings.Settings["s2VMDLExportEnabled"].Value == "True";
    }
    
    #endregion


    #region exportSavePath

    public static string GetExportSavePath()
    {
        if (_config.AppSettings.Settings["exportSavePath"] == null)
        {
            return "";
        }
        return _config.AppSettings.Settings["exportSavePath"].Value;
    }
    #endregion

    #region unrealInteropPath
 
    public static string GetUnrealInteropPath()
    {
        if (_config.AppSettings.Settings["unrealInteropPath"] == null)
        {
            return "";
        }
        return _config.AppSettings.Settings["unrealInteropPath"].Value;
    }
    
    #endregion

    #region unrealInteropEnabled

    public static bool GetUnrealInteropEnabled()
    {
        if (_config.AppSettings.Settings["unrealInteropEnabled"] == null)
        {
            return false;
        }
        return _config.AppSettings.Settings["unrealInteropEnabled"].Value == "True";
    }
    
    #endregion

    //#region singleFolderMapsEnabled

    //public static void SetSingleFolderMapsEnabled(bool bSingleFolderMapsEnabled)
    //{
    //    if (_config.AppSettings.Settings["singleFolderMapsEnabled"] == null)
    //    {
    //        _config.AppSettings.Settings.Add("singleFolderMapsEnabled", bSingleFolderMapsEnabled.ToString());
    //    }
    //    else
    //    {
    //        _config.AppSettings.Settings["singleFolderMapsEnabled"].Value = bSingleFolderMapsEnabled.ToString();
    //    }

    //    Save();
    //}
    
    //public static bool GetSingleFolderMapsEnabled()
    //{
    //    if (_config.AppSettings.Settings["singleFolderMapsEnabled"] == null)
    //    {
    //        return true;
    //    }
    //    return _config.AppSettings.Settings["singleFolderMapsEnabled"].Value == "True";
    //}

    //#endregion

    //#region IndvidualStaticsEnabled

    //public static void SetIndvidualStaticsEnabled(bool bIndvidualStaticsEnabled)
    //{
    //    if (_config.AppSettings.Settings["indvidualStaticsEnabled"] == null)
    //    {
    //        _config.AppSettings.Settings.Add("indvidualStaticsEnabled", bIndvidualStaticsEnabled.ToString());
    //    }
    //    else
    //    {
    //        _config.AppSettings.Settings["indvidualStaticsEnabled"].Value = bIndvidualStaticsEnabled.ToString();
    //    }
    //    Save();
    //}

    //public static bool GetIndvidualStaticsEnabled()
    //{
    //    if (_config.AppSettings.Settings["indvidualStaticsEnabled"] == null)
    //    {
    //        return true;
    //    }
    //    return _config.AppSettings.Settings["indvidualStaticsEnabled"].Value == "True";
    //}

    //#endregion

    //#region IndvidualEntitiesEnabled

    //public static void SetIndvidualEntitiesEnabled(bool bIndvidualEntitiesEnabled)
    //{
    //    if (_config.AppSettings.Settings["indvidualEntitiesEnabled"] == null)
    //    {
    //        _config.AppSettings.Settings.Add("indvidualEntitiesEnabled", bIndvidualEntitiesEnabled.ToString());
    //    }
    //    else
    //    {
    //        _config.AppSettings.Settings["indvidualEntitiesEnabled"].Value = bIndvidualEntitiesEnabled.ToString();
    //    }
    //    Save();
    //}

    //public static bool GetIndvidualEntitiesEnabled()
    //{
    //    if (_config.AppSettings.Settings["indvidualEntitiesEnabled"] == null)
    //    {
    //        return true;
    //    }
    //    return _config.AppSettings.Settings["indvidualEntitiesEnabled"].Value == "True";
    //}

    //#endregion

    //#region SaveCBuffersEnabled

    //public static void SetSaveCBuffersEnabled(bool bSaveCBuffersEnabled)
    //{
    //    if (_config.AppSettings.Settings["saveCBuffersEnabled"] == null)
    //    {
    //        _config.AppSettings.Settings.Add("saveCBuffersEnabled", bSaveCBuffersEnabled.ToString());
    //    }
    //    else
    //    {
    //        _config.AppSettings.Settings["saveCBuffersEnabled"].Value = bSaveCBuffersEnabled.ToString();
    //    }
    //    Save();
    //}

    //public static bool GetSaveCBuffersEnabled()
    //{
    //    if (_config.AppSettings.Settings["saveCBuffersEnabled"] == null)
    //    {
    //        return false;
    //    }
    //    return _config.AppSettings.Settings["saveCBuffersEnabled"].Value == "True";
    //}

    //#endregion

    //#region outputTextureFormat

    //public static void SetOutputTextureFormat(ETextureFormat outputTextureFormat)
    //{
    //    if (_config.AppSettings.Settings["outputTextureFormat"] == null)
    //    {
    //        _config.AppSettings.Settings.Add("outputTextureFormat", outputTextureFormat.ToString());
    //    }
    //    else
    //    {
    //        _config.AppSettings.Settings["outputTextureFormat"].Value = outputTextureFormat.ToString();
    //    }

    //    Save();
    //}
    
    //public static ETextureFormat GetOutputTextureFormat()
    //{
    //    if (_config.AppSettings.Settings["outputTextureFormat"] == null)
    //    {
    //        return ETextureFormat.DDS_BGRA_UNCOMP_DX10;
    //    }
    //    return FindEnumValue(_config.AppSettings.Settings["outputTextureFormat"].Value);
    //}
    
    //private static ETextureFormat FindEnumValue(string description)
    //{
    //    for (int i = 0; i < typeof(ETextureFormat).GetFields().Length-1; i++)
    //    {
    //        if (((ETextureFormat)i).ToString() == description)
    //        {
    //            return (ETextureFormat)i;
    //        }
    //    }
    //    return ETextureFormat.DDS_BGRA_UNCOMP_DX10;
    //}

    //#endregion

}