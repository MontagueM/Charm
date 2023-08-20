#pragma once
#include "DDSTextureLoader.h"
#include "Entity.h"

#include <vector>

class Camera;

class DebugCube
{
public:
    explicit DebugCube();

    ID3D11VertexShader* GetVertexShader();
    ID3D11PixelShader* GetPixelShader();
    ID3D11InputLayout* GetVertexLayout() const;
    ID3D11Buffer* GetIndexBuffer() const;
    ID3D11Buffer* const* GetVertexBuffers() const;
    HRESULT Initialise(ID3D11Device* Device);
    void Render(ID3D11DeviceContext* DeviceContext, Camera* Camera, float DeltaTime);
    ~DebugCube() = default;

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
