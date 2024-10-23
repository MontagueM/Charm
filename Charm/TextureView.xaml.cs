using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tiger;
using Tiger.Schema;

namespace Charm;

public partial class TextureView : UserControl
{
    public TextureView()
    {
        InitializeComponent();
    }

    public void LoadTexture(Texture textureHeader)
    {
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = textureHeader.GetTexture();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        // Divide aspect ratio to fit 960x1000
        float widthDivisionRatio = (float)textureHeader.TagData.Width / 960;
        float heightDivisionRatio = (float)textureHeader.TagData.Height / 1000;
        float transformRatio = Math.Max(heightDivisionRatio, widthDivisionRatio);
        int imgWidth = (int)Math.Floor(textureHeader.TagData.Width / transformRatio);
        int imgHeight = (int)Math.Floor(textureHeader.TagData.Height / transformRatio);
        bitmapImage.DecodePixelWidth = imgWidth;
        bitmapImage.DecodePixelHeight = imgHeight;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        TextureDisplayData data = new()
        {
            Image = bitmapImage,
            Dimensions = $"{textureHeader.GetDimension().GetEnumDescription()}: {textureHeader.TagData.Width}x{textureHeader.TagData.Height}x{textureHeader.TagData.Depth}",
            Format = $"{textureHeader.TagData.GetFormat().ToString()} ({(textureHeader.IsSrgb() ? "Srgb" : "Linear")})"
        };

        DataContext = data;
    }

    public static void ExportTexture(FileHash fileHash)
    {
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        string pkgName = PackageResourcer.Get().GetPackage(fileHash.PackageId).GetPackageMetadata().Name.Split(".")[0];
        string savePath = config.GetExportSavePath() + $"/Textures/{pkgName}";
        Directory.CreateDirectory($"{savePath}/");

        Texture texture = FileResourcer.Get().GetFile<Texture>(fileHash);
        texture.SavetoFile($"{savePath}/{fileHash}");
    }

    public struct TextureDisplayData
    {
        public ImageSource Image { get; set; }
        public string Dimensions { get; set; }
        public string Format { get; set; }
    }
}
