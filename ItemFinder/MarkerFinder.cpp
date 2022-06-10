#include "pch.h"

#include "ItemFinder.h"
#include "MarkerFinder.h"

using namespace ItemFinder;

cv::Mat ProcessImageForMode(cv::Mat image, SearchMode mode)
{
	cv::Mat result;

	if (mode == SearchMode::ToGray)
	{
		cv::cvtColor(image, result, cv::COLOR_BGRA2GRAY);
	}
	else
	{
		cv::Mat bgr[3];
		cv::split(image, bgr);
		if (mode == SearchMode::BlueOnly)
		{
			result = bgr[0];
		}
		else if (mode == SearchMode::GreenOnly)
		{
			result = bgr[1];
		}
		else if (mode == SearchMode::RedOnly)
		{
			result = bgr[2];
		}
	}

	return result;
}

MarkerFindSession::MarkerFindSession(std::map<uint32_t, Marker> markers, cv::Mat image) :
	m_markers(markers),
	m_originalImage(image)
{
}

Point MarkerFindSession::FindMarker(uint32_t id, double threshold)
{
	Marker const& marker = m_markers[id];

	int imageIndex = static_cast<int>(marker.Mode);
	if (m_image[imageIndex].empty())
	{
		m_image[imageIndex] = ProcessImageForMode(m_originalImage, marker.Mode);
	}

	cv::Mat searchResult;
	if (marker.Mask.empty())
	{
		cv::matchTemplate(m_image[imageIndex], marker.Image, searchResult, cv::TemplateMatchModes::TM_SQDIFF_NORMED);
	}
	else
	{
		cv::matchTemplate(m_image[imageIndex], marker.Image, searchResult, cv::TemplateMatchModes::TM_SQDIFF_NORMED, marker.Mask);
	}
	
	double minVal, maxVal;
	cv::Point minLoc, maxLoc;
	cv::minMaxLoc(searchResult, &minVal, &maxVal, &minLoc, &maxLoc);

	Point markerPosition(-1, -1);
	if (minVal < threshold)
	{
		markerPosition = Point(minLoc.x, minLoc.y);
	}

	return markerPosition;
}

MarkerFinder::MarkerFinder()
{
}

void MarkerFinder::AddMarker(uint32_t id, std::wstring path, std::wstring maskPath, SearchMode mode)
{
	m_markerInfos[id] = MarkerInfo(path, maskPath, mode);
}

void MarkerFinder::RemoveMarker(uint32_t id)
{
	m_markerInfos.erase(id);
}

void MarkerFinder::SetMarkerSize(uint32_t size)
{
	m_markerSize = size;
}

std::unique_ptr<MarkerFindSession> MarkerFinder::CreateSessionForImage(cv::Mat image, double scale)
{
	cv::Mat resizedImage;
	cv::resize(image, resizedImage, cv::Size(), scale, scale, cv::INTER_LINEAR);

	std::map<uint32_t, Marker> markers;
	for (auto const& kv : m_markerInfos)
	{
		markers[kv.first] = LoadMarkerFromFile(kv.second, scale);
	}

	return std::make_unique<MarkerFindSession>(markers, resizedImage);
}

Marker MarkerFinder::LoadMarkerFromFile(MarkerInfo const& markerInfo, double additionalScale)
{
	Marker marker;
	marker.Mode = markerInfo.Mode;

	auto buffer = ReadFileContents(markerInfo.ImagePath);
	auto tmpImage = cv::imdecode(buffer, cv::IMREAD_COLOR);
	cv::Mat resizedImage;
	cv::resize(tmpImage, resizedImage, cv::Size(m_markerSize, m_markerSize), 0, 0, cv::INTER_LINEAR);

	marker.Image = ProcessImageForMode(resizedImage, marker.Mode);

	if (additionalScale != 1.0)
	{
		cv::resize(marker.Image, marker.Image, cv::Size(), additionalScale, additionalScale, cv::INTER_LINEAR);
	}

	if (!markerInfo.MaskPath.empty())
	{
		buffer = ReadFileContents(markerInfo.MaskPath);

		auto tmpMask = cv::imdecode(buffer, cv::IMREAD_COLOR);
		cv::Mat resizedMask;
		cv::resize(tmpMask, resizedMask, cv::Size(m_markerSize, m_markerSize), 0, 0, cv::INTER_LINEAR);

		cv::Mat bgr[3];
		cv::split(resizedMask, bgr);

		marker.Mask = bgr[0];

		if (additionalScale != 1.0)
		{
			cv::resize(marker.Mask, marker.Mask, cv::Size(), additionalScale, additionalScale, cv::INTER_LINEAR);
		}
	}

	return marker;
}

std::vector<byte> MarkerFinder::ReadFileContents(const std::wstring & path)
{
	wil::unique_handle file(CreateFile(path.c_str(), GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, 0, nullptr));
	THROW_LAST_ERROR_IF(!file);

	DWORD size = GetFileSize(file.get(), nullptr);
	DWORD bytesRead = 0;
	std::vector<byte> buffer(size);
	THROW_IF_WIN32_BOOL_FALSE(ReadFile(file.get(), buffer.data(), size, nullptr, nullptr));

	return buffer;
}
