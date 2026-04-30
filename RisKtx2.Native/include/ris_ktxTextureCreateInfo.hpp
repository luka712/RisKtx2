#include <cstdint>

//! Structure for passing parameters to ktxTexture2_Create().
struct ris_ktxTextureCreateInfo
{
    //! Width of the base level of the texture.
    uint32_t baseWidth;

    //! Height of the base level of the texture.
    uint32_t baseHeight;
    VkFormat vkFormat;
};
