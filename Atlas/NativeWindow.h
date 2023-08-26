#pragma once
#include "Camera.h"

struct Rect
{
    UINT X;
    UINT Y;
    UINT Width;
    UINT Height;
};

// todo generalise
struct Delegate
{
    std::list<std::function<void(int width, int height)>> functions;

public:
    void Add(std::function<void(int width, int height)> function) { functions.push_back(function); }

    void Execute(int width, int height)
    {
        for (const auto& function : functions)
        {
            function(width, height);
        }
    }
};

class RenderPanel
{
public:
    Delegate OnSizeChanged;

    HWND GetHandle() const;
    Rect GetRect() const;
    float GetAspectRatio() const;
    void SetRect(UINT width, UINT height);

protected:
    HWND Handle;
    Rect WindowRect;

    RenderPanel(UINT width, UINT height);
    RenderPanel(HWND handle, UINT width, UINT height);
};

class ExternalWindow : public RenderPanel
{
public:
    ExternalWindow(HWND handle, UINT width, UINT height);
};

class NativeWindow : public RenderPanel
{
public:
    NativeWindow(LPCWSTR title, UINT width, UINT height);
    ~NativeWindow();

private:
    LPCWSTR Title;
    HINSTANCE Instance;

    void MakeClass() const;
    void MakeWindow();
};
