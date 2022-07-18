﻿using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Windows;
using Field.Textures;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Charm;

public class ConfigHandler
{
    private static Configuration _config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.ExecutablePath);

    // Todo convert these into general functions, eg GetBool(...) or GetPath(...) SetPath(...) etc, way cleaner
    public static bool DoesPathKeyExist(string key)
    {
        return _config.AppSettings.Settings[key] != null;
    }

    #region packagesPath

    public static void OpenPackagesPathDialog()
    {
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
            dialog.Description = "Select the folder where your D2-WQ packages (*.pkg) are located";
            bool success = false;
            while (!success)
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    success = TrySetPackagePath(dialog.SelectedPath);
                }

                if (!success)
                {
                    MessageBox.Show("Directory selected is invalid, please select the correct packages directory.");
                }
            }
        }
    }
        
    public static string GetPackagesPath()
    {
        if (_config.AppSettings.Settings["packagesPath"] == null)
        {
            return "";
        }
        return _config.AppSettings.Settings["packagesPath"].Value;
    }

    private static bool TrySetPackagePath(string path)
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
        
        if (_config.AppSettings.Settings["packagesPath"] == null)
        {
            _config.AppSettings.Settings.Add("packagesPath", path);
        }
        else
        {
            _config.AppSettings.Settings["packagesPath"].Value = path;
        }
        Save();
        return true;
    }

    #endregion
    
    #region exportSavePath

    public static void OpenExportSavePathDialog()
    {
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
            dialog.Description = "Select the folder to export to";
            bool success = false;
            while (!success)
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    success = TrySetExportSavePath(dialog.SelectedPath);
                }

                if (!success)
                {
                    MessageBox.Show("Directory selected is invalid, please select the correct directory.");
                }
            }
        }
    }
        
    public static string GetExportSavePath()
    {
        if (_config.AppSettings.Settings["exportSavePath"] == null)
        {
            return "";
        }
        return _config.AppSettings.Settings["exportSavePath"].Value;
    }

    private static bool TrySetExportSavePath(string path)
    {
        if (path == "")
        {
            return false;
        }
        if (_config.AppSettings.Settings["exportSavePath"] == null)
        {
            _config.AppSettings.Settings.Add("exportSavePath", path);
        }
        else
        {
            _config.AppSettings.Settings["exportSavePath"].Value = path;
        }
        Save();
        return true;
    }
    #endregion

    #region unrealInteropPath
    
    public static void OpenUnrealInteropPathDialog()
    {
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
            bool success = false;
            while (!success)
            {
                dialog.Description = "Select the folder where you want to import to unreal engine (eg Content folder)";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    success = TrySetUnrealInteropPath(dialog.SelectedPath);
                }
            }
        }
    }
    
    public static bool TrySetUnrealInteropPath(string interopPath)
    {
        if (!interopPath.Contains("Content"))
        {
            return false;
        }
        if (_config.AppSettings.Settings["unrealInteropPath"] == null)
        {
            _config.AppSettings.Settings.Add("unrealInteropPath", interopPath);
            SetUnrealInteropEnabled(true);
        }
        else
        {
            _config.AppSettings.Settings["unrealInteropPath"].Value = interopPath;
        }
        Save();
        return true;
    }

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

    public static void SetUnrealInteropEnabled(bool bUnrealInteropEnabled)
    {
        if (_config.AppSettings.Settings["unrealInteropEnabled"] == null)
        {
            _config.AppSettings.Settings.Add("unrealInteropEnabled", bUnrealInteropEnabled.ToString());
        }
        else
        {
            _config.AppSettings.Settings["unrealInteropEnabled"].Value = bUnrealInteropEnabled.ToString();
        }

        Save();
    }

    public static bool GetUnrealInteropEnabled()
    {
        if (_config.AppSettings.Settings["unrealInteropEnabled"] == null)
        {
            return false;
        }
        return _config.AppSettings.Settings["unrealInteropEnabled"].Value == "True";
    }
    
    #endregion

    #region blenderInteropEnabled
    
    public static void SetBlenderInteropEnabled(bool bBlenderInteropEnabled)
    {
        if (_config.AppSettings.Settings["blenderInteropEnabled"] == null)
        {
            _config.AppSettings.Settings.Add("blenderInteropEnabled", bBlenderInteropEnabled.ToString());
        }
        else
        {
            _config.AppSettings.Settings["blenderInteropEnabled"].Value = bBlenderInteropEnabled.ToString();
        }

        Save();
    }
    
    public static bool GetBlenderInteropEnabled()
    {
        if (_config.AppSettings.Settings["blenderInteropEnabled"] == null)
        {
            return false;
        }
        return _config.AppSettings.Settings["blenderInteropEnabled"].Value == "True";
    }

    #endregion

    #region singleFolderMapsEnabled

    public static void SetSingleFolderMapsEnabled(bool bSingleFolderMapsEnabled)
    {
        if (_config.AppSettings.Settings["singleFolderMapsEnabled"] == null)
        {
            _config.AppSettings.Settings.Add("singleFolderMapsEnabled", bSingleFolderMapsEnabled.ToString());
        }
        else
        {
            _config.AppSettings.Settings["singleFolderMapsEnabled"].Value = bSingleFolderMapsEnabled.ToString();
        }

        Save();
    }
    
    public static bool GetSingleFolderMapsEnabled()
    {
        if (_config.AppSettings.Settings["singleFolderMapsEnabled"] == null)
        {
            return true;
        }
        return _config.AppSettings.Settings["singleFolderMapsEnabled"].Value == "True";
    }

    #endregion
    
    #region outputTextureFormat

    public static void SetOutputTextureFormat(ETextureFormat outputTextureFormat)
    {
        if (_config.AppSettings.Settings["outputTextureFormat"] == null)
        {
            _config.AppSettings.Settings.Add("outputTextureFormat", outputTextureFormat.ToString());
        }
        else
        {
            _config.AppSettings.Settings["outputTextureFormat"].Value = outputTextureFormat.ToString();
        }

        Save();
    }
    
    public static ETextureFormat GetOutputTextureFormat()
    {
        if (_config.AppSettings.Settings["outputTextureFormat"] == null)
        {
            return ETextureFormat.DDS_BGRA_UNCOMP_DX10;
        }
        return FindEnumValue(_config.AppSettings.Settings["outputTextureFormat"].Value);
    }
    
    private static ETextureFormat FindEnumValue(string description)
    {
        for (int i = 0; i < typeof(ETextureFormat).GetFields().Length-1; i++)
        {
            if (((ETextureFormat)i).ToString() == description)
            {
                return (ETextureFormat)i;
            }
        }
        return ETextureFormat.DDS_BGRA_UNCOMP_DX10;
    }

    #endregion
    
    private static void Save()
    {
        _config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
    }
}