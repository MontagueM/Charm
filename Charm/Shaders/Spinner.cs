using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Charm;

public class SpinnerShader : ShaderEffect
{
    private static PixelShader _pixelShader = new();
    private DispatcherTimer _renderTimer;

    static SpinnerShader()
    {
        Uri uri = MakePackUri("Shaders/Spinner.fx");
        _pixelShader.UriSource = uri;
    }

    public SpinnerShader()
    {
        PixelShader = _pixelShader;

        // Bind the properties to the shader registers
        UpdateShaderValue(ScreenWidthProperty);
        UpdateShaderValue(ScreenHeightProperty);
        UpdateShaderValue(TimeProperty);
        PixelShader.InvalidPixelShaderEncountered += PixelShader_InvalidPixelShaderEncountered;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            // Limiting the spinner fps just to save some gpu usage
            _renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000 / 90) // 90 fps
            };
            _renderTimer.Tick += (s, args) => UpdateTime();
            _renderTimer.Start();

        }), System.Windows.Threading.DispatcherPriority.Loaded);

        //CompositionTarget.Rendering += UpdateTime;
    }

    private void PixelShader_InvalidPixelShaderEncountered(object? sender, System.EventArgs e) => System.Console.WriteLine($"The spinner shader broke somehow");

    // Define the dependency property
    public static readonly DependencyProperty ScreenWidthProperty = DependencyProperty.Register(
     "ScreenWidth", typeof(float), typeof(SpinnerShader),
     new UIPropertyMetadata(1920.0f, PixelShaderConstantCallback(0)));

    public static readonly DependencyProperty ScreenHeightProperty = DependencyProperty.Register(
    "ScreenHeight", typeof(float), typeof(SpinnerShader),
    new UIPropertyMetadata(1080.0f, PixelShaderConstantCallback(1)));

    public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
    "Time", typeof(float), typeof(SpinnerShader),
    new UIPropertyMetadata(0.0f, PixelShaderConstantCallback(2)));

    public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
    "Scale", typeof(Point), typeof(SpinnerShader),
    new UIPropertyMetadata(new Point(2, 2), PixelShaderConstantCallback(3)));

    public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
    "Offset", typeof(Point), typeof(SpinnerShader),
    new UIPropertyMetadata(new Point(-1, -1), PixelShaderConstantCallback(4)));

    public float ScreenWidth
    {
        get => (float)GetValue(ScreenWidthProperty);
        set => SetValue(ScreenWidthProperty, value);
    }

    public float ScreenHeight
    {
        get => (float)GetValue(ScreenHeightProperty);
        set => SetValue(ScreenHeightProperty, value);
    }

    public float Time
    {
        get => (float)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public Point Scale
    {
        get => (Point)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public Point Offset
    {
        get => (Point)GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public static System.Uri MakePackUri(string relativeFile)
    {
        System.Reflection.Assembly a = typeof(SpinnerShader).Assembly;
        string assemblyShortName = a.ToString().Split(',')[0];
        string uriString = "pack://application:,,,/" + assemblyShortName + ";component/" + relativeFile;
        return new System.Uri(uriString);
    }

    public void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var width = MainWindow.Current.ActualWidth;
        var height = MainWindow.Current.ActualHeight;

        ScreenWidth = (float)width;
        ScreenHeight = (float)height;

        Console.WriteLine($"{ScreenWidth}x{ScreenHeight}");
    }

    private void UpdateTime()//(object sender, EventArgs e)
    {
        Time = (float)(DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds;
    }
}
