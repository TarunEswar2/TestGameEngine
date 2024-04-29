#pragma once
#include"ComponentsCommon.h"

namespace tge::script {
	struct init_info
	{
		detail::script_creator script_creator;//pointer to create_script function
	};

	component create(init_info info, game_entity::entity e);
	void remove(component id);
	void update(float dt);
}