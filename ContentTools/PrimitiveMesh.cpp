#include"PrimitivesMesh.h"
#include"Geometry.h"

namespace tge::tools {
	namespace
	{

		using primitive_mesh_creator = void(*)(scene&, const primitive_init_info&);

		void create_plane(scene& scene, const primitive_init_info& info);
		void create_cube(scene& scene, const primitive_init_info& info);
		void create_uv_sphere(scene& scene, const primitive_init_info& info);
		void create_ico_sphere(scene& scene, const primitive_init_info& info);
		void create_cylinder(scene& scene, const primitive_init_info& info);
		void create_capsule(scene& scene, const primitive_init_info& info);

		primitive_mesh_creator creators[]
		{
			create_plane,
			create_cube,
			create_uv_sphere,
			create_ico_sphere,
			create_cylinder,
			create_capsule
		};

		static_assert(_countof(creators) == primitive_mesh_type::count);

		struct axis {
			enum : u32 {
				x = 0,
				y = 1,
				z = 2
			};
		};

		//horizontal index and vertical index give us the plane in which the plane is created x=0,y=1,z=2 by default we use xz plane
		//flip winding tells which direction a triangle should face(normal direction)
		// offset is the center of the plane
		// u and v range are used when we want to use a subset of uv coordinate  
		mesh create_plane(const primitive_init_info& info, u32 horizontal_index = axis::x, u32 vertical_index = axis::z, bool flip_winding = false,
			math::v3 offset = { -0.5f,0.f,-0.5f }, math::v2 u_range = { 0.f,0.f }, math::v2 v_range = { 0.f,0.f })
		{
			assert(horizontal_index < 3 && vertical_index < 3);
			assert(horizontal_index != vertical_index);

			const u32 horizontal_count = tge::math::clamp(info.segments[horizontal_index], 1u, 10u);
			const u32 vertical_count = tge::math::clamp(info.segments[vertical_index], 1u, 10u);
			const f32 horizontal_step = 1.f / horizontal_count;
			const f32 vertical_step = 1.f / vertical_count;
			const f32 u_step = (u_range.y - u_range.x) / horizontal_count;
			const f32 v_step = (v_range.y - v_range.x) / vertical_count;

			mesh m{};
			utl::vector<math::v2> uvs;

			for (u32 j = 0; j <= vertical_count; ++j)
			{
				for (u32 i = 0; i <= horizontal_count; ++i)
				{
					math::v3 positions = offset;
					f32* const as_aarry = &positions.x;//pointer to x of positions

					as_aarry[horizontal_index] += i * horizontal_step;//plane of the object 0,1,2 => x,y,z see comment above declaration
					as_aarry[vertical_index] += j * vertical_step;
					m.positions.emplace_back(positions.x * info.size.x, positions.y * info.size.y, positions.z * info.size.z);

					//math::v2 uv = { u_range.x ,1.f - v_range.x };
					//uv.x += i * u_step;
					//uv.y -= j * v_step;
					math::v2 uv = { 0,1.f };
					uv.x += (i % 2);
					uv.y -= (j % 2);
					uvs.emplace_back(uv);
				}
			}

			const u32 row_length = horizontal_count + 1;//no of vertices in a row
			const u32 coloumn_length = vertical_count + 1;//no of vertices in a row
			assert(m.positions.size() == row_length * coloumn_length);

			u32 k = 0;
			for (u32 j = 0; j < vertical_count; j++)
			{
				for (u32 i = k; i < horizontal_count; i++)
				{
					const u32 index[4] =
					{
						i + j * row_length, //0
						i + (j + 1) * row_length,//5
						(i + 1) + j * row_length,//1
						(i + 1) + (j + 1) * row_length,//6
					};
					m.raw_indices.emplace_back(index[0]);
					m.raw_indices.emplace_back(index[flip_winding ? 2 : 1]);
					m.raw_indices.emplace_back(index[flip_winding ? 1 : 2]);

					m.raw_indices.emplace_back(index[2]);
					m.raw_indices.emplace_back(index[flip_winding ? 3 : 1]);
					m.raw_indices.emplace_back(index[flip_winding ? 1 : 3]);
				}
				++k;
			}

			const u32 num_indices = 3 * 2 * (horizontal_count) * (vertical_count);
			assert(m.raw_indices.size() == num_indices);

			m.uv_sets.resize(1);

			for (u32 i = 0; i < num_indices; i++)
			{
				m.uv_sets[0].emplace_back(uvs[m.raw_indices[i]]);
			}

			return m;
		}

		mesh create_uv_sphere(const primitive_init_info& info)
		{
			const u32 phi_count = math::clamp(info.segments[axis::x],3u,64u);
			const u32 theta_count = math::clamp(info.segments[axis::y],2u,64u);

			const f32 theta_step = math::pi / theta_count;
			const f32 phi_step = math::two_pi / phi_count;

			const u32 num_indices = 2 * 3 * phi_count + 2 * 3 * phi_count * (theta_count - 2);// top circle and bottom circle then middle protion
			const u32 num_vertices = 2 + phi_count * (theta_count - 1);

			mesh m{};

			m.name = "uv_sphere";
			m.positions.resize(num_vertices);

			u32 c = 0;
			m.positions[c++] = { 0.f,info.size.y,0.f };

			for (u32 j = 1; j <= (theta_count - 1); ++j)
			{
				const f32 theta = j * theta_step;
				for (u32 i = 0; i < phi_count; ++i)
				{
					const f32 phi{ i * phi_step };
					m.positions[c++] = {
						info.size.x * DirectX::XMScalarSin(theta) * DirectX::XMScalarCos(phi),
						info.size.y * DirectX::XMScalarCos(theta),
						-info.size.z * DirectX::XMScalarSin(theta) * DirectX::XMScalarSin(phi),
					};
				}
			}

			//add bottom vertex
			m.positions[c++] = { 0.f, -info.size.y, 0.f };
			assert(num_vertices == c);
			
			c = 0;
			m.raw_indices.resize(num_indices);

			//top cap
			for (u32 i = 0; i < phi_count - 1; i++)
			{
				m.raw_indices[c++] = 0;
				m.raw_indices[c++] = i + 1;
				m.raw_indices[c++] = i + 2;
			}

			m.raw_indices[c++] = 0;
			m.raw_indices[c++] = phi_count;
			m.raw_indices[c++] = 1;

			//indices for section between
			for (u32 j = 0; j < (theta_count - 2); ++j)
			{
				for (u32 i = 0; i < (phi_count - 1); ++i)
				{
					const u32 index[4] = {
						1 + i + j * phi_count,
						1 + i + (j + 1) * phi_count,
						1 + (i + 1) + (j + 1) * phi_count,
						1 + (i + 1) + j * phi_count
					};

					m.raw_indices[c++] = index[0];
					m.raw_indices[c++] = index[1];
					m.raw_indices[c++] = index[2];

					m.raw_indices[c++] = index[0];
					m.raw_indices[c++] = index[2];
					m.raw_indices[c++] = index[3];
				}

				const u32 index[4]{
					phi_count + j * phi_count,
					phi_count + (j + 1) * phi_count,
					1 + (j + 1) * phi_count,
					1 + j * phi_count
				};

				m.raw_indices[c++] = index[0];
				m.raw_indices[c++] = index[1];
				m.raw_indices[c++] = index[2];

				m.raw_indices[c++] = index[0];
				m.raw_indices[c++] = index[2];
				m.raw_indices[c++] = index[3];
			}

			//bottom cap
			const u32 south_pole_index = (u32)m.positions.size() - 1;
			for (u32 i = 0; i < (phi_count - 1); ++i)
			{
				m.raw_indices[c++] = south_pole_index;
				m.raw_indices[c++] = south_pole_index - phi_count + i + 1;
				m.raw_indices[c++] = south_pole_index - phi_count + i;
			}

			m.raw_indices[c++] = south_pole_index;
			m.raw_indices[c++] = south_pole_index - phi_count;
			m.raw_indices[c++] = south_pole_index - 1;

			m.uv_sets.resize(1);
			m.uv_sets[0].resize(m.indices.size());

			return m;
		}

		void create_plane(scene& scene, const primitive_init_info& info)
		{
			lod_group lod{};
			lod.name = "plane";
			lod.meshes.emplace_back(create_plane(info));
			scene.lod_group.emplace_back(lod);
		}

		void create_cube(scene& scene, const primitive_init_info& info)
		{

		}

		void create_uv_sphere(scene& scene, const primitive_init_info& info)
		{
			lod_group lod{};
			lod.name = "uv_sphere";
			lod.meshes.emplace_back(create_uv_sphere(info));
			scene.lod_group.emplace_back(lod);
		}

		void create_ico_sphere(scene& scene, const primitive_init_info& info)
		{

		}

		void create_cylinder(scene& scene, const primitive_init_info& info)
		{

		}

		void create_capsule(scene& scene, const primitive_init_info& info)
		{

		}
	}


	EDITOR_INTERFACE
	void CreatePrimitiveMesh(scene_data* data, primitive_init_info* info)
	{
		assert(data && info);
		assert(info->type < primitive_mesh_type::count);
		scene scene{};

		creators[info->type](scene, *info);
	
		data->settings.calculate_normals = 1;

		process_scene(scene, data->settings);
		pack_data(scene, *data);
	}

}