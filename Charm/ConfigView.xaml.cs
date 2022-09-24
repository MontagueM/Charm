using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Field;
using Field.General;
using Serilog;

namespace Charm;

public partial class ConfigView : UserControl
{
    private Activity _activity;
    
    private readonly ILogger _activityLog = Log.ForContext<ActivityView>();
    
    public ConfigView()
    {
        InitializeComponent();
    }
    
    public async void LoadActivity(TagHash hash)
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "loading"
        });
        ConfigControl.Visibility = Visibility.Hidden;
        _activity = null;
        MainWindow.Progress.CompleteStage();
        await Task.Run(() =>
        {
            Dispatcher.Invoke(() =>
            {
                ConfigControl.OnControlLoaded(null, null);
            });
            MainWindow.Progress.CompleteStage();
            Dispatcher.Invoke(() =>
            {
                Source2Control.OnControlLoaded(null, null);
            });
            MainWindow.Progress.CompleteStage();
            Dispatcher.Invoke(() =>
            {
                UnrealControl.OnControlLoaded(null, null);
            });
            MainWindow.Progress.CompleteStage();
        });

        //MapControl.Visibility = Visibility.Visible;
    }

    public void Dispose()
    {
        //ConfigControl.Dispose();
        //Source2Control.Dispose();
    }
}