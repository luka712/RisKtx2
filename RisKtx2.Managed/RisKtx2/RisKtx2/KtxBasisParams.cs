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
        /// ETC1S compression effort level.
        /// Range is [0,6].
        /// Higher values are much slower but give slightly higher quality.
        /// <para/>
        /// Higher levels are intended for video.
        /// <para/>
        /// This parameter controls numerous internal encoding speed vs. compression efficiency/performance tradeoffs.
        /// Note this is NOT the same as the ETC1S quality level, and most users shouldn't change this.
        /// There is no default.
        /// Callers must explicitly set this value.
        /// Callers can use KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL as a default value. Currently, this is <c>2</c>.
        /// </summary>
        /// <remarks>
        /// This controls how hard the encoder works to optimize compression.
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
        /// <remarks>
        /// This controls the visual quality target of ETC1S encoding.
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
        /// The full range is [.001,50.0].
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
                uastc = Uastc,
                qualityLevel = QualityLevel,
                etc1sCompressionLevel = ETC1SCompressionLevel,
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

                risKtxBasisParams.inputSwizzleR = (byte) InputSwizzle[0];
                risKtxBasisParams.inputSwizzleG = (byte) InputSwizzle[1];
                risKtxBasisParams.inputSwizzleB = (byte) InputSwizzle[2];
                risKtxBasisParams.inputSwizzleA = (byte) InputSwizzle[3];
            }

            return risKtxBasisParams;
        }
    }

    /// <summary>
    /// Structure for passing extended parameters to ktxTexture2_CompressBasisEx().
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
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
            inputSwizzleR = 0;
            inputSwizzleG = 0;
            inputSwizzleB = 0;
            inputSwizzleA = 0;
        }

        public uint etc1sCompressionLevel;
        public uint qualityLevel;

        [MarshalAs(UnmanagedType.I1)]
        public bool normalMap;

        public uint threadCount;

        [MarshalAs(UnmanagedType.I1)]
        public bool uastc;

        [MarshalAs(UnmanagedType.I1)]
        public bool uastcRDO;

        public float uastcRDOQualityScalar;

        [MarshalAs(UnmanagedType.I1)]
        public bool verbose;

        public byte inputSwizzleR;
        public byte inputSwizzleG;
        public byte inputSwizzleB;
        public byte inputSwizzleA;
    }

}
