#include "Interop.h"

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

extern HRESULT __cdecl Init(HWND hwnd)
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

    window = std::make_shared<ExternalWindow>(hwnd, 720, 480);

    renderer = new DX11Renderer(false);
    camera = new Camera(90.0f, 0.1f, 1000.0f, window->GetAspectRatio());
    // cube = new CCube();
    //
    renderer->InitialiseGeneral(window);
    // cube->InitDevice();
    return hr;
}

extern HRESULT __cdecl Render(void* pResource, bool isNewSurface)
{
    HRESULT hr = S_OK;
    // return cube->Render(pResource, isNewSurface);
    if (rdoc_api && frameCaptureCount < maxFrameCaptureCount)
        rdoc_api->StartFrameCapture(NULL, NULL);
    // If we've gotten a new Surface, need to initialize the renderTarget.
    // One of the times that this happens is on a resize.
    if (isNewSurface)
    {
        if (FAILED(hr = renderer->Initialise()))
        {
            return hr;
        }

        hr = renderer->InitRenderTarget(pResource);
        if (FAILED(hr))
        {
            return hr;
        }
    }

    camera->Update(0.01f);
    renderer->Render(camera, 0.01f);

    if (rdoc_api && frameCaptureCount++ < maxFrameCaptureCount)
        rdoc_api->EndFrameCapture(NULL, NULL);
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

extern void __cdecl CreateStaticMesh(uint32_t hash, Blob staticMeshTransforms)
{
    renderer->StaticMesh = std::make_shared<StaticMesh>(hash, staticMeshTransforms);
    renderer->StaticMesh->Initialise(renderer->Device);
}

extern void __cdecl AddStaticMeshBufferGroup(uint32_t hash, BufferGroup bufferGroup)
{
    renderer->StaticMesh->AddStaticMeshBufferGroup(bufferGroup);
    auto a = 0;
}

// do we have to copy on an extern? or can we use const ref
extern void __cdecl CreateStaticMeshPart(uint32_t hash, PartInfo partInfo)
{
    renderer->StaticMesh->AddPart(partInfo);
}
