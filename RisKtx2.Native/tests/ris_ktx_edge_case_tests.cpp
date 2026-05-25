#include <catch2/catch_test_macros.hpp>
#include <ris_ktx.hpp>
#include "test_utilities.hpp"

// ---------------------------------------------------------------------------
// Null pointer safety
// ---------------------------------------------------------------------------

TEST_CASE("ktx - GetWidth returns 0 on null texture",
          "[ris_ktxTexture2_GetWidth]")
{
    REQUIRE(ris_ktxTexture2_GetWidth(nullptr) == 0);
}

TEST_CASE("ktx - GetHeight returns 0 on null texture",
          "[ris_ktxTexture2_GetHeight]")
{
    REQUIRE(ris_ktxTexture2_GetHeight(nullptr) == 0);
}

TEST_CASE("ktx - GetDataSize returns 0 on null texture",
          "[ris_ktxTexture2_GetDataSize]")
{
    REQUIRE(ris_ktxTexture2_GetDataSize(nullptr) == 0);
}

TEST_CASE("ktx - GetNumLevels returns 0 on null texture",
          "[ris_ktxTexture2_GetNumLevels]")
{
    REQUIRE(ris_ktxTexture2_GetNumLevels(nullptr) == 0);
}

TEST_CASE("ktx - NeedsTranscoding returns 0 on null texture",
          "[ris_ktxTexture2_NeedsTranscoding]")
{
    REQUIRE(ris_ktxTexture2_NeedsTranscoding(nullptr) == 0);
}

TEST_CASE("ktx - GetData returns nullptr on null texture",
          "[ris_ktxTexture2_GetData]")
{
    REQUIRE(ris_ktxTexture2_GetData(nullptr) == nullptr);
}

TEST_CASE("ktx - GetImageSize returns 0 on null texture",
          "[ris_ktxTexture2_GetImageSize]")
{
    REQUIRE(ris_ktxTexture2_GetImageSize(nullptr, 0) == 0);
}

TEST_CASE("ktx - GetSupercompressionScheme returns NONE on null texture",
          "[ris_ktxTexture2_GetSupercompressionScheme]")
{
    REQUIRE(ris_ktxTexture2_GetSupercompressionScheme(nullptr) == KTX_SS_NONE);
}

TEST_CASE("ktx - GetImageOffset returns INVALID_VALUE on null texture",
          "[ris_ktxTexture2_GetImageOffset]")
{
    size_t offset = 0;
    REQUIRE(ris_ktxTexture2_GetImageOffset(nullptr, 0, 0, 0, &offset) == KTX_INVALID_VALUE);
}

TEST_CASE("ktx - SetImageFromMemory returns INVALID_VALUE on null texture",
          "[ris_ktxTexture2_SetImageFromMemory]")
{
    uint8_t dummy = 0;
    REQUIRE(ris_ktxTexture2_SetImageFromMemory(nullptr, 0, 0, 0, &dummy, 1) == KTX_INVALID_VALUE);
}

// ---------------------------------------------------------------------------
// Create / Destroy round-trip
// ---------------------------------------------------------------------------

TEST_CASE("ktx - Create and destroy a simple texture",
          "[ris_ktxTexture2_Create]")
{
    ris_ktxTextureCreateInfo createInfo = {};
    createInfo.baseWidth = 64;
    createInfo.baseHeight = 64;
    createInfo.vkFormat = VK_FORMAT_R8G8B8A8_UNORM;
    createInfo.numLevels = 1;
    createInfo.generateMipmaps = 0;

    ktxTexture2* texture = nullptr;
    auto err = ris_ktxTexture2_Create(&createInfo, KTX_TEXTURE_CREATE_ALLOC_STORAGE, &texture);

    REQUIRE(err == KTX_SUCCESS);
    REQUIRE(texture != nullptr);
    REQUIRE(ris_ktxTexture2_GetWidth(texture) == 64);
    REQUIRE(ris_ktxTexture2_GetHeight(texture) == 64);

    ris_ktxTexture2_Destroy(texture);
}

// ---------------------------------------------------------------------------
// CreateFromNamedFile with invalid path
// ---------------------------------------------------------------------------

TEST_CASE("ktx - CreateFromNamedFile fails gracefully on bad path",
          "[ris_ktxTexture2_CreateFromNamedFile]")
{
    ktxTexture2* texture = nullptr;
    auto err = ris_ktxTexture2_CreateFromNamedFile(
        "definitely_does_not_exist.ktx2",
        KTX_TEXTURE_CREATE_NO_FLAGS,
        &texture);

    REQUIRE(err != KTX_SUCCESS);
    REQUIRE(texture == nullptr);
}

// ---------------------------------------------------------------------------
// GetVkFormat sanity
// ---------------------------------------------------------------------------

TEST_CASE("ktx - GetVkFormat returns VK_FORMAT_UNDEFINED for null texture",
          "[ris_ktxTexture2_GetVkFormat]")
{
    REQUIRE(ris_ktxTexture2_GetVkFormat(nullptr) == VK_FORMAT_UNDEFINED);
}

// ---------------------------------------------------------------------------
// Logging controls
// ---------------------------------------------------------------------------

TEST_CASE("ktx - Logging is disabled by default",
          "[ris_ktxTexture2_IsLoggingEnabled]")
{
    REQUIRE(ris_ktxTexture2_IsLoggingEnabled() == 0);
}

TEST_CASE("ktx - EnableLogging can be called explicitly",
          "[ris_ktxTexture2_EnableLogging]")
{
    REQUIRE(ris_ktxTexture2_IsLoggingEnabled() == 0);

    ris_ktxTexture2_EnableLogging(nullptr);

    REQUIRE(ris_ktxTexture2_IsLoggingEnabled() == 1);
}

TEST_CASE("ktx - EnableLogging is idempotent",
          "[ris_ktxTexture2_EnableLogging]")
{
    ris_ktxTexture2_EnableLogging(nullptr);
    REQUIRE(ris_ktxTexture2_IsLoggingEnabled() == 1);

    // Second call should not crash or change state
    ris_ktxTexture2_EnableLogging(nullptr);
    REQUIRE(ris_ktxTexture2_IsLoggingEnabled() == 1);
}
