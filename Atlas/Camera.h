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

enum class CameraMode : int
{
    Orbit,
    Free
};

class Camera
{
public:
    Camera(float fovDegrees, float nearDistance, float farDistance, float aspectRatio);
    void Update(float tickDelta);
    XMMATRIX GetViewMatrix() const;
    XMVECTOR GetPosition() const;
    XMVECTOR GetDirection() const;
    float GetFOVDegrees() const;
    float GetFOVRadians() const;
    float GetAspectRatio() const;

    void UpdateFromKeyboard(MoveDirection direction, float tickDelta);
    bool UpdateFromMouse(SimpleMath::Vector2 mouseDelta, float tickDelta, bool isMouseCaptured);
    void SetMode(CameraMode mode);
    void UpdateScroll(int delta);

    std::function<void(int, int)> OnWindowSizeChanged;
    void OnWindowSizeChangedImpl(int width, int height);

private:
    float FovDegrees;
    float FovRadians;
    float NearDistance;
    float FarDistance;
    float AspectRatio;
    // todo consider changing this to be polymorphic
    CameraMode Mode = CameraMode::Orbit;

    // XMVECTOR Position = XMVectorSet(2.0f, -3.0f, 2.0f, 0.0f);
    XMVECTOR Position = XMVectorSet(0.0f, 0.0f, 1.0f, 0.0f);
    // XMVECTOR Position = XMVectorSet(350.553864f, -878.668212f, 10.7369051f, 1.0f);
    // XMVECTOR Position = XMVectorSet(344.f, -867.f, 11.f, 1.0f);
    // XMVECTOR RotationEulerDegrees = XMVectorSet(14.0f, -74.0f, 0.0f, 0.0f);

    // Spherical coordinates for orbit camera - theta is XY plane
    // In freecam, only theta and phi are used
    float Theta = -50.f;
    float Phi = 30.f;
    float Radius = 3.f;

    const XMVECTOR UpDirection = XMVectorSet(0.0f, 0.0f, 1.0f, 0.0f);
    XMVECTOR RightDirection = XMVectorSet(-1.0f, 0.0f, 0.0f, 0.0f);
    XMVECTOR ForwardDirection = XMVectorSet(0.0f, 1.0f, 0.0f, 0.0f);
    const float MovementSpeed = 3.0f;
    const double RotationSpeed = 10.0f;
    XMMATRIX ViewMatrix = XMMatrixIdentity();

    bool UpdateFromInput(float tickDelta);
    bool UpdateFromKeyboard(float tickDelta);
    bool UpdateFromMouse(float tickDelta);

    void UpdateViewMatrix();
};
