#pragma once
#include"Test.h"
#include"../GameEngine/Components/Entity.h"
#include"../GameEngine/Components/Transform.h"

#include<iostream>
#include<ctime>

using namespace tge;

class engine_test : public test
{
public:
	bool initialize() override 
	{ 
		srand((u32)time(nullptr));
		return true; 
	}

	void run() override 
	{ 
		int i = 1;
		do {
			std::cout << "Iteration : " << i << std::endl;
			for (u32 i = 0; i < 10000; ++i)
			{
				create_random();
				remove_random();
				_num_entites = (u32)_entities.size();
			}
			print_results();
			++i;
		} while (getchar() != 'q');
	}

	void shutdown() override 
	{ 
		
	}

private:
	utl::vector<game_entity::entity> _entities;

	u32 _added{ 0 };
	u32 _removed{ 0 };
	u32 _num_entites{ 0 };

	void create_random()
	{
		u32 count = rand() % 20;
		if (_entities.empty()) count = 1000;
		transform::init_info transform_info;
		game_entity::entity_info e_info{ &transform_info };

		while(count > 0)
		{
			++_added;
			game_entity::entity entity{ game_entity::create(e_info) };
			assert(entity.is_valid() && id::isvalid(entity.get_id()));
			_entities.push_back(entity);
			assert(game_entity::is_alive(entity.get_id()));
			--count;
		}
	}

	void remove_random()
	{
		u32 count = rand() % 20;
		if (_entities.size() < 1000) return;
		while (count > 0)
		{
			const u32 index = (u32)rand() % (u32)_entities.size();
			const game_entity::entity entity = _entities[index];
			assert(entity.is_valid() && id::isvalid(entity.get_id()));
			if (entity.is_valid())
			{
				++_removed;
				game_entity::remove(entity.get_id());
				_entities.erase(_entities.begin() + index);
				assert(!game_entity::is_alive(entity.get_id()));
			}
			--count;
		}
	}

	void print_results()
	{
		std::cout << "Entites created : " << _added << std::endl;
		std::cout << "Entites removed : " << _removed << std::endl;
		std::cout << "Current difference : " << _added - _removed << std::endl;
		std::cout << "Total Entites : " << _num_entites << std::endl;
	}
};
