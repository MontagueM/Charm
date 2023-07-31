using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Tiger;
using Tiger.Schema.Activity;

namespace Charm;

public partial class ActivityView : UserControl
{
    private Activity _activity;

    public ActivityView()
    {
        InitializeComponent();
    }

    public async void LoadActivity(FileHash hash)
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "loading activity tag",
            "loading map ui",
            "loading dialogue ui",
            "loading directive ui",
            "loading music ui",
        });
        MapControl.Visibility = Visibility.Hidden;
        _activity = null;
        await Task.Run(() =>
        {
            _activity = FileResourcer.Get().GetFile<Activity>(hash);
        });
        MainWindow.Progress.CompleteStage();
        await Task.Run(() =>
        {
            Dispatcher.Invoke(() =>
            {
                MapControl.LoadUI(_activity);
            });
            MainWindow.Progress.CompleteStage();
            Dispatcher.Invoke(() =>
            {
                DialogueControl.LoadUI(_activity.Hash);
            });
            MainWindow.Progress.CompleteStage();
            Dispatcher.Invoke(() =>
            {
                DirectiveControl.LoadUI(_activity.Hash);
            });
            MainWindow.Progress.CompleteStage();
            Dispatcher.Invoke(() =>
            {
                MusicControl.LoadUI(_activity.Hash);
            });
            MainWindow.Progress.CompleteStage();
        });

        MapControl.Visibility = Visibility.Visible;
    }

    public void Dispose()
    {
        MapControl.Dispose();
        MusicControl.TagList.TagView.MusicControl.WemsControl.MusicPlayer.Dispose();
    }
}
