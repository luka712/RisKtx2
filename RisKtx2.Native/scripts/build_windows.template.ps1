$env:VCPKG_ROOT = "C:\path\to\vcpkg"
$env:PATH = "$env:VCPKG_ROOT;$env:PATH"
vcpkg install

# Add your own path to vcpkg.cmake
# Should be {{YOUR_PATH_TO_VCPKG}}/scripts/buildsystems/vcpkg.cmake
cmake -B cmake-build-debugvisualstudio -S . -DCMAKE_TOOLCHAIN_FILE=path/to/vcpkg/scripts/buildsystems/vcpkg.cmake -DCMAKE_BUILD_TYPE=Debug
# cmake -B cmake-build-releasevisualstudio -S . -DCMAKE_TOOLCHAIN_FILE=path/to/vcpkg/scripts/buildsystems/vcpkg.cmake -DCMAKE_BUILD_TYPE=Release