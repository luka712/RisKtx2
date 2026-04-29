#pragma once

#include <stdint.h>
#include <stddef.h>

#ifdef _WIN32
#  ifdef RISKTX2NATIVE_EXPORTS
#    define RISKTX2_API __declspec(dllexport)
#  else
#    define RISKTX2_API __declspec(dllimport)
#  endif
#else
#  define RISKTX2_API __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

/* Opaque handle to a KTX2 texture object. */
typedef struct RisKtx2Texture_T* RisKtx2Texture;

/* Error codes returned by RisKtx2 functions. */
typedef enum RisKtx2Result
{
    RISKTX2_SUCCESS                        = 0,
    RISKTX2_ERROR_INVALID_VALUE            = 1,
    RISKTX2_ERROR_INVALID_OPERATION        = 2,
    RISKTX2_ERROR_OUT_OF_MEMORY            = 3,
    RISKTX2_ERROR_FILE_NOT_FOUND           = 4,
    RISKTX2_ERROR_FILE_DATA_ERROR          = 5,
    RISKTX2_ERROR_UNSUPPORTED_TEXTURE_TYPE = 6,
    RISKTX2_ERROR_UNSUPPORTED_FEATURE      = 7,
    RISKTX2_ERROR_LIBRARY_NOT_LINKED       = 8,
    RISKTX2_ERROR_UNKNOWN                  = 9
} RisKtx2Result;

/* Transcode targets for basis-compressed KTX2 textures. */
typedef enum RisKtx2TranscodeFormat
{
    RISKTX2_TRANSCODE_FORMAT_BC1_RGB    = 0,
    RISKTX2_TRANSCODE_FORMAT_BC3_RGBA   = 1,
    RISKTX2_TRANSCODE_FORMAT_BC4_R      = 2,
    RISKTX2_TRANSCODE_FORMAT_BC5_RG     = 3,
    RISKTX2_TRANSCODE_FORMAT_BC7_RGBA   = 6,
    RISKTX2_TRANSCODE_FORMAT_ASTC_4x4   = 10,
    RISKTX2_TRANSCODE_FORMAT_RGBA32     = 13,
    RISKTX2_TRANSCODE_FORMAT_RGB565     = 14,
    RISKTX2_TRANSCODE_FORMAT_BGR565     = 15,
    RISKTX2_TRANSCODE_FORMAT_RGBA4444   = 16
} RisKtx2TranscodeFormat;

/**
 * Create a RisKtx2Texture from a file on disk.
 *
 * @param filePath  Null-terminated path to the KTX2 file.
 * @param pTexture  Receives the created texture handle on success.
 * @return RISKTX2_SUCCESS on success, or an error code on failure.
 */
RISKTX2_API RisKtx2Result risKtx2TextureCreateFromFile(
    const char*      filePath,
    RisKtx2Texture*  pTexture);

/**
 * Create a RisKtx2Texture from a memory buffer.
 *
 * @param pData     Pointer to the KTX2 data in memory.
 * @param dataSize  Size in bytes of the data buffer.
 * @param pTexture  Receives the created texture handle on success.
 * @return RISKTX2_SUCCESS on success, or an error code on failure.
 */
RISKTX2_API RisKtx2Result risKtx2TextureCreateFromMemory(
    const uint8_t*   pData,
    size_t           dataSize,
    RisKtx2Texture*  pTexture);

/**
 * Destroy a RisKtx2Texture and release all associated resources.
 *
 * @param texture  The texture handle to destroy.
 */
RISKTX2_API void risKtx2TextureDestroy(RisKtx2Texture texture);

/**
 * Transcode a basis-compressed KTX2 texture to a GPU-ready format.
 *
 * Must be called before accessing texture data for basis-compressed textures.
 *
 * @param texture  The texture handle.
 * @param format   Target transcode format.
 * @return RISKTX2_SUCCESS on success, or an error code on failure.
 */
RISKTX2_API RisKtx2Result risKtx2TextureTranscodeBasis(
    RisKtx2Texture        texture,
    RisKtx2TranscodeFormat format);

/**
 * Get the raw data pointer for a specific mip level and layer/face.
 *
 * @param texture   The texture handle.
 * @param level     Mip level index (0 = base level).
 * @param layer     Array layer index.
 * @param faceSlice Face index (for cube maps) or depth slice index.
 * @param pData     Receives a pointer to the image data.
 * @param pSize     Receives the size in bytes of the image data.
 * @return RISKTX2_SUCCESS on success, or an error code on failure.
 */
RISKTX2_API RisKtx2Result risKtx2TextureGetImageData(
    RisKtx2Texture  texture,
    uint32_t        level,
    uint32_t        layer,
    uint32_t        faceSlice,
    const uint8_t** pData,
    size_t*         pSize);

/**
 * Get the base width of the texture (in texels).
 */
RISKTX2_API uint32_t risKtx2TextureGetBaseWidth(RisKtx2Texture texture);

/**
 * Get the base height of the texture (in texels).
 */
RISKTX2_API uint32_t risKtx2TextureGetBaseHeight(RisKtx2Texture texture);

/**
 * Get the base depth of the texture (in texels; 1 for 2-D textures).
 */
RISKTX2_API uint32_t risKtx2TextureGetBaseDepth(RisKtx2Texture texture);

/**
 * Get the number of mip levels in the texture.
 */
RISKTX2_API uint32_t risKtx2TextureGetNumLevels(RisKtx2Texture texture);

/**
 * Get the number of array layers in the texture.
 */
RISKTX2_API uint32_t risKtx2TextureGetNumLayers(RisKtx2Texture texture);

/**
 * Get the number of faces (6 for cube maps, 1 otherwise).
 */
RISKTX2_API uint32_t risKtx2TextureGetNumFaces(RisKtx2Texture texture);

/**
 * Returns non-zero if the texture uses a supercompressed (basis/BasisU) encoding.
 */
RISKTX2_API int risKtx2TextureNeedsTranscoding(RisKtx2Texture texture);

/**
 * Returns the VkFormat (Vulkan format) identifier of the texture,
 * or 0 (VK_FORMAT_UNDEFINED) if not applicable.
 */
RISKTX2_API uint32_t risKtx2TextureGetVkFormat(RisKtx2Texture texture);

#ifdef __cplusplus
}
#endif
