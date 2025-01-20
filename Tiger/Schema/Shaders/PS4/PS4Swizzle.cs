
// From RawTex by daemon1
using MathNet.Numerics;
using static GcnSurfaceFormatExtensions;

public static class PS4SwizzleAlgorithm
{
    public static SwizzleType Type => SwizzleType.PS4;

    public static byte[] Swizzle(byte[] data, int width, int height, int arraySize, GcnSurfaceFormat format)
    {
        return DoSwizzle(data, width, height, arraySize, format, false);
    }

    public static byte[] UnSwizzle(byte[] data, int width, int height, int arraySize, GcnSurfaceFormat format)
    {
        return DoSwizzle(data, width, height, arraySize, format, true);
    }

    // TODO: try to figure out cubemap faces
    private static byte[] DoSwizzle(byte[] data, int width, int height, int arraySize, GcnSurfaceFormat format, bool unswizzle)
    {
        byte[] processed = new byte[data.Length];
        int pixelBlockSize = format.PixelBlockSize();
        int blockSize = format.BlockSize();

        int width_src = format.IsCompressed() ? width.CeilingToPowerOfTwo() : width;
        int height_src = format.IsCompressed() ? height.CeilingToPowerOfTwo() : height;
        int width_texels_dest = width / pixelBlockSize;
        int height_texels_dest = height / pixelBlockSize;

        int heightTexels = height_src / pixelBlockSize;
        int heightTexelsAligned = (heightTexels + 7) / 8;

        int widthTexels = width_src / pixelBlockSize;
        int widthTexelsAligned = (widthTexels + 7) / 8;

        int dataIndex = 0;
        for (int z = 0; z < arraySize; ++z)
        {
            int sliceOffset = (z * width * height * format.Bpp()) / 8;
            for (int y = 0; y < heightTexelsAligned; ++y)
            {
                for (int x = 0; x < widthTexelsAligned; ++x)
                {
                    for (int t = 0; t < 64; ++t)
                    {
                        int pixelIndex = Morton(t, 8, 8);
                        int num8 = pixelIndex / 8;
                        int num9 = pixelIndex % 8;
                        int yOffset = (y * 8) + num8;
                        int xOffset = (x * 8) + num9;

                        if (xOffset < width_texels_dest && yOffset < height_texels_dest)
                        {
                            int destPixelIndex = yOffset * width_texels_dest + xOffset;
                            int destIndex = blockSize * destPixelIndex;

                            try
                            {
                                int src = unswizzle ? dataIndex : destIndex;
                                int dst = unswizzle ? destIndex : dataIndex;

                                if ((src + blockSize) <= data.Length && (dst + blockSize) <= processed.Length - sliceOffset)
                                    Array.Copy(data, src, processed, sliceOffset + dst, blockSize);
                            }
                            catch (Exception e)
                            {
                                throw new ArgumentException(e.Message);
                            }
                        }
                        dataIndex += blockSize;
                    }
                }
            }
        }
        return processed;
    }

    /// <summary>
    /// Z-order curve
    /// https://en.wikipedia.org/wiki/Z-order_curve
    /// </summary>
    public static int Morton(int t, int sx, int sy)
    {
        int num1;
        int num2 = num1 = 1;
        int num3 = t;
        int num4 = sx;
        int num5 = sy;
        int num6 = 0;
        int num7 = 0;

        while (num4 > 1 || num5 > 1)
        {
            if (num4 > 1)
            {
                num6 += num2 * (num3 & 1);
                num3 >>= 1;
                num2 *= 2;
                num4 >>= 1;
            }
            if (num5 > 1)
            {
                num7 += num1 * (num3 & 1);
                num3 >>= 1;
                num1 *= 2;
                num5 >>= 1;
            }
        }

        return num7 * sx + num6;
    }

    public enum SwizzleType
    {
        None,
        PS3,
        PS4,
        Switch
    }
}



