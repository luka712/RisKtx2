//! Structure for passing extended parameters to ktxTexture2_CompressBasisEx().
//! If you only want default values, use ktxTexture2_CompressBasis().Here, at a minimum you must initialize the structure as follows :
//! <code>
//! ris_ktxBasisParams params = { 0 };
//! ris_ktxBasisParams.etc1sCompressionLevel = KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL;
//! </code>
//! compressionLevel has to be explicitly set because 0 is a valid compressionLevel but is not the default used by the BasisU encoder when no value is set.Only the other settings that are to be non - default must be non - zero.
struct ris_ktxBasisParams
{
	//! ETC1S compression effort level. 
	//! Range is [0,6]. Higher values are much slower, but give slightly higher quality. 
	//! Higher levels are intended for video.
	//! This parameter controls numerous internal encoding speed vs. compression efficiency/performance tradeoffs.
	//! Note this is NOT the same as the ETC1S quality level, and most users shouldn't change this.
	//! There is no default. 
	//! Callers must explicitly set this value.
	//! Callers can use KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL as a default value. Currently this is 2.
	uint32_t etc1sCompressionLevel;

	//! Compression quality. 
	//! Range is [1,255].
	//! Lower gives better compression/lower quality/faster.
	//! Higher gives less compression /higher quality/slower.
	//! This automatically determines values for maxEndpoints, maxSelectors, endpointRDOThreshold and selectorRDOThreshold for the target quality level.
	//! Setting these parameters overrides the values determined by qualityLevel which defaults to 128 if neither it nor both of maxEndpoints and maxSelectors have been set. 
	uint32_t qualityLevel;

	//! Tunes codec parameters for better quality on normal maps (no selector RDO, no endpoint RDO) and sets the texture's DFD appropriately.
	//! Only valid for linear textures. 
	bool normalMap;

	//! Number of threads used for compression. Default is 1. 
	uint32_t threadCount;

	//! A swizzle to apply before encoding.
	//! It must match the regular expression /^[rgba01]{4}$/.
	//! If both this and preSwizzle are specified ktxTexture_CompressBasisEx will raise KTX_INVALID_OPERATION.
	//! Usable with both ETC1S and UASTC. 
	char inputSwizzle[4];

	//! True to use UASTC base, false to use ETC1S base. 
	bool uastc;

	//! Enable Rate Distortion Optimization (RDO) post-processing.
	bool uastcRDO;

	//! UASTC RDO quality scalar (lambda).
	//! Lower values yield higher quality/larger LZ compressed files, higher values yield lower quality/smaller LZ compressed files. 
	//! A good range to try is [.2,4].
	//! Full range is [.001,50.0].
	//! Default is 1.0. 
	float uastcRDOQualityScalar;

	//! If true, prints Basis Universal encoder operation details to stdout.
	//! Not recommended for GUI apps. 
	bool verbose;

	ris_ktxBasisParams()
	{
		etc1sCompressionLevel = 0;
		qualityLevel = 0;
		normalMap = false;
		uastc = false;
		uastcRDO = false;
		uastcRDOQualityScalar = 0;
		verbose = false;
		threadCount = 0;

		for (int i = 0; i < 4; ++i) {
			inputSwizzle[i] = '\0';
		}
	}
};
