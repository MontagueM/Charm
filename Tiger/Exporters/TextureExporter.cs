using System.ComponentModel;
using Arithmic;
using DirectXTexNet;
using Tiger.Schema;

namespace Tiger.Exporters;

public class TextureExporter
{
    private static TextureExportFormat _format = TextureExportFormat.PNG;
    private static readonly object _lock = new();

    public static void SetTextureFormat(TextureExportFormat textureExportFormat) => _format = textureExportFormat;

    public static void ExportTexture(FileHash fileHash, int index = 0)
    {
        var config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        var pkgName = PackageResourcer.Get()
            .GetPackage(fileHash.PackageId)
            .GetPackageMetadata()
            .Name.Split('.')[0];

        var savePath = Path.Combine(config.GetExportSavePath(), "Textures", pkgName);
        Directory.CreateDirectory(savePath);

        var texture = FileResourcer.Get().GetFile<Texture>(fileHash);
        texture.SavetoFile(Path.Combine(savePath, fileHash.ToString()), index);
    }

    public static bool SaveTextureToFile(string savePath, ScratchImage scratchImage,
        TextureDimension dimension = TextureDimension.D2, TextureExportFormat? overrideFormat = null)
    {
        try
        {
            lock (_lock)
            {
                if (savePath.Contains('.')) // TODO: Figure this out
                {
                    Log.Error($"[BUG] Save Path {savePath} contains a period, cant export texture!");
                    scratchImage.Dispose();
                    return false;
                }

                var format = overrideFormat ?? _format;

                switch (format)
                {
                    case TextureExportFormat.DDS_BGRA_UNCOMP_DX10:
                        scratchImage.SaveToDDSFile(DDS_FLAGS.FORCE_DX10_EXT, savePath + ".dds");
                        break;

                    case TextureExportFormat.DDS_BGRA_BC3_DX10:
                        scratchImage = CompressBC3(scratchImage);
                        scratchImage.SaveToDDSFile(DDS_FLAGS.FORCE_DX9_LEGACY, savePath + ".dds");
                        break;

                    case TextureExportFormat.PNG:
                        SavePNGTexture(scratchImage, savePath, dimension, WICCodecs.PNG);
                        break;

                    case TextureExportFormat.TGA:
                        SaveTGATexture(scratchImage, savePath, dimension);
                        break;
                }

                scratchImage.Dispose();
                return true;
            }
        }
        catch (Exception e)
        {
            Log.Error($"{Path.GetFileName(savePath)} : {e.Message}");
            scratchImage.Dispose();
            return false;
        }
    }

    public static string GetExtension(TextureExportFormat exportFormat) => exportFormat switch
    {
        TextureExportFormat.DDS_BGRA_UNCOMP_DX10 or
        TextureExportFormat.DDS_BGRA_BC3_DX10 => "dds",
        TextureExportFormat.PNG => "png",
        TextureExportFormat.TGA => "tga",
        _ => string.Empty
    };

    #region Helpers

    private static ScratchImage CompressBC3(ScratchImage scratchImage)
    {
        var metadata = scratchImage.GetMetadata();
        return TexHelper.Instance.IsSRGB(metadata.Format)
            ? scratchImage.Compress(DXGI_FORMAT.BC3_UNORM_SRGB, TEX_COMPRESS_FLAGS.SRGB, 0)
            : scratchImage.Compress(DXGI_FORMAT.BC3_UNORM, TEX_COMPRESS_FLAGS.DEFAULT, 0);
    }

    private static void SavePNGTexture(ScratchImage scratchImage, string savePath, TextureDimension dimension, WICCodecs codec)
    {
        var guid = TexHelper.Instance.GetWICCodec(codec);

        if (dimension == TextureDimension.CUBE && scratchImage.GetImageCount() == 6)
        {
            using var flattened = Texture.FlattenCubemap(scratchImage);
            flattened.SaveToWICFile(0, WIC_FLAGS.FORCE_SRGB, guid, savePath + ".png");

            if (ConfigSubsystem.Get().GetExportEquirectCubemaps())
            {
                using var projected = Texture.CubemapCrossToEquirectangular(flattened);
                projected.SaveToWICFile(0, WIC_FLAGS.FORCE_SRGB, guid, savePath + "_Projected.png");
            }

        }
        else
        {
            scratchImage.SaveToWICFile(0, WIC_FLAGS.FORCE_SRGB, guid, savePath + ".png");
        }
    }

    private static void SaveTGATexture(ScratchImage scratchImage, string savePath, TextureDimension dimension)
    {
        if (dimension == TextureDimension.CUBE && scratchImage.GetImageCount() == 6)
        {
            using var flattened = Texture.FlattenCubemap(scratchImage);
            ScratchImage finalImage = flattened;

            if (flattened.GetMetadata().Format == DXGI_FORMAT.R16G16B16A16_UNORM)
            {
                finalImage = flattened.Convert(DXGI_FORMAT.R8G8B8A8_UNORM, 0, 0);
            }

            using (finalImage)
            {
                finalImage.SaveToTGAFile(0, savePath + ".tga");

                if (ConfigSubsystem.Get().GetExportEquirectCubemaps())
                {
                    using var projected = Texture.CubemapCrossToEquirectangular(finalImage);
                    projected.SaveToTGAFile(0, savePath + "_Projected.tga");
                }
            }
        }
        else
        {
            scratchImage.SaveToTGAFile(0, savePath + ".tga");
        }
    }

    #endregion
}

public enum TextureExportFormat
{
    [Description("DDS Uncompressed")]
    DDS_BGRA_UNCOMP_DX10,
    [Description("DDS")]
    DDS_BGRA_BC3_DX10,
    PNG,
    TGA
}
