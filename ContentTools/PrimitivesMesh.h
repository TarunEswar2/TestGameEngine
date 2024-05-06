#pragma once
#include"ToolsCommon.h"

namespace tge::tools {
	enum primitive_mesh_type {
		plane,
		cube,
		us_sphere,
		ico_sphere,
		cylinder,
		capsule,

		count
	};

	struct primitive_init_info
	{
		primitive_mesh_type		type;
		u32						segments[3]{ 1,1,1 };//subdivisions
		math::v3				size{ 1,1,1 };
		u32						lod{ 0 };//level of detail
	};
}
