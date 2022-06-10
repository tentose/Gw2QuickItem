#pragma once

#include <string>
#include <map>
#include <opencv2/core.hpp>
#include "ItemFinder.h"

namespace ItemFinder
{

struct MarkerInfo
{
	std::wstring ImagePath;
	std::wstring MaskPath;
	SearchMode Mode = SearchMode::ToGray;

	MarkerInfo(std::wstring path, std::wstring maskPath, SearchMode mode) :
		ImagePath(path),
		MaskPath(maskPath),
		Mode(mode)
	{
	}

	MarkerInfo()
	{
	}
};

struct Marker
{
	cv::Mat Image;
	cv::Mat Mask;
	SearchMode Mode = SearchMode::ToGray;
};

class MarkerFindSession
{
public:
	MarkerFindSession(std::map<uint32_t, Marker> markers, cv::Mat image);

	Point FindMarker(uint32_t id, double threshold = 0.01);

private:
	cv::Mat m_originalImage;
	cv::Mat m_image[static_cast<int>(SearchMode::Count)];
	std::map<uint32_t, Marker> m_markers;
};

class MarkerFinder
{
public:
	MarkerFinder();

	void AddMarker(uint32_t id, std::wstring path, std::wstring maskPath, SearchMode mode);
	void RemoveMarker(uint32_t id);
	void SetMarkerSize(uint32_t size);

	std::unique_ptr<MarkerFindSession> CreateSessionForImage(cv::Mat image, double scale = 0.6);

private:
	Marker LoadMarkerFromFile(MarkerInfo const& markerInfo, double additionalScale = 1.0);
	std::vector<byte> ReadFileContents(std::wstring const& path);

	uint32_t m_markerSize = 62;
	std::map<uint32_t, MarkerInfo> m_markerInfos;
};

}