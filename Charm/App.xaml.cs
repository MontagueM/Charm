using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;
using Arithmic;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using Tiger;
using VersionChecker;

namespace Charm;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class CharmApp : Application
{
    public static ApplicationVersion CurrentVersion = new("3.2.3");
    public static Assembly? CharmRedacted = null;
    public static Assembly? CharmRenderer = null;

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

        using (var reader = XmlReader.Create("HLSLSyntax.xshd"))
        {
            var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            HighlightingManager.Instance.RegisterHighlighting("HLSL", new[] { ".hlsl", ".cg" }, highlighting);
        }

        LoadRedactedDLL();
        IsRendererDllAvailable();
    }

    public void LoadRedactedDLL()
    {
        string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Charm.Redacted.dll");
        if (File.Exists(dllPath))
        {
            var asm = Assembly.LoadFrom(dllPath);

            Type type = asm.GetType("Charm.Redacted.CharmRedacted");
            if (type == null)
                return;

            FieldInfo field = type.GetField("CurrentVersion", BindingFlags.Public | BindingFlags.Static);
            if (field == null)
                return;

            var redactedVersion = field.GetValue(null) as ApplicationVersion;
            if (redactedVersion.Id != CurrentVersion.Id)
                return;

            Log.Info("Loaded Charm.Redacted");
            CharmRedacted = asm;
        }
    }

    public void IsRendererDllAvailable()
    {
        string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Charm.Renderer.dll");
        if (File.Exists(dllPath))
            CharmRenderer = Assembly.LoadFrom(dllPath);
        else
            return;

        Log.Info("Loaded Charm.Renderer");
    }

    public static bool CanUseRenderer()
    {
        return CharmRenderer is not null
            && ConfigSubsystem.Get().GetCustomRenderer()
            && Strategy.IsLatest();
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


