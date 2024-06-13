#include <Windows.h>
#include <easyhook.h>

extern "C" {
	__declspec(dllexport) const WCHAR* InjectIntoProcess(int processID, WCHAR* dllPath) {
		NTSTATUS result = RhInjectLibrary(processID, NULL, EASYHOOK_INJECT_DEFAULT, NULL, dllPath, NULL, 0);

		if (!NT_SUCCESS(result)) {
			return RtlGetLastErrorString();
		}

		return L"Success";
	}
}