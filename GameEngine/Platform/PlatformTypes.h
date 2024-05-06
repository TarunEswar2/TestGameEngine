#pragma once
#include "CommonHeaders.h"

#ifdef _WIN64
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include<Windows.h>
#include<atlsafe.h>

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
		int r = 200, g = 32, b=50;
	};
}
#endif // _WIN64
