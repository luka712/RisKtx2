#pragma once

#include "ktx.h"
#include "macros.hpp"
#include <cstdint>
#include <vulkan/vulkan.h>
#include "ris_ktxTextureCreateInfo.hpp"
#include "ris_ktxBasisParams.hpp"
#include "ris_ktx_logging.hpp"

extern "C" {

// ---------------------------------------------------------------------------
// Creation & Destruction
// ---------------------------------------------------------------------------

/**
 * @brief Create a new KTX2 texture.
 * @param createInfo      Texture creation parameters.
 * @param storageAllocation Storage allocation mode.
 * @param outTexture      Receives the created texture pointer.
 * @return KTX_SUCCESS on success, or a KTX_error_code.
 */
API_EXPORT
KTX_error_code ris_ktxTexture2_Create(
    const ris_ktxTextureCreateInfo* createInfo,
    uint32_t storageAllocation,
    ktxTexture2** outTexture);

/**
 * @brief Load a KTX2 texture from a file.
 * @param filename   Path to the .ktx2 file.
 * @param flags      Creation flags (e.g. KTX_TEXTURE_CREATE_NO_FLAGS).
 * @param outTexture Receives the created texture pointer.
 * @return KTX_SUCCESS on success, or a KTX_error_code.
 */
API_EXPORT
KTX_error_code ris_ktxTexture2_CreateFromNamedFile(
    const char* filename,
    ktxTextureCreateFlagBits flags,
    ktxTexture2** outTexture);

/**
 * @brief Destroy a KTX2 texture.
 * @param tex Texture to destroy.
 */
API_EXPORT
void ris_ktxTexture2_Destroy(const ktxTexture2* tex);

// ---------------------------------------------------------------------------
// File I/O
// ---------------------------------------------------------------------------

/**
 * @brief Write a KTX2 texture to a file.
 * @param tex     Texture to write.
 * @param dstname Destination file path.
 * @return KTX_SUCCESS on success, or a KTX_error_code.
 */
API_EXPORT
KTX_error_code ris_ktxTexture2_WriteToNamedFile(
    const ktxTexture2* tex,
    const char* const dstname);

// ---------------------------------------------------------------------------
// Image Data Manipulation
// ---------------------------------------------------------------------------

/**
 * @brief Set image data from memory for a specific mip/array/face.
 * @param tex      Target texture.
 * @param level    Mipmap level.
 * @param layer    Array layer.
 * @param faceSlice Face or slice index.
 * @param src      Source data pointer.
 * @param srcSize  Size of source data in bytes.
 * @return KTX_SUCCESS on success, or a KTX_error_code.
 */
API_EXPORT
KTX_error_code ris_ktxTexture2_SetImageFromMemory(
    ktxTexture2* tex,
    uint32_t level,
    uint32_t layer,
    uint32_t faceSlice,
    const uint8_t* src,
    size_t srcSize);

API_EXPORT
KTX_error_code ris_ktxTexture2_CompressBasis(ktxTexture2* tex, uint32_t quality);

/**
 * @brief Compress a KTX2 texture using Basis Universal.
 * @param tex    Texture to compress.
 * @param params Compression parameters.
 * @return KTX_SUCCESS on success, or a KTX_error_code.
 */
API_EXPORT
KTX_error_code ris_ktxTexture2_CompressBasisEx(
    ktxTexture2* tex,
    const ris_ktxBasisParams* params);

/**
 * @brief Transcode a Basis Universal compressed KTX2 texture.
 * @param texture        The KTX2 texture (must need transcoding).
 * @param outputFormat   Desired transcode target format (e.g. KTX_TTF_BC7_RGBA).
 * @param transcodeFlags Transcoding flags (e.g. 0 for default).
 * @return KTX_SUCCESS on success, or a KTX_error_code.
 */
API_EXPORT
KTX_error_code ris_ktxTexture2_TranscodeBasis(
    ktxTexture2* texture,
    ktx_transcode_fmt_e outputFormat,
    ktx_transcode_flags transcodeFlags);

// ---------------------------------------------------------------------------
// Query Helpers
// ---------------------------------------------------------------------------

/**
 * @brief Get the width of the texture.
 * @param tex Texture pointer.
 * @return Texture width in pixels.
 */
API_EXPORT
uint32_t ris_ktxTexture2_GetWidth(const ktxTexture2* tex);

/**
 * @brief Get the height of the texture.
 * @param tex Texture pointer.
 * @return Texture height in pixels.
 */
API_EXPORT
uint32_t ris_ktxTexture2_GetHeight(const ktxTexture2* tex);

API_EXPORT
size_t ris_ktxTexture2_GetDataSize(const ktxTexture2* tex);

API_EXPORT
unsigned char* ris_ktxTexture2_GetPData(const ktxTexture2* tex);

/**
 * @brief Get the number of mipmap levels.
 * @param tex Texture pointer.
 * @return Number of mipmap levels.
 */
API_EXPORT
uint32_t ris_ktxTexture2_GetNumLevels(const ktxTexture2* tex);

API_EXPORT
uint32_t ris_ktxTexture2_GetRowPitch(const ktxTexture2* tex, uint32_t level);

API_EXPORT
uint32_t ris_ktxTexture2_GetElementSize(const ktxTexture2* tex);

/**
 * @brief Get the Vulkan format of the texture.
 * @param tex Texture pointer.
 * @return VkFormat value.
 */
API_EXPORT
VkFormat ris_ktxTexture2_GetVkFormat(const ktxTexture2* tex);

/**
 * @brief Get a pointer to the raw texture data.
 * @param tex Texture pointer.
 * @return Pointer to texture data.
 */
API_EXPORT
uint8_t* ris_ktxTexture2_GetData(const ktxTexture2* tex);

/**
 * @brief Check if the texture needs transcoding.
 * @param tex Texture pointer.
 * @return 1 if transcoding is needed, 0 otherwise.
 */
API_EXPORT
uint32_t ris_ktxTexture2_NeedsTranscoding(ktxTexture2* tex);

/**
 * @brief Get the size of an image at a specific mip level.
 * @param tex   Texture pointer.
 * @param level Mipmap level.
 * @return Image size in bytes.
 */
API_EXPORT
size_t ris_ktxTexture2_GetImageSize(const ktxTexture2* tex, uint32_t level);

/**
 * @brief Get the supercompression scheme used.
 * @param tex Texture pointer.
 * @return ktxSupercmpScheme value.
 */
API_EXPORT
ktxSupercmpScheme ris_ktxTexture2_GetSupercompressionScheme(ktxTexture2* tex);

/**
 * @brief Get the offset of an image in the data block.
 * @param tex      Texture pointer.
 * @param level    Mipmap level.
 * @param layer    Array layer.
 * @param faceSlice Face or slice index.
 * @param pOffset  Receives the offset in bytes.
 * @return KTX_SUCCESS on success, or a KTX_error_code.
 */
API_EXPORT
KTX_error_code ris_ktxTexture2_GetImageOffset(
    const ktxTexture2* tex,
    uint32_t level,
    uint32_t layer,
    uint32_t faceSlice,
    size_t* pOffset);

} // extern "C"