#pragma once
#include "pch.h"

struct GalleryData
{
	GalleryData();
	GalleryData(int index, int image_count,
		char* gallery_prefix[3], const wchar_t* thumbnail);

	int index;
	int image_count;

	char* gallery_prefix[3];
	std::wstring thumbnail;
};

extern "C" DLL_API int LoadGalleryFile(const char* fileName);