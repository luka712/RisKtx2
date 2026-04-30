#!/bin/bash

export VCPKG_ROOT=/Users/lerkapic/Desktop/Development/vcpkg
export PATH="$VCPKG_ROOT:$PATH"

# Install dependencies listed in vcpkg.json
vcpkg install

cmake \
  -G Xcode \
  -B cmake-build-debug-macos-xcode \
  -S . \
  -DCMAKE_BUILD_TYPE=Debug \
  -DCMAKE_EXPORT_COMPILE_COMMANDS=ON \
  -DCMAKE_TOOLCHAIN_FILE=$VCPKG_ROOT/scripts/buildsystems/vcpkg.cmake \
  -DVCPKG_TARGET_TRIPLET=arm64-osx \
  -DVCPKG_MANIFEST_MODE=ON
