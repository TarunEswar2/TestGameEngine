#include"ContentLoader.h"
#include"..\Components\Entity.h"
#include"..\Components\Transform.h"
#include"..\Components\Script.h"

#if !defined(SHIPPING)

#include<fstream>
#include<filesystem>
#include<Windows.h>
#include<iostream>

namespace tge::content {
	namespace {
		enum component_type
		{
			transform,
			script,

			count
		};

		struct print_infos
		{
			std::string script_name;
			f32 position[3];
			f32 rotation[3];
			f32 scale[3];
		};

		utl::vector<print_infos> infos;
		utl::vector<game_entity::entity> entities;

		transform::init_info transform_info{};
		script::init_info script_info{};

		bool read_transform(const u8*& data, game_entity::entity_info& info, print_infos& print_info)
		{
			using namespace DirectX;
			f32 rotation[3];

			assert(!info.transform);//empty container
			memcpy(&transform_info.position[0], data, sizeof(transform_info.position)); data += sizeof(transform_info.position);
			memcpy(&rotation[0], data, sizeof(rotation)); data += sizeof(rotation);
			memcpy(&transform_info.scale[0], data, sizeof(transform_info.scale)); data += sizeof(transform_info.scale);

			XMFLOAT3A rot{ &rotation[0] };
			XMVECTOR quat = XMQuaternionRotationRollPitchYawFromVector(XMLoadFloat3A(&rot));
			XMFLOAT4A rot_quat{};
			XMStoreFloat4A(&rot_quat, quat);
			memcpy(&transform_info.rotation, &rot_quat.x, sizeof(transform_info.rotation));

			info.transform = &transform_info;
			print_info.position[0] = transform_info.position[0];
			print_info.position[1] = transform_info.position[1];
			print_info.position[2] = transform_info.position[2];

			print_info.rotation[0] = rotation[0];
			print_info.rotation[1] = rotation[1];
			print_info.rotation[2] = rotation[2];

			print_info.scale[0] = transform_info.scale[0];
			print_info.scale[1] = transform_info.scale[1];
			print_info.scale[2] = transform_info.scale[2];
			return true;
		}

		bool read_script(const u8*& data, game_entity::entity_info& info, print_infos& print_info)
		{
			assert(!info.script);
			const u32 name_length{ *data }; data += sizeof(u32);
			if (!name_length) return false;
			assert(name_length < 256);
			char script_name[256];
			memcpy(&script_name[0], data, name_length); data += name_length;

			script_name[name_length] = 0;
			script_info.script_creator = script::detail::get_script_creator(script::detail::string_hash()(script_name));

			info.script = &script_info;
			print_info.script_name = script_name;
			return script_info.script_creator != nullptr;
		}

		using component_reader = bool(*)(const u8*&, game_entity::entity_info&, print_infos&);
		component_reader component_readers[]
		{
			read_transform,
			read_script
		};
		static_assert(_countof(component_readers) == component_type::count);
	
		void print_entity_info()
		{
			// Allocate a console window
			AllocConsole();
			// Redirect cout to the console window
			freopen("CONOUT$", "w", stdout);

			int num_infos = infos.size();
			std::cout << "number of entities on active scene" << num_infos << std::endl;
			for (int i = 0; i < num_infos; ++i)
			{
				std::cout << "entity " << i << std::endl;
				auto info = infos[i];
				std::cout << "trasforms " << std::endl;
				std::cout << "positions " << info.position[0] << " " << info.position[1] << " " << info.position[2] << " " << std::endl;
				std::cout << "rotations " << info.rotation[0] << " " << info.rotation[1] << " " << info.rotation[2] << " " << std::endl;
				std::cout << "scales " << info.scale[0] << " " << info.scale[1] << " " << info.scale[2] << " " << std::endl;
				if (info.script_name.empty())std::cout << "No script" << std::endl;
				else std::cout << "Script name : " << info.script_name << std::endl;
				std::cout << std::endl;
			}
			FreeConsole();
		}
	}

	bool load_game()
	{
		//set the working directory to executable path
		wchar_t path[MAX_PATH];
		const u32 length = GetModuleFileName(0, &path[0], MAX_PATH);//returns path of executable
		if (!length || GetLastError() == ERROR_INSUFFICIENT_BUFFER) return false;
		std::filesystem::path p= path;
		SetCurrentDirectory(p.parent_path().wstring().c_str());

		//read game.bin and create entities
		std::ifstream game("game.bin", std::ios::in || std::ios::binary);
		utl::vector<u8> buffer(std::istreambuf_iterator<char>(game), {});
		assert(buffer.size());
		const u8* at= buffer.data();
		constexpr u32 su32 = sizeof(u32);
		const u32 num_entities = *at;at += su32;

		if (!num_entities) return false;

		for (u32 entity_index = 0; entity_index < num_entities; ++entity_index)
		{
			game_entity::entity_info info{};
			print_infos p_info;
			const u32 entity_type = *at; at += su32;
			const u32 num_components = *at; at += su32;
			if (!num_components) return false;

			for (u32 component_index = 0; component_index < num_components; component_index++)
			{
				const u32 component_type = *at; at += su32;
				assert(component_type < component_type::count);
				if (!component_readers[component_type](at, info, p_info)) return false;
			}

			assert(info.transform);
			game_entity::entity entity = game_entity::create(info);
			if (!entity.is_valid()) return false;
			entities.emplace_back(entity);
			infos.emplace_back(p_info);
		}

		assert(at == buffer.data() + buffer.size());

#ifdef _DEBUG
		print_entity_info();
#endif // 

		return true;
	}

	void unload_game()
	{
		for (auto entity : entities)
		{
			game_entity::remove(entity.get_id());
		}
	}
}
#endif