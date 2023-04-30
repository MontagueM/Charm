using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Interop;
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
        CharmInstance.Args = new CharmArgs(args);
        
        // todo figure out how to make sure commandlets initialise all the subsystems they need
        TestInclude testInclude = new();
        if (Commandlet.RunCommandlet())
        {
            return;
        }
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}