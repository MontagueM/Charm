#include "Camera.h"

#include "Input.h"

#include <algorithm>
#include <iostream>
#include <ostream>

Camera::Camera(float fovDegrees, float nearDistance, float farDistance, float aspectRatio)
    : FovDegrees(fovDegrees)
    , FovRadians(XMConvertToRadians(fovDegrees))
    , NearDistance(nearDistance)
    , FarDistance(farDistance)
    , AspectRatio(aspectRatio)
{
    Reset();
    Update(0);
    OnWindowSizeChanged = [&](int width, int height) { OnWindowSizeChangedImpl(width, height); };
}

void Camera::Update(float tickDelta)
{
    // bool Moved = UpdateFromInput(tickDelta);
    if (XMMatrixIsIdentity(ViewMatrix) || true)
    {
        UpdateViewMatrix();
    }
}

bool Camera::UpdateFromInput(float tickDelta)
{
    // Mouse should be done first to get correct RightDirection vector
    bool MouseMoved = UpdateFromMouse(tickDelta);
    bool KeyboardMoved = UpdateFromKeyboard(tickDelta);
    return KeyboardMoved | MouseMoved;
}

void Camera::UpdateFromKeyboard(MoveDirection direction, float tickDelta)
{
    // if (Mode == CameraMode::Orbit)
    // {
    //     return;
    // }
    switch (direction)
    {
        case MoveDirection::Forward:
            Position += ForwardDirection * MovementSpeed * tickDelta;
            break;
        case MoveDirection::Backward:
            Position -= ForwardDirection * MovementSpeed * tickDelta;
            break;
        case MoveDirection::Left:
            Position += RightDirection * MovementSpeed * tickDelta;
            break;
        case MoveDirection::Right:
            Position -= RightDirection * MovementSpeed * tickDelta;
            break;
        case MoveDirection::Up:
            Position -= UpDirection * MovementSpeed * tickDelta;
            break;
        case MoveDirection::Down:
            Position += UpDirection * MovementSpeed * tickDelta;
            break;
    }
}

bool Camera::UpdateFromKeyboard(float tickDelta)
{
    bool Moved = false;
    if (!Input::IsMouseCaptured())    // || Mode == CameraMode::Orbit)
    {
        return Moved;
    }
    if (Input::IsKeyDown('W'))
    {
        UpdateFromKeyboard(MoveDirection::Forward, tickDelta);
        Moved = true;
    }
    else if (Input::IsKeyDown('S'))
    {
        UpdateFromKeyboard(MoveDirection::Backward, tickDelta);
        Moved = true;
    }
    if (Input::IsKeyDown('A'))
    {
        UpdateFromKeyboard(MoveDirection::Left, tickDelta);
        Moved = true;
    }
    else if (Input::IsKeyDown('D'))
    {
        UpdateFromKeyboard(MoveDirection::Right, tickDelta);
        Moved = true;
    }
    if (Input::IsKeyDown('Q'))
    {
        UpdateFromKeyboard(MoveDirection::Up, tickDelta);
        Moved = true;
    }
    else if (Input::IsKeyDown('E'))
    {
        UpdateFromKeyboard(MoveDirection::Down, tickDelta);
        Moved = true;
    }
    return Moved;
}

bool Camera::UpdateFromMouse(DXSM::Vector2 mouseDelta, float tickDelta, bool isMouseCaptured)
{
    bool Moved = false;

    if (XMMatrixIsIdentity(ViewMatrix) || isMouseCaptured && (mouseDelta.x != 0 || mouseDelta.y != 0))
    {
        if (isMouseCaptured)
        {
            // This seems inefficient
            const float thetaDelta = mouseDelta.x * RotationSpeed * 0.005;
            const float phiDelta = -mouseDelta.y * RotationSpeed * 0.005;

            Theta += thetaDelta;
            Phi += phiDelta;
        }

        Moved = true;
    }
    return Moved;
}

void Camera::SetMode(CameraMode mode)
{
    Mode = mode;
}

void Camera::UpdateScroll(int delta)
{
    if (Mode == CameraMode::Orbit)
    {
        Radius += -delta * 0.001f;
        Radius = max(0.01f, Radius);
    }
}

void Camera::Reset()
{
    Position = XMVectorSet(0.0f, 0.0f, 1.0f, 0.0f);
    Theta = -50.f;
    Phi = 30.f;

    Radius = 3.f;
    OrbitOrigin = XMVectorSet(0.0f, 0.0f, 0.0f, 0.0f);

    RightDirection = XMVectorSet(-1.0f, 0.0f, 0.0f, 0.0f);
    ForwardDirection = XMVectorSet(0.0f, 1.0f, 0.0f, 0.0f);
}

void Camera::MoveOrbitOrigin(float mouseX, float mouseY)
{
    if (Mode != CameraMode::Orbit)
    {
        return;
    }

    OrbitOrigin += RightDirection * mouseX * 0.01f;
    OrbitOrigin += UpDirection * mouseY * 0.01f;
}

void Camera::OnWindowSizeChangedImpl(int width, int height)
{
    AspectRatio = (float) width / height;
}

bool Camera::UpdateFromMouse(float tickDelta)
{
    const DXSM::Vector2 MouseDelta = Input::GetMouseDelta();
    return UpdateFromMouse(MouseDelta, tickDelta, Input::IsMouseCaptured());
}

void Camera::UpdateViewMatrix()
{
    const float theta = XMConvertToRadians(Theta);
    const float phi = XMConvertToRadians(Phi);

    switch (Mode)
    {
        case CameraMode::Orbit:
        {
            // spherical coordinate calculation for position of point on sphere
            Position = XMVectorSet(Radius * sinf(phi) * cosf(theta), Radius * sinf(phi) * sinf(theta), Radius * cosf(phi), 0.f);
            ViewMatrix = XMMatrixLookAtRH(Position, OrbitOrigin, UpDirection);
            break;
        }
        case CameraMode::Free:
        {
            ForwardDirection = XMVectorSet(cosf(phi) * sinf(theta), cosf(phi) * cosf(theta), sinf(phi), 0);
            ViewMatrix = XMMatrixLookToRH(Position, ForwardDirection, UpDirection);

            RightDirection = XMVector3Cross(UpDirection, ForwardDirection);
            break;
        }
    }

    XMVector3Normalize(ForwardDirection);
    XMVector3Normalize(UpDirection);
    XMVector3Normalize(RightDirection);

    // std::cout << "CameraPos: " << Position.m128_f32[0] << ", " << Position.m128_f32[1] << ", " << Position.m128_f32[2] << std::endl;
    // std::cout << "CameraDir: " << Theta << ", " << Phi << std::endl;
    // std::cout << "Radius: " << Radius << std::endl;
}

XMMATRIX Camera::GetViewMatrix() const
{
    return ViewMatrix;
}

XMVECTOR Camera::GetPosition() const
{
    return Position;
}

XMVECTOR Camera::GetDirection() const
{
    return ForwardDirection;
}

float Camera::GetFOVDegrees() const
{
    return FovDegrees;
}

float Camera::GetFOVRadians() const
{
    return FovRadians;
}

float Camera::GetAspectRatio() const
{
    return AspectRatio;
}
