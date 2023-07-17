using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Charm.Views;
using CommunityToolkit.Mvvm.Input;
using DirectXTex;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Tiger;
using Tiger.Schema;
using Matrix = System.Windows.Media.Matrix;
using MeshGeometry3D = HelixToolkit.SharpDX.Core.MeshGeometry3D;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;


namespace Charm.Objects;

public struct DisplayPart
{
    public MeshPart BasePart;
    public List<Tiger.Schema.Vector3> Translations = new();
    public List<Tiger.Schema.Vector4> Rotations = new();
    public List<Tiger.Schema.Vector3> Scales = new();

    public DisplayPart(MeshPart basePart)
    {
        BasePart = basePart;
    }
}

// need a nice way of storing <Texture> while keeping it type safe (just make it one type to inherit from)
public class ModelViewModel : HashListItemModel
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
            SetField(ref title, value);
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
            SetField(ref subTitle, value);
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
    public Transform3D GridTransform { get; private set; }

    public Viewport3DX Viewport { get; set; }

    private bool _initialised = false;

    public ModelViewModel() : base()
    {
        Initialise();

    }

    public ModelViewModel(TigerHash hash, string typeName) :  base(hash, typeName)
    {
        // Initialise();
    }

    protected void Initialise()
    {
        EffectsManager = new  DefaultEffectsManager();
        Scene = new HelixToolkitScene(new GroupNode());
        ModelGroup.AddNode(Scene.Root);

        // RotateCommand = new RelayCommand(ResetCamera);
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
        _initialised = true;
        Title = "Loading...";
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

        // https://stackoverflow.com/questions/33374434/improve-wpf-rendering-performance-using-helix-toolkit
    public void AddPartsToViewport(IEnumerable<DisplayPart> parts)
    {
        // if (!_initialised)
        // {
        //     Initialise();
        // }
        foreach (var part in parts)
        {
            MeshNode model = new();
            MeshGeometry3D Model = new();
            PhongMaterial ModelMaterial = new();

            MeshGeometry3D mesh = new();
            IntCollection triangleIndices = new();
            Vector3Collection positions = new();
            Vector3Collection normals = new();
            Vector2Collection textureCoordinates = new();
            mesh.SetAsTransient();
            if (part.BasePart.Indices.Count > 0)
            {
                // Conversion lookup table
                Dictionary<int, int> lookup = new();
                for (int i = 0; i < part.BasePart.VertexIndices.Count; i++)
                {
                    lookup[(int)part.BasePart.VertexIndices[i]] = i;
                }
                foreach (var vertexIndex in part.BasePart.VertexIndices)
                {
                    var v4p = part.BasePart.VertexPositions[lookup[(int)vertexIndex]];
                    SharpDX.Vector3 p = new(v4p.X, v4p.Y, v4p.Z);
                    positions.Add(p);
                    // We need to check if the normal is Euler or Quaternion
                    var v4n = part.BasePart.VertexNormals[lookup[(int)vertexIndex]];
                    var v3n = ConsiderQuatToEulerConvert(v4n);
                    SharpDX.Vector3 n = new(v3n.X, v3n.Y, v3n.Z);
                    normals.Add(n);
                    var v2t = part.BasePart.VertexTexcoords0[lookup[(int)vertexIndex]];
                    SharpDX.Vector2 t = new(v2t.X, v2t.Y);
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
            model.Material =
                new HelixToolkit.Wpf.SharpDX.DiffuseMaterial
                {
                    DiffuseColor = new Color4(0.7f, 0.7f, 0.7f, 1.0f)
                };
            List<SharpDX.Matrix> instances = new();

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

            model.Instances = instances;
            ModelGroup.AddNode(model);
        }
    }

    private Tiger.Schema.Vector3 ConsiderQuatToEulerConvert(Tiger.Schema.Vector4 v4N)
    {
        Tiger.Schema.Vector3 res = new();
        if (Math.Abs(v4N.Magnitude - 1) < 0.01)  // Quaternion
        {
            var quat = new SharpDX.Quaternion(v4N.X, v4N.Y, v4N.Z, v4N.W);
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
}
