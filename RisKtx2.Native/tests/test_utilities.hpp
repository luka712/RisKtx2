//
// Created by lukaa on 26.4.2026..
//

#ifndef TEST_UTILITIES_H
#define TEST_UTILITIES_H

#define TEST_PNG "test_files/test.png"
#define TEST_KTX_BASIS_UASTC "test_files/test_basis_uastc.ktx2"
#define TEST_OUTPUT_KTX "test_files/output_basis_uastc.ktx2"
#define TEST_OUTPUST_ASTC "test_files/output_astc.ktx2"

// TODO: doc comment
unsigned char* loadTestPng(int* width, int* height, int* channels, bool vertical_flip = false);

// TODO: doc comment
unsigned char* generateMipLevel(unsigned char* input_data, int base_width, int base_height, int channels, int level, int* out_width, int* out_height);

//! Frees the memory allocated for the PNG image data.
void freeTestPng(unsigned char* data);


#endif //TEST_UTILITIES_H
