# RisKtx2.Native

A thin, P/Invoke-friendly C wrapper around [Khronos KTX-Software](https://github.com/KhronosGroup/KTX-Software) (libktx). It exposes a stable C ABI for creating, reading, writing, compressing (Basis Universal), and transcoding KTX2 texture files.

This library is designed to be consumed from C# via P/Invoke, but can be used from any language that supports C interop.

## What this project does

- **Load KTX2 files** from disk into memory (`ris_ktxTexture2_CreateFromNamedFile`)
- **Create KTX2 textures** from raw image data (`ris_ktxTexture2_Create`)
- **Write KTX2 files** back to disk (`ris_ktxTexture2_WriteToNamedFile`)
- **Compress textures** with Basis Universal (ETC1S or UASTC) (`ris_ktxTexture2_CompressBasisEx`)
- **Transcode** Basis Universal compressed textures to GPU-ready formats like BC7, ETC2, ASTC, etc. (`ris_ktxTexture2_TranscodeBasis`)
- **Query texture properties** (width, height, format, mip levels, data size, offsets, etc.)
- **Load PNG images** via STB Image (`ris_stbi_load`) for use as source data

## Why this wrapper exists

The Khronos KTX library is a C++ library with a C API (`ktx.h`), but:
- Some structs and enums are not ideal for direct P/Invoke (e.g., C++ `bool` size mismatches)
- We wanted a curated, stable subset of functionality with consistent naming
- We wanted built-in logging via spdlog for debugging texture pipeline issues

This wrapper solves those issues by:
- Using plain C structs with `extern "C"` linkage
- Replacing C++ `bool` with `uint32_t` (0/1) for blittable P/Invoke compatibility
- Exposing only the functions needed for texture pipeline operations
- Adding null-check guards and debug logging

## Prerequisites

- [CMake](https://cmake.org/) 3.30+
- [vcpkg](https://vcpkg.io/) for dependency management
- C++20 compiler
- Platform-specific toolchain (see per-platform sections below)

### Dependencies (managed via vcpkg)

| Package   | Purpose                                    |
|-----------|-------------------------------------------|
| `ktx`     | Khronos KTX-Software library               |
| `spdlog`  | Logging                                    |
| `vulkan`  | Vulkan headers (for `VkFormat`)            |
| `catch2`  | Testing framework (tests only)             |

Install them via `vcpkg install` or let the build scripts handle it.

## Build

All build scripts are in `scripts/`. Templates are provided for you to copy and customize with your local vcpkg path.

### Linux (x64)

1. Copy `scripts/build_linux.template.sh` to `scripts/build_linux.sh` and set your vcpkg path.
2. Build:
   ```bash
   ./scripts/build_linux.sh debug    # or release
   ```
3. Output:
   - Shared library: `cmake-build-debug/libris_ktx2.so`
   - Tests: `cmake-build-debug/ris_ktx2_tests`

### Linux (ARM64)

Requires ARM64 cross-compiler:
```bash
sudo apt install g++-aarch64-linux-gnu
```

1. Copy `scripts/build_linux_arm.template.sh` to `scripts/build_linux_arm.sh` and set your vcpkg path.
2. Build:
   ```bash
   ./scripts/build_linux_arm.sh debug    # or release
   ```
3. Output:
   - Shared library: `cmake-build-debug-arm64/libris_ktx2.so`

You will need vcpkg packages compiled for `arm64-linux` triplet. To install them:
```bash
vcpkg install --triplet arm64-linux
```

### Windows (x64)

Requires Visual Studio 2022 with C++ workload.

1. Copy `scripts/build_windows.template.ps1` to `scripts/build_windows.ps1` and set your vcpkg path.
2. Build:
   ```powershell
   .\scripts\build_windows.ps1 Debug    # or Release
   ```
3. Output:
   - Shared library: `cmake-build-debug-win64/Debug/ris_ktx2.dll`
   - Tests: `cmake-build-debug-win64/Debug/ris_ktx2_tests.exe`

### Windows (ARM64)

1. Copy `scripts/build_windows_arm.template.ps1` to `scripts/build_windows_arm.ps1` and set your vcpkg path.
2. Build:
   ```powershell
   .\scripts\build_windows_arm.ps1 Release    # or Debug
   ```
3. Output:
   - Shared library: `cmake-build-release-arm64/Release/ris_ktx2.dll`

You will need vcpkg packages compiled for `arm64-windows` triplet:
```powershell
vcpkg install --triplet arm64-windows
```

### macOS (Apple Silicon)

1. Copy `scripts/build_macos.template.sh` to `scripts/build_macos.sh` and set your vcpkg path.
2. Build:
   ```bash
   ./scripts/build_macos.sh
   ```
3. Output:
   - Shared library: `cmake-build-debug-macos-xcode/Debug/libris_ktx2.dylib`

## Running Tests

After building, run the test executable:

```bash
# Linux / macOS
./cmake-build-debug/ris_ktx2_tests

# Windows
.\cmake-build-debug-win64\Debug\ris_ktx2_tests.exe
```

Tests cover:
- Loading KTX2 files
- Transcoding to BC7 and ETC2
- Writing textures with Basis Universal compression
- Creating textures with manual mip levels

## C# P/Invoke Usage

The API is designed to be blittable from C#. Here is a quick example:

```csharp
using System;
using System.Runtime.InteropServices;

public static class RisKtx2
{
    private const string DllName = "ris_ktx2";

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ris_ktxTexture2_CreateFromNamedFile(
        string filename,
        uint flags,
        out IntPtr outTexture);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ris_ktxTexture2_Destroy(IntPtr tex);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ris_ktxTexture2_GetWidth(IntPtr tex);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ris_ktxTexture2_GetHeight(IntPtr tex);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ris_ktxTexture2_NeedsTranscoding(IntPtr tex);
}
```

### Structs for P/Invoke

Define the structs with `[StructLayout(LayoutKind.Sequential)]`:

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct ris_ktxTextureCreateInfo
{
    public uint vkFormat;
    public uint baseWidth;
    public uint baseHeight;
    public uint baseDepth;
    public uint numDimensions;
    public uint numLevels;
    public uint numLayers;
    public uint numFaces;
    public uint isArray;       // 0 or 1
    public uint generateMipmaps; // 0 or 1
}

[StructLayout(LayoutKind.Sequential)]
public struct ris_ktxBasisParams
{
    public uint etc1sCompressionLevel;
    public uint qualityLevel;
    public uint normalMap;      // 0 or 1
    public uint threadCount;
    public uint uastc;          // 0 or 1
    public uint uastcRDO;       // 0 or 1
    public float uastcRDOQualityScalar;
    public uint verbose;        // 0 or 1
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public char[] inputSwizzle;
}
```

> **Note on `size_t`**: On 64-bit platforms `size_t` is 8 bytes. Use `UIntPtr` or `nuint` in C# for parameters like `ris_ktxTexture2_GetDataSize` and `ris_ktxTexture2_GetImageSize`.

> **Note on `bool` fields**: All boolean fields in structs are `uint32_t` (0 = false, 1 = true) to avoid C++ `bool` / C# `bool` size mismatches during P/Invoke.

## Logging

Debug logging is **disabled by default** to avoid file I/O overhead and log file proliferation in production.

You can enable it in two ways:

### 1. Environment variable
Set `RIS_KTX2_ENABLE_LOGGING=1` before running your application. The library will auto-initialize logging on the first API call that needs it.

```bash
export RIS_KTX2_ENABLE_LOGGING=1
./your_app
```

### 2. Explicit API call
Call `ris_ktxTexture2_EnableLogging` before any other API call. You can optionally specify a custom log file path; pass `nullptr` for the default (`ris_ktx2.log`).

```csharp
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void ris_ktxTexture2_EnableLogging(string logFilePath);

// Enable logging with default file
RisKtx2.ris_ktxTexture2_EnableLogging(null);
```

Check whether logging is active:

```csharp
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern uint ris_ktxTexture2_IsLoggingEnabled();
```

## Project Structure

```
.
├── include/                          # Public C headers
│   ├── ris_ktx.hpp                  # Main API declarations
│   ├── ris_ktx_logging.hpp          # Logging API
│   ├── ris_ktxTextureCreateInfo.hpp # Blittable create-info struct
│   ├── ris_ktxBasisParams.hpp       # Blittable Basis params struct
│   ├── ris_stb_image.h              # STB Image wrapper declarations
│   └── macros.hpp                   # API_EXPORT macro (dllexport / visibility)
├── src/                              # Implementation
│   ├── ris_ktx.cpp                  # KTX wrapper implementation
│   ├── ris_ktx_logging.cpp          # Logging implementation
│   └── ris_stb_image.cpp            # STB Image wrapper implementation
├── tests/                            # Catch2 tests
│   ├── ris_ktx_tests.cpp
│   ├── ris_ktx_universal_basis_tests.cpp
│   ├── ris_ktx_edge_case_tests.cpp
│   └── test_utilities.cpp           # PNG loading helpers for tests
├── scripts/                          # Build scripts per platform
├── cmake/                            # CMake modules
├── vendor/                           # Vendored headers (stb_image)
├── CMakeLists.txt
└── vcpkg.json                        # vcpkg manifest
```

## License

This wrapper is internal/proprietary.

This project uses several open-source libraries. See [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md) for the full license text of all dependencies, including:

| Library                     | License                  | Usage                    |
|-----------------------------|--------------------------|--------------------------|
| Khronos KTX-Software        | Apache 2.0               | Runtime (linked)         |
| spdlog                      | MIT                      | Runtime (linked)         |
| {fmt}                       | MIT                      | Runtime (linked via spdlog) |
| Vulkan Headers              | Apache-2.0 OR MIT        | Runtime (linked)         |
| Vulkan Loader               | Apache 2.0               | Runtime (linked)         |
| Zstandard (zstd)            | BSD or GPLv2             | Runtime (linked via ktx) |
| STB Image / STB Resize      | Public Domain / MIT      | Runtime (vendored headers) |
| Catch2                      | Boost Software License 1.0 | Tests only               |
