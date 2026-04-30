namespace RisKtx2
{

    /// <summary>
    /// @~English
    /// Enumerators for specifying the transcode target format.
    /// 
    /// For BasisU/ETC1S format, @e Opaque and @e alpha here refer to 2 separate
    /// RGB images, a.k.a slices within the BasisU compressed data.For UASTC
    /// format they refer to the RGB and the alpha components of the UASTC data.If
    /// the original image had only 2 components, R will be in the opaque portion
    /// and G in the alpha portion.The R value will be replicated in the RGB
    /// components.In the case of BasisU the G value will be replicated in all 3
    /// components of the alpha slice.If the original image had only 1 component
    /// it's value is replicated in all 3 components of the opaque portion and
    /// there is no alpha.
    /// </summary>
    /// 
    /// <remarks>
    /// @note You should not transcode sRGB encoded data to @c KTX_TTF_BC4_R,
    /// @c KTX_TTF_BC5_RG, @c KTX_TTF_ETC2_EAC_R{,G}11, @c KTX_TTF_RGB565,
    /// @c KTX_TTF_BGR565 or @c KTX_TTF_RGBA4444 formats as neither OpenGL nor
    /// Vulkan support sRGB variants of these.Doing sRGB decoding in the shader
    /// will not produce correct results if any texture filtering is being used.
    /// </remarks
    public enum KtxTranscodeFormat : uint
    {
        // Compressed formats

        // ETC1-2
        KTX_TTF_ETC1_RGB = 0,
        /*!< Opaque only. Returns RGB or alpha data, if
             KTX_TF_TRANSCODE_ALPHA_DATA_TO_OPAQUE_FORMATS flag is
             specified. */
        KTX_TTF_ETC2_RGBA = 1,
        /*!< Opaque+alpha. EAC_A8 block followed by an ETC1 block. The
             alpha channel will be opaque for textures without an alpha
             channel. */

        // BC1-5, BC7 (desktop, some mobile devices)
        KTX_TTF_BC1_RGB = 2,
        /*!< Opaque only, no punchthrough alpha support yet.  Returns RGB
             or alpha data, if KTX_TF_TRANSCODE_ALPHA_DATA_TO_OPAQUE_FORMATS
             flag is specified. */
        KTX_TTF_BC3_RGBA = 3,
        /*!< Opaque+alpha. BC4 block with alpha followed by a BC1 block. The
             alpha channel will be opaque for textures without an alpha
             channel. */
        KTX_TTF_BC4_R = 4,
        /*!< One BC4 block. R = opaque.g or alpha.g, if
             KTX_TF_TRANSCODE_ALPHA_DATA_TO_OPAQUE_FORMATS flag is
             specified. */
        KTX_TTF_BC5_RG = 5,

        /// <summary>
        /// BC7 compressed RGBA texture.
        /// High quality block compression for color textures.
        /// Supports alpha channel and is suitable for diffuse/physically based textures.
        /// </summary>
        BC7_RGBA = 6,
        /*!< RGB or RGBA mode 5 for ETC1S, modes 1, 2, 3, 4, 5, 6, 7 for
             UASTC. */

        // PVRTC1 4bpp (mobile, PowerVR devices)
        KTX_TTF_PVRTC1_4_RGB = 8,
        /*!< Opaque only. Returns RGB or alpha data, if
             KTX_TF_TRANSCODE_ALPHA_DATA_TO_OPAQUE_FORMATS flag is
             specified. */
        KTX_TTF_PVRTC1_4_RGBA = 9,
        /*!< Opaque+alpha. Most useful for simple opacity maps. If the
             texture doesn't have an alpha channel KTX_TTF_PVRTC1_4_RGB
             will be used instead. Lowest quality of any supported
             texture format. */

        // ASTC (mobile, Intel devices, hopefully all desktop GPU's one day)
        KTX_TTF_ASTC_4x4_RGBA = 10,
        /*!< Opaque+alpha, ASTC 4x4. The alpha channel will be opaque for
             textures without an alpha channel.  The transcoder uses
             RGB/RGBA/L/LA modes, void extent, and up to two ([0,47] and
             [0,255]) endpoint precisions. */

        // ATC and FXT1 formats are not supported by KTX2 as there
        // are no equivalent VkFormats.

        KTX_TTF_PVRTC2_4_RGB = 18,
        /*!< Opaque-only. Almost BC1 quality, much faster to transcode
             and supports arbitrary texture dimensions (unlike
             PVRTC1 RGB). */
        KTX_TTF_PVRTC2_4_RGBA = 19,
        /*!< Opaque+alpha. Slower to transcode than cTFPVRTC2_4_RGB.
             Premultiplied alpha is highly recommended, otherwise the
             color channel can leak into the alpha channel on transparent
             blocks. */

        KTX_TTF_ETC2_EAC_R11 = 20,
        /*!< R only (ETC2 EAC R11 unsigned). R = opaque.g or alpha.g, if
             KTX_TF_TRANSCODE_ALPHA_DATA_TO_OPAQUE_FORMATS flag is
             specified. */
        KTX_TTF_ETC2_EAC_RG11 = 21,
        /*!< RG only (ETC2 EAC RG11 unsigned), R=opaque.g, G=alpha.g. The
             texture should have an alpha channel (if not G will be all
             255's. For tangent space normal maps. */

        // Uncompressed (raw pixel) formats
        KTX_TTF_RGBA32 = 13,
        /*!< 32bpp RGBA image stored in raster (not block) order in
             memory, R is first byte, A is last byte. */
        KTX_TTF_RGB565 = 14,
        /*!< 16bpp RGB image stored in raster (not block) order in memory,
             R at bit position 11. */
        KTX_TTF_BGR565 = 15,
        /*!< 16bpp RGB image stored in raster (not block) order in memory,
             R at bit position 0. */
        KTX_TTF_RGBA4444 = 16,
        /*!< 16bpp RGBA image stored in raster (not block) order in memory,
             R at bit position 12, A at bit position 0. */

        // Values for automatic selection of RGB or RGBA depending if alpha
        // present.
        KTX_TTF_ETC = 22,
        /*!< Automatically selects @c KTX_TTF_ETC1_RGB or
             @c KTX_TTF_ETC2_RGBA according to presence of alpha. */
        KTX_TTF_BC1_OR_3 = 23,
        /*!< Automatically selects @c KTX_TTF_BC1_RGB or
             @c KTX_TTF_BC3_RGBA according to presence of alpha. */

        KTX_TTF_NOSELECTION = 0x7fffffff,
     
    }
}
