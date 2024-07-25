using System.Windows.Controls;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Tiger.Schema;

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

        // Can't use binding since DataContext is already taken up by something else 
        Dimensions.Text = $"{textureHeader.GetDimension()}: {textureHeader.TagData.Width}x{textureHeader.TagData.Height}x{textureHeader.TagData.Depth}";
        Format.Text = $"{textureHeader.TagData.GetFormat().ToString()} ({(textureHeader.IsSrgb() ? "Srgb" : "Linear")})";
    }
}
