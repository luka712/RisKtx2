#include <catch2/catch_test_macros.hpp>
#include <ris_ktx.hpp>
#include <iostream>
#include <filesystem>
#include <spdlog/spdlog.h>
#include "test_utilities.hpp"

TEST_CASE("ktx - Write astc",
              "[ris_ktx_texture_write_astc_test]")
{
    int width, height, channels;
    auto data = loadTestPng(&width, &height, &channels);

    ris_ktxTextureCreateInfo createInfo = {};
    createInfo.baseWidth = width;
    createInfo.baseHeight = height;
    createInfo.vkFormat = VK_FORMAT_R8G8B8A8_UNORM;
	createInfo.numLevels = 1;
	createInfo.generateMipmaps = 0;
    createInfo.numDimensions = 2;

    ktxTexture2* texture = nullptr;
    ktxTextureCreateStorageEnum storageAllocation = KTX_TEXTURE_CREATE_ALLOC_STORAGE;
    auto errorCode = ris_ktxTexture2_Create(&createInfo, storageAllocation, &texture);

    REQUIRE(errorCode == KTX_SUCCESS);
    REQUIRE(texture != nullptr);

    errorCode = ris_ktxTexture2_SetImageFromMemory(texture, 0, 0, 0, data, width * height * channels);
	REQUIRE(errorCode == KTX_SUCCESS);

    ris_ktxAstcParams params = ris_ktxAstcParams_InitDefaults();
    params.qualityLevel = KTX_PACK_ASTC_QUALITY_LEVEL_MEDIUM;
    params.blockDimension = KTX_PACK_ASTC_BLOCK_DIMENSION_4x4;

    errorCode = ris_ktxTexture2_CompressAstcEx(texture, &params);
    REQUIRE(errorCode == KTX_SUCCESS);

    errorCode = ris_ktxTexture2_WriteToNamedFile(texture, TEST_OUTPUST_ASTC);
    ris_ktxTexture2_Destroy(texture);

    freeTestPng(data);
    REQUIRE(errorCode == KTX_SUCCESS);
}

TEST_CASE("ktx - Load astc and confirm properties",
          "[ris_ktx_texture_load_and_confirm_properties_test]")
{
    ktxTexture2* texture = nullptr;
    auto errorCode = ktxTexture2_CreateFromNamedFile(TEST_OUTPUST_ASTC, KTX_TEXTURE_CREATE_NO_FLAGS, &texture);

    REQUIRE(errorCode == KTX_SUCCESS);
    REQUIRE(texture != nullptr);

    auto width = ris_ktxTexture2_GetWidth(texture);
    auto height = ris_ktxTexture2_GetHeight(texture);
    uint32_t needsTranscoding = ris_ktxTexture2_NeedsTranscoding(texture);

    REQUIRE(width > 0);
    REQUIRE(height > 0);
    REQUIRE(needsTranscoding == 0);
    REQUIRE(ris_ktxTexture2_GetVkFormat(texture) == VK_FORMAT_ASTC_4x4_UNORM_BLOCK);

    ris_ktxTexture2_Destroy(texture);
}


