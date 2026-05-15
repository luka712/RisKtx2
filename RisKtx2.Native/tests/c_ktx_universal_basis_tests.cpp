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
    params.etc1sCompressionLevel = KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL;

    errorCode = ris_ktxTexture2_CompressBasisEx(texture, &params);
    REQUIRE(errorCode == KTX_SUCCESS);

    errorCode = ris_ktxTexture2_WriteToNamedFile(texture, TEST_OUTPUT_KTX);
    ris_ktxTexture2_Destroy(texture);

    freeTestPng(data);
    REQUIRE(errorCode == KTX_SUCCESS);
}

TEST_CASE("ktx - Write universal basis uastc",
              "[ris_ktx_texture_write_universal_basis_uastc_test]")
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
    params.uastc = true;
    params.qualityLevel= 255;

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


