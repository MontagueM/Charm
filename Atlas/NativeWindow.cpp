#include "NativeWindow.h"

#include "Camera.h"
#include "Input.h"

RenderPanel::RenderPanel(UINT width, UINT height) : WindowRect({100, 100, width, height})
{
}

RenderPanel::RenderPanel(HWND handle, UINT width, UINT height) : Handle(handle), WindowRect({100, 100, width, height})
{
}

ExternalWindow::ExternalWindow(HWND handle, UINT width, UINT height) : RenderPanel(handle, width, height)
{
}

HWND RenderPanel::GetHandle() const
{
    return Handle;
}

Rect RenderPanel::GetRect() const
{
    return WindowRect;
}

float RenderPanel::GetAspectRatio() const
{
    return static_cast<float>(WindowRect.Width) / static_cast<float>(WindowRect.Height);
}

void RenderPanel::SetRect(UINT width, UINT height)
{
    WindowRect.Width = width;
    WindowRect.Height = height;
    OnSizeChanged.Execute(width, height);
}

NativeWindow::NativeWindow(LPCWSTR title, UINT width, UINT height) : RenderPanel(width, height), Title(title)
{
    Instance = GetModuleHandle(NULL);
    MakeClass();
    MakeWindow();
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    PAINTSTRUCT ps;
    HDC hdc;

    switch (message)
    {
        case WM_PAINT:
            hdc = BeginPaint(hWnd, &ps);
            EndPaint(hWnd, &ps);
            break;

        case WM_DESTROY:
            PostQuitMessage(0);
            break;

            // Note that this tutorial does not handle resizing (WM_SIZE) requests,
            // so we created the window without the resize border.

        case WM_LBUTTONDOWN:
            Input::SetMouseCaptured(true, hWnd);

            break;

        case WM_RBUTTONDOWN:
            Input::SetMouseCaptured(false, hWnd);
            break;

        default:
            return DefWindowProc(hWnd, message, wParam, lParam);
    }

    return 0;
}

void NativeWindow::MakeClass() const
{
    WNDCLASSEX wc = {0};
    wc.style = CS_HREDRAW | CS_VREDRAW;
    wc.lpfnWndProc = WndProc;
    wc.hInstance = Instance;
    wc.hCursor = LoadCursor(NULL, IDC_ARROW);
    wc.lpszClassName = Title;
    wc.cbSize = sizeof(WNDCLASSEX);
    ATOM atom = RegisterClassEx(&wc);
}

void NativeWindow::MakeWindow()
{
    Handle = CreateWindowEx(0, Title, Title, WS_CAPTION | WS_MINIMIZEBOX | WS_SYSMENU, WindowRect.X, WindowRect.Y, WindowRect.Width,
        WindowRect.Height, 0, 0, Instance, nullptr);
    ShowWindow(Handle, SW_SHOW);
    SetForegroundWindow(Handle);
    SetFocus(Handle);
}

NativeWindow::~NativeWindow()
{
    if (Handle)
    {
        UnregisterClass(Title, Instance);
        DestroyWindow(Handle);
    }
}
