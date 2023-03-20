using System.ComponentModel;
using System.Runtime.InteropServices;
using DirectXTex;
using DirectXTexNet;
using Field.General;

namespace Field;

public class TextureHeader : Tag
{
    public D2Class_TextureHeader Header;
    
    public TextureHeader(TagHash hash) : base(hash)
    {
    }

    public bool IsCubemap()
    {
        return Header.ArraySize == 6;
    }
    
    public bool IsVolume()
    {
        return Header.Depth != 1;
    }
    
    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_TextureHeader>();
    }

    public ScratchImage GetScratchImage()
    {
        DXGI_FORMAT format = (DXGI_FORMAT)Header.Format;
        byte[] data;
        if (Header.LargeBuffer != null)
        {
            data = Header.LargeBuffer.GetBufferData();
        }
        else
        {
            data = PackageHandler.GetTag(typeof(TextureBuffer), PackageHandler.GetEntryReference(Hash)).GetBufferData();
        }

        DirectXTexUtility.TexMetadata metadata = DirectXTexUtility.GenerateMetaData(Header.Width, Header.Height, 1, (DirectXTexUtility.DXGIFormat)format, Header.ArraySize == 6);
        DirectXTexUtility.DDSHeader ddsHeader;
        DirectXTexUtility.DX10Header dx10Header;
        DirectXTexUtility.GenerateDDSHeader(metadata, DirectXTexUtility.DDSFlags.NONE, out ddsHeader, out dx10Header);
        byte[] header = DirectXTexUtility.EncodeDDSHeader(ddsHeader, dx10Header);
        byte[] final = new byte[data.Length + header.Length];
        Array.Copy(header, 0, final, 0, header.Length);
        Array.Copy(data, 0, final, header.Length, data.Length);
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
				scratchImage = scratchImage.Convert(DXGI_FORMAT.B8G8R8A8_UNORM_SRGB, TEX_FILTER_FLAGS.SEPARATE_ALPHA, 0);
			}
            else
            {
                scratchImage = scratchImage.Convert(DXGI_FORMAT.B8G8R8A8_UNORM, 0, 0);
            }
        }
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

    public bool IsSrgb()
    {
        return TexHelper.Instance.IsSRGB((DXGI_FORMAT)Header.Format);
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
        try
        {
            TextureExtractor.SaveTextureToFile(savePath, simg);
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

[StructLayout(LayoutKind.Sequential, Size = 0x40)]
public struct D2Class_TextureHeader
{
    public uint DataSize;
    public uint Format;  // DXGI_FORMAT
    [DestinyOffset(0x10)] 
    public float Unk10;
    [DestinyOffset(0x14)] 
    public float Unk14;
    [DestinyOffset(0x20)] 
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

    [DestinyOffset(0x3C), DestinyField(FieldType.TagHash)]
    public TextureBuffer LargeBuffer;
}