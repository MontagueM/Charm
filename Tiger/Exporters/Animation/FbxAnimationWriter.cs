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
    public static void Write(FbxManager manager, FbxScene scene, IReadOnlyList<FbxNode> skeletonNodes, FbxAnimationData animation)
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

        for (int i = 0; i < skeletonNodes.Count; i++)
        {
            FbxNode node = skeletonNodes[i];
            FbxBoneCurves c = animation.Bones[i];

            WriteChannel(node.LclTranslation, layer, FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_X, c.TranslationX, animation.FrameRate);
            WriteChannel(node.LclTranslation, layer, FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_Y, c.TranslationY, animation.FrameRate);
            WriteChannel(node.LclTranslation, layer, FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_Z, c.TranslationZ, animation.FrameRate);
            WriteChannel(node.LclRotation, layer, FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_X, c.RotationX, animation.FrameRate);
            WriteChannel(node.LclRotation, layer, FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_Y, c.RotationY, animation.FrameRate);
            WriteChannel(node.LclRotation, layer, FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_Z, c.RotationZ, animation.FrameRate);
            WriteChannel(node.LclScaling, layer, FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_X, c.ScaleX, animation.FrameRate);
            WriteChannel(node.LclScaling, layer, FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_Y, c.ScaleY, animation.FrameRate);
            WriteChannel(node.LclScaling, layer, FbxWrapperNative.FBXSDK_CURVENODE_COMPONENT_Z, c.ScaleZ, animation.FrameRate);
        }
    }

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
