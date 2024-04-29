#pragma once
#include "CommonHeaders.h"

#ifdef _WIN64
#ifndef WIN32_MEAN_AND_LEAN
#define WIN32_MEAN_AND_LEAC
#endif
#include<Windows.h>

namespace tge::platform {
	using window_proc = LRESULT(*)(HWND, UINT, WPARAM, LPARAM); //function pointer to window procedure to handle messages
	using window_handle = HWND;

	struct window_init_info
	{
		window_proc callback{ nullptr };
		window_handle parent{ nullptr };
		const wchar_t* caption{ nullptr };
		s32 left{ 0 };
		s32 top{ 0 };
		s32 width{ 960 };
		s32 height{ 540 };
	};
}
#endif // _WIN64
