namespace RisKtx2
{
    /// <summary>
    /// The supercompression scheme used in a KTX2 texture.
    /// </summary>
    public enum SupercompressionScheme
    {
        /// <summary>
        /// No supercompression. The texture data is stored in its original compressed or uncompressed format.
        /// </summary>
        NONE = 0,            
        /// <summary>
        /// The Basis LZ supercompression scheme.
        /// </summary>
        BASIS_LZ = 1,       
        /// <summary>
        /// The ZStd supercompression scheme.
        /// </summary>
        ZSTD = 2,

        /// <summary>
        /// The ZLIB supercompression scheme. This is a legacy option and is not recommended for new textures.
        /// </summary>
        ZLIB = 3,            
    }
}
