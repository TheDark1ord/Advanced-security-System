#pragma once
#include "pch.h"

#ifdef CDLLS_EXPORTS
#define DLL_API __declspec(dllexport)
#else
#define DLL_API __declspec(dllimport)
#endif

class FileManager
{
public:
	FileManager(const char* root_folder = "Data")
	{

	}

private:

};