using System.Runtime.InteropServices;

namespace RisKtx2
{
    /// <summary>
    /// Structure for passing extended parameters to ktxTexture2_CompressBasisEx().
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KtxBasisParams
    {
        private uint _compressionLevel = KtxConstants.ETC1S_DEFAULT_COMPRESSION_LEVEL;
        private uint _qualityLevel = KtxConstants.BASISU_DEFAULT_QUALITY_LEVEL;

        /// <summary>
        /// The constructor.
        /// </summary>
        public KtxBasisParams()
        {

        }

        /// <summary>
        /// Gets or sets the compression level used for data processing.
        /// </summary>
        /// <remarks>
        /// Valid values range from 0 (no compression) to 6 (maximum compression).
        /// Higher values may result in better compression ratios at the cost of increased processing time.
        /// </remarks>
        public uint CompressionLevel
        {
            get => _compressionLevel;
            set
            {
                if (value > 6)
                    throw new ArgumentOutOfRangeException(nameof(CompressionLevel), "CompressionLevel must be in the range [0, 6].");
                _compressionLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the quality level for compression.
        /// </summary>
        /// <remarks>
        /// QualityLevel must be in the range [1, 255].
        /// Lower values give better compression and faster processing but lower quality,
        /// while higher values give less compression, higher quality, and slower processing.
        /// This parameter automatically determines values for maxEndpoints, maxSelectors, endpointRDOThreshold,
        /// and selectorRDOThreshold for the target quality level. 
        /// Setting these parameters overrides the values determined by QualityLevel,
        /// which defaults to 128 if neither it nor both of maxEndpoints and maxSelectors have been set.
        /// </remarks>
        public uint QualityLevel
        {
            get => _qualityLevel;
            set
            {
                if (value < 1 || value > 255)
                    throw new ArgumentOutOfRangeException(nameof(QualityLevel), "QualityLevel must be in the range [1, 255].");
                _qualityLevel = value;
            }
        }

        /// <summary>
        /// Specifies whether to use UASTC compression. 
        /// </summary>
        public bool UseUastc { get; set; }

        /// <summary>
        /// Converts this managed KtxBasisParams instance to its native representation (ris_ktxBasisParams) for use in interop calls.
        /// </summary>
        /// <returns>The <see cref="ris_ktxBasisParams"/>.</returns>
        internal ris_ktxBasisParams ToNative()
        {
            return new ris_ktxBasisParams
            {
                compressionLevel = CompressionLevel,
                qualityLevel = QualityLevel,
                useUastc = UseUastc
            };
        }
    }

    /// <summary>
    /// Structure for passing extended parameters to ktxTexture2_CompressBasisEx().
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ris_ktxBasisParams
    {
        public uint compressionLevel;
        public uint qualityLevel;
        public bool useUastc;
    }

}
