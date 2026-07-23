using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Arithmic;

namespace Charm;

public partial class ProgressView : UserControl
{
    private Queue<string> _progressStages;
    private int TotalStageCount;
    private bool LogProgress { get; set; } = true;
    private bool HideBar { get; set; } = false;

    public ProgressView()
    {
        InitializeComponent();
        Hide();
    }

    public void Hide()
    {
        Visibility = Visibility.Hidden;
    }

    public void Show()
    {
        ProgressBarPanel.Visibility = HideBar ? Visibility.Collapsed : Visibility.Visible;
        Visibility = Visibility.Visible;
    }

    private void UpdateProgress()
    {
        ProgressBar.Value = GetProgressPercentage();
        ProgressText.Text = GetCurrentStageName();
    }

    public void SetProgressStage(string stage, bool bLogProgress = true, bool bHideBar = false)
    {
        SetProgressStages(new List<string> { stage }, bLogProgress, bHideBar);
    }

    public void SetProgressStages(List<string> progressStages, bool bLogProgress = true, bool bHideBar = false)
    {
        Dispatcher.Invoke(() =>
        {
            LogProgress = bLogProgress;
            TotalStageCount = progressStages.Count;
            HideBar = bHideBar || TotalStageCount == 1;

            _progressStages = new Queue<string>();
            foreach (string progressStage in progressStages)
            {
                _progressStages.Enqueue(progressStage);
            }

            UpdateProgress();
            Show();
        });
    }

    public void CompleteStage()
    {
        Dispatcher.Invoke(() =>
        {
            if (_progressStages.Count == 0)
            {
                Hide();
                return;
            }
            string removed = _progressStages.Dequeue();
            if (LogProgress)
                Log.Verbose($"Completed loading stage: {removed}");

            UpdateProgress();
            if (_progressStages.Count == 0)
            {
                Hide();
            }
        });
    }

    public string GetCurrentStageName()
    {
        if (_progressStages.Count > 0)
        {
            string stage = _progressStages.Peek();
            if (LogProgress)
                Log.Verbose($"Starting loading stage: {stage}");
            return stage;
        }
        return "Loading";
    }

    public int GetProgressPercentage()
    {
        if (TotalStageCount == 1)
            return 50;
        else
            return 100 - 95 * _progressStages.Count / TotalStageCount;
    }
}
