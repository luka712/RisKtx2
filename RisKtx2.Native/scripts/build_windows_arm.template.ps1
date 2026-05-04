$env:VCPKG_ROOT = "C:\path\to\vcpkg"
$env:PATH = "$env:VCPKG_ROOT;$env:PATH"
vcpkg install --triplet arm64-windows

# Add your own path to vcpkg.cmake
# Should be {{YOUR_PATH_TO_VCPKG}}/scripts/buildsystems/vcpkg.cmake
cmake -B cmake-build-debugvisualstudio -S . `
  -DCMAKE_TOOLCHAIN_FILE=path/to/vcpkg/scripts/buildsystems/vcpkg.cmake `
  -DCMAKE_BUILD_TYPE=Debug `
  -A ARM64 `
  -DVCPKG_TARGET_TRIPLET=arm64-windows

# cmake -B cmake-build-releasevisualstudio -S . `
#   -DCMAKE_TOOLCHAIN_FILE=path/to/vcpkg/scripts/buildsystems/vcpkg.cmake `
#   -DCMAKE_BUILD_TYPE=Release `
#   -A ARM64 `
#   -DVCPKG_TARGET_TRIPLET=arm64-windows