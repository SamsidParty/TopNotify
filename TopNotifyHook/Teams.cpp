#include "Teams.h"
#include <easyhook.h>
#include <mmsystem.h>

void Teams::SetupHooks() {
	HOOK_TRACE_INFO posHook = { NULL };

	NTSTATUS result = LhInstallHook(GetProcAddress(GetModuleHandle(TEXT("user32")), "SetWindowPos"), SetWindowPosHook, NULL, &posHook);

	if (!NT_SUCCESS(result)) {
		//Display Error Message
		MessageBoxW(NULL, L"Failed To Setup Hooks For Microsoft Teams, TopNotify Won't Intercept Teams Notifications", L"TopNotify", 0);
		return;
	}

	ULONG ACLEntries[1] = { 0 };
	LhSetExclusiveACL(ACLEntries, 1, &posHook);
}

BOOL Teams::SetWindowPosHook(HWND hWnd, HWND hWndInsertAfter, int x, int y, int cx, int cy, UINT uFlags) {

	//372PX Seems To Be The Standard Width Of Teams Notifications
	//So Check If It's 372PX To Determine Whether This Window Is A Notification Window
	if (cx == 372) {
		//PlaySoundW(L"C:\\Users\\SamarthCat\\Documents\\Programming Stuff\\TopNotify\\TopNotify.WWW\\topnotify\\public\\Audio\\fortnite\\levelup_c5s1.wav", NULL, SND_FILENAME | SND_ASYNC);

		return SetWindowPos(hWnd, hWndInsertAfter, 15, 15, cx, cy, uFlags);
	}
	else {
		return SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags);
	}
}