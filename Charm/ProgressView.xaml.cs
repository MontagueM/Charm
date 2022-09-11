using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Serilog;

namespace Charm;

public partial class ProgressView : UserControl
{
    private Queue<string> _progressStages;
    private int TotalStageCount;
    private ILogger _progressLog;
    private bool bLogProgress = true;
    private bool bUseFullBar = false;

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
        this.bUseFullBar = bUseFullBar;
        _progressLog = Log.Logger.ForContext<ProgressView>();
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
            string removed = _progressStages.Dequeue();
            if (bLogProgress)
                _progressLog.Debug($"Completed loading stage: {removed}");
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
                _progressLog.Debug($"Starting loading stage: {stage}");
            return stage;
        }
        return "Loading";
    }
    
    public int GetProgressPercentage()
    {
        // We want to artificially make it more meaningful, so we pad by 15% on each side
        if (bUseFullBar)
            return 100 - 100 * _progressStages.Count / TotalStageCount;
        else
            return 95 - 90 * _progressStages.Count / TotalStageCount;
    }
}