﻿using System.Runtime.InteropServices;
using Arithmic;
using DirectXTex;
using DirectXTexNet;

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

    public string GetDimension()
    {
        if (IsCubemap())
            return "Cube";
        else if (IsVolume())
            return "3D";
        else
            return "2D";
    }

    public ScratchImage GetScratchImage()
    {
        DXGI_FORMAT format = (DXGI_FORMAT)_tag.Format;
        byte[] data;
        if (_tag.LargeTextureBuffer != null)
        {
            // todo we shouldnt cache this
            data = _tag.LargeTextureBuffer.GetData(false);
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
            if (scratchImage.GetMetadata().ArraySize != 6)
            {
                Log.Error($"Cubemap texture '{Hash}' has invalid array size '{scratchImage.GetMetadata().ArraySize}'");
                return scratchImage;
            }
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
                scratchImage = scratchImage.Convert(DXGI_FORMAT.B8G8R8A8_UNORM_SRGB, TEX_FILTER_FLAGS.SEPARATE_ALPHA, 0);
            }
            else
            {
                scratchImage = scratchImage.Convert(DXGI_FORMAT.B8G8R8A8_UNORM, 0, 0);
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
        ScratchImage outputPlate = TexHelper.Instance.Initialize2D(bSrgb ? DXGI_FORMAT.B8G8R8A8_UNORM_SRGB : DXGI_FORMAT.B8G8R8A8_UNORM, image.Width * input.GetImageCount(), image.Height, 1, 0, 0);

        for (int i = 0; i < input.GetImageCount(); i++)
        {
            TexHelper.Instance.CopyRectangle(input.GetImage(i), 0, 0, image.Width, image.Height, outputPlate.GetImage(0), bSrgb ? TEX_FILTER_FLAGS.SEPARATE_ALPHA : 0, image.Width * i, 0);
        }
        input.Dispose();
        return outputPlate;
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
        // ms.Dispose();
        return ms;
    }

    public static void SavetoFile(string savePath, ScratchImage simg, bool isCubemap = false)
    {
        try
        {
            TextureExtractor.SaveTextureToFile(savePath, simg, isCubemap);
        }
        catch (FileLoadException)
        {
        }
    }

    public void SavetoFile(string savePath)
    {
        ScratchImage simg = GetScratchImage();
        SavetoFile(savePath, simg, IsCubemap());
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

[NonSchemaStruct(0x40, 32, new[] { 1, 2, 3 })]
public struct STextureHeader
{
    public uint DataSize;
    public uint Format;  // DXGI_FORMAT
    [SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public float Unk10;
    [SchemaField(0x14, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public float Unk14;
    [SchemaField(0x0C, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public ushort CAFE;

    public ushort Width;
    public ushort Height;
    public ushort Depth;
    public ushort ArraySize;
    public ushort MipLevels; // not mip levels idk what this is
    public ushort Unk2C;
    public ushort Unk2E;
    public ushort Unk30;
    public ushort Unk32;
    public ushort Unk34;

    [SchemaField(0x24, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x3C, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public TigerFile? LargeTextureBuffer;
}
