using System.Runtime.InteropServices;

namespace RisKtx2.Native
{
    /// <summary>
    /// Provides P/Invoke signatures for interacting with the native KTX2 texture handling functions 
    /// in the Ris Texture Toolkit native library.
    /// </summary>
    internal static class RisKtx2
    {
        private const string DLL_NAME = "ris_ktx2";

        #region Texture Creation and Loading

        /// <summary>
        /// Creates a KTX2 texture from the specified creation information.
        /// </summary>
        /// <param name="createInfo">The texture creation information.</param>
        /// <param name="storageAllocation">The storage allocation flag.</param>
        /// <param name="texture">The output texture pointer.</param>
        /// <returns>The error code indicating success or failure.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern KtxErrorCode ris_ktxTexture2_Create(
            in ris_ktxTextureCreateInfo createInfo,
            uint storageAllocation,
            out IntPtr texture);

        /// <summary>
        /// Creates a KTX2 texture from a named file.
        /// </summary>
        /// <param name="fileName">The path to the KTX2 file.</param>
        /// <param name="flags">The texture creation flags.</param>
        /// <param name="outTexture">The output texture pointer.</param>
        /// <returns>The error code indicating success or failure.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern KtxErrorCode ris_ktxTexture2_CreateFromNamedFile(
            string fileName,
            KtxTextureCreateFlags flags,
            out IntPtr outTexture);

        #endregion

        #region Transcoding

        /// <summary>
        /// Transcodes a Basis Universal compressed texture to the specified output format.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <param name="outputFormat">The desired output format.</param>
        /// <param name="transcodeFlags">The transcode flags.</param>
        /// <returns>The error code indicating success or failure.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern KtxErrorCode ris_ktxTexture2_TranscodeBasis(
            IntPtr texture,
            KtxTranscodeFormat outputFormat,
            KtxTranscodeFlags transcodeFlags);

        /// <summary>
        /// Checks if the texture needs transcoding (i.e., is Basis Universal compressed).
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <returns>True if the texture needs transcoding, false otherwise.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ris_ktxTexture2_NeedsTranscoding(IntPtr texture);

        #endregion

        #region Compression

        /// <summary>
        /// Compresses the texture using Basis Universal compression with the specified parameters.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <param name="basisParams">Pointer to the Basis compression parameters.</param>
        /// <returns>The error code indicating success or failure.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern KtxErrorCode ris_ktxTexture2_CompressBasisEx(
            IntPtr texture,
            IntPtr basisParams);

        // TODO: doc comment
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern KtxErrorCode ris_ktxTexture2_CompressBasis(IntPtr texture, uint quality);

        // TODO: doc comment
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern KtxErrorCode ris_ktxTexture2_CompressAstc(IntPtr texture, uint quality);

        // TODO: doc comment
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern KtxErrorCode ris_ktxTexture2_CompressAstcEx(IntPtr texture, IntPtr astcParams);

        #endregion

        #region Data Access

        /// <summary>
        /// Gets a pointer to the texture data.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <returns>Pointer to the texture data.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ris_ktxTexture2_GetData(IntPtr texture);

        /// <summary>
        /// Gets a pointer to the image data for the texture.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <returns>Pointer to the image data.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ris_ktxTexture2_GetImageData(IntPtr texture);

        /// <summary>
        /// Gets the size of the texture data in bytes.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <returns>The data size.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong ris_ktxTexture2_GetDataSize(IntPtr texture);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ris_ktxTexture2_GetPData(IntPtr texture);

        /// <summary>
        /// Sets the image data for a specific level, layer, and face/slice of the texture.
        /// </summary>
        /// <param name="ktx2TexturePtr">The texture pointer.</param>
        /// <param name="level">The mipmap level.</param>
        /// <param name="layer">The array layer.</param>
        /// <param name="faceSlice">The face or slice index.</param>
        /// <param name="src">Pointer to the source data.</param>
        /// <param name="srcSize">Size of the source data in bytes.</param>
        /// <returns>The error code indicating success or failure.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern KtxErrorCode ris_ktxTexture2_SetImageFromMemory(
            IntPtr ktx2TexturePtr,
            uint level,
            uint layer,
            uint faceSlice,
            IntPtr src,
            ulong srcSize);

        /// <summary>
        /// Gets the offset of the image data for a specific level, layer, and face/slice.
        /// </summary>
        /// <param name="ktx2TexturePtr">The texture pointer.</param>
        /// <param name="level">The mipmap level.</param>
        /// <param name="layer">The array layer.</param>
        /// <param name="faceSlice">The face or slice index.</param>
        /// <param name="offset">The output offset in bytes.</param>
        /// <returns>The error code indicating success or failure.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern KtxErrorCode ris_ktxTexture2_GetImageOffset(
            IntPtr ktx2TexturePtr,
            uint level,
            uint layer,
            uint faceSlice,
            out ulong offset);

        /// <summary>
        /// Gets the size of the image data for a specific mipmap level.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <param name="level">The mipmap level.</param>
        /// <returns>The size of the image data in bytes.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong ris_ktxTexture2_GetImageSize(IntPtr texture, uint level);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint ris_ktxTexture2_GetRowPitch(IntPtr texture, uint level);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint ris_ktxTexture2_GetElementSize(IntPtr texture);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the width of the base level of the texture.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <returns>The width in pixels.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint ris_ktxTexture2_GetWidth(IntPtr texture);

        /// <summary>
        /// Gets the height of the base level of the texture.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <returns>The height in pixels.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint ris_ktxTexture2_GetHeight(IntPtr texture);

        /// <summary>
        /// Gets the supercompression scheme used by the texture.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <returns>The supercompression scheme.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern SupercompressionScheme ris_ktxTexture2_GetSupercompressionScheme(IntPtr texture);

        #endregion

        #region File I/O

        /// <summary>
        /// Writes the texture to a file at the specified path.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <param name="filename">The output file path.</param>
        /// <returns>The error code indicating success or failure.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern KtxErrorCode ris_ktxTexture2_WriteToNamedFile(
            IntPtr texture,
            string filename);

        #endregion

        #region Resource Management

        /// <summary>
        /// Destroys the texture and releases associated resources.
        /// </summary>
        /// <param name="texture">The texture pointer to destroy.</param>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ris_ktxTexture2_Destroy(IntPtr texture);

        #endregion

        #region Querying

        /// <summary>
        /// Gets the number of mipmap levels in the texture.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <returns>The number of mipmap levels.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint ris_ktxTexture2_GetNumLevels(IntPtr texture);

        /// <summary>
        /// Gets the Vulkan format of the texture.
        /// </summary>
        /// <param name="texture">The texture pointer.</param>
        /// <returns>The Vulkan format.</returns>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern VkFormat ris_ktxTexture2_GetVkFormat(IntPtr texture);

        #endregion
    }
}