#pragma once

//disacble warnings
#pragma warning(disable: 4538) 

//C/C++

#include<stdint.h>
#include<assert.h>
#include<typeinfo>
#include<memory>
#include<unordered_map>
#include<string>

#if defined(_WIN64)
#include <DirectXMath.h>
#endif

//common headers
#include"PrimitiveTypes.h"
#include"../Utility/Utility.h"
#include"../Utility/MathTypes.h"
