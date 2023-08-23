using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DirectXTexNet;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;

namespace AtlasSharp;

public enum MoveDirection : int
{
    None,
    Forward,
    Backward,
    Left,
    Right,
    Up,
    Down
};

[StructLayout(LayoutKind.Sequential)]
public struct Transform
{
    public Vector4 Rotation;
    public Vector4 Position;
    public float Scale;
}

[StructLayout(LayoutKind.Sequential)]
public struct StaticMeshInstancedInfo
{
    public Vector4 MeshTransform;
    public Vector4 TexcoordTransform;
    public Transform[] InstanceTransforms;
}

public partial class AtlasView : UserControl
{
    private Point _centrePosition;

    public AtlasView()
    {
        InitializeComponent();

        ImageHost.Loaded += ImageHost_Loaded;
        ImageHost.Unloaded += ImageHost_Unloaded;
        ImageHost.SizeChanged += Host_SizeChanged;
    }

    private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _centrePosition = GetCentrePositionAbsolute();
    }

    private Point GetCentrePositionAbsolute()
    {
        PresentationSource source = PresentationSource.FromVisual(this);
        return new Point(source.CompositionTarget.TransformToDevice.M11*(Window.GetWindow(this).Left + ActualWidth / 2), source.CompositionTarget.TransformToDevice.M22*(Window.GetWindow(this).Top + ActualHeight / 2));
    }

    private Point GetCentrePositionWindow()
    {
        return new Point(ActualWidth / 2, ActualHeight / 2);
    }

    private void ImageHost_Unloaded(object sender, RoutedEventArgs e)
    {
    }

    private void ImageHost_Loaded(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this).PreviewKeyDown += MainWindow_OnPreviewKeyDown;
        Window.GetWindow(this).PreviewKeyUp += MainWindow_OnPreviewKeyUp;

        // InitializeRendering();
    }

    private Point _currentMousePositionAbsolute = new();
    MoveDirection _direction = MoveDirection.None;

    public void LoadStatic(FileHash staticHash, Window? window = null)
    {
        InitializeRendering(staticHash, window);
    }

    private void InitializeRendering(FileHash staticHash, Window? window = null)
    {
        InteropImage.WindowOwner = (new System.Windows.Interop.WindowInteropHelper(window ?? Window.GetWindow(this))).Handle;


        // WindowOwner is a uint
        int result = NativeMethods.Init(InteropImage.WindowOwner);

        if (result != 0)
        {
            throw new Exception();
        }

        InitStaticMesh(staticHash);



        InteropImage.OnRender = DoRender;
        InteropImage.SetPixelSize((int)ImageHost.Width, (int)ImageHost.Height);


        CompositionTarget.Rendering += (sender, args) =>
        {
            // get delta between centre of screen and mouse position, then reset mouse position to centre of screen
            if (_isMouseCaptured)
            {
                var centre = GetCentrePositionAbsolute();
                var delta = _currentMousePositionAbsolute - centre;
                float sensitivity = 1.0f;
                NativeMethods.RegisterMouseDelta((float)(delta.X * sensitivity), (float)(delta.Y * sensitivity));

                _currentMousePositionAbsolute = centre;
                SetCursorPosition(centre);

                NativeMethods.MoveCamera(_direction);
            }

            InteropImage.RequestRender();
        };

        // Start rendering now!
         InteropImage.RequestRender();
    }

    private void InitStaticMesh(FileHash staticHash)
    {
        StaticMesh staticMesh = FileResourcer.Get().GetFile<StaticMesh>(staticHash);

        Blob staticMeshTransforms = staticMesh.TagData.StaticData.GetTransformsBlob();
        NativeMethods.CreateStaticMesh(staticHash, staticMeshTransforms);


        List<StaticPart> parts = staticMesh.Load(ExportDetailLevel.MostDetailed);
        List<BufferGroup> bufferGroups = staticMesh.TagData.StaticData.GetBuffers();
        List<int> strides = staticMesh.TagData.StaticData.GetStrides();
        foreach (BufferGroup bufferGroup in bufferGroups)
        {
            NativeMethods.AddStaticMeshBufferGroup(staticHash, bufferGroup);
            // bufferGroup.VertexBuffers[0].TempDump("0");
            // bufferGroup.VertexBuffers[1].TempDump("1");
            // bufferGroup.VertexBuffers[2].TempDump("2");
        }
        int partIndex = 0;

        foreach (StaticPart part in parts)
        {
            if (part.Material == null)
            {
                continue;
            }

            // part.Material.VertexShader.TempDumpRef();

            PartMaterial partMaterial = new(part.Material, strides);
            PartInfo partInfo = new() {IndexOffset = part.IndexOffset, IndexCount = part.IndexCount, Material = partMaterial};
            NativeMethods.CreateStaticMeshPart(staticHash, partInfo);
            partIndex++;
        }
    }

    public struct InputSignature
    {
        public InputSemantic Semantic;
        public int SemanticIndex;
        public int DxgiFormat;
        public int BufferIndex;
    }

    public struct PartMaterial
    {
        public Blob VSBytecode;
        public Blob PSBytecode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public InputSignature[] InputSignatures;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Blob[] VSTextures;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Blob[] PSTextures;
        public Blob PScb0;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public DirectXSampler.D3D11_SAMPLER_DESC[] VSSamplers;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public DirectXSampler.D3D11_SAMPLER_DESC[] PSSamplers;

        public PartMaterial(IMaterial material, List<int> strides)
        {
            if (material.VertexShader == null || material.PixelShader == null)
            {
                throw new Exception();
            }
            VSBytecode = new Blob(material.VertexShader.GetBytecode());
            PSBytecode = new Blob(material.PixelShader.GetBytecode());

            InputSignatures = new InputSignature[8];
            int sigIndex = 0;

            Tiger.Schema.InputSignature[] inputSignatures = material.VertexShader.InputSignatures.ToArray();
            Helpers.DecorateSignaturesWithBufferIndex(ref inputSignatures, strides); // absorb into the getter probs

            foreach (Tiger.Schema.InputSignature signature in inputSignatures)
            {
                InputSignatures[sigIndex].Semantic = signature.Semantic;
                InputSignatures[sigIndex].SemanticIndex = (int)signature.SemanticIndex;
                InputSignatures[sigIndex].BufferIndex = signature.BufferIndex;
                switch (signature.Mask)
                {
                    case ComponentMask.XYZW:
                        if (signature.Semantic == InputSemantic.Colour)
                        {
                            InputSignatures[sigIndex].DxgiFormat = (int)DXGI_FORMAT.R8G8B8A8_UNORM;
                            break;
                        }

                        InputSignatures[sigIndex].DxgiFormat = signature.ComponentType switch
                        {
                            RegisterComponentType.Float32 => (int)DXGI_FORMAT.R16G16B16A16_SNORM,
                            RegisterComponentType.SInt32 => (int)DXGI_FORMAT.R16G16B16A16_SINT,
                            RegisterComponentType.UInt32 => (int)DXGI_FORMAT.R16G16B16A16_UINT,
                            _ => throw new Exception()
                        };
                        break;
                    case ComponentMask.XYZ:
                        InputSignatures[sigIndex].DxgiFormat = signature.ComponentType switch
                        {
                            RegisterComponentType.Float32 => (int)DXGI_FORMAT.R32G32B32_FLOAT,
                            _ => throw new Exception()
                        };
                        break;
                    case ComponentMask.XY:
                        InputSignatures[sigIndex].DxgiFormat = signature.ComponentType switch
                        {
                            RegisterComponentType.Float32 => (int)DXGI_FORMAT.R16G16_SNORM,
                            RegisterComponentType.SInt32 => (int)DXGI_FORMAT.R16G16_SINT,
                            RegisterComponentType.UInt32 => (int)DXGI_FORMAT.R16G16_UINT,
                            _ => throw new Exception()
                        };
                        break;
                    case ComponentMask.X:
                        InputSignatures[sigIndex].DxgiFormat = signature.ComponentType switch
                        {
                            RegisterComponentType.Float32 => (int)DXGI_FORMAT.R32_FLOAT,
                            RegisterComponentType.SInt32 => (int)DXGI_FORMAT.R16_SINT,
                            RegisterComponentType.UInt32 => (int)DXGI_FORMAT.R16_UINT,
                            _ => throw new Exception()
                        };
                        break;
                    default:
                        throw new Exception();
                }
                sigIndex++;
            }

            VSTextures = new Blob[16];
            foreach (STextureTag vsTexture in material.EnumerateVSTextures())
            {
                byte[] final = vsTexture.Texture.GetDDSBytes();
                VSTextures[vsTexture.TextureIndex] = new Blob(final);
            }

            PSTextures = new Blob[16];
            foreach (STextureTag psTexture in material.EnumeratePSTextures())
            {
                byte[] final = psTexture.Texture.GetDDSBytes();
                PSTextures[psTexture.TextureIndex] = new Blob(final);
            }

            if (material.PSVector4Container.IsValid())
            {
                TigerFile container = FileResourcer.Get().GetFile(material.PSVector4Container.GetReferenceHash());
                PScb0 = new Blob(container.GetData());
            }
            else
            {
                PScb0 = new Blob();
            }

            VSSamplers = new DirectXSampler.D3D11_SAMPLER_DESC[8];
            for (int i = 0; i < material.VS_Samplers.Count; i++)
            {
                SDirectXSamplerTag vsSampler = material.VS_Samplers[i];
                VSSamplers[i] = vsSampler.Samplers.Sampler;
            }

            PSSamplers = new DirectXSampler.D3D11_SAMPLER_DESC[8];
            for (int i = 0; i < material.PS_Samplers.Count; i++)
            {
                SDirectXSamplerTag psSampler = material.PS_Samplers[i];
                PSSamplers[i] = psSampler.Samplers.Sampler;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PartInfo
    {
        public uint IndexOffset;
        public uint IndexCount;
        public PartMaterial Material;
    }

    private void DoRender(IntPtr surface, bool isNewSurface)
    {
        NativeMethods.Render(surface, isNewSurface);
    }

    private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.W:
                _direction = MoveDirection.Forward;
                break;
            case Key.S:
                _direction = MoveDirection.Backward;
                break;
            case Key.A:
                _direction = MoveDirection.Left;
                break;
            case Key.D:
                _direction = MoveDirection.Right;
                break;
            case Key.Q:
                _direction = MoveDirection.Up;
                break;
            case Key.E:
                _direction = MoveDirection.Down;
                break;
            case Key.Escape:
                _direction = MoveDirection.None;
                _isMouseCaptured = false;
                Cursor = Cursors.Arrow;
                break;
            default:
                _direction = MoveDirection.None;
                return;
        }
    }

    private void MainWindow_OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        _direction = MoveDirection.None;
    }

    private bool _isMouseCaptured = false;

    /// <summary>
    /// Struct representing a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Point(POINT point) {
            return new Point(point.X, point.Y);
        }
    }

    // dpi correct already
    private void MainWindow_OnMouseMove(object sender, MouseEventArgs e)
    {
        GetCursorPos(out POINT point);
        _currentMousePositionAbsolute = point;
    }

    private void SetCursorPosition(Point position)
    {
        SetCursorPos((int)position.X, (int)position.Y);
    }

    [DllImport("User32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("User32.dll")]
    private static extern bool GetCursorPos(out POINT position);

    private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right)
        {
            _isMouseCaptured = true;
            Cursor = Cursors.None;
        }
    }

    private void MainWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right)
        {
            _isMouseCaptured = false;
            Cursor = Cursors.Arrow;
        }
    }
}

