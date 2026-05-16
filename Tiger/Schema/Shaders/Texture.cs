using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using Arithmic;
using DirectXTex;
using DirectXTexNet;
using Newtonsoft.Json;
using Tiger.Exporters;
using Tiger.Schema.Entity;
using static DirectXTex.DirectXTexUtility;
using TexMetadata = DirectXTexNet.TexMetadata;

namespace Tiger.Schema;

public class Texture : TigerReferenceFile<STextureHeader>
{
    public Texture(FileHash hash) : base(hash)
    {
    }

    public int Width => _tag.Width;
    public int Height => _tag.Height;
    public int Depth => IsCubemap() ? _tag.ArraySize : _tag.Depth;

    public bool IsCubemap()
    {
        return _tag.ArraySize == 6;
    }

    public bool IsVolume()
    {
        return _tag.Depth != 1;
    }

    public bool IsSrgb()
    {
        return TexHelper.Instance.IsSRGB(_tag.GetFormat());
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

    public byte[] GetRawBytes(bool loadMips = true)
    {
        byte[] data;
        if (Strategy.IsD1())
        {
            if (ReferenceHash.IsValid() && ReferenceHash.GetReferenceHash().IsValid())
                data = PackageResourcer.Get().GetFileData(ReferenceHash.GetReferenceHash());
            else
                data = GetReferenceData();

            if ((_tag.Flags1 & 0xF00) != 0x500 || IsCubemap() || IsVolume())
            {
                GcnSurfaceFormatExtensions.GcnSurfaceFormat gcnformat = GcnSurfaceFormatExtensions.GetFormat(_tag.ROIFormat);
                data = PS4SwizzleAlgorithm.UnSwizzle(data, _tag.Width, _tag.Height, IsVolume() ? _tag.Depth : _tag.ArraySize, gcnformat);
            }
        }
        else
        {
            if (_tag.LargeTextureBuffer != null)
            {
                if (loadMips)
                    data = Helpers.ConcatBytes(_tag.LargeTextureBuffer.GetData(false), GetReferenceData());
                else
                    data = _tag.LargeTextureBuffer.GetData(false);
            }
            else
                data = GetReferenceData();
        }
        return data;
    }

    public byte[] GetDDSBytes(DXGI_FORMAT format)
    {
        byte[] data = GetRawBytes(false);

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
                scratchImage = DecompressScratchImage(scratchImage,
                    format == DXGI_FORMAT.BC6H_UF16 ? DXGI_FORMAT.R16G16B16A16_UNORM : DXGI_FORMAT.R8G8B8A8_UNORM); //IsSrgb() ? DXGI_FORMAT.R8G8B8A8_UNORM_SRGB : DXGI_FORMAT.R8G8B8A8_UNORM);
                scratchImage = PremultiplyAlpha(scratchImage, TEX_PMALPHA_FLAGS.SRGB_OUT);
            }
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

    // Not a fix at all but this (and the method above) stops(?) the random release build only crash that can happen here, idfk whats going on im not smart enough for this shit man.
    private ScratchImage PremultiplyAlpha(ScratchImage scratchImage, TEX_PMALPHA_FLAGS flags)
    {
        while (true)
        {
            try
            {
                scratchImage = scratchImage.PremultiplyAlpha(flags);
                return scratchImage;
            }
            catch (AccessViolationException)
            {
            }
        }
    }

    public static ScratchImage FlattenCubemap(ScratchImage input)
    {
        if (input == null)
            return null;

        if (input.GetImageCount() != 6)
            return input;

        var img0 = input.GetImage(0);
        if (img0 == null || img0.Width == 0)
            return null;

        int w = img0.Width;
        int h = img0.Height;

        int outW = w * 4;
        int outH = h * 3;
        var output = TexHelper.Instance.Initialize2D(
            img0.Format,
            outW, outH, 1, 0, 0);

        ScratchImage xPos = input.FlipRotate(0, TEX_FR_FLAGS.ROTATE90);
        ScratchImage xNeg = input.FlipRotate(1, TEX_FR_FLAGS.ROTATE270);
        ScratchImage yPos = input.FlipRotate(2, TEX_FR_FLAGS.ROTATE180);
        ScratchImage yNeg = input.CreateImageCopy(3, false, 0);
        ScratchImage zPos = input.FlipRotate(4, TEX_FR_FLAGS.ROTATE90);
        ScratchImage zNeg = input.FlipRotate(5, TEX_FR_FLAGS.ROTATE90);

        //    -- Z+ -- --
        //    Y-  X+  Y+  X-
        //    -- Z- -- --
        TexHelper.Instance.CopyRectangle(zPos.GetImage(0), 0, 0, w, h, output.GetImage(0), 0, w, 0);
        TexHelper.Instance.CopyRectangle(yNeg.GetImage(0), 0, 0, w, h, output.GetImage(0), 0, 0, h);
        TexHelper.Instance.CopyRectangle(xPos.GetImage(0), 0, 0, w, h, output.GetImage(0), 0, w, h);
        TexHelper.Instance.CopyRectangle(yPos.GetImage(0), 0, 0, w, h, output.GetImage(0), 0, w * 2, h);
        TexHelper.Instance.CopyRectangle(xNeg.GetImage(0), 0, 0, w, h, output.GetImage(0), 0, w * 3, h);
        TexHelper.Instance.CopyRectangle(zNeg.GetImage(0), 0, 0, w, h, output.GetImage(0), 0, w, h * 2);

        xPos.Dispose();
        xNeg.Dispose();
        yPos.Dispose();
        yNeg.Dispose();
        zPos.Dispose();
        zNeg.Dispose();

        return output;
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
        if (index == 0) // for thumbnail purposes
            scratchImage = scratchImage.FlipRotate(0, TEX_FR_FLAGS.ROTATE90);

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
            TextureExporter.SaveTextureToFile(savePath, simg, dimension);
        }
        catch (FileLoadException e)
        {
            Log.Error(e.Message);
        }
    }

    public void SavetoFile(string savePath, int index = 0)
    {
        if (Hash.CheckRedacted())
        {
            Log.Warning($"Texture {Hash} is Redacted. Can not export.");
            return;
        }

        ScratchImage simg = GetScratchImage(index: index);
        if (ConfigSubsystem.Get().GetSBoxExportEnabled()) // TODO make not shit
            simg = ResizeToNearestPowerOf2(simg);

        if (ConfigSubsystem.Get().GetSBoxExportEnabled())
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

        SavetoFile(savePath, simg, GetDimension());// || (IsVolume() && !ConfigSubsystem.Get().GetSBoxExportEnabled()));
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

    public static void ComputePitch(DXGI_FORMAT format, long width, long height, out long rowPitch, out long slicePitch, CPFLAGS flags)
    {
        switch (format)
        {
            case DXGI_FORMAT.BC1_TYPELESS:
            case DXGI_FORMAT.BC1_UNORM:
            case DXGI_FORMAT.BC1_UNORM_SRGB:
            case DXGI_FORMAT.BC4_TYPELESS:
            case DXGI_FORMAT.BC4_UNORM:
            case DXGI_FORMAT.BC4_SNORM:
                {
                    if (flags.HasFlag(CPFLAGS.BADDXTNTAILS))
                    {
                        long nbw = width >> 2;
                        long nbh = height >> 2;
                        rowPitch = Math.Clamp(1, nbw * 8, Int64.MaxValue);
                        slicePitch = Math.Clamp(1, rowPitch * nbh, Int64.MaxValue);
                    }
                    else
                    {
                        long nbw = Math.Clamp(1, (width + 3) / 4, Int64.MaxValue);
                        long nbh = Math.Clamp(1, (height + 3) / 4, Int64.MaxValue);
                        rowPitch = nbw * 8;
                        slicePitch = rowPitch * nbh;
                    }
                }
                break;
            case DXGI_FORMAT.BC2_TYPELESS:
            case DXGI_FORMAT.BC2_UNORM:
            case DXGI_FORMAT.BC2_UNORM_SRGB:
            case DXGI_FORMAT.BC3_TYPELESS:
            case DXGI_FORMAT.BC3_UNORM:
            case DXGI_FORMAT.BC3_UNORM_SRGB:
            case DXGI_FORMAT.BC5_TYPELESS:
            case DXGI_FORMAT.BC5_UNORM:
            case DXGI_FORMAT.BC5_SNORM:
            case DXGI_FORMAT.BC6H_TYPELESS:
            case DXGI_FORMAT.BC6H_UF16:
            case DXGI_FORMAT.BC6H_SF16:
            case DXGI_FORMAT.BC7_TYPELESS:
            case DXGI_FORMAT.BC7_UNORM:
            case DXGI_FORMAT.BC7_UNORM_SRGB:
                {
                    if (flags.HasFlag(CPFLAGS.BADDXTNTAILS))
                    {
                        long nbw = width >> 2;
                        long nbh = height >> 2;
                        rowPitch = Math.Clamp(1, nbw * 16, Int64.MaxValue);
                        slicePitch = Math.Clamp(1, rowPitch * nbh, Int64.MaxValue);
                    }
                    else
                    {
                        long nbw = Math.Clamp(1, (width + 3) / 4, Int64.MaxValue);
                        long nbh = Math.Clamp(1, (height + 3) / 4, Int64.MaxValue);
                        rowPitch = nbw * 16;
                        slicePitch = rowPitch * nbh;
                    }
                }
                break;
            case DXGI_FORMAT.R8G8_B8G8_UNORM:
            case DXGI_FORMAT.G8R8_G8B8_UNORM:
            case DXGI_FORMAT.YUY2:
                rowPitch = ((width + 1) >> 1) * 4;
                slicePitch = rowPitch * height;
                break;
            case DXGI_FORMAT.Y210:
            case DXGI_FORMAT.Y216:
                rowPitch = ((width + 1) >> 1) * 8;
                slicePitch = rowPitch * height;
                break;

            case DXGI_FORMAT.NV12:
            case DXGI_FORMAT.OPAQUE_420:
                rowPitch = ((width + 1) >> 1) * 2;
                slicePitch = rowPitch * (height + ((height + 1) >> 1));
                break;

            case DXGI_FORMAT.P010:
            case DXGI_FORMAT.P016:
                rowPitch = ((width + 1) >> 1) * 4;
                slicePitch = rowPitch * (height + ((height + 1) >> 1));
                break;
            case DXGI_FORMAT.NV11:
                rowPitch = ((width + 3) >> 2) * 4;
                slicePitch = rowPitch * height * 2;
                break;
            default:
                {

                    long bpp;

                    if (flags.HasFlag(CPFLAGS.BPP24))
                        bpp = 24;
                    else if (flags.HasFlag(CPFLAGS.BPP16))
                        bpp = 16;
                    else if (flags.HasFlag(CPFLAGS.BPP8))
                        bpp = 8;
                    else
                        bpp = BitsPerPixel(format);

                    if (flags.HasFlag(CPFLAGS.LEGACYDWORD | CPFLAGS.PARAGRAPH | CPFLAGS.YMM | CPFLAGS.ZMM | CPFLAGS.PAGE4K))
                    {
                        if (flags.HasFlag(CPFLAGS.PAGE4K))
                        {
                            rowPitch = ((width * bpp + 32767) / 32768) * 4096;
                            slicePitch = rowPitch * height;
                        }
                        else if (flags.HasFlag(CPFLAGS.ZMM))
                        {
                            rowPitch = ((width * bpp + 511) / 512) * 64;
                            slicePitch = rowPitch * height;
                        }
                        else if (flags.HasFlag(CPFLAGS.YMM))
                        {
                            rowPitch = ((width * bpp + 255) / 256) * 32;
                            slicePitch = rowPitch * height;
                        }
                        else if (flags.HasFlag(CPFLAGS.PARAGRAPH))
                        {
                            rowPitch = ((width * bpp + 127) / 128) * 16;
                            slicePitch = rowPitch * height;
                        }
                        else // DWORD alignment
                        {
                            // Special computation for some incorrectly created DDS files based on
                            // legacy DirectDraw assumptions about pitch alignment
                            rowPitch = ((width * bpp + 31) / 32) * 4;
                            slicePitch = rowPitch * height;
                        }
                    }
                    else
                    {
                        // Default byte alignment
                        rowPitch = (width * bpp + 7) / 8;
                        slicePitch = rowPitch * height;
                    }
                }
                break;
        }

        if (rowPitch <= 0 || slicePitch <= 0)
            throw new InvalidOperationException($"Invalid pitch: rowPitch={rowPitch}, slicePitch={slicePitch}, format={format}, width={width}, height={height}");
    }

    private static long BitsPerPixel(DXGI_FORMAT format)
    {
        return format switch
        {
            DXGI_FORMAT.R32G32B32A32_TYPELESS or DXGI_FORMAT.R32G32B32A32_FLOAT or DXGI_FORMAT.R32G32B32A32_UINT or DXGI_FORMAT.R32G32B32A32_SINT => 128,
            DXGI_FORMAT.R32G32B32_TYPELESS or DXGI_FORMAT.R32G32B32_FLOAT or DXGI_FORMAT.R32G32B32_UINT or DXGI_FORMAT.R32G32B32_SINT => 96,
            DXGI_FORMAT.R16G16B16A16_TYPELESS or DXGI_FORMAT.R16G16B16A16_FLOAT or DXGI_FORMAT.R16G16B16A16_UNORM or DXGI_FORMAT.R16G16B16A16_UINT or DXGI_FORMAT.R16G16B16A16_SNORM or DXGI_FORMAT.R16G16B16A16_SINT or DXGI_FORMAT.R32G32_TYPELESS or DXGI_FORMAT.R32G32_FLOAT or DXGI_FORMAT.R32G32_UINT or DXGI_FORMAT.R32G32_SINT or DXGI_FORMAT.R32G8X24_TYPELESS or DXGI_FORMAT.D32_FLOAT_S8X24_UINT or DXGI_FORMAT.R32_FLOAT_X8X24_TYPELESS or DXGI_FORMAT.X32_TYPELESS_G8X24_UINT or DXGI_FORMAT.Y416 or DXGI_FORMAT.Y210 or DXGI_FORMAT.Y216 => 64,
            DXGI_FORMAT.R10G10B10A2_TYPELESS or DXGI_FORMAT.R10G10B10A2_UNORM or DXGI_FORMAT.R10G10B10A2_UINT or DXGI_FORMAT.R11G11B10_FLOAT or DXGI_FORMAT.R8G8B8A8_TYPELESS or DXGI_FORMAT.R8G8B8A8_UNORM or DXGI_FORMAT.R8G8B8A8_UNORM_SRGB or DXGI_FORMAT.R8G8B8A8_UINT or DXGI_FORMAT.R8G8B8A8_SNORM or DXGI_FORMAT.R8G8B8A8_SINT or DXGI_FORMAT.R16G16_TYPELESS or DXGI_FORMAT.R16G16_FLOAT or DXGI_FORMAT.R16G16_UNORM or DXGI_FORMAT.R16G16_UINT or DXGI_FORMAT.R16G16_SNORM or DXGI_FORMAT.R16G16_SINT or DXGI_FORMAT.R32_TYPELESS or DXGI_FORMAT.D32_FLOAT or DXGI_FORMAT.R32_FLOAT or DXGI_FORMAT.R32_UINT or DXGI_FORMAT.R32_SINT or DXGI_FORMAT.R24G8_TYPELESS or DXGI_FORMAT.D24_UNORM_S8_UINT or DXGI_FORMAT.R24_UNORM_X8_TYPELESS or DXGI_FORMAT.X24_TYPELESS_G8_UINT or DXGI_FORMAT.R9G9B9E5_SHAREDEXP or DXGI_FORMAT.R8G8_B8G8_UNORM or DXGI_FORMAT.G8R8_G8B8_UNORM or DXGI_FORMAT.B8G8R8A8_UNORM or DXGI_FORMAT.B8G8R8X8_UNORM or DXGI_FORMAT.R10G10B10_XR_BIAS_A2_UNORM or DXGI_FORMAT.B8G8R8A8_TYPELESS or DXGI_FORMAT.B8G8R8A8_UNORM_SRGB or DXGI_FORMAT.B8G8R8X8_TYPELESS or DXGI_FORMAT.B8G8R8X8_UNORM_SRGB or DXGI_FORMAT.AYUV or DXGI_FORMAT.Y410 or DXGI_FORMAT.YUY2 => 32,
            DXGI_FORMAT.P010 or DXGI_FORMAT.P016 => 24,
            DXGI_FORMAT.R8G8_TYPELESS or DXGI_FORMAT.R8G8_UNORM or DXGI_FORMAT.R8G8_UINT or DXGI_FORMAT.R8G8_SNORM or DXGI_FORMAT.R8G8_SINT or DXGI_FORMAT.R16_TYPELESS or DXGI_FORMAT.R16_FLOAT or DXGI_FORMAT.D16_UNORM or DXGI_FORMAT.R16_UNORM or DXGI_FORMAT.R16_UINT or DXGI_FORMAT.R16_SNORM or DXGI_FORMAT.R16_SINT or DXGI_FORMAT.B5G6R5_UNORM or DXGI_FORMAT.B5G5R5A1_UNORM or DXGI_FORMAT.A8P8 or DXGI_FORMAT.B4G4R4A4_UNORM => 16,
            DXGI_FORMAT.NV12 or DXGI_FORMAT.OPAQUE_420 or DXGI_FORMAT.NV11 => 12,
            DXGI_FORMAT.R8_TYPELESS or DXGI_FORMAT.R8_UNORM or DXGI_FORMAT.R8_UINT or DXGI_FORMAT.R8_SNORM or DXGI_FORMAT.R8_SINT or DXGI_FORMAT.A8_UNORM or DXGI_FORMAT.AI44 or DXGI_FORMAT.IA44 or DXGI_FORMAT.P8 => 8,
            DXGI_FORMAT.R1_UNORM => 1,
            DXGI_FORMAT.BC1_TYPELESS or DXGI_FORMAT.BC1_UNORM or DXGI_FORMAT.BC1_UNORM_SRGB or DXGI_FORMAT.BC4_TYPELESS or DXGI_FORMAT.BC4_UNORM or DXGI_FORMAT.BC4_SNORM => 4,
            DXGI_FORMAT.BC2_TYPELESS or DXGI_FORMAT.BC2_UNORM or DXGI_FORMAT.BC2_UNORM_SRGB or DXGI_FORMAT.BC3_TYPELESS or DXGI_FORMAT.BC3_UNORM or DXGI_FORMAT.BC3_UNORM_SRGB or DXGI_FORMAT.BC5_TYPELESS or DXGI_FORMAT.BC5_UNORM or DXGI_FORMAT.BC5_SNORM or DXGI_FORMAT.BC6H_TYPELESS or DXGI_FORMAT.BC6H_UF16 or DXGI_FORMAT.BC6H_SF16 or DXGI_FORMAT.BC7_TYPELESS or DXGI_FORMAT.BC7_UNORM or DXGI_FORMAT.BC7_UNORM_SRGB => 8,
            _ => (long)0,
        };
    }

    public static unsafe ScratchImage CubemapCrossToEquirectangular(ScratchImage crossImage)
    {
        var meta = crossImage.GetMetadata();
        int faceSize = meta.Height / 3;
        int outWidth = faceSize * 4;
        int outHeight = faceSize * 2;

        ScratchImage output = TexHelper.Instance.Initialize2D(
            meta.Format,
            outWidth,
            outHeight,
            1,
            0,
            0
        );

        var outImg = output.GetImage(0);
        var srcImg = crossImage.GetImage(0);

        (int tx, int ty)[] faceTiles =
        {
            (2, 1), // +X
            (0, 1), // -X
            (1, 0), // +Y
            (1, 2), // -Y
            (1, 1), // +Z
            (3, 1), // -Z
        };

        int bytesPerPixel = (int)BitsPerPixel(meta.Format) / 8;

        byte* outPtr = (byte*)outImg.Pixels;
        long outRowPitch = outImg.RowPitch;
        byte* srcPtr = (byte*)srcImg.Pixels;
        long srcRowPitch = srcImg.RowPitch;

        for (int y = 0; y < outHeight; y++)
        {
            for (int x = 0; x < outWidth; x++)
            {
                float u = (x + 0.5f) / outWidth;
                float v = (y + 0.5f) / outHeight;

                float theta = u * 2.0f * MathF.PI;
                float phi = v * MathF.PI;

                Vector3 dir;
                dir.X = MathF.Sin(phi) * MathF.Cos(theta);
                dir.Y = MathF.Cos(phi);
                dir.Z = MathF.Sin(phi) * MathF.Sin(theta);

                DirectionToCubeUV(dir, out int face, out float uFace, out float vFace);
                float[] c = SampleFaceBilinear(face, uFace, vFace);
                byte* dstPixel = outPtr + y * outRowPitch + x * bytesPerPixel;

                if (bytesPerPixel == 4)
                {
                    dstPixel[0] = (byte)(MathF.Min(1f, MathF.Max(0f, c[0])) * 255f);
                    dstPixel[1] = (byte)(MathF.Min(1f, MathF.Max(0f, c[1])) * 255f);
                    dstPixel[2] = (byte)(MathF.Min(1f, MathF.Max(0f, c[2])) * 255f);
                    dstPixel[3] = (byte)(MathF.Min(1f, MathF.Max(0f, c[3])) * 255f);
                }
                else if (bytesPerPixel == 8)
                {
                    ushort* up = (ushort*)dstPixel;
                    up[0] = (ushort)(MathF.Min(1f, MathF.Max(0f, c[0])) * 65535f);
                    up[1] = (ushort)(MathF.Min(1f, MathF.Max(0f, c[1])) * 65535f);
                    up[2] = (ushort)(MathF.Min(1f, MathF.Max(0f, c[2])) * 65535f);
                    up[3] = (ushort)(MathF.Min(1f, MathF.Max(0f, c[3])) * 65535f);
                }
            }
        }

        float[] ReadPixel(int face, int x, int y)
        {
            var (tx, ty) = faceTiles[face];
            int sx = tx * faceSize + x;
            int sy = ty * faceSize + y;

            byte* p = srcPtr + sy * srcRowPitch + sx * bytesPerPixel;

            float[] c = new float[4];

            if (bytesPerPixel == 4)
            {
                c[0] = p[0] / 255f;
                c[1] = p[1] / 255f;
                c[2] = p[2] / 255f;
                c[3] = p[3] / 255f;
            }
            else if (bytesPerPixel == 8)
            {
                ushort* up = (ushort*)p;
                c[0] = up[0] / 65535f;
                c[1] = up[1] / 65535f;
                c[2] = up[2] / 65535f;
                c[3] = up[3] / 65535f;
            }

            return c;
        }

        float Lerp(float a, float b, float t) => a + (b - a) * t;
        float[] SampleFaceBilinear(int face, float uFace, float vFace)
        {
            float fx = uFace * (faceSize - 1);
            float fy = vFace * (faceSize - 1);

            int x0 = (int)MathF.Floor(fx);
            int x1 = Math.Min(x0 + 1, faceSize - 1);
            int y0 = (int)MathF.Floor(fy);
            int y1 = Math.Min(y0 + 1, faceSize - 1);

            float wx = fx - x0;
            float wy = fy - y0;

            float[] c00 = ReadPixel(face, x0, y0);
            float[] c10 = ReadPixel(face, x1, y0);
            float[] c01 = ReadPixel(face, x0, y1);
            float[] c11 = ReadPixel(face, x1, y1);

            float[] c = new float[4];
            for (int i = 0; i < 4; i++)
            {
                float c0 = Lerp(c00[i], c10[i], wx);
                float c1 = Lerp(c01[i], c11[i], wx);
                c[i] = Lerp(c0, c1, wy);
            }

            return c;
        }

        return output;
    }

    private static void DirectionToCubeUV(Vector3 dir, out int face, out float u, out float v)
    {
        float ax = MathF.Abs(dir.X);
        float ay = MathF.Abs(dir.Y);
        float az = MathF.Abs(dir.Z);

        if (ax >= ay && ax >= az)
        {
            if (dir.X > 0) { face = 0; u = -dir.Z / ax; v = -dir.Y / ax; } // +X
            else { face = 1; u = dir.Z / ax; v = -dir.Y / ax; } // -X
        }
        else if (ay >= ax && ay >= az)
        {
            if (dir.Y > 0) { face = 2; u = dir.X / ay; v = dir.Z / ay; } // +Y
            else { face = 3; u = dir.X / ay; v = -dir.Z / ay; } // -Y
        }
        else
        {
            if (dir.Z > 0) { face = 4; u = dir.X / az; v = -dir.Y / az; } // +Z
            else { face = 5; u = -dir.X / az; v = -dir.Y / az; } // -Z
        }

        // Remap from [-1,1] to [0,1]
        u = u * 0.5f + 0.5f;
        v = v * 0.5f + 0.5f;
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

        //Debug.Assert(_tag.PlateTransforms.Count == 1, $"{Hash}");
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
        if (Hash.CheckRedacted())
        {
            Log.Warning($"Texture {Hash} is Redacted. Can not export.");
            return;
        }

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

    [SchemaField(0x2D, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public byte MipCount;

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

