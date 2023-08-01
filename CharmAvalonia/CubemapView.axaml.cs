using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Tiger.Schema;

namespace CharmAvalonia;

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

