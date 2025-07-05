using Arithmic;
using DirectXTexNet;

namespace Tiger.Schema;

public class TextureExtractor
{
    private static TextureExportFormat _format = TextureExportFormat.DDS_BGRA_UNCOMP_DX10;
    public static object _lock = new();

    public static void SetTextureFormat(TextureExportFormat textureExportFormat)
    {
        _format = textureExportFormat;
    }

    public static void ExportTexture(FileHash fileHash, int index = 0)
    {
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        string pkgName = PackageResourcer.Get().GetPackage(fileHash.PackageId).GetPackageMetadata().Name.Split(".")[0];
        string savePath = config.GetExportSavePath() + $"/Textures/{pkgName}";
        Directory.CreateDirectory($"{savePath}/");

        Texture texture = FileResourcer.Get().GetFile<Texture>(fileHash);
        texture.SavetoFile($"{savePath}/{fileHash}", index);
    }

    public static bool SaveTextureToFile(string savePath, ScratchImage scratchImage, TextureDimension dimension = TextureDimension.D2, TextureExportFormat? overrideFormat = null)
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
                                scratchImage.SaveToWICFile(0, WIC_FLAGS.FORCE_SRGB, guid, savePath + ".png");
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
            Log.Error($"{Path.GetFileName(savePath)} : {e.Message}");
            scratchImage.Dispose();
            return false;
        }
    }

    public static string GetExtension(TextureExportFormat exportFormat)
    {
        return exportFormat switch
        {
            TextureExportFormat.DDS_BGRA_UNCOMP_DX10 or TextureExportFormat.DDS_BGRA_BC3_DX10 or TextureExportFormat.DDS_BGRA_UNCOMP => "dds",
            TextureExportFormat.PNG => "png",
            TextureExportFormat.TGA => "tga",
            _ => String.Empty,
        };
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
