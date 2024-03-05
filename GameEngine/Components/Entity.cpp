#include"Entity.h"
#include"Transform.h"
#include"Script.h"

namespace tge::game_entity{
	//anaymous namespace:: makes everything private tot his file and inaccessible outside the file
	//can also be done by using static
	namespace {
		utl::vector<transform::component> transforms;
		utl::vector<script::component> scripts;
		
		utl::vector<id::generation_type> generations;
		utl::deque<entity_id> free_ids;
	}//ananymous namescape

	entity
		create(entity_info info)
	{
		assert(info.transform); //all entites must have transform
		if (!info.transform) return entity(); // returns entity with invalid id

		entity_id id;

		if (free_ids.size() > id::min_deleted_elements)
		{
			id = free_ids.front();
			assert(!is_alive(id));
			free_ids.pop_front();
			id = entity_id{ id::new_generation(id)};
			++generations[id::index(id)];
		}
		else
		{
			id = entity_id{ (id::id_type)generations.size() };
			generations.push_back(0);

			//resize components
			transforms.emplace_back();
			scripts.emplace_back();
		}

		const entity new_entity(id);
		const id::id_type index{ id::index(id) };

		//tranform component
		assert(!transforms[index].is_valid());
		transforms[index] = transform::create(*info.transform, new_entity);
		if (!transforms[index].is_valid()) return entity();

		//script component
		if (info.script != nullptr && info.script->script_creator != nullptr)  
		{
			assert(!scripts[index].is_valid());
			scripts[index] = script::create(*info.script, new_entity);
			assert(scripts[index].is_valid());
		}
		
		return new_entity;
	}
	
	void remove(entity_id id)
	{
		const id::id_type index = id::index(id);
		assert(is_alive(id));
		transform::remove(transforms[index]);
		transforms[index] = transform::component();//setting transform index as invalid
		if (scripts[index].is_valid())
		{
			script::remove(scripts[index]);
			scripts[index] = {};//invalid ID
		}

		free_ids.push_back(id);
	}

	bool is_alive(entity_id id)
	{
		assert(id::isvalid(id));
		const id::id_type index= id::index(id);

		assert(index < generations.size());
		assert(generations[index] == id::generation(id));
		return generations[index] == id::generation(id) && transforms[index].is_valid();
	}

	transform::component game_entity::entity::transform() const
	{
		assert(is_alive(_id));
		const id::id_type index = id::index(_id);
		return transforms[index];

	}

	script::component entity::script() const
	{
		assert(is_alive(_id));
		const id::id_type index = id::index(_id);
		return scripts[index];
	}
}


