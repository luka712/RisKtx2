namespace RisKtx2
{
    /// <summary>
    /// The KtxErrorCode enumeration defines the various error codes that can be returned by functions in the KTX library.
    /// These error codes provide information about the type of error that occurred during operations such as loading, transcoding, or uploading textures. By using these error codes, developers can handle errors more effectively and provide better feedback to users when issues arise with KTX texture processing.
    /// </summary>
    public enum KtxErrorCode : int
    {
        /// <summary>
        /// The operation was successful.
        /// </summary>
        KTX_SUCCESS = 0,
        KTX_FILE_DATA_ERROR,     /*!< The data in the file is inconsistent with the spec. */
        KTX_FILE_ISPIPE,         /*!< The file is a pipe or named pipe. */
        
        /// <summary>
        /// The target file could not be opened.
        /// </summary>
        KTX_FILE_OPEN_FAILED,
        KTX_FILE_OVERFLOW,       /*!< The operation would exceed the max file size. */
        KTX_FILE_READ_ERROR,     /*!< An error occurred while reading from the file. */
        KTX_FILE_SEEK_ERROR,     /*!< An error occurred while seeking in the file. */
        KTX_FILE_UNEXPECTED_EOF, /*!< File does not have enough data to satisfy request. */
        KTX_FILE_WRITE_ERROR,    /*!< An error occurred while writing to the file. */
        KTX_GL_ERROR,            /*!< GL operations resulted in an error. */
        /// <summary>
        /// The operation is not allowed in the current state.
        /// </summary>
        KTX_INVALID_OPERATION,
        KTX_INVALID_VALUE,       /*!< A parameter value was not valid. */
        KTX_NOT_FOUND,           /*!< Requested metadata key or required dynamically loaded GPU function was not found. */
        KTX_OUT_OF_MEMORY,       /*!< Not enough memory to complete the operation. */
        KTX_TRANSCODE_FAILED,    /*!< Transcoding of block compressed texture failed. */
        KTX_UNKNOWN_FILE_FORMAT, /*!< The file not a KTX file */
        KTX_UNSUPPORTED_TEXTURE_TYPE, /*!< The KTX file specifies an unsupported texture type. */
        KTX_UNSUPPORTED_FEATURE,  /*!< Feature not included in in-use library or not yet implemented. */
        KTX_LIBRARY_NOT_LINKED,  /*!< Library dependency (OpenGL or Vulkan) not linked into application. */
        KTX_DECOMPRESS_LENGTH_ERROR, /*!< Decompressed byte count does not match expected byte size */
        KTX_DECOMPRESS_CHECKSUM_ERROR, /*!< Checksum mismatch when decompressing */
        KTX_ERROR_MAX_ENUM = KTX_DECOMPRESS_CHECKSUM_ERROR /*!< For safety checks. */

    }

    /// <summary>
    /// The <see cref="KtxTextureCreateStorage"/> enum defines options for allocating image storage when creating a KTX texture.
    /// </summary>
    public enum KtxTextureCreateStorage : uint
    {
        /// <summary>
        /// Don't allocate any image storage.
        /// </summary>
        NO_STORAGE = 0,

        /// <summary>
        /// Allocate image storage.
        /// </summary>
        ALLOC_STORAGE = 1
    }

    /// <summary>
    /// The KtxTextureCreateFlags enumeration defines flags that can be used to control the behavior of texture creation when loading KTX files.
    /// </summary>
    public enum KtxTextureCreateFlags : uint
    {
        /// <summary>
        /// No special handling. The texture will be created with the default behavior, which includes loading image data and key-value data from the KTX source.
        /// </summary>
        NO_FLAGS = 0x00,

        /// <summary>
        /// Load the image data from the KTX source.
        /// If this flag is not set, the texture will be created without loading the image data, which can be useful for scenarios where
        /// you only need to access metadata or key-value data without needing the actual texture data in memory.
        /// </summary>
        TEXTURE_CREATE_LOAD_IMAGE_DATA_BIT = 0x01,

        TEXTURE_CREATE_RAW_KVDATA_BIT = 0x02,
        /*!< Load the raw key-value data instead of
             creating a @c ktxHashList from it. */
        TEXTURE_CREATE_SKIP_KVDATA_BIT = 0x04,
        /*!< Skip any key-value data. This overrides
             the RAW_KVDATA_BIT. */
        TEXTURE_CREATE_CHECK_GLTF_BASISU_BIT = 0x08
        /*!< Load texture compatible with the rules
             of KHR_texture_basisu glTF extension */
    }

    /// <summary>
    /// The KtxTranscodeFlags enumeration defines flags that can be used to control the transcoding process when converting BasisU/ETC1S or UASTC compressed textures to other formats. These flags can specify options such as how to handle non-power-of-two textures, whether to transcode alpha data for opaque formats, and whether to request higher quality transcoding for certain formats.
    /// </summary>
    public enum KtxTranscodeFlags : uint
    {
        /// <summary>
        /// No special transcoding options.
        NONE = 0,

        KTX_TF_PVRTC_DECODE_TO_NEXT_POW2 = 2,
        /*!< PVRTC1: decode non-pow2 ETC1S texture level to the next larger
             power of 2 (not implemented yet, but we're going to support it).
             Ignored if the slice's dimensions are already a power of 2.
         */
        KTX_TF_TRANSCODE_ALPHA_DATA_TO_OPAQUE_FORMATS = 4,
        /*!< When decoding to an opaque texture format, if the Basis data has
             alpha, decode the alpha slice instead of the color slice to the
             output texture format. Has no effect if there is no alpha data.
         */
        KTX_TF_HIGH_QUALITY = 32,
        /*!< Request higher quality transcode of UASTC to BC1, BC3, ETC2_EAC_R11 and
             ETC2_EAC_RG11. The flag is unused by other UASTC transcoders.
         */
    }
}
