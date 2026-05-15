#pragma once

#include <cstdint>
#include <ktx.h>

#ifdef __cplusplus
extern "C" {
#endif

//! Structure for passing parameters to ktxTexture2_Create().
//! NOTE: This is a plain C struct (no constructor) to remain blittable for C# P/Invoke.
//! Use ris_ktxTextureCreateInfo_InitDefaults() to get sensible defaults.
typedef struct ris_ktxTextureCreateInfo
{
    //! VkFormat for texture.
    uint32_t vkFormat;

    //! Width of the base level of the texture.
    uint32_t baseWidth;

    //! Height of the base level of the texture.
    uint32_t baseHeight;

    //! Depth of the base level of the texture.
    uint32_t baseDepth;

    //! Number of dimensions in the texture, 1, 2 or 3.
    uint32_t numDimensions;

    //! Number of mip levels in the texture. Should be 1 if generateMipmaps is 'true'.
    uint32_t numLevels;

    //! Number of array layers in the texture.
    uint32_t numLayers;

    //! Number of faces: 6 for cube maps, 1 otherwise.
    uint32_t numFaces;

    //! Set to 1 if the texture is to be an array texture. Means OpenGL will use a GL_TEXTURE_*_ARRAY target.
    //! NOTE: Use uint32_t (0 or 1) instead of bool for C# P/Invoke compatibility.
    uint32_t isArray;

    //! Set to 1 if mipmaps should be generated for the texture when loading into a 3D API.
    //! NOTE: Use uint32_t (0 or 1) instead of bool for C# P/Invoke compatibility.
    uint32_t generateMipmaps;

} ris_ktxTextureCreateInfo;

//! Returns a ris_ktxTextureCreateInfo with sensible defaults:
//! - vkFormat = VK_FORMAT_UNDEFINED (0)
//! - baseWidth / baseHeight = 0
//! - baseDepth = 1
//! - numDimensions = 2
//! - numLevels / numLayers / numFaces = 1
//! - isArray / generateMipmaps = 0
inline ris_ktxTextureCreateInfo ris_ktxTextureCreateInfo_InitDefaults(void)
{
    ris_ktxTextureCreateInfo info = {};
    info.vkFormat = VK_FORMAT_UNDEFINED;
    info.baseDepth = 1;
    info.numDimensions = 2;
    info.numLevels = 1;
    info.numLayers = 1;
    info.numFaces = 1;
    return info;
}

#ifdef __cplusplus
} // extern "C"
#endif
