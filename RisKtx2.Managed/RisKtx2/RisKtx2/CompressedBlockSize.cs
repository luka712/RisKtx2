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
        public uint BytesPerBlock { get; }

        /// <summary>
        /// Creates a new <see cref="TextureFormatInfo"/> instance.
        /// </summary>
        /// <param name="blockWidth">Width of a block in pixels.</param>
        /// <param name="blockHeight">Height of a block in pixels.</param>
        /// <param name="blockDepth">Depth of a block in pixels.</param>
        /// <param name="bytesPerBlock">Size of a block in bytes.</param>
        public TextureFormatInfo(uint blockWidth, uint blockHeight, uint blockDepth, uint bytesPerBlock)
        {
            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
            BlockDepth = blockDepth;
            BytesPerBlock = bytesPerBlock;
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
        /// Gets the number of block rows required to cover a height.
        /// </summary>
        public uint GetBlocksPerColumn(uint height)
        {
            return (height + BlockHeight - 1) / BlockHeight;
        }

        /// <summary>
        /// Gets the number of block slices required to cover a depth.
        /// </summary>
        public uint GetBlocksPerSlice(uint depth)
        {
            return (depth + BlockDepth - 1) / BlockDepth;
        }

        /// <summary>
        /// Gets the unaligned number of bytes in a row of blocks.
        /// </summary>
        public ulong GetBytesPerRow(uint width)
        {
            return (ulong)GetBlocksPerRow(width) * BytesPerBlock;
        }

        /// <summary>
        /// Gets the WebGPU-aligned bytes-per-row value.
        /// WebGPU requires bytesPerRow to be a multiple of 256.
        /// </summary>
        public ulong GetAlignedBytesPerRow(uint width)
        {
            ulong bytesPerRow = GetBytesPerRow(width);
            return (bytesPerRow + 255) & ~255UL;
        }

        /// <summary>
        /// Computes the size in bytes of a 2D texture level.
        /// </summary>
        public ulong GetDataSize(uint width, uint height)
        {
            ulong blocksX = GetBlocksPerRow(width);
            ulong blocksY = GetBlocksPerColumn(height);

            return blocksX * blocksY * BytesPerBlock;
        }

        /// <summary>
        /// Computes the size in bytes of a 3D texture level.
        /// </summary>
        public ulong GetDataSize(uint width, uint height, uint depth)
        {
            ulong blocksX = GetBlocksPerRow(width);
            ulong blocksY = GetBlocksPerColumn(height);
            ulong blocksZ = GetBlocksPerSlice(depth);

            return blocksX * blocksY * blocksZ * BytesPerBlock;
        }

        /// <summary>
        /// Predefined format info for BC7 compressed textures.
        /// </summary>
        public static TextureFormatInfo BC7 { get; } =
            new TextureFormatInfo(4, 4, 1, 16);

        /// <summary>
        /// Predefined format info for BC3 compressed textures.
        /// </summary>
        public static TextureFormatInfo BC3 { get; } =
            new TextureFormatInfo(4, 4, 1, 16);

        /// <summary>
        /// Predefined format info for ETC2 RGBA compressed textures.
        /// </summary>
        public static TextureFormatInfo ETC2_RGBA { get; } =
            new TextureFormatInfo(4, 4, 1, 16);

        /// <summary>
        /// Predefined format info for ASTC 4x4 RGBA compressed textures.
        /// </summary>
        public static TextureFormatInfo ASTC_4X4_RGBA { get; } =
            new TextureFormatInfo(4, 4, 1, 16);

        /// <summary>
        /// Predefined format info for uncompressed RGBA8 textures.
        /// </summary>
        public static TextureFormatInfo RGBA32 { get; } =
            new TextureFormatInfo(1, 1, 1, 4);
    }

}
