#pragma once
#include "DDSTextureLoader.h"

class Debug
{
public:
    static HRESULT Initialise(ID3D11Device* device);
    static void DrawGrid(ID3D11DeviceContext* deviceContext);

private:
    inline static ID3D11VertexShader* VertexShader;
    inline static ID3D11PixelShader* PixelShader;
    inline static ID3D11InputLayout* InputLayout;
    inline static ID3D11Buffer* VertexBuffer;
    inline static bool bInitialised = false;
    inline static int NumVertices;

    static void SetupRender(ID3D11DeviceContext* deviceContext);
};
