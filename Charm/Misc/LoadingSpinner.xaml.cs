using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Charm;

public partial class LoadingSpinner : UserControl
{
    private static readonly ImageSource[] Frames =
    {
        new BitmapImage(UIHelper.MakePackUri("/Assets/icons/loading/load0.png")),
        new BitmapImage(UIHelper.MakePackUri("/Assets/icons/loading/load1.png")),
        new BitmapImage(UIHelper.MakePackUri("/Assets/icons/loading/load2.png")),
        new BitmapImage(UIHelper.MakePackUri("/Assets/icons/loading/load3.png")),
        new BitmapImage(UIHelper.MakePackUri("/Assets/icons/loading/load4.png")),
        new BitmapImage(UIHelper.MakePackUri("/Assets/icons/loading/load5.png")),
    };

    private DispatcherTimer _timer;
    private int _nextIndex;
    private bool _aIsFront = true;

    public LoadingSpinner()
    {
        InitializeComponent();
        ImageA.Source = Frames[0];
        _nextIndex = 1 % Frames.Length;
        IsVisibleChanged += OnIsVisibleChanged;
    }

    public double FramesPerSecond
    {
        get => (double)GetValue(FramesPerSecondProperty);
        set => SetValue(FramesPerSecondProperty, value);
    }
    public static readonly DependencyProperty FramesPerSecondProperty = DependencyProperty.Register(nameof(FramesPerSecond), typeof(double), typeof(LoadingSpinner), new PropertyMetadata(4.0));

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
            Start();
        else
            Stop();
    }

    private void Start()
    {
        Stop();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.0 / FramesPerSecond) };
        _timer.Tick += (_, __) => NextFrame();
        _timer.Start();
    }

    private void Stop()
    {
        _timer?.Stop();
        _timer = null;
    }

    private void NextFrame()
    {
        var front = _aIsFront ? ImageA : ImageB;
        var back = _aIsFront ? ImageB : ImageA;

        back.Source = Frames[_nextIndex];
        _nextIndex = (_nextIndex + 1) % Frames.Length;

        var fadeDuration = TimeSpan.FromSeconds(1.0 / FramesPerSecond);
        front.BeginAnimation(OpacityProperty, new DoubleAnimation(0, fadeDuration));
        back.BeginAnimation(OpacityProperty, new DoubleAnimation(1, fadeDuration));

        _aIsFront = !_aIsFront;
    }
}
