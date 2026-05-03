using RisKtx2.Native;
using static RisKtx2.Native.RisKtx2;
using static System.Net.Mime.MediaTypeNames;

namespace RisKtx2
{
    /// <summary>
    /// The KtxTexture class represents a texture loaded from a KTX file. 
    /// It provides methods for loading, transcoding, and retrieving texture data.
    /// The class manages the lifecycle of the native KTX texture object, ensuring that resources are properly released when the texture is no longer needed. 
    /// It also includes error handling to provide informative exceptions when operations fail, such as loading or transcoding errors.
    /// </summary>
    public class Ktx2Texture : IDisposable
    {
        private static bool _firstLoad = true;

        private readonly string _filePath;

        /// <summary>
        /// The constructor for the KtxTexture class. It attempts to load a KTX texture from the specified file path. 
        /// If the loading process fails, it throws an exception with a descriptive error message.
        /// </summary>
        /// <param name="filePath">The file path to the .ktx or .ktx2 texture.</param>
        /// <param name="createFlags">The <see cref="KtxTextureCreateFlags"/>. By default, it is <c>TEXTURE_CREATE_LOAD_IMAGE_DATA_BIT</c>.</param>
        /// <param name="transcodeFlags">The <see cref="KtxTranscodeFlags"/>. By default, it is <c>0</c>.</param>
        /// <exception cref="Exception">
        /// If the texture fails to load from the specified file path, an exception is thrown with details about the failure.
        /// </exception>
        public Ktx2Texture(string filePath,
            KtxTextureCreateFlags createFlags = KtxTextureCreateFlags.TEXTURE_CREATE_LOAD_IMAGE_DATA_BIT)
        {
            NativeResolver.Setup();
            _filePath = filePath;

            TexturePtr = IntPtr.Zero;

            KtxErrorCode errorCode = ris_ktxTexture2_CreateFromNamedFile(_filePath, createFlags, out IntPtr texture);
            if (errorCode == KtxErrorCode.FILE_OPEN_FAILED)
            {
                throw new FileNotFoundException($"The specified KTX file '{_filePath}' could not be found.");
            }
            else if (errorCode != KtxErrorCode.KTX_SUCCESS)
            {
                throw new Exception($"Failed to load KTX texture from '{_filePath}'. Error code: {errorCode}");
            }
            TexturePtr = texture;
        }

        /// <summary>
        /// Creates a KTX texture using the specified creation information.
        /// </summary>
        /// <param name="createInfo">The <see cref="KtxTextureCreateInfo"/>.</param>
        /// <param name="storageAllocation">The <see cref="KtxTextureCreateStorage"/>.</param>
        /// <exception cref="Exception">
        /// Throws an exception if the texture creation fails, providing details about the error code returned by the native function.
        /// </exception>
        public Ktx2Texture(
            KtxTextureCreateInfo createInfo,
            KtxTextureCreateStorage storageAllocation = KtxTextureCreateStorage.ALLOC_STORAGE)
        {
            NativeResolver.Setup();
            TexturePtr = IntPtr.Zero;
            uint storageAllocValue = (uint)storageAllocation;
            KtxErrorCode errorCode = ris_ktxTexture2_Create(createInfo, storageAllocValue, out IntPtr texture);
            if (errorCode != KtxErrorCode.KTX_SUCCESS)
            {
                throw new Exception($"Failed to create KTX texture. Error code: {errorCode}");
            }
            TexturePtr = texture;
        }

        /// <summary>
        /// The pointer to the native KTX texture object.
        /// This should be released using the appropriate native function when no longer needed.
        /// </summary>
        internal IntPtr TexturePtr { get; private set; }

        /// <summary>
        /// Gets the width of the texture.
        /// </summary>
        public uint Width => ris_ktxTexture2_GetWidth(TexturePtr);

        /// <summary>
        /// Gets the height of the texture.
        /// </summary>
        public uint Height => ris_ktxTexture2_GetHeight(TexturePtr);

        /// <summary>
        /// Gets the data size.
        /// </summary>
        public ulong DataSize => ris_ktxTexture2_GetDataSize(TexturePtr);

        /// <summary>
        /// Gets the supercompression scheme used for the texture, if any.
        /// </summary>
        public SupercompressionScheme SupercompressionScheme => ris_ktxTexture2_GetSupercompressionScheme(TexturePtr);

        public uint ElementSize => ris_ktxTexture2_GetElementSize(TexturePtr);

        /// <summary>
        /// Checks if the texture needs transcoding. 
        /// This is typically true for textures that are compressed using BasisU/ETC1S or UASTC formats 
        /// and have not yet been transcoded to a GPU-compatible format.
        /// </summary>
        public bool NeedsTranscoding => ris_ktxTexture2_NeedsTranscoding(TexturePtr);

        public IntPtr DataPtr => ris_ktxTexture2_GetPData(TexturePtr);

        /// <summary>
        /// Creates a TextureFormatInfo instance based on the provided KtxTranscodeFormat.
        /// </summary>
        /// <param name="format">The <see cref="KtxTranscodeFormat"/>.</param>
        /// <returns>The <see cref="TextureFormatInfo"/>.</returns>
        private TextureFormatInfo CreateTextureFormatInfo(KtxTranscodeFormat format)
        {
            return format switch
            {
                KtxTranscodeFormat.KTX_TTF_BC7_RGBA => new TextureFormatInfo(4, 4, 1, 16),
                _ => throw new NotSupportedException($"Unsupported transcode format: {format}")
            };
        }

        /// <summary>
        /// Gets the size of the image data for a specific mip level.
        /// </summary>
        /// <param name="mipLevel">The mip level.</param>
        /// <returns>The size of image.</returns>
        public ulong GetImageSize(uint mipLevel)
        {
            return ris_ktxTexture2_GetImageSize(TexturePtr, mipLevel);
        }

        public uint GetRowPitch(uint mipLevel)
        {
            return ris_ktxTexture2_GetRowPitch(TexturePtr, mipLevel);
        }

        /// <summary>
        /// Transcodes the basis texture to the specified transcode format.
        /// </summary>
        /// <param name="transcodeFormat">The <see cref="KtxTranscodeFormat"/>.</param>
        /// <param name="transcodeFlags">The <see cref="KtxTranscodeFlags"/>.</param>
        public void TranscodeBasis(KtxTranscodeFormat transcodeFormat, KtxTranscodeFlags transcodeFlags = KtxTranscodeFlags.NONE)
        {
            var errorCode = ris_ktxTexture2_TranscodeBasis(TexturePtr, transcodeFormat, transcodeFlags);
            if (errorCode == KtxErrorCode.KTX_INVALID_OPERATION)
            {
                throw new InvalidOperationException($"The specified transcode format '{transcodeFormat}' is not valid for transcoding the KTX texture.");
            }
            if (errorCode != KtxErrorCode.KTX_SUCCESS)
            {
                throw new Exception($"Failed to transcode KTX texture. Error code: {errorCode}");
            }
        }

        /// <summary>
        /// Sets the image data for a specific level, layer, and face/slice of the texture from a byte array in memory.
        /// </summary>
        /// <param name="level">
        /// The mipmap level of the texture to set the image data for. 
        /// Level 0 corresponds to the base level, and higher levels correspond to mipmap levels.
        /// </param>
        /// <param name="layer">
        /// The layer of the texture to set the image data for.
        /// Layer 0 corresponds to the first layer, and higher layers correspond to additional layers in array textures or 3D textures.
        /// </param>
        /// <param name="faceSlice">
        /// The face or slice of the texture to set the image data for.
        /// For cubemap textures, this corresponds to the specific face (e.g., positive X, negative X, positive Y, etc.).
        /// </param>
        /// <param name="src">The source data.</param>
        /// <param name="srcSize">The source data size.</param>
        /// <exception cref="Exception">
        /// If the operation to set the image data fails, an exception is thrown with details about the error code returned by the native function.
        /// </exception>
        public void SetImageFromMemory(uint level, uint layer, uint faceSlice, byte[] src, uint srcSize)
        {
            unsafe
            {
                fixed (byte* srcPtr = src)
                {
                    SetImageFromMemory(level, layer, faceSlice, (IntPtr)srcPtr, srcSize);
                }
            }
        }

        /// <summary>
        /// Sets the image data for a specific level, layer, and face/slice of the texture from a byte array in memory.
        /// </summary>
        /// <param name="level">
        /// The mipmap level of the texture to set the image data for. 
        /// Level 0 corresponds to the base level, and higher levels correspond to mipmap levels.
        /// </param>
        /// <param name="layer">
        /// The layer of the texture to set the image data for.
        /// Layer 0 corresponds to the first layer, and higher layers correspond to additional layers in array textures or 3D textures.
        /// </param>
        /// <param name="faceSlice">
        /// The face or slice of the texture to set the image data for.
        /// For cubemap textures, this corresponds to the specific face (e.g., positive X, negative X, positive Y, etc.).
        /// </param>
        /// <param name="src">The source data pointer.</param>
        /// <param name="srcSize">The source data size.</param>
        /// <exception cref="Exception">
        /// If the operation to set the image data fails, an exception is thrown with details about the error code returned by the native function.
        /// </exception>
        public void SetImageFromMemory(uint level, uint layer, uint faceSlice, IntPtr src, ulong srcSize)
        {
            KtxErrorCode errorCode = ris_ktxTexture2_SetImageFromMemory(TexturePtr, level, layer, faceSlice, src, srcSize);
            if (errorCode == KtxErrorCode.KTX_INVALID_OPERATION)
            {
                throw new InvalidOperationException($"No storage was allocated when the texture was created.");
            }
            if (errorCode != KtxErrorCode.KTX_SUCCESS)
            {
                throw new Exception($"Failed to set image data for KTX texture. Error code: {errorCode}");
            }
        }

        /// <summary>
        /// Gets the offset of the image data for a specific level, layer, and face/slice of the texture.
        /// </summary>
        /// <param name="level">The mip level of the image.</param>
        /// <param name="layer">The array layer level of the image.</param>
        /// <param name="faceSlice">The cube map face or depth slice of the image. </param>
        /// <returns>The offset.</returns>
        public ulong GetImageOffset(uint level, uint layer, uint faceSlice)
        {
            ulong offset;
            KtxErrorCode errorCode = ris_ktxTexture2_GetImageOffset(TexturePtr, level, layer, faceSlice, out offset);
            if (errorCode != KtxErrorCode.KTX_SUCCESS)
            {
                throw new Exception($"Failed to get image offset for KTX texture. Error code: {errorCode}");
            }
            return offset;
        }

        /// <summary>
        /// Gets the raw texture data from the native KTX texture object.
        /// </summary>
        /// <returns>The texture data as byte array.</returns>
        public IntPtr GetTextureData(ulong offset = 0)
        {
            return ris_ktxTexture2_GetData(TexturePtr) + (int)offset;
        }

        /// <summary>
        /// Writes the KTX texture to a file at the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void WriteToNamedFile(string filePath)
        {
            KtxErrorCode errorCode = ris_ktxTexture2_WriteToNamedFile(TexturePtr, filePath);
            if (errorCode != KtxErrorCode.KTX_SUCCESS)
            {
                throw new Exception($"Failed to write KTX texture to file '{filePath}'. Error code: {errorCode}");
            }
        }

        /// <summary>
        /// Compresses the texture using the specified basis parameters.
        /// </summary>
        /// <param name="basisParams">The <see cref="KtxBasisParams"/>.</param>
        public void CompressBasis(KtxBasisParams basisParams)
        {
            unsafe
            {
                ris_ktxBasisParams* basisParamsPtr = stackalloc ris_ktxBasisParams[1];
                *basisParamsPtr = basisParams.ToNative();
                var result = ris_ktxTexture2_CompressBasisEx(TexturePtr, (nint)basisParamsPtr);

                if (result != KtxErrorCode.KTX_SUCCESS)
                {
                    throw new Exception($"Failed to compress KTX texture with basis parameters. Error code: {result}");
                }
            }
        }

        /// <summary>
        /// Compresses a KTX2 texture using Basis Universal supercompression.
        ///
        /// The source images are encoded into Basis Universal format (typically ETC1S, depending on configuration)
        /// and stored in a supercompressed form inside the KTX2 container.
        ///
        /// This process replaces the original uncompressed image data and updates the texture metadata (including DFD)
        /// to reflect the new compressed state.
        ///
        /// <para/>
        /// After compression, the texture cannot be directly uploaded to the GPU. It must first be transcoded
        /// into a GPU-supported block-compressed format (such as ASTC, BC7, or ETC2) before use in rendering APIs.
        /// </summary>
        /// <param name="quality">
        /// Compression quality value in the range 1–255.
        /// If 0 is provided, a default value of 128 is used by the underlying implementation.
        /// Lower values produce faster compression and smaller files with lower quality.
        /// Higher values produce better quality but slower compression and larger output.
        /// </param>
        /// <exception cref="Exception">
        /// Thrown if the Basis compression operation fails.
        /// </exception>
        /// <remarks>
        /// Based on KTX-Software / libktx API:
        /// https://github.khronos.org/KTX-Software/libktx/group__writer.html#ga405c44d6daf8ddf83dc805810bf4f989
        /// </remarks>
        public void CompressBasis(uint quality)
        {
            if (quality > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be in the range 0-255.");
            }

            var result = ris_ktxTexture2_CompressBasis(TexturePtr, quality);

            if (result != KtxErrorCode.KTX_SUCCESS)
            {
                throw new Exception($"Failed to compress KTX texture with quality {quality}. Error code: {result}");
            }
        }

        /// <summary>
        /// Gets the number of mipmap levels.
        /// </summary>
        public uint NumLevels => ris_ktxTexture2_GetNumLevels(TexturePtr);

        /// <summary>
        /// Gets the Vulkan format of the texture.
        /// </summary>
        public VkFormat VkFormat => ris_ktxTexture2_GetVkFormat(TexturePtr);

        /// <inheritdoc/>
        public void Dispose()
        {
            if (TexturePtr != IntPtr.Zero)
            {
                ris_ktxTexture2_Destroy(TexturePtr);
                TexturePtr = IntPtr.Zero;
            }
        }
    }
}
