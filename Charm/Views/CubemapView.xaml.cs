using System.IO;
using System.Windows.Controls;
using DirectXTexNet;
using HelixToolkit.SharpDX;
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
            Texture = TextureModel.Create(GetDisplayTexture(textureHeader)),
        });

        // Can't use binding since DataContext is already taken up by Helix3D
        Hash.Text = $"{textureHeader.Hash}";
        Dimensions.Text = $"{textureHeader.GetDimension()}: {textureHeader.TagData.Width}x{textureHeader.TagData.Height}x{textureHeader.TagData.ArraySize}";
        Format.Text = $"{textureHeader.TagData.GetFormat().ToString()} ({(textureHeader.IsSrgb() ? "Srgb" : "Linear")})";
    }

    private UnmanagedMemoryStream GetDisplayTexture(Texture textureHeader)
    {
        ScratchImage scratchImage = textureHeader.GetScratchImage();
        UnmanagedMemoryStream ms;

        if (scratchImage.GetMetadata().ArraySize != 6)
        {
            ms = scratchImage.SaveToDDSMemory(DDS_FLAGS.NONE);
            scratchImage.Dispose();
            return ms;
        }

        ScratchImage s1 = scratchImage.FlipRotate(2, TEX_FR_FLAGS.FLIP_VERTICAL).FlipRotate(0, TEX_FR_FLAGS.FLIP_HORIZONTAL);
        ScratchImage s2 = scratchImage.FlipRotate(0, TEX_FR_FLAGS.ROTATE90);
        ScratchImage s3 = scratchImage.FlipRotate(1, TEX_FR_FLAGS.ROTATE270);
        ScratchImage s4 = scratchImage.FlipRotate(4, TEX_FR_FLAGS.FLIP_VERTICAL).FlipRotate(0, TEX_FR_FLAGS.FLIP_HORIZONTAL);
        scratchImage = TexHelper.Instance.InitializeTemporary(
            new[]
            {
                s3.GetImage(0),
                s2.GetImage(0),
                s4.GetImage(0),
                scratchImage.GetImage(5),
                s1.GetImage(0),
                scratchImage.GetImage(3),
            },
            scratchImage.GetMetadata());

        ms = scratchImage.SaveToDDSMemory(DDS_FLAGS.NONE);

        s1.Dispose();
        s2.Dispose();
        s3.Dispose();
        s4.Dispose();
        scratchImage.Dispose();
        return ms;
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
