#include "pch.h"
#include "GalleryData.h"


GalleryData::GalleryData()
{
}

GalleryData::GalleryData(int index, int image_count, char* gallery_prefix[3], const wchar_t* thumbnail)
	:index(index), image_count(image_count)
{
	*this->gallery_prefix = *gallery_prefix;
	this->thumbnail = thumbnail;
}

int LoadGalleryFile(const char* fileName)
{
	std::ofstream out_file;
	out_file.open("log.txt");
	out_file << "Hello world\n";
	out_file.close();

	try
	{
		// Returns number of loaded galleries
		std::fstream gallery_file;
		gallery_file.open(fileName, std::ios::in | std::ios::binary);

		if (!gallery_file.is_open())
			throw std::runtime_error("Specified file name does not exsist");

		char* mem_read = new char[8];

		gallery_file.read(mem_read, 1);
		const int gallery_count = (int)mem_read[0];

		GalleryData* outputArray = new GalleryData[gallery_count];

		for (int i = 0; i < gallery_count; i++)
		{
			gallery_file.read(mem_read, 1);
			outputArray[i].index = (int)mem_read[0];

			gallery_file.read(mem_read, 2);
			outputArray[i].image_count = (int)(mem_read[0] << 8 + mem_read[1]);

			gallery_file.read(mem_read, 3);
			memmove(&outputArray[i].gallery_prefix, &mem_read, 3);

			std::wstring thumbnail;
			gallery_file.read(mem_read, 2);

			while (mem_read[0] != 0x00)
			{
				thumbnail.append(((wchar_t*)(mem_read[0] << 8) + mem_read[1]));
				gallery_file.read(mem_read, 2);
			}

			//Read separator
			if (i != (gallery_count - 1))
				gallery_file.read(mem_read, 4);
		}

		gallery_file.close();
		return gallery_count;
	}
	catch (std::exception exception)
	{
		std::ofstream out_file;
		out_file.open("log.txt");
		
		out_file << exception.what() << std::endl;
		out_file.close();

		throw exception;
	}
}
