using DirectXTexNet;

public static class GcnSurfaceFormatExtensions
{
    public static GcnSurfaceFormat GetFormat(ushort value)
    {
        ushort maskedValue = (ushort)((value >> 4) & 0x3F);
        return (GcnSurfaceFormat)maskedValue;
    }

    public static DXGI_FORMAT ToDXGI(ushort value)
    {
        GcnSurfaceFormat format = GetFormat(value);
        return format switch
        {
            GcnSurfaceFormat.Format8 => DXGI_FORMAT.R8_UNORM,
            GcnSurfaceFormat.Format16 => DXGI_FORMAT.R16_UNORM,
            GcnSurfaceFormat.Format8_8 => DXGI_FORMAT.R8G8_UNORM,
            GcnSurfaceFormat.Format32 => DXGI_FORMAT.R32_FLOAT,
            GcnSurfaceFormat.Format16_16 => DXGI_FORMAT.R16G16_UNORM,
            GcnSurfaceFormat.Format10_11_11 => DXGI_FORMAT.R11G11B10_FLOAT,
            GcnSurfaceFormat.Format10_10_10_2 => DXGI_FORMAT.R10G10B10A2_UNORM,
            GcnSurfaceFormat.Format2_10_10_10 => DXGI_FORMAT.R10G10B10A2_UNORM,
            GcnSurfaceFormat.Format8_8_8_8 => DXGI_FORMAT.R8G8B8A8_UNORM,
            GcnSurfaceFormat.Format32_32 => DXGI_FORMAT.R32G32_FLOAT,
            GcnSurfaceFormat.Format16_16_16_16 => DXGI_FORMAT.R16G16B16A16_UNORM,
            GcnSurfaceFormat.Format32_32_32_32 => DXGI_FORMAT.R32G32B32A32_FLOAT,
            GcnSurfaceFormat.Format8_24 => DXGI_FORMAT.D24_UNORM_S8_UINT,
            GcnSurfaceFormat.BC1 => DXGI_FORMAT.BC1_UNORM_SRGB,
            GcnSurfaceFormat.BC2 => DXGI_FORMAT.BC2_UNORM_SRGB,
            GcnSurfaceFormat.BC3 => DXGI_FORMAT.BC3_UNORM_SRGB,
            GcnSurfaceFormat.BC4 => DXGI_FORMAT.BC4_UNORM,
            GcnSurfaceFormat.BC5 => DXGI_FORMAT.BC5_UNORM,
            GcnSurfaceFormat.BC6 => DXGI_FORMAT.BC6H_SF16,
            GcnSurfaceFormat.BC7 => DXGI_FORMAT.BC7_UNORM_SRGB,
            _ => throw new NotSupportedException($"Unsupported GCN surface format conversion ({format} => ??)")
        };
    }

    public static int Bpp(this GcnSurfaceFormat format)
    {
        return format switch
        {
            GcnSurfaceFormat.Format8 => 8,
            GcnSurfaceFormat.Format16 => 16,
            GcnSurfaceFormat.Format8_8 => 16,
            GcnSurfaceFormat.Format32 => 32,
            GcnSurfaceFormat.Format16_16 => 32,
            GcnSurfaceFormat.Format10_11_11 => 32,
            GcnSurfaceFormat.Format10_10_10_2 => 32,
            GcnSurfaceFormat.Format2_10_10_10 => 32,
            GcnSurfaceFormat.Format8_8_8_8 => 32,
            GcnSurfaceFormat.Format32_32 => 64,
            GcnSurfaceFormat.Format16_16_16_16 => 64,
            GcnSurfaceFormat.Format32_32_32_32 => 128,
            GcnSurfaceFormat.Format8_24 => 32,
            GcnSurfaceFormat.Invalid => 0,
            GcnSurfaceFormat.Format1_5_5_5 => 16,
            GcnSurfaceFormat.Format24_8 => 32,
            GcnSurfaceFormat.FormatX24_8_32 => 64,
            GcnSurfaceFormat.GB_GR => 0,
            GcnSurfaceFormat.BG_RG => 0,
            GcnSurfaceFormat.BC1 or GcnSurfaceFormat.BC4 => 4,
            GcnSurfaceFormat.BC2 or GcnSurfaceFormat.BC3 or GcnSurfaceFormat.BC5 or GcnSurfaceFormat.BC6 or GcnSurfaceFormat.BC7 => 8,
            GcnSurfaceFormat.Format11_11_10 => 32,
            GcnSurfaceFormat.Format32_32_32 => 96,
            GcnSurfaceFormat.Format5_6_5 => 16,
            GcnSurfaceFormat.Format5_5_5_1 => 16,
            GcnSurfaceFormat.Format4_4_4_4 => 16,
            GcnSurfaceFormat.Format5_9_9_9 => 32,
            _ => throw new NotSupportedException($"Unsupported GCN surface format bpp ({format})")
        };
    }

    public static int BlockSize(this GcnSurfaceFormat format)
    {
        return format switch
        {
            GcnSurfaceFormat.BC1 or GcnSurfaceFormat.BC4 => 8,
            GcnSurfaceFormat.BC2 or GcnSurfaceFormat.BC3 or GcnSurfaceFormat.BC5 or GcnSurfaceFormat.BC6 or GcnSurfaceFormat.BC7 => 16,
            _ => format.Bpp() / 8
        };
    }

    public static int PixelBlockSize(this GcnSurfaceFormat format)
    {
        return format switch
        {
            GcnSurfaceFormat.BC1 or GcnSurfaceFormat.BC2 or GcnSurfaceFormat.BC3 or GcnSurfaceFormat.BC4 or GcnSurfaceFormat.BC5 or GcnSurfaceFormat.BC6 or GcnSurfaceFormat.BC7 => 4,
            _ => 1
        };
    }

    public static bool IsCompressed(this GcnSurfaceFormat format)
    {
        return format is GcnSurfaceFormat.BC1 or GcnSurfaceFormat.BC2 or GcnSurfaceFormat.BC3 or GcnSurfaceFormat.BC4 or GcnSurfaceFormat.BC5 or GcnSurfaceFormat.BC6 or GcnSurfaceFormat.BC7;
    }


    public enum GcnSurfaceFormat
    {
        /// <summary>Invalid surface format.</summary>
        Invalid = 0x00000000,
        /// <summary>One 8-bit channel. X=0xFF</summary>
        Format8 = 0x00000001,
        /// <summary>One 16-bit channel. X=0xFFFF</summary>
        Format16 = 0x00000002,
        /// <summary>Two 8-bit channels. X=0x00FF, Y=0xFF00</summary>
        Format8_8 = 0x00000003,
        /// <summary>One 32-bit channel. X=0xFFFFFFFF</summary>
        Format32 = 0x00000004,
        /// <summary>Two 16-bit channels. X=0x0000FFFF, Y=0xFFFF0000</summary>
        Format16_16 = 0x00000005,
        /// <summary>One 10-bit channel (Z) and two 11-bit channels (Y,X). X=0x000007FF, Y=0x003FF800, Z=0xFFC00000 Interpreted only as floating-point by texture unit, but also as integer by rasterizer.</summary>
        Format10_11_11 = 0x00000006,
        /// <summary>Two 11-bit channels (Z,Y) and one 10-bit channel (X). X=0x000003FF, Y=0x001FFC00, Z=0xFFE00000 Interpreted only as floating-point by texture unit, but also as integer by rasterizer.</summary>
        Format11_11_10 = 0x00000007,
        /// <summary>Three 10-bit channels (W,Z,Y) and one 2-bit channel (X). X=0x00000003, Y=0x00000FFC, Z=0x003FF000, W=0xFFC00000 X is never negative, even when YZW are.</summary>
        Format10_10_10_2 = 0x00000008,
        /// <summary>One 2-bit channel (W) and three 10-bit channels (Z,Y,X). X=0x000003FF, Y=0x000FFC00, Z=0x3FF00000, W=0xC0000000 W is never negative, even when XYZ are.</summary>
        Format2_10_10_10 = 0x00000009,
        /// <summary>Four 8-bit channels. X=0x000000FF, Y=0x0000FF00, Z=0x00FF0000, W=0xFF000000</summary>
        Format8_8_8_8 = 0x0000000a,
        /// <summary>Two 32-bit channels.</summary>
        Format32_32 = 0x0000000b,
        /// <summary>Four 16-bit channels.</summary>
        Format16_16_16_16 = 0x0000000c,
        /// <summary>Three 32-bit channels.</summary>
        Format32_32_32 = 0x0000000d,
        /// <summary>Four 32-bit channels.</summary>
        Format32_32_32_32 = 0x0000000e,
        /// <summary>One 5-bit channel (Z), one 6-bit channel (Y), and a second 5-bit channel (X). X=0x001F, Y=0x07E0, Z=0xF800</summary>
        Format5_6_5 = 0x00000010,
        /// <summary>One 1-bit channel (W) and three 5-bit channels (Z,Y,X). X=0x001F, Y=0x03E0, Z=0x7C00, W=0x8000</summary>
        Format1_5_5_5 = 0x00000011,
        /// <summary>Three 5-bit channels (W,Z,Y) and one 1-bit channel (X). X=0x0001, Y=0x003E, Z=0x07C0, W=0xF800</summary>
        Format5_5_5_1 = 0x00000012,
        /// <summary>Four 4-bit channels. X=0x000F, Y=0x00F0, Z=0x0F00, W=0xF000</summary>
        Format4_4_4_4 = 0x00000013,
        /// <summary>One 8-bit channel and one 24-bit channel.</summary>
        Format8_24 = 0x00000014,
        /// <summary>One 24-bit channel and one 8-bit channel.</summary>
        Format24_8 = 0x00000015,
        /// <summary>One 24-bit channel, one 8-bit channel, and one 32-bit channel.</summary>
        FormatX24_8_32 = 0x00000016,
        /// <summary>To be documented.</summary>
        GB_GR = 0x00000020,
        /// <summary>To be documented.</summary>
        BG_RG = 0x00000021,
        /// <summary>One 5-bit channel (W) and three 9-bit channels (Z,Y,X). X=0x000001FF, Y=0x0003FE00, Z=0x07FC0000, W=0xF8000000. Interpreted only as three 9-bit denormalized mantissas, and one shared 5-bit exponent.</summary>
        Format5_9_9_9 = 0x00000022,
        /// <summary>BC1 block-compressed surface.</summary>
        BC1 = 0x00000023,
        /// <summary>BC2 block-compressed surface.</summary>
        BC2 = 0x00000024,
        /// <summary>BC3 block-compressed surface.</summary>
        BC3 = 0x00000025,
        /// <summary>BC4 block-compressed surface.</summary>
        BC4 = 0x00000026,
        /// <summary>BC5 block-compressed surface.</summary>
        BC5 = 0x00000027,
        /// <summary>BC6 block-compressed surface.</summary>
        BC6 = 0x00000028,
        /// <summary>BC7 block-compressed surface.</summary>
        BC7 = 0x00000029,
        /// <summary>8 bits-per-element FMASK surface (2 samples, 1 fragment).</summary>
        Fmask8_S2_F1 = 0x0000002C,
        /// <summary>8 bits-per-element FMASK surface (4 samples, 1 fragment).</summary>
        Fmask8_S4_F1 = 0x0000002D,
        /// <summary>8 bits-per-element FMASK surface (8 samples, 1 fragment).</summary>
        Fmask8_S8_F1 = 0x0000002E,
        /// <summary>8 bits-per-element FMASK surface (2 samples, 2 fragments).</summary>
        Fmask8_S2_F2 = 0x0000002F,
        /// <summary>8 bits-per-element FMASK surface (8 samples, 2 fragments).</summary>
        Fmask8_S4_F2 = 0x00000030,
        /// <summary>8 bits-per-element FMASK surface (4 samples, 4 fragments).</summary>
        Fmask8_S4_F4 = 0x00000031,
        /// <summary>16 bits-per-element FMASK surface (16 samples, 1 fragment).</summary>
        Fmask16_S16_F1 = 0x00000032,
        /// <summary>16 bits-per-element FMASK surface (8 samples, 2 fragments).</summary>
        Fmask16_S8_F2 = 0x00000033,
        /// <summary>32 bits-per-element FMASK surface (16 samples, 2 fragments).</summary>
        Fmask32_S16_F2 = 0x00000034,
        /// <summary>32 bits-per-element FMASK surface (8 samples, 4 fragments).</summary>
        Fmask32_S8_F4 = 0x00000035,
        /// <summary>32 bits-per-element FMASK surface (8 samples, 8 fragments).</summary>
        Fmask32_S8_F8 = 0x00000036,
        /// <summary>64 bits-per-element FMASK surface (16 samples, 4 fragments).</summary>
        Fmask64_S16_F4 = 0x00000037,
        /// <summary>64 bits-per-element FMASK surface (16 samples, 8 fragments).</summary>
        Fmask64_S16_F8 = 0x00000038,
        /// <summary>Two 4-bit channels (Y,X). X=0x0F, Y=0xF0</summary>
        Format4_4 = 0x00000039,
        /// <summary>One 6-bit channel (Z) and two 5-bit channels (Y,X). X=0x001F, Y=0x03E0, Z=0xFC00</summary>
        Format6_5_5 = 0x0000003A,
        /// <summary>One 1-bit channel. 8 pixels per byte, with pixel index increasing from LSB to MSB.</summary>
        Format1 = 0x0000003B,
        /// <summary>One 1-bit channel. 8 pixels per byte, with pixel index increasing from MSB to LSB.</summary>
        Format1Reversed = 0x0000003C,
    }

}
