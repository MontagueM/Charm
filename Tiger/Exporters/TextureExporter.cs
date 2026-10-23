using System.Collections.Concurrent;
using System.ComponentModel;
using Arithmic;
using DirectXTexNet;
using Tiger.Schema;

namespace Tiger.Exporters;

public class TextureExporter
{
    private static TextureExportFormat _format = TextureExportFormat.PNG;

    // Saves currently being written, keyed by output file. Different textures save in parallel, but two saves of the
    // same file at once (a texture shared by two materials) would fight over it, so the second one waits for the
    // first and reuses its result instead of writing it again.
    private static readonly ConcurrentDictionary<string, Lazy<bool>> _inFlightSaves = new(StringComparer.OrdinalIgnoreCase);

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

    /// <summary>
    /// Saves and then disposes <paramref name="scratchImage"/>. Safe to call from several threads at once.
    /// </summary>
    public static bool SaveTextureToFile(string savePath, ScratchImage scratchImage,
        TextureDimension dimension = TextureDimension.D2, TextureExportFormat? overrideFormat = null)
    {
        var format = overrideFormat ?? _format;
        string key = $"{GetFullPathOrSelf(savePath)}|{format}";

        Lazy<bool> save = new(() => SaveTextureToFileCore(savePath, scratchImage, dimension, format));
        Lazy<bool> activeSave = _inFlightSaves.GetOrAdd(key, save);
        if (!ReferenceEquals(activeSave, save))
        {
            scratchImage.Dispose();
            return activeSave.Value;
        }

        try
        {
            return save.Value;
        }
        finally
        {
            _inFlightSaves.TryRemove(new KeyValuePair<string, Lazy<bool>>(key, save));
        }
    }

    private static string GetFullPathOrSelf(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception)
        {
            return path;
        }
    }

    private static bool SaveTextureToFileCore(string savePath, ScratchImage scratchImage, TextureDimension dimension, TextureExportFormat format)
    {
        try
        {
            if (Path.GetFileName(savePath).Contains('.')) // TODO: Figure this out
            {
                Log.Error($"[BUG] Save Path {savePath} contains a period, cant export texture!");
                scratchImage.Dispose();
                return false;
            }

            switch (format)
            {
                case TextureExportFormat.DDS_BGRA_UNCOMP_DX10:
                    scratchImage.SaveToDDSFile(DDS_FLAGS.FORCE_DX10_EXT, savePath + ".dds");
                    break;

                case TextureExportFormat.DDS_BGRA_BC3_DX10:
                    using (ScratchImage compressed = CompressBC3(scratchImage))
                    {
                        compressed.SaveToDDSFile(DDS_FLAGS.FORCE_DX9_LEGACY, savePath + ".dds");
                    }
                    break;

                case TextureExportFormat.PNG:
                    using (ScratchImage savable = MakeSavable(scratchImage, format))
                    {
                        SavePNGTexture(savable, savePath, dimension, WICCodecs.PNG);
                    }
                    break;

                case TextureExportFormat.TGA:
                    using (ScratchImage savable = MakeSavable(scratchImage, format))
                    {
                        SaveTGATexture(savable, savePath, dimension);
                    }
                    break;
            }

            scratchImage.Dispose();
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"{Path.GetFileName(savePath)}: could not save {DescribeImage(scratchImage)} as {format}: {e.Message}");
            scratchImage.Dispose();
            return false;
        }
    }

    private static readonly HashSet<DXGI_FORMAT> _wicFormats = new()
    {
        DXGI_FORMAT.R32G32B32A32_FLOAT, DXGI_FORMAT.R16G16B16A16_FLOAT, DXGI_FORMAT.R16G16B16A16_UNORM,
        DXGI_FORMAT.R8G8B8A8_UNORM, DXGI_FORMAT.R8G8B8A8_UNORM_SRGB, DXGI_FORMAT.B8G8R8A8_UNORM, DXGI_FORMAT.B8G8R8A8_UNORM_SRGB,
        DXGI_FORMAT.B8G8R8X8_UNORM, DXGI_FORMAT.B8G8R8X8_UNORM_SRGB, DXGI_FORMAT.R10G10B10A2_UNORM,
        DXGI_FORMAT.R10G10B10_XR_BIAS_A2_UNORM, DXGI_FORMAT.B5G5R5A1_UNORM, DXGI_FORMAT.B5G6R5_UNORM,
        DXGI_FORMAT.R32_FLOAT, DXGI_FORMAT.R16_FLOAT, DXGI_FORMAT.R16_UNORM, DXGI_FORMAT.R8_UNORM, DXGI_FORMAT.A8_UNORM,
    };

    private static readonly HashSet<DXGI_FORMAT> _tgaFormats = new()
    {
        DXGI_FORMAT.R8G8B8A8_UNORM, DXGI_FORMAT.R8G8B8A8_UNORM_SRGB, DXGI_FORMAT.B8G8R8A8_UNORM, DXGI_FORMAT.B8G8R8A8_UNORM_SRGB,
        DXGI_FORMAT.B8G8R8X8_UNORM, DXGI_FORMAT.B8G8R8X8_UNORM_SRGB, DXGI_FORMAT.B5G5R5A1_UNORM, DXGI_FORMAT.R8_UNORM, DXGI_FORMAT.A8_UNORM,
    };


    private static ScratchImage MakeSavable(ScratchImage scratchImage, TextureExportFormat format)
    {
        DXGI_FORMAT imageFormat = scratchImage.GetMetadata().Format;
        HashSet<DXGI_FORMAT> supported = format == TextureExportFormat.TGA ? _tgaFormats : _wicFormats;
        if (supported.Contains(imageFormat) || TexHelper.Instance.IsCompressed(imageFormat))
        {
            return TexHelper.Instance.InitializeTemporary(Enumerable.Range(0, scratchImage.GetImageCount()).Select(scratchImage.GetImage).ToArray(), scratchImage.GetMetadata());
        }

        string name = imageFormat.ToString();
        bool wide = name.Contains("16") || name.Contains("32") || name.StartsWith("R11G11B10") || name.StartsWith("R9G9B9E5");
        DXGI_FORMAT target = format == TextureExportFormat.PNG && wide ? DXGI_FORMAT.R16G16B16A16_UNORM : DXGI_FORMAT.R8G8B8A8_UNORM;
        Log.Debug($"Converting {imageFormat} to {target} to save as {format}");
        return scratchImage.Convert(target, TEX_FILTER_FLAGS.DEFAULT, 0.5f);
    }

    private static string DescribeImage(ScratchImage scratchImage)
    {
        try
        {
            TexMetadata metadata = scratchImage.GetMetadata();
            return $"{metadata.Format} {metadata.Dimension} {metadata.Width}x{metadata.Height} (depth {metadata.Depth}, array {metadata.ArraySize}, mips {metadata.MipLevels})";
        }
        catch (Exception)
        {
            return "texture";
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
