using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Arithmic;
using SharpDX;

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using Buffer = SharpDX.Direct3D11.Buffer;
using PixelShader = SharpDX.Direct3D11.PixelShader;
using VertexShader = SharpDX.Direct3D11.VertexShader;

namespace Charm;

public partial class Spinner2 : UserControl, IDisposable
{
    private Device5 _d3d11Device;
    private DeviceContext4 _d3d11Context;
    private Texture2D1 _renderTexture;
    private Texture2D1 _displayTexture;
    private RenderTargetView _renderView;
    private Direct3DEx _direct3dEx;
    private DeviceEx _deviceEx;
    private Texture _direct3D9Texture;
    private D3DImage _renderedImage;
    private SharpDX.Direct3D11.VertexShader _vertexShader;
    private SharpDX.Direct3D11.PixelShader _pixelShader;
    private Buffer _constantBuffer;
    private Buffer _posScaleConstantBuffer;
    private Stopwatch clock;

    private int _width;
    private int _height;
    private Vector2 _invResolution;

    private DispatcherTimer _renderTimer;

    public Vector4 PositionScale = new(2, 2, -1, -1);

    public Spinner2()
    {
        Load((int)Width, (int)Height);
    }

    public Spinner2(int width, int height)
    {
        Load(width, height);
    }

    private void Load(int width, int height)
    {
        if (_d3d11Device == null)
        {
            InitialSetup();
        }

        InitializeComponent();
        _width = width;
        _height = height;

        Loaded += ShaderRenderHost_Loaded;
        SizeChanged += ShaderRenderHost_SizeChanged;
        Unloaded += (_, _) => DisposeControl();

        UIHelper.AnimateFade(D3DImageHost, 0.5f);
    }

    private void ShaderRenderHost_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize device & context ONCE
        if (_d3d11Device == null)
        {
            InitialSetup();
        }

        Dispatcher.BeginInvoke(new Action(() =>
        {
            CreateRenderingResources((int)ActualWidth, (int)ActualHeight);
            CompositionTarget.Rendering += OnRendering;

            //// Limiting the spinner fps just to save some gpu usage
            //_renderTimer = new DispatcherTimer
            //{
            //    Interval = TimeSpan.FromMilliseconds(1000 / 90) // 90 fps
            //};
            //_renderTimer.Tick += (s, args) => Render();
            //_renderTimer.Start();

        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private DateTime _lastRender = DateTime.MinValue;
    private readonly double _targetFrameTime = 1000.0 / 90; // 90 FPS
    private void OnRendering(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        if ((now - _lastRender).TotalMilliseconds < _targetFrameTime)
            return; // Skip this frame

        _lastRender = now;
        Render();
    }

    private void ShaderRenderHost_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        int newWidth = Math.Max(1, (int)ActualWidth);
        int newHeight = Math.Max(1, (int)ActualHeight);

        if (newWidth != _width || newHeight != _height)
        {
            _width = newWidth;
            _height = newHeight;
            _invResolution = new Vector2(1f / _width, 1f / _height);
            CreateRenderingResources(_width, _height);
        }
    }

    private void InitialSetup()
    {
        var creationFlags = DeviceCreationFlags.BgraSupport;
        var featureLevels = new[]
        {
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_11_1,
        };

        using (var device = new SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags, featureLevels))
        {
            _d3d11Device = device.QueryInterface<Device5>();
            _d3d11Context = _d3d11Device.ImmediateContext.QueryInterface<DeviceContext4>();
        }
    }

    private void CreateRenderingResources(int imageWidth, int imageHeight)
    {
        try
        {
            if (imageWidth <= 0 || imageHeight <= 0)
                return;

            DisposeRenderingResources();
            _renderedImage?.Lock();
            _renderedImage?.Unlock();

            // Start creating the textures
            _renderTexture = new SharpDX.Direct3D11.Texture2D1(_d3d11Device, new SharpDX.Direct3D11.Texture2DDescription1
            {
                Width = imageWidth,
                Height = imageHeight,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | SharpDX.Direct3D11.BindFlags.ShaderResource,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None
            });

            _renderView = new RenderTargetView(_d3d11Device, _renderTexture);
            _d3d11Context.OutputMerger.SetTargets(_renderView);

            _displayTexture = new SharpDX.Direct3D11.Texture2D1(_d3d11Device, new SharpDX.Direct3D11.Texture2DDescription1
            {
                Width = imageWidth,
                Height = imageHeight,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | SharpDX.Direct3D11.BindFlags.ShaderResource,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.Shared
            });

            using var resource = _displayTexture.QueryInterface<SharpDX.DXGI.Resource1>();
            var sharedHandle = resource.SharedHandle;

            var windowRef = new WindowInteropHelper(Window.GetWindow(this)).Handle;

            var presenterParams = new SharpDX.Direct3D9.PresentParameters
            {
                Windowed = true,
                SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
                DeviceWindowHandle = windowRef,
                PresentationInterval = SharpDX.Direct3D9.PresentInterval.One,
            };

            _direct3dEx = new SharpDX.Direct3D9.Direct3DEx();
            _deviceEx = new SharpDX.Direct3D9.DeviceEx(_direct3dEx, 0, SharpDX.Direct3D9.DeviceType.Hardware, IntPtr.Zero, SharpDX.Direct3D9.CreateFlags.HardwareVertexProcessing, presenterParams);

            _direct3D9Texture = new SharpDX.Direct3D9.Texture(_deviceEx, _displayTexture.Description.Width, _displayTexture.Description.Height, 1, SharpDX.Direct3D9.Usage.RenderTarget, SharpDX.Direct3D9.Format.A8R8G8B8, SharpDX.Direct3D9.Pool.Default, ref sharedHandle);

            // This will contain the output image on each render cycle, bind it to an Image control for example.  
            _renderedImage = new D3DImage(96, 96);

            using (var sur = _direct3D9Texture.GetSurfaceLevel(0))
            {
                _renderedImage.Lock();
                _renderedImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, sur.NativePointer);
                _renderedImage.AddDirtyRect(new Int32Rect(0, 0, imageWidth, imageHeight));
                _renderedImage.Unlock();
            }

            // Compile Vertex and Pixel shaders
            MemoryStream stream = new MemoryStream(File.ReadAllBytes("shaders/procedural_spinner.vs.cso"));
            var vertexShaderByteCode = SharpDX.D3DCompiler.ShaderBytecode.Load(stream);
            _vertexShader = new VertexShader(_d3d11Device, vertexShaderByteCode);
            stream.Dispose();

            stream = new MemoryStream(File.ReadAllBytes("shaders/procedural_spinner_dark.ps.cso"));
            var pixelShaderByteCode = SharpDX.D3DCompiler.ShaderBytecode.Load(stream);
            _pixelShader = new PixelShader(_d3d11Device, pixelShaderByteCode);
            stream.Dispose();

            // Prepare All the stages
            _d3d11Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            _d3d11Context.VertexShader.Set(_vertexShader);
            _d3d11Context.PixelShader.Set(_pixelShader);
            _d3d11Context.Rasterizer.SetViewport(0, 0, imageWidth, imageHeight, 0.0f, 1.0f);
            _d3d11Context.OutputMerger.SetTargets(_renderView);

            // Create Constant Buffer
            _constantBuffer = new Buffer(_d3d11Device, Utilities.SizeOf<Vector4>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            _posScaleConstantBuffer = new Buffer(_d3d11Device, Utilities.SizeOf<Vector4>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            _d3d11Context.VertexShader.SetConstantBuffer(0, _constantBuffer);
            _d3d11Context.PixelShader.SetConstantBuffer(0, _posScaleConstantBuffer);

            if (clock is null)
                clock = new();

            clock.Start();
        }
        catch (Exception e)
        {
            Log.Error($"Something went wrong trying to render the Spinner: {e.Message}");
        }
    }

    private void Render()//Render(object? sender, EventArgs e)
    {
        try
        {
            _d3d11Context.OutputMerger.SetTargets(_renderView);
            _d3d11Context.ClearRenderTargetView(_renderView, new RawColor4(0f, 0f, 0f, 1f));

            // Update the cbuffer with the inverse rez and time
            Vector4 invTime = new Vector4(_invResolution.X, _invResolution.Y, clock.ElapsedMilliseconds / 1000f, 0);
            _d3d11Context.UpdateSubresource(ref invTime, _constantBuffer);
            _d3d11Context.UpdateSubresource(ref PositionScale, _posScaleConstantBuffer);

            // Draw
            _d3d11Context.Draw(6, 0);

            //IMPORTANT: You need to manually copy the resource from the render texture to the display texture, to avoid artifacts (double buffer).  
            //Also, flush so your D3D9 resource gets the rendered content.  
            _d3d11Device!.ImmediateContext.CopyResource(_renderTexture, _displayTexture);
            _d3d11Device.ImmediateContext.Flush();

            //Call Lock(),AddDirtyRect(), Unlock() in a dispatcher call if you need the screen updated with the new image.
            Dispatcher.BeginInvoke(() =>
            {
                _renderedImage.Lock();
                _renderedImage.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
                _renderedImage.Unlock();
                D3DImageHost.Source = _renderedImage;
            });
        }
        catch (Exception e)
        {
            Log.Error($"Something went wrong trying to render the Spinner: {e.Message}");
        }
    }

    public void DisposeRenderingResources()
    {
        _constantBuffer?.Dispose();
        _vertexShader?.Dispose();
        _pixelShader?.Dispose();
        _renderTexture?.Dispose();
        _displayTexture?.Dispose();
        _direct3D9Texture?.Dispose();
        _renderedImage = null;
        _renderTexture = null;
        _displayTexture = null;
    }

    public void DisposeControl()
    {
        CompositionTarget.Rendering -= OnRendering;
        Loaded -= ShaderRenderHost_Loaded;
        SizeChanged -= ShaderRenderHost_SizeChanged;

        DisposeRenderingResources();
        _renderTimer?.Stop();
        _renderTimer = null;

        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        DisposeControl();
        GC.SuppressFinalize(this);
    }
}
