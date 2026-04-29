cmake_minimum_required(VERSION 3.20)
project(RisKtx2Native VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# -----------------------------------------------------------------------
# Dependencies – fetch KTX-Software from Khronos
# -----------------------------------------------------------------------
include(FetchContent)

FetchContent_Declare(
    ktx
    GIT_REPOSITORY https://github.com/KhronosGroup/KTX-Software.git
    GIT_TAG        v4.3.2
    GIT_SHALLOW    TRUE
)

# Disable parts of KTX-Software we don't need to keep the build lean.
set(KTX_FEATURE_TESTS      OFF CACHE BOOL "" FORCE)
set(KTX_FEATURE_TOOLS      OFF CACHE BOOL "" FORCE)
set(KTX_FEATURE_DOC        OFF CACHE BOOL "" FORCE)
set(KTX_FEATURE_GL_UPLOAD  OFF CACHE BOOL "" FORCE)

FetchContent_MakeAvailable(ktx)

# -----------------------------------------------------------------------
# RisKtx2Native shared library
# -----------------------------------------------------------------------
add_library(RisKtx2Native SHARED
    src/RisKtx2Native.cpp
)

target_include_directories(RisKtx2Native
    PUBLIC
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
        $<INSTALL_INTERFACE:include>
)

target_compile_definitions(RisKtx2Native PRIVATE RISKTX2NATIVE_EXPORTS)

target_link_libraries(RisKtx2Native PRIVATE ktx)

set_target_properties(RisKtx2Native PROPERTIES
    VERSION   ${PROJECT_VERSION}
    SOVERSION ${PROJECT_VERSION_MAJOR}
)

# -----------------------------------------------------------------------
# Install rules
# -----------------------------------------------------------------------
include(GNUInstallDirs)

install(TARGETS RisKtx2Native
    EXPORT  RisKtx2NativeTargets
    RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
    LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
    ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
)

install(FILES include/RisKtx2Native.h
    DESTINATION ${CMAKE_INSTALL_INCLUDEDIR}
)
