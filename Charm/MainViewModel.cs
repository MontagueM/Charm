using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Field.Entities;
using Field.Models;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Animations;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Controls;
using Microsoft.Toolkit.Mvvm.Input;
using SharpDX;
using Vector3 = Field.Models.Vector3;
using Vector4 = Field.Models.Vector4;

using Media3D = System.Windows.Media.Media3D;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Transform3D = System.Windows.Media.Media3D.Transform3D;
using TranslateTransform3D = System.Windows.Media.Media3D.TranslateTransform3D;
using Color = System.Windows.Media.Color;
using Plane = SharpDX.Plane;
using Colors = System.Windows.Media.Colors;
using Color4 = SharpDX.Color4;
using MeshBuilder = HelixToolkit.SharpDX.Core.MeshBuilder;

namespace Charm;
using System.ComponentModel;
    
/// <summary>
/// Provides a ViewModel for the Main window.
/// </summary>
public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    public EffectsManager EffectsManager { get; set; }
        
    private string subTitle;

    private string title;
    
    public string Title
    {
        get
        {
            return title;
        }
        set
        {
            SetValue(ref title, value, "Title");
        }
    }

    public string SubTitle
    {
        get
        {
            return subTitle;
        }
        set
        {
            SetValue(ref subTitle, value, "SubTitle");
        }
    }
    
    public SceneNodeGroupModel3D ModelGroup { get; } = new SceneNodeGroupModel3D();

    public HelixToolkitScene Scene;

    private Point3D _cameraPosition;
    private Vector3D _cameraLookDirection;

    public RelayCommand RotateCommand { get; set; }
    public RelayCommand LeftCommand { get; set; }
    public RelayCommand ForwardCommand { get; set; }
    public RelayCommand BackCommand { get; set; }
    public RelayCommand RightCommand { get; set; }


    public PerspectiveCamera Camera { get; set; }
    
    public LineGeometry3D Grid { get; private set; }
    public Media3D.Transform3D GridTransform { get; private set; }
    
    private CompositionTargetEx compositeHelper = new CompositionTargetEx();
    private NodeAnimationUpdater animationUpdater;

    public MainViewModel()
    {
        EffectsManager = new  DefaultEffectsManager();
        Scene = new HelixToolkitScene(new GroupNode());
        ModelGroup.AddNode(Scene.Root);
        
        RotateCommand = new RelayCommand(ResetCamera);
        // ForwardCommand = new RelayCommand(MoveCameraForward);

        Camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            UpDirection = new Vector3D(0, 1, 0),
            LookDirection = new Vector3D(-0, -0, -5),
            FarPlaneDistance = 100000000,
        };
        
        Grid = LineBuilder.GenerateGrid();
        GridTransform = new TranslateTransform3D(-5, 0, -5);
        
        compositeHelper.Rendering += CompositeHelper_Rendering;
        // EnvironmentMap = TextureModel.Create("C:/T/full/Textures/2D47A280.dds");
    }

    private void ResetCamera()
    {
        Camera.Position = new Point3D(0, 0, 5);
        Camera.UpDirection = new Vector3D(0, 1, 0);
        Camera.LookDirection = new Vector3D(-0, -0, -5);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected bool SetValue<T>(ref T backingField, T value, [CallerMemberName]string propertyName = "")
    {
        if (object.Equals(backingField, value))
        {
            return false;
        }

        backingField = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }
        
    public void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler == null) return;
        handler(this, new PropertyChangedEventArgs(propertyName));
    }

    public struct DisplayPart
    {
        public Part BasePart = new Part();
        public DynamicPart EntityPart = new DynamicPart();
        public List<Vector3> Translations = new List<Vector3>();
        public List<Vector4> Rotations = new List<Vector4>();
        public List<Vector3> Scales = new List<Vector3>();

        public DisplayPart()
        {
                
        }
    }

    public void Clear()
    {
        // need to iterate over everything to wipe the arrays
        Parallel.ForEach(ModelGroup.GroupNode.Items, node =>
        {
            if (node is MeshNode mn)
            {
                MeshGeometry3D mesh = mn.Geometry as MeshGeometry3D;
                mn.Instances = null;
                mesh.ClearAllGeometryData();
                var q = mesh as IDisposable;
                Disposer.RemoveAndDispose(ref q);
                mn.Material = null;
            }
            node.Detach();
            node.Dispose();
            var n = node as IDisposable;
            Disposer.RemoveAndDispose(ref n);
        });
        ModelGroup.Clear();
        GC.Collect();
    }
    
    public bool LoadEntityFromFbx(string modelFile)
    {
        Clear();
        var importer = new Importer();
        importer.Configuration.ImportAnimations = true;
        importer.Configuration.SkeletonSizeScale = 0.02f;
        importer.Configuration.GlobalScale = 1f;
        HelixToolkitScene scene = importer.Load(modelFile);
        if (scene == null)  // unsure why this happens, but seems to be always bone related for massive objects
        {
            return false;
        }
        bool bSkel = false;
        ModelGroup.AddNode(scene.Root);
        foreach (var node in scene.Root.Items.Traverse(false))
        {
            if (node is MeshNode mn)
            {
                var material = new DiffuseMaterial
                {
                    DiffuseColor = new Color4(0.7f, 0.7f, 0.7f, 1.0f)
                };
                mn.ModelMatrix = node.ModelMatrix * SharpDX.Matrix.RotationX(-(float) Math.PI / 2) * SharpDX.Matrix.RotationY(-(float) Math.PI / 2);
                mn.Material = material;
                if (mn is BoneSkinMeshNode m)
                {
                    if (!bSkel)
                    {
                        var mat = new DiffuseMaterial
                        {
                            DiffuseColor = new Color4(1f, 0f, 0f, 1.0f)
                        };
                        var skeleton = m.CreateSkeletonNode(mat, importer.Configuration.SkeletonEffects, importer.Configuration.SkeletonSizeScale);
                        skeleton.ModelMatrix = m.ModelMatrix;
                        ModelGroup.AddNode(skeleton);
                        ModelGroup.AddNode(new NodePostEffectXRayGrid {
                            EffectName = importer.Configuration.SkeletonEffects,
                            Color = mat.DiffuseColor,
                            GridDensity = 1,
                        });
                        bSkel = true;
                    }
                }
            }
        }

        if (scene.Animations != null)
        {
            if (scene.Animations.Count == 1)
            {
                var anim = scene.Animations[0];
                animationUpdater = new NodeAnimationUpdater(anim);
                animationUpdater.RepeatMode = AnimationRepeatMode.Loop;
                scene.Root.ModelMatrix = Matrix.RotationX(-(float) Math.PI / 2);
                // animationUpdater.Speed = (float)0.00001;
            }
            else if (scene.Animations.Count > 1)
            {
                throw new NotImplementedException();
            }  
        }

        return true;
    }
    
    private void CompositeHelper_Rendering(object sender, System.Windows.Media.RenderingEventArgs e)
    {
        if (animationUpdater != null)
        {
            animationUpdater.Update(Stopwatch.GetTimestamp(), Stopwatch.Frequency);
        }
    }
    
    // https://stackoverflow.com/questions/33374434/improve-wpf-rendering-performance-using-helix-toolkit
    public void SetChildren(List<DisplayPart> parts)
    {
        foreach (var part in parts)
        { 
            MeshNode model = new MeshNode();
            MeshGeometry3D Model = new MeshGeometry3D();
            Matrix[] ModelInstances = new Matrix[part.Translations.Count];
            PhongMaterial ModelMaterial = new PhongMaterial();
                
            HelixToolkit.SharpDX.Core.MeshGeometry3D mesh = new HelixToolkit.SharpDX.Core.MeshGeometry3D();
            IntCollection triangleIndices = new IntCollection();
            Vector3Collection positions = new Vector3Collection();
            Vector3Collection normals = new Vector3Collection();
            Vector2Collection textureCoordinates = new Vector2Collection();
            mesh.SetAsTransient();
            if (part.BasePart.Indices.Count > 0)
            {
                // Conversion lookup table
                Dictionary<int, int> lookup = new Dictionary<int, int>();
                for (int i = 0; i < part.BasePart.VertexIndices.Count; i++)
                {
                    lookup[(int)part.BasePart.VertexIndices[i]] = i;
                }
                foreach (var vertexIndex in part.BasePart.VertexIndices)
                {
                    var v4p = part.BasePart.VertexPositions[lookup[(int)vertexIndex]];
                    SharpDX.Vector3 p = new SharpDX.Vector3(v4p.X, v4p.Y, v4p.Z);
                    positions.Add(p);
                    // We need to check if the normal is Euler or Quaternion
                    var v4n = part.BasePart.VertexNormals[lookup[(int)vertexIndex]];
                    SharpDX.Vector3 n = new SharpDX.Vector3(v4n.X, v4n.Y, v4n.Z);
                    normals.Add(n);
                    var v2t = part.BasePart.VertexTexcoords[lookup[(int)vertexIndex]];
                    SharpDX.Vector2 t = new SharpDX.Vector2(v2t.X, v2t.Y);
                    textureCoordinates.Add(t);
                }
                foreach (UIntVector3 face in part.BasePart.Indices)
                {
                    triangleIndices.Add(lookup[(int)face.X]);
                    triangleIndices.Add(lookup[(int)face.Y]);
                    triangleIndices.Add(lookup[(int)face.Z]);
                }  
            }
                
            mesh.Positions = positions;
            mesh.Normals = normals;
            mesh.TextureCoordinates = textureCoordinates;
            mesh.TriangleIndices = triangleIndices;
            model.Geometry = mesh;
            model.Material = new DiffuseMaterial
            {
                DiffuseColor = new Color4(0.7f, 0.7f, 0.7f, 1.0f)
            };
            List<Matrix> instances = new List<Matrix>();
                
            for (int i = 0; i < part.Translations.Count; i++)
            {
                SharpDX.Vector3 scale = new SharpDX.Vector3(part.Scales[i].X, part.Scales[i].Y, part.Scales[i].Z);
                SharpDX.Quaternion rotation = new SharpDX.Quaternion(part.Rotations[i].X, part.Rotations[i].Y, part.Rotations[i].Z, part.Rotations[i].W);
                SharpDX.Vector3 translation = new SharpDX.Vector3(part.Translations[i].X, part.Translations[i].Y, part.Translations[i].Z);
                SharpDX.Matrix matrix = new SharpDX.Matrix();
                SharpDX.Vector3 scalingOrigin = SharpDX.Vector3.Zero;
                matrix = SharpDX.Matrix.Transformation(scalingOrigin, SharpDX.Quaternion.Identity, scale, SharpDX.Vector3.Zero, rotation, translation);
                // Transform Y-up to Z-up
                // instances.Add(matrix * SharpDX.Matrix.RotationX(-(float)Math.PI / 2) * SharpDX.Matrix.RotationY(-(float)Math.PI / 2));
                instances.Add(matrix * SharpDX.Matrix.RotationX(-(float)Math.PI / 2) * SharpDX.Matrix.RotationY(-(float)Math.PI / 2));

            }
            ModelInstances = instances.ToArray();
            model.Instances = ModelInstances;
            ModelGroup.AddNode(model);
        }
    }

    private Vector3 ConsiderQuatToEulerConvert(Vector4 v4N)
    {
        Vector3 res = new Vector3();
        if (Math.Abs(v4N.Magnitude - 1) < 0.01)  // Quaternion
        {
            var quat = new Quaternion(v4N.X, v4N.Y, v4N.Z, v4N.W);
            var a = new SharpDX.Vector3(1, 0, 0);
            var result = SharpDX.Vector3.Transform(a, quat);
            res.X = result.X;
            res.Y = result.Y;
            res.Z = result.Z;
        }
        else
        {
            res.X = v4N.X;
            res.Y = v4N.Y;
            res.Z = v4N.Z;
        }
        return res;
    }

    public void Dispose()
    {
        Clear();
        if (ModelGroup != null)
        {
            var modelGroup = ModelGroup as IDisposable;
            Disposer.RemoveAndDispose(ref modelGroup);
        }
        if (EffectsManager != null)
        {
            var effectManager = EffectsManager as IDisposable;
            Disposer.RemoveAndDispose(ref effectManager);
        }
        GC.SuppressFinalize(this);
    }
}