namespace RisKtx2;

/// <summary>
/// The block dimension for the ASTC compression algorithm.
/// </summary>
public enum KtxPackAstcBlockDimension : uint 
{
    // 2D formats
   
    /// <summary>
    /// The 4x4 block dimension of 8 bits per pixel. 
    /// </summary>
    KTX_PACK_ASTC_BLOCK_DIMENSION_4X4,                    //: 8.00 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_5X4,                    //: 6.40 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_5X5,                    //: 5.12 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_6X5,                    //: 4.27 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_6X6,                    //: 3.56 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_8X5,                    //: 3.20 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_8X6,                    //: 2.67 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_10X5,                   //: 2.56 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_10X6,                   //: 2.13 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_8X8,                    //: 2.00 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_10X8,                   //: 1.60 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_10X10,                  //: 1.28 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_12X10,                  //: 1.07 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_12X12,                  //: 0.89 bpp
    // 3D formats
    KTX_PACK_ASTC_BLOCK_DIMENSION_3X3X3,                  //: 4.74 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_4X3X3,                  //: 3.56 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_4X4X3,                  //: 2.67 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_4X4X4,                  //: 2.00 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_5X4X4,                  //: 1.60 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_5X5X4,                  //: 1.28 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_5X5X5,                  //: 1.02 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_6X5X5,                  //: 0.85 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_6X6X5,                  //: 0.71 bpp
    KTX_PACK_ASTC_BLOCK_DIMENSION_6X6X6,                  //: 0.59 bpp
}