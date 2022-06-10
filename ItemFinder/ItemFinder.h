#pragma once

// The following ifdef block is the standard way of creating macros which make exporting
// from a DLL simpler. All files within this DLL are compiled with the ITEMFINDER_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see
// ITEMFINDER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef ITEMFINDER_EXPORTS
#define ITEMFINDER_API __declspec(dllexport)
#else
#define ITEMFINDER_API __declspec(dllimport)
#endif

struct Point
{
	int32_t X;
	int32_t Y;

	Point(int32_t X, int32_t Y) :
		X(X),
		Y(Y)
	{
	}

	Point() :
		X(0),
		Y(0)
	{
	}
};

enum class SearchMode
{
	ToGray,
	RedOnly,
	BlueOnly,
	GreenOnly,
	Count,
};

enum class LogLevel
{
	Error,
	Warn,
	Info,
	Verbose,
};

extern "C"
{

typedef void(CALLBACK * LogCallback)(LogLevel, PCWSTR);

extern LogCallback s_LogString;

ITEMFINDER_API HRESULT InitializeItemFinder(LogCallback logCallback);

ITEMFINDER_API HRESULT SetWindow(HWND hwndGame);

ITEMFINDER_API HRESULT AddMarker(uint32_t id, PCWSTR path, PCWSTR maskPath, SearchMode mode);
ITEMFINDER_API HRESULT RemoveMarker(uint32_t id);

ITEMFINDER_API HRESULT SetParameters(uint32_t itemSize, double searchScale);
ITEMFINDER_API HRESULT InvalidateSession();
ITEMFINDER_API HRESULT FindMarker(uint32_t id, double threshold, Point* markerPosition);

}