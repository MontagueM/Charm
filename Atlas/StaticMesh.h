#pragma once
#include "Entity.h"

class Camera;

struct Blob
{
    void* Data;
    int Size;
};

enum InputSemantic : int
{
    None,
    Position,
    Texcoord,
    Normal,
    Tangent,
    BlendIndices,
    BlendWeight,
    Colour
};

struct InputSignature
{
    InputSemantic Semantic;
    int SemanticIndex;
    int DxgiFormat;
    int BufferIndex;
};

struct PartMaterial
{
    Blob VSBytecode;
    Blob PSBytecode;
    InputSignature InputSignatures[8];
    Blob VSTextures[16];
    Blob PSTextures[16];
    Blob VSConstantBuffers[16];
    Blob PSConstantBuffers[16];
    Blob VSSamplers[8];
    Blob PSSamplers[8];
};

struct PartInfo
{
    uint32_t IndexOffset;
    uint32_t IndexCount;
    PartMaterial PartMaterial;
};

struct BufferGroup
{
    Blob IndexBuffer;
    Blob VertexBuffers[3];
    uint32_t IndexOffset;
};

class StaticMesh
{
public:
    explicit StaticMesh(uint32_t hash);
    ID3D11Device* Device;

    ID3D11Buffer* GetIndexBuffer() const;
    ID3D11Buffer* const* GetVertexBuffers() const;
    HRESULT Initialise(ID3D11Device* Device);
    HRESULT AddStaticMeshBufferGroup(const BufferGroup& bufferGroup);
    HRESULT AddPart(const PartInfo& partInfo);
    HRESULT Render(ID3D11DeviceContext* DeviceContext, Camera* Camera, float DeltaTime);
    ~StaticMesh() = default;

private:
    ID3D11Buffer* IndexBuffer = nullptr;
    std::vector<ID3D11Buffer*> VertexBuffers;
    std::vector<class Part> Parts;

    HRESULT CreateIndexBuffer(const Blob& indexBlob);
    HRESULT CreateVertexBuffers(const Blob vertexBufferBlobs[3]);
};

class Part
{
public:
    Part(const PartInfo& partInfo) : PartInfo(partInfo) {}

    ID3D11Device* Device;

    ID3D11VertexShader* GetVertexShader() const;
    ID3D11PixelShader* GetPixelShader() const;
    ID3D11InputLayout* GetVertexLayout() const;
    HRESULT Render(ID3D11DeviceContext* DeviceContext, Camera* Camera, float DeltaTime);
    HRESULT Initialise(ID3D11Device* device);

private:
    ID3D11InputLayout* VertexLayout = nullptr;
    ID3D11VertexShader* VertexShader = nullptr;
    ID3D11PixelShader* PixelShader = nullptr;
    std::vector<Resource<ID3D11Buffer>*> VSConstantBuffers;
    std::vector<Resource<ID3D11Buffer>*> PSConstantBuffers;
    std::vector<ID3D11ShaderResourceView*> TextureSRVs;
    std::vector<Resource<ID3D11SamplerState>*> SamplerStates;

    ID3D11Buffer* ViewBuffer = nullptr;

    PartInfo PartInfo;

    HRESULT CreateVertexShader(const Blob& shaderBlob);
    HRESULT CreatePixelShader(const Blob& shaderBlob);
    HRESULT CreateVertexLayout(const InputSignature inputSignatures[8], const Blob& shaderBlob);

    HRESULT CreateConstantBuffers(const Blob vsBuffers[16], const Blob psBuffers[16]);
    HRESULT CreateTextureResources(const Blob vsTextures[16], const Blob psTextures[16]);
    HRESULT CreateSamplers(const Blob vsSamplers[8], const Blob psSamplers[8]);
};
