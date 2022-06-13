// ItemFinder.cpp : Defines the exported functions for the DLL.
//

#include "pch.h"
#include "framework.h"
#include "ItemFinder.h"
#include "ScreenCaptureWinRT.h"
#include "Stopwatch.h"
#include "MarkerFinder.h"
#include "ItemFinderState.h"

using namespace ItemFinder;

static std::unique_ptr<ItemFinderState> s_state;

ITEMFINDER_API HRESULT InitializeItemFinder(LogCallback logCallback) try
{
    s_LogString = logCallback;

    if (s_LogString != nullptr)
    {
        s_LogString(LogLevel::Info, L"InitializeItemFinder");
    }

    s_state = std::make_unique<ItemFinderState>();
    return S_OK;
} CATCH_LOG_RETURN_HR(E_FAIL) // Ugh. But we're just using this to indicate the call failed. Hopefully there's enough in the logs to figure out what went wrong.

ITEMFINDER_API HRESULT SetWindow(HWND hwndGame) try
{
    s_state->capture.SetWindow(hwndGame);

    return S_OK;
} CATCH_LOG_RETURN_HR(E_FAIL)

ITEMFINDER_API HRESULT AddMarker(uint32_t id, PCWSTR path, PCWSTR maskPath, SearchMode mode) try
{
    s_state->finder.AddMarker(id, path, maskPath, mode);

    return S_OK;
} CATCH_LOG_RETURN_HR(E_FAIL)

ITEMFINDER_API HRESULT RemoveMarker(uint32_t id) try
{
    s_state->finder.RemoveMarker(id);

    return S_OK;
} CATCH_LOG_RETURN_HR(E_FAIL)

ITEMFINDER_API HRESULT SetParameters(uint32_t itemSize, double searchScale, PCWSTR debugOutputDirectory) try
{
    s_state->finder.SetMarkerSize(itemSize);
    s_state->scale = searchScale;
    s_state->debugOutputDirectory = debugOutputDirectory;

    return InvalidateSession();
} CATCH_LOG_RETURN_HR(E_FAIL)

ITEMFINDER_API HRESULT InvalidateSession() try
{
    s_state->session = nullptr;

    return S_OK;
} CATCH_LOG_RETURN_HR(E_FAIL)

ITEMFINDER_API HRESULT FindMarker(uint32_t id, double threshold, Point* markerPosition) try
{
    auto result = s_state->capture.CaptureFrame();
    auto image = result->GetMat();

    if (!s_state->session)
    {
        s_state->session = s_state->finder.CreateSessionForImage(image, s_state->scale, s_state->debugOutputDirectory);
    }

    auto point = s_state->session->FindMarker(id, threshold);
    *markerPosition = point;

    return S_OK;
} CATCH_LOG_RETURN_HR(E_FAIL)
