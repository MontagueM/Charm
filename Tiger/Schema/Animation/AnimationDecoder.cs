namespace Tiger.Schema.Animation;

using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;


public class BoneTracks
{
    public float[]? Scales;
    public Vector4[]? Rotations;
    public Vector3[]? Translations;
}

public class DecodedStreams
{
    public float[][] Scales = Array.Empty<float[]>();
    public Vector4[][] Rotations = Array.Empty<Vector4[]>();
    public Vector3[][] Translations = Array.Empty<Vector3[]>();
}

public static class AnimationDecoder
{
    public static BoneTracks[] Decode(Animation clip)
    {
        var bones = new BoneTracks[clip.RigControlCount];
        for (int i = 0; i < bones.Length; i++)
            bones[i] = new BoneTracks();

        if (clip.Static != null)
        {
            var streams = DecodeStreams(clip.Static, clip.FrameCount);
            Assign(bones, streams, clip.Maps.StaticScale, clip.Maps.StaticRotation, clip.Maps.StaticTranslation);

        }

        if (clip.Animated != null)
        {
            var streams = DecodeStreams(clip.Animated, clip.FrameCount);
            Assign(bones, streams, clip.Maps.AnimatedScale, clip.Maps.AnimatedRotation, clip.Maps.AnimatedTranslation);
        }

        return bones;
    }


    private static void Assign(BoneTracks[] bones, DecodedStreams t, ushort[] scaleMap, ushort[] rotMap, ushort[] transMap)
    {
        for (int i = 0; i < scaleMap.Length; i++)
            bones[scaleMap[i]].Scales = t.Scales[i];

        for (int i = 0; i < rotMap.Length; i++)
            bones[rotMap[i]].Rotations = t.Rotations[i];

        for (int i = 0; i < transMap.Length; i++)
            bones[transMap[i]].Translations = t.Translations[i];
    }

    public static DecodedStreams DecodeStreams(CodecData c, int clipFrameCount)
    {
        return c.CodecType switch
        {
            AnimationCodecType.Raw => DecodeCodec0(c),
            AnimationCodecType.Compressed => DecodeCodec1(c, clipFrameCount),
            AnimationCodecType.Uncompressed => DecodeCodec2(c),
            AnimationCodecType.PoseQuantized => DecodeCodec3(c),
            _ => throw new NotSupportedException($"Animation codec {c.CodecType} is not supported"),
        };
    }

    private static double Snorm16(short v) => v / 32767.0;
    private static double Unorm16(ushort v) => v / 65535.0;

    private static void DecompressQuaternion(short rw, short rx, short ry, double[] result)
    {
        bool negateZ = (rx & 1) == 1;

        double w = Snorm16(rw), x = Snorm16((short)(rx & ~1)), y = Snorm16(ry);
        double z = Math.Sqrt(Math.Max(1 - w * w - x * x - y * y, 0));

        if (negateZ)
            z = -z;

        result[0] = x; result[1] = y; result[2] = z; result[3] = w;
    }

    private static double Tangent(byte interp, double p0, double p1, bool upper)
    {
        int value = upper ? interp >> 4 : interp & 15;
        double tan = value - 7;
        return (Math.Abs(tan) / 7) * (tan / 7 * 0.3) + (p1 - p0);
    }

    private static double Hermite(double p0, double p1, double t0, double t1, double t)
    {
        double t2 = t * t, t3 = t2 * t;
        return (2 * t3 - 3 * t2 + 1) * p0 + (t3 - 2 * t2 + t) * t0 + (3 * t2 - 2 * t3) * p1 + (t3 - t2) * t1;
    }

    private static Vector4 V4(double[] d) => new((float)d[0], (float)d[1], (float)d[2], (float)d[3]);
    private static Vector3 V3(double[] d) => new((float)d[0], (float)d[1], (float)d[2]);

    private enum Kind
    {
        Scale,
        Rotation,
        Translation
    }

    private static DecodedStreams DecodeCodec1(CodecData c, int frames)
    {
        if (c.UnkArray7.Length == 0 || c.UnkArray7[c.UnkArray7.Length - 1] != c.CompressedData.Length)
            throw new Exception("Codec 1: last element of array 7 does not equal the compressed data length");

        var d = new DecodedStreams
        {
            Scales = new float[c.ScaleStreamCount][],
            Rotations = new Vector4[c.RotationStreamCount][],
            Translations = new Vector3[c.TranslationStreamCount][],
        };

        int a7 = 0;

        for (int s = 0; s < c.ScaleStreamCount; s++, a7++)
        {
            double[][] values = DecodeCodec1Stream(c, c.UnkArray7[a7], frames, Kind.Scale, s);
            var track = new float[frames];
            for (int f = 0; f < frames; f++) track[f] = (float)values[f][0];
            d.Scales[s] = track;
        }

        for (int s = 0; s < c.RotationStreamCount; s++, a7++)
        {
            double[][] values = DecodeCodec1Stream(c, c.UnkArray7[a7], frames, Kind.Rotation, s);
            var track = new Vector4[frames];
            for (int f = 0; f < frames; f++) track[f] = V4(values[f]);
            d.Rotations[s] = track;
        }

        for (int s = 0; s < c.TranslationStreamCount; s++, a7++)
        {
            double[][] values = DecodeCodec1Stream(c, c.UnkArray7[a7], frames, Kind.Translation, s);
            var track = new Vector3[frames];
            for (int f = 0; f < frames; f++) track[f] = V3(values[f]);
            d.Translations[s] = track;
        }

        return d;
    }


    private static double[][] DecodeCodec1Stream(CodecData c, int array7Element, int frames, Kind kind, int streamIndex)
    {
        int comps = kind == Kind.Scale ? 1 : kind == Kind.Rotation ? 4 : 3;
        int rawComps = kind == Kind.Scale ? 1 : 3;
        bool scaleFirst = c.QuantizationExtents.Length >= c.ScaleStreamCount + c.TranslationStreamCount;
        int quantIndex = kind == Kind.Translation && scaleFirst ? c.ScaleStreamCount + streamIndex : streamIndex;
        bool quantized = kind == Kind.Translation || (kind == Kind.Scale && scaleFirst);
        double extent = quantized ? c.QuantizationExtents[quantIndex] : 0;
        double minimum = quantized ? c.QuantizationMinimums[quantIndex] : 0;

        var result = new double[frames][];

        double[] ReadKey(short[] data, int offset)
        {
            var key = new double[comps];
            switch (kind)
            {
                case Kind.Scale:
                    key[0] = quantized ? Snorm16(data[offset]) * extent + minimum : Snorm16(data[offset]);
                    break;
                case Kind.Rotation:
                    DecompressQuaternion(data[offset], data[offset + 1], data[offset + 2], key);
                    break;
                default:
                    for (int i = 0; i < 3; i++) key[i] = Snorm16(data[offset + i]) * extent + minimum;
                    break;
            }
            return key;
        }

        if (array7Element > 0)
        {
            short[] comp = c.CompressedData;
            int offset = array7Element - 1;
            int deltasOffset = comp[offset];
            int interpOffset = comp[offset + 1];
            int deltaCount = comp[offset + 2];
            offset += 3;

            double[] first = ReadKey(comp, offset);
            offset += rawComps;
            result[0] = first;

            int insert = 1;
            for (int i = 0; i < deltaCount; i++)
            {
                double[] second = ReadKey(comp, offset);
                offset += rawComps;

                int delta = c.KeyframeDeltas[deltasOffset + i];
                var t0 = new double[comps];
                var t1 = new double[comps];

                for (int k = 0; k < comps; k++)
                {
                    byte interp = c.InterpolationData[interpOffset + k];
                    t0[k] = Tangent(interp, first[k], second[k], true);
                    t1[k] = Tangent(interp, first[k], second[k], false);
                }

                interpOffset += comps;

                for (int step = 1; step <= delta; step++)
                {
                    double t = (double)step / delta;
                    var frame = new double[comps];
                    for (int k = 0; k < comps; k++)
                        frame[k] = Hermite(first[k], second[k], t0[k], t1[k], t);

                    result[insert++] = frame;
                }

                first = second;
            }

            for (; insert < frames; insert++)
                result[insert] = result[insert - 1];
        }
        else
        {
            var data = c.UncompressedData;
            int offset = Math.Abs((int)array7Element) - 1;

            for (int f = 0; f < frames; f++)
            {
                var key = new double[comps];
                switch (kind)
                {
                    case Kind.Scale:
                        key[0] = quantized ? Snorm16(data[offset + f]) * extent + minimum : Snorm16(data[offset + f]);
                        break;
                    case Kind.Rotation:
                        DecompressQuaternion(data[offset + f * 3], data[offset + f * 3 + 1], data[offset + f * 3 + 2], key);
                        break;
                    default:
                        for (int i = 0; i < 3; i++)
                            key[i] = Snorm16(data[offset + f * 3 + i]) * extent + minimum;
                        break;
                }
                result[f] = key;
            }
        }

        return result;
    }

    private static DecodedStreams DecodeCodec0(CodecData c)
    {
        int frames = c.FrameCount;
        int S = c.ScaleStreamCount, R = c.RotationStreamCount, T = c.TranslationStreamCount;

        if (c.RawScales.Length != S * frames || c.RawRotations.Length != R * frames || c.RawTranslations.Length != T * frames)
            throw new Exception($"Codec 0: array sizes ({c.RawScales.Length}, {c.RawRotations.Length}, {c.RawTranslations.Length}) don't match {S}/{R}/{T} streams * {frames} frames");

        var d = new DecodedStreams
        {
            Scales = new float[S][],
            Rotations = new Vector4[R][],
            Translations = new Vector3[T][],
        };

        for (int s = 0; s < S; s++)
            d.Scales[s] = c.RawScales.AsSpan(s * frames, frames).ToArray();

        for (int s = 0; s < R; s++)
            d.Rotations[s] = c.RawRotations.AsSpan(s * frames, frames).ToArray();

        for (int s = 0; s < T; s++)
        {
            d.Translations[s] = new Vector3[frames];
            for (int f = 0; f < frames; f++)
            {
                var v = c.RawTranslations[s * frames + f];
                d.Translations[s][f] = new Vector3(v.X, v.Y, v.Z);
            }
        }

        return d;
    }

    private static DecodedStreams DecodeCodec2(CodecData c)
    {
        int frames = c.FrameCount;
        int S = c.ScaleStreamCount, R = c.RotationStreamCount, T = c.TranslationStreamCount;

        var d = new DecodedStreams
        {
            Scales = new float[S][],
            Rotations = new Vector4[R][],
            Translations = new Vector3[T][],
        };

        int data = 0;
        int quant = 0;

        for (int s = 0; s < S; s++, quant++)
        {
            d.Scales[s] = new float[frames];
            for (int f = 0; f < frames; f++)
                d.Scales[s][f] = (float)(Unorm16(c.StreamData[data++]) * c.QuantizationExtents[quant] + c.QuantizationMinimums[quant]);
        }

        int rotQuant = quant;
        for (int s = 0; s < R; s++, rotQuant += 4)
        {
            d.Rotations[s] = new Vector4[frames];
            for (int f = 0; f < frames; f++)
            {
                float x = Quant2(c, data++, rotQuant);
                float y = Quant2(c, data++, rotQuant + 1);
                float z = Quant2(c, data++, rotQuant + 2);
                float w = Quant2(c, data++, rotQuant + 3);
                d.Rotations[s][f] = new Vector4(x, y, z, w);
            }
        }

        int transQuant = rotQuant;
        for (int s = 0; s < T; s++, transQuant += 3)
        {
            d.Translations[s] = new Vector3[frames];
            for (int f = 0; f < frames; f++)
            {
                float x = Quant2(c, data++, transQuant);
                float y = Quant2(c, data++, transQuant + 1);
                float z = Quant2(c, data++, transQuant + 2);
                d.Translations[s][f] = new Vector3(x, y, z);
            }
        }

        return d;
    }

    private static float Quant2(CodecData c, int dataIndex, int quantIndex)
    {
        return (float)(Unorm16(c.StreamData[dataIndex]) * c.QuantizationExtents[quantIndex] + c.QuantizationMinimums[quantIndex]);
    }


    private static DecodedStreams DecodeCodec3(CodecData c)
    {
        int frames = c.FrameCount;
        int S = c.ScaleStreamCount, R = c.RotationStreamCount, T = c.TranslationStreamCount;

        var d = new DecodedStreams
        {
            Scales = new float[S][],
            Rotations = new Vector4[R][],
            Translations = new Vector3[T][],
        };

        int data = 0;
        for (int s = 0; s < S; s++)
        {
            d.Scales[s] = new float[frames];
            for (int f = 0; f < frames; f++)
            {
                d.Scales[s][f] = (float)(Unorm16(c.StreamData[data++]) * c.ScaleExtent + c.ScaleMinimum);
            }
        }

        for (int s = 0; s < R; s++)
        {
            d.Rotations[s] = new Vector4[frames];
            for (int f = 0; f < frames; f++)
            {
                float x = (float)(Unorm16(c.StreamData[data++]) * 2.0 - 1.0);
                float y = (float)(Unorm16(c.StreamData[data++]) * 2.0 - 1.0);
                float z = (float)(Unorm16(c.StreamData[data++]) * 2.0 - 1.0);
                float w = (float)(Unorm16(c.StreamData[data++]) * 2.0 - 1.0);
                d.Rotations[s][f] = new Vector4(x, y, z, w);
            }
        }

        for (int s = 0; s < T; s++)
        {
            d.Translations[s] = new Vector3[frames];
            for (int f = 0; f < frames; f++)
            {
                float x = (float)(Unorm16(c.StreamData[data++]) * c.TranslationExtent[0] + c.TranslationMinimum[0]);
                float y = (float)(Unorm16(c.StreamData[data++]) * c.TranslationExtent[1] + c.TranslationMinimum[1]);
                float z = (float)(Unorm16(c.StreamData[data++]) * c.TranslationExtent[2] + c.TranslationMinimum[2]);
                d.Translations[s][f] = new Vector3(x, y, z);
            }
        }

        return d;
    }
}
