using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Arithmic;

namespace Charm;

public class HolofoilShader : ShaderEffect
{
    static HolofoilShader()
    {
        var uri = MakePackUri("Shaders/Holofoil_ps.fx");
        _pixelShader.UriSource = uri;
    }

    private static PixelShader _pixelShader = new PixelShader();

    public HolofoilShader()
    {
        PixelShader = _pixelShader;

        UpdateShaderValue(TimeProperty);
        PixelShader.InvalidPixelShaderEncountered += PixelShader_InvalidPixelShaderEncountered;

        CompositionTarget.Rendering += UpdateTime;
    }

    private void PixelShader_InvalidPixelShaderEncountered(object? sender, System.EventArgs e) => Log.Error($"The Holofoil shader broke...");

    public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
    "Time", typeof(float), typeof(HolofoilShader),
    new UIPropertyMetadata(0.0f, PixelShaderConstantCallback(0)));


    public float Time
    {
        get => (float)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public static System.Uri MakePackUri(string relativeFile)
    {
        System.Reflection.Assembly a = typeof(HolofoilShader).Assembly;
        string assemblyShortName = a.ToString().Split(',')[0];
        string uriString = "pack://application:,,,/" + assemblyShortName + ";component/" + relativeFile;
        return new System.Uri(uriString);
    }

    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private float _lastTime;
    private double _lastFrameTime;
    private void UpdateTime(object sender, EventArgs e)
    {
        double now = _stopwatch.Elapsed.TotalSeconds;
        if (now - _lastFrameTime < 1.0 / 60.0)
            return;

        _lastFrameTime = now;
        float t = (float)now;

        if (Math.Abs(t - _lastTime) < 0.0001f)
            return;

        _lastTime = t;
        Time = t;
    }
}
