#pragma once

#include"../Components/ComponentsCommon.h"
#include"TransformComponent.h"
#include"ScriptComponent.h"

namespace tge {
	namespace game_entity{
		DEFINE_TYPED_ID(entity_id);

		class entity {
		public:
			constexpr explicit entity(entity_id id) : _id(id) {}
			constexpr entity() : _id(id::invalid_id) {}
			constexpr entity_id get_id() const { return _id; }
			constexpr bool is_valid() const { return id::isvalid(_id); }

			transform::component transform() const;
			script::component script() const;
		private:
			entity_id _id;
		};
	}

	namespace script {
		class entity_script : public game_entity::entity
		{
		public:
			virtual ~entity_script() = default;
			virtual void begin_play() {};
			virtual void update(float) {};
		protected:
			constexpr explicit entity_script(game_entity::entity entity)
				: game_entity::entity(entity.get_id())
			{}
		};

		namespace detail
		{
			using script_ptr = std::unique_ptr<entity_script>;
			using script_creator = script_ptr(*)(game_entity::entity entity);//pointer to a function that take entity as argument and returns a script pointer
			using string_hash = std::hash<std::string>;
			u8 register_script(size_t, script_creator);
#ifdef USE_WITH_EDITOR
			extern "C" _declspec(dllexport)
#endif // 

			script_creator get_script_creator(size_t tag);

			//function pointer to create a new script ,returns unique pointer of script_ptr
			template<class script_class>
			script_ptr create_script(game_entity::entity entity)
			{
				assert(entity.is_valid());
				return std::make_unique<script_class>(entity); 
				//dynamically allocates a new script_class with constructor arguments script_class
				// and returns unique_pointer<script_class>
			}

#ifdef USE_WITH_EDITOR
			u8 add_script_name(const char* name);

#define REGISTER_SCRIPT(TYPE)\
	namespace {\
	u8 _reg_##TYPE = \
	tge::script::detail::register_script\
	(tge::script::detail::string_hash()(#TYPE), &tge::script::detail::create_script<TYPE>);\
	const u8 _name_##TYPE\
	{tge::script::detail::add_script_name(#TYPE)};\
	}\

#else
			//forward declaration of class A
			// inside the ananymous namespace we have a variable _reg_##Type type is concatenated to _reg_ and calls register script 
#define REGISTER_SCRIPT(TYPE)\
	namespace {\
	u8 _reg_##TYPE = \
	tge::script::detail::register_script\
	(tge::script::detail::string_hash()(#TYPE), &tge::script::detail::create_script<TYPE>);\
	}\

#endif //USE_WITH_EDITOR
		}
	}
};
