using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using VersionChecker;

namespace Charm;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static ApplicationVersion CurrentVersion = new("3.0.8");

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Idk why for some people Charm is looking at system32 instead of the exe location...
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        if (!IsVcRedistInstalled())
        {
            MessageBoxResult result = MessageBox.Show(
                "Charm requires Visual C++ Redistributables to function properly, would you like to install these now?",
                "Missing Dependency",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );
            if (result == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version",
                    UseShellExecute = true
                });
                Environment.Exit(0);
            }
        }

        string[] args = e.Args;
        if (args.Length > 0)
        {
            uint apiHash = 0;
            int c = 0;
            while (c < args.Length)
            {
                if (args[c] == "--api")
                {
                    apiHash = Convert.ToUInt32(args[c + 1]);
                    break;
                }
                c++;
            }
            if (apiHash != 0)
            {
                return;
            }
        }
    }

    bool IsVcRedistInstalled()
    {
        // Key for VC++ 2015-2022 Redistributable (x64)
        const string keyPath = @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64";

        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
        {
            if (key != null)
            {
                object installed = key.GetValue("Installed");
                if (installed is int value && value == 1)
                {
                    return true;
                }
            }
        }
        return false;
    }
}


