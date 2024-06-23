#include "Settings.h"

void GlobalSettings::SetSettings(Settings* newSettings) {

	if (GlobalSettings::LoadedSettingsFile != nullptr) {
		free(GlobalSettings::LoadedSettingsFile);
	}

	GlobalSettings::LoadedSettingsFile = newSettings;
}

Settings* GlobalSettings::LoadedSettingsFile = nullptr;