﻿#include "Renderer.h"

#include "DDSTextureLoader.h"
#include "Debug.h"
#include "Logger.h"
#include "StaticMesh.h"

#include <d3dcompiler.h>

#include <fstream>
#include <iostream>
#include <ostream>

DX11Renderer::DX11Renderer(bool useSwapchain) : UseSwapchain(useSwapchain)
{
}

struct GeometryVertex0
{
    XMFLOAT3 Pos;
    XMFLOAT3 Normal;
};

struct GeometryVertex1
{
    XMFLOAT2 Tex;
};

struct QuadVertex
{
    XMFLOAT4 Pos;
    XMFLOAT2 Tex;
};

// struct CBNeverChanges
// {
// };

struct CBChangeOnResize
{
    XMMATRIX Projection;
};

struct ScopeView
{
    XMMATRIX WorldToProjective;
    XMMATRIX CameraToWorld;
    XMFLOAT4 Target;
    XMFLOAT4 ViewMiscellaneous;
};

struct CBLightingChangesEveryFrame
{
    XMFLOAT4 LightDir[2];
    XMFLOAT4 LightColor[2];
};

struct SimpleVertex
{
    XMFLOAT3 Pos;
    XMFLOAT4 Color;
};

struct ConstantBuffer
{
    XMMATRIX mWorld;
    XMMATRIX mView;
    XMMATRIX mProjection;
};

HRESULT DX11Renderer::Initialise()
{
    // Initialize the projection matrix - todo camera

    // HRESULT hr = InitialiseGeneral();
    // if (FAILED(hr))
    //     return hr;

    HRESULT hr = CreateDepthStencilView();
    if (FAILED(hr))
    {
        Logger::Log("Failed to initialise depth stencil");
        return hr;
    }

    hr = InitialiseLightingPass();
    if (FAILED(hr))
    {
        Logger::Log("Failed to initialise lighting pass");
        return hr;
    }

    hr = Debug::Initialise(Device);
    if (FAILED(hr))
    {
        Logger::Log("Failed to initialise debug pass");
        return hr;
    }

    return S_OK;
}

HRESULT DX11Renderer::CreateShaderFromCompiledFile(LPCWSTR FileName, ID3D11PixelShader** PixelShader, ID3D11Device* Device)
{
    ID3D10Blob* ShaderBlob;
    HRESULT hr = D3DReadFileToBlob(FileName, &ShaderBlob);
    if (FAILED(hr))
        return hr;

    hr = Device->CreatePixelShader(ShaderBlob->GetBufferPointer(), ShaderBlob->GetBufferSize(), nullptr, PixelShader);
    if (FAILED(hr))
        return hr;

    return hr;
}

HRESULT DX11Renderer::CreateShaderFromCompiledFile(
    LPCWSTR FileName, ID3D11VertexShader** VertexShader, ID3D10Blob*& ShaderBlob, ID3D11Device* Device)
{
    HRESULT hr = D3DReadFileToBlob(FileName, &ShaderBlob);
    if (FAILED(hr))
        return hr;

    hr = Device->CreateVertexShader(ShaderBlob->GetBufferPointer(), ShaderBlob->GetBufferSize(), nullptr, VertexShader);
    if (FAILED(hr))
        return hr;

    return hr;
}

HRESULT DX11Renderer::CreateShaderFromHlslFile(
    LPCWSTR FileName, LPCSTR EntryPoint, ID3D11VertexShader** VertexShader, ID3D10Blob*& ShaderBlob, ID3D11Device* Device)
{
    const LPWSTR path = new WCHAR[MAX_PATH];
    GetCurrentDirectoryW(MAX_PATH, path);
    wcscat_s(path, MAX_PATH, L"\\");
    wcscat_s(path, MAX_PATH, FileName);

    Logger::Log("Creating shader from file %ws", path);

    HRESULT hr = CompileShaderFromFile(path, EntryPoint, "vs_5_0", &ShaderBlob);
    if (FAILED(hr))
        return hr;

    hr = Device->CreateVertexShader(ShaderBlob->GetBufferPointer(), ShaderBlob->GetBufferSize(), nullptr, VertexShader);
    if (FAILED(hr))
        return hr;

    return hr;
}

HRESULT DX11Renderer::CreateShaderFromHlslFile(LPCWSTR FileName, LPCSTR EntryPoint, ID3D11PixelShader** PixelShader, ID3D11Device* Device)
{
    ID3DBlob* ShaderBlob = nullptr;
    HRESULT hr = CompileShaderFromFile(FileName, EntryPoint, "ps_5_0", &ShaderBlob);
    if (FAILED(hr))
        return hr;

    hr = Device->CreatePixelShader(ShaderBlob->GetBufferPointer(), ShaderBlob->GetBufferSize(), nullptr, PixelShader);
    ShaderBlob->Release();
    if (FAILED(hr))
        return hr;

    return hr;
}

HRESULT DX11Renderer::CompileShaderFromFile(LPCWSTR FileName, LPCSTR EntryPoint, LPCSTR ShaderModel, ID3DBlob** ShaderBlob)
{
    ID3DBlob* ErrorShaderBlob = nullptr;
    DWORD dwShaderFlags = D3DCOMPILE_ENABLE_STRICTNESS;
#ifdef _DEBUG
    dwShaderFlags |= D3DCOMPILE_DEBUG;
    dwShaderFlags |= D3DCOMPILE_SKIP_OPTIMIZATION;
#endif

    HRESULT hr = D3DCompileFromFile(FileName, nullptr, nullptr, EntryPoint, ShaderModel, dwShaderFlags, 0, ShaderBlob, &ErrorShaderBlob);
    if (hr != S_OK && ErrorShaderBlob != nullptr)
    {
        const char* error = static_cast<const char*>(ErrorShaderBlob->GetBufferPointer());
        std::cout << error << std::endl;
    }

    return hr;
}

HRESULT DX11Renderer::InitialiseGeneral(std::shared_ptr<RenderPanel> window)
{
    this->window = window;

    HRESULT hr = S_OK;

    if (UseSwapchain)
    {
        hr = CreateDeviceAndSwapChain();
        if (FAILED(hr))
            return hr;

        hr = CreateBackBufferView();
        if (FAILED(hr))
            return hr;

        SetRasterizerViewport();
    }
    else
    {
        hr = CreateDevice();
        if (FAILED(hr))
            return hr;
    }

    // SetRasterizerViewport();

    // Initialize the world matrix
    World1 = XMMatrixIdentity();
    World2 = XMMatrixIdentity();

    return hr;
}

HRESULT DX11Renderer::CreateDeviceAndSwapChain()
{
    DXGI_SWAP_CHAIN_DESC Desc{};
    Desc.BufferCount = 1;
    Desc.BufferDesc.Width = window->GetRect().Width;
    Desc.BufferDesc.Height = window->GetRect().Height;
    Desc.BufferDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    Desc.BufferDesc.RefreshRate.Numerator = 240;
    Desc.BufferDesc.RefreshRate.Denominator = 1;
    Desc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;
    Desc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    Desc.OutputWindow = window->GetHandle();
    Desc.SampleDesc.Count = 1;
    Desc.SampleDesc.Quality = 0;
    Desc.Windowed = TRUE;
    // Desc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;    // enables vsync

    UINT createDeviceFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
#ifdef _DEBUG
    createDeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif
    D3D_FEATURE_LEVEL featureLevel;
    const D3D_FEATURE_LEVEL featureLevelArray[1] = {D3D_FEATURE_LEVEL_11_0};

    HRESULT hr = D3D11CreateDeviceAndSwapChain(NULL, D3D_DRIVER_TYPE_HARDWARE, NULL, createDeviceFlags, featureLevelArray, 1,
        D3D11_SDK_VERSION, &Desc, &SwapChain, &Device, &featureLevel, &DeviceContext);

    return hr;
}

HRESULT DX11Renderer::CreateDevice()
{
    UINT createDeviceFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
#ifdef _DEBUG
    createDeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif
    D3D_FEATURE_LEVEL featureLevel;
    const D3D_FEATURE_LEVEL featureLevelArray[1] = {D3D_FEATURE_LEVEL_11_0};

    HRESULT hr = D3D11CreateDevice(NULL, D3D_DRIVER_TYPE_HARDWARE, NULL, createDeviceFlags, featureLevelArray, 1, D3D11_SDK_VERSION,
        &Device, &featureLevel, &DeviceContext);

    return hr;
}

HRESULT DX11Renderer::CreateBackBufferView()
{
    ID3D11Texture2D* pBackBuffer = nullptr;
    HRESULT hr = SwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), reinterpret_cast<void**>(&pBackBuffer));
    if (FAILED(hr))
        return hr;

    hr = Device->CreateRenderTargetView(pBackBuffer, nullptr, &BackBufferView);
    pBackBuffer->Release();
    if (FAILED(hr))
        return hr;

    return hr;
}

HRESULT DX11Renderer::SetRasterizerViewport()
{
    D3D11_VIEWPORT vp;
    vp.Width = static_cast<FLOAT>(window->GetRect().Width);
    vp.Height = static_cast<FLOAT>(window->GetRect().Height);
    vp.MinDepth = 0.0f;
    vp.MaxDepth = 1.0f;
    vp.TopLeftX = 0;
    vp.TopLeftY = 0;
    DeviceContext->RSSetViewports(1, &vp);

    D3D11_RASTERIZER_DESC rasterizerDesc{};
    rasterizerDesc.CullMode = D3D11_CULL_NONE;
    rasterizerDesc.FillMode = D3D11_FILL_SOLID;
    rasterizerDesc.FrontCounterClockwise = true;
    rasterizerDesc.DepthClipEnable = true;

    ID3D11RasterizerState* rasterizerState;
    HRESULT hr = Device->CreateRasterizerState(&rasterizerDesc, &rasterizerState);
    if (FAILED(hr))
        return hr;

    DeviceContext->RSSetState(rasterizerState);

    return S_OK;
}

HRESULT DX11Renderer::CreateDepthStencilView()
{
    HRESULT hr;
    D3D11_TEXTURE2D_DESC descDepth{};
    descDepth.Width = window->GetRect().Width;
    descDepth.Height = window->GetRect().Height;
    descDepth.MipLevels = 1;
    descDepth.ArraySize = 1;
    descDepth.Format = DXGI_FORMAT_D24_UNORM_S8_UINT;
    descDepth.SampleDesc.Count = 1;
    descDepth.SampleDesc.Quality = 0;
    descDepth.Usage = D3D11_USAGE_DEFAULT;
    descDepth.BindFlags = D3D11_BIND_DEPTH_STENCIL;
    descDepth.CPUAccessFlags = 0;
    descDepth.MiscFlags = 0;
    // todo DepthStencil texture might not need to be global? unsure how views work here if the texture goes out of scope
    hr = Device->CreateTexture2D(&descDepth, nullptr, &DepthStencil);
    if (FAILED(hr))
        return hr;

    D3D11_DEPTH_STENCIL_VIEW_DESC descDSV = {};
    descDSV.Format = descDepth.Format;
    descDSV.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;
    descDSV.Texture2D.MipSlice = 0;
    hr = Device->CreateDepthStencilView(DepthStencil, &descDSV, &DepthStencilView);
    if (FAILED(hr))
        return hr;

    D3D11_DEPTH_STENCIL_DESC dsDesc;

    // Depth test parameters
    dsDesc.DepthEnable = true;
    dsDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
    dsDesc.DepthFunc = D3D11_COMPARISON_GREATER;

    // Stencil test parameters
    dsDesc.StencilEnable = false;
    dsDesc.StencilReadMask = 0xFF;
    dsDesc.StencilWriteMask = 0xFF;

    // Stencil operations if pixel is front-facing
    dsDesc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_INCR;
    dsDesc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;

    // Stencil operations if pixel is back-facing
    dsDesc.BackFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.BackFace.StencilDepthFailOp = D3D11_STENCIL_OP_DECR;
    dsDesc.BackFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
    dsDesc.BackFace.StencilFunc = D3D11_COMPARISON_ALWAYS;

    // Create depth stencil state
    ID3D11DepthStencilState* depthStencilState;
    Device->CreateDepthStencilState(&dsDesc, &depthStencilState);
    DeviceContext->OMSetDepthStencilState(depthStencilState, 1);

    return hr;
}

HRESULT DX11Renderer::InitialiseLightingPass()
{
    HRESULT hr;

    hr = CreateLightingRenderTargets();
    if (FAILED(hr))
    {
        Logger::Log("Failed to create lighting RTs");
        return hr;
    }

    ID3D10Blob* VertexShaderBlob = nullptr;
    hr = CreateLightingVertexShader(VertexShaderBlob);
    if (FAILED(hr))
    {
        Logger::Log("Failed to create lighting vertex shader");
        return hr;
    }

    hr = CreateLightingVertexLayout(VertexShaderBlob);
    if (FAILED(hr))
    {
        Logger::Log("Failed to create lighting vertex layout");
        return hr;
    }

    VertexShaderBlob->Release();

    hr = CreateLightingPixelShader();
    if (FAILED(hr))
    {
        Logger::Log("Failed to create lighting pixel shader");
        return hr;
    }

    hr = CreateLightingVertexBuffer();
    if (FAILED(hr))
    {
        Logger::Log("Failed to create lighting vertex buffer");
        return hr;
    }

    hr = CreateLightingIndexBuffer();
    if (FAILED(hr))
    {
        Logger::Log("Failed to create lighting index buffer");
        return hr;
    }

    hr = CreateLightingConstantBuffer();
    if (FAILED(hr))
    {
        Logger::Log("Failed to create lighting constant buffer");
        return hr;
    }

    return hr;
}

HRESULT DX11Renderer::CreateLightingRenderTargets()
{
    HRESULT hr;

    D3D11_TEXTURE2D_DESC descDepth = {};
    descDepth.Width = window->GetRect().Width;
    descDepth.Height = window->GetRect().Height;
    descDepth.MipLevels = 1;
    descDepth.ArraySize = 1;
    descDepth.Format = DXGI_FORMAT_R32G32B32A32_FLOAT;
    descDepth.SampleDesc.Count = 1;
    descDepth.SampleDesc.Quality = 0;
    descDepth.Usage = D3D11_USAGE_DEFAULT;
    descDepth.BindFlags = D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE;
    descDepth.CPUAccessFlags = 0;
    descDepth.MiscFlags = 0;

    D3D11_RENDER_TARGET_VIEW_DESC renderTargetViewDesc;
    renderTargetViewDesc.Format = DXGI_FORMAT_R32G32B32A32_FLOAT;
    renderTargetViewDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2D;
    renderTargetViewDesc.Texture2D.MipSlice = 0;

    D3D11_SHADER_RESOURCE_VIEW_DESC shaderResourceViewDesc;
    shaderResourceViewDesc.Format = DXGI_FORMAT_R32G32B32A32_FLOAT;
    shaderResourceViewDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
    shaderResourceViewDesc.Texture2D.MostDetailedMip = 0;
    shaderResourceViewDesc.Texture2D.MipLevels = 1;

    hr = Device->CreateTexture2D(&descDepth, nullptr, &RT0Texture);
    if (FAILED(hr))
        return hr;
    hr = Device->CreateRenderTargetView(RT0Texture, &renderTargetViewDesc, &RT0View);
    if (FAILED(hr))
        return hr;
    hr = Device->CreateShaderResourceView(RT0Texture, &shaderResourceViewDesc, &RT0SRV);
    // RT0Texture->Release(); // todo test if this is allowed or not
    if (FAILED(hr))
        return hr;

    hr = Device->CreateTexture2D(&descDepth, nullptr, &RT1Texture);
    if (FAILED(hr))
        return hr;
    hr = Device->CreateRenderTargetView(RT1Texture, nullptr, &RT1View);
    if (FAILED(hr))
        return hr;
    hr = Device->CreateShaderResourceView(RT1Texture, &shaderResourceViewDesc, &RT1SRV);
    // RT1Texture->Release();
    if (FAILED(hr))
        return hr;

    hr = Device->CreateTexture2D(&descDepth, nullptr, &RT2Texture);
    if (FAILED(hr))
        return hr;
    hr = Device->CreateRenderTargetView(RT2Texture, nullptr, &RT2View);
    if (FAILED(hr))
        return hr;
    hr = Device->CreateShaderResourceView(RT2Texture, &shaderResourceViewDesc, &RT2SRV);
    // RT1Texture->Release();
    if (FAILED(hr))
        return hr;

    return hr;
}

HRESULT DX11Renderer::CreateLightingVertexShader(ID3D10Blob*& VertexShaderBlob)
{
    const HRESULT hr = CreateShaderFromHlslFile(L"Shaders/Lighting.hlsl", "VS2", &LightingVertexShader, VertexShaderBlob, Device);
    return hr;
}

HRESULT DX11Renderer::CreateLightingPixelShader()
{
    const HRESULT hr = CreateShaderFromHlslFile(L"Shaders/Lighting.hlsl", "PS2", &LightingPixelShader, Device);
    return hr;
}

HRESULT DX11Renderer::CreateLightingVertexLayout(ID3D10Blob* VertexShaderBlob)
{
    D3D11_INPUT_ELEMENT_DESC layout[] = {
        {"SV_POSITION", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0},
        {"TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 16, D3D11_INPUT_PER_VERTEX_DATA, 0},
    };
    UINT numElements = ARRAYSIZE(layout);

    HRESULT hr = Device->CreateInputLayout(
        layout, numElements, VertexShaderBlob->GetBufferPointer(), VertexShaderBlob->GetBufferSize(), &QuadVertexLayout);

    VertexShaderBlob->Release();

    return hr;
}

HRESULT DX11Renderer::CreateLightingVertexBuffer()
{
    QuadVertex vertices[] = {
        {XMFLOAT4(-1.0f, 1.0f, 0.0f, 1.0f), XMFLOAT2(0.0f, 0.0f)},
        {XMFLOAT4(1.0f, 1.0f, 0.0f, 1.0f), XMFLOAT2(1.0f, 0.0f)},
        {XMFLOAT4(1.0f, -1.0f, 0.0f, 1.0f), XMFLOAT2(1.0f, 1.0f)},
        {XMFLOAT4(-1.0f, -1.0f, 0.0f, 1.0f), XMFLOAT2(0.0f, 1.0f)},
    };
    D3D11_BUFFER_DESC bd = {};
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = ARRAYSIZE(vertices) * sizeof(QuadVertex);
    bd.BindFlags = D3D11_BIND_VERTEX_BUFFER;
    bd.CPUAccessFlags = 0;

    D3D11_SUBRESOURCE_DATA InitData = {};
    InitData.pSysMem = vertices;
    HRESULT hr = Device->CreateBuffer(&bd, &InitData, &QuadVertexBuffer);

    return hr;
}

HRESULT DX11Renderer::CreateLightingIndexBuffer()
{
    WORD indices[] = {0, 2, 1, 0, 3, 2};

    D3D11_BUFFER_DESC bd{};
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = ARRAYSIZE(indices) * sizeof(WORD);
    bd.BindFlags = D3D11_BIND_INDEX_BUFFER;
    bd.CPUAccessFlags = 0;

    D3D11_SUBRESOURCE_DATA InitData = {};
    InitData.pSysMem = indices;
    const HRESULT hr = Device->CreateBuffer(&bd, &InitData, &QuadIndexBuffer);

    return hr;
}

HRESULT DX11Renderer::CreateLightingConstantBuffer()
{
    D3D11_BUFFER_DESC bd{};
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    bd.CPUAccessFlags = 0;
    bd.ByteWidth = sizeof(CBLightingChangesEveryFrame);
    HRESULT hr = Device->CreateBuffer(&bd, nullptr, &pCBLightingChangesEveryFrame);

    return hr;
}

HRESULT DX11Renderer::Render(Camera* camera, float deltaTime)
{
    // todo put this into a function
    DeviceContext->ClearRenderTargetView(BackBufferView, new FLOAT[4]{0.0f, 0.0f, 0.0f, 0.0f});
    DeviceContext->ClearRenderTargetView(RT0View, new FLOAT[4]{0.1f, 0.1f, 0.1f, 0.0f});
    DeviceContext->ClearRenderTargetView(RT1View, new FLOAT[4]{0.0f, 0.0f, 0.0f, 0.0f});
    DeviceContext->ClearRenderTargetView(RT2View, new FLOAT[4]{0.0f, 0.0f, 0.0f, 0.0f});
    DeviceContext->ClearDepthStencilView(DepthStencilView, D3D11_CLEAR_DEPTH, 0.0f, 0);

    HRESULT hr = RenderGeometry(camera, deltaTime);
    if (FAILED(hr))
    {
        return hr;
    }
    RenderLightingPass(camera, deltaTime);

    if (UseSwapchain)
    {
        SwapChain->Present(0, 0);
    }
    else
    {
        if (DeviceContext != NULL)
        {
            DeviceContext->Flush();
        }
    }

    return S_OK;
}

void DX11Renderer::Cleanup()
{
    // if (DeviceContext)
    // {
    //     DeviceContext->ClearState();
    // }
    // Logger::Log("Cleaning up renderer");

    if (StaticMesh)
    {
        StaticMesh.reset();
    }
}

struct SVertexPositionColor
{
    XMFLOAT3 position;
    XMFLOAT4 color;
};

HRESULT DX11Renderer::RenderGeometry(Camera* camera, float deltaTime)
{
    ID3D11RenderTargetView* renderTargets[3] = {RT0View, RT1View, RT2View};
    DeviceContext->OMSetRenderTargets(3, renderTargets, DepthStencilView);

    static const XMVECTORF32 xAxis = {20.f, 0.f, 0.f, 0.f};
    static const XMVECTORF32 yAxis = {0.f, 0.f, 20.f, 0.f};
    // Debug::DrawGrid(xAxis, yAxis, XMVectorSet(0, 0, 0, 0), 20, 20, Colors::Gray);

    HRESULT hr = S_OK;
    if (StaticMesh)
    {
        hr = StaticMesh->Render(DeviceContext, camera, deltaTime);
    }

    // draw after so we can use the cb12 thats already bound for now
    Debug::DrawGrid(DeviceContext);

    return hr;
}

void DX11Renderer::RenderLightingPass(Camera* camera, float deltaTime)
{
    DeviceContext->OMSetRenderTargets(1, &BackBufferView, nullptr);

    DeviceContext->VSSetShader(LightingVertexShader, nullptr, 0);
    DeviceContext->IASetInputLayout(QuadVertexLayout);
    DeviceContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    ID3D11ShaderResourceView* RenderTargets[3] = {RT0SRV, RT1SRV, RT2SRV};
    DeviceContext->PSSetShaderResources(0, 3, RenderTargets);
    DeviceContext->PSSetShader(LightingPixelShader, nullptr, 0);

    UINT stride = sizeof(QuadVertex);
    UINT offset = 0;
    DeviceContext->IASetVertexBuffers(0, 1, &QuadVertexBuffer, &stride, &offset);
    DeviceContext->IASetIndexBuffer(QuadIndexBuffer, DXGI_FORMAT_R16_UINT, 0);

    CBLightingChangesEveryFrame cb1;
    static float t = 0.0f;
    t += (float) XM_PI * deltaTime * 0.1f;
    XMFLOAT4 vLightDirs[2] = {
        XMFLOAT4(-0.577f, 0.577f, -0.577f, 1.0f),
        XMFLOAT4(0.0f, 0.0f, -1.0f, 1.0f),
    };
    XMFLOAT4 vLightColors[2] = {XMFLOAT4(0.5f, 1.0f, 1.0f, 1.0f), XMFLOAT4(0.5f, 0.0f, 0.0f, 1.0f)};
    cb1.LightDir[0] = vLightDirs[0];
    cb1.LightDir[1] = vLightDirs[1];
    cb1.LightColor[0] = vLightColors[0];
    cb1.LightColor[1] = vLightColors[1];
    // DeviceContext->UpdateSubresource(pCBLightingChangesEveryFrame, 0, nullptr, &cb1, 0, 0);
    // DeviceContext->PSSetConstantBuffers(0, 1, &pCBLightingChangesEveryFrame);

    DeviceContext->DrawIndexed(6, 0, 0);

    ID3D11ShaderResourceView* pSRV[3] = {NULL, NULL, NULL};
    DeviceContext->PSSetShaderResources(0, 3, pSRV);
}

DX11Renderer::~DX11Renderer()
{
    // if (DeviceContext)
    //     DeviceContext->ClearState();
    //
    // // if( pCBNeverChanges ) pCBNeverChanges->Release();
    // if (pCBChangeOnResize)
    //     pCBChangeOnResize->Release();
    // if (pCBChangesEveryFrame)
    //     pCBChangesEveryFrame->Release();
    // if (TextureRV)
    //     TextureRV->Release();
    // if (SamplerLinear)
    //     SamplerLinear->Release();
    // if (VertexBuffer)
    //     VertexBuffer->Release();
    // if (IndexBuffer)
    //     IndexBuffer->Release();
    // if (VertexLayout)
    //     VertexLayout->Release();
    // if (VertexShader)
    //     VertexShader->Release();
    // if (PixelShader)
    //     PixelShader->Release();
    // if (PixelShaderSolid)
    //     PixelShaderSolid->Release();
    // if (DepthStencil)
    //     DepthStencil->Release();
    // if (DepthStencilView)
    //     DepthStencilView->Release();
    // if (BackBufferView)
    //     BackBufferView->Release();
    // if (SwapChain)
    //     SwapChain->Release();
    // if (DeviceContext)
    //     DeviceContext->Release();
    // if (Device)
    //     Device->Release();
}

HRESULT DX11Renderer::InitRenderTarget(void* pResource)
{
    DeviceContext->OMSetRenderTargets(0, NULL, NULL);

    HRESULT hr = S_OK;

    IUnknown* pUnk = (IUnknown*) pResource;

    IDXGIResource* pDXGIResource;
    hr = pUnk->QueryInterface(__uuidof(IDXGIResource), (void**) &pDXGIResource);
    if (FAILED(hr))
    {
        return hr;
    }

    HANDLE sharedHandle;
    hr = pDXGIResource->GetSharedHandle(&sharedHandle);
    if (FAILED(hr))
    {
        return hr;
    }

    pDXGIResource->Release();

    IUnknown* tempResource11;
    hr = Device->OpenSharedResource(sharedHandle, __uuidof(ID3D11Resource), (void**) (&tempResource11));
    if (FAILED(hr))
    {
        return hr;
    }

    ID3D11Texture2D* pOutputResource;
    hr = tempResource11->QueryInterface(__uuidof(ID3D11Texture2D), (void**) (&pOutputResource));
    if (FAILED(hr))
    {
        return hr;
    }
    tempResource11->Release();

    D3D11_RENDER_TARGET_VIEW_DESC rtDesc;
    // DXGI_FORMAT_R8G8B8A8_UNORM
    rtDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    rtDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2D;
    rtDesc.Texture2D.MipSlice = 0;

    hr = Device->CreateRenderTargetView(pOutputResource, &rtDesc, &BackBufferView);
    if (FAILED(hr))
    {
        return hr;
    }

    D3D11_TEXTURE2D_DESC outputResourceDesc;
    pOutputResource->GetDesc(&outputResourceDesc);
    if (outputResourceDesc.Width != window->GetRect().Width || outputResourceDesc.Height != window->GetRect().Height)
    {
        // this should never happen
        // todo add const back
        window->SetRect(outputResourceDesc.Width, outputResourceDesc.Height);
    }

    hr = SetRasterizerViewport();
    if (FAILED(hr))
    {
        return hr;
    }

    // this happens when we do the lighting pass
    DeviceContext->OMSetRenderTargets(1, &BackBufferView, NULL);

    if (NULL != pOutputResource)
    {
        pOutputResource->Release();
    }

    return hr;
}
