using System;
using System.Collections.Generic;
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
    private WwiseSound _sound;
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

    public bool SetWem(Wem wem)
    {
        if (_output != null)
            _output.Dispose();
        _wem = wem;
        _waveProvider = wem.MakeWaveChannel();
        if (_waveProvider == null)
        {
            CanPlay = false;
            _musicLog.Error("WaveProvider is null");
            MessageBox.Show("Error: WaveProvider is null");
            return false;
        }
        MakeOutput();
        _output.Init(_waveProvider);
        SetVolume(VolumeBar.Value);
        CanPlay = true;
        CurrentDuration.Text = Wem.GetDurationString(_waveProvider.CurrentTime);  // todo make this all correct
        TotalDuration.Text = wem.Duration;
        _prevPositionValue = 0;
        ProgressBar.Value = 0;
        SetPlayingText(wem.Hash);
        return true;
    }
    
    public async Task SetSound(WwiseSound sound)
    {
        if (_output != null)
            _output.Dispose();
        _sound = sound;
        if (sound.Header.Unk20.Count > 10)
        {
            MainWindow.Progress.SetProgressStages(new List<string>
            {
                $"loading sound {sound.Hash}",
            });
            await Task.Run(() => 
            {
                _waveProvider = sound.MakeWaveChannel();
            });
            MainWindow.Progress.CompleteStage();            
        }
        else
        {
            _waveProvider = sound.MakeWaveChannel();
        }

        MakeOutput();
        _output.Init(_waveProvider);
        SetVolume(VolumeBar.Value);
        CanPlay = true;
        CurrentDuration.Text = Wem.GetDurationString(_waveProvider.CurrentTime);  // todo make this all correct
        TotalDuration.Text = sound.Duration;
        _prevPositionValue = 0;
        ProgressBar.Value = 0;
        SetPlayingText(sound.Hash);
    }
    
    public void Play()
    {
        if (_output == null)
        {
            _musicLog.Error("Output is null");
            return;
        }
        string name = _wem == null ? _sound.Hash : _wem.Hash;
        _musicLog.Information($"Playing {name}");
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
                if (IsPlaying())
                    SetPosition(_waveProvider.Position);
            });
            System.Threading.Thread.Sleep(100);
        }
    }

    private void SetPosition(long bytePosition, bool bForce = false)
    {
        var duration = _wem == null ? _sound.GetDuration() : _wem.GetDuration();
        var proportion = bytePosition / (duration.TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
        _prevPositionValue = ProgressBar.Value;
        if (Math.Abs(ProgressBar.Value - proportion)*duration.TotalMilliseconds < 500 || bForce)
        {
            CurrentDuration.Text = Wem.GetDurationString(TimeSpan.FromMilliseconds(proportion * duration.TotalMilliseconds));
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
        string name = _wem == null ? _sound.Hash : _wem.Hash;
        _musicLog.Debug($"Paused {name}");
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
        if (_wem == null && _sound == null)
            return;

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
        var duration = _wem == null ? _sound.GetDuration() : _wem.GetDuration();
        var s = sender as Slider;
        if (IsPlaying())  // To only take manual changes
        {
            _output.Stop();
            _waveProvider.Position = (long)(s.Value * duration.TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
            SetPosition(_waveProvider.Position, true);
            Play();
        }
        else
        {
            _waveProvider.Position = (long)(s.Value * duration.TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
        }
    }

    private void ProgressBar_OnDragCompleted(object sender, DragCompletedEventArgs e)
    {
        _prevPositionValue = 0;
        var duration = _wem == null ? _sound.GetDuration() : _wem.GetDuration();
        var s = sender as Slider;
        if (IsPlaying())  // To only take manual changes
        {
            _output.Stop();
            _waveProvider.Position = (long)(s.Value * duration.TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
            SetPosition(_waveProvider.Position, true);
            Play();
        }
        else
        {
            _waveProvider.Position = (long)(s.Value * duration.TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
        }    
    }

    public void Dispose()
    {
        CanPlay = false;
        _output?.Stop();
        _waveProvider?.Dispose();
        _output?.Dispose();
        _wem?.Dispose();
    }
}