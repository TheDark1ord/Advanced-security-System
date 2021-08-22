#pragma once
#include "pch.h"

#ifdef CDLLS_EXPORTS
	#define DLL_API __declspec(dllexport)
#else
	#define DLL_API __declspec(dllimport)
#endif

extern "C" DLL_API bool validate_pin(const char* pin);
extern "C" DLL_API int test(int input);