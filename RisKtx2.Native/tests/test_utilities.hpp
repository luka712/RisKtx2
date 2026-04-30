//
// Created by lukaa on 26.4.2026..
//

#ifndef TEST_UTILITIES_H
#define TEST_UTILITIES_H

#define TEST_PNG "test_files/test.png"
#define TEST_KTX_BASIS_UASTC "test_files/test_basis_uastc.ktx2"
#define TEST_OUTPUT_KTX "test_files/output_basis_uastc.ktx2"

//! Loads a PNG image from disk and returns a pointer to the pixel data.
// The caller is responsible for freeing the returned data using stbi_image_free.
// @param width Output parameter for the image width.
// @param height Output parameter for the image height.
// @param channels Output parameter for the number of color channels in the image.
unsigned char* loadTestPng(int* width, int* height, int* channels);

//! Frees the memory allocated for the PNG image data.
void freeTestPng(unsigned char* data);


#endif //TEST_UTILITIES_H
