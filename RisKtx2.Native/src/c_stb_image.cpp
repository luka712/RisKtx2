#include "c_stb_image.h"

#define STB_IMAGE_IMPLEMENTATION
#include "stb_image/stb_image.h"

#define STB_IMAGE_RESIZE_IMPLEMENTATION
#include "stb_image/stb_image_resize.h"

unsigned char* ris_stbi_load(const char* filename, int* width, int* height, int* channels, int desired_channels)
{
	return stbi_load(filename, width, height, channels, desired_channels);
}

int ris_stbir_resize_uint8(const unsigned char* input_pixels, int input_w, int input_h, int input_stride_in_bytes,
	unsigned char* output_pixels, int output_w, int output_h, int output_stride_in_bytes,
	int num_channels)
{
	return stbir_resize_uint8(input_pixels, input_w, input_h, input_stride_in_bytes,
		output_pixels, output_w, output_h, output_stride_in_bytes,
		num_channels);
}

void ris_stbi_image_free(unsigned char* data)
{
	stbi_image_free(data);
}