#include"Common.h"
#include"CommonHeaders.h"

#ifndef WIN32_MEAN_AND_LEAN
#define WIN32_MEAN_AND_LEAN
#endif

#include<Windows.h>

namespace {
	HMODULE game_code_dll = nullptr;
}//anannymous namesapce

EDITOR_INTERFACE u32
LoadGameCodeDll(const char* dll_path)
{
	if (game_code_dll) return FALSE;
	game_code_dll = LoadLibraryA(dll_path); //ascii version not unicode as we are using char array
	assert(game_code_dll);

	return game_code_dll ? TRUE:FALSE;
}

EDITOR_INTERFACE u32
UnloadGameCodeDll()
{
	if (!game_code_dll) return FALSE;
	assert(game_code_dll);
	int result= FreeLibrary(game_code_dll);
	assert(result);
	game_code_dll = nullptr;
	return TRUE;
}


