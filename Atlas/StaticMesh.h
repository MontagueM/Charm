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
    Blob VSSamplers[16];
    Blob PSSamplers[16];
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

    ID3D11VertexShader* GetVertexShader();
    ID3D11PixelShader* GetPixelShader();
    ID3D11InputLayout* GetVertexLayout() const;
    ID3D11Buffer* GetIndexBuffer() const;
    ID3D11Buffer* const* GetVertexBuffers() const;
    HRESULT Initialise(ID3D11Device* Device);
    HRESULT AddStaticMeshBufferGroup(const BufferGroup& bufferGroup);
    void CreateStaticMeshPart(const int partIndex, const PartInfo& partInfo);
    HRESULT Render(ID3D11DeviceContext* DeviceContext, Camera* Camera, float DeltaTime);
    ~StaticMesh() = default;

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

    HRESULT CreateIndexBuffer(const Blob& indexBlob);
    HRESULT CreateVertexBuffers(const Blob vertexBufferBlobs[3]);

    HRESULT CreateVertexShader(ID3D11Device* Device);
    HRESULT CreateVertexLayout(ID3D11Device* Device);
    HRESULT CreatePixelShader(ID3D11Device* Device);

    HRESULT CreateConstantBuffers(ID3D11Device* Device);
    HRESULT CreateTextureResources(ID3D11Device* Device);
    HRESULT CreateSamplers(ID3D11Device* Device);
};
