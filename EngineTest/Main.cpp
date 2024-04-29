#pragma comment(lib, "GameEngine.lib")

#define TEST_ENTITY_COMPONENTS 0
#define TEST_WINDOWS 1

#if TEST_ENTITY_COMPONENTS
#include"TestEntityCOmponents.h"
#define current_test engine_test
#elif TEST_WINDOWS
#include"TestWindows.h"
#define current_test window_test
#else
#error One of the test needs to be enabled
#endif

#ifdef _WIN64
#include<Windows.h>

int WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int)
{
#if _DEBUG
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif

	current_test test{};

	if (test.initialize())
	{
		MSG msg{};
		bool is_running = true;
		while (is_running)
		{
			//reads, removes and dispathces messages from msg queue untill there are no messages left
			while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE))
			{
				TranslateMessage(&msg);
				DispatchMessage(&msg);// passes messages to window procedure so we can handle them

				is_running &= (msg.message != WM_QUIT);
			}
			test.run();
		}
	}
	test.shutdown();
	return 0;
}
#else
int main()
{
#if _DEBUG
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif

	current_test test{};

	if (test.initialize())
	{
		test.run();
	}

	test.shutdown();
}

#endif // 

