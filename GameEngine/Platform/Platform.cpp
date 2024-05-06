#include "Platform.h"
#include "PlatformTypes.h"

namespace tge::platform
{
#ifdef _WIN64
	namespace {
		//stored in an array with index windowid to access handler to the window
		struct window_info
		{
			HWND hwnd{ nullptr };
			RECT client_area = { 0,0,960,540 };
			RECT fullscreen_area = {};
			POINT top_left = { 0,0 };
			DWORD style = WS_VISIBLE;
			bool is_fullscreen = false;
			bool is_closed = false;
		};

		utl::vector<window_info> windows;
		utl::vector<u32> available_slots;

		u32 add_to_windows(window_info info)
		{
			u32 id = u32_invalid_id;
			if (available_slots.empty())
			{
				id = (id::id_type)windows.size();
				windows.emplace_back(info);
			}
			else
			{
				id = available_slots.back();
				available_slots.pop_back();
				assert(id != u32_invalid_id);
				windows[id] = info;
			}
			return id;
		}

		void remove_from_windows(u32 id)
		{
			assert(id < windows.size());
			available_slots.emplace_back(id);

		}

		window_info& get_from_id(window_id id)
		{
			assert(id < windows.size());
			assert(windows[id].hwnd);
			return windows[id];
		}

		window_info& get_from_hwnd(window_handle hwnd)
		{
			const window_id id = (window_id)GetWindowLongPtr(hwnd,GWLP_USERDATA);
			return get_from_id(id);
		}

		void resize_window(const window_info& info, RECT& area)
		{
			RECT window_rect = area;
			AdjustWindowRect(&window_rect, info.style, FALSE);

			const s32 width = window_rect.right - window_rect.left;
			const s32 height = window_rect.bottom - window_rect.top;

			MoveWindow(info.hwnd, info.top_left.x, info.top_left.y, width, height, true);
		}

		void set_window_fullscreen(window_id id, bool is_fullscreen)
		{
			window_info& info = get_from_id(id);
			if (info.is_fullscreen != is_fullscreen)
			{
				info.is_fullscreen = is_fullscreen;

				if (is_fullscreen)
				{
					GetClientRect(info.hwnd, &info.client_area);
					RECT rect;
					GetWindowRect(info.hwnd, &rect);
					info.top_left.x = rect.left;
					info.top_left.y = rect.top;
					SetWindowLongPtr(info.hwnd,GWL_STYLE,0);
					ShowWindow(info.hwnd, SW_MAXIMIZE);
				}
				else
				{
					SetWindowLongPtr(info.hwnd, GWL_STYLE, info.style);
					resize_window(info, info.client_area);
					ShowWindow(info.hwnd, SW_SHOWNORMAL);
				}
			}
		}

		bool is_window_fullscreen(window_id id)
		{
			return get_from_id(id).is_fullscreen;
		}

		//window_handle is an alias for HWND
		window_handle get_window_handle(window_id id)
		{
			return get_from_id(id).hwnd;
		}

		void set_window_caption(window_id id, const wchar_t* caption)
		{
			window_info& info = get_from_id(id);
			SetWindowText(info.hwnd, caption);
		}

		math::u32v4 get_window_size(window_id id)
		{
			window_info& info = get_from_id(id);
			RECT& area = info.is_fullscreen ? info.fullscreen_area : info.client_area;
			return { (u32)area.left, (u32)area.top, (u32)area.right, (u32)area.bottom };
		}

		void resize_window(window_id id, u32 width, u32 height)
		{
			window_info& info = get_from_id(id);

			if (info.style & WS_CHILD)
			{
				//when we host the window in the level editor we set style to child
				//the resize has laready taken place so we just change interanl data
				GetClientRect(info.hwnd, &info.client_area);
			}
			else
			{
				RECT& area = info.is_fullscreen ? info.fullscreen_area : info.client_area;
				area.bottom = area.top + height;
				area.right = area.left + width;

				resize_window(info, area);
			}
		}

		void change_color_window(window_id id, int r, int g, int b) {
			HWND hwnd = get_from_id(id).hwnd;

			// Create new custom brush
			HBRUSH hCustomBrush = CreateSolidBrush(RGB(r, g, b));
			assert(hCustomBrush != NULL);

			// Get current background brush and delete it
			HBRUSH hOldBrush = (HBRUSH)SetClassLongPtr(hwnd, GCLP_HBRBACKGROUND, (LONG_PTR)hCustomBrush);
			if (hOldBrush != NULL && hOldBrush != GetStockObject(NULL_BRUSH)) {
				DeleteObject(hOldBrush);
			}

			// Invalidate the window to trigger a repaint
			InvalidateRect(hwnd, NULL, TRUE);
		}



		bool is_window_closed(window_id id)
		{
			return get_from_id(id).is_closed;
		}


		LRESULT CALLBACK internal_window_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
		{
			window_info* info{ nullptr };
			switch (msg)
			{
			case WM_DESTROY:
				get_from_hwnd(hwnd).is_closed = true;
				break;
			case WM_EXITSIZEMOVE:
				info = &get_from_hwnd(hwnd);
				break;
			case WM_SIZE:
				if (wparam == SIZE_MAXIMIZED)
				{
					info = &get_from_hwnd(hwnd);
				}
				break;
			case WM_SYSCOMMAND:
				if (wparam == SC_RESTORE)
				{
					info = &get_from_hwnd(hwnd);
				}
				break;
			case WM_ERASEBKGND:
				return TRUE; // Prevent background erasing, since we handle it in WM_PAINT
			case WM_PAINT: {
				PAINTSTRUCT ps;
				HDC hdc = BeginPaint(hwnd, &ps);

				RECT clientRect;
				GetClientRect(hwnd, &clientRect);

				HBRUSH hbrBackground = (HBRUSH)GetClassLongPtr(hwnd, GCLP_HBRBACKGROUND);

				FillRect(hdc, &clientRect, hbrBackground);

				EndPaint(hwnd, &ps);
				break;
			}
			default:
				break;
			}

			if (info)
			{
				assert(info->hwnd);
				GetClientRect(info->hwnd, info->is_fullscreen ? &info->fullscreen_area : &info->client_area); //updates either fullscreen area or client area
			}

			LONG_PTR long_ptr(GetWindowLongPtr(hwnd, 0));
			return long_ptr ? ((window_proc)long_ptr)(hwnd,msg,wparam,lparam) : DefWindowProc(hwnd,msg,wparam,lparam);
		}
	}

	window create_window(const window_init_info* const init_info)
	{
		window_proc callback = init_info ? init_info->callback : nullptr;
		window_handle parent = init_info ? init_info->parent : nullptr;
		

		//setup a window class
		WNDCLASSEX wc;
		ZeroMemory(&wc, sizeof(wc));

		wc.cbSize = sizeof(WNDCLASSEX);
		wc.style = CS_HREDRAW | CS_VREDRAW; //redraw when resized horizontally and vertically
		wc.lpfnWndProc = internal_window_proc; // ptr to window_proc
		wc.cbClsExtra = 0;
		wc.cbWndExtra = callback ? sizeof(callback) : 0;//additional operations
		wc.hInstance = 0;
		wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
		wc.hCursor = LoadCursor(NULL,IDC_ARROW);
		wc.hbrBackground = CreateSolidBrush(RGB(init_info->r, init_info->g, init_info->b));//windows color
		wc.lpszMenuName = NULL;
		wc.lpszClassName = L"TgeWindow";
		wc.hIconSm = LoadIcon(NULL, IDI_APPLICATION);

		//Register the window class
		RegisterClassEx(&wc);

		window_info info{};
		info.client_area.right = (init_info && init_info->width) ? info.client_area.left + init_info->width : info.client_area.right;
		info.client_area.right = (init_info && init_info->height) ? info.client_area.top + init_info->height : info.client_area.bottom;

		info.style |= parent ? WS_CHILD : WS_OVERLAPPEDWINDOW;
		RECT rect = info.client_area;

		AdjustWindowRect(&rect, info.style, FALSE);

		const wchar_t* caption = (init_info && init_info->caption) ? init_info->caption : L"Tge game"; //wchar_t is for unicode not ascii
		const s32 left = (init_info) ? init_info->left : info.top_left.x;
		const s32 top = (init_info) ? init_info->top : info.top_left.y;
		const s32 width = rect.right - rect.left;
		const s32 height = rect.bottom - rect.top;
		

		//Create an instance of the window class
		info.hwnd = CreateWindowEx(
			0,                     //extened style
			wc.lpszClassName,      //window registered class name
			caption,				  //window instance title
			info.style,			  //window style
			left,				  //initial diemensions
			top,					  //
			width,				  //
			height,				  //
			parent,				  //handle to parent window
			NULL,				  //handle to menu
			NULL,				  //instance of this application
			NULL				  //extra creation parameters
		);

		if (info.hwnd)
		{
			DEBUG_OP(SetLastError(0));
			window_id id = (window_id)add_to_windows(info);//adding to windows
			SetWindowLongPtr(info.hwnd, GWLP_USERDATA, (LONG_PTR)id);//settig id

			if (callback) SetWindowLongPtr(info.hwnd, 0, (LONG_PTR)callback);
			assert(GetLastError() == 0);

			ShowWindow(info.hwnd, SW_SHOWNORMAL);
			UpdateWindow(info.hwnd);
			window created_window = window{ id };
			return created_window;
		}
		return {};
	}

	void tge::platform::remove_window(window_id id)
	{
		window_info& info = get_from_id(id);
		DestroyWindow(info.hwnd);
		remove_from_windows(id);
	}
#else
#error "must implement at least one platform"
#endif //_WIN64 

	//platform independent functions
	void  window::set_fullscreen(bool is_fullscreen) const
	{
		assert(is_valid());
		set_window_fullscreen(_id, is_fullscreen);
	}

	bool  window::is_fullscreen() const
	{
		assert(is_valid());
		return(is_window_fullscreen(_id));

	}

	void* window::handle() const
	{
		assert(is_valid());
		return get_window_handle(_id);
	}

	void  window::set_caption(const wchar_t* caption) const
	{
		assert(is_valid());
		set_window_caption(_id, caption);
	}

	math::u32v4 window::size() const
	{
		assert(is_valid());
		return get_window_size(_id);
	}

	void  window::resize(u32 width, u32 height) const
	{
		assert(is_valid());
		resize_window(_id, width, height);
	}

	u32 window::width() const
	{
		math::u32v4 s = size();
		return s.z - s.x;
	}

	u32 window::height() const
	{
		math::u32v4 s = size();
		return s.w - s.y;
	}

	bool  window::is_closed() const
	{
		assert(is_valid());

		return is_window_closed(_id);
	}

	void window::change_color(int red, int green, int blue)
	{
		assert(red < 256, green < 356, blue < 256);
		change_color_window(_id, red, green, blue);
	}
}


