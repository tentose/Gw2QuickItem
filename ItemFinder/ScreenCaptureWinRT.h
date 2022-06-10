#pragma once

#include "IScreenCapture.h"

namespace ItemFinder
{

class CapturedFrameWinRT : public ICapturedFrame
{
public:
	CapturedFrameWinRT(winrt::com_ptr<ID3D11Device> d3dDevice, winrt::com_ptr<ID3D11DeviceContext> d3dContext, winrt::com_ptr<ID3D11Texture2D> texture);

	virtual cv::Mat GetMat();

private:
	winrt::com_ptr<ID3D11Texture2D> m_texture;
	D3D11_MAPPED_SUBRESOURCE m_resource;
	UINT m_width;
	UINT m_height;
};

class ScreenCaptureWinRT : public IScreenCapture
{
public:
	ScreenCaptureWinRT();

	// IScreenCapture
	virtual void SetWindow(HWND hwndGame);
	virtual std::unique_ptr<ICapturedFrame> CaptureFrame();

private:
	winrt::com_ptr<ID3D11Device> m_d3dDevice;
	winrt::com_ptr<ID3D11DeviceContext> m_d3dContext{ nullptr };
	winrt::Windows::Graphics::DirectX::Direct3D11::IDirect3DDevice m_device{ nullptr };
	winrt::Windows::Graphics::Capture::GraphicsCaptureItem m_item{ nullptr };
};

}