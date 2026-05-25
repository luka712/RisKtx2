#!/bin/bash

export VCPKG_ROOT=/Users/lerkapic/Desktop/Development/vcpkg
export PATH="$VCPKG_ROOT:$PATH"

vcpkg install

cmake \
  -G Xcode \
  -B cmake-build-release-macos-xcode \
  -S . \
  -DCMAKE_EXPORT_COMPILE_COMMANDS=ON \
  -DCMAKE_TOOLCHAIN_FILE=$VCPKG_ROOT/scripts/buildsystems/vcpkg.cmake \
  -DVCPKG_TARGET_TRIPLET=arm64-osx \
  -DVCPKG_MANIFEST_MODE=ON

cmake --build cmake-build-release-macos-xcode --config Release
