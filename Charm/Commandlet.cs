using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Arithmic;
using Interop;
using Tiger;

namespace Charm;

public abstract class Commandlet
{
    /// <summary>
    ///
    /// </summary>
    /// <returns>true if a commandlet was run.</returns>
    public static bool RunCommandlet()
    {
        Log.AddSink<ConsoleSink>();
        Log.AddSink<FileSink>();
        if (CharmInstance.Args.GetArgValue("commandlet", out string commandletName))
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            ParseBaseCommandletParams();
            RunCommandletFromClassName(commandletName);

            stopwatch.Stop();
            Log.Info($"Commandlet '{commandletName}' complete. Duration: {stopwatch.Elapsed.TotalSeconds} seconds");

            return true;
        }
        else
        {
            return false;
        }
    }

    private static void ParseBaseCommandletParams()
    {
        if (CharmInstance.Args.GetArgValue("strategy", out string strategyName))
        {
            Strategy.SetStrategy(strategyName);
        }
        if (CharmInstance.Args.IsArgPresent("NewPackagePathsCache"))
        {
            PackagePathsCache.ClearCacheFiles();
        }
    }

    private static void RunCommandletFromClassName(string commandletName)
    {
        Type? commandletType = FindCommandletFromClassName(commandletName);
        if (commandletType == null)
        {
            throw new Exception($"Could not find commandlet with name {commandletName}");
        }

        ICommandlet commandlet = (ICommandlet)Activator.CreateInstance(commandletType);
        commandlet.Run(CharmInstance.Args);
    }

    private static Type? FindCommandletFromClassName(string commandletName)
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .FirstOrDefault(t => t.Name == commandletName);
    }
}

