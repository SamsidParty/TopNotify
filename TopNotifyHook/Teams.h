#pragma once

#include <Windows.h>

class Teams
{
public:
	static void SetupHooks();
	static BOOL SetWindowPosHook(HWND hWnd, HWND hWndInsertAfter, int x, int y, int cx, int cy, UINT uFlags);
};

