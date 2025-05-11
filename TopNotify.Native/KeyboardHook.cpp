#include <Windows.h>

// This callback is sent to C# to tell it that the user pressed or released a key
// Only certain keys (like the alt key) will be sent
typedef void(__stdcall* KeyPressCallback)();
KeyPressCallback OnKeyUpdate;

HHOOK KeyboardHook;
LRESULT CALLBACK HandleKeyboardEvent(int nCode, WPARAM wParam, LPARAM lParam)
{
	KBDLLHOOKSTRUCT keyInfo = *(KBDLLHOOKSTRUCT*)lParam;
	bool doesAffectTopNotify = keyInfo.vkCode == VK_LMENU; // Currently only the alt key is handled

	LRESULT result = CallNextHookEx(KeyboardHook, nCode, wParam, lParam);

	if (doesAffectTopNotify) {
		OnKeyUpdate();
	}

	return result;
}

extern "C" {
	__declspec(dllexport) void TopNotifyRegisterKeyboardHook(KeyPressCallback onKeyUpdate) {
		OnKeyUpdate = onKeyUpdate;
		KeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, HandleKeyboardEvent, NULL, 0);
	}
}