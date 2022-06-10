#include "pch.h"

#include "MarkerFinder.h"
#include "ScreenCaptureWinRT.h"

#include "Stopwatch.h"

#include "ItemFinderState.h"

using namespace ItemFinder;

ItemFinderState::ItemFinderState()
{
	m_finder.AddMarker(2056189, LR"(C:\ProgramData\Blish HUD\cache\assets\2\2056189.png)", LR"(D:\Repos\Gw2QuickItem\QuickItem\ref\Textures\itemmask.png)", SearchMode::ToGray);
}

void ItemFinderState::FindMarker()
{
	Stopwatch watch;

	auto targetHwnd = FindWindow(L"ArenaNet_Gr_Window_Class", NULL);
	if (targetHwnd != m_lastHwnd)
	{
		m_capture.SetWindow(targetHwnd);
		m_lastHwnd = targetHwnd;
	}

	watch.PrintElapsed();

	auto result = m_capture.CaptureFrame();

	watch.PrintElapsed();

	auto image = result->GetMat();

	if (!m_session)
	{
		m_session = m_finder.CreateSessionForImage(image, 0.6);
	}
	
	auto point = m_session->FindMarker(2056189);

	watch.PrintElapsed();

	std::wstring output = L"Found at " + std::to_wstring(point.X) + L", " + std::to_wstring(point.Y) + L"\n";
	OutputDebugString(output.c_str());
}