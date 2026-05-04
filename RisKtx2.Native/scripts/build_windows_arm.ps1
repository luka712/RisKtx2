$env:VCPKG_ROOT = "D:\_Windows\vcpkg"
$env:PATH = "$env:VCPKG_ROOT;$env:PATH"

vcpkg install --triplet arm64-windows

cmake -B cmake-release-debugvisualstudio -S . `
  -G "Visual Studio 17 2022" `
  -DCMAKE_TOOLCHAIN_FILE=D:/_Windows/vcpkg/scripts/buildsystems/vcpkg.cmake `
  -DCMAKE_BUILD_TYPE=Release `
  -A ARM64 `
  -DVCPKG_TARGET_TRIPLET=arm64-windows

cmake --build cmake-release-debugvisualstudio --config Release
