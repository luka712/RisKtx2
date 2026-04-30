//! Structure for passing extended parameters to ktxTexture2_CompressBasisEx().
	//! If you only want default values, use ktxTexture2_CompressBasis().Here, at a minimum you must initialize the structure as follows :
	//! <code>
	//! ris_ktxBasisParams params = { 0 };
	//! ris_ktxBasisParams.compressionLevel = KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL;
	//! </code>
	//! compressionLevel has to be explicitly set because 0 is a valid compressionLevel but is not the default used by the BasisU encoder when no value is set.Only the other settings that are to be non - default must be non - zero.
	struct ris_ktxBasisParams
	{
		//! Encoding speed vs. quality tradeoff. Range is [0,6].
		//! Higher values are much slower, but give slightly higher quality.
		//! Higher levels are intended for video.
		//! There is no default. Callers must explicitly set this value. 
		//! Callers can use KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL as a default value. 
		//! Currently this is 2. 
		uint32_t compressionLevel;

		//! Compression quality. 
		//! Range is [1,255].
		//! Lower gives better compression/lower quality/faster.
		//! Higher gives less compression /higher quality/slower.
		//! This automatically determines values for maxEndpoints, maxSelectors, endpointRDOThreshold and selectorRDOThreshold for the target quality level.
		//! Setting these parameters overrides the values determined by qualityLevel which defaults to 128 if neither it nor both of maxEndpoints and maxSelectors have been set. 
		uint32_t qualityLevel;

		//! True to use UASTC base, false to use ETC1S base. 
		bool uastc;
	};
