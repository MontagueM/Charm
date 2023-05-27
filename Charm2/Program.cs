using System;
using Arithmic;
using Avalonia;
// using Avalonia.ReactiveUI;
using Tiger;

namespace Charm;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Log.Info("Initialising Charm subsystems");
        CharmInstance.Args = new CharmArgs(args);
        CharmInstance.InitialiseSubsystems();
        Log.Info("Initialised Charm subsystems");
        // todo figure out how to make sure commandlets initialise all the subsystems they need

        if (Commandlet.RunCommandlet())
        {
            return;
        }

        var config = Strategy.GetStrategyConfiguration(TigerStrategy.DESTINY2_WITCHQUEEN_6307);
        config.PackagesDirectory = "I:/v6307/packages/";
        Strategy.UpdateStrategyConfiguration(TigerStrategy.DESTINY2_WITCHQUEEN_6307, config);
        Strategy.SetStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307);

        Log.Info("Starting Charm UI");
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    // .UseReactiveUI();
}
