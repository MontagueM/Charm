using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Charm;

public partial class ProgressView : UserControl
{
    private Queue<string> _progressStages;
    private int TotalStageCount;
    
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

    public void SetProgressStages(List<string> progressStages)
    {
        TotalStageCount = progressStages.Count;
        _progressStages = new Queue<string>();
        foreach (var progressStage in progressStages)
        {
            _progressStages.Enqueue(progressStage);
        }
        UpdateProgress();
        Show();
    }

    public void CompleteStage()
    {
        _progressStages.Dequeue();
        UpdateProgress();
        if (_progressStages.Count == 0)
        {
            Hide();
        }
    }

    public string GetCurrentStageName()
    {
        if (_progressStages.Count > 0)
        {
            return _progressStages.Peek();
        }
        return "Loading";
    }
    
    public int GetProgressPercentage()
    {
        // We want to artificially make it more meaningful, so we pad by 15% on each side
        return 95 - 90 * _progressStages.Count / TotalStageCount;
    }
}