#!/bin/bash
set -euo pipefail

export VCPKG_ROOT=#ADD VCKPG PATH
export PATH="$VCPKG_ROOT:$PATH"

BUILD_TYPE="${1:-Debug}"
BUILD_DIR="cmake-build-${BUILD_TYPE,,}"  # debug/release lowercase

# Build
cmake \
  -G Ninja \
  -B "$BUILD_DIR" \
  -S . \
  -DCMAKE_BUILD_TYPE="$BUILD_TYPE" \
  -DCMAKE_EXPORT_COMPILE_COMMANDS=ON \
  -DCMAKE_TOOLCHAIN_FILE="$VCPKG_ROOT/scripts/buildsystems/vcpkg.cmake"

cmake --build "$BUILD_DIR"
