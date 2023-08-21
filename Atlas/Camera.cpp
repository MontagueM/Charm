#include "Camera.h"

#include "Input.h"

#include <iostream>
#include <ostream>

Camera::Camera(float fov, float nearDistance, float farDistance, float aspectRatio)
    : Fov(fov), NearDistance(nearDistance), FarDistance(farDistance), AspectRatio(aspectRatio)
{
    Update(0);
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
    if (!Input::IsMouseCaptured())
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
            const double pitchDelta = mouseDelta.y * RotationSpeed * 0.005;
            const double yawDelta = mouseDelta.x * RotationSpeed * 0.005;

            RotationEulerDegrees.m128_f32[0] += pitchDelta;
            RotationEulerDegrees.m128_f32[1] += yawDelta;
        }

        Moved = true;
    }
    return Moved;
}

bool Camera::UpdateFromMouse(float tickDelta)
{
    const DXSM::Vector2 MouseDelta = Input::GetMouseDelta();
    return UpdateFromMouse(MouseDelta, tickDelta, Input::IsMouseCaptured());
}

void Camera::UpdateViewMatrix()
{
    // const XMVECTOR lookAt = XMVectorSet(0, 0, 0, 0);
    const XMVECTOR up = XMVectorSet(0, 0, 1, 0);

    const float pitch = XMConvertToRadians(RotationEulerDegrees.m128_f32[0]);
    const float yaw = XMConvertToRadians(RotationEulerDegrees.m128_f32[1]);
    const float roll = XMConvertToRadians(RotationEulerDegrees.m128_f32[2]);
    const XMMATRIX RotationMatrix = XMMatrixRotationRollPitchYaw(pitch, yaw, roll);

    // ForwardDirection =
    // ForwardDirection = XMVector3Transform(lookAt, RotationMatrix);
    // UpDirection = XMVector3TransformCoord(up, RotationMatrix);
    RightDirection = XMVector3Cross(UpDirection, ForwardDirection);
    const XMVECTOR lookAt = XMVectorSet(cos(pitch) * sin(yaw), cos(pitch) * cos(yaw), -sin(pitch), 0);

    ForwardDirection = lookAt;
    XMVector3Normalize(ForwardDirection);
    XMVector3Normalize(UpDirection);
    XMVector3Normalize(RightDirection);

    std::cout << "CameraPos: " << Position.m128_f32[0] << ", " << Position.m128_f32[1] << ", " << Position.m128_f32[2] << std::endl;
    std::cout << "CameraDir: " << RotationEulerDegrees.m128_f32[0] << ", " << RotationEulerDegrees.m128_f32[1] << ", "
              << RotationEulerDegrees.m128_f32[2] << std::endl;

    ViewMatrix = XMMatrixLookToRH(Position, ForwardDirection, UpDirection);
}

DirectX::XMMATRIX Camera::GetViewMatrix() const
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

float Camera::GetFOV() const
{
    return Fov;
}
