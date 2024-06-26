#include"Common.h"
#include"CommonHeaders.h"
#include"..\GameEngine\Components\Script.h"
#include"..\Platform\PlatformTypes.h"
#include"..\Platform\Platform.h"
#include"..\Graphics\Renderer.h"

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#include<Windows.h>

namespace {
	HMODULE game_code_dll = nullptr;
	using _get_script_creator = tge::script::detail::script_creator(*)(size_t);
	_get_script_creator get_script_creator{ nullptr };
	using _get_script_names = LPSAFEARRAY(*)(void);
	_get_script_names get_script_names{ nullptr };
	tge::utl::vector<tge::graphics::render_surface> surfaces;
}//anannymous namesapce

EDITOR_INTERFACE 
u32 LoadGameCodeDll(const char* dll_path)
{
	if (game_code_dll) return FALSE;
	game_code_dll = LoadLibraryA(dll_path); //ascii version not unicode as we are using char array
	assert(game_code_dll);

	get_script_names = (_get_script_names)GetProcAddress(game_code_dll, "get_script_names");
	get_script_creator = (_get_script_creator)GetProcAddress(game_code_dll, "get_script_creator");

	return (game_code_dll && get_script_names && get_script_creator) ? TRUE:FALSE;
}

EDITOR_INTERFACE 
u32 UnloadGameCodeDll()
{
	if (!game_code_dll) return FALSE;
	assert(game_code_dll);
	int result= FreeLibrary(game_code_dll);
	assert(result);
	game_code_dll = nullptr;
	return TRUE;
}

EDITOR_INTERFACE
tge::script::detail::script_creator
GetScriptCreator(const char* name)
{
	return (game_code_dll && get_script_creator) ? get_script_creator(tge::script::detail::string_hash()(name)) : nullptr;
}

EDITOR_INTERFACE
LPSAFEARRAY GetScriptNames()
{
	return (game_code_dll && get_script_names) ? get_script_names() : nullptr;
}

EDITOR_INTERFACE
u32 CreateRenderSurface(HWND host, s32 width, s32 height)
{
	assert(host);
	tge::platform::window_init_info info{ nullptr,host,nullptr,0,0,width,height };
	tge::graphics::render_surface surface{ tge::platform::create_window(&info),{ } };
	assert(surface.window.is_valid());
	surfaces.emplace_back(surface);
	return (u32)surfaces.size() - 1;
}

EDITOR_INTERFACE
void RemoveRenderSurface(u32 id)
{
	assert(id < surfaces.size());
	tge::platform::remove_window(surfaces[id].window.get_id());
}

EDITOR_INTERFACE
HWND GetWindowHandle(u32 id)
{
	assert(id < surfaces.size());
	return (HWND)surfaces[id].window.handle();
}

EDITOR_INTERFACE
void ResizeRenderSurface(u32 id)
{
	assert(id < surfaces.size());
	surfaces[id].window.resize(0,0);
}

