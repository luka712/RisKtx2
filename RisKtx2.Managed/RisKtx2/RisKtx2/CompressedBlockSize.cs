namespace RisKtx2
{
    /// <summary>
    /// Describes how a texture format is laid out in memory in terms of blocks.
    /// This abstraction works for both compressed and uncompressed formats.
    /// </summary>
    /// <remarks>
    /// For uncompressed formats, a "block" is equivalent to a single pixel:
    /// BlockWidth = 1, BlockHeight = 1, BlockSizeInBytes = bytes per pixel.
    ///
    /// For compressed formats (e.g. BC7), a block represents a group of pixels
    /// (typically 4x4) stored in a fixed number of bytes.
    /// </remarks>
    public class TextureFormatInfo
    {
        /// <summary>
        /// Width of a single block in pixels.
        /// For example, BC formats use 4.
        /// </summary>
        public uint BlockWidth { get; }

        /// <summary>
        /// Height of a single block in pixels.
        /// For example, BC formats use 4.
        /// </summary>
        public uint BlockHeight { get; }

        /// <summary>
        /// Depth of a single block in pixels.
        /// Typically 1 for 2D textures. Used for 3D textures.
        /// </summary>
        public uint BlockDepth { get; }

        /// <summary>
        /// Size of a single block in bytes.
        /// For example, BC7 uses 16 bytes per 4x4 block.
        /// </summary>
        public uint BlockSizeInBytes { get; }

        /// <summary>
        /// Creates a new <see cref="TextureFormatInfo"/> instance.
        /// </summary>
        /// <param name="blockWidth">Width of a block in pixels.</param>
        /// <param name="blockHeight">Height of a block in pixels.</param>
        /// <param name="blockDepth">Depth of a block in pixels.</param>
        /// <param name="blockSizeInBytes">Size of a block in bytes.</param>
        public TextureFormatInfo(uint blockWidth, uint blockHeight, uint blockDepth, uint blockSizeInBytes)
        {
            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
            BlockDepth = blockDepth;
            BlockSizeInBytes = blockSizeInBytes;
        }

        /// <summary>
        /// Computes the number of blocks per row for a given texture width.
        /// </summary>
        /// <param name="width">Texture width in pixels.</param>
        /// <returns>Number of blocks required to cover the width.</returns>
        public uint GetBlocksPerRow(uint width)
        {
            return (width + BlockWidth - 1) / BlockWidth;
        }

        /// <summary>
        /// Computes the number of rows per image for a given texture height.
        /// </summary>
        /// <param name="height">The height in pixels.</param>
        /// <returns>Number of rows per image.</returns>
        public uint RowsPerImage(uint height)
        {
            return (height + BlockHeight - 1) / BlockHeight;
        }

        /// <summary>
        /// Computes the number of blocks per column for a given texture height.
        /// </summary>
        /// <param name="height">Texture height in pixels.</param>
        /// <returns>Number of blocks required to cover the height.</returns>
        public uint GetBlocksPerColumn(uint height)
        {
            return (height + BlockHeight - 1) / BlockHeight;
        }

        /// <summary>
        /// Computes the unaligned number of bytes per row.
        /// </summary>
        /// <param name="width">Texture width in pixels.</param>
        /// <returns>Bytes per row without alignment padding.</returns>
        public uint GetBytesPerRow(uint width)
        {
            return GetBlocksPerRow(width) * BlockSizeInBytes;
        }

        /// <summary>
        /// Computes the WebGPU-aligned number of bytes per row.
        /// WebGPU requires bytesPerRow to be a multiple of 256.
        /// </summary>
        /// <param name="width">Texture width in pixels.</param>
        /// <returns>Aligned bytes per row.</returns>
        public uint GetAlignedBytesPerRow(uint width)
        {
            uint bytesPerRow = GetBytesPerRow(width);
            return (bytesPerRow + 255) & ~255u;
        }

        /// <summary>
        /// Computes the total data size in bytes for a 2D texture.
        /// </summary>
        /// <param name="width">Texture width in pixels.</param>
        /// <param name="height">Texture height in pixels.</param>
        /// <returns>Total size in bytes.</returns>
        public uint GetDataSize(uint width, uint height)
        {
            uint blocksX = GetBlocksPerRow(width);
            uint blocksY = GetBlocksPerColumn(height);
            return blocksX * blocksY * BlockSizeInBytes;
        }

        /// <summary>
        /// Predefined format info for BC7 compressed textures.
        /// </summary>
        /// <remarks>
        /// BC7 uses 4x4 pixel blocks, each 16 bytes in size.
        /// </remarks>
        public static TextureFormatInfo BC7 { get; } =
            new TextureFormatInfo(4, 4, 1, 16);

        /// <summary>
        /// Predefined format info for uncompressed RGBA8 textures.
        /// </summary>
        public static TextureFormatInfo RGBA8 { get; } =
            new TextureFormatInfo(1, 1, 1, 4);
    }

}
