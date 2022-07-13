using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Field;
using NAudio.Wave;

namespace Charm;

public partial class MusicPlayerControl : UserControl
{
    private NAudio.Wave.WaveOutEvent _output;
    private Wem _wem;
    private WaveChannel32 _waveProvider;
    private bool _bAlreadySet;
    public bool CanPlay { get; set; } = false;
    private double _prevPositionValue = 0;

    
    public MusicPlayerControl()
    {
        InitializeComponent();
        _output = new WaveOutEvent();
        SetVolume(VolumeBar.Value);
    }

    public void SetWem(Wem wem)
    {
        if (_output != null)
        {
            _output.Dispose();
            _output = new WaveOutEvent();
        }
        _wem = wem;
        _waveProvider = wem.MakeWaveChannel();
        
        _output.Init(_waveProvider);
        CanPlay = true;
        CurrentDuration.Text = Wem.GetDurationString(_waveProvider.CurrentTime);
        TotalDuration.Text = wem.Duration;
        _prevPositionValue = 0;
    }
    
    public void Play()
    {
        _output.Play();
        StartPositionHandlerAsync();
    }

    public void StartPositionHandlerAsync()
    {
        var duration = _wem.GetDuration().TotalMilliseconds;
        Task.Run(() =>
        {
            double prevPosition = _waveProvider.CurrentTime.TotalMilliseconds;
            while (IsPlaying())
            {
                if (_waveProvider.CurrentTime.TotalMilliseconds - prevPosition < 0.1 && CanPlay)
                {
                    Dispatcher.Invoke(() =>
                    {
                        SetPosition(_waveProvider.CurrentTime, duration);
                    });
                }
                prevPosition = _waveProvider.CurrentTime.TotalMilliseconds;
                System.Threading.Thread.Sleep(50);
            }
        });
    }

    private void SetPosition(TimeSpan timeSpan, double duration)
    {
        if (_bAlreadySet)
        {
            _bAlreadySet = false;
            System.Threading.Thread.Sleep(50);
            return;
        }
        CurrentDuration.Text = Wem.GetDurationString(timeSpan);
        ProgressBar.Value = timeSpan.TotalMilliseconds / duration;
    }

    public bool IsPlaying()
    {
        return _output.PlaybackState == PlaybackState.Playing;
    }

    public void Pause()
    {
        _output.Pause();
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
        var s = sender as Slider;
        if (Math.Abs(s.Value - _prevPositionValue) > 0.1 && CanPlay && s.Value != 0)  // To only take manual changes
        {
            _prevPositionValue = s.Value;
            _output.Stop();
            var nt = TimeSpan.FromMilliseconds(s.Value * _wem.GetDuration().TotalMilliseconds);
            // _waveProvider.CurrentTime = nt;
            _waveProvider.Position = (long)(s.Value * _wem.GetDuration().TotalSeconds * _waveProvider.WaveFormat.AverageBytesPerSecond);
            SetPosition(nt, _wem.GetDuration().TotalMilliseconds);
            // var x = 0;
            Play();
            // Task.Run(async () =>
            // {
            //     await Task.Delay(50);
            //     Play();
            // });
        }
        _prevPositionValue = s.Value;
    }
}