#pragma once

#include "DDSTextureLoader.h"
#include "Renderer.h"

#include <DirectXMath.h>
#include <windows.h>

struct Blob;
enum class MoveDirection : int;
enum class CameraMode : int;
struct PartInfo;
struct BufferGroup;
struct StaticMeshInfo;

extern "C"
{
    __declspec(dllexport) HRESULT __cdecl Init(HWND hwnd, int width, int height);
    __declspec(dllexport) HRESULT __cdecl Render(void* pResource, bool isNewSurface);
    __declspec(dllexport) void __cdecl MoveCamera(MoveDirection direction);
    __declspec(dllexport) void __cdecl SetCameraMode(CameraMode mode);
    __declspec(dllexport) void __cdecl RegisterMouseDelta(float mouseX, float mouseY);
    __declspec(dllexport) void __cdecl RegisterMouseScroll(int delta);
    __declspec(dllexport) void __cdecl CreateStaticMesh(uint32_t hash, Blob staticMeshTransforms);
    __declspec(dllexport) void __cdecl AddStaticMeshBufferGroup(uint32_t hash, BufferGroup bufferGroup);
    __declspec(dllexport) void __cdecl CreateStaticMeshPart(uint32_t hash, PartInfo partInfo);
    __declspec(dllexport) void __cdecl Resize(int width, int height);
}
