#include <easyhook.h>
#include <string>
#include <iostream>
#include <Windows.h>
#include <stdlib.h>  
#include "Teams.h"

extern "C" void __declspec(dllexport) __stdcall NativeInjectionEntryPoint(REMOTE_ENTRY_INFO * inRemoteInfo);

void __stdcall NativeInjectionEntryPoint(REMOTE_ENTRY_INFO* inRemoteInfo) {

	//Check If Already Hooked
	if (getenv("TOPNOTIFY_IS_HOOKED")) {
		//If So, Don't Load The Hooks
		return;
	}

	//Mark As Hooked
	_putenv("TOPNOTIFY_IS_HOOKED=1");

	Teams::SetupHooks();
}