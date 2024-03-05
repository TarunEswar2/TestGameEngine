#pragma once
#include "CommonHeaders.h"

namespace tge::id
{
	using id_type = u32;

	namespace detail {
		constexpr u32 generation_bits{ 8 };
		constexpr u32 index_bits(sizeof(id_type) * 8 - generation_bits); //sizeof is in bytes
		constexpr id_type index_mask{ (id_type{1} << index_bits) - 1 };
		constexpr id_type generaton_mask{ (id_type{1} << generation_bits) - 1 };
	}//internal
	
	constexpr id_type invalid_id{ id_type(-1) }; //id_mask = invalid id
	constexpr u32 min_deleted_elements{ 1024 };


	using generation_type = std::conditional_t<detail::generation_bits <= 16, std::conditional_t<detail::generation_bits <= 8, u8, u16>, u32>;
	static_assert(sizeof(generation_type) * 8 >= detail::generation_bits); //prevents generation bits > 31
	static_assert(sizeof(id_type) - sizeof(generation_type) > 0);

	//max id
	constexpr bool isvalid(id_type id)
	{
		return id != invalid_id;
	}

	constexpr id_type index(id_type id)
	{
		id_type index{ id & detail::index_mask };
		assert(index != detail::index_mask);
		return id & detail::index_mask;
	}

	constexpr id_type generation(id_type id)
	{
		return (id >> detail::index_bits) & detail::generaton_mask;
	}

	constexpr id_type new_generation(id_type id)
	{
		const id_type gen = id::generation(id) + 1;
		assert(gen < ((id_type{ 1 } << detail::generation_bits) - 1));
		return id::index(id) | (gen << detail::index_bits);
	}

#if _DEBUG
	//additional debug info for 
	namespace internal {
		struct id_base
		{
			constexpr explicit id_base(id_type id)
				: _id(id)
			{

			}

			constexpr operator id_type() const
			{
				return _id;
			}


		private:
			id_type _id;
		};
	}

#define DEFINE_TYPED_ID(name) \
    struct name final : id::internal::id_base \
    { \
        constexpr explicit name(id::id_type id) : id_base(id) {} \
        constexpr name() : id_base(0) {} \
    };

#else
#define DEFINE_TYPED_ID(name) using name = id::id_type;

#endif
}