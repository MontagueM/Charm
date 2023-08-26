#pragma once
#include "Camera.h"
#include "NativeWindow.h"

#include <d3d11.h>

#include <memory>

class StaticMesh;
using namespace DirectX;

enum ShaderType
{
    Vertex,
    Pixel
};

class IRenderer
{
public:
    virtual ~IRenderer() = default;
    virtual void Render(Camera* camera) = 0;
};

class DX11Renderer
{
public:
    std::shared_ptr<StaticMesh> StaticMesh;

    DX11Renderer(bool useSwapchain = true);
    ~DX11Renderer();
    HRESULT InitRenderTarget(void* pResource);
    HRESULT Initialise();
    void Render(Camera* camera, float deltaTime);

    static HRESULT CreateShaderFromCompiledFile(
        LPCWSTR FileName, ID3D11VertexShader** VertexShader, ID3D10Blob*& ShaderBlob, ID3D11Device* Device);
    static HRESULT CreateShaderFromCompiledFile(LPCWSTR FileName, ID3D11PixelShader** PixelShader, ID3D11Device* Device);
    static HRESULT CreateShaderFromHlslFile(
        LPCWSTR FileName, LPCSTR EntryPoint, ID3D11VertexShader** VertexShader, ID3D10Blob*& ShaderBlob, ID3D11Device* Device);
    static HRESULT CreateShaderFromHlslFile(LPCWSTR FileName, LPCSTR EntryPoint, ID3D11PixelShader** PixelShader, ID3D11Device* Device);
    static HRESULT CompileShaderFromFile(LPCWSTR FileName, LPCSTR EntryPoint, LPCSTR ShaderModel, ID3DBlob** ShaderBlob);

private:
    std::shared_ptr<RenderPanel> window;

    XMMATRIX World1;
    XMMATRIX World2;
    XMMATRIX View;
    XMMATRIX Projection;
    ID3D11InputLayout* GeometryVertexLayout;
    ID3D11Buffer* GeometryIndexBuffer;
    ID3D11Buffer* GeometryVertexBuffer0;
    ID3D11Buffer* GeometryVertexBuffer1;
    ID3D11VertexShader* GeometryVertexShader;
    ID3D11PixelShader* GeometryPixelShader;
    ID3D11PixelShader* GeometryPixelShaderSolid;

    ID3D11VertexShader* LightingVertexShader;
    ID3D11PixelShader* LightingPixelShader;
    ID3D11InputLayout* QuadVertexLayout;
    ID3D11Buffer* QuadIndexBuffer;
    ID3D11Buffer* QuadVertexBuffer;

    // ID3D11Buffer* pCBNeverChanges = nullptr;
    ID3D11Buffer* pCBChangeOnResize = nullptr;
    ID3D11Buffer* pScopeView = nullptr;
    ID3D11Buffer* pCBLightingChangesEveryFrame = nullptr;

    ID3D11ShaderResourceView* TextureRV = nullptr;
    ID3D11SamplerState* SamplerLinear = nullptr;

    IDXGISwapChain* SwapChain;

public:
    ID3D11Device* Device;

    HRESULT InitialiseGeneral(std::shared_ptr<RenderPanel> window);
    HRESULT SetRasterizerViewport();

private:
    ID3D11DeviceContext* DeviceContext;
    ID3D11Texture2D* RT0Texture;
    ID3D11Texture2D* RT1Texture;
    ID3D11Texture2D* RT2Texture;
    ID3D11RenderTargetView* RT0View;
    ID3D11RenderTargetView* RT1View;
    ID3D11RenderTargetView* RT2View;
    ID3D11ShaderResourceView* RT0SRV = nullptr;
    ID3D11ShaderResourceView* RT1SRV = nullptr;
    ID3D11ShaderResourceView* RT2SRV = nullptr;
    ID3D11RenderTargetView* BackBufferView;
    ID3D11Texture2D* DepthStencil;
    ID3D11DepthStencilView* DepthStencilView;

    std::shared_ptr<class Entity> CarEntity;

    bool UseSwapchain;

    HRESULT CreateDeviceAndSwapChain();
    HRESULT CreateDevice();
    HRESULT CreateBackBufferView();

    HRESULT InitialiseGeometryPass();
    HRESULT CreateDepthStencilView();
    HRESULT CreateGeometryVertexShader(ID3D10Blob*& VertexShaderBlob);
    HRESULT CreateGeometryPixelShader();
    HRESULT CreateGeometryVertexLayout(ID3D10Blob* VertexShaderBlob);
    HRESULT CreateGeometryVertexBuffer();
    HRESULT CreateGeometryIndexBuffer();
    HRESULT CreateGeometryConstantBuffers();
    HRESULT CreateGeometryTexture();
    HRESULT CreateGeometrySampler();

    HRESULT InitialiseLightingPass();
    HRESULT CreateLightingRenderTargets();
    HRESULT CreateLightingVertexShader(ID3D10Blob*& VertexShaderBlob);
    HRESULT CreateLightingPixelShader();
    HRESULT CreateLightingVertexLayout(ID3D10Blob* VertexShaderBlob);
    HRESULT CreateLightingVertexBuffer();
    HRESULT CreateLightingIndexBuffer();
    HRESULT CreateLightingConstantBuffer();
    void RenderGeometry(Camera* camera, float deltaTime);
    void RenderLightingPass(Camera* camera, float deltaTime);
};
