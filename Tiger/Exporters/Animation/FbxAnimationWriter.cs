using Internal.Fbx;
using Tiger.Schema.Animation;

namespace Tiger.Exporters;

/// <summary>
/// Writes an <see cref="FbxAnimationData"/> onto an FBX skeleton as an animation stack: one translation, rotation and
/// scale curve per bone. FBX is not thread-safe, so call this from the same thread as the rest of the export.
/// </summary>
internal static class FbxAnimationWriter
{
    /// <param name="skeletonNodes">The skeleton's nodes in bone order, as returned by AddSkeleton.</param>
    /// <param name="skipChannels">[bone, channel] channels that get no curve, see <see cref="FindRestChannels"/>.</param>
    public static void Write(FbxManager manager, FbxScene scene, IReadOnlyList<FbxNode> skeletonNodes, FbxAnimationData animation, bool[,]? skipChannels = null)
    {
        if (skeletonNodes.Count != animation.Bones.Length)
        {
            throw new ArgumentException($"The skeleton has {skeletonNodes.Count} nodes but the animation has curves for {animation.Bones.Length} bones");
        }

        if (animation.FrameCount <= 0)
        {
            return;
        }

        FbxAnimStack stack = FbxAnimStack.Create(scene, string.IsNullOrEmpty(animation.Name) ? "Animation" : animation.Name);
        FbxAnimLayer layer = FbxAnimLayer.Create(scene, $"{animation.LayerType}");
        stack.AddMember(layer);
        scene.SetCurrentAnimationStack(stack);

        if (Math.Abs(animation.FrameRate - 30) < 1e-6)
        {
            scene.GetGlobalSettings().SetTimeMode(FbxTime.EMode.eFrames30);
        }
        else
        {
            scene.GetGlobalSettings().SetTimeMode(FbxTime.EMode.eCustom);
            scene.GetGlobalSettings().SetCustomFrameRate(animation.FrameRate);
        }

        stack.SetLocalTimeSpan(new FbxTimeSpan(SecondsToTime(0), SecondsToTime((animation.FrameCount - 1) / animation.FrameRate)));

        string[] components = { FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_X, FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_Y, FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_Z };
        for (int i = 0; i < skeletonNodes.Count; i++)
        {
            FbxNode node = skeletonNodes[i];
            float[][] channels = Channels(animation.Bones[i]);
            for (int channel = 0; channel < ChannelCount; channel++)
            {
                if (skipChannels != null && skipChannels[i, channel])
                    continue;

                FbxProperty property = channel < 3 ? node.LclTranslation : channel < 6 ? node.LclRotation : node.LclScaling;
                WriteChannel(property, layer, components[channel % 3], channels[channel], animation.FrameRate);
            }
        }
    }

    private const int ChannelCount = 9;
    private static readonly float[] Tolerances =
    {
        TranslationTolerance, TranslationTolerance, TranslationTolerance,
        RotationTolerance, RotationTolerance, RotationTolerance,
        ScaleTolerance, ScaleTolerance, ScaleTolerance
    };

    // translation xyz, rotation xyz, scale xyz
    private static float[][] Channels(FbxBoneCurves c) => new[]
    {
        c.TranslationX, c.TranslationY, c.TranslationZ,
        c.RotationX, c.RotationY, c.RotationZ,
        c.ScaleX, c.ScaleY, c.ScaleZ
    };

    /// <summary>
    /// [bone, channel] channels that stay at the bone's rest value in every animation. Those dont need a curve at all,
    /// most of the export time is spent creating and writing curves. It has to hold for every animation since in Blender
    /// a channel without a curve keeps whatever the previous action left it at.
    /// </summary>
    /// <param name="restValues">Per bone, the 9 rest pose local values the skeleton nodes were created with.</param>
    public static bool[,] FindRestChannels(IReadOnlyList<FbxAnimationData> animations, IReadOnlyList<float[]> restValues)
    {
        var skip = new bool[restValues.Count, ChannelCount];
        for (int bone = 0; bone < restValues.Count; bone++)
        {
            for (int channel = 0; channel < ChannelCount; channel++)
            {
                float rest = restValues[bone][channel];
                float tolerance = Tolerances[channel];
                skip[bone, channel] = animations.All(animation =>
                    bone < animation.Bones.Length && Channels(animation.Bones[bone])[channel].All(value => MathF.Abs(value - rest) <= tolerance));
            }
        }
        return skip;
    }

    // How close to the rest value a channel has to stay to count as not animated
    private const float TranslationTolerance = 1e-5f; // units
    private const float RotationTolerance = 1e-3f; // degrees
    private const float ScaleTolerance = 1e-5f;

    private static FbxTime SecondsToTime(double seconds)
    {
        var time = new FbxTime();
        time.SetSecondDouble(seconds);
        return time;
    }

    private static void WriteChannel(FbxProperty property, FbxAnimLayer layer, string channel, float[] values, double frameRate)
    {
        FbxAnimCurve curve = property.GetCurve(layer, channel, true);
        if (curve == null)
            throw new InvalidOperationException($"Couldnt create an animation curve for channel {channel}, animation layer is not part of the scene.");

        int keyCount = IsConstant(values) ? 1 : values.Length;

        curve.KeyModifyBegin();
        for (int frame = 0; frame < keyCount; frame++)
        {
            int index = curve.KeyAdd(SecondsToTime(frame / frameRate));
            curve.KeySetValue(index, values[frame]);
            curve.KeySetInterpolation(index, FbxAnimCurveDef.EInterpolationType.eInterpolationLinear);
        }
        curve.KeyModifyEnd();
    }

    private static bool IsConstant(float[] values)
    {
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] != values[0])
            {
                return false;
            }
        }
        return true;
    }
}
