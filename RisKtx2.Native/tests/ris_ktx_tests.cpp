#include <catch2/catch_test_macros.hpp>
#include <ris_ktx.hpp>
#include <spdlog/spdlog.h>
#include "test_utilities.hpp"


// ---------------------------------------------------------------------------
// ris_ktxTexture2_CreateFromNamedFile
// ---------------------------------------------------------------------------

TEST_CASE("ktx  - CreateFromNamedFile loads a valid KTX file",
	"[ris_ktxTexture2_CreateFromNamedFile]")
{
    ktxTexture2* texture = nullptr;
    KTX_error_code err = ris_ktxTexture2_CreateFromNamedFile(
        TEST_KTX_BASIS_UASTC,
        KTX_TEXTURE_CREATE_NO_FLAGS,
        &texture);

    REQUIRE(err == KTX_SUCCESS);
    REQUIRE(texture != nullptr);

    // Verify basic properties are readable.
    REQUIRE(ris_ktxTexture2_GetWidth(texture) > 0);
    REQUIRE(ris_ktxTexture2_GetHeight(texture) > 0);

    ris_ktxTexture2_Destroy(texture);
}

TEST_CASE("ktx ai - CreateFromNamedFile fails on missing file",
          "[ris_ktxTexture2_CreateFromNamedFile]")
{
    ktxTexture2* texture = nullptr;
    KTX_error_code err = ris_ktxTexture2_CreateFromNamedFile(
        "nonexistent_file.ktx2",
        KTX_TEXTURE_CREATE_NO_FLAGS,
        &texture);

    REQUIRE(err != KTX_SUCCESS);
}

// ---------------------------------------------------------------------------
// ris_ktxTexture2_TranscodeBasis
// ---------------------------------------------------------------------------

TEST_CASE("ktx - TranscodeBasis transcode to BC7_RGBA",
          "[ris_ktxTexture2_TranscodeBasis]")
{
    ktxTexture2* texture = nullptr;
    KTX_error_code err = ris_ktxTexture2_CreateFromNamedFile(
        TEST_KTX_BASIS_UASTC,
        KTX_TEXTURE_CREATE_NO_FLAGS,
        &texture);
    REQUIRE(err == KTX_SUCCESS);
    REQUIRE(texture != nullptr);

    // The test file is BasisU super-compressed, so it should need transcoding.
    REQUIRE(ris_ktxTexture2_NeedsTranscoding(texture) == 1);

    // Transcode to BC7_RGBA (widely supported on desktop GPUs).
    err = ris_ktxTexture2_TranscodeBasis(
        texture,
        KTX_TTF_BC7_RGBA,
        0);
    REQUIRE(err == KTX_SUCCESS);

    // After transcoding it should no longer need transcoding.
    REQUIRE(ris_ktxTexture2_NeedsTranscoding(texture) == 0);

    // Data should be accessible now.
    uint8_t* data = ris_ktxTexture2_GetData(texture);
    REQUIRE(data != nullptr);

    size_t imageSize = ris_ktxTexture2_GetImageSize(texture, 0);
    REQUIRE(imageSize > 0);

    ris_ktxTexture2_Destroy(texture);
}

TEST_CASE("ktx - TranscodeBasis transcode to ETC2_RGBA",
          "[ris_ktxTexture2_TranscodeBasis]")
{
    ktxTexture2* texture = nullptr;
    KTX_error_code err = ris_ktxTexture2_CreateFromNamedFile(
        TEST_KTX_BASIS_UASTC,
        KTX_TEXTURE_CREATE_NO_FLAGS,
        &texture);
    REQUIRE(err == KTX_SUCCESS);

    REQUIRE(ris_ktxTexture2_NeedsTranscoding(texture) == 1);

    err = ris_ktxTexture2_TranscodeBasis(
        texture,
        KTX_TTF_ETC2_RGBA,
        0);
    REQUIRE(err == KTX_SUCCESS);

    REQUIRE(ris_ktxTexture2_NeedsTranscoding(texture) == 0);
    REQUIRE(ris_ktxTexture2_GetData(texture) != nullptr);

    ris_ktxTexture2_Destroy(texture);
}

// ---------------------------------------------------------------------------
// ris_ktxTexture2_GetNumLevels
// ---------------------------------------------------------------------------

TEST_CASE("ktx - GetNumLevels returns at least 1",
          "[ris_ktxTexture2_GetNumLevels]")
{
    ktxTexture2* texture = nullptr;
    KTX_error_code err = ris_ktxTexture2_CreateFromNamedFile(
        TEST_KTX_BASIS_UASTC,
        KTX_TEXTURE_CREATE_NO_FLAGS,
        &texture);
    REQUIRE(err == KTX_SUCCESS);

    uint32_t numLevels = ris_ktxTexture2_GetNumLevels(texture);
    REQUIRE(numLevels >= 1);

    // Image size should be > 0 for level 0
    REQUIRE(ris_ktxTexture2_GetImageSize(texture, 0) > 0);

    ris_ktxTexture2_Destroy(texture);
}

// ---------------------------------------------------------------------------
// ris_ktxTexture2_GetVkFormat
// ---------------------------------------------------------------------------

TEST_CASE("ktx - GetVkFormat returns a valid Vulkan format",
          "[ris_ktxTexture2_GetVkFormat]")
{
    ktxTexture2* texture = nullptr;
    KTX_error_code err = ris_ktxTexture2_CreateFromNamedFile(
        TEST_KTX_BASIS_UASTC,
        KTX_TEXTURE_CREATE_NO_FLAGS,
        &texture);
    REQUIRE(err == KTX_SUCCESS);

    VkFormat vkFormat = ris_ktxTexture2_GetVkFormat(texture);

    // The BasisU ASTC test file should be compressed format (VK_FORMAT_UNDEFINED
    // until transcoded, but the field should still be populated from file metadata).
    // Basic sanity: it's a 32-bit value and not VK_FORMAT_MAX_ENUM.
    REQUIRE(vkFormat < VK_FORMAT_MAX_ENUM);

    ris_ktxTexture2_Destroy(texture);
}

// ---------------------------------------------------------------------------
// ris_ktxTexture2_SetImageFromMemory ( mip levels )
// ---------------------------------------------------------------------------

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
    createInfo.generateMipmaps = 0;

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
    params.etc1sCompressionLevel = KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL;

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