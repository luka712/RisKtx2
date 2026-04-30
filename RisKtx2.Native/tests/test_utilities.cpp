
#include "c_stb_image.h"	
#include "test_utilities.hpp"


unsigned char* loadTestPng(int* width, int* height, int* channels)
{
	return ris_stbi_load(TEST_PNG, width, height, channels, 0);
}

void freeTestPng(unsigned char* data)
{
	ris_stbi_image_free(data);
}
