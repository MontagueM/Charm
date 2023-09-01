#include "Interop.h"

#include "Logger.h"
#include "NativeWindow.h"
#include "Renderer.h"
#include "StaticMesh.h"
#include "renderdoc_app.h"

#include <d3d11.h>

#include <memory>

RENDERDOC_API_1_1_2* rdoc_api = NULL;
static int frameCaptureCount = 0;
static constexpr int maxFrameCaptureCount = 1;
DX11Renderer* renderer;
std::shared_ptr<ExternalWindow> window;
Camera* camera;

extern HRESULT __cdecl Init(HWND hwnd, int width, int height)
{
    HRESULT hr = S_OK;

    if (HMODULE mod = GetModuleHandleA("renderdoc.dll"))
    {
        pRENDERDOC_GetAPI RENDERDOC_GetAPI = (pRENDERDOC_GetAPI) GetProcAddress(mod, "RENDERDOC_GetAPI");
        int ret = RENDERDOC_GetAPI(eRENDERDOC_API_Version_1_1_2, (void**) &rdoc_api);
        assert(ret == 1);
    }

    if (hwnd == nullptr)
    {
        return E_INVALIDARG;
    }

    window = std::make_shared<ExternalWindow>(hwnd, width, height);

    renderer = new DX11Renderer(false);
    camera = new Camera(90.0f, 0.1f, 1000.0f, window->GetAspectRatio());
    window->OnSizeChanged.Add(camera->OnWindowSizeChanged);
    // cube = new CCube();
    //
    renderer->InitialiseGeneral(window);
    // cube->InitDevice();
    return hr;
}

extern HRESULT __cdecl Render(void* pResource, bool isNewSurface)
{
    HRESULT hr = S_OK;
    if (rdoc_api && frameCaptureCount < maxFrameCaptureCount)
        rdoc_api->StartFrameCapture(NULL, NULL);

    if (isNewSurface)
    {
        if (FAILED(hr = renderer->Initialise()))
        {
            Logger::Log("Failed to initialise renderer");
            return hr;
        }

        hr = renderer->InitRenderTarget(pResource);
        if (FAILED(hr))
        {
            return hr;
        }
    }

    camera->Update(0.01f);
    hr = renderer->Render(camera, 0.01f);

    if (rdoc_api && frameCaptureCount++ < maxFrameCaptureCount)
        rdoc_api->EndFrameCapture(NULL, NULL);

    return hr;
}

extern void __cdecl RegisterMouseDelta(float mouseX, float mouseY)
{
    camera->UpdateFromMouse(SimpleMath::Vector2(mouseX, mouseY), 0.01f, true);
}

BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD fwdReason, LPVOID lpvReserved)
{
    return TRUE;
}

extern void __cdecl MoveCamera(MoveDirection direction)
{
    if (camera == nullptr)
    {
        return;
    }

    camera->UpdateFromKeyboard(direction, 0.1f);
}

extern void __cdecl SetCameraMode(CameraMode mode)
{
    if (camera == nullptr)
    {
        return;
    }

    camera->SetMode(mode);
}

extern void __cdecl CreateStaticMesh(uint32_t hash, Blob staticMeshTransforms)
{
    renderer->StaticMesh = std::make_shared<StaticMesh>(hash, staticMeshTransforms);
    renderer->StaticMesh->Initialise(renderer->Device);
}

extern HRESULT __cdecl AddStaticMeshBufferGroup(uint32_t hash, BufferGroup bufferGroup)
{
    HRESULT hr = renderer->StaticMesh->AddStaticMeshBufferGroup(bufferGroup);

    if (FAILED(hr))
    {
        Logger::Log("Failed to add buffer group for static mesh %x with error %x", hash, hr);
    }

    return hr;
}

// do we have to copy on an extern? or can we use const ref
extern HRESULT __cdecl CreateStaticMeshPart(uint32_t hash, PartInfo partInfo)
{
    HRESULT hr = renderer->StaticMesh->AddPart(partInfo);

    if (FAILED(hr))
    {
        Logger::Log("Failed to create static mesh part for static mesh %x with error %x", hash, hr);
    }

    return hr;
}

extern void __cdecl ResizeWindow(int width, int height)
{
    window->SetRect(width, height);
    renderer->SetRasterizerViewport();
}

extern void __cdecl RegisterMouseScroll(int delta)
{
    camera->UpdateScroll(delta);
}

extern void __cdecl ResetCamera()
{
    camera->Reset();
}

extern void __cdecl MoveOrbitOrigin(float mouseX, float mouseY)
{
    camera->MoveOrbitOrigin(mouseX, mouseY);
}

extern void __cdecl Cleanup()
{
    if (renderer == nullptr)
    {
        return;
    }

    renderer->Cleanup();
}
