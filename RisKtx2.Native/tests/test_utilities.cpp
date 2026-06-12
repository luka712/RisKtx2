
#include "ris_stb_image.h"	
#include "test_utilities.hpp"


unsigned char* loadTestPng(int* width, int* height, int* channels, bool vertical_flip)
{
	ris_stbi_set_flip_vertically_on_load(vertical_flip);
	return ris_stbi_load(TEST_PNG, width, height, channels, 0);
}

unsigned char* generateMipLevel(unsigned char* input_data,
	int base_width, int base_height, int channels,
	int level,
	int* out_width, int* out_height)
{
	*out_width = base_width >> level;
	*out_height = base_height >> level;

	// Allocate memory for the mip level data.
	auto stride_in_bytes = base_width * channels;
	auto output_stride_in_bytes = (*out_width) * channels;
	unsigned char* mip_data = new unsigned char[(*out_width) * (*out_height) * channels];
	int result = ris_stbir_resize_uint8(
		input_data, base_width, base_height, stride_in_bytes,
		mip_data, *out_width, *out_height, output_stride_in_bytes,
		channels);

	if (result == 0) {
		delete[] mip_data;
		return nullptr; // Resize failed
	}

	return mip_data;
}

void freeTestPng(unsigned char* data)
{
	ris_stbi_image_free(data);
}
