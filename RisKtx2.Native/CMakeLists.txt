cmake_minimum_required(VERSION 3.30)
project(ris_ktx2 LANGUAGES CXX)

set(TARGET_NAME ris_ktx2)
set(TESTS_NAME ${TARGET_NAME}_tests)

# C++ standard
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_OSX_ARCHITECTURES "arm64")

# Dependencies
include(cmake/VcpkgDependencies.cmake)
include(cmake/VcpkgTestDependencies.cmake)
include(cmake/CopyTestFiles.cmake)

# Include directories
include_directories(${CMAKE_SOURCE_DIR}/include)
include_directories(${CMAKE_SOURCE_DIR}/vendor)

# Source files
file(GLOB SRC_FILES src/*.cpp src/**/*.cpp src/**/**/*.cpp src/**/**/**/*.cpp)

# AI-GENERATED: Tests are now split — keep both globs
file(GLOB TEST_SRC_FILES tests/*.cpp tests/**/*.cpp tests/**/**/*.cpp tests/**/**/**/*.cpp)

# Define the target FIRST
add_library(${TARGET_NAME} SHARED
        library.cpp
        ${SRC_FILES}
        include/ris_ktxAstcParams.hpp
)

add_executable(${TESTS_NAME}
        ${SRC_FILES}
        ${TEST_SRC_FILES}
        tests/ris_ktx_astc_tests.cpp
)

# Include dirs
target_include_directories(${TARGET_NAME} PRIVATE
        ${CMAKE_SOURCE_DIR}/include
        ${PACKAGES_INCLUDE}
)
target_include_directories(${TESTS_NAME} PRIVATE
        ${CMAKE_SOURCE_DIR}/include
        ${PACKAGES_INCLUDE}
        ${TEST_PACKAGES_INCLUDE}
)

# SOURCES
file(GLOB COMMON_SOURCES src/*.cpp src/**/*.cpp src/**/**/*.cpp src/**/**/**/*.cpp)

# Link libraries
target_link_libraries(${TARGET_NAME} PRIVATE
        ${PACKAGES_LIBRARIES}
)
target_link_libraries(${TESTS_NAME} PRIVATE
        ${PACKAGES_LIBRARIES}
        ${TEST_PACKAGES_LIBRARIES}
)

