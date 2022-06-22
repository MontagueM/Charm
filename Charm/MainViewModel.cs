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
    
    public void LoadEntityFromFbx(string modelFile)
    {
        Clear();
        var importer = new Importer();
        importer.Configuration.ImportAnimations = true;
        importer.Configuration.SkeletonSizeScale = 0.02f;
        importer.Configuration.GlobalScale = 1f;
        HelixToolkitScene scene = importer.Load(modelFile);
        bool bSkel = false;
        ModelGroup.AddNode(scene.Root);
        foreach (var node in scene.Root.Items.Traverse(false))
        {
            if (node is BoneSkinMeshNode m)
            {
                var material = new DiffuseMaterial
                {
                    DiffuseColor = new Color4(0.7f, 0.7f, 0.7f, 1.0f)
                };
                m.ModelMatrix = m.ModelMatrix * SharpDX.Matrix.RotationX(-(float) Math.PI / 2) * SharpDX.Matrix.RotationY(-(float) Math.PI / 2);
                m.Material = material;
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
}