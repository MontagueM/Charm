using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DirectXTexNet;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;
using Blob = Tiger.Blob;
using Device = SharpDX.Direct3D11.Device;
using Point = System.Windows.Point;
using RegisterComponentType = Tiger.Schema.RegisterComponentType;
using Vector4 = System.Numerics.Vector4;

namespace AtlasSharp;

enum MoveDirection : int
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

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Point _centrePosition;

    public MainWindow()
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
        return new Point(source.CompositionTarget.TransformToDevice.M11*(Left + ActualWidth / 2), source.CompositionTarget.TransformToDevice.M22*(Top + ActualHeight / 2));
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
        InitializeRendering();
    }

    private Point _currentMousePositionAbsolute = new();
    MoveDirection _direction = MoveDirection.None;

    private void InitializeRendering()
    {
        InteropImage.WindowOwner = (new System.Windows.Interop.WindowInteropHelper(this)).Handle;


        // WindowOwner is a uint
        int result = NativeMethods.Init(InteropImage.WindowOwner);

        InitStaticMesh();

        if (result != 0)
        {
            throw new Exception();
        }


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

    private void InitStaticMesh()
    {
        InitTiger();

        // uint staticHash = 0x80bce840; // 40E8BC80
        uint staticHash = 0x80bce912; // 12e9bc80
        StaticMesh staticMesh = FileResourcer.Get().GetFile<StaticMesh>(new FileHash(staticHash));

        Blob staticMeshTransforms = staticMesh.TagData.StaticData.GetTransformsBlob();
        NativeMethods.CreateStaticMesh(staticHash, staticMeshTransforms);


        List<StaticPart> parts = staticMesh.Load(ExportDetailLevel.MostDetailed);
        List<BufferGroup> bufferGroups = staticMesh.TagData.StaticData.GetBuffers();
        List<int> strides = staticMesh.TagData.StaticData.GetStrides();
        foreach (BufferGroup bufferGroup in bufferGroups)
        {
            NativeMethods.AddStaticMeshBufferGroup(staticHash, bufferGroup);
            bufferGroup.VertexBuffers[0].TempDump("0");
            bufferGroup.VertexBuffers[1].TempDump("1");
            bufferGroup.VertexBuffers[2].TempDump("2");
        }
        int partIndex = 0;

        foreach (StaticPart part in parts)
        {
            if (part.Material == null)
            {
                continue;
            }

            part.Material.VertexShader.TempDumpRef();

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

    private void InitTiger()
    {
        HashSet<Type> lazyStrategistSingletons = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Select(t => t.GetNonGenericParent(typeof(Strategy.LazyStrategistSingleton<>)))
            .Where(t => t is { ContainsGenericParameters: false })
            .Select(t => t.GetNonGenericParent(typeof(Strategy.StrategistSingleton<>)))
            .ToHashSet();

        // Get all classes that inherit from StrategistSingleton<>
        // Then call RegisterEvents() on each of them
        HashSet<Type> allStrategistSingletons = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Select(t => t.GetNonGenericParent(typeof(Strategy.StrategistSingleton<>)))
            .Where(t => t is { ContainsGenericParameters: false })
            .ToHashSet();

        allStrategistSingletons.ExceptWith(lazyStrategistSingletons);

        // order dependencies from InitializesAfterAttribute
        List<Type> strategistSingletons = SortByInitializationOrder(allStrategistSingletons.ToList()).ToList();

        foreach (Type strategistSingleton in strategistSingletons)
        {
            strategistSingleton.GetMethod("RegisterEvents", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        }

        string[] args = Environment.GetCommandLineArgs();
        CharmInstance.Args = new CharmArgs(args);
        CharmInstance.InitialiseSubsystems();
    }

    private static IEnumerable<Type> SortByInitializationOrder(IEnumerable<Type> types)
    {
        var dependencyMap = new Dictionary<Type, List<Type>>();
        var dependencyCount = new Dictionary<Type, int>();

        // Build dependency map and count dependencies
        foreach (var type in types)
        {
            var attributes = type.GenericTypeArguments[0].GetCustomAttributes(typeof(InitializeAfterAttribute), true);
            foreach (InitializeAfterAttribute attribute in attributes)
            {
                var dependentType = attribute.TypeToInitializeAfter.GetNonGenericParent(
                    typeof(Strategy.StrategistSingleton<>));
                if (!dependencyMap.ContainsKey(dependentType))
                {
                    dependencyMap[dependentType] = new List<Type>();
                    dependencyCount[dependentType] = 0;
                }
                dependencyMap[dependentType].Add(type);
                dependencyCount[type] = dependencyCount.ContainsKey(type) ? dependencyCount[type] + 1 : 1;
            }
        }

        // Perform topological sorting
        var sortedTypes = types.Where(t => !dependencyCount.ContainsKey(t)).ToList();
        var queue = new Queue<Type>(dependencyMap.Keys.Where(k => dependencyCount[k] == 0));
        while (queue.Count > 0)
        {
            var type = queue.Dequeue();
            sortedTypes.Add(type);

            if (dependencyMap.ContainsKey(type))
            {
                foreach (var dependentType in dependencyMap[type])
                {
                    dependencyCount[dependentType]--;
                    if (dependencyCount[dependentType] == 0)
                    {
                        queue.Enqueue(dependentType);
                    }
                }
            }
        }

        if (sortedTypes.Count < types.Count())
        {
            throw new InvalidOperationException("Circular dependency detected.");
        }

        return sortedTypes;
    }
}

static class NativeMethods
{
    /// <summary>
    /// Variable used to track whether the missing dependency dialog has been displayed,
    /// used to prevent multiple notifications of the same failure.
    /// </summary>
    private static bool errorHasDisplayed;

    [DllImport("C:/Users/monta/Desktop/Projects/Charm/x64/Debug/Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Init(nint hwnd);

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Cleanup();

    [DllImport("C:/Users/monta/Desktop/Projects/Charm/x64/Debug/Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Render(IntPtr resourcePointer, bool isNewSurface);

    [DllImport("C:/Users/monta/Desktop/Projects/Charm/x64/Debug/Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int MoveCamera(MoveDirection _direction);

    [DllImport("C:/Users/monta/Desktop/Projects/Charm/x64/Debug/Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int RegisterMouseDelta(float mouseX, float mouseY);

    [DllImport("C:/Users/monta/Desktop/Projects/Charm/x64/Debug/Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int CreateStaticMesh(uint hash, Blob staticMeshTransforms);

    [DllImport("C:/Users/monta/Desktop/Projects/Charm/x64/Debug/Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int AddStaticMeshBufferGroup(uint hash, BufferGroup bufferGroup);

    [DllImport("C:/Users/monta/Desktop/Projects/Charm/x64/Debug/Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int CreateStaticMeshPart(uint hash, MainWindow.PartInfo partInfo);

    /// <summary>
    /// Method used to invoke an Action that will catch DllNotFoundExceptions and display a warning dialog.
    /// </summary>
    /// <param name="action">The Action to invoke.</param>
    public static void InvokeWithDllProtection(Action action)
    {
        InvokeWithDllProtection(
            () =>
            {
                action.Invoke();
                return 0;
            });
    }

    /// <summary>
    /// Method used to invoke A Func that will catch DllNotFoundExceptions and display a warning dialog.
    /// </summary>
    /// <param name="func">The Func to invoke.</param>
    /// <returns>The return value of func, or default(T) if a DllNotFoundException was caught.</returns>
    /// <typeparam name="T">The return type of the func.</typeparam>
    public static T InvokeWithDllProtection<T>(Func<T> func)
    {
        try
        {
            return func.Invoke();
        }
        catch (DllNotFoundException e)
        {
            if (!errorHasDisplayed)
            {
                MessageBox.Show("This sample requires:\nManual build of the D3DVisualization project, which requires installation of Windows 10 SDK or DirectX SDK.\n" +
                                "Installation of the DirectX runtime on non-build machines.\n\n"+
                                "Detailed exception message: " + e.Message, "WPF D3D11 Interop",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                errorHasDisplayed = true;

                if (Application.Current != null)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        return default(T);
    }
}
