using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Tiger;
using Tiger.Schema;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;

namespace Charm;

public partial class CubemapView : UserControl
{
    public CubemapView()
    {
        InitializeComponent();
    }

    public void LoadCubemap(Texture textureHeader)
    {
        CubemapViewport.Items.Clear();
        CubemapViewport.Items.Add(new EnvironmentMap3D
        {
            Texture = TextureModel.Create(textureHeader.GetTexture()),
        });
    }
}
