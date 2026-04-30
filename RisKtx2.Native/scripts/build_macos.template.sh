#! /bin/bash

export VCPKG_ROOT=/path/tp/vcpkg
export PATH=$VCPKG_ROOT:$PATH

vcpkg install
cmake \
  -G Xcode \
  -B cmake-build-debug-macos-xcode \
  -S . \
  -DCMAKE_BUILD_TYPE=Debug \
  -DCMAKE_EXPORT_COMPILE_COMMANDS=ON \
  -DCMAKE_TOOLCHAIN_FILE=/path/to/vcpkg/scripts/buildsystems/vcpkg.cmake \
  -DVCPKG_TARGET_TRIPLET=arm64-osx
