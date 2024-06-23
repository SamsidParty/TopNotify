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
	int CustomPositionX;
	int CustomPositionY;
	std::string SoundPath;
};

class GlobalSettings {
	public:
		static Settings* LoadedSettingsFile;
		static void SetSettings(Settings* newSettings);
};