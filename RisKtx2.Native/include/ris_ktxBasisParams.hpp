#pragma once

#include <cstdint>
#include <ktx.h>

#ifdef __cplusplus
extern "C" {
#endif

//! Structure for passing extended parameters to ktxTexture2_CompressBasisEx().
//! If you only want default values, use ktxTexture2_CompressBasis(). Here, at a minimum you must initialize the structure as follows:
//! <code>
//! ris_ktxBasisParams params = { 0 };
//! ris_ktxBasisParams.etc1sCompressionLevel = KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL;
//! </code>
//! compressionLevel has to be explicitly set because 0 is a valid compressionLevel but is not the default used by the BasisU encoder when no value is set.
//! Only the other settings that are to be non-default must be non-zero.
//!
//! NOTE: This is a plain C struct (no constructor) to remain blittable for C# P/Invoke.
//! Use ris_ktxBasisParams_InitDefaults() to get zero-initialized default values.
typedef struct ris_ktxBasisParams
{
	//! ETC1S compression effort level.
	//! Range is [0,6]. Higher values are much slower, but give slightly higher quality.
	//! Higher levels are intended for video.
	//! This parameter controls numerous internal encoding speed vs. compression efficiency/performance tradeoffs.
	//! Note this is NOT the same as the ETC1S quality level, and most users shouldn't change this.
	//! There is no default.
	//! Callers must explicitly set this value.
	//! Callers can use KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL as a default value. Currently this is 2.
	uint32_t compressionLevel;

	//! Compression quality.
	//! Range is [1,255].
	//! Lower gives better compression/lower quality/faster.
	//! Higher gives less compression /higher quality/slower.
	//! This automatically determines values for maxEndpoints, maxSelectors, endpointRDOThreshold and selectorRDOThreshold for the target quality level.
	//! Setting these parameters overrides the values determined by qualityLevel which defaults to 128 if neither it nor both of maxEndpoints and maxSelectors have been set.
	uint32_t qualityLevel;

	//! Tunes codec parameters for better quality on normal maps (no selector RDO, no endpoint RDO) and sets the texture's DFD appropriately.
	//! Only valid for linear textures.
	//! NOTE: Use uint32_t (0 or 1) instead of bool for C# P/Invoke compatibility.
	uint32_t normalMap;

	//! Number of threads used for compression. Default is 1.
	uint32_t threadCount;

	//! True to use UASTC base, false to use ETC1S base.
	//! NOTE: Use uint32_t (0 or 1) instead of bool for C# P/Invoke compatibility.
	uint32_t uastc;

	//! A set of ::ktx_pack_uastc_flag_bits_e controlling UASTC encoding.
	//! The most important value is the level given in the least-significant 4 bits which selects a speed vs quality tradeoff
	//! as shown in the following table:
	   //! Level/Speed | Quality
	   //! :-----: | :-------:
	   //! KTX_PACK_UASTC_LEVEL_FASTEST | 43.45dB
	   //! KTX_PACK_UASTC_LEVEL_FASTER | 46.49dB
	   //! KTX_PACK_UASTC_LEVEL_DEFAULT | 47.47dB
	   //! KTX_PACK_UASTC_LEVEL_SLOWER  | 48.01dB
	   //! KTX_PACK_UASTC_LEVEL_VERYSLOW | 48.24dB
	ktx_pack_uastc_flag_bits_e uastcFlags;

	//! Enable Rate Distortion Optimization (RDO) post-processing.
	//! NOTE: Use uint32_t (0 or 1) instead of bool for C# P/Invoke compatibility.
	uint32_t uastcRDO;

	//! UASTC RDO quality scalar (lambda).
	//! Lower values yield higher quality/larger LZ compressed files, higher values yield lower quality/smaller LZ compressed files.
	//! A good range to try is [.2,4].
	//! Full range is [.001,50.0].
	//! Default is 1.0.
	float uastcRDOQualityScalar;

	//! If true, prints Basis Universal encoder operation details to stdout.
	//! Not recommended for GUI apps.
	//! NOTE: Use uint32_t (0 or 1) instead of bool for C# P/Invoke compatibility.
	uint32_t verbose;

	//! A swizzle to apply before encoding.
	//! It must match the regular expression /^[rgba01]{4}$/.
	//! If both this and preSwizzle are specified ktxTexture_CompressBasisEx will raise KTX_INVALID_OPERATION.
	//! Usable with both ETC1S and UASTC.
	char inputSwizzle[4];

} ris_ktxBasisParams;

//! Returns a zero-initialized ris_ktxBasisParams struct.
//! Callers should then set etc1sCompressionLevel explicitly.
inline ris_ktxBasisParams ris_ktxBasisParams_InitDefaults(void)
{
	ris_ktxBasisParams params = {};
	return params;
}

#ifdef __cplusplus
} // extern "C"
#endif
