#pragma once
#include "Entity.h"

class Camera;

struct Blob
{
    void* Data;
    int Size;

    bool IsInvalid() const { return Data == nullptr || Size == 0; }
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
    Blob PScb0;
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

struct Vector4
{
    float X;
    float Y;
    float Z;
    float W;
};

class StaticMesh
{
public:
    explicit StaticMesh(uint32_t hash, const Blob& staticMeshTransforms) : StaticMeshTransforms(staticMeshTransforms){};
    ~StaticMesh();
    ID3D11Device* Device;
    Blob StaticMeshTransforms;

    ID3D11Buffer* GetIndexBuffer() const;
    ID3D11Buffer* const* GetVertexBuffers() const;
    HRESULT Initialise(ID3D11Device* Device);
    HRESULT AddStaticMeshBufferGroup(const BufferGroup& bufferGroup);
    HRESULT AddPart(const PartInfo& partInfo);
    HRESULT Render(ID3D11DeviceContext* DeviceContext, Camera* Camera, float DeltaTime);

private:
    ID3D11Buffer* IndexBuffer = nullptr;
    std::vector<ID3D11Buffer*> VertexBuffers;
    std::vector<std::unique_ptr<class Part>> Parts;

    HRESULT CreateIndexBuffer(const Blob& indexBlob);
    HRESULT CreateVertexBuffers(const Blob vertexBufferBlobs[3]);
};

class Part
{
public:
    Part(StaticMesh* staticMesh, const PartInfo& partInfo) : Parent(staticMesh), PartInfo(partInfo) {}
    ~Part();

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
    std::vector<ID3D11ShaderResourceView*> VSTextureSRVs;
    std::vector<ID3D11ShaderResourceView*> PSTextureSRVs;
    std::vector<Resource<ID3D11SamplerState>*> VSSamplerStates;
    std::vector<Resource<ID3D11SamplerState>*> PSSamplerStates;

    StaticMesh* Parent = nullptr;

    ID3D11Buffer* ViewBuffer = nullptr;

    PartInfo PartInfo;

    HRESULT CreateVertexShader(const Blob& shaderBlob);
    HRESULT CreatePixelShader(const Blob& shaderBlob);
    HRESULT CreateVertexLayout(const InputSignature inputSignatures[8], const Blob& shaderBlob);

    HRESULT CreateConstantBuffers(const Blob& psCb0);
    HRESULT CreateTextureResources(const Blob vsTextures[16], const Blob psTextures[16]);
    HRESULT CreateSamplers(const Blob vsSamplers[16], const Blob psSamplers[16]);
};
