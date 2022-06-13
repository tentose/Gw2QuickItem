#include "pch.h"

#include "MarkerFinder.h"
#include "ScreenCaptureWinRT.h"

#include "Stopwatch.h"

#include "ItemFinderState.h"

using namespace ItemFinder;

ItemFinderState::ItemFinderState()
{
	finder.AddMarker(2056189, LR"(C:\ProgramData\Blish HUD\cache\assets\2\2056189.png)", LR"(D:\Repos\Gw2QuickItem\QuickItem\ref\Textures\itemmask.png)", SearchMode::ToGray);
}

void ItemFinderState::FindMarker()
{
	Stopwatch watch;

	auto targetHwnd = FindWindow(L"ArenaNet_Gr_Window_Class", NULL);
	if (targetHwnd != lastHwnd)
	{
		capture.SetWindow(targetHwnd);
		lastHwnd = targetHwnd;
	}

	watch.PrintElapsed();

	auto result = capture.CaptureFrame();

	watch.PrintElapsed();

	auto image = result->GetMat();

	if (!session)
	{
		session = finder.CreateSessionForImage(image, 0.6);
	}
	
	auto point = session->FindMarker(2056189);

	watch.PrintElapsed();

	std::wstring output = L"Found at " + std::to_wstring(point.X) + L", " + std::to_wstring(point.Y) + L"\n";
	OutputDebugString(output.c_str());
}