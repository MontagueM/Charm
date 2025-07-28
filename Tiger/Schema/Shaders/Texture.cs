using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using Arithmic;
using DirectXTex;
using DirectXTexNet;
using Newtonsoft.Json;
using Tiger.Schema.Entity;

namespace Tiger.Schema;

public class Texture : TigerReferenceFile<STextureHeader>
{
    public Texture(FileHash hash) : base(hash)
    {
    }

    public int Width => _tag.Width;
    public int Height => _tag.Height;

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
        if (Strategy.IsD1())
        {
            if (ReferenceHash.IsValid() && ReferenceHash.GetReferenceHash().IsValid())
                data = PackageResourcer.Get().GetFileData(ReferenceHash.GetReferenceHash());
            else
                data = GetReferenceData();

            //if ((_tag.Flags1 & 0xC00) != 0x400 || IsCubemap())
            if ((_tag.Flags1 & 0xF00) != 0x500 || IsCubemap())
            {
                GcnSurfaceFormatExtensions.GcnSurfaceFormat gcnformat = GcnSurfaceFormatExtensions.GetFormat(_tag.ROIFormat);
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

    public ScratchImage GetScratchImage(bool overrideConvert = false, int index = 0)
    {
        DXGI_FORMAT format = _tag.GetFormat();

        byte[] final = GetDDSBytes(format);
        GCHandle gcHandle = GCHandle.Alloc(final, GCHandleType.Pinned);
        IntPtr pixelPtr = gcHandle.AddrOfPinnedObject();
        ScratchImage scratchImage = TexHelper.Instance.LoadFromDDSMemory(pixelPtr, final.Length, DDS_FLAGS.NONE);
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
            ScratchImage s1 = scratchImage.FlipRotate(2, TEX_FR_FLAGS.FLIP_VERTICAL).FlipRotate(0, TEX_FR_FLAGS.FLIP_HORIZONTAL);
            ScratchImage s2 = scratchImage.FlipRotate(0, TEX_FR_FLAGS.ROTATE90);
            ScratchImage s3 = scratchImage.FlipRotate(1, TEX_FR_FLAGS.ROTATE270);
            ScratchImage s4 = scratchImage.FlipRotate(4, TEX_FR_FLAGS.FLIP_VERTICAL).FlipRotate(0, TEX_FR_FLAGS.FLIP_HORIZONTAL);
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

        if (index != 0 && index < scratchImage.GetImageCount())
        {
            // Have to change the metadata to act like its a 2D texture if we want to do anything else with it later (such as resizing)
            TexMetadata metadata = scratchImage.GetMetadata();
            metadata.Depth = 1;
            metadata.Dimension = TEX_DIMENSION.TEXTURE2D;
            scratchImage = TexHelper.Instance.InitializeTemporary(new[] { scratchImage.GetImage(index) }, metadata);
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
        Image image = input.GetImage(0);
        if (image.Width == 0)
            return null;

        if (input.GetImageCount() != 6)
            return input;

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
            (int x, int y) = facePositions[i];
            TexHelper.Instance.CopyRectangle(
                input.GetImage(i), 0, 0, faceWidth, faceHeight,
                outputPlate.GetImage(0),
                bSrgb ? TEX_FILTER_FLAGS.SEPARATE_ALPHA : 0,
                x, y);
        }

        input.Dispose();
        return outputPlate;
    }

    public static ScratchImage FlattenVolume(ScratchImage input) // TODO: Figure out why R#G#_ formats dont work here
    {
        Image image = input.GetImage(0);
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
        if (!IsCubemap())
            index = 0;

        ScratchImage scratchImage = GetScratchImage();

        UnmanagedMemoryStream ms;
        Guid guid = TexHelper.Instance.GetWICCodec(WICCodecs.BMP);
        ms = scratchImage.SaveToWICMemory(index, WIC_FLAGS.NONE, guid);
        scratchImage.Dispose();
        return ms;
    }

    public UnmanagedMemoryStream GetVolumeSlice(int index)
    {
        if (!IsVolume())
            index = 0;

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

    public void SavetoFile(string savePath, int index = 0)
    {
        ScratchImage simg = GetScratchImage(index: index);
        if (ConfigSubsystem.Get().GetS2TexPow2Enabled()) // TODO make not shit
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
        TexMetadata metadata = image.GetMetadata();
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

    public static UnmanagedMemoryStream? GetTextureFromHash(FileHash hash)
    {
        Texture texture = FileResourcer.Get().GetFile<Texture>(hash);

        return texture.GetTexture();
    }
}

// todo move this
public class TexturePlate : Tag<S919E8080>
{
    public TexturePlate(FileHash hash) : base(hash)
    {
    }

    public ScratchImage? MakePlatedTexture()
    {
        IntVector2 dimension = GetPlateDimensions();
        if (dimension.X == 0)
        {
            return null;
        }

        using TigerReader reader = GetReader();
        bool bSrgb = _tag.PlateTransforms[reader, 0].Texture.IsSrgb();
        ScratchImage outputPlate = TexHelper.Instance.Initialize2D(bSrgb ? DXGI_FORMAT.B8G8R8A8_UNORM_SRGB : DXGI_FORMAT.B8G8R8A8_UNORM, dimension.X, dimension.Y, 1, 0, 0);

        foreach (S939E8080 transform in _tag.PlateTransforms.Enumerate(reader))
        {
            ScratchImage original = transform.Texture.GetScratchImage();
            ScratchImage resizedOriginal = original.Resize(transform.Scale.X, transform.Scale.Y, TEX_FILTER_FLAGS.SEPARATE_ALPHA);
            TexHelper.Instance.CopyRectangle(resizedOriginal.GetImage(0, 0, 0), 0, 0, transform.Scale.X, transform.Scale.Y, outputPlate.GetImage(0, 0, 0), bSrgb ? TEX_FILTER_FLAGS.SEPARATE_ALPHA : 0, transform.Translation.X, transform.Translation.Y);
            original.Dispose();
            resizedOriginal.Dispose();
        }
        return outputPlate;
    }

    public void SavePlatedTexture(string savePath)
    {
        ScratchImage? simg = MakePlatedTexture();
        if (simg != null)
        {
            Texture.SavetoFile(savePath, simg);
        }
    }

    public IntVector2 GetPlateDimensions()
    {
        int maxDimension = 0;  // plate must be square
        foreach (S939E8080 transform in _tag.PlateTransforms.Enumerate(GetReader()))
        {
            if (transform.Translation.X + transform.Scale.X > maxDimension)
            {
                maxDimension = transform.Translation.X + transform.Scale.X;
            }
            if (transform.Translation.Y + transform.Scale.Y > maxDimension)
            {
                maxDimension = transform.Translation.Y + transform.Scale.Y;
            }
        }

        // Find power of two that fits this dimension, ie round up the exponent to nearest integer
        maxDimension = (int)Math.Pow(2, Math.Ceiling(Math.Log2(maxDimension)));

        return new IntVector2(maxDimension, maxDimension);
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

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Vector4 TilingScaleOffset;

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

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public ushort TileCount;

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
        return Strategy.CurrentStrategy switch
        {
            TigerStrategy.DESTINY1_RISE_OF_IRON => GcnSurfaceFormatExtensions.ToDXGI(ROIFormat),
            _ => (DXGI_FORMAT)Format,
        };
    }
}

