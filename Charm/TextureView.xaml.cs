using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Field.General;
using Field;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;

namespace Charm;

public partial class TextureView : UserControl
{
    public TextureView()
    {
        InitializeComponent();
    }

    public void LoadTexture(TextureHeader textureHeader)
    {
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = textureHeader.GetTexture();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        // Divide aspect ratio to fit 960x1000
        float widthDivisionRatio = (float)textureHeader.Header.Width / 960;
        float heightDivisionRatio = (float)textureHeader.Header.Height / 1000;
        float transformRatio = Math.Max(heightDivisionRatio, widthDivisionRatio);
        int imgWidth = (int)Math.Floor(textureHeader.Header.Width / transformRatio);
        int imgHeight = (int)Math.Floor(textureHeader.Header.Height / transformRatio);
        bitmapImage.DecodePixelWidth = imgWidth;
        bitmapImage.DecodePixelHeight = imgHeight;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        Image.Source = bitmapImage;
        Image.Width = imgWidth;
        Image.Height = imgHeight;
    }

    public static void ExportTexture(TagHash tagHash)
    {
        TextureHeader textureHeader = PackageHandler.GetTag(typeof(TextureHeader), tagHash);
        string savePath = ConfigHandler.GetExportSavePath() + $"/Textures/{tagHash}";
        Directory.CreateDirectory(ConfigHandler.GetExportSavePath() + "/Textures/");
        textureHeader.SavetoFile(savePath);
    }
}