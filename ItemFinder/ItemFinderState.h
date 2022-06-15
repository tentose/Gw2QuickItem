#pragma once

namespace ItemFinder
{

struct ItemFinderState
{
    ScreenCaptureWinRT capture;
    HWND lastHwnd = nullptr;
    MarkerFinder finder;
    double scale = 0.6;
    std::unique_ptr<MarkerFindSession> session;
    std::wstring debugOutputDirectory;
};

}
