using System.Runtime.InteropServices;

namespace RisKtx2
{
    /// <summary>
    /// Structure for passing extended parameters to ktxTexture2_CompressBasisEx().
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KtxBasisParams
    {
        private uint _etc1sCompressionLevel = KtxConstants.ETC1S_DEFAULT_COMPRESSION_LEVEL;
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
        public uint ETC1SCompressionLevel
        {
            get => _etc1sCompressionLevel;
            set
            {
                if (value > 6)
                    throw new ArgumentOutOfRangeException(nameof(ETC1SCompressionLevel), "CompressionLevel must be in the range [0, 6].");
                _etc1sCompressionLevel = value;
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
        public bool Uastc { get; set; }

        /// <summary>
        /// Tunes codec parameters for better quality on normal maps (no selector RDO, no endpoint RDO) and sets the texture's DFD appropriately.
	    /// Only valid for linear textures. 
        /// </summary>
        public bool NormalMap { get; set; }

        /// <summary>
        /// Number of threads used for compression. Default is <c>1</c>.
        /// </summary>
        public uint ThreadCount { get; set; } = 1;

        /// <summary>
        /// A swizzle to apply before encoding.
        /// It must match the regular expression /^[rgba01]{4}$/.
        /// If both this and preSwizzle are specified ktxTexture_CompressBasisEx will raise KTX_INVALID_OPERATION.
        /// Usable with both ETC1S and UASTC. 
        /// </summary>
        public char[] InputSwizzle { get; set; }

        ///<summary>
        ///Enable Rate Distortion Optimization (RDO) post-processing.
        ///</summary>
        public bool UastcRDO { get; set; }

        /// <summary>
        /// UASTC RDO quality scalar (lambda).
        /// Lower values yield higher quality/larger LZ compressed files, higher values yield lower quality/smaller LZ compressed files. 
        /// A good range to try is [.2,4].
        /// Full range is [.001,50.0].
        /// Default is 1.0. 
        /// </summary>
        public float UastcRDOQualityScalar { get; set; }

        /// <summary>
        /// If <c>true</c>>, prints Basis Universal encoder operation details to output stream.
	    /// Not recommended for GUI apps. 
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Converts this managed KtxBasisParams instance to its native representation (ris_ktxBasisParams) for use in interop calls.
        /// </summary>
        /// <returns>The <see cref="ris_ktxBasisParams"/>.</returns>
        internal ris_ktxBasisParams ToNative()
        {
            var risKtxBasisParams = new ris_ktxBasisParams()
            {
                etc1sCompressionLevel = ETC1SCompressionLevel,
                qualityLevel = QualityLevel,
                uastc = Uastc,
                normalMap = NormalMap,
                threadCount = ThreadCount,
                uastcRDO = UastcRDO,
                uastcRDOQualityScalar = UastcRDOQualityScalar,
                verbose = Verbose
            };

            // Handle the InputSwizzle array. We need to pin it in memory to get a stable pointer for the native code.
            if (InputSwizzle != null)
            {
                if (InputSwizzle.Length != 4)
                {
                    throw new ArgumentException("InputSwizzle must be an array of 4 characters.", nameof(InputSwizzle));
                }

                risKtxBasisParams.inputSwizzleR = InputSwizzle[0];
                risKtxBasisParams.inputSwizzleG = InputSwizzle[1];
                risKtxBasisParams.inputSwizzleB = InputSwizzle[2];
                risKtxBasisParams.inputSwizzleA = InputSwizzle[3];
            }

            return risKtxBasisParams;
        }
    }

    /// <summary>
    /// Structure for passing extended parameters to ktxTexture2_CompressBasisEx().
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct ris_ktxBasisParams
    {
        public ris_ktxBasisParams()
        {
            etc1sCompressionLevel = 0;
            qualityLevel = 0;
            normalMap = false;
            threadCount = 0;
            uastc = false;
            uastcRDO = false;
            uastcRDOQualityScalar = 0.0f;
            verbose = false;
            inputSwizzleR = '\0';
            inputSwizzleG = '\0';
            inputSwizzleB = '\0';
            inputSwizzleA = '\0';
        }

        internal uint etc1sCompressionLevel;
        internal uint qualityLevel;
        internal bool normalMap;
        internal uint threadCount;
        internal char inputSwizzleR;
        internal char inputSwizzleG;
        internal char inputSwizzleB;
        internal char inputSwizzleA;
        internal bool uastc;
        internal bool uastcRDO;
        internal float uastcRDOQualityScalar;
        internal bool verbose;
    }

}
