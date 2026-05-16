$env:VCPKG_ROOT = "D:\_Windows\vcpkg"
$env:PATH = "$env:VCPKG_ROOT;$env:PATH"

$BuildType = if ($args[0]) { $args[0] } else { "Release" }
$BuildDir = "cmake-build-$($BuildType.ToLower())-arm64"

vcpkg install --triplet arm64-windows

cmake -B "$BuildDir" -S . `
  -G "Visual Studio 17 2022" `
  -DCMAKE_BUILD_TYPE="$BuildType" `
  -A ARM64 `
  -DVCPKG_TARGET_TRIPLET=arm64-windows `
  -DCMAKE_TOOLCHAIN_FILE="$env:VCPKG_ROOT\scripts\buildsystems\vcpkg.cmake"

cmake --build "$BuildDir" --config "$BuildType"
