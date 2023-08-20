#pragma once

#include "Camera.h"
#include "DDSTextureLoader.h"

#include <d3d11.h>

#include <memory>
#include <vector>

template <class T>
struct Resource
{
    int Slot;
    T* ResourcePointer;
};

struct Texture
{
    LPCWSTR FileName;
};

namespace DirectX
{
inline HRESULT CreateTextureSRVsFromFiles(
    ID3D11Device* Device, std::vector<LPCWSTR> FileNames, std::vector<ID3D11ShaderResourceView*>& TextureSRVs)
{
    std::vector<ID3D11ShaderResourceView*> Output;
    for (const auto& FileName : FileNames)
    {
        ID3D11ShaderResourceView* TextureSRV;
        HRESULT hr = CreateDDSTextureFromFile(Device, FileName, nullptr, &TextureSRV);
        if (FAILED(hr))
            return hr;
        Output.push_back(TextureSRV);
    }

    TextureSRVs = Output;
    return S_OK;
}
}    // namespace DirectX

class Entity
{
public:
    explicit Entity(LPCWSTR FileHash);

    ID3D11VertexShader* GetVertexShader();
    ID3D11PixelShader* GetPixelShader();
    ID3D11InputLayout* GetVertexLayout() const;
    ID3D11Buffer* GetIndexBuffer() const;
    ID3D11Buffer* const* GetVertexBuffers() const;
    HRESULT Initialise(ID3D11Device* Device);
    void Render(ID3D11DeviceContext* DeviceContext, Camera* Camera, float DeltaTime);
    ~Entity() = default;

private:
    ID3D11InputLayout* VertexLayout = nullptr;
    ID3D11Buffer* IndexBuffer = nullptr;
    std::vector<ID3D11Buffer*> VertexBuffers;
    ID3D10Blob* VertexShaderBlob = nullptr;
    ID3D11VertexShader* VertexShader = nullptr;
    ID3D11PixelShader* PixelShader = nullptr;
    std::vector<Resource<ID3D11Buffer>*> VSConstantBuffers;
    std::vector<Resource<ID3D11Buffer>*> PSConstantBuffers;
    std::vector<ID3D11ShaderResourceView*> TextureSRVs;
    std::vector<Resource<ID3D11SamplerState>*> SamplerStates;

    ID3D11Buffer* ViewBuffer = nullptr;

    HRESULT CreateVertexShader(ID3D11Device* Device);
    HRESULT CreateVertexLayout(ID3D11Device* Device);
    HRESULT CreatePixelShader(ID3D11Device* Device);
    HRESULT CreateIndexBuffer(ID3D11Device* Device);
    HRESULT CreateVertexBuffers(ID3D11Device* Device);
    HRESULT CreateConstantBuffers(ID3D11Device* Device);
    HRESULT CreateTextureResources(ID3D11Device* Device);
    HRESULT CreateSamplers(ID3D11Device* Device);
};
