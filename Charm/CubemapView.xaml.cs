using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Field;
using Field.General;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;

namespace Charm;

public partial class CubemapView : UserControl
{
    public CubemapView()
    {
        InitializeComponent();
    }
    
    public void LoadCubemap(TextureHeader textureHeader)
    {
        CubemapViewport.Items.Clear();
        CubemapViewport.Items.Add(new EnvironmentMap3D
        {
            Texture = TextureModel.Create(textureHeader.GetTexture()),
        });
    }
}