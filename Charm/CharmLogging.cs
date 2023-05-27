using System;
using System.IO;
using Arithmic;
using Tiger;

namespace Charm;

public class CharmLogging : Subsystem
{
    protected override bool Initialise()
    {
        AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;
        return true;
    }

    private static void CatchUnhandledException
        (object sender, UnhandledExceptionEventArgs e)
    {
        LogException((Exception)e.ExceptionObject);
    }

    public static void LogException(Exception ex)
    {
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();

        Log.Fatal("\n### Crash ###\n" + ex.Source + ex.InnerException + ex + ex.Message + ex.StackTrace);
        Log.Fatal("Config file:\n" + File.ReadAllText(config.GetConfigFilePath()));
        foreach (TigerStrategy strategy in Strategy.GetAllStrategies())
        {
            Log.Fatal("Strategy: " + strategy);
            Log.Fatal("Config: " + Strategy.GetStrategyConfiguration(strategy));
            Log.Fatal("Number of packages: " + PackageResourcer.Get(strategy).PackagePathsCache.GetAllPackageIds().Count);
        }
        // if (ConfigHandler.GetExportSavePath() != String.Empty)
        // Log.Fatal("Exported directory:\n" + string.Join("\n", Directory.GetFiles(ConfigHandler.GetExportSavePath()))
        // + "\n" + string.Join("\n", Directory.GetDirectories(ConfigHandler.GetExportSavePath())));
        // Log.CloseAndFlush();
    }
}
