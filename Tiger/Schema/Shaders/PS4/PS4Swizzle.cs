
// From RawTex by daemon1
public static class PS4SwizzleAlgorithm
{
    public static SwizzleType Type => SwizzleType.PS4;

    public static byte[] Swizzle(byte[] data, int width, int height, int pixelBlockSize, int blockSize)
    {
        return DoSwizzle(data, width, height, blockSize, pixelBlockSize, false);
    }

    public static byte[] UnSwizzle(byte[] data, int width, int height, int pixelBlockSize, int blockSize)
    {
        return DoSwizzle(data, width, height, blockSize, pixelBlockSize, true);
    }

    private static byte[] DoSwizzle(byte[] data, int width, int height, int blockSize, int pixelBlockSize, bool unswizzle)
    {
        byte[] processed = new byte[data.Length];
        int heightTexels = height / pixelBlockSize;
        int heightTexelsAligned = (heightTexels + 7) / 8;
        int widthTexels = width / pixelBlockSize;
        int widthTexelsAligned = (widthTexels + 7) / 8;
        int dataIndex = 0;
        string errorMesage = string.Empty;

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

                    if (xOffset < widthTexels && yOffset < heightTexels)
                    {
                        int destPixelIndex = yOffset * widthTexels + xOffset;
                        int destIndex = blockSize * destPixelIndex;

                        try
                        {
                            if (unswizzle)
                            {
                                if (processed.Length < destIndex + blockSize)
                                    Array.Resize<byte>(ref processed, destIndex + blockSize); //blockSize = processed.Length - destIndex;
                                Array.Copy(data, dataIndex, processed, destIndex, blockSize);
                            }
                            else
                            {
                                if (processed.Length < dataIndex + blockSize)
                                    Array.Resize<byte>(ref processed, dataIndex + blockSize); //blockSize = processed.Length - dataIndex;
                                Array.Copy(data, destIndex, processed, dataIndex, blockSize);
                            }
                        }
                        catch (Exception e)
                        {
                            errorMesage = e.Message;
                        }
                    }

                    dataIndex += blockSize;
                }
            }
        }
        if (errorMesage != string.Empty) Console.WriteLine(errorMesage);

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



