
find_package(ktx CONFIG REQUIRED)
find_package(spdlog CONFIG REQUIRED)
find_package(Vulkan REQUIRED)

SET(PACKAGES_LIBRARIES
        ${PACKAGES}
        ${SLANG_LIBRARY}
        KTX::ktx
        spdlog::spdlog
        Vulkan::Vulkan
)

SET(PACKAGES_INCLUDE
        ${PACKAGES_INCLUDE}
        ${SLANG_INCLUDE_DIR}
)

