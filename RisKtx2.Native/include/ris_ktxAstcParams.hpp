#pragma once

#include <cstdint>
#include <ktx.h>

#ifdef __cplusplus
extern "C" {
#endif

//! Structure for passing extended parameters to ktxTexture_CompressAstc.
//! Passing a struct initialized to 0 (e.g. " = {0};") will use blockDimension 4x4, mode LDR and qualityLevel FASTEST. Setting qualityLevel to KTX_PACK_ASTC_QUALITY_LEVEL_MEDIUM is recommended.
//! NOTE: This is a plain C struct (no constructor) to remain blittable for C# P/Invoke.
//! Use ris_ktxBasisParams_InitDefaults() to get zero-initialized default values.
typedef struct ris_ktxAstcParams {

    //! astcenc supports -fastest, -fast, -medium, -thorough, -exhaustive
    ktx_pack_astc_quality_levels_e qualityLevel;

    //! If true, prints Basis Universal encoder operation details to stdout.
    //! Not recommended for GUI apps.
    //! NOTE: Use uint32_t (0 or 1) instead of bool for C# P/Invoke compatibility.
    uint32_t verbose;

    //! Combinations of block dimensions that astcenc supports i.e. 6x6, 8x8, 6x5, etc.
    ktx_pack_astc_block_dimension_e blockDimension;

    //! A swizzle to apply before encoding.
    //! It must match the regular expression /^[rgba01]{4}$/.
    //! If both this and preSwizzle are specified, ktxTexture_CompressBasisEx will raise KTX_INVALID_OPERATION.
    //! Usable with both ETC1S and UASTC.
    char inputSwizzle[4];
} ris_ktxAstcParams;

//! Returns a zero-initialized ris_ktxBasisParams struct.
//! Callers should then set etc1sCompressionLevel explicitly.
inline ris_ktxAstcParams ris_ktxAstcParams_InitDefaults(void) {
    ris_ktxAstcParams params = {};
    params.inputSwizzle[0] = 'R';
    params.inputSwizzle[1] = 'G';
    params.inputSwizzle[2] = 'B';
    params.inputSwizzle[3] = 'A';
    return params;
}

#ifdef __cplusplus
} // extern "C"
#endif
