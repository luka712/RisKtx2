//namespace RisKtx2
//{
//    /// <summary>
//    /// Class responsible for loading and halding KTX2 textures.
//    /// </summary>
//    public class Ktx2Loader
//    {
//        /// <summary>
//        /// Loads a KTX2 texture from the specified file path, optionally transcoding it if required,
//        /// and returns a <see cref="RawImage"/> containing the decoded texture data for the requested mip, layer, and face slice.
//        /// </summary>
//        /// <param name="filePath">
//        /// The file path to the KTX2 texture on disk.
//        /// </param>
//        /// <param name="transcodeFormat">
//        /// The Basis Universal transcode format to use if the texture requires transcoding
//        /// (e.g., BC7, ETC2, ASTC, or RGBA8 depending on platform support).
//        /// </param>
//        /// <param name="mipLevel">
//        /// The mipmap level to load. Level 0 is the base resolution texture.
//        /// Higher values return progressively smaller mip levels.
//        /// </param>
//        /// <param name="layer">
//        /// The array layer index for array textures. Use 0 for non-array textures.
//        /// </param>
//        /// <param name="faceSlice">
//        /// The cube map face index or 3D texture slice index, depending on texture type.
//        /// Use 0 for standard 2D textures.
//        /// </param>
//        /// <returns>
//        /// A <see cref="RawImage"/> containing the texture data, dimensions, and format information
//        /// for the specified mip level, layer, and face slice.
//        /// </returns>
//        public RawImage Load(string filePath, KtxTranscodeFormat transcodeFormat, uint mipLevel = 0, uint layer = 0, uint faceSlice = 0)
//        {
//            using var texture = new Ktx2Texture(filePath);

//            if (texture.NeedsTranscoding)
//            {
//                texture.TranscodeBasis(transcodeFormat);
//            }

//            //if (mipLevel >= texture.LevelCount)
//            //    throw new ArgumentOutOfRangeException(nameof(mipLevel));

//            var offset = texture.GetImageOffset(mipLevel, layer, faceSlice);
//            var dataPtr = texture.GetTextureData(offset);

//            uint width = System.Math.Max(1u, texture.Width >> (int)mipLevel);
//            uint height = System.Math.Max(1u, texture.Height >> (int)mipLevel);

//            ulong imageSize = texture.GetImageSize(mipLevel);

//            byte[] data = new byte[imageSize];
//            System.Runtime.InteropServices.Marshal.Copy(dataPtr, data, 0, (int)imageSize);

//            var formatInfo = CreateTextureFormatInfo(transcodeFormat);

//            return new RawImage(
//                filePath,
//                (int)width,
//                (int)height,
//                data,
//                formatInfo
//            );
//        }

//        /// <summary>
//        /// This method loads a KTX2 texture from the specified file path and returns a <see cref="Ktx2Texture"/> instance.
//        /// </summary>
//        /// <param name="filePath">The file path.</param>
//        /// <returns>The <see cref="Ktx2Texture"/>.</returns>
//        public Ktx2Texture Load(string filePath)
//        {
//            return new Ktx2Texture(filePath);
//        }

//        /// <summary>
//        /// Creates a TextureFormatInfo instance based on the provided KtxTranscodeFormat.
//        /// </summary>
//        /// <param name="format">The <see cref="KtxTranscodeFormat"/>.</param>
//        /// <returns>The <see cref="TextureFormatInfo"/>.</returns>
//        private TextureFormatInfo CreateTextureFormatInfo(KtxTranscodeFormat format)
//        {
//            return format switch
//            {
//                KtxTranscodeFormat.BC7_RGBA => new TextureFormatInfo(4, 4, 1, 16),
//                _ => throw new NotSupportedException($"Unsupported transcode format: {format}")
//            };
//        }
//    }
//}
