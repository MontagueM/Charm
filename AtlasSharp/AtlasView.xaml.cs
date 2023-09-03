using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Arithmic;
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
    private bool _initialisedRenderer = false;

    public AtlasView()
    {
        InitializeComponent();

        ImageHost.Loaded += ImageHost_Loaded;
        ImageHost.Unloaded += ImageHost_Unloaded;
        SizeChanged += OnSizeChanged;
    }

    public void OnLocationChanged(object? sender, EventArgs e)
    {
        _centrePosition = GetCentrePositionAbsolute();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _centrePosition = GetCentrePositionAbsolute();
        if (_initialisedRenderer)
        {
            NativeMethods.ResizeWindow((int)ActualWidth, (int)ActualHeight);
        }}

    private Point GetCentrePositionAbsolute()
    {
        PresentationSource source = PresentationSource.FromVisual(this);
        var window = Window.GetWindow(this);

        var parentTransform = TransformToAncestor(Window.GetWindow(this)).Transform(new Point());
        var offsetX = window.Left + parentTransform.X + ActualWidth / 2;
        var offsetY = window.Top + parentTransform.Y + ActualHeight / 2;

        var dpiScaledX = offsetX * source.CompositionTarget.TransformToDevice.M11;
        var dpiScaledY = offsetY * source.CompositionTarget.TransformToDevice.M22;

        return new Point(dpiScaledX, dpiScaledY);
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
        Window.GetWindow(this).LocationChanged += OnLocationChanged;

        // InitializeRendering();
    }

    private Point _currentMousePositionAbsolute = new();
    MoveDirection _direction = MoveDirection.None;
    private bool _shouldRender = false;
    private List<PartInfo> _partInfos = new();

    public void LoadStatic(FileHash staticHash, Window? window = null)
    {
        Cleanup();
        if (!_initialisedRenderer)
        {
            InitializeRendering(window);
            _initialisedRenderer = true;
        }

        if (!InitStaticMesh(staticHash))
        {
            InteropImage.OnRender = null;
            _shouldRender = false;
            return;
        }

        InteropImage.OnRender = DoRender;
        InteropImage.SetPixelSize((int)ActualWidth, (int)ActualHeight);
        InteropImage.RequestRender();
        _shouldRender = true;
    }

    public void LoadMap(FileHash mapHash, Window? window = null)
    {
        Cleanup();
        if (!_initialisedRenderer)
        {
            InitializeRendering(window);
            _initialisedRenderer = true;
        }

        if (!InitMap(mapHash))
        {
            InteropImage.OnRender = null;
            _shouldRender = false;
            return;
        }

        InteropImage.OnRender = DoRender;
        InteropImage.SetPixelSize((int)ActualWidth, (int)ActualHeight);
        InteropImage.RequestRender();
        _shouldRender = true;
    }

    private void Cleanup()
    {
        NativeMethods.Cleanup();

        foreach (var part in _partInfos)
        {
            part.Material.PSSamplers.ToList().ForEach(s => s.Dispose());
            part.Material.VSSamplers.ToList().ForEach(s => s.Dispose());
            part.Material.PSTextures.ToList().ForEach(t => t.Dispose());
            part.Material.VSTextures.ToList().ForEach(t => t.Dispose());
            part.Material.PScb0.Dispose();
            part.Material.PSBytecode.Dispose();
            part.Material.VSBytecode.Dispose();
        }
        _partInfos.Clear();
    }

    private void InitializeRendering(Window? window = null)
    {
        InteropImage.WindowOwner = (new System.Windows.Interop.WindowInteropHelper(window ?? Window.GetWindow(this))).Handle;


        // WindowOwner is a uint
        long result = NativeMethods.Init(InteropImage.WindowOwner, (int)ActualWidth, (int)ActualHeight);

        if (result != 0)
        {
            MessageBox.Show("Failed to initialise renderer");
        }


        CompositionTarget.Rendering += (sender, args) =>
        {
            // get delta between centre of screen and mouse position, then reset mouse position to centre of screen
            if (_isMouseCaptured)
            {
                var centre = GetCentrePositionAbsolute();
                var delta = _currentMousePositionAbsolute - centre;
                float sensitivity = 1.0f;
                if (_movingOrbitOrigin)
                {
                    NativeMethods.MoveOrbitOrigin((float)(delta.X * sensitivity), (float)(delta.Y * sensitivity));
                }
                else
                {
                    NativeMethods.RegisterMouseDelta((float)(delta.X * sensitivity), (float)(delta.Y * sensitivity));
                }

                _currentMousePositionAbsolute = centre;
                SetCursorPosition(centre);

                NativeMethods.MoveCamera(_direction);
            }

            if (_shouldRender)
            {
                InteropImage.RequestRender();
            }
        };
    }

    private bool InitStaticMesh(FileHash staticHash)
    {
        StaticMesh staticMesh = FileResourcer.Get().GetFile<StaticMesh>(staticHash);

        Blob staticMeshTransforms = staticMesh.TagData.StaticData.GetTransformsBlob();
        NativeMethods.CreateStaticMesh(staticHash, staticMeshTransforms);


        List<StaticPart> parts = staticMesh.Load(ExportDetailLevel.MostDetailed);
        List<BufferGroup> bufferGroups = staticMesh.TagData.StaticData.GetBuffers();
        List<int> strides = staticMesh.TagData.StaticData.GetStrides();
        if (strides.Count == 0)
        {
            Log.Warning($"Static mesh {staticHash} has no strides");
            return false;
        }
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
            long result = NativeMethods.CreateStaticMeshPart(staticHash, partInfo);
            if (result != 0)
            {
                MessageBox.Show("Failed to create static mesh part");
                return false;
            }
            partIndex++;
            _partInfos.Add(partInfo);
        }

        return true;
    }

    struct StaticMeshDesc
    {
        public uint Hash;
        public Blob Transforms;
    }

    struct StaticMeshMapDesc
    {
        public StaticMeshDesc StaticMesh;
        public Blob InstanceTransforms;
    }

    struct MapInfo
    {
        public List<StaticMeshDesc> StaticMeshes;
    }

    private bool InitMap(FileHash mapHash)
    {
        StaticMapData staticMap = FileResourcer.Get().GetFile<StaticMapData>(mapHash);

        MapInfo mapInfo = new();
        mapInfo.StaticMeshes = new List<StaticMeshDesc>();
        foreach (SStaticMeshInstanceMap staticInstanceMap in staticMap.TagData.InstanceCounts)
        {
            staticMap.TagData.Statics[staticInstanceMap.StaticIndex].Static;
        }

        Blob staticMeshTransforms = staticMesh.TagData.StaticData.GetTransformsBlob();
        NativeMethods.CreateStaticMesh(staticHash, staticMeshTransforms);



        List<StaticPart> parts = staticMesh.Load(ExportDetailLevel.MostDetailed);
        List<BufferGroup> bufferGroups = staticMesh.TagData.StaticData.GetBuffers();
        List<int> strides = staticMesh.TagData.StaticData.GetStrides();
        if (strides.Count == 0)
        {
            Log.Warning($"Static mesh {staticHash} has no strides");
            return false;
        }
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
            long result = NativeMethods.CreateStaticMeshPart(staticHash, partInfo);
            if (result != 0)
            {
                MessageBox.Show("Failed to create static mesh part");
                return false;
            }
            partIndex++;
            _partInfos.Add(partInfo);
        }

        return true;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public Blob[] PSTextures;
        public Blob PScb0;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Blob[] VSSamplers;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Blob[] PSSamplers;

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

            if (material.VertexShader.InputSignatures.Count > 8)
            {
                throw new Exception();
            }

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
            if (material.EnumerateVSTextures().ToList().Count > 16)
            {
                throw new Exception();
            }
            foreach (STextureTag vsTexture in material.EnumerateVSTextures())
            {
                if (vsTexture.Texture == null)
                {
                    continue;
                }
                byte[] final = vsTexture.Texture.GetDDSBytes();
                if (vsTexture.TextureIndex >= 16)
                {
                    throw new Exception();
                }
                VSTextures[vsTexture.TextureIndex] = new Blob(final);
            }

            PSTextures = new Blob[32];
            if (material.EnumeratePSTextures().ToList().Count > 32)
            {
                throw new Exception();
            }
            foreach (STextureTag psTexture in material.EnumeratePSTextures())
            {
                if (psTexture.Texture == null)
                {
                    continue;
                }
                byte[] final = psTexture.Texture.GetDDSBytes();
                if (psTexture.TextureIndex >= 32)
                {
                    throw new Exception();
                }
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

            VSSamplers = new Blob[16];
            if (material.VS_Samplers.Count > 16)
            {
                throw new Exception();
            }
            for (int i = 0; i < material.VS_Samplers.Count; i++)
            {
                DirectXSampler vsSampler = material.VS_Samplers[i];

                // for some reason samplers can actually be textures too? idk
                if (PackageResourcer.Get().GetFileMetadata(vsSampler.Hash).Type == 32)
                {
                    continue;
                }

                VSSamplers[i] = new Blob(StructConverter.FromType(vsSampler.Sampler));
            }

            PSSamplers = new Blob[16];
            if (material.PS_Samplers.Count > 16)
            {
                throw new Exception();
            }

            // File.WriteAllText($"TempFiles/PSBytecode_{material.FileHash}.txt", material.Decompile(material.PixelShader.GetBytecode(), material.FileHash));
            // PSBytecode.TempDump(material.FileHash);

            for (int i = 0; i < material.PS_Samplers.Count; i++)
            {
                DirectXSampler psSampler = material.PS_Samplers[i];

                // for some reason samplers can actually be textures too? idk
                if (PackageResourcer.Get().GetFileMetadata(psSampler.Hash).Type == 32)
                {
                    continue;
                }

                PSSamplers[i] = new Blob(StructConverter.FromType(psSampler.Sampler));
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
        long hr = NativeMethods.Render(surface, isNewSurface);
        if (hr != 0)
        {
            MessageBox.Show($"Failed to render with error code {hr:X8}");
        }
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
            case Key.R:
                NativeMethods.ResetCamera();
                break;
            case Key.O:
                NativeMethods.SetCameraMode(CameraMode.Orbit);
                break;
            case Key.F:
                NativeMethods.SetCameraMode(CameraMode.Free);
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

    private bool _movingOrbitOrigin = false;

    private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right)
        {
            // check if shift key is pressed
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                _movingOrbitOrigin = true;
            }

            _isMouseCaptured = true;
            Cursor = Cursors.None;
        }
    }

    private void MainWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _movingOrbitOrigin = false;
        if (e.ChangedButton == MouseButton.Right)
        {
            _isMouseCaptured = false;
            Cursor = Cursors.Arrow;
        }
    }

    private void AtlasView_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        NativeMethods.RegisterMouseScroll(e.Delta);
    }
}

