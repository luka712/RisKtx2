//
// AI-GENERATED: Tests for the additional KTX API wrappers.
// Keep this file separate from hand-written test code.
//

#include <catch2/catch_test_macros.hpp>
#include <c_ktx_ai_generated.hpp>
#include <spdlog/spdlog.h>
#include "test_utilities.hpp"

// region AI_GENERATED

// ---------------------------------------------------------------------------
// ris_ktxTexture2_CreateFromNamedFile
// ---------------------------------------------------------------------------

TEST_CASE("ktx ai - CreateFromNamedFile loads a valid KTX file",
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

TEST_CASE("ktx ai - TranscodeBasis transcode to BC7_RGBA",
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
    REQUIRE(ris_ktxTexture2_NeedsTranscoding(texture));

    // Transcode to BC7_RGBA (widely supported on desktop GPUs).
    err = ris_ktxTexture2_TranscodeBasis(
        texture,
        KTX_TTF_BC7_RGBA,
        0);
    REQUIRE(err == KTX_SUCCESS);

    // After transcoding it should no longer need transcoding.
    REQUIRE_FALSE(ris_ktxTexture2_NeedsTranscoding(texture));

    // Data should be accessible now.
    uint8_t* data = ris_ktxTexture2_GetData(texture);
    REQUIRE(data != nullptr);

    size_t imageSize = ris_ktxTexture2_GetImageSize(texture, 0);
    REQUIRE(imageSize > 0);

    ris_ktxTexture2_Destroy(texture);
}

TEST_CASE("ktx ai - TranscodeBasis transcode to ETC2_RGBA",
          "[ris_ktxTexture2_TranscodeBasis]")
{
    ktxTexture2* texture = nullptr;
    KTX_error_code err = ris_ktxTexture2_CreateFromNamedFile(
        TEST_KTX_BASIS_UASTC,
        KTX_TEXTURE_CREATE_NO_FLAGS,
        &texture);
    REQUIRE(err == KTX_SUCCESS);

    REQUIRE(ris_ktxTexture2_NeedsTranscoding(texture));

    err = ris_ktxTexture2_TranscodeBasis(
        texture,
        KTX_TTF_ETC2_RGBA,
        0);
    REQUIRE(err == KTX_SUCCESS);

    REQUIRE_FALSE(ris_ktxTexture2_NeedsTranscoding(texture));
    REQUIRE(ris_ktxTexture2_GetData(texture) != nullptr);

    ris_ktxTexture2_Destroy(texture);
}

// ---------------------------------------------------------------------------
// ris_ktxTexture2_GetNumLevels
// ---------------------------------------------------------------------------

TEST_CASE("ktx ai - GetNumLevels returns at least 1",
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

TEST_CASE("ktx ai - GetVkFormat returns a valid Vulkan format",
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

// endregion AI_GENERATED