using System.IO;
using System.Windows.Controls;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Tiger;
using Tiger.Schema;

namespace Charm;

public partial class CubemapView : UserControl
{
    private Texture _currentTexture;
    public CubemapView()
    {
        InitializeComponent();
    }

    public void LoadCubemap(Texture textureHeader)
    {
        _currentTexture = textureHeader;
        CubemapViewport.Items.Clear();
        CubemapViewport.Items.Add(new EnvironmentMap3D
        {
            Texture = TextureModel.Create(textureHeader.GetTexture()),
        });

        // Can't use binding since DataContext is already taken up by something else
        Hash.Text = $"{textureHeader.Hash}";
        Dimensions.Text = $"{textureHeader.GetDimension()}: {textureHeader.TagData.Width}x{textureHeader.TagData.Height}x{textureHeader.TagData.Depth}";
        Format.Text = $"{textureHeader.TagData.GetFormat().ToString()} ({(textureHeader.IsSrgb() ? "Srgb" : "Linear")})";
    }

    public void ExportCurrent()
    {
        if (_currentTexture is null)
            return;

        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        string pkgName = PackageResourcer.Get().GetPackage(_currentTexture.Hash.PackageId).GetPackageMetadata().Name.Split(".")[0];
        string savePath = config.GetExportSavePath() + $"/Textures/{pkgName}";
        Directory.CreateDirectory($"{savePath}/");

        _currentTexture.SavetoFile($"{savePath}/{_currentTexture.Hash}");
    }
}
