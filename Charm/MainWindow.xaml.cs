using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Field;
using Field.General;

namespace Charm;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public Configuration Config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.ExecutablePath);
    public MainWindow()
    {
        InitializeComponent();
            
        // Hide tab by default
        MainTabControl.Visibility = Visibility.Hidden;
            
        // Check if packages path exists in config
        CheckPackagesPathValidity();

        // Initialise FNV handler
        FnvHandler.Initialise();
            
        // Initialise global string cache
        PackageHandler.GenerateGlobalStringContainerCache();
            
        // Get all hash64
        TagHash64Handler.Initialise();
        
        // Initialise fbx handler
        FbxHandler.Initialise();
    }

    private void CheckPackagesPathValidity()
    {
        if (Config.AppSettings.Settings["packagesPath"] == null)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select the folder where your D2-WQ packages are located";
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
        else
        {
            MainTabControl.Visibility = Visibility.Visible;
        }
    }
        
    public string GetPackagesPath()
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.ExecutablePath);
        return config.AppSettings.Settings["PackagesPath"].Value;
    }

    private bool TrySetPackagePath(string path)
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
            
        Config.AppSettings.Settings.Add("packagesPath", path);
        Config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
        MainTabControl.Visibility = Visibility.Visible;
        return true;
    }
}