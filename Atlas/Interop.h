#pragma once

#include "DDSTextureLoader.h"
#include "Renderer.h"

#include <DirectXMath.h>
#include <windows.h>

enum class MoveDirection : int;
struct PartInfo;
struct BufferGroup;

extern "C"
{
    __declspec(dllexport) HRESULT __cdecl Init(HWND hwnd);
    __declspec(dllexport) HRESULT __cdecl Render(void* pResource, bool isNewSurface);
    __declspec(dllexport) void __cdecl MoveCamera(MoveDirection direction);
    __declspec(dllexport) void __cdecl RegisterMouseDelta(float mouseX, float mouseY);
    __declspec(dllexport) void __cdecl CreateStaticMesh(uint32_t hash);
    __declspec(dllexport) void __cdecl AddStaticMeshBufferGroup(uint32_t hash, BufferGroup bufferGroup);
    __declspec(dllexport) void __cdecl CreateStaticMeshPart(uint32_t hash, int partIndex, PartInfo partInfo);
}
