#include <catch2/catch_test_macros.hpp>
#include <c_ktx.hpp>
#include <iostream>
#include <filesystem>
#include <spdlog/spdlog.h>
#include "test_utilities.hpp"

TEST_CASE("ktx - Write universal basis",
              "[ris_ktx_texture_write_unviversal_basis_test]")
{
    int width, height, channels;
    auto data = loadTestPng(&width, &height, &channels);

    ris_ktxTextureCreateInfo createInfo = {};
    createInfo.baseWidth = width;
    createInfo.baseHeight = height;
    createInfo.vkFormat = VK_FORMAT_R8G8B8A8_UNORM;
	createInfo.numLevels = 1;
	createInfo.generateMipmaps = KTX_FALSE;

    ktxTexture2* texture = nullptr;
    ktxTextureCreateStorageEnum storageAllocation = KTX_TEXTURE_CREATE_ALLOC_STORAGE;
    auto errorCode = ris_ktxTexture2_Create(&createInfo, storageAllocation, &texture);

    REQUIRE(errorCode == KTX_SUCCESS);
    REQUIRE(texture != nullptr);

    errorCode = ris_ktxTexture2_SetImageFromMemory(texture, 0, 0, 0, data, width * height * channels);
	REQUIRE(errorCode == KTX_SUCCESS);

    ris_ktxBasisParams params = {};
    params.compressionLevel = KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL;

    errorCode = ris_ktxTexture2_CompressBasisEx(texture, &params);
    REQUIRE(errorCode == KTX_SUCCESS);

    errorCode = ris_ktxTexture2_WriteToNamedFile(texture, TEST_OUTPUT_KTX);
    ris_ktxTexture2_Destroy(texture);

    freeTestPng(data);
    REQUIRE(errorCode == KTX_SUCCESS);
}



TEST_CASE("ktx - Load universal basis and confirm properties",
          "[ris_ktx_texture_load_and_confirm_properties_test]")
{
    ktxTexture2* texture = nullptr;
    auto errorCode = ktxTexture2_CreateFromNamedFile(TEST_OUTPUT_KTX, KTX_TEXTURE_CREATE_NO_FLAGS, &texture);

    REQUIRE(errorCode == KTX_SUCCESS);
    REQUIRE(texture != nullptr);

    auto width = ris_ktxTexture2_GetWidth(texture);
    auto height = ris_ktxTexture2_GetHeight(texture);
    bool needsTranscoding = ris_ktxTexture2_NeedsTranscoding(texture);

    REQUIRE(width > 0);
    REQUIRE(height > 0);
    REQUIRE(needsTranscoding);

    ris_ktxTexture2_Destroy(texture);
}


TEST_CASE("ktx - Write universal basis with manual mip levels",
    "[ris_ktx_texture_write_unviversal_basis_manual_mip_levels_test]")
{
    int width, height, channels;
    auto data = loadTestPng(&width, &height, &channels);

    ris_ktxTextureCreateInfo createInfo = {};
    createInfo.baseWidth = width;
    createInfo.baseHeight = height;
    createInfo.vkFormat = VK_FORMAT_R8G8B8A8_UNORM;
	createInfo.numLevels = 3;
    createInfo.generateMipmaps = false; // Enable automatic mipmap generation

    ktxTexture2* texture = nullptr;
    ktxTextureCreateStorageEnum storageAllocation = KTX_TEXTURE_CREATE_ALLOC_STORAGE;
    auto errorCode = ris_ktxTexture2_Create(&createInfo, storageAllocation, &texture);

    REQUIRE(errorCode == KTX_SUCCESS);
    REQUIRE(texture != nullptr);

    // Level 0
    errorCode = ris_ktxTexture2_SetImageFromMemory(texture, 0, 0, 0, data, width * height * channels);
    REQUIRE(errorCode == KTX_SUCCESS);

    // Level 1
	int mipLevel1Width, mipLevel1Height;
	auto mipLevel1Data = generateMipLevel(data, width, height, channels, 1, &mipLevel1Width, &mipLevel1Height);
	errorCode = ris_ktxTexture2_SetImageFromMemory(texture, 1, 0, 0, mipLevel1Data, mipLevel1Width * mipLevel1Height * channels);
	REQUIRE(errorCode == KTX_SUCCESS);

	// Level 2
	int mipLevel2Width, mipLevel2Height;
	auto mipLevel2Data = generateMipLevel(data, width, height, channels, 2, &mipLevel2Width, &mipLevel2Height);
	errorCode = ris_ktxTexture2_SetImageFromMemory(texture, 2, 0, 0, mipLevel2Data, mipLevel2Width * mipLevel2Height * channels);
	REQUIRE(errorCode == KTX_SUCCESS);

    ris_ktxBasisParams params = {};
    params.compressionLevel = KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL;

    errorCode = ris_ktxTexture2_CompressBasisEx(texture, &params);
    REQUIRE(errorCode == KTX_SUCCESS);

    errorCode = ris_ktxTexture2_WriteToNamedFile(texture, TEST_OUTPUT_KTX);
	ris_ktxTexture2_Destroy(texture);

	// Now load the file back and confirm it has the expected number of mip levels.
	errorCode = ris_ktxTexture2_CreateFromNamedFile(TEST_OUTPUT_KTX, KTX_TEXTURE_CREATE_NO_FLAGS, &texture);
	REQUIRE(errorCode == KTX_SUCCESS);

	uint32_t numLevels = ris_ktxTexture2_GetNumLevels(texture);
	REQUIRE(numLevels == 3);

	ris_ktxTexture2_Destroy(texture);
    freeTestPng(data);
    REQUIRE(errorCode == KTX_SUCCESS);
}