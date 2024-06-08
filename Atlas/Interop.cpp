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
    if (camera == nullptr)
    {
        return;
    }

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
    if (renderer == nullptr)
    {
        return;
    }

    renderer->StaticMesh = std::make_shared<StaticMesh>(hash, staticMeshTransforms);
    renderer->StaticMesh->Initialise(renderer->Device);
}

extern HRESULT __cdecl AddStaticMeshBufferGroup(uint32_t hash, BufferGroup bufferGroup)
{
    if (renderer == nullptr)
    {
        return E_FAIL;
    }

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
    if (renderer == nullptr)
    {
        return E_FAIL;
    }

    HRESULT hr = renderer->StaticMesh->AddPart(partInfo);

    if (FAILED(hr))
    {
        Logger::Log("Failed to create static mesh part for static mesh %x with error %x", hash, hr);
    }

    return hr;
}

extern HRESULT __cdecl BakeMaterial(uint32_t hash, PartMaterial material)
{
    if (renderer == nullptr)
    {
        return E_FAIL;
    }

    /// to bake a material we need to take the render targets created from executing the VS and PS and bake them into textures, done via bake.hlsl
    /// for this we create new RTV and SRV for the textures (albedo, normal, roughness, metallic, ao, emission)
    /// only issue is the VS is going to align the vertices from world to projection space
    /// so we need to pass in a set of values that makes the texture appear perfectly with the correct size
    /// 1. update projection matrix to account for texture size
    /// 2. position camera to look at the object
    /// 3. set object to be same as lighting, a square matching the aspect ratio?
    /// 4. render
    /// 5. take the output render targets and save them as textures

    // create a static mesh with one part which is a square
    std::vector<float> meshTransform = {0.f, 0.f, 0.f, 1.f};
    std::vector<float> uvTransform = {1.f, 1.f, 0.f, 0.f};
    byte* buffer[20];
    memcpy(buffer, meshTransform.data(), meshTransform.size() * sizeof(float));
    memcpy(buffer + meshTransform.size(), uvTransform.data(), uvTransform.size() * sizeof(float));
    StaticMesh quad(hash, Blob(buffer, 0x20));
    PartInfo partInfo;
    partInfo.IndexCount = 6;
    partInfo.IndexOffset = 0;
    partInfo.PartMaterial = material;
    if (FAILED(quad.AddPart(partInfo)))
    {
        Logger::Log("Failed to create static mesh part for static mesh %x", hash);
        return E_FAIL;
    }

    //
    //
    // QuadVertex vertices[] = {
    //     {XMFLOAT4(-1.0f, 1.0f, 0.0f, 1.0f), XMFLOAT2(0.0f, 0.0f)},
    //     {XMFLOAT4(1.0f, 1.0f, 0.0f, 1.0f), XMFLOAT2(1.0f, 0.0f)},
    //     {XMFLOAT4(1.0f, -1.0f, 0.0f, 1.0f), XMFLOAT2(1.0f, 1.0f)},
    //     {XMFLOAT4(-1.0f, -1.0f, 0.0f, 1.0f), XMFLOAT2(0.0f, 1.0f)},
    // };

    BufferGroup bufferGroup;
    bufferGroup.IndexBuffer = Blob();
    if (FAILED(quad.AddStaticMeshBufferGroup(bufferGroup)))
    {
        Logger::Log("Failed to add buffer group for static mesh %x", hash);
        return E_FAIL;
    }

    return S_OK;
}

extern void __cdecl ResizeWindow(int width, int height)
{
    if (window == nullptr || renderer == nullptr)
    {
        return;
    }

    window->SetRect(width, height);
    renderer->SetRasterizerViewport();
}

extern void __cdecl RegisterMouseScroll(int delta)
{
    if (camera == nullptr)
    {
        return;
    }
    camera->UpdateScroll(delta);
}

extern void __cdecl ResetCamera()
{
    if (camera == nullptr)
    {
        return;
    }
    camera->Reset();
}

extern void __cdecl MoveOrbitOrigin(float mouseX, float mouseY)
{
    if (camera == nullptr)
    {
        return;
    }

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
