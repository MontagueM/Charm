using System;
using System.Runtime.InteropServices;
using System.Windows;
using Tiger;
using Tiger.Schema.Static;

namespace AtlasSharp;

public enum CameraMode
{
    Orbit,
    Free,
}

public static class NativeMethods
{
    /// <summary>
    /// Variable used to track whether the missing dependency dialog has been displayed,
    /// used to prevent multiple notifications of the same failure.
    /// </summary>
    private static bool errorHasDisplayed;

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern long Init(nint hwnd, int width, int height);

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ResizeWindow(int width, int height);

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Cleanup();

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern long Render(IntPtr resourcePointer, bool isNewSurface);

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void MoveCamera(MoveDirection direction);

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetCameraMode(CameraMode mode);

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ResetCamera();

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern long RegisterMouseDelta(float mouseX, float mouseY);

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern long MoveOrbitOrigin(float mouseX, float mouseY);

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern long RegisterMouseScroll(int delta);

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern long CreateStaticMesh(uint hash, Blob staticMeshTransforms);

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern long AddStaticMeshBufferGroup(uint hash, BufferGroup bufferGroup);

    [DllImport("Atlas.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern long CreateStaticMeshPart(uint hash, AtlasView.PartInfo partInfo);

    /// <summary>
    /// Method used to invoke an Action that will catch DllNotFoundExceptions and display a warning dialog.
    /// </summary>
    /// <param name="action">The Action to invoke.</param>
    public static void InvokeWithDllProtection(Action action)
    {
        InvokeWithDllProtection(
            () =>
            {
                action.Invoke();
                return 0;
            });
    }

    /// <summary>
    /// Method used to invoke A Func that will catch DllNotFoundExceptions and display a warning dialog.
    /// </summary>
    /// <param name="func">The Func to invoke.</param>
    /// <returns>The return value of func, or default(T) if a DllNotFoundException was caught.</returns>
    /// <typeparam name="T">The return type of the func.</typeparam>
    public static T InvokeWithDllProtection<T>(Func<T> func)
    {
        try
        {
            return func.Invoke();
        }
        catch (DllNotFoundException e)
        {
            if (!errorHasDisplayed)
            {
                MessageBox.Show("This sample requires:\nManual build of the D3DVisualization project, which requires installation of Windows 10 SDK or DirectX SDK.\n" +
                                "Installation of the DirectX runtime on non-build machines.\n\n"+
                                "Detailed exception message: " + e.Message, "WPF D3D11 Interop",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                errorHasDisplayed = true;

                if (Application.Current != null)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        return default(T);
    }
}
