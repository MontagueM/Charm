using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Arithmic;

namespace Charm;

public partial class ProgressView : UserControl
{
    private Queue<string> _progressStages;
    private int TotalStageCount;
    private bool bLogProgress = true;

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
        // Grid.Background = new SolidColorBrush(new Color {A = 0, B = 0, G = 0, R = 0});
        Visibility = Visibility.Visible;
    }

    private void UpdateProgress()
    {
        ProgressBar.Value = GetProgressPercentage();
        ProgressText.Text = GetCurrentStageName();
    }

    public void SetProgressStages(List<string> progressStages, bool bLogProgress = true, bool bUseFullBar = false)
    {
        this.bLogProgress = bLogProgress;
        //this.bUseFullBar = bUseFullBar;
        Dispatcher.Invoke(() =>
        {
            TotalStageCount = progressStages.Count;
            _progressStages = new Queue<string>();
            foreach (var progressStage in progressStages)
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
            if (bLogProgress)
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
            var stage = _progressStages.Peek();
            if (bLogProgress)
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
