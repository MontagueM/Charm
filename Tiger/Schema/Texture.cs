using System.Runtime.InteropServices;
using DirectXTex;
using DirectXTexNet;

namespace Tiger.Schema;

public class Texture : TigerReferenceFile64<STextureHeader>
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

    public ScratchImage GetScratchImage()
    {
        DXGI_FORMAT format = (DXGI_FORMAT)_tag.Format;
        byte[] data;
        if (_tag.LargeTextureBuffer.Hash.IsValid())
        {
            data = _tag.LargeTextureBuffer.GetData();
        }
        else
        {
            data = GetReferenceData();
        }

        DirectXTexUtility.TexMetadata metadata = DirectXTexUtility.GenerateMetaData(_tag.Width, _tag.Height, 1, (DirectXTexUtility.DXGIFormat)format, _tag.ArraySize == 6);
        DirectXTexUtility.DDSHeader ddsHeader;
        DirectXTexUtility.DX10Header dx10Header;
        DirectXTexUtility.GenerateDDSHeader(metadata, DirectXTexUtility.DDSFlags.NONE, out ddsHeader, out dx10Header);
        byte[] tag = DirectXTexUtility.EncodeDDSHeader(ddsHeader, dx10Header);
        byte[] final = new byte[data.Length + tag.Length];
        Array.Copy(tag, 0, final, 0, tag.Length);
        Array.Copy(data, 0, final, tag.Length, data.Length);
        GCHandle gcHandle = GCHandle.Alloc(final, GCHandleType.Pinned);
        IntPtr pixelPtr = gcHandle.AddrOfPinnedObject();
        var scratchImage = TexHelper.Instance.LoadFromDDSMemory(pixelPtr, final.Length, DDS_FLAGS.NONE);
        gcHandle.Free();
        if (IsCubemap())
        {
            if (TexHelper.Instance.IsCompressed(format))
            {
                scratchImage = DecompressScratchImage(scratchImage, DXGI_FORMAT.R8G8B8A8_UNORM);
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
            if (TexHelper.Instance.IsCompressed(format))
            {
                if (TexHelper.Instance.IsSRGB(format))
                {
                    scratchImage = DecompressScratchImage(scratchImage, DXGI_FORMAT.B8G8R8A8_UNORM_SRGB);
                }
                else
                {
                    scratchImage = DecompressScratchImage(scratchImage, DXGI_FORMAT.B8G8R8A8_UNORM);
                }
            }
            else if (TexHelper.Instance.IsSRGB(format))
            {
                scratchImage = scratchImage.Convert(DXGI_FORMAT.B8G8R8A8_UNORM_SRGB, TEX_FILTER_FLAGS.SRGB, 0);
            }
            else
            {
                scratchImage = scratchImage.Convert(DXGI_FORMAT.B8G8R8A8_UNORM, 0, 0);
            }
        }
        return scratchImage;
    }

    private ScratchImage DecompressScratchImage(ScratchImage scratchImage )
    {
        while (true)
        {
            try
            {
                scratchImage = scratchImage.Decompress(DXGI_FORMAT.B8G8R8A8_UNORM);
                return scratchImage;
            }
            catch (AccessViolationException)
            {
            }
        }
    }

    public bool IsSrgb()
    {
        return TexHelper.Instance.IsSRGB((DXGI_FORMAT)_tag.Format);
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

    public static void SavetoFile(string savePath, ScratchImage simg)
    {
        throw new NotImplementedException();
        try
        {
            // TextureExtractor.SaveTextureToFile(savePath, simg);
        }
        catch (FileLoadException)
        {
        }
    }

    public void SavetoFile(string savePath)
    {
        ScratchImage simg = GetScratchImage();
        SavetoFile(savePath, simg);
    }


    // public void SaveToDDSFile(string savePath)
    // {
    //     ScratchImage scratchImage = GetScratchImage();
    //     SaveToDDSFile(savePath, scratchImage);
    // }

    // public static void SaveToDDSFile(string savePath, ScratchImage scratchImage)
    // {
    //     scratchImage.SaveToDDSFile(DDS_FLAGS.FORCE_DX10_EXT, savePath);
    //     scratchImage.Dispose();
    // }

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
}

[SchemaStruct(0x40)]
public struct STextureHeader
{
    public uint DataSize;
    public uint Format;  // DXGI_FORMAT
    [SchemaField(0x10)]
    public float Unk10;
    [SchemaField(0x14)]
    public float Unk14;
    [SchemaField(0x20)]
    public ushort CAFE;

    public ushort Width;
    public ushort Height;
    public ushort Depth;
    public ushort ArraySize;
    public ushort MipLevels; // not mip levels ig
    public ushort Unk2C;
    public ushort Unk2E;
    public ushort Unk30;
    public ushort Unk32;
    public ushort Unk34;

    [SchemaField(0x3C)]
    public TigerFile LargeTextureBuffer;
}
