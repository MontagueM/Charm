using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

public class SpinnerShader : ShaderEffect
{
    static SpinnerShader()
    {
        var uri = MakePackUri("Shaders/Spinner.fx");
        _pixelShader.UriSource = uri;
    }

    private static PixelShader _pixelShader = new PixelShader();

    public SpinnerShader()
    {
        PixelShader = _pixelShader;

        // Bind the properties to the shader registers
        UpdateShaderValue(ScreenWidthProperty);
        UpdateShaderValue(ScreenHeightProperty);
        UpdateShaderValue(TimeProperty);
        PixelShader.InvalidPixelShaderEncountered += PixelShader_InvalidPixelShaderEncountered;

        CompositionTarget.Rendering += UpdateTime;
    }

    private void PixelShader_InvalidPixelShaderEncountered(object? sender, System.EventArgs e) => System.Console.WriteLine("idk");

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
    new UIPropertyMetadata(new Point(1, 1), PixelShaderConstantCallback(3)));

    public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
    "Offset", typeof(Point), typeof(SpinnerShader),
    new UIPropertyMetadata(new Point(0, 0), PixelShaderConstantCallback(4)));

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
        ScreenWidth = (float)e.NewSize.Width;
        ScreenHeight = (float)e.NewSize.Height;
    }

    private void UpdateTime(object sender, EventArgs e)
    {
        Time = (float)(DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds;
    }
}
