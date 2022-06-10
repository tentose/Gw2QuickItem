#pragma once

#include <Windows.h>
#include <opencv2/core.hpp>

struct ICapturedFrame
{
    virtual cv::Mat GetMat() = 0;
};

struct IScreenCapture
{
    virtual void SetWindow(HWND hwndGame) = 0;
    virtual std::unique_ptr<ICapturedFrame> CaptureFrame() = 0;
};
