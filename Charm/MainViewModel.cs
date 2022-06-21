using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Field.Entities;
using Field.Models;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Animations;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX;
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
public class MainViewModel : INotifyPropertyChanged
{
    public EffectsManager EffectsManager { get; set; }
        
    private string subTitle;

    private string title;
        
    public TextureModel SkyboxTexture { set; get; }
        
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
    
    public MainViewModel()
    {
        EffectsManager = new  DefaultEffectsManager();
        Scene = new HelixToolkitScene(new GroupNode());
        ModelGroup.AddNode(Scene.Root);
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
        ModelGroup.Clear();
    }
        
        
    // https://stackoverflow.com/questions/33374434/improve-wpf-rendering-performance-using-helix-toolkit
    public void SetEntity(List<DisplayPart> parts, List<BoneNode> boneNodes)
    {
        Clear();
        AddEntity(parts, boneNodes);
    }
    public void AddEntity(List<DisplayPart> parts, List<BoneNode> boneNodes)
    {
        GroupNode gp = new GroupNode();
        BoneSkinnedMeshGeometry3D m = new BoneSkinnedMeshGeometry3D();
        foreach (var part in parts)
        { 
            InstancingMeshGeometryModel3D model = new InstancingMeshGeometryModel3D();
            Matrix[] ModelInstances = new Matrix[part.Translations.Count];
                
            BoneSkinnedMeshGeometry3D mesh = new BoneSkinnedMeshGeometry3D();
            IntCollection triangleIndices = new IntCollection();
            Vector3Collection positions = new Vector3Collection();
            Vector3Collection normals = new Vector3Collection();
            Vector2Collection textureCoordinates = new Vector2Collection();


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
                    Vector3 res = v4n.ToEuler();
                    SharpDX.Vector3 n = new SharpDX.Vector3(res.X, res.Y, res.Z);
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
            else
            {
                // Conversion lookup table
                Dictionary<int, int> lookup = new Dictionary<int, int>();
                for (int i = 0; i < part.EntityPart.VertexIndices.Count; i++)
                {
                    lookup[(int)part.EntityPart.VertexIndices[i]] = i;
                }
                foreach (var vertexIndex in part.EntityPart.VertexIndices)
                {
                    var v4p = part.EntityPart.VertexPositions[lookup[(int)vertexIndex]];
                    SharpDX.Vector3 p = new SharpDX.Vector3(v4p.X, v4p.Y, v4p.Z);
                    positions.Add(p);
                    var v4n = part.EntityPart.VertexNormals[lookup[(int)vertexIndex]];
                    SharpDX.Vector3 n = new SharpDX.Vector3(v4n.X, v4n.Y, v4n.Z);
                    normals.Add(n);
                    var v2t = part.EntityPart.VertexTexcoords[lookup[(int)vertexIndex]];
                    SharpDX.Vector2 t = new SharpDX.Vector2(v2t.X, v2t.Y);
                    textureCoordinates.Add(t);
                }

                foreach (UIntVector3 face in part.EntityPart.Indices)
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
            // model.Material = ModelMaterial;
            gp.AddChildNode(model.SceneNode);
        }
        ModelGroup.AddNode(gp);
        
        BoneSkinMeshNode mn = new BoneSkinMeshNode();
        // mn.Geometry = gp;
        mn.SetupIdentitySkeleton();
        mn.Visible = true;
        mn.RenderWireframe = true;
        mn.WireframeColor = new Color4(1, 0, 0, 1);
        ModelGroup.AddNode(mn);
    }

    public void SetSkeleton(List<BoneNode> nodes)
    {
        BoneSkinMeshNode n = new BoneSkinMeshNode();
        Bone[] bones = new Bone[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            Bone b = new Bone();
            b.ParentIndex = node.ParentNodeIndex;
            
            // Transform
            var x = node.DefaultObjectSpaceTransform;
            SharpDX.Vector3 scale = new SharpDX.Vector3(x.Scale, x.Scale, x.Scale);
            SharpDX.Quaternion rotation = new SharpDX.Quaternion(x.QuaternionRotation.X, x.QuaternionRotation.Y, x.QuaternionRotation.Z, x.QuaternionRotation.W);
            SharpDX.Vector3 translation = new SharpDX.Vector3(x.Translation.X, x.Translation.Y, x.Translation.Z);
            SharpDX.Matrix matrix = new SharpDX.Matrix();
            SharpDX.Vector3 scalingOrigin = SharpDX.Vector3.Zero;
            matrix = SharpDX.Matrix.Transformation(scalingOrigin, SharpDX.Quaternion.Identity, scale, SharpDX.Vector3.Zero, rotation, translation);
            // Transform Y-up to Z-up
            // instances.Add(matrix * SharpDX.Matrix.RotationX(-(float)Math.PI / 2) * SharpDX.Matrix.RotationY(-(float)Math.PI / 2));
            b.BoneLocalTransform = matrix * SharpDX.Matrix.RotationX(-(float) Math.PI / 2) * SharpDX.Matrix.RotationY(-(float) Math.PI / 2);
            bones[i] = b;
        }
        // n.Bones = bones;
        // n.SetupIdentitySkeleton();
        // n.Visible = true;

        var a = 0;
    }
}