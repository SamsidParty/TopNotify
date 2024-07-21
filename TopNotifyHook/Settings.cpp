#include "Settings.h"

void GlobalSettings::SetSettings(Settings* newSettings) {

	if (GlobalSettings::LoadedSettingsFile != nullptr) {
		free(GlobalSettings::LoadedSettingsFile);
	}

	GlobalSettings::LoadedSettingsFile = newSettings;
}

void GlobalSettings::SetHandleToReport(int newHandle) {
	GlobalSettings::HandleToReport = newHandle;
}

Settings* GlobalSettings::LoadedSettingsFile = nullptr;
int GlobalSettings::HandleToReport = 0;