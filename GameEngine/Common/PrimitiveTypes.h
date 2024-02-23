#pragma once

#include<stdint.h>

//unsigned int

using u64 = uint64_t;
using u32 = uint32_t;
using u16 = uint16_t;
using u8 = uint8_t; //255 =1111 1111= -1 so we use it to represent the invalid index value

//signed int
using s64 = int64_t;
using s32 = int32_t;
using s16 = int16_t;
using s8 = int8_t;

//invalid indexes
constexpr u64 u64_invalid_id( 0xffff'ffff'ffff'ffffui64 );
constexpr u64 u32_invalid_id( 0xffff'ffffui32);
constexpr u64 u16_invalid_id( 0xffffui16 );
constexpr u64 u8_invalid_id( 0xffui8 );

//float
using f32 = float;	