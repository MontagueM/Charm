#pragma once
#include "SimpleMath.h"

using namespace DirectX;

enum class MoveDirection : int
{
    None,
    Forward,
    Backward,
    Left,
    Right,
    Up,
    Down
};

class Camera
{
public:
    Camera(float fov, float nearDistance, float farDistance, float aspectRatio);
    void Update(float tickDelta);
    XMMATRIX GetViewMatrix() const;
    XMVECTOR GetPosition() const;
    XMVECTOR GetDirection() const;
    float GetFOV() const;

    void UpdateFromKeyboard(MoveDirection direction, float tickDelta);
    bool UpdateFromMouse(SimpleMath::Vector2 mouseDelta, float tickDelta, bool isMouseCaptured);

private:
    float Fov;
    float NearDistance;
    float FarDistance;
    float AspectRatio;

    // XMVECTOR Position = XMVectorSet(13.0f, 3.0f, -4.0f, 0.0f);
    XMVECTOR Position = XMVectorSet(350.553864f, -878.668212f, 10.7369051f, 1.0f);
    XMVECTOR UpDirection = XMVectorSet(0.0f, 0.0f, 1.0f, 0.0f);
    XMVECTOR RightDirection = XMVectorSet(-1.0f, 0.0f, 0.0f, 0.0f);
    XMVECTOR ForwardDirection = XMVectorSet(0.0f, 1.0f, 0.0f, 0.0f);
    const float MovementSpeed = 10.0f;
    const double RotationSpeed = 10.0f;
    XMVECTOR RotationEulerDegrees = XMVectorSet(14.0f, -74.0f, 0.0f, 0.0f);
    XMMATRIX ViewMatrix = XMMatrixIdentity();

    bool UpdateFromInput(float tickDelta);
    bool UpdateFromKeyboard(float tickDelta);
    bool UpdateFromMouse(float tickDelta);

    void UpdateViewMatrix();
};
