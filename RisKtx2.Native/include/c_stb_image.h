#include "macros.hpp"

extern "C" {

	API_EXPORT
		unsigned char* ris_stbi_load(const char* filename, int* width, int* height, int* channels, int desired_channels);

	API_EXPORT
		void ris_stbi_image_free(unsigned char* data);

}