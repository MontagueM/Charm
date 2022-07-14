using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Field;
using Field.General;
using Serilog;

namespace Charm;

public partial class ActivityView : UserControl
{
    private Activity _activity;
    
    private readonly ILogger _activityLog = Log.ForContext<ActivityView>();
    
    public ActivityView()
    {
        InitializeComponent();
    }
    
    public async void LoadActivity(TagHash hash)
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "loading activity tag",
            "loading map ui",
            "loading dialogue ui",
            "loading directive ui",
            "loading music ui",
        });
        _activity = null;
        await Task.Run(() =>
        {
            _activity = PackageHandler.GetTag(typeof(Activity), hash);
        });
        MainWindow.Progress.CompleteStage();
        MapControl.LoadUI(_activity);
        MainWindow.Progress.CompleteStage();
        DialogueControl.LoadUI(_activity.Hash);
        MainWindow.Progress.CompleteStage();
        DirectiveControl.LoadUI(_activity.Hash);
        MainWindow.Progress.CompleteStage();
        MusicControl.LoadUI(_activity);
        MainWindow.Progress.CompleteStage();
    }
}