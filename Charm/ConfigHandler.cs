using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace Charm;

public class ConfigHandler
{
    private static Configuration _config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.ExecutablePath);

    #region packagesPath
    public static void CheckPackagesPathIsValid()
    {
        if (_config.AppSettings.Settings["packagesPath"] == null)
        {
            OpenPackagesPathDialog();
        }
    }

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
            
        _config.AppSettings.Settings.Add("packagesPath", path);
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

    #region importToChildFolder

    public static void SetUnrealInteropImportToChildFolder(bool bImportToChildFolder)
    {
        if (_config.AppSettings.Settings["importToChildFolder"] == null)
        {
            _config.AppSettings.Settings.Add("importToChildFolder", bImportToChildFolder.ToString());
        }
        else
        {
            _config.AppSettings.Settings["importToChildFolder"].Value = bImportToChildFolder.ToString();
        }

        Save();
    }
    
    public static bool GetUnrealInteropImportToChildFolder()
    {
        if (_config.AppSettings.Settings["importToChildFolder"] == null)
        {
            return true;
        }
        return _config.AppSettings.Settings["importToChildFolder"].Value == "True";
    }
    
    #endregion
    
    private static void Save()
    {
        _config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
    }
}