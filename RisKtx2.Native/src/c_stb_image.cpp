#include "c_stb_image.h"

#define STB_IMAGE_IMPLEMENTATION
#include "stb_image/stb_image.h"

unsigned char* ris_stbi_load(const char* filename, int* width, int* height, int* channels, int desired_channels)
{
	return stbi_load(filename, width, height, channels, desired_channels);
}

void ris_stbi_image_free(unsigned char* data)
{
	stbi_image_free(data);
}