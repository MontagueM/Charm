using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.Toolkit.Mvvm.Input;
using SharpDX;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Color4 = SharpDX.Color4;
using Log = Arithmic.Log;
using Media3D = System.Windows.Media.Media3D;
using Point3D = System.Windows.Media.Media3D.Point3D;
using TranslateTransform3D = System.Windows.Media.Media3D.TranslateTransform3D;
using Vector3 = Tiger.Schema.Vector3;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Vector4 = Tiger.Schema.Vector4;

namespace Charm;

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

    public RelayCommand ResetCameraTransforms { get; set; }

    public PerspectiveCamera Camera { get; set; }

    public LineGeometry3D Grid { get; private set; }
    public Media3D.Transform3D GridTransform { get; private set; }

    public MainViewModel()
    {
        EffectsManager = new DefaultEffectsManager();
        Scene = new HelixToolkitScene(new GroupNode());
        ModelGroup.AddNode(Scene.Root);

        ResetCameraTransforms = new RelayCommand(ResetCamera);
        // ForwardCommand = new RelayCommand(MoveCameraForward);

        Camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            UpDirection = new Vector3D(0, 1, 0),
            LookDirection = new Vector3D(-0, -0, -5),
            FarPlaneDistance = 1000000,
            FieldOfView = 60
        };

        Grid = LineBuilder.GenerateGrid();
        GridTransform = new TranslateTransform3D(-5, 0, -5);

        // EnvironmentMap = TextureModel.Create("C:/T/full/Textures/2D47A280.dds");
    }

    private void ResetCamera()
    {
        Camera.Position = new Point3D(0, 0, 5);
        Camera.UpDirection = new Vector3D(0, 1, 0);
        Camera.LookDirection = new Vector3D(-0, -0, -5);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected bool SetValue<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
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
        public MeshPart BasePart = new();
        public DynamicMeshPart EntityPart = new();
        public List<Vector3> Translations = new();
        public List<Vector4> Rotations = new();
        public List<Vector3> Scales = new();
        public List<BoneNode> BoneNodes = new();
        public DiffuseMaterial DiffuseMaterial = new()
        {
            DiffuseColor = new Color4(0.9f, 0.9f, 0.9f, 1.0f)
        };

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
        // this can crash, and theres nothing I can do about it :)
        HelixToolkitScene scene = importer.Load(modelFile);
        if (scene == null)  // unsure why this happens, but seems to be always bone related for massive objects
        {
            Log.Error("Failed to load scene");
            return false;
        }
        bool bSkel = false;
        ModelGroup.AddNode(scene.Root);
        foreach (SceneNode? node in scene.Root.Items.Traverse(false))
        {
            if (node is MeshNode mn)
            {
                var material = new DiffuseMaterial
                {
                    DiffuseColor = new Color4(0.7f, 0.7f, 0.7f, 1.0f)
                };
                mn.ModelMatrix = node.ModelMatrix * SharpDX.Matrix.RotationX(-(float)Math.PI / 2) * SharpDX.Matrix.RotationY(-(float)Math.PI / 2);
                mn.Material = material;
                if (mn is BoneSkinMeshNode m)
                {
                    if (!bSkel)
                    {
                        var mat = new DiffuseMaterial
                        {
                            DiffuseColor = new Color4(1f, 0f, 0f, 1.0f)
                        };
                        BoneSkinMeshNode skeleton = m.CreateSkeletonNode(mat, importer.Configuration.SkeletonEffects, importer.Configuration.SkeletonSizeScale);
                        skeleton.ModelMatrix = m.ModelMatrix;
                        ModelGroup.AddNode(skeleton);
                        ModelGroup.AddNode(new NodePostEffectXRayGrid
                        {
                            EffectName = importer.Configuration.SkeletonEffects,
                            Color = mat.DiffuseColor,
                            GridDensity = 1,
                        });
                        bSkel = true;
                    }
                }
            }
        }

        return true;
    }

    // https://stackoverflow.com/questions/33374434/improve-wpf-rendering-performance-using-helix-toolkit
    public void SetChildren(List<DisplayPart> parts)
    {
        foreach (DisplayPart part in parts)
        {
            MeshNode model = new();
            Matrix[] ModelInstances = new Matrix[part.Translations.Count];

            HelixToolkit.SharpDX.Core.MeshGeometry3D mesh = new();
            IntCollection triangleIndices = new();
            Vector3Collection positions = new();
            Vector3Collection normals = new();
            Vector2Collection textureCoordinates = new();
            mesh.SetAsTransient();
            //Random rand = new();

            if (part.BasePart.Indices.Count > 0)
            {
                // Conversion lookup table
                Dictionary<int, int> lookup = new();
                for (int i = 0; i < part.BasePart.VertexIndices.Count; i++)
                {
                    lookup[(int)part.BasePart.VertexIndices[i]] = i;
                }

                foreach (uint vertexIndex in part.BasePart.VertexIndices)
                {
                    Vector4 v4p = part.BasePart.VertexPositions[lookup[(int)vertexIndex]];
                    if (float.IsInfinity(v4p.X) || float.IsNaN(v4p.X))
                        v4p.X = 0;
                    if (float.IsInfinity(v4p.Y) || float.IsNaN(v4p.Y))
                        v4p.Y = 0;
                    if (float.IsInfinity(v4p.Z) || float.IsNaN(v4p.Z))
                        v4p.Z = 0;

                    SharpDX.Vector3 p = new(v4p.X, v4p.Y, v4p.Z);
                    positions.Add(p);
                    // We need to check if the normal is Euler or Quaternion
                    if (part.BasePart.VertexNormals.Count > 0)
                    {
                        Vector4 v4n = part.BasePart.VertexNormals[lookup[(int)vertexIndex]];
                        Vector3 v3ne = part.BasePart is DynamicMeshPart ? new Vector3(v4n.X, v4n.Y, v4n.Z) : ConsiderQuatToEulerConvert(v4n);
                        SharpDX.Vector3 n = new(v3ne.X, v3ne.Y, v3ne.Z);
                        normals.Add(n);
                    }
                    if (part.BasePart.VertexTexcoords0.Count > 0)
                    {
                        Tiger.Schema.Vector2 v2t = part.BasePart.VertexTexcoords0[lookup[(int)vertexIndex]];
                        SharpDX.Vector2 t = new(v2t.X, 1 - v2t.Y);
                        textureCoordinates.Add(t);
                    }
                }
                foreach (UIntVector3 face in part.BasePart.Indices)
                {
                    triangleIndices.Add(lookup[(int)face.X]);
                    triangleIndices.Add(lookup[(int)face.Y]);
                    triangleIndices.Add(lookup[(int)face.Z]);
                }
            }
            if (part.BoneNodes.Count > 0)
            {
                AddSkeletonVisual(part.BoneNodes);
            }

            mesh.Positions = positions;
            mesh.Normals = normals;
            mesh.TextureCoordinates = textureCoordinates;
            mesh.TriangleIndices = triangleIndices;
            model.Geometry = mesh;

            model.Material = part.DiffuseMaterial;
            model.CullMode = SharpDX.Direct3D11.CullMode.Back;

            List<Matrix> instances = new();
            for (int i = 0; i < part.Translations.Count; i++)
            {
                SharpDX.Vector3 scale = new(part.Scales[i].X, part.Scales[i].Y, part.Scales[i].Z);
                SharpDX.Quaternion rotation = new(part.Rotations[i].X, part.Rotations[i].Y, part.Rotations[i].Z, part.Rotations[i].W);
                SharpDX.Vector3 translation = new(part.Translations[i].X, part.Translations[i].Y, part.Translations[i].Z);
                SharpDX.Matrix matrix = new();
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

    public void AddSkeletonVisual(List<BoneNode> bones)
    {
        var positions = new Vector3Collection();
        var indices = new IntCollection();
        Matrix correction = SharpDX.Matrix.RotationX(-(float)Math.PI / 2) * SharpDX.Matrix.RotationY(-(float)Math.PI / 2);

        foreach (BoneNode bone in bones)
        {
            if (bone.ParentNodeIndex > 0 && bone.ParentNodeIndex < bones.Count)
            {
                BoneNode parent = bones[bone.ParentNodeIndex];

                int startIndex = positions.Count;

                var childPos = SharpDX.Vector3.TransformCoordinate(
                    new SharpDX.Vector3(bone.DefaultObjectSpaceTransform.Translation.X,
                                        bone.DefaultObjectSpaceTransform.Translation.Y,
                                        bone.DefaultObjectSpaceTransform.Translation.Z),
                    correction);

                var parentPos = SharpDX.Vector3.TransformCoordinate(
                    new SharpDX.Vector3(parent.DefaultObjectSpaceTransform.Translation.X,
                                        parent.DefaultObjectSpaceTransform.Translation.Y,
                                        parent.DefaultObjectSpaceTransform.Translation.Z),
                    correction);

                positions.Add(childPos);
                positions.Add(parentPos);

                indices.Add(startIndex);
                indices.Add(startIndex + 1);


                // Adds sphere at joint
                var jointPos = SharpDX.Vector3.TransformCoordinate(
                    new SharpDX.Vector3(bone.DefaultObjectSpaceTransform.Translation.X,
                                        bone.DefaultObjectSpaceTransform.Translation.Y,
                                        bone.DefaultObjectSpaceTransform.Translation.Z),
                    correction);

                var sphereMeshBuilder = new MeshBuilder();
                sphereMeshBuilder.AddSphere(new SharpDX.Vector3(jointPos.X, jointPos.Y, jointPos.Z), 0.0075, 6, 6);

                var sphereModel = new MeshGeometryModel3D
                {
                    Geometry = sphereMeshBuilder.ToMeshGeometry3D(),
                    Material = new DiffuseMaterial
                    {
                        DiffuseColor = new Color4(1, 0, 0, 1)
                    },
                    DepthBias = -int.MaxValue
                };

                ModelGroup.AddNode((SceneNode)sphereModel);
            }
        }

        var skeletonLines = new LineGeometryModel3D
        {
            Geometry = new LineGeometry3D
            {
                Positions = positions,
                Indices = indices
            },
            Color = System.Windows.Media.Colors.Red,
            Thickness = 1.5,
            DepthBias = -int.MaxValue,
        };

        ModelGroup.AddNode((SceneNode)skeletonLines);
    }

    private Vector3 ConsiderQuatToEulerConvert(Vector4 v4N)
    {
        // shadowkeep and below don't have quaternion normals
        if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999)
        {
            return new Vector3(v4N.X, v4N.Y, v4N.Z);
        }
        Vector3 res = new();
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
