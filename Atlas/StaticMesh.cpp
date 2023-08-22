#include "StaticMesh.h"

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
    XMMATRIX WorldToProjective;     // cb12[0-3]
    XMMATRIX CameraToWorld;         // cb12[4-7]
    XMVECTOR ViewportDimensions;    // cb12[8], 0/1 is width/height, 2/3 is 1/width and 1/height
    // XMVECTOR CameraPosition;        // cb12[10]
};

ID3D11Buffer* StaticMesh::GetIndexBuffer() const
{
    return IndexBuffer;
}

ID3D11Buffer* const* StaticMesh::GetVertexBuffers() const
{
    return VertexBuffers.data();
}

HRESULT StaticMesh::Initialise(ID3D11Device* Device)
{
    this->Device = Device;

    return S_OK;
}

HRESULT StaticMesh::AddStaticMeshBufferGroup(const BufferGroup& bufferGroup)
{
    HRESULT hr = S_OK;

    hr = CreateVertexBuffers(bufferGroup.VertexBuffers);
    if (FAILED(hr))
        return hr;

    hr = CreateIndexBuffer(bufferGroup.IndexBuffer);
    if (FAILED(hr))
        return hr;

    return hr;
}

HRESULT StaticMesh::Render(ID3D11DeviceContext* DeviceContext, Camera* Camera, float DeltaTime)
{
    if (GetVertexBuffers() == nullptr || GetIndexBuffer() == nullptr)
    {
        return E_FAIL;
    }

    UINT strides[2] = {16, 4};
    UINT offsets[2] = {0, 0};
    DeviceContext->IASetVertexBuffers(0, 2, GetVertexBuffers(), strides, offsets);
    DeviceContext->IASetIndexBuffer(GetIndexBuffer(), DXGI_FORMAT_R16_UINT, 0);
    DeviceContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    // DeviceContext->DrawIndexed(1116, 0, 0);    // 11529 entire thing, 1116 first part lod0
    // DeviceContext->DrawIndexed(3606, 1116, 0);
    // DeviceContext->DrawIndexed(1503, 5718, 0);

    HRESULT hr = S_OK;
    for (auto& part : Parts)
    {
        if (FAILED(hr = part.Render(DeviceContext, Camera, DeltaTime)))
        {
            return hr;
        }
    }

    return S_OK;
}

HRESULT StaticMesh::CreateIndexBuffer(const Blob& indexBlob)
{
    byte* IndexData = new byte[indexBlob.Size];
    memcpy(IndexData, indexBlob.Data, indexBlob.Size);

    D3D11_BUFFER_DESC bd{};
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = indexBlob.Size;
    bd.BindFlags = D3D11_BIND_INDEX_BUFFER;
    bd.CPUAccessFlags = 0;

    D3D11_SUBRESOURCE_DATA InitData = {};
    InitData.pSysMem = std::move(IndexData);
    const HRESULT hr = Device->CreateBuffer(&bd, &InitData, &IndexBuffer);
    return hr;
}

HRESULT StaticMesh::CreateVertexBuffers(const Blob vertexBufferBlobs[3])
{
    D3D11_BUFFER_DESC bd{};
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.BindFlags = D3D11_BIND_VERTEX_BUFFER;
    bd.CPUAccessFlags = 0;

    for (int i = 0; i < 3; i++)
    {
        const Blob& VertexBlob = vertexBufferBlobs[i];

        // copy the memory so we can free the GCHandle in c#
        byte* VertexData = new byte[VertexBlob.Size];
        memcpy(VertexData, VertexBlob.Data, VertexBlob.Size);

        bd.ByteWidth = VertexBlob.Size;

        D3D11_SUBRESOURCE_DATA InitData = {};
        // actually we probably dont need manualy copy as this should copy too? or maybe not idk
        // idk what assignment operator does to a byte array
        InitData.pSysMem = std::move(VertexData);
        ID3D11Buffer* VertexBuffer;
        HRESULT hr = Device->CreateBuffer(&bd, &InitData, &VertexBuffer);
        if (FAILED(hr))
            return hr;
        VertexBuffers.push_back(VertexBuffer);
    }

    return S_OK;
}

HRESULT Part::CreateConstantBuffers(const Blob vsBuffers[16], const Blob psBuffers[16])
{
    byte cb1[0x500];

    memcpy(cb1, Parent->StaticMeshTransforms.Data, Parent->StaticMeshTransforms.Size);

    const XMMATRIX transformMatrix = XMMatrixIdentity();
    memcpy(cb1 + 0x20, &transformMatrix, sizeof(transformMatrix));

    D3D11_BUFFER_DESC bd{};
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = 0x500;
    bd.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    bd.CPUAccessFlags = 0;

    D3D11_SUBRESOURCE_DATA InitData = {};
    InitData.pSysMem = cb1;

    ID3D11Buffer* ConstantBuffer1;
    HRESULT hr = Device->CreateBuffer(&bd, &InitData, &ConstantBuffer1);
    if (FAILED(hr))
        return hr;

    VSConstantBuffers.push_back(new Resource(1, ConstantBuffer1));

    std::ifstream ConstantBufferFile12(
        "C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/6C24BB80/VS_cb12.bin", std::ios::in | std::ios::binary);
    if (!ConstantBufferFile12)
    {
        return MK_E_CANTOPENFILE;
    }
    ConstantBufferFile12.seekg(0, std::ios::end);
    const UINT FileLength12 = ConstantBufferFile12.tellg();
    ConstantBufferFile12.seekg(0, std::ios::beg);
    BYTE* cb12 = new BYTE[FileLength12];
    ConstantBufferFile12.read((char*) cb12, FileLength12);

    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = FileLength12;
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
        "C:/Users/monta/Desktop/Projects/Charm/Charm/bin/x64/Debug/net7.0-windows/6C24BB80/PS_cb0_03FBBA80.bin",
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

HRESULT Part::CreateTextureResources(const Blob vsTextures[16], const Blob psTextures[16])
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

    HRESULT hr = S_OK;
    for (int i = 0; i < 16; i++)
    {
        const Blob& vsTexture = vsTextures[i];
        if (vsTexture.Size == 0)
        {
            continue;
        }

        ID3D11ShaderResourceView* TextureSRV;
        if (FAILED(
                hr = CreateDDSTextureFromMemory(Device, static_cast<const uint8_t*>(vsTexture.Data), vsTexture.Size, nullptr, &TextureSRV)))
        {
            return hr;
        }
        VSTextureSRVs.push_back(TextureSRV);
    }
    for (int i = 0; i < 16; i++)
    {
        const Blob& psTexture = psTextures[i];
        if (psTexture.Size == 0)
        {
            continue;
        }

        ID3D11ShaderResourceView* TextureSRV;
        if (FAILED(
                hr = CreateDDSTextureFromMemory(Device, static_cast<const uint8_t*>(psTexture.Data), psTexture.Size, nullptr, &TextureSRV)))
        {
            return hr;
        }
        PSTextureSRVs.push_back(TextureSRV);
    }

    return S_OK;
}

HRESULT Part::CreateSamplers(const Blob vsSamplers[8], const Blob psSamplers[8])
{
    HRESULT hr = S_OK;

    for (int i = 0; i < 8; i++)
    {
        const Blob& vsSampler = vsSamplers[i];
        if (vsSampler.IsInvalid())
        {
            continue;
        }

        D3D11_SAMPLER_DESC sampDesc = *static_cast<D3D11_SAMPLER_DESC*>(vsSampler.Data);
        ID3D11SamplerState* samplerState;
        hr = Device->CreateSamplerState(&sampDesc, &samplerState);
        if (FAILED(hr))
            return hr;

        VSSamplerStates.push_back(new Resource(i, samplerState));
    }

    for (int i = 0; i < 8; i++)
    {
        const Blob& psSampler = psSamplers[i];
        if (psSampler.IsInvalid())
        {
            continue;
        }

        D3D11_SAMPLER_DESC sampDesc = *static_cast<D3D11_SAMPLER_DESC*>(psSampler.Data);
        ID3D11SamplerState* samplerState;
        hr = Device->CreateSamplerState(&sampDesc, &samplerState);
        if (FAILED(hr))
            return hr;

        PSSamplerStates.push_back(new Resource(i, samplerState));
    }

    return hr;
}

ID3D11VertexShader* Part::GetVertexShader() const
{
    return VertexShader;
}

ID3D11PixelShader* Part::GetPixelShader() const
{
    return PixelShader;
}

ID3D11InputLayout* Part::GetVertexLayout() const
{
    return VertexLayout;
}

static XMMATRIX CreatePerspectiveInfiniteReverseRH(const float fov, const float aspectRatio, const float zNear)
{
    assert(zNear > 0);
    const float yScale = 1.0f / tan(fov * 0.5);
    return {yScale / aspectRatio, 0, 0, 0, 0, yScale, 0, 0, 0, 0, 0, -1, 0, 0, zNear, 0};
}

HRESULT Part::Render(ID3D11DeviceContext* DeviceContext, Camera* Camera, float DeltaTime)
{
    if (GetVertexShader() == nullptr || GetPixelShader() == nullptr || GetVertexLayout() == nullptr)
    {
        return E_FAIL;
    }

    DeviceContext->VSSetShader(GetVertexShader(), nullptr, 0);
    DeviceContext->VSSetShaderResources(0, VSTextureSRVs.size(), VSTextureSRVs.data());
    for (const auto& SamplerState : VSSamplerStates)
    {
        DeviceContext->VSSetSamplers(SamplerState->Slot, 1, &SamplerState->ResourcePointer);
    }

    DeviceContext->PSSetShader(GetPixelShader(), nullptr, 0);
    DeviceContext->PSSetShaderResources(0, PSTextureSRVs.size(), PSTextureSRVs.data());
    for (const auto& SamplerState : PSSamplerStates)
    {
        DeviceContext->PSSetSamplers(SamplerState->Slot, 1, &SamplerState->ResourcePointer);
    }

    DeviceContext->IASetInputLayout(GetVertexLayout());

    cb12_View View;

    auto projection = CreatePerspectiveInfiniteReverseRH(Camera->GetFOVRadians(), Camera->GetAspectRatio(), 0.01f);

    View.WorldToProjective = Camera->GetViewMatrix() * projection;
    View.CameraToWorld = XMMatrixIdentity();
    View.CameraToWorld.r[3].m128_f32[0] = Camera->GetPosition().m128_f32[0];
    View.CameraToWorld.r[3].m128_f32[1] = Camera->GetPosition().m128_f32[1];
    View.CameraToWorld.r[3].m128_f32[2] = Camera->GetPosition().m128_f32[2];
    View.CameraToWorld.r[3].m128_f32[3] = 1.0f;

    // std::cout << "Camera Position: " << View.CameraPosition.m128_f32[0] << ", " << View.CameraPosition.m128_f32[1] << ", "
    //           << View.CameraPosition.m128_f32[2] << std::endl;
    // std::cout << "Camera Direction: " << Camera->GetDirection().m128_f32[0] << ", " << Camera->GetDirection().m128_f32[1] << ", "
    //           << Camera->GetDirection().m128_f32[2] << std::endl;

    // view matrix
    D3D11_BOX Box;
    Box.left = 0;
    Box.top = 0;
    Box.front = 0;
    Box.right = 0 + 16 * 8;    // 2 matrices = 8 vectors
    Box.bottom = 1;
    Box.back = 1;
    DeviceContext->UpdateSubresource(ViewBuffer, 0, &Box, &View, 0, 0);

    // // player position
    // Box.left = 112;
    // Box.top = 0;
    // Box.front = 0;
    // Box.right = 112 + 16;
    // Box.bottom = 1;
    // Box.back = 1;
    // DeviceContext->UpdateSubresource(ViewBuffer, 0, &Box, &View.CameraPosition, 0, 0);

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

    DeviceContext->DrawIndexed(PartInfo.IndexCount, PartInfo.IndexOffset, 0);
}

HRESULT Part::Initialise(ID3D11Device* device)
{
    Device = device;

    HRESULT hr;
    hr = CreateVertexShader(PartInfo.PartMaterial.VSBytecode);
    if (FAILED(hr))
        return hr;

    hr = CreatePixelShader(PartInfo.PartMaterial.PSBytecode);
    if (FAILED(hr))
        return hr;

    hr = CreateVertexLayout(PartInfo.PartMaterial.InputSignatures, PartInfo.PartMaterial.VSBytecode);
    if (FAILED(hr))
        return hr;

    hr = CreateConstantBuffers(PartInfo.PartMaterial.VSConstantBuffers, PartInfo.PartMaterial.VSConstantBuffers);
    if (FAILED(hr))
        return hr;

    hr = CreateTextureResources(PartInfo.PartMaterial.VSTextures, PartInfo.PartMaterial.PSTextures);
    if (FAILED(hr))
        return hr;

    hr = CreateSamplers(PartInfo.PartMaterial.VSSamplers, PartInfo.PartMaterial.PSSamplers);
    if (FAILED(hr))
        return hr;

    return hr;
}

HRESULT Part::CreateVertexShader(const Blob& shaderBlob)
{
    // does this copy? want to know if its safe or not
    HRESULT hr = Device->CreateVertexShader(shaderBlob.Data, shaderBlob.Size, nullptr, &VertexShader);
    return hr;
}

HRESULT Part::CreatePixelShader(const Blob& shaderBlob)
{
    HRESULT hr = Device->CreatePixelShader(shaderBlob.Data, shaderBlob.Size, nullptr, &PixelShader);
    return hr;
}

static LPCSTR GetSemanticName(InputSemantic semantic)
{
    switch (semantic)
    {
        case Position:
            return "POSITION";
        case Normal:
            return "NORMAL";
        case Tangent:
            return "TANGENT";
        case Colour:
            return "COLOR";
        case Texcoord:
            return "TEXCOORD";
        case BlendIndices:
            return "BLENDINDICES";
        case BlendWeight:
            return "BLENDWEIGHT";
        default:
            return "";
    }
}

HRESULT Part::CreateVertexLayout(const InputSignature inputSignatures[8], const Blob& shaderBlob)
{
    D3D11_INPUT_ELEMENT_DESC layout[8];

    int numElements = 0;

    for (int i = 0; i < 8; i++)
    {
        if (inputSignatures[i].Semantic == InputSemantic::None)
        {
            continue;
        }

        numElements++;
        layout[i] = {};
        layout[i].SemanticName = GetSemanticName(inputSignatures[i].Semantic);
        layout[i].SemanticIndex = inputSignatures[i].SemanticIndex;
        layout[i].Format = (DXGI_FORMAT) inputSignatures[i].DxgiFormat;
        layout[i].InputSlot = inputSignatures[i].BufferIndex;
        layout[i].InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA;
        layout[i].InstanceDataStepRate = 0;
        layout[i].AlignedByteOffset = D3D11_APPEND_ALIGNED_ELEMENT;
    }

    HRESULT hr = Device->CreateInputLayout(layout, numElements, shaderBlob.Data, shaderBlob.Size, &VertexLayout);
    // VertexShaderBlob->Release();

    return hr;
}

HRESULT StaticMesh::AddPart(const PartInfo& partInfo)
{
    Part part(this, partInfo);
    HRESULT hr = part.Initialise(Device);
    if (FAILED(hr))
    {
        return hr;
    }
    Parts.push_back(std::move(part));

    return hr;
}
