#pragma once
#include"ToolsCommon.h"

namespace tge::tools {
	namespace packed_vertex {
		struct vertex_static
		{
			math::v3							position;
			u8									reserved[3];
			u8									t_sign;//bit 0, tangent handedness * (tangent.z sign), bit 1 normal.z sign (0 means -1, 1 means 1)
			u16									normal[2];
			u16									tangent[2];
			math::v2							uv;
		};
	}

	struct vertex {
		math::v4								tangent{};
		math::v3								positions{};
		math::v3								normal{};
		math::v2								uv{};
	};

	struct mesh {
		utl::vector<math::v3>					positions;
		utl::vector<math::v3>					normals;
		utl::vector<math::v4>					tangents;
		utl::vector < utl::vector<math::v2>>	uv_sets;//2d texture coordinates onto the surface of 3d objects

		utl::vector<u32>						raw_indices;

		// Intermidiate data
		utl::vector<vertex>						vertices;
		utl::vector<u32>						indices;

		// Output data
		std::string								name;
		utl::vector<packed_vertex::vertex_static> packed_vertices_static;
		f32										lod_thresdold = -1.f;
		u32										lod_id = u32_invalid_id;
	};

	struct lod_group
	{
		std::string								name;
		utl::vector<mesh>						meshes;
	};

	struct scene
	{
		std::string								name;
		utl::vector<lod_group>					lod_group;//level of detail LOD 0 is highest detail version LOD 1 is lower detailed with fewer polygons, LOG group manages these defferent versions
	};

	struct geometry_import_settings
	{
		f32										smoothing_angle;//maximum angle between edges that will be rendered smooth
		u8										calculate_normals;
		u8										calculate_tangents;
		u8										reverse_handedness;//left handed z axis goes into the screen, right handed z axis is out of the screen
		u8										import_embeded_textures;
		u8										import_animations;
	};
	
	struct scene_data
	{
		u8*										buffer;
		u32										buffer_size;
		geometry_import_settings				settings;
	};

	void process_scene(scene& scene, const geometry_import_settings& settings);
	void pack_data(const scene& scene, scene_data& data);
}