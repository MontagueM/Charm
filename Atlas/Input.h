#pragma once
#include "NativeWindow.h"
#include "SimpleMath.h"

#include <d3d11.h>

#include <memory>

#define DXSM DirectX::SimpleMath

class Input
{
public:
    // static NativeWindow* InputWindow;
    static const DXSM::Vector2 GetMouseDelta();
    static void UpdatePrevious(std::shared_ptr<const NativeWindow> window);
    static bool IsKeyDown(const char& key);
    static bool IsMouseCaptured();
    static void SetMouseCaptured(bool captured, HWND hwnd);

private:
    static DXSM::Vector2 PreviousMousePosition;
    static bool bIsMouseCaptured;

    static const DXSM::Vector2 GetMousePosition();
};
