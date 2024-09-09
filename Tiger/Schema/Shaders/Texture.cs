using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using Arithmic;
using DirectXTex;
using DirectXTexNet;
using Newtonsoft.Json;

namespace Tiger.Schema;

public class Texture : TigerReferenceFile<STextureHeader>
{
    public Texture(FileHash hash) : base(hash)
    {
    }

    public bool IsCubemap()
    {
        return _tag.ArraySize == 6;
    }

    public bool IsVolume()
    {
        return _tag.Depth != 1;
    }

    public TextureDimension GetDimension()
    {
        if (IsCubemap())
            return TextureDimension.CUBE;
        else if (IsVolume())
            return TextureDimension.D3;
        else
            return TextureDimension.D2;
    }

    public byte[] GetDDSBytes(DXGI_FORMAT format)
    {
        byte[] data;
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            if (ReferenceHash.IsValid() && ReferenceHash.GetReferenceHash().IsValid())
                data = PackageResourcer.Get().GetFileData(ReferenceHash.GetReferenceHash());
            else
                data = GetReferenceData();

            if ((_tag.Flags1 & 0xC00) != 0x400 || IsCubemap())
            {
                var gcnformat = GcnSurfaceFormatExtensions.GetFormat(_tag.ROIFormat);
                data = PS4SwizzleAlgorithm.UnSwizzle(data, _tag.Width, _tag.Height, _tag.ArraySize, gcnformat);
            }
        }
        else
        {
            if (_tag.LargeTextureBuffer != null)
                data = _tag.LargeTextureBuffer.GetData(false);
            else
                data = GetReferenceData();
        }

        DirectXTexUtility.TexMetadata metadata = DirectXTexUtility.GenerateMetaData(_tag.Width, _tag.Height, _tag.Depth, 1, (DirectXTexUtility.DXGIFormat)format, _tag.ArraySize == 6);
        DirectXTexUtility.DDSHeader ddsHeader;
        DirectXTexUtility.DX10Header dx10Header;
        DirectXTexUtility.GenerateDDSHeader(metadata, DirectXTexUtility.DDSFlags.NONE, out ddsHeader, out dx10Header);
        byte[] tag = DirectXTexUtility.EncodeDDSHeader(ddsHeader, dx10Header);
        byte[] final = new byte[data.Length + tag.Length];
        Array.Copy(tag, 0, final, 0, tag.Length);
        Array.Copy(data, 0, final, tag.Length, data.Length);

        return final;
    }

    public ScratchImage GetScratchImage(bool overrideConvert = false)
    {
        DXGI_FORMAT format = _tag.GetFormat();

        byte[] final = GetDDSBytes(format);
        GCHandle gcHandle = GCHandle.Alloc(final, GCHandleType.Pinned);
        IntPtr pixelPtr = gcHandle.AddrOfPinnedObject();
        var scratchImage = TexHelper.Instance.LoadFromDDSMemory(pixelPtr, final.Length, DDS_FLAGS.NONE);
        gcHandle.Free();

        if (IsCubemap())
        {
            if (scratchImage.GetMetadata().ArraySize != 6)
            {
                Log.Error($"Cubemap texture '{Hash}' has invalid array size '{scratchImage.GetMetadata().ArraySize}'");
                return scratchImage;
            }
            if (TexHelper.Instance.IsCompressed(format))
            {
                scratchImage = DecompressScratchImage(scratchImage, IsSrgb() ? DXGI_FORMAT.R8G8B8A8_UNORM_SRGB : DXGI_FORMAT.R8G8B8A8_UNORM);
            }
            var s1 = scratchImage.FlipRotate(2, TEX_FR_FLAGS.FLIP_VERTICAL).FlipRotate(0, TEX_FR_FLAGS.FLIP_HORIZONTAL);
            var s2 = scratchImage.FlipRotate(0, TEX_FR_FLAGS.ROTATE90);
            var s3 = scratchImage.FlipRotate(1, TEX_FR_FLAGS.ROTATE270);
            var s4 = scratchImage.FlipRotate(4, TEX_FR_FLAGS.FLIP_VERTICAL).FlipRotate(0, TEX_FR_FLAGS.FLIP_HORIZONTAL);
            scratchImage = TexHelper.Instance.InitializeTemporary(
                new[]
                {
                    s3.GetImage(0), s2.GetImage(0), s4.GetImage(0),
                    scratchImage.GetImage(5), s1.GetImage(0), scratchImage.GetImage(3),
                },
                scratchImage.GetMetadata());
        }
        else
        {
            try
            {
                if (!overrideConvert)
                {
                    if (TexHelper.Instance.IsCompressed(format))
                        scratchImage = DecompressScratchImage(scratchImage, IsSrgb() ? DXGI_FORMAT.B8G8R8A8_UNORM_SRGB : DXGI_FORMAT.B8G8R8A8_UNORM);
                    else if (IsSrgb())
                        scratchImage = scratchImage.Convert(DXGI_FORMAT.B8G8R8A8_UNORM_SRGB, TEX_FILTER_FLAGS.SEPARATE_ALPHA, 0);
                    else
                        scratchImage = scratchImage.Convert(DXGI_FORMAT.B8G8R8A8_UNORM, 0, 0);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

        }

        // scratchImage = scratchImage.Convert(DXGI_FORMAT.B8G8R8A8_UNORM, TEX_FILTER_FLAGS.RGB_COPY_RED, 0);

        return scratchImage;
    }

    private ScratchImage DecompressScratchImage(ScratchImage scratchImage, DXGI_FORMAT format)
    {
        while (true)
        {
            try
            {
                scratchImage = scratchImage.Decompress(format);
                return scratchImage;
            }
            catch (AccessViolationException)
            {
            }
        }
    }

    public static ScratchImage FlattenCubemap(ScratchImage input)
    {
        var image = input.GetImage(0);
        if (image.Width == 0)
        {
            return null;
        }

        bool bSrgb = TexHelper.Instance.IsSRGB(image.Format);
        int faceWidth = image.Width;
        int faceHeight = image.Height;

        // Define the dimensions for the output image (4x3 layout)
        int outputWidth = faceWidth * 4;
        int outputHeight = faceHeight * 3;

        ScratchImage outputPlate = TexHelper.Instance.Initialize2D(
            bSrgb ? DXGI_FORMAT.B8G8R8A8_UNORM_SRGB : DXGI_FORMAT.B8G8R8A8_UNORM,
            outputWidth, outputHeight, 1, 0, 0);

        // Arrange the faces in a 4x3 layout
        // Define the positions for each cubemap face
        var facePositions = new (int x, int y)[]
        {
            (faceWidth * 2, faceHeight),       // +Z (Front) 
            (0, faceHeight),                   // -X (Left)
            (faceWidth, 0),                    // +Y (Up)
            (faceWidth, faceHeight * 2),       // -Y (Down)
            (faceWidth, faceHeight),           // +X (Right)
            (faceWidth * 3, faceHeight)        // -Z (Back)
        };

        for (int i = 0; i < input.GetImageCount(); i++)
        {
            var (x, y) = facePositions[i];
            TexHelper.Instance.CopyRectangle(
                input.GetImage(i), 0, 0, faceWidth, faceHeight,
                outputPlate.GetImage(0),
                bSrgb ? TEX_FILTER_FLAGS.SEPARATE_ALPHA : 0,
                x, y);
        }

        input.Dispose();
        return outputPlate;
    }

    public static ScratchImage FlattenVolume(ScratchImage input)
    {
        var image = input.GetImage(0);
        if (image.Width == 0)
        {
            return null;
        }

        bool bSrgb = TexHelper.Instance.IsSRGB(image.Format);
        ScratchImage outputPlate = TexHelper.Instance.Initialize2D(image.Format, image.Width * input.GetImageCount(), image.Height, 1, 0, 0);

        for (int i = 0; i < input.GetImageCount(); i++)
        {
            TexHelper.Instance.CopyRectangle(input.GetImage(i), 0, 0, image.Width, image.Height, outputPlate.GetImage(0), bSrgb ? TEX_FILTER_FLAGS.SEPARATE_ALPHA : 0, image.Width * i, 0);
        }
        input.Dispose();
        return outputPlate;
    }

    public bool IsSrgb()
    {
        return TexHelper.Instance.IsSRGB(_tag.GetFormat());
    }

    public UnmanagedMemoryStream GetTexture()
    {
        ScratchImage scratchImage = GetScratchImage();

        UnmanagedMemoryStream ms;
        if (IsCubemap())
        {
            ms = scratchImage.SaveToDDSMemory(DDS_FLAGS.NONE);
        }
        else
        {
            Guid guid = TexHelper.Instance.GetWICCodec(WICCodecs.BMP);
            ms = scratchImage.SaveToWICMemory(0, WIC_FLAGS.NONE, guid);
        }
        scratchImage.Dispose();
        return ms;
    }

    public UnmanagedMemoryStream GetCubemapFace(int index)
    {
        ScratchImage scratchImage = GetScratchImage();

        UnmanagedMemoryStream ms;
        Guid guid = TexHelper.Instance.GetWICCodec(WICCodecs.BMP);
        ms = scratchImage.SaveToWICMemory(index, WIC_FLAGS.NONE, guid);
        scratchImage.Dispose();
        return ms;
    }

    public static void SavetoFile(string savePath, ScratchImage simg, TextureDimension dimension = TextureDimension.D2)
    {
        try
        {
            TextureExtractor.SaveTextureToFile(savePath, simg, dimension);
        }
        catch (FileLoadException e)
        {
            Log.Error(e.Message);
        }
    }

    public void SavetoFile(string savePath)
    {
        ScratchImage simg = GetScratchImage();
        if (ConfigSubsystem.Get().GetS2TexPow2Enabled())
            simg = ResizeToNearestPowerOf2(simg);

        if (ConfigSubsystem.Get().GetS2ShaderExportEnabled())
        {
            if (_tag.LargeTextureBuffer == null) // ??
            {
                try
                {
                    File.WriteAllText($"{savePath}.{ConfigSubsystem.Get().GetOutputTextureFormat().ToString().ToLower()}.meta", JsonConvert.SerializeObject(new { nomip = 1 }, Formatting.Indented));
                }
                catch (IOException)
                {
                }
            }
        }

        SavetoFile(savePath, simg, GetDimension());// || (IsVolume() && !ConfigSubsystem.Get().GetS2ShaderExportEnabled()));
    }

    public UnmanagedMemoryStream GetTextureToDisplay()
    {
        ScratchImage scratchImage = GetScratchImage();
        UnmanagedMemoryStream ms;
        if (IsCubemap())
        {
            // TODO add assemble feature to show the entire cubemap in display
            Guid guid = TexHelper.Instance.GetWICCodec(WICCodecs.BMP);
            ms = scratchImage.SaveToWICMemory(0, WIC_FLAGS.NONE, guid);
        }
        else
        {
            Guid guid = TexHelper.Instance.GetWICCodec(WICCodecs.BMP);
            ms = scratchImage.SaveToWICMemory(0, WIC_FLAGS.NONE, guid);
        }
        scratchImage.Dispose();
        return ms;
    }

    public ScratchImage ResizeToNearestPowerOf2(ScratchImage image)
    {
        var metadata = image.GetMetadata();
        int width = metadata.Width;
        int height = metadata.Height;

        int newWidth = NearestPowerOfTwo(width);
        int newHeight = NearestPowerOfTwo(height);

        if (newWidth == width && newHeight == height)
            return image;

        return image.Resize(newWidth, newHeight, TEX_FILTER_FLAGS.SEPARATE_ALPHA);
    }

    private static int NearestPowerOfTwo(int value)
    {
        return 1 << (sizeof(uint) * 8 - BitOperations.LeadingZeroCount((uint)(value - 1)));
    }
}

public enum TextureDimension
{
    [Description("1D")]
    D1,
    [Description("2D")]
    D2,
    [Description("3D")]
    D3,
    [Description("1DArray")]
    D1ARRAY,
    [Description("2DArray")]
    D2ARRAY,
    [Description("3DArray")]
    D3ARRAY,
    [Description("CUBE")]
    CUBE,
    [Description("CUBEARRAY")]
    CUBEARRAY
}

// Adding D1 stuff to everything is gonna become an ugly mess...
[NonSchemaStruct(0x40, 32, new[] { 1, 2, 3 })]
public struct STextureHeader
{
    public uint DataSize;
    [SchemaField(0x06, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public ushort ROIFormat;

    [SchemaField(0x04, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public uint Format;  // DXGI_FORMAT, ushort GcnSurfaceFormat for ROI

    [SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public float Unk10;
    [SchemaField(0x14, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public float Unk14;

    [SchemaField(0x24, TigerStrategy.DESTINY1_RISE_OF_IRON)] // is BEEFCAFE (uint32) in D1
    [SchemaField(0x0C, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public ushort CAFE;

    [SchemaField(0x28, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x0E, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x22, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public ushort Width;
    public ushort Height;
    public ushort Depth;
    public ushort ArraySize;

    [SchemaField(0x30, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public uint Flags1; // Flags for ROI
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public uint Flags2;
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public uint Flags3;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x24, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x3C, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public TigerFile? LargeTextureBuffer;

    public DXGI_FORMAT GetFormat()
    {
        switch (Strategy.CurrentStrategy)
        {
            case TigerStrategy.DESTINY1_RISE_OF_IRON:
                return GcnSurfaceFormatExtensions.ToDXGI(ROIFormat);
            default:
                return (DXGI_FORMAT)Format;
        }
    }
}

