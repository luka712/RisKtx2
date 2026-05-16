#pragma once

#include "macros.hpp"

#ifdef __cplusplus
extern "C" {
#endif

API_EXPORT
unsigned char *ris_stbi_load(const char *filename, int *width, int *height, int *channels, int desired_channels);

API_EXPORT
int ris_stbir_resize_uint8(const unsigned char *input_pixels, int input_w, int input_h, int input_stride_in_bytes,
                           unsigned char *output_pixels, int output_w, int output_h, int output_stride_in_bytes,
                           int num_channels);

API_EXPORT
void ris_stbi_image_free(unsigned char *data);

#ifdef __cplusplus
} // extern "C"
#endif
