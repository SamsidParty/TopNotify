#pragma once

#include <string>

enum NotifyLocation {
	TopLeft,
	TopRight,
	BottomLeft,
	BottomRight,
	Custom
};

struct Settings
{
	NotifyLocation Location;
	float CustomPositionPercentX;
	float CustomPositionPercentY;
	std::string SoundPath;

	// Data Passed By C#, Not Part Of The ConfigFile
	int __ScreenWidth;
	int __ScreenHeight;
};

class GlobalSettings {
	public:
		static Settings* LoadedSettingsFile;
		static void SetSettings(Settings* newSettings);
};