#pragma comment(lib, "GameEngine.lib")

#define TEST_ENTITY_COMPONENTS 1

#if TEST_ENTITY_COMPONENTS
#include"TestEntityCOmponents.h"
#define current_test engine_test
#else
#error One of the test needs to be enabled
#endif


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