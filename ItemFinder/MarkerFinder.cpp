#include "pch.h"

#include "ItemFinder.h"
#include "MarkerFinder.h"

using namespace ItemFinder;

cv::Mat ProcessImageForMode(cv::Mat image, SearchMode mode)
{
	cv::Mat result;

	if (mode == SearchMode::ToGray || mode == SearchMode::ToGrayWithMeanHueCheck)
	{
		cv::cvtColor(image, result, cv::COLOR_BGRA2GRAY);
	}
	else if (mode == SearchMode::BlueOnly || mode == SearchMode::GreenOnly || mode == SearchMode::RedOnly)
	{
		cv::Mat bgra[4];
		cv::split(image, bgra);
		if (mode == SearchMode::BlueOnly)
		{
			result = bgra[0];
		}
		else if (mode == SearchMode::GreenOnly)
		{
			result = bgra[1];
		}
		else if (mode == SearchMode::RedOnly)
		{
			result = bgra[2];
		}
	}
	else if (mode == SearchMode::YXorCrCb)
	{
	cv::Mat converted;
	cv::cvtColor(image, converted, cv::COLOR_BGR2YCrCb);
	cv::Mat ycrcb[3];
	cv::split(converted, ycrcb);

	cv::Mat crcb;
	cv::bitwise_or(ycrcb[1] / 16 * 16, ycrcb[2] / 16, crcb);
	cv::bitwise_xor(crcb, ycrcb[0], result);
	}
	else if (mode == SearchMode::HXorV)
	{
	cv::Mat converted;
	cv::cvtColor(image, converted, cv::COLOR_BGR2HSV);
	cv::Mat hsv[3];
	cv::split(converted, hsv);
	cv::bitwise_xor(hsv[0], hsv[2], result);
	}

	return result;
}

MarkerFindSession::MarkerFindSession(std::map<uint32_t, Marker> markers, cv::Mat image, double scale, std::wstring const& debugOutputDir) :
	m_markers(markers),
	m_originalImage(image),
	m_scale(scale),
	m_debugOutputDirectory(debugOutputDir)
{
	cv::resize(m_originalImage, m_originalScaledImage, cv::Size(), m_scale, m_scale, cv::INTER_LINEAR);
}

Point MarkerFindSession::FindMarker(uint32_t id, double threshold, int hueThreshold)
{
	Marker const& marker = m_markers[id];

	int imageIndex = static_cast<int>(marker.Mode);
	if (m_image[imageIndex].empty())
	{
		m_image[imageIndex] = ProcessImageForMode(m_originalImage, marker.Mode);

		cv::resize(m_image[imageIndex], m_image[imageIndex], cv::Size(), m_scale, m_scale, cv::INTER_LINEAR);

		if (!m_debugOutputDirectory.empty())
		{
			WriteDebugImage(m_image[imageIndex]);
		}
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
	Point markerPosition(-1, -1);

	cv::Mat debugOutput;
	if (!m_debugOutputDirectory.empty())
	{
		debugOutput = m_image[imageIndex].clone();
	}

	for (int attempt = 0; attempt < 5; attempt++)
	{
		cv::minMaxLoc(searchResult, &minVal, &maxVal, &minLoc, &maxLoc);
		
		if (minVal > threshold)
		{
			// no results below threshold, break;
			break;
		}

		if (marker.Mode == SearchMode::ToGrayWithMeanHueCheck)
		{
			// mode requires a hue check
			if (GetMeanHueDifference(marker, minLoc) > hueThreshold)
			{
				// failed hue check, blank out the location and try again
				cv::rectangle(searchResult, minLoc, cv::Point(minLoc.x + marker.Image.cols, minLoc.y + marker.Image.rows), cv::Scalar(100), -1);

				if (!m_debugOutputDirectory.empty())
				{
					cv::drawMarker(debugOutput, minLoc, cv::Scalar(0, 0, 0), cv::MARKER_TILTED_CROSS, 20, 2);
				}
				continue;
			}
		}

		// all checks passed. set result and break;
		markerPosition = Point(minLoc.x, minLoc.y);
		break;
	}

	if (!m_debugOutputDirectory.empty() && s_LogString != nullptr)
	{
		s_LogString(LogLevel::Info, std::format(L"Marker {} best match {} found at ({},{})", id, minVal, minLoc.x, minLoc.y).c_str());
		
		auto white = cv::Scalar(255, 255, 255);
		cv::drawMarker(debugOutput, minLoc, white, cv::MARKER_CROSS, 20, 3);
		cv::putText(debugOutput, std::format("{}: {}", id, minVal).c_str(), cv::Point(0, marker.Image.rows + 30), cv::FONT_HERSHEY_PLAIN, 0.8, white);

		cv::Rect roi(cv::Point(0, 0), marker.Image.size());
		marker.Image.copyTo(debugOutput(roi));

		WriteDebugImage(debugOutput);
	}

	return markerPosition;
}

double MarkerFindSession::GetMeanHueDifference(Marker const& marker, cv::Point const& location)
{
	cv::Rect roi(location, marker.Image.size());
	cv::Mat region = m_originalScaledImage(roi);

	cv::Mat converted;
	cv::cvtColor(region, converted, cv::COLOR_BGR2HSV);
	cv::Mat hsv[3];
	cv::split(converted, hsv);
	auto imageMeanHueValue = cv::mean(hsv[0])[0];
	auto markerMeanHueValue = marker.MeanHue[0];

	double largerValue, smallerValue;
	if (imageMeanHueValue > markerMeanHueValue)
	{
		largerValue = imageMeanHueValue;
		smallerValue = markerMeanHueValue;
	}
	else
	{
		largerValue = markerMeanHueValue;
		smallerValue = imageMeanHueValue;
	}

	double difference = largerValue - smallerValue;
	if (difference > 256 / 2)
	{
		difference = 256 - difference;
	}

	if (s_LogString != nullptr)
	{
		s_LogString(LogLevel::Info, std::format(L"Hue check at ({},{}) image: {}, marker: {}, diff: {}", location.x, location.y, imageMeanHueValue, markerMeanHueValue, difference).c_str());
	}

	return difference;
}

void MarkerFindSession::WriteDebugImage(cv::Mat image)
{
	auto now = std::chrono::system_clock::now();
	auto msSinceEpoch = std::chrono::duration_cast<std::chrono::milliseconds>(now.time_since_epoch()).count();
	auto path = m_debugOutputDirectory / (std::to_wstring(msSinceEpoch) + L".png");

	std::vector<byte> imageContent;
	cv::imencode(".png", image, imageContent);

	try
	{
		// Best effort. Don't do bad things if we can't write
		wil::unique_handle file(CreateFile(path.c_str(), GENERIC_WRITE, 0, nullptr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr));
		THROW_LAST_ERROR_IF(!file);

		DWORD written = 0;
		THROW_IF_WIN32_BOOL_FALSE(WriteFile(file.get(), imageContent.data(), imageContent.size(), &written, nullptr));
	}
	CATCH_LOG();
}

MarkerFinder::MarkerFinder()
{
}

void MarkerFinder::AddMarker(uint32_t id, std::wstring const& path, std::wstring const& maskPath, SearchMode mode)
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

std::unique_ptr<MarkerFindSession> MarkerFinder::CreateSessionForImage(cv::Mat image, double scale, std::wstring const& debugOutputDirectory)
{
	std::map<uint32_t, Marker> markers;
	for (auto const& kv : m_markerInfos)
	{
		markers[kv.first] = LoadMarkerFromFile(kv.second, scale);
	}

	return std::make_unique<MarkerFindSession>(markers, image, scale, debugOutputDirectory);
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

	if (marker.Mode == SearchMode::ToGrayWithMeanHueCheck)
	{
		cv::resize(resizedImage, resizedImage, cv::Size(), additionalScale, additionalScale, cv::INTER_LINEAR);
		cv::Mat converted;
		cv::cvtColor(resizedImage, converted, cv::COLOR_BGR2HSV);
		cv::Mat hsv[3];
		cv::split(converted, hsv);
		marker.MeanHue = cv::mean(hsv[0]);
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

std::vector<byte> MarkerFinder::ReadFileContents(std::wstring const& path)
{
	wil::unique_handle file(CreateFile(path.c_str(), GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, 0, nullptr));
	THROW_LAST_ERROR_IF(!file);

	DWORD size = GetFileSize(file.get(), nullptr);
	DWORD bytesRead = 0;
	std::vector<byte> buffer(size);
	THROW_IF_WIN32_BOOL_FALSE(ReadFile(file.get(), buffer.data(), size, nullptr, nullptr));

	return buffer;
}
