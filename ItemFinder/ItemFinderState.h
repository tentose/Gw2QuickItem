#pragma once

namespace ItemFinder
{

struct ItemFinderState
{
    ItemFinderState();

    void FindMarker();

    ScreenCaptureWinRT m_capture;
    HWND m_lastHwnd = nullptr;
    MarkerFinder m_finder;
    double m_scale = 0.6;
    std::unique_ptr<MarkerFindSession> m_session;
};

}
