#include "pch.h"

#include "ScreenCaptureWinRT.h"

using namespace ItemFinder;

winrt::com_ptr<ID3D11Device> m_d3dDevice;
winrt::com_ptr<ID3D11DeviceContext> m_d3dContext{ nullptr };

CapturedFrameWinRT::CapturedFrameWinRT(winrt::com_ptr<ID3D11Device> d3dDevice, winrt::com_ptr<ID3D11DeviceContext> d3dContext, winrt::com_ptr<ID3D11Texture2D> texture)
{
    D3D11_TEXTURE2D_DESC capturedTextureDesc;
    texture->GetDesc(&capturedTextureDesc);

    m_width = capturedTextureDesc.Width;
    m_height = capturedTextureDesc.Height;

    capturedTextureDesc.Usage = D3D11_USAGE_STAGING;
    capturedTextureDesc.BindFlags = 0;
    capturedTextureDesc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
    capturedTextureDesc.MiscFlags = 0;

    winrt::com_ptr<ID3D11Texture2D> userTexture = nullptr;
    winrt::check_hresult(d3dDevice->CreateTexture2D(&capturedTextureDesc, NULL, userTexture.put()));

    d3dContext->CopyResource(userTexture.get(), texture.get());

    winrt::check_hresult(d3dContext->Map(userTexture.get(), NULL, D3D11_MAP_READ, 0, &m_resource));
}

cv::Mat CapturedFrameWinRT::GetMat()
{
    cv::Mat windowMat(m_height, m_width, CV_8UC4, m_resource.pData);
    return windowMat;
}

ScreenCaptureWinRT::ScreenCaptureWinRT()
{
    winrt::check_hresult(D3D11CreateDevice(nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, D3D11_CREATE_DEVICE_BGRA_SUPPORT,
        nullptr, 0, D3D11_SDK_VERSION, m_d3dDevice.put(), nullptr, nullptr));

    const auto dxgiDevice = m_d3dDevice.as<IDXGIDevice>();
    {
        winrt::com_ptr<::IInspectable> inspectable;
        winrt::check_hresult(CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice.get(), inspectable.put()));
        m_device = inspectable.as<winrt::Windows::Graphics::DirectX::Direct3D11::IDirect3DDevice>();
    }

    auto idxgiDevice2 = dxgiDevice.as<IDXGIDevice2>();
    winrt::com_ptr<IDXGIAdapter> adapter;
    winrt::check_hresult(idxgiDevice2->GetParent(winrt::guid_of<IDXGIAdapter>(), adapter.put_void()));
    winrt::com_ptr<IDXGIFactory2> factory;
    winrt::check_hresult(adapter->GetParent(winrt::guid_of<IDXGIFactory2>(), factory.put_void()));

    m_d3dDevice->GetImmediateContext(m_d3dContext.put());
}

void ScreenCaptureWinRT::SetWindow(HWND hwndGame)
{
    const auto activationFactory = winrt::get_activation_factory<
        winrt::Windows::Graphics::Capture::GraphicsCaptureItem>();
    auto interopFactory = activationFactory.as<IGraphicsCaptureItemInterop>();

    interopFactory->CreateForWindow(hwndGame, winrt::guid_of<ABI::Windows::Graphics::Capture::IGraphicsCaptureItem>(),
        reinterpret_cast<void**>(winrt::put_abi(m_item)));
}

std::unique_ptr<ICapturedFrame> ScreenCaptureWinRT::CaptureFrame()
{
    winrt::com_ptr<ID3D11Texture2D> texture;

    auto frameArrivedEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
    std::atomic_flag frameArrived = ATOMIC_FLAG_INIT;
    
    auto framePool = winrt::Windows::Graphics::Capture::Direct3D11CaptureFramePool::CreateFreeThreaded(
        m_device,
        winrt::Windows::Graphics::DirectX::DirectXPixelFormat::B8G8R8A8UIntNormalized,
        2,
        m_item.Size());
    auto session = framePool.CreateCaptureSession(m_item);
    auto revokeArrivedHandler = framePool.FrameArrived(winrt::auto_revoke, [&](auto& framePool, auto&)
        {
            if (!frameArrived.test_and_set())
            {
                auto frame = framePool.TryGetNextFrame();

                auto access = frame.Surface().as<IDirect3DDxgiInterfaceAccess>();
                access->GetInterface(winrt::guid_of<ID3D11Texture2D>(), texture.put_void());
                SetEvent(frameArrivedEvent);
            }
        });

    session.IsCursorCaptureEnabled(false);
    session.StartCapture();

    auto waitResult = WaitForSingleObject(frameArrivedEvent, 5000);
    if (waitResult != WAIT_OBJECT_0)
    {
        // Timed out
        return nullptr;
    }

    session.Close();

    return std::make_unique<CapturedFrameWinRT>(m_d3dDevice, m_d3dContext, texture);
}

void SaveBitmap(D3D11_MAPPED_SUBRESOURCE resource, D3D11_TEXTURE2D_DESC capturedTextureDesc)
{
    BITMAPINFO bmpInfo;

    // BMP 32 bpp
    ZeroMemory(&bmpInfo, sizeof(BITMAPINFO));
    bmpInfo.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    bmpInfo.bmiHeader.biBitCount = 32;
    bmpInfo.bmiHeader.biCompression = BI_RGB;
    bmpInfo.bmiHeader.biWidth = capturedTextureDesc.Width;
    bmpInfo.bmiHeader.biHeight = capturedTextureDesc.Height;
    bmpInfo.bmiHeader.biPlanes = 1;
    bmpInfo.bmiHeader.biSizeImage = capturedTextureDesc.Width * capturedTextureDesc.Height * 4;

    std::unique_ptr<BYTE> pBuf(new BYTE[bmpInfo.bmiHeader.biSizeImage]);
    UINT bmpRowPitch = capturedTextureDesc.Width * 4;
    auto sptr = static_cast<BYTE*>(resource.pData);
    auto dptr = pBuf.get() + bmpInfo.bmiHeader.biSizeImage - bmpRowPitch;

    UINT lRowPitch = std::min<UINT>(bmpRowPitch, resource.RowPitch);

    for (size_t h = 0; h < capturedTextureDesc.Height; ++h)
    {
        memcpy_s(dptr, bmpRowPitch, sptr, lRowPitch);
        sptr += resource.RowPitch;
        dptr -= bmpRowPitch;
    }

    std::wstring path = L"ScreenShot.bmp";

    FILE* file = nullptr;

    if (auto err = _wfopen_s(&file, path.c_str(), L"wb"); err != 0)
        return;

    if (file != nullptr)
    {
        BITMAPFILEHEADER bmpFileHeader;

        bmpFileHeader.bfReserved1 = 0;
        bmpFileHeader.bfReserved2 = 0;
        bmpFileHeader.bfSize = sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) + bmpInfo.bmiHeader.biSizeImage;
        bmpFileHeader.bfType = 'MB';
        bmpFileHeader.bfOffBits = sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER);

        fwrite(&bmpFileHeader, sizeof(BITMAPFILEHEADER), 1, file);
        fwrite(&bmpInfo.bmiHeader, sizeof(BITMAPINFOHEADER), 1, file);
        fwrite(pBuf.get(), bmpInfo.bmiHeader.biSizeImage, 1, file);

        fclose(file);
    }
}
