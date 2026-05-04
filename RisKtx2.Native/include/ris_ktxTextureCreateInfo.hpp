#include <cstdint>
#include <ktx.h>

//! Structure for passing parameters to ktxTexture2_Create().
struct ris_ktxTextureCreateInfo
{
    //! VkFormat for texture.
    VkFormat vkFormat;

    //! Width of the base level of the texture.
    uint32_t baseWidth;

    //! Height of the base level of the texture.
    uint32_t baseHeight;

    //! Depth of the base level of the texture. 
	uint32_t baseDepth;

    //! Number of dimensions in the texture, 1, 2 or 3. 
	uint32_t numDimensions;
    
    //! Number of mip levels in the texture. Should be 1 if generateMipmaps is 'true'. 
    uint32_t numLevels;

    //! Number of array layers in the texture. 
	uint32_t numLayers;
    
    //! Number of faces: 6 for cube maps, 1 otherwise. 
	uint32_t numFaces;

    // Set to true if the texture is to be an array texture. Means OpenGL will use a GL_TEXTURE_*_ARRAY target. 
	bool isArray;

    //! Set to true if mipmaps should be generated for the texture when loading into a 3D API. 
    bool generateMipmaps;

	//! Default constructor initializes members to default values.
    ris_ktxTextureCreateInfo()
    {
        vkFormat = VK_FORMAT_UNDEFINED;
        baseWidth = 0;
        baseHeight = 0;
        baseDepth = 1;
        numDimensions = 2;
        numLevels = 1;
        numLayers = 1;
        numFaces = 1;
        isArray = false;
		generateMipmaps = false;
    }
};
