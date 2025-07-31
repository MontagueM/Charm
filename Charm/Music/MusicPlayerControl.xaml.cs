using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Arithmic;
using NAudio.Wave;
using Tiger;
using Tiger.Schema.Audio;

namespace Charm;

public partial class MusicPlayerControl : UserControl, IDisposable
{
    private WaveOut _output;
    private Wem _wem;
    private WaveChannel32 _waveProvider;
    private DispatcherTimer _positionTimer;

    public bool CanPlay { get; private set; } = false;

    public MusicPlayerControl()
    {
        InitializeComponent();
        SetVolume(VolumeBar.Value);
        InitializePositionTimer();
    }

    public void SetPlayingText(string name) => PlayingText.Text = $"PLAYING: {name}";
    public bool IsPlaying() => _output?.PlaybackState == PlaybackState.Playing;
    private void UpdatePlayButtonText(string text) => ((TextBlock)PlayPause.Content).Text = text;

    public FileHash GetWem() => _wem?.Hash;

    public bool SetWem(Wem wem)
    {
        CleanupResources();
        CanPlay = false;

        if (wem == null) return false;

        try
        {
            _wem = wem;
            _waveProvider = wem.MakeWaveChannel();

            if (_waveProvider == null)
            {
                MessageBox.Show("WaveProvider is null");
                HandleError("WaveProvider is null");
                return false;
            }

            MakeOutput();
            _output.Init(_waveProvider);
            _output.Stop();
            _waveProvider.Position = 0;
            SetSliderPosition(0, true);

            SetVolume(VolumeBar.Value);
            CanPlay = true;

            var totalTime = _waveProvider.TotalTime;
            CurrentDuration.Text = Wem.GetDurationString((float)totalTime.TotalSeconds);
            TotalDuration.Text = Wem.GetDurationString((float)totalTime.TotalSeconds);

            ProgressBar.Value = 0;
            SetPlayingText(wem.Hash);
            UpdatePlayButtonText("PLAY");

            return true;
        }
        catch (Exception ex)
        {
            HandleError($"Failed to initialize audio output: {ex.Message}");
            return false;
        }
    }

    private void InitializePositionTimer()
    {
        _positionTimer = new DispatcherTimer();
        _positionTimer.Interval = TimeSpan.FromMilliseconds(100);
        _positionTimer.Tick += PositionTimer_Tick;
    }

    private void PositionTimer_Tick(object sender, EventArgs e)
    {
        if (IsPlaying() && CanPlay)
        {
            SetSliderPosition(_waveProvider.Position);
        }
    }

    private void MakeOutput()
    {
        _output = new WaveOut();
        _output.PlaybackStopped += (sender, args) =>
        {
            _output.Stop();
            _waveProvider.Position = 0;
            SetSliderPosition(0, true);
            UpdatePlayButtonText("PLAY");
        };
    }

    public void Play()
    {
        if (_output == null || !CanPlay)
        {
            Log.Error("Output is null or cannot play");
            return;
        }

        UpdatePlayButtonText("PAUSE");
        //Log.Info($"Playing {_wem.Hash}");

        Task.Run(() =>
        {
            try
            {
                _positionTimer.Start();
                _output.Play();
            }
            catch (Exception e)
            {
                Log.Warning($"Play failed: {e.Message}");
                return;
            }
        });
    }

    public void Pause()
    {
        _positionTimer.Stop();
        _output?.Pause();
        UpdatePlayButtonText("PLAY");
        //Log.Verbose($"Paused {_wem.Hash}");
    }

    private void SetVolume(double volume)
    {
        if (_waveProvider != null)
            _waveProvider.Volume = (float)volume;
    }

    private void SetSliderPosition(long bytePosition, bool forceUpdate = false)
    {
        if (_waveProvider == null) return;

        var waveFormat = _waveProvider.WaveFormat;
        var totalSeconds = _waveProvider.TotalTime.TotalSeconds;
        var bytesPerSecond = waveFormat.AverageBytesPerSecond;

        if (bytesPerSecond <= 0 || totalSeconds <= 0) return;

        double proportion = bytePosition / (totalSeconds * bytesPerSecond);

        double progressMilliseconds = proportion * _waveProvider.TotalTime.TotalMilliseconds;
        double deltaMilliseconds = Math.Abs(ProgressBar.Value - proportion) * _waveProvider.TotalTime.TotalMilliseconds;

        if (deltaMilliseconds < 500 || forceUpdate)
        {
            TimeSpan currentTime = TimeSpan.FromMilliseconds(progressMilliseconds);
            CurrentDuration.Text = Wem.GetDurationString((float)currentTime.TotalSeconds);
            ProgressBar.Value = proportion;
        }
    }

    private void SetPosition(Slider slider)
    {
        if (_wem == null) return;

        bool isAlreadyPaused = _output.PlaybackState == PlaybackState.Paused;
        Pause();

        bool timerWasRunning = _positionTimer.IsEnabled;
        _positionTimer.Stop();

        double timeInSeconds = slider.Value * _waveProvider.TotalTime.TotalSeconds;
        long targetPosition = (long)(timeInSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);

        // Clamp to valid byte range
        targetPosition = Math.Min(targetPosition, _waveProvider.Length - _waveProvider.WaveFormat.BlockAlign);
        targetPosition = Math.Max(targetPosition, 0);

        if (targetPosition >= _waveProvider.Length - _waveProvider.WaveFormat.BlockAlign)
        {
            SetSliderPosition(targetPosition, true);
            return;
        }

        long alignedPosition = (targetPosition / _waveProvider.WaveFormat.BlockAlign) * _waveProvider.WaveFormat.BlockAlign;
        _waveProvider.Position = alignedPosition;

        SetSliderPosition(targetPosition);
        if (!isAlreadyPaused) Play();

        if (timerWasRunning && !isAlreadyPaused)
        {
            _positionTimer.Start();
        }
    }

    private void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (_wem == null) return;

        if (IsPlaying()) Pause();
        else Play();
    }

    private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => SetVolume(e.NewValue);

    private void ProgressBar_PreviewMouseUp(object sender, MouseButtonEventArgs e) => SetPosition(sender as Slider);

    private void ProgressBar_DragCompleted(object sender, DragCompletedEventArgs e) => SetPosition(sender as Slider);

    private void HandleError(string message)
    {
        Log.Error(message);
    }

    public void Dispose()
    {
        CleanupResources();
    }

    private void CleanupResources()
    {
        CanPlay = false;
        _positionTimer?.Stop();
        _output?.Stop();
        _waveProvider?.Dispose();
        _output?.Dispose();
        _wem?.Dispose();

        _output = null;
        _waveProvider = null;
        _wem = null;
    }
}
