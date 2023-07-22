using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Tiger;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
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
        Image.Source = bitmapImage;
        Image.Width = imgWidth;
        Image.Height = imgHeight;
    }

    public static void ExportTexture(FileHash fileHash)
    {
        string savePath = ConfigHandler.GetExportSavePath() + $"/Textures/{fileHash}";
        Directory.CreateDirectory(ConfigHandler.GetExportSavePath() + "/Textures/");

        Texture texture = FileResourcer.Get().GetFile<Texture>(fileHash);
        texture.SavetoFile(savePath);
    }
}
