#include "Input.h"

#include "NativeWindow.h"

#include <iostream>
#include <ostream>

DXSM::Vector2 Input::PreviousMousePosition = DXSM::Vector2(0, 0);
bool Input::bIsMouseCaptured = false;

const DXSM::Vector2 Input::GetMouseDelta()
{
    const DXSM::Vector2& CurrentMousePosition = GetMousePosition();
    const DXSM::Vector2 Delta = CurrentMousePosition - PreviousMousePosition;
    return Delta;
}

void Input::UpdatePrevious(std::shared_ptr<const NativeWindow> window)
{
    // PreviousMousePosition = GetMousePosition();
    if (IsMouseCaptured())
    {
        const Rect& WindowRect = window->GetRect();
        const float MiddleRectX = WindowRect.X + WindowRect.Width / 2.0f;
        const float MiddleRectY = WindowRect.Y + WindowRect.Height / 2.0f;
        SetCursorPos(MiddleRectX, MiddleRectY);
        auto x = GetMousePosition();
        PreviousMousePosition = DXSM::Vector2(MiddleRectX, MiddleRectY);
    }
    else
    {
        PreviousMousePosition = GetMousePosition();
    }
}

bool Input::IsKeyDown(const char& key)
{
    return (GetAsyncKeyState(key) & 0x8000) != 0;
}

bool Input::IsMouseCaptured()
{
    return bIsMouseCaptured;
}

void Input::SetMouseCaptured(bool captured, HWND hwnd)
{
    bIsMouseCaptured = captured;
    if (IsMouseCaptured())
    {
        SetCapture(hwnd);
        RECT rect;
        GetWindowRect(hwnd, &rect);
        ClipCursor(&rect);
        ShowCursor(false);
    }
    else
    {
        ReleaseCapture();
        ClipCursor(NULL);
        ShowCursor(true);
    }
}

const DXSM::Vector2 Input::GetMousePosition()
{
    POINT p;
    GetCursorPos(&p);
    return DXSM::Vector2((float) p.x, (float) p.y);
}
