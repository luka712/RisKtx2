// src/c_ktx.cpp

#include "c_ktx.hpp"
#include <spdlog/spdlog.h>
#include <spdlog/sinks/basic_file_sink.h>

static bool _isLoggerInitialized = false;

// -----------------------------------------------------------------------------
// Creation & Destruction
// -----------------------------------------------------------------------------
#pragma region Creation_Destruction

API_EXPORT KTX_error_code ris_ktxTexture2_Create(
	const ris_ktxTextureCreateInfo* createInfo,
	uint32_t storageAllocation,
	ktxTexture2** outTexture)
{
	ktxTextureCreateInfo ktxCreateInfo = {};
	ktxCreateInfo.baseWidth = createInfo->baseWidth;
	ktxCreateInfo.baseHeight = createInfo->baseHeight;
	ktxCreateInfo.baseDepth = createInfo->baseDepth;
	ktxCreateInfo.numDimensions = createInfo->numDimensions;
	ktxCreateInfo.numLevels = createInfo->numLevels;
	ktxCreateInfo.numLayers = createInfo->numLayers;
	ktxCreateInfo.numFaces = createInfo->numFaces;
	ktxCreateInfo.isArray = createInfo->isArray ? KTX_TRUE : KTX_FALSE;
	ktxCreateInfo.generateMipmaps = createInfo->generateMipmaps;
	ktxCreateInfo.vkFormat = createInfo->vkFormat;
	ktxCreateInfo.pDfd = nullptr; // Used only if vkFormat is VK_FORMAT_UNDEFINED, which we don't support for creation at this time.
	ktxCreateInfo.glInternalformat = 0; // Ignored when creating a ktxTexture2, as it's used by ktxTexture1, but must be set to 0 to avoid validation errors in ktxTexture2_Create().

	ktxTextureCreateStorageEnum storageAllocationEnum = static_cast<ktxTextureCreateStorageEnum>(storageAllocation);
	return ktxTexture2_Create(&ktxCreateInfo, storageAllocationEnum, outTexture);
}

API_EXPORT KTX_error_code ris_ktxTexture2_CreateFromNamedFile(
	const char* filename,
	ktxTextureCreateFlagBits flags,
	ktxTexture2** outTexture)
{
	if (!_isLoggerInitialized) {
		auto logger = spdlog::basic_logger_mt("global_logger", "ris_ktx2.log");
		// Set as global default
		spdlog::set_default_logger(logger);
		spdlog::set_level(spdlog::level::debug);
		_isLoggerInitialized = true;
	}

	return ktxTexture2_CreateFromNamedFile(filename, flags, outTexture);
}

API_EXPORT void ris_ktxTexture2_Destroy(const ktxTexture2* tex)
{
	ktxTexture_Destroy(ktxTexture(tex));
}

#pragma endregion

// -----------------------------------------------------------------------------
// File I/O
// -----------------------------------------------------------------------------
#pragma region File_IO

API_EXPORT KTX_error_code ris_ktxTexture2_WriteToNamedFile(
	const ktxTexture2* tex,
	const char* const dstname)
{
	return ktxTexture_WriteToNamedFile(ktxTexture(tex), dstname);
}

#pragma endregion

// -----------------------------------------------------------------------------
// Image Data Manipulation
// -----------------------------------------------------------------------------
#pragma region Image_Data_Manipulation

API_EXPORT KTX_error_code ris_ktxTexture2_SetImageFromMemory(
	ktxTexture2* tex,
	uint32_t level,
	uint32_t layer,
	uint32_t faceSlice,
	const uint8_t* src,
	size_t srcSize)
{
	if (tex == nullptr)
	{
		spdlog::error("ris_ktxTexture2_SetImageFromMemory: tex is null");
		return KTX_INVALID_VALUE;
	}

	spdlog::debug("ris_ktxTexture2_SetImageFromMemory: level={}, layer={}, faceSlice={}, src={}, srcSize={}",
		std::to_string(level),
		std::to_string(layer),
		std::to_string(faceSlice),
		std::to_string(reinterpret_cast<uintptr_t>(src)),
		std::to_string(srcSize));

	return ktxTexture_SetImageFromMemory(ktxTexture(tex), level, layer, faceSlice, src, srcSize);
}

API_EXPORT KTX_error_code ris_ktxTexture2_CompressBasis(ktxTexture2* tex, uint32_t quality)
{
	return ktxTexture2_CompressBasis(tex, quality);	
}

API_EXPORT KTX_error_code ris_ktxTexture2_CompressBasisEx(
	ktxTexture2* tex,
	const ris_ktxBasisParams* params)
{
	ktxBasisParams ktxParams = {};
	ktxParams.structSize = sizeof(ktxBasisParams);
	ktxParams.uastc = params->uastc;
	ktxParams.qualityLevel = params->qualityLevel;
	ktxParams.compressionLevel = params->etc1sCompressionLevel;
	ktxParams.uastcFlags = KTX_PACK_UASTC_LEVEL_DEFAULT;
	ktxParams.threadCount = params->threadCount;
	ktxParams.uastcRDO = params->uastcRDO;
	ktxParams.uastcRDOQualityScalar = params->uastcRDOQualityScalar;
	for (int i = 0; i < 4; ++i) {
		ktxParams.inputSwizzle[i] = params->inputSwizzle[i];
	}
	ktxParams.verbose = params->verbose;

	return ktxTexture2_CompressBasisEx(tex, &ktxParams);
}

API_EXPORT KTX_error_code ris_ktxTexture2_TranscodeBasis(
	ktxTexture2* texture,
	ktx_transcode_fmt_e outputFormat,
	ktx_transcode_flags transcodeFlags)
{
	return ktxTexture2_TranscodeBasis(texture, outputFormat, transcodeFlags);
}

#pragma endregion

// -----------------------------------------------------------------------------
// Query Helpers
// -----------------------------------------------------------------------------
#pragma region Query_Helpers

API_EXPORT uint32_t ris_ktxTexture2_GetWidth(const ktxTexture2* tex)
{
	return tex->baseWidth;
}

API_EXPORT uint32_t ris_ktxTexture2_GetHeight(const ktxTexture2* tex)
{
	return tex->baseHeight;
}

API_EXPORT size_t ris_ktxTexture2_GetDataSize(const ktxTexture2* tex)
{
	return tex->dataSize;
}

unsigned char* ris_ktxTexture2_GetPData(const ktxTexture2* tex)
{
	return tex->pData;
}

API_EXPORT uint32_t ris_ktxTexture2_GetNumLevels(const ktxTexture2* tex)
{
	return tex->numLevels;
}

API_EXPORT uint32_t ris_ktxTexture2_GetRowPitch(const ktxTexture2* tex, uint32_t level)
{
	return ktxTexture_GetRowPitch(ktxTexture(tex), level);
}

API_EXPORT
uint32_t ris_ktxTexture2_GetElementSize(const ktxTexture2* tex)
{
	return ktxTexture_GetElementSize(ktxTexture(tex));
}

API_EXPORT VkFormat ris_ktxTexture2_GetVkFormat(const ktxTexture2* tex)
{
	return static_cast<VkFormat>(tex->vkFormat);
}

API_EXPORT uint8_t* ris_ktxTexture2_GetData(const ktxTexture2* tex)
{
	return ktxTexture_GetData(ktxTexture(tex));
}

API_EXPORT bool ris_ktxTexture2_NeedsTranscoding(ktxTexture2* tex)
{
	return ktxTexture2_NeedsTranscoding(tex) == KTX_TRUE;
}

API_EXPORT size_t ris_ktxTexture2_GetImageSize(const ktxTexture2* tex, uint32_t level)
{
	return ktxTexture_GetImageSize(ktxTexture(tex), level);
}

API_EXPORT ktxSupercmpScheme ris_ktxTexture2_GetSupercompressionScheme(ktxTexture2* tex)
{
	return tex->supercompressionScheme;
}

API_EXPORT KTX_error_code ris_ktxTexture2_GetImageOffset(
	const ktxTexture2* tex,
	uint32_t level,
	uint32_t layer,
	uint32_t faceSlice,
	size_t* pOffset)
{
	return ktxTexture_GetImageOffset(ktxTexture(tex), level, layer, faceSlice, pOffset);
}

#pragma endregion