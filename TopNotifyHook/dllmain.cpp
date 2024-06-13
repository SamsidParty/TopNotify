// dllmain.cpp : Defines the entry point for the DLL application.
#include <easyhook.h>
#include <string>
#include <iostream>
#include <Windows.h>
#include "Teams.h"

extern "C" void __declspec(dllexport) __stdcall NativeInjectionEntryPoint(REMOTE_ENTRY_INFO * inRemoteInfo);

void __stdcall NativeInjectionEntryPoint(REMOTE_ENTRY_INFO* inRemoteInfo) {
	Teams::SetupHooks();
}