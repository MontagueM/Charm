#include "Camera.h"
#include "Input.h"
#include "NativeWindow.h"
#include "Renderer.h"

#include <chrono>
#include <iostream>
#include <queue>
#include <string>

using namespace std::chrono;

float getTimeDeltaMs(const time_point<system_clock>& begin, const time_point<system_clock>& end)
{
    return std::chrono::duration_cast<duration<float, std::milli>>(end - begin).count();
}

float getTimeDeltaSecs(const time_point<system_clock>& begin, const time_point<system_clock>& end)
{
    return std::chrono::duration_cast<duration<float>>(end - begin).count();
}

int main(int argc, char* argv[])
{
    std::shared_ptr<NativeWindow> window = std::make_shared<NativeWindow>(L"ATLAS", 720, 480);
    // Input::InputWindow = window;
    DX11Renderer* renderer = new DX11Renderer();
    Camera* camera = new Camera(90.0f, 0.1f, 1000.0f, window->GetAspectRatio());
    HRESULT hr = renderer->InitialiseGeneral(window);
    if (FAILED(hr))
    {
        printf("Failed to initialise renderer: 0x%llx", hr);
        return 1;
    }
    hr = renderer->Initialise();
    if (FAILED(hr))
    {
        printf("Failed to initialise renderer: 0x%llx", hr);
        return 1;
    }
    printf("Renderer initialised\n");

    time_point<system_clock> tickTimeBefore;
    time_point<system_clock> tickTimeAfter = system_clock::now();
    float tickDelta = 0.0f;    // in seconds

    // Main message loop
    MSG msg = {0};
    std::queue<float> frametimes = std::queue<float>();
    int frameCount = 0;

    float targetFrameTime = static_cast<float>(1) / 144;

    while (WM_QUIT != msg.message)
    {
        if (PeekMessage(&msg, nullptr, 0, 0, PM_REMOVE))
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
        else
        {
            // if (tickDelta < targetFrameTime)
            // Sleep((targetFrameTime - tickDelta) * 1000);
            tickTimeBefore = tickTimeAfter;
            camera->Update(tickDelta);
            Input::UpdatePrevious(window);
            renderer->Render(camera, tickDelta);
            tickTimeAfter = system_clock::now();
            tickDelta = getTimeDeltaSecs(tickTimeBefore, tickTimeAfter);
            if (frameCount % 10 == 0)
            {
                frametimes.push(tickDelta);
            }
            frameCount++;
            if (frameCount > 1000)
            {
                float total = 0.0f;
                for (int i = 0; i < 100; i++)
                {
                    total += frametimes.front();
                    frametimes.pop();
                }
                float averageFrametimeMs = 1000.0f * total / 100.0f;
                std::cout << "Average frametime: " << std::to_string(averageFrametimeMs)
                          << ", Average FPS: " << std::to_string(1000 / averageFrametimeMs) << std::endl;
                frameCount = 0;
            }
        }
    }

    renderer = nullptr;

    return 0;
}
