#!/bin/bash
set -euo pipefail

export VCPKG_ROOT=#ADD VCPKG PATH
export PATH="$VCPKG_ROOT:$PATH"

BUILD_TYPE="${1:-Debug}"
BUILD_DIR="cmake-build-${BUILD_TYPE,,}-arm64"  # debug/release lowercase

# Build for ARM64 Linux
# Requires: sudo apt install g++-aarch64-linux-gnu
# And vcpkg arm64-linux triplet packages installed

cmake \
  -G Ninja \
  -B "$BUILD_DIR" \
  -S . \
  -DCMAKE_BUILD_TYPE="$BUILD_TYPE" \
  -DCMAKE_EXPORT_COMPILE_COMMANDS=ON \
  -DCMAKE_TOOLCHAIN_FILE="$VCPKG_ROOT/scripts/buildsystems/vcpkg.cmake" \
  -DVCPKG_TARGET_TRIPLET=arm64-linux \
  -DCMAKE_C_COMPILER=aarch64-linux-gnu-gcc \
  -DCMAKE_CXX_COMPILER=aarch64-linux-gnu-g++

cmake --build "$BUILD_DIR"
