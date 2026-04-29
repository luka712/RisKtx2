#include "RisKtx2Native.h"

#include <ktx.h>
#include <cstring>

/* Internal texture state that wraps a ktxTexture2 pointer. */
struct RisKtx2Texture_T
{
    ktxTexture2* ktxTex;
};

/* Map a KTX error code to a RisKtx2Result. */
static RisKtx2Result mapKtxError(KTX_error_code code)
{
    switch (code)
    {
    case KTX_SUCCESS:              return RISKTX2_SUCCESS;
    case KTX_INVALID_VALUE:        return RISKTX2_ERROR_INVALID_VALUE;
    case KTX_INVALID_OPERATION:    return RISKTX2_ERROR_INVALID_OPERATION;
    case KTX_OUT_OF_MEMORY:        return RISKTX2_ERROR_OUT_OF_MEMORY;
    case KTX_FILE_OPEN_FAILED:     return RISKTX2_ERROR_FILE_NOT_FOUND;
    case KTX_FILE_DATA_ERROR:      return RISKTX2_ERROR_FILE_DATA_ERROR;
    case KTX_UNSUPPORTED_TEXTURE_TYPE:
                                   return RISKTX2_ERROR_UNSUPPORTED_TEXTURE_TYPE;
    case KTX_UNSUPPORTED_FEATURE:  return RISKTX2_ERROR_UNSUPPORTED_FEATURE;
    case KTX_LIBRARY_NOT_LINKED:   return RISKTX2_ERROR_LIBRARY_NOT_LINKED;
    default:                       return RISKTX2_ERROR_UNKNOWN;
    }
}

RISKTX2_API RisKtx2Result risKtx2TextureCreateFromFile(
    const char*      filePath,
    RisKtx2Texture*  pTexture)
{
    if (!filePath || !pTexture)
        return RISKTX2_ERROR_INVALID_VALUE;

    ktxTexture2* ktxTex = nullptr;
    KTX_error_code result = ktxTexture2_CreateFromNamedFile(
        filePath,
        KTX_TEXTURE_CREATE_LOAD_IMAGE_DATA_BIT,
        &ktxTex);

    if (result != KTX_SUCCESS)
        return mapKtxError(result);

    RisKtx2Texture handle = new RisKtx2Texture_T{ktxTex};
    *pTexture = handle;
    return RISKTX2_SUCCESS;
}

RISKTX2_API RisKtx2Result risKtx2TextureCreateFromMemory(
    const uint8_t*   pData,
    size_t           dataSize,
    RisKtx2Texture*  pTexture)
{
    if (!pData || dataSize == 0 || !pTexture)
        return RISKTX2_ERROR_INVALID_VALUE;

    ktxTexture2* ktxTex = nullptr;
    KTX_error_code result = ktxTexture2_CreateFromMemory(
        pData,
        dataSize,
        KTX_TEXTURE_CREATE_LOAD_IMAGE_DATA_BIT,
        &ktxTex);

    if (result != KTX_SUCCESS)
        return mapKtxError(result);

    RisKtx2Texture handle = new RisKtx2Texture_T{ktxTex};
    *pTexture = handle;
    return RISKTX2_SUCCESS;
}

RISKTX2_API void risKtx2TextureDestroy(RisKtx2Texture texture)
{
    if (!texture)
        return;

    ktxTexture_Destroy(ktxTexture(texture->ktxTex));
    delete texture;
}

RISKTX2_API RisKtx2Result risKtx2TextureTranscodeBasis(
    RisKtx2Texture         texture,
    RisKtx2TranscodeFormat format)
{
    if (!texture)
        return RISKTX2_ERROR_INVALID_VALUE;

    KTX_error_code result = ktxTexture2_TranscodeBasis(
        texture->ktxTex,
        static_cast<ktx_transcode_fmt_e>(format),
        0);

    return mapKtxError(result);
}

RISKTX2_API RisKtx2Result risKtx2TextureGetImageData(
    RisKtx2Texture  texture,
    uint32_t        level,
    uint32_t        layer,
    uint32_t        faceSlice,
    const uint8_t** pData,
    size_t*         pSize)
{
    if (!texture || !pData || !pSize)
        return RISKTX2_ERROR_INVALID_VALUE;

    size_t offset = 0;
    KTX_error_code result = ktxTexture_GetImageOffset(
        ktxTexture(texture->ktxTex),
        level,
        layer,
        faceSlice,
        &offset);

    if (result != KTX_SUCCESS)
        return mapKtxError(result);

    *pData = ktxTexture_GetData(ktxTexture(texture->ktxTex)) + offset;
    *pSize = ktxTexture_GetImageSize(ktxTexture(texture->ktxTex), level);
    return RISKTX2_SUCCESS;
}

RISKTX2_API uint32_t risKtx2TextureGetBaseWidth(RisKtx2Texture texture)
{
    if (!texture) return 0;
    return texture->ktxTex->baseWidth;
}

RISKTX2_API uint32_t risKtx2TextureGetBaseHeight(RisKtx2Texture texture)
{
    if (!texture) return 0;
    return texture->ktxTex->baseHeight;
}

RISKTX2_API uint32_t risKtx2TextureGetBaseDepth(RisKtx2Texture texture)
{
    if (!texture) return 0;
    return texture->ktxTex->baseDepth;
}

RISKTX2_API uint32_t risKtx2TextureGetNumLevels(RisKtx2Texture texture)
{
    if (!texture) return 0;
    return texture->ktxTex->numLevels;
}

RISKTX2_API uint32_t risKtx2TextureGetNumLayers(RisKtx2Texture texture)
{
    if (!texture) return 0;
    return texture->ktxTex->numLayers;
}

RISKTX2_API uint32_t risKtx2TextureGetNumFaces(RisKtx2Texture texture)
{
    if (!texture) return 0;
    return texture->ktxTex->numFaces;
}

RISKTX2_API int risKtx2TextureNeedsTranscoding(RisKtx2Texture texture)
{
    if (!texture) return 0;
    return ktxTexture2_NeedsTranscoding(texture->ktxTex) ? 1 : 0;
}

RISKTX2_API uint32_t risKtx2TextureGetVkFormat(RisKtx2Texture texture)
{
    if (!texture) return 0;
    return static_cast<uint32_t>(texture->ktxTex->vkFormat);
}
