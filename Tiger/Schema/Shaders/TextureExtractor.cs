using Arithmic;
using DirectXTexNet;

namespace Tiger.Schema;

public class TextureExtractor
{
    private static TextureExportFormat _format = TextureExportFormat.DDS_BGRA_UNCOMP_DX10;
    public static object _lock = new object();

    public static void SetTextureFormat(TextureExportFormat textureExportFormat)
    {
        _format = textureExportFormat;
    }

    public static bool SaveTextureToFile(string savePath, ScratchImage scratchImage, TextureDimension dimension = TextureDimension.D2, TextureExportFormat? overrideFormat = null)
    {
        try
        {
            lock (_lock)
            {
                if (savePath.Contains('.')) // TODO: Figure this out
                {
                    scratchImage.Dispose();
                    return false;
                }

                switch (overrideFormat != null ? overrideFormat : _format)
                {
                    case TextureExportFormat.DDS_BGRA_UNCOMP_DX10:
                        scratchImage.SaveToDDSFile(DDS_FLAGS.FORCE_DX10_EXT, savePath + ".dds");
                        break;
                    case TextureExportFormat.DDS_BGRA_BC3_DX10:
                        if (TexHelper.Instance.IsSRGB(scratchImage.GetMetadata().Format))
                            scratchImage = scratchImage.Compress(DXGI_FORMAT.BC3_UNORM_SRGB, TEX_COMPRESS_FLAGS.SRGB, 0);
                        else
                            scratchImage = scratchImage.Compress(DXGI_FORMAT.BC3_UNORM, TEX_COMPRESS_FLAGS.DEFAULT, 0);

                        scratchImage.SaveToDDSFile(DDS_FLAGS.FORCE_DX9_LEGACY, savePath + ".dds");
                        break;
                    case TextureExportFormat.DDS_BGRA_UNCOMP:
                        scratchImage.SaveToDDSFile(DDS_FLAGS.FORCE_DX9_LEGACY, savePath + ".dds");
                        break;
                    case TextureExportFormat.PNG:
                        Guid guid = TexHelper.Instance.GetWICCodec(WICCodecs.PNG);
                        switch (dimension)
                        {
                            //case TextureDimension.D3 when ConfigSubsystem.Get().GetS2ShaderExportEnabled():
                            //    Texture.FlattenVolume(scratchImage).SaveToWICFile(0, WIC_FLAGS.NONE, guid, savePath + ".png");
                            //    break;
                            case TextureDimension.CUBE when scratchImage.GetImageCount() == 6:
                                Texture.FlattenCubemap(scratchImage).SaveToWICFile(0, WIC_FLAGS.NONE, guid, savePath + ".png");
                                break;
                            default:
                                scratchImage.SaveToWICFile(0, WIC_FLAGS.NONE, guid, savePath + ".png");
                                break;
                        }
                        break;
                    case TextureExportFormat.TGA:
                        switch (dimension)
                        {
                            //case TextureDimension.D3 when ConfigSubsystem.Get().GetS2ShaderExportEnabled():
                            //    Texture.FlattenVolume(scratchImage).SaveToTGAFile(0, savePath + ".tga");
                            //    break;
                            case TextureDimension.CUBE when scratchImage.GetImageCount() == 6:
                                Texture.FlattenCubemap(scratchImage).SaveToTGAFile(0, savePath + ".tga");
                                break;
                            default:
                                scratchImage.SaveToTGAFile(0, savePath + ".tga");
                                break;
                        }
                        break;
                }
                scratchImage.Dispose();
                return true;
            }
        }
        catch (Exception e)
        {
            Log.Error($"{e.Message}");
            scratchImage.Dispose();
            return false;
        }
    }

    public static string GetExtension(TextureExportFormat exportFormat)
    {
        switch (exportFormat)
        {
            case TextureExportFormat.DDS_BGRA_UNCOMP_DX10:
            case TextureExportFormat.DDS_BGRA_BC3_DX10:
            case TextureExportFormat.DDS_BGRA_UNCOMP:
                return "dds";
            case TextureExportFormat.PNG:
                return "png";
            case TextureExportFormat.TGA:
                return "tga";
        }

        return String.Empty;
    }
}

public enum TextureExportFormat
{
    DDS_BGRA_UNCOMP_DX10,
    DDS_BGRA_BC3_DX10,
    DDS_BGRA_UNCOMP,
    PNG,
    TGA,
}
