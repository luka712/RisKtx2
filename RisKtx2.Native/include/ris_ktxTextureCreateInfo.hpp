#include <cstdint>
#include <ktx.h>

//! Structure for passing parameters to ktxTexture2_Create().
struct ris_ktxTextureCreateInfo
{
    //! Width of the base level of the texture.
    uint32_t baseWidth;

    //! Height of the base level of the texture.
    uint32_t baseHeight;
    
    //! VkFormat for texture.
    VkFormat vkFormat;
    
    //! Number of mip levels in the texture. Should be 1 if generateMipmaps is 'true'. 
    uint32_t numLevels = 1;
    
    //! Set to KTX_TRUE if mipmaps should be generated for the texture when loading into a 3D API. 
    bool generateMipmaps = KTX_FALSE;
};
