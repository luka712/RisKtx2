$env:VCPKG_ROOT = "D:\_Windows\vcpkg"
$env:PATH = "$env:VCPKG_ROOT;$env:PATH"

$BuildType = if ($args[0]) { $args[0] } else { "Debug" }
$BuildDir = "cmake-build-$($BuildType.ToLower())-win64"

vcpkg install

cmake -B "$BuildDir" -S . `
  -G "Visual Studio 17 2022" `
  -A x64 `
  -DCMAKE_BUILD_TYPE="$BuildType" `
  -DCMAKE_TOOLCHAIN_FILE="$env:VCPKG_ROOT\scripts\buildsystems\vcpkg.cmake"

cmake --build "$BuildDir" --config "$BuildType"
