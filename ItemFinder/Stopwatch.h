#pragma once

#include <chrono>
#include <string>

class Stopwatch
{
public:
    using elapsed_resolution = std::chrono::milliseconds;

    Stopwatch()
    {
        Reset();
    }

    void Reset()
    {
        reset_time = clock.now();
    }

    elapsed_resolution Elapsed()
    {
        return std::chrono::duration_cast<elapsed_resolution>(clock.now() - reset_time);
    }

    void PrintElapsed()
    {
        int count = static_cast<int>(Elapsed().count());
        std::wstring str = L"Elapsed: ";
        str += std::to_wstring(count);
        str += L"\n";
        OutputDebugString(str.c_str());
    }

private:
    std::chrono::high_resolution_clock clock;
    std::chrono::high_resolution_clock::time_point reset_time;
};