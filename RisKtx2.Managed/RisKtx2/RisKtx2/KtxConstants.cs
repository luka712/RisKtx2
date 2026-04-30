namespace RisKtx2
{
    /// <summary>
    /// This class serves as a container for constant values related to KTX texture processing.
    /// </summary>
    public class KtxConstants
    {
        /// <summary>
        /// The default compression level used by the BasisU encoder when no value is set.
        /// </summary>
        public const uint ETC1S_DEFAULT_COMPRESSION_LEVEL = 2;

        /// <summary>
        /// The default quality level used by the BasisU encoder when no value is set for QualityLevel or both maxEndpoints and maxSelectors.
        /// </summary>
        public const uint BASISU_DEFAULT_QUALITY_LEVEL = 128;
    }
}
