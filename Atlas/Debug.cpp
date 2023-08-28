#include "Debug.h"

#include "Logger.h"
#include "Renderer.h"

struct LineVertex
{
    float x, y, z;
};

// clang-format off
LineVertex Vertices[] =
    {
    {-1, -1, 0},
    {1, -1, 0},

    {1, -1, 0},
    {1, 1, 0},

    {1, 1, 0},
    {1, -1, 0},

    {1, -1, 0},
    {-1, -1, 0},

    {0, -1, 0},
    {0, 1, 0},

    {-1, 0, 0},
    {1, 0, 0},
    };
// clang-format on

void Debug::DrawGrid(ID3D11DeviceContext* deviceContext)
{
    if (!bInitialised)
    {
        Logger::Log("Cannot debug render as has not been initialised");
        return;
    }
    SetupRender(deviceContext);

    deviceContext->Draw(ARRAYSIZE(Vertices), 0);
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

    D3D11_BUFFER_DESC bd = {};
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = ARRAYSIZE(Vertices) * sizeof(LineVertex);
    bd.BindFlags = D3D11_BIND_VERTEX_BUFFER;
    bd.CPUAccessFlags = 0;

    D3D11_SUBRESOURCE_DATA InitData = {};
    InitData.pSysMem = Vertices;
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

    UINT strides[1] = {12};
    UINT offsets[1] = {0};
    deviceContext->IASetVertexBuffers(0, 1, &VertexBuffer, strides, offsets);
}
