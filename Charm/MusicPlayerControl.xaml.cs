using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Field;
using NAudio.Wave;
using Serilog;

namespace Charm;

public partial class MusicPlayerControl : UserControl
{
    private WaveOut _output;
    private Wem _wem;
    private WaveChannel32 _waveProvider;
    private readonly ILogger _musicLog = Log.ForContext<MusicPlayerControl>();

    public bool CanPlay { get; set; } = false;
    private double _prevPositionValue = 0;

    
    public MusicPlayerControl()
    {
        InitializeComponent();
        SetVolume(VolumeBar.Value);
    }

    private void MakeOutput()
    {
        _output = new WaveOut();
        _output.PlaybackStopped += (sender, args) =>
        {
            _output.Stop();
            _waveProvider.Position = 0;
            _prevPositionValue = 0;
            (PlayPause.Content as TextBlock).Text = "PLAY";
        };
    }

    public void SetPlayingText(string name)
    {
        PlayingText.Text = $"PLAYING: {name}";
    }

    public void SetWem(Wem wem)
    {
        if (_output != null)
            _output.Dispose();
        _wem = wem;
        _waveProvider = wem.MakeWaveChannel();
        MakeOutput();
        _output.Init(_waveProvider);
        SetVolume(VolumeBar.Value);
        CanPlay = true;
        CurrentDuration.Text = Wem.GetDurationString(_waveProvider.CurrentTime);  // todo make this all correct
        TotalDuration.Text = wem.Duration;
        _prevPositionValue = 0;
        ProgressBar.Value = 0;
        SetPlayingText(wem.Hash);
    }
    
    public void Play()
    {
        _musicLog.Information($"Playing {_wem.Hash}");
        (PlayPause.Content as TextBlock).Text = "PAUSE";
        Task.Run(() =>
        {
            try
            {
                _output.Play();  // can sometimes break if its still ending the playback
            }
            catch (Exception e)
            {
                _musicLog.Warning(e.Message);
                return;
            }
            StartPositionHandlerAsync();
        });
    }

    public void StartPositionHandlerAsync()
    {
        while (IsPlaying() && CanPlay)
        {
            Dispatcher.Invoke(() =>
            {
                SetPosition(_waveProvider.Position);
            });
            System.Threading.Thread.Sleep(100);
        }
    }

    private void SetPosition(long bytePosition, bool bForce = false)
    {
        var proportion = bytePosition / (_wem.GetDuration().TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
        _prevPositionValue = ProgressBar.Value;
        if (Math.Abs(ProgressBar.Value - proportion)*_wem.GetDuration().TotalMilliseconds < 500 || bForce)
        {
            CurrentDuration.Text = Wem.GetDurationString(TimeSpan.FromMilliseconds(proportion * _wem.GetDuration().TotalMilliseconds));
            ProgressBar.Value = proportion;
        }

    }

    public bool IsPlaying()
    {
        return _output.PlaybackState == PlaybackState.Playing;
    }

    public void Pause()
    {
        _output.Pause();
        (PlayPause.Content as TextBlock).Text = "PLAY";
        _musicLog.Debug($"Paused {_wem.Hash}");
    }
    
    public void SetVolume(double volume)
    {
        if (_waveProvider != null)
            _waveProvider.Volume = (float)volume;
    }

    private void VolumeBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var s = sender as Slider;
        SetVolume(s.Value);
    }
    
    private void ProgressBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // var s = sender as Slider;
        // // depends on the size of the song, so we scale the value to the duration of the song
        // var newPosition = _wem.GetDuration().TotalMilliseconds * s.Value;
        // if (Math.Abs(newPosition - _prevPositionValue) > 300 && IsPlaying() && s.Value != 0)  // To only take manual changes
        // {
        //     _prevPositionValue = newPosition;
        //     _output.Stop();
        //     _waveProvider.Position = (long)(s.Value * _wem.GetDuration().TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
        //     SetPosition(_waveProvider.Position);
        //     Play();
        // }
        // else if (Math.Abs(newPosition - _prevPositionValue) > 300)
        // {
        //     _prevPositionValue = newPosition;
        //     _waveProvider.Position = (long)(s.Value * _wem.GetDuration().TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
        // }
        // _prevPositionValue = newPosition;
    }

    private void PlayPause_OnClick(object sender, RoutedEventArgs e)
    {
        if (IsPlaying())
        {
            Pause();
        }
        else
        {
            Play();
        }
    }

    private void ProgressBar_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        _prevPositionValue = 0;
        var s = sender as Slider;
        if (IsPlaying())  // To only take manual changes
        {
            _output.Stop();
            _waveProvider.Position = (long)(s.Value * _wem.GetDuration().TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
            SetPosition(_waveProvider.Position, true);
            Play();
        }
        else
        {
            _waveProvider.Position = (long)(s.Value * _wem.GetDuration().TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
        }
    }

    private void ProgressBar_OnDragCompleted(object sender, DragCompletedEventArgs e)
    {
        _prevPositionValue = 0;
        var s = sender as Slider;
        if (IsPlaying())  // To only take manual changes
        {
            _output.Stop();
            _waveProvider.Position = (long)(s.Value * _wem.GetDuration().TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
            SetPosition(_waveProvider.Position, true);
            Play();
        }
        else
        {
            _waveProvider.Position = (long)(s.Value * _wem.GetDuration().TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
        }    
    }
}