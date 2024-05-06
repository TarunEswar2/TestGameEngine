#include"Geometry.h"

namespace tge::tools {
	namespace {
		void recalculate_normals(mesh& m)
		{
			const u32 num_indices= (u32)m.raw_indices.size();
			m.normals.resize(num_indices);//we have a normal for each vertex of each triangle, the same for uv

			for (u32 i = 0; i < num_indices; ++i)
			{
				const u32 i0 = m.raw_indices[i];
				const u32 i1 = m.raw_indices[++i];
				const u32 i2 = m.raw_indices[++i]; 

				DirectX::XMVECTOR v0 = DirectX::XMLoadFloat3(&m.positions[i0]);
				DirectX::XMVECTOR v1 = DirectX::XMLoadFloat3(&m.positions[i1]);
				DirectX::XMVECTOR v2 = DirectX::XMLoadFloat3(&m.positions[i2]);
			
				DirectX::XMVECTOR e0 = DirectX::XMVectorSubtract(v1,v0);
				DirectX::XMVECTOR e1 = DirectX::XMVectorSubtract(v2,v0);

				DirectX::XMVECTOR n = DirectX::XMVector3Normalize(DirectX::XMVector3Cross(e0, e1));//normalize will make the length 1 leaving only direction
				DirectX::XMStoreFloat3(&m.normals[i], n);

				m.normals[i - 1] = m.normals[i];
				m.normals[i - 2] = m.normals[i];
			}


		}

		void process_normals(mesh& m, f32 smoothing_angle)
		{
			const f32 cos_alpha = DirectX::XMScalarCos(math::pi - (smoothing_angle * math::pi/180.f ));//smoothing angle is angle between faces but this is angle between normal so we use this
			const bool is_hard_edge = DirectX::XMScalarNearEqual(smoothing_angle, 180.f, math::epsilon);//convention 180
			const bool is_soft_edge = DirectX::XMScalarNearEqual(smoothing_angle, 0.f, math::epsilon);

			const u32 num_indices = (u32) m.raw_indices.size();
			const u32 num_vertices = (u32)m.positions.size();
			assert(num_indices && num_vertices);

			m.indices.resize(num_indices);

			utl::vector<utl::vector<u32>> idx_ref(num_vertices);//keeps track of all poitions where the same vertex is seen
			for (u32 i = 0; i < num_indices; ++i)
				idx_ref[m.raw_indices[i]].emplace_back(i); //no of elements in idx is same as num_vertices, idx keeps track of position in raw_indices
			//size is not the same as it is container for a vector

			for (u32 i = 0; i < num_vertices; i++)
			{
				std::vector<u32> ref = idx_ref[i];
				u32 num_ref = (u32)ref.size();

				for (u32 j = 0; j < num_ref; ++j)
				{
					m.indices[ref[j]] = (u32)m.vertices.size();//position in vertice vector

					vertex& v = m.vertices.emplace_back();//reference to last element
					v.positions = m.positions[m.raw_indices[ref[j]]];//position of vertex

					DirectX::XMVECTOR n1 = DirectX::XMLoadFloat3(&m.normals[ref[j]]);//first normal

					if (!is_hard_edge)
					{
						for (u32 k = j + 1; k < num_ref; ++k)
						{
							f32 cos_theta = 0.f;
							DirectX::XMVECTOR n2(DirectX::XMLoadFloat3(&m.normals[ref[k]]));//next normal
							
							if (!is_soft_edge)
							{
								DirectX::XMStoreFloat(&cos_theta, DirectX::XMVectorMultiply(DirectX::XMVector3Dot(n1, n2),DirectX::XMVector3ReciprocalLength(n1)));//n1 and n2 have a length of 1
							}

							//smoothing
							if (is_soft_edge || cos_theta > cos_alpha)
							{
								n1 = DirectX::XMVectorAdd(n1,n2);
								m.indices[ref[k]] = m.indices[ref[j]];//indice will reference the other vertex

								ref.erase(ref.begin() + k);//removing this instance of vertice
								--num_ref;//reduce count as we check if the next normal has to be normalized
								--k;//increment in for loop is decremented
							}
						}
					}

					DirectX::XMStoreFloat3(&v.normal, DirectX::XMVector3Normalize(n1));//every non normlaized vertex will have a vertex
				}
			}
		}

		void process_uvs(mesh& m)
		{
			utl::vector<vertex> old_vertices;
			old_vertices.swap(m.vertices);
			utl::vector<u32> old_indices(m.indices.size());
			old_indices.swap(m.indices);

			const u32 num_vertices = (u32)old_vertices.size();
			const u32 num_indices = (u32)old_indices.size();
			assert(num_vertices && num_indices);

			utl::vector<utl::vector<u32>> idx_ref(num_vertices);
			
			//instances of the same vertex which is held by old_indices[i]
			for (u32 i = 0; i < num_indices; ++i)
				idx_ref[old_indices[i]].emplace_back(i);

			for (u32 i = 0; i < num_vertices; ++i)
			{
				auto& ref = idx_ref[i];
				u32 num_ref = (u32)ref.size();

				for (u32 j = 0; j < num_ref; ++j)
				{
					m.indices[ref[j]] = (u32)m.vertices.size();
					vertex& v = old_vertices[old_indices[j]];

					v.uv = m.uv_sets[0][ref[j]];
					m.vertices.emplace_back(v);

					for (u32 k{ j + 1 }; k < num_ref; ++k)
					{
						math::v2& uv1 = m.uv_sets[0][ref[k]];
						if(DirectX::XMScalarNearEqual(v.uv.x,uv1.x,math::epsilon) &&
							DirectX::XMScalarNearEqual(v.uv.y,uv1.y,math::epsilon));
						{
							m.indices[ref[k]] = m.indices[ref[j]];
							ref.erase(ref.begin() + k);
							--num_ref;
							--k;
						}
					}
				}
			}
		}

		void pack_vertices_static(mesh& m)
		{
			const u32 num_vertices{ (u32)m.vertices.size() };
			assert(num_vertices);

			for (u32 i = 0; i < num_vertices; ++i)
			{
				vertex& v = m.vertices[i];
				const u8 signs = (u8)((v.normal.z > 0.f) << 1);
				const u16 normal_x = (u16)math::pack_float<16>(v.normal.x, -1.f, 1.f);
				const u16 normal_y = (u16)math::pack_float<16>(v.normal.y, -1.f, 1.f);
			
				m.packed_vertices_static
					.emplace_back(packed_vertex::vertex_static{ v.positions, { 0,0,0 }, signs,
						{ normal_x,normal_y }, {},
						v.uv
						});
			}
		}

		void process_vertices(mesh& m, const geometry_import_settings& settings)
		{
			assert((m.raw_indices.size() % 3) == 0);
			
			if (settings.calculate_normals || m.normals.empty())
			{
				recalculate_normals(m);
			}

			process_normals(m, settings.smoothing_angle);//average the normals for soft edges
			
			if (m.uv_sets.empty())
			{
				process_uvs(m);
			}

			pack_vertices_static(m);
		}

		u64 get_mesh_size(const mesh& m)
		{
			const u64 num_vertices = m.vertices.size();
			const u64 vertex_buffer_size = sizeof(packed_vertex::vertex_static) * num_vertices;
			const u64 index_size = num_vertices < (1 << 16) ? sizeof(u16) : sizeof(u32);
			const u64 index_buffer_size = index_size * m.indices.size();
			constexpr u64 su32 = sizeof(u32);
			const u64 size =
				su32 + m.name.size() +	//mesh_name_length and mesh name string
				su32 +					//lod id(group all mesh in the same lod together)
				su32 +					//vertex size
				su32 +					//number of vertices
				su32 +					//index size(16 or 32 bits)
				su32 +					//number of indices
				sizeof(f32) +			//LOD threshold
				vertex_buffer_size +		//vertices data
				index_buffer_size		//indices data
				;

			return size;
		}

		u64 get_scene_size(const scene& scene)
		{
			constexpr u64 su32 = sizeof(u32);
			u64 size =
				su32 +						//name length
				scene.name.size() +			//scene name string
				su32						//number of LODs
				;

			for (auto& lod : scene.lod_group)
			{
				u64 lod_size =
					su32 + lod.name.size() +//Lod name length and LPD name string
					su32					//number of meshes in this LOD
					;

				for (auto& m : lod.meshes)
				{
					lod_size += get_mesh_size(m);
				}

				size += lod_size;
			}

			return size;
		}

		void pack_mesh_data(const mesh& m, u8* buffer, u64& at)
		{
			const u64 su32 = sizeof(u32);
			u32 s = 0;

			s = (u32)m.name.size();
			memcpy(&buffer[at], &s, su32); at += su32;
			memcpy(&buffer[at], m.name.c_str(), s); at += s;
			//lod id
			s = m.lod_id;
			memcpy(&buffer[at], &s, su32); at += su32;
			//vertex size
			constexpr u32 vertex_size = sizeof(packed_vertex::vertex_static);
			s = vertex_size;
			memcpy(&buffer[at], &s, su32); at += su32;
			//number of vertices
			const u32 num_vertices = (u32)m.vertices.size();
			s = num_vertices;
			memcpy(&buffer[at], &s, su32); at += su32;
			//index size (16 bit or 32 bit)
			const u32 index_size{ num_vertices < (1 << 16) ? sizeof(u16) : sizeof(u32) };
			s = index_size;
			memcpy(&buffer[at], &s, su32); at += su32;
			//number of indices
			const u32 num_indices = (u32)m.indices.size();
			s = num_indices;
			memcpy(&buffer[at], &s, su32); at += su32;
			// lod threshold
			memcpy(&buffer[at], &m.lod_thresdold, sizeof(f32)); at += sizeof(f32);
			//vertex data
			s = vertex_size * num_vertices;
			memcpy(&buffer[at], m.packed_vertices_static.data(), s); at += s;
			//index data
			s = index_size * num_indices;
			void* data = (void*)m.indices.data();
			utl::vector<u16> indices;
			if (index_size == sizeof(u16))
			{
				indices.resize(num_indices);
				for (u32 i = 0; i < num_indices; ++i) indices[i] = (u16)m.indices[i];
				data = (void*)indices.data();
			}
			memcpy(&buffer[at], data, s); at += s;
		}
	}

	void process_scene(scene& scene, const geometry_import_settings& settings)
	{
		for(auto& lod : scene.lod_group)
			for (auto& m : lod.meshes)
			{
				process_vertices(m, settings);
			}
	}

	void pack_data(const scene& scene, scene_data& data)
	{
		constexpr u64 su32 = sizeof(u32);
		const u64 scene_size = get_scene_size(scene);
		data.buffer_size = (u32)scene_size;
		data.buffer = (u8*)CoTaskMemAlloc(scene_size);//level editor is incharge of freeing the memory
		assert(data.buffer);

		u8* const buffer = data.buffer;
		u64 at = 0;
		u32 s = 0;

		//scene name
		s = (u32)scene.name.size();
		memcpy(&buffer[at], &s, su32); at += su32;
		memcpy(&buffer[at], scene.name.c_str(), s); at += s;
		//number of LODs
		s = (u32)scene.lod_group.size();
		memcpy(&buffer[at], &s, su32); at += su32;

		for (auto& lod : scene.lod_group)
		{
			//lod name
			s = (u32)lod.name.size();
			memcpy(&buffer[at], &s, su32); at += su32;
			memcpy(&buffer[at], lod.name.c_str(), s); at += s;
			//number of meshse in this LOD
			s = (u32)lod.meshes.size();
			memcpy(&buffer[at], &s, su32); at += su32;

			for (auto& m : lod.meshes)
			{
				pack_mesh_data(m, buffer, at);
			}
		}
		assert(scene_size == at);
	}
}