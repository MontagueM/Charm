#include "Debug.h"

#include "Logger.h"
#include "Renderer.h"

struct LineVertex
{
    float x, y, z;
    SimpleMath::Color colour;
};

// clang-format off
// LineVertex Vertices[] =
//     {
//     {-1, -1, 0},
//     {1, -1, 0},
//
//     {1, -1, 0},
//     {1, 1, 0},
//
//     {1, 1, 0},
//     {1, -1, 0},
//
//     {1, -1, 0},
//     {-1, -1, 0},
//
//     {0, -1, 0},
//     {0, 1, 0},
//
//     {-1, 0, 0},
//     {1, 0, 0},
//     };
// clang-format on

void Debug::DrawGrid(ID3D11DeviceContext* deviceContext)
{
    if (!bInitialised)
    {
        Logger::Log("Cannot debug render as has not been initialised");
        return;
    }

    SetupRender(deviceContext);

    deviceContext->Draw(NumVertices, 0);
}

HRESULT Debug::Initialise(ID3D11Device* device)
{
    HRESULT hr = S_OK;
    ID3D10Blob* VertexShaderBlob = nullptr;
    hr = DX11Renderer::CreateShaderFromHlslFile(L"Shaders/Debug.hlsl", "VS", &VertexShader, VertexShaderBlob, device);
    if (FAILED(hr))
    {
        Logger::Log("Failed to create debug vertex shader");
        return hr;
    }
    hr = DX11Renderer::CreateShaderFromHlslFile(L"Shaders/Debug.hlsl", "PS", &PixelShader, device);
    if (FAILED(hr))
    {
        Logger::Log("Failed to create debug pixel shader");
        return hr;
    }

    D3D11_INPUT_ELEMENT_DESC layout[] = {
        {"POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0},
        {"COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT, D3D11_INPUT_PER_VERTEX_DATA, 0},
    };
    UINT numElements = ARRAYSIZE(layout);

    hr = device->CreateInputLayout(
        layout, numElements, VertexShaderBlob->GetBufferPointer(), VertexShaderBlob->GetBufferSize(), &InputLayout);
    if (FAILED(hr))
    {
        Logger::Log("Failed to create debug input layout");
        return hr;
    }

    VertexShaderBlob->Release();

    // define grid size
    constexpr float gridLengthX = 5.f, gridLengthY = 5.f;
    constexpr float macroDivisionsSpacing = 0.5f;    // inclusive
    constexpr float microDivisionsSpacing = 0.1f;

    // For simplicity, we draw each line in each axis. There's duplicates but doesn't really matter that much.
    // constexpr int numLines = (gridLengthX + gridLengthY) * (macroDivisionsSpacing + numMicroDivisionsPerMetre);
    std::vector<LineVertex> vertices;
    // for every line, we need two vertex definitions
    // vertices.reserve(numLines * 2);

    constexpr int numLinesMacroX = gridLengthX * 2 / macroDivisionsSpacing;
    for (int i = 0; i < numLinesMacroX; i++)
    {
        const float x = -gridLengthX + i * macroDivisionsSpacing;
        constexpr float c = 0.7f;
        vertices.push_back({x, -gridLengthY, 0, {c, c, c, 1.f}});
        vertices.push_back({x, gridLengthY, 0, {c, c, c, 1.f}});
    }

    constexpr int numLinesMacroY = gridLengthY * 2 / macroDivisionsSpacing;
    for (int i = 0; i < numLinesMacroY; i++)
    {
        const float y = -gridLengthY + i * macroDivisionsSpacing;
        constexpr float c = 0.7f;
        vertices.push_back({-gridLengthX, y, 0, {c, c, c, 1.f}});
        vertices.push_back({gridLengthX, y, 0, {c, c, c, 1.f}});
    }

    constexpr int numLinesMicroX = gridLengthX * 2 / microDivisionsSpacing;
    for (int i = 0; i < numLinesMicroX; i++)
    {
        const float x = -gridLengthX + i * microDivisionsSpacing;
        constexpr float c = 0.4f;
        vertices.push_back({x, -gridLengthY, 0, {c, c, c, 1.f}});
        vertices.push_back({x, gridLengthY, 0, {c, c, c, 1.f}});
    }

    constexpr int numLinesMicroY = gridLengthY * 2 / microDivisionsSpacing;
    for (int i = 0; i < numLinesMicroY; i++)
    {
        const float y = -gridLengthY + i * microDivisionsSpacing;
        constexpr float c = 0.4f;
        vertices.push_back({-gridLengthX, y, 0, {c, c, c, 1.f}});
        vertices.push_back({gridLengthX, y, 0, {c, c, c, 1.f}});
    }

    NumVertices = vertices.size();

    D3D11_BUFFER_DESC bd = {};
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = NumVertices * sizeof(LineVertex);
    bd.BindFlags = D3D11_BIND_VERTEX_BUFFER;
    bd.CPUAccessFlags = 0;

    D3D11_SUBRESOURCE_DATA InitData = {};
    InitData.pSysMem = vertices.data();
    hr = device->CreateBuffer(&bd, &InitData, &VertexBuffer);
    if (FAILED(hr))
    {
        Logger::Log("Failed to create debug vertex buffer");
        return hr;
    }

    bInitialised = true;

    return hr;
}

void Debug::SetupRender(ID3D11DeviceContext* deviceContext)
{
    deviceContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_LINELIST);
    deviceContext->VSSetShader(VertexShader, nullptr, 0);
    deviceContext->IASetInputLayout(InputLayout);
    deviceContext->PSSetShader(PixelShader, nullptr, 0);

    UINT strides[1] = {4 * 7};
    UINT offsets[1] = {0};
    deviceContext->IASetVertexBuffers(0, 1, &VertexBuffer, strides, offsets);
}
