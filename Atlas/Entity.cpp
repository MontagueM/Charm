#include "Entity.h"

#include "DDSTextureLoader.h"
#include "Renderer.h"

#include <fstream>
#include <iostream>

struct cb1_InstanceData
{
    XMVECTOR MeshTransform;
    XMVECTOR UVTransform;
    XMVECTOR InstanceTransformMatrices[];
};

struct cb12_View
{
    XMMATRIX View;    // cb12[0,1,2]
    // . . . eyeDirection.x
    // . . . eyeDirection.y
    // . . . eyeDirection.z
    XMVECTOR Unk04;
    XMVECTOR Unk05;
    XMVECTOR NegativeCameraDirection;    // cb12[6]
    XMVECTOR CameraPosition;             // cb12[7]
    XMVECTOR Target;                     // cb12[8], 0/1 is width/height, 2/3 is 1/width and 1/height
    XMVECTOR CameraPosition2;            // cb12[10]
};

Entity::Entity(LPCWSTR FileHash)
{
}

ID3D11VertexShader* Entity::GetVertexShader()
{
    return VertexShader;
}

ID3D11PixelShader* Entity::GetPixelShader()
{
    return PixelShader;
}

ID3D11InputLayout* Entity::GetVertexLayout() const
{
    return VertexLayout;
}

ID3D11Buffer* Entity::GetIndexBuffer() const
{
    return IndexBuffer;
}

ID3D11Buffer* const* Entity::GetVertexBuffers() const
{
    return VertexBuffers.data();
}

HRESULT Entity::Initialise(ID3D11Device* Device)
{
    HRESULT hr;
    hr = CreateVertexShader(Device);
    if (FAILED(hr))
        return hr;

    hr = CreateVertexLayout(Device);
    if (FAILED(hr))
        return hr;

    hr = CreatePixelShader(Device);
    if (FAILED(hr))
        return hr;

    hr = CreateVertexBuffers(Device);
    if (FAILED(hr))
        return hr;

    hr = CreateIndexBuffer(Device);
    if (FAILED(hr))
        return hr;

    hr = CreateConstantBuffers(Device);
    if (FAILED(hr))
        return hr;

    hr = CreateTextureResources(Device);
    if (FAILED(hr))
        return hr;

    return S_OK;
}

void Entity::Render(ID3D11DeviceContext* DeviceContext, Camera* Camera, float DeltaTime)
{
    DeviceContext->VSSetShader(GetVertexShader(), nullptr, 0);
    DeviceContext->IASetInputLayout(GetVertexLayout());

    DeviceContext->PSSetShader(GetPixelShader(), nullptr, 0);

    UINT strides[2] = {16, 4};
    UINT offsets[2] = {0, 0};
    DeviceContext->IASetVertexBuffers(0, 2, GetVertexBuffers(), strides, offsets);
    DeviceContext->IASetIndexBuffer(GetIndexBuffer(), DXGI_FORMAT_R16_UINT, 0);

    DeviceContext->PSSetShaderResources(0, TextureSRVs.size(), TextureSRVs.data());
    for (const auto& SamplerState : SamplerStates)
    {
        DeviceContext->PSSetSamplers(SamplerState->Slot, 1, &SamplerState->ResourcePointer);
    }

    cb12_View View;
    View.View = Camera->GetViewMatrix();
    View.View.r[0].m128_f32[3] = Camera->GetDirection().m128_f32[0];
    View.View.r[1].m128_f32[3] = Camera->GetDirection().m128_f32[1];
    View.View.r[2].m128_f32[3] = Camera->GetDirection().m128_f32[2];
    View.View.r[0].m128_f32[2] = 0;
    View.View.r[1].m128_f32[2] = 0;
    View.View.r[2].m128_f32[2] = 0;
    View.CameraPosition = Camera->GetPosition();

    // std::cout << "Camera Position: " << View.CameraPosition.m128_f32[0] << ", " << View.CameraPosition.m128_f32[1] << ", "
    //           << View.CameraPosition.m128_f32[2] << std::endl;
    // std::cout << "Camera Direction: " << Camera->GetDirection().m128_f32[0] << ", " << Camera->GetDirection().m128_f32[1] << ", "
    //           << Camera->GetDirection().m128_f32[2] << std::endl;

    // view matrix
    D3D11_BOX Box;
    Box.left = 0;
    Box.top = 0;
    Box.front = 0;
    Box.right = 0 + 16 * 3;
    Box.bottom = 1;
    Box.back = 1;
    DeviceContext->UpdateSubresource(ViewBuffer, 0, &Box, &View.View, 0, 0);

    // player position
    Box.left = 112;
    Box.top = 0;
    Box.front = 0;
    Box.right = 112 + 16;
    Box.bottom = 1;
    Box.back = 1;
    DeviceContext->UpdateSubresource(ViewBuffer, 0, &Box, &View.CameraPosition, 0, 0);

    for (const auto& ConstantBuffer : VSConstantBuffers)
    {
        if (ConstantBuffer->Slot == 12)
        {
            DeviceContext->CopyResource(ConstantBuffer->ResourcePointer, ViewBuffer);
        }
        DeviceContext->VSSetConstantBuffers(ConstantBuffer->Slot, 1, &ConstantBuffer->ResourcePointer);
    }
    for (const auto& ConstantBuffer : PSConstantBuffers)
    {
        DeviceContext->PSSetConstantBuffers(ConstantBuffer->Slot, 1, &ConstantBuffer->ResourcePointer);
    }

    // DeviceContext->DrawIndexed(1116, 0, 0);    // 11529 entire thing, 1116 first part lod0
    // DeviceContext->DrawIndexed(3606, 1116, 0);
    DeviceContext->DrawIndexed(1503, 5718, 0);
}

HRESULT Entity::CreateVertexShader(ID3D11Device* Device)
{
    // HRESULT hr = DX11Renderer::CreateShaderFromCompiledFile(L"6C24BB80/VS_CBEBD680.bin", &VertexShader, VertexShaderBlob, Device);
    HRESULT hr = DX11Renderer::CreateShaderFromCompiledFile(
        L"C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/VS_DC6BBA80.bin", &VertexShader,
        VertexShaderBlob, Device);
    // const HRESULT hr = DX11Renderer::CreateShaderFromHlslFile(L"Shaders/Main.hlsl", "VSPole", &VertexShader, VertexShaderBlob, Device);
    return hr;
}

HRESULT Entity::CreateVertexLayout(ID3D11Device* Device)
{
    D3D11_INPUT_ELEMENT_DESC layout[] = {
        {"POSITION", 0, DXGI_FORMAT_R16G16B16A16_SNORM, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0},
        {"TANGENT", 0, DXGI_FORMAT_R16G16B16A16_SNORM, 0, 8, D3D11_INPUT_PER_VERTEX_DATA, 0},
        {"TEXCOORD", 0, DXGI_FORMAT_R16G16_SNORM, 1, D3D11_APPEND_ALIGNED_ELEMENT, D3D11_INPUT_PER_VERTEX_DATA, 0},
    };
    UINT numElements = ARRAYSIZE(layout);

    HRESULT hr = Device->CreateInputLayout(
        layout, numElements, VertexShaderBlob->GetBufferPointer(), VertexShaderBlob->GetBufferSize(), &VertexLayout);
    VertexShaderBlob->Release();

    return hr;
}

HRESULT Entity::CreatePixelShader(ID3D11Device* Device)
{
    const HRESULT hr = DX11Renderer::CreateShaderFromCompiledFile(
        L"C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_1864BA80.bin", &PixelShader, Device);
    // const HRESULT hr = DX11Renderer::CreateShaderFromHlslFile(L"Shaders/Main.hlsl", "PSTexture", &PixelShader, Device);
    return hr;
}

HRESULT Entity::CreateIndexBuffer(ID3D11Device* Device)
{
    std::ifstream IndexBufferFile(
        "C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/IndexBuffer_CEFBBA80.bin",
        std::ios::in | std::ios::binary);
    if (!IndexBufferFile)
    {
        return ERROR_INTERNAL_ERROR;
    }
    IndexBufferFile.seekg(0, std::ios::end);
    const UINT FileLength = IndexBufferFile.tellg();
    IndexBufferFile.seekg(0, std::ios::beg);
    WORD* Indices = new WORD[FileLength / sizeof(WORD)];
    IndexBufferFile.read((char*) Indices, FileLength);

    D3D11_BUFFER_DESC bd{};
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = FileLength;
    bd.BindFlags = D3D11_BIND_INDEX_BUFFER;
    bd.CPUAccessFlags = 0;

    D3D11_SUBRESOURCE_DATA InitData = {};
    InitData.pSysMem = Indices;
    const HRESULT hr = Device->CreateBuffer(&bd, &InitData, &IndexBuffer);
    return hr;
}

HRESULT Entity::CreateVertexBuffers(ID3D11Device* Device)
{
    std::ifstream VertexBufferFile0(
        "C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/VertexBuffer0_D0FBBA80.bin",
        std::ios::in | std::ios::binary);
    if (!VertexBufferFile0)
    {
        return MK_E_CANTOPENFILE;
    }
    VertexBufferFile0.seekg(0, std::ios::end);
    const UINT FileLength0 = VertexBufferFile0.tellg();
    VertexBufferFile0.seekg(0, std::ios::beg);
    WORD* Vertices0 = new WORD[FileLength0 / sizeof(WORD)];
    VertexBufferFile0.read((char*) Vertices0, FileLength0);

    std::ifstream VertexBufferFile1(
        "C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/VertexBuffer1_D2FBBA80.bin",
        std::ios::in | std::ios::binary);
    if (!VertexBufferFile1)
    {
        return MK_E_CANTOPENFILE;
    }
    VertexBufferFile1.seekg(0, std::ios::end);
    const UINT FileLength1 = VertexBufferFile1.tellg();
    VertexBufferFile1.seekg(0, std::ios::beg);
    WORD* Vertices1 = new WORD[FileLength1 / sizeof(WORD)];
    VertexBufferFile1.read((char*) Vertices1, FileLength1);

    D3D11_BUFFER_DESC bd{};
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = FileLength0;
    bd.BindFlags = D3D11_BIND_VERTEX_BUFFER;
    bd.CPUAccessFlags = 0;

    D3D11_SUBRESOURCE_DATA InitData = {};
    InitData.pSysMem = Vertices0;
    ID3D11Buffer* VertexBuffer0;
    HRESULT hr = Device->CreateBuffer(&bd, &InitData, &VertexBuffer0);
    if (FAILED(hr))
        return hr;
    VertexBuffers.push_back(VertexBuffer0);

    bd.ByteWidth = FileLength1;
    InitData.pSysMem = Vertices1;
    ID3D11Buffer* VertexBuffer1;
    hr = Device->CreateBuffer(&bd, &InitData, &VertexBuffer1);
    if (FAILED(hr))
        return hr;
    VertexBuffers.push_back(VertexBuffer1);

    return S_OK;
}

HRESULT Entity::CreateConstantBuffers(ID3D11Device* Device)
{
    std::ifstream ConstantBufferFile1(
        "C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/VS_cb1.bin", std::ios::in | std::ios::binary);
    if (!ConstantBufferFile1)
    {
        return MK_E_CANTOPENFILE;
    }
    ConstantBufferFile1.seekg(0, std::ios::end);
    const UINT FileLength1 = ConstantBufferFile1.tellg();
    ConstantBufferFile1.seekg(0, std::ios::beg);
    BYTE* cb1 = new BYTE[FileLength1];
    ConstantBufferFile1.read((char*) cb1, FileLength1);

    D3D11_BUFFER_DESC bd{};
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = FileLength1;
    bd.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    bd.CPUAccessFlags = 0;

    D3D11_SUBRESOURCE_DATA InitData = {};
    InitData.pSysMem = cb1;

    ID3D11Buffer* ConstantBuffer1;
    HRESULT hr = Device->CreateBuffer(&bd, &InitData, &ConstantBuffer1);
    if (FAILED(hr))
        return hr;

    VSConstantBuffers.push_back(new Resource<ID3D11Buffer>(1, ConstantBuffer1));

    std::ifstream ConstantBufferFile12(
        "C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/VS_cb12.bin", std::ios::in | std::ios::binary);
    if (!ConstantBufferFile12)
    {
        return MK_E_CANTOPENFILE;
    }
    ConstantBufferFile12.seekg(0, std::ios::end);
    const UINT FileLength12 = ConstantBufferFile12.tellg();
    ConstantBufferFile12.seekg(0, std::ios::beg);
    BYTE* cb12 = new BYTE[FileLength1];
    ConstantBufferFile12.read((char*) cb12, FileLength12);

    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = FileLength1;
    bd.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    bd.CPUAccessFlags = 0;

    InitData.pSysMem = cb12;

    ID3D11Buffer* ConstantBuffer12;
    hr = Device->CreateBuffer(&bd, &InitData, &ConstantBuffer12);
    if (FAILED(hr))
        return hr;

    VSConstantBuffers.push_back(new Resource<ID3D11Buffer>(12, ConstantBuffer12));

    // to be able to update the VS, then we copy over to the resource
    bd.BindFlags = 0;
    hr = hr = Device->CreateBuffer(&bd, &InitData, &ViewBuffer);
    if (FAILED(hr))
        return hr;

    std::ifstream ConstantBufferFile0(
        "C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_cb0_75FFBA80.bin",
        std::ios::in | std::ios::binary);
    if (!ConstantBufferFile0)
    {
        return MK_E_CANTOPENFILE;
    }
    ConstantBufferFile0.seekg(0, std::ios::end);
    const UINT FileLength0 = ConstantBufferFile0.tellg();
    ConstantBufferFile0.seekg(0, std::ios::beg);
    BYTE* cb0 = new BYTE[FileLength0];
    ConstantBufferFile0.read((char*) cb0, FileLength0);

    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = FileLength0;
    bd.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    bd.CPUAccessFlags = 0;

    InitData.pSysMem = cb0;

    ID3D11Buffer* ConstantBuffer;
    hr = Device->CreateBuffer(&bd, &InitData, &ConstantBuffer);
    if (FAILED(hr))
        return hr;

    PSConstantBuffers.push_back(new Resource<ID3D11Buffer>(0, ConstantBuffer));

    std::ifstream PSConstantBufferFile12(
        "C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_cb12.bin", std::ios::in | std::ios::binary);
    if (!PSConstantBufferFile12)
    {
        return MK_E_CANTOPENFILE;
    }
    PSConstantBufferFile12.seekg(0, std::ios::end);
    const UINT PSFileLength12 = PSConstantBufferFile12.tellg();
    PSConstantBufferFile12.seekg(0, std::ios::beg);
    BYTE* PScb12 = new BYTE[PSFileLength12];
    PSConstantBufferFile12.read((char*) cb0, PSFileLength12);

    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = PSFileLength12;
    bd.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    bd.CPUAccessFlags = 0;

    InitData.pSysMem = PScb12;

    ID3D11Buffer* PSConstantBuffer12;
    hr = Device->CreateBuffer(&bd, &InitData, &PSConstantBuffer12);
    if (FAILED(hr))
        return hr;

    PSConstantBuffers.push_back(new Resource<ID3D11Buffer>(12, PSConstantBuffer12));

    return S_OK;
}

HRESULT Entity::CreateTextureResources(ID3D11Device* Device)
{
    const std::vector<LPCWSTR> TextureFiles = {
        L"C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_0_AFC5BC80.dds",
        L"C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_1_8848A380.dds",
        L"C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_2_53C7BC80.dds",
        L"C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_3_9742BD80.dds",
        L"C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_4_9342BD80.dds",
        L"C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_5_7DC5BC80.dds",
        L"C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_6_7D63BC80.dds",
        L"C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_7_9B42BD80.dds",
        L"C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/C325BB80/PS_8_85C5BC80.dds"};

    HRESULT hr = CreateTextureSRVsFromFiles(Device, TextureFiles, TextureSRVs);
    if (FAILED(hr))
        return hr;

    hr = CreateSamplers(Device);
    if (FAILED(hr))
        return hr;

    return S_OK;
}

HRESULT Entity::CreateSamplers(ID3D11Device* Device)
{
    D3D11_SAMPLER_DESC sampDesc = {};
    sampDesc.Filter = D3D11_FILTER_ANISOTROPIC;
    sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
    sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
    sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    sampDesc.ComparisonFunc = D3D11_COMPARISON_NEVER;
    sampDesc.MaxAnisotropy = 4;
    sampDesc.MinLOD = 0;
    sampDesc.MaxLOD = D3D11_FLOAT32_MAX;
    sampDesc.MipLODBias = 0;
    ID3D11SamplerState* SamplerState;
    HRESULT hr = Device->CreateSamplerState(&sampDesc, &SamplerState);
    if (FAILED(hr))
        return hr;
    SamplerStates.push_back(new Resource<ID3D11SamplerState>(1, SamplerState));

    sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
    hr = Device->CreateSamplerState(&sampDesc, &SamplerState);
    if (FAILED(hr))
        return hr;
    SamplerStates.push_back(new Resource<ID3D11SamplerState>(2, SamplerState));

    sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
    sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
    sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    hr = Device->CreateSamplerState(&sampDesc, &SamplerState);
    if (FAILED(hr))
        return hr;
    SamplerStates.push_back(new Resource<ID3D11SamplerState>(3, SamplerState));

    return S_OK;
}
