# RisKtx2

**C# bindings for the [KTX-Software](https://github.com/KhronosGroup/KTX-Software) library.**

RisKtx2 provides idiomatic, managed .NET access to the Khronos KTX2 texture
container format — including loading, writing, Basis Universal compression and
GPU-format transcoding (BC7, ETC2, ASTC, …).

Although these bindings were created primarily to power the **Ris Game Engine**
and the surrounding Ris tooling, the public C# API is intentionally generic and
has no engine-specific dependencies, so it can be dropped into **any .NET
project** that needs to work with `.ktx2` textures.

---

## Repository layout

The repository is split into two cooperating projects:

| Folder | Language | Audience | Purpose |
| --- | --- | --- | --- |
| [`RisKtx2.Native/`](./RisKtx2.Native) | C++20 | **Internal** | Thin C ABI shared library that wraps [KTX-Software](https://github.com/KhronosGroup/KTX-Software) (and a small slice of [`stb_image`](https://github.com/nothings/stb)) into a flat, P/Invoke-friendly surface. Built with CMake + vcpkg. |
| [`RisKtx2.Managed/`](./RisKtx2.Managed) | C# (.NET 10) | **External** — this is what you consume | High-level, object-oriented C# API (`Ktx2Texture`, `KtxBasisParams`, `StbImageLoader`, …) that calls into the native library via P/Invoke. |

> The C++ project is an implementation detail. End users of the library only
> need to reference the C# project (or its produced NuGet package / DLLs).

---

## Features

- Load `.ktx2` textures from disk (`Ktx2Texture(path)`).
- Create textures from scratch and populate them from raw memory
  (`Ktx2Texture(KtxTextureCreateInfo, …)` + `SetImageFromMemory`).
- Write textures back out to `.ktx2` files (`WriteToNamedFile`).
- **Basis Universal** compression (ETC1S and UASTC) via `CompressBasis`.
- **Transcoding** of Basis-compressed textures to GPU-native formats
  (BC7, ETC2, ASTC, RGBA8, …) via `TranscodeBasis`.
- Query helpers: width, height, mip levels, row pitch, element size,
  Vulkan format, supercompression scheme, image size/offset, raw data pointer.
- Bonus: a tiny `StbImageLoader` for loading PNG/JPG/etc. as raw RGBA bytes —
  handy for feeding pixel data into a freshly created KTX2 texture.
- Deterministic cleanup through `IDisposable` (native handles are released
  automatically on `Dispose`).

---

## Requirements

### Consuming the C# bindings (most users)

- .NET 10 SDK (see [`RisKtx2.csproj`](./RisKtx2.Managed/RisKtx2/RisKtx2/RisKtx2.csproj))
- Windows x64 (the repo currently ships `runtimes/win-x64/native/*.dll`).
  Linux/macOS builds can be produced from the native project — see below.

### Building the native library (contributors only)

- A C++20 compiler (MSVC, Clang or GCC)
- [CMake](https://cmake.org/) ≥ 3.30
- [vcpkg](https://vcpkg.io/) — used to pull in `ktx`, `vulkan`, `spdlog`,
  and `catch2` (see [`vcpkg.json`](./RisKtx2.Native/vcpkg.json))
- A Vulkan SDK installation (headers are required by the KTX library)

---

## Getting started

### 1. Reference the managed project

Add a project reference to `RisKtx2.Managed/RisKtx2/RisKtx2/RisKtx2.csproj`,
or copy its build output (the managed assembly **plus** the
`runtimes/win-x64/native/*.dll` files) into your application.

The native DLLs (`ris_ktx2.dll`, `ktx.dll`, `zstd.dll`, `spdlog*.dll`,
`fmt*.dll`) are automatically copied next to your executable thanks to the
`<None Update="runtimes\win-x64\native\..." />` entries in the csproj.

### 2. Load an existing `.ktx2` texture

```csharp
using RisKtx2;

using var texture = new Ktx2Texture("assets/wall_basis_uastc.ktx2");

Console.WriteLine($"{texture.Width} x {texture.Height}");
Console.WriteLine($"Supercompression: {texture.SupercompressionScheme}");

if (texture.NeedsTranscoding)
{
    // Transcode Basis Universal data to a GPU-native format.
    texture.TranscodeBasis(KtxTranscodeFormat.BC7_RGBA);
}

ulong offset = texture.GetImageOffset(level: 0, layer: 0, faceSlice: 0);
IntPtr  data   = texture.GetTextureData(offset);
ulong   size   = texture.GetImageSize(0);

// `data` / `size` can now be uploaded straight to your GPU API
// (Vulkan, D3D12, WebGPU, …).
```

### 3. Create a KTX2 texture from a PNG and write it to disk

```csharp
using RisKtx2;

var loader = new StbImageLoader();
byte[] pixels = loader.LoadImage("assets/test.png",
                                 out int width, out int height, out int channels);

var createInfo = new KtxTextureCreateInfo
{
    BaseWidth  = (uint)width,
    BaseHeight = (uint)height,
    VkFormat   = VkFormat.R8G8B8A8_UNORM,
};

using var texture = new Ktx2Texture(createInfo, KtxTextureCreateStorage.ALLOC_STORAGE);
texture.SetImageFromMemory(level: 0, layer: 0, faceSlice: 0,
                           pixels, (uint)pixels.Length);

// Optional: Basis Universal compression
texture.CompressBasis(new KtxBasisParams
{
    UseUastc        = true,
    CompressionLevel = 2,
    QualityLevel     = 128,
});

texture.WriteToNamedFile("assets/test.ktx2");
```

For more end-to-end examples, see the
[`RisKtx2.Tests`](./RisKtx2.Managed/RisKtx2/RisKtx2.Tests) project.

---

## Building the native library

Pre-built Windows x64 DLLs are checked in under
`RisKtx2.Managed/RisKtx2/RisKtx2/runtimes/win-x64/native/`, so most users do
**not** need to build the C++ side themselves. If you want to rebuild it (or
build for Linux / macOS), helper scripts are provided:

```bash
# Windows (PowerShell)
RisKtx2.Native/scripts/build_windows.ps1

# Linux
RisKtx2.Native/scripts/build_linux.sh

# macOS
RisKtx2.Native/scripts/build_macos.sh
RisKtx2.Native/scripts/build_macos_release.sh
```

Each script bootstraps vcpkg dependencies and runs CMake. The resulting
`ris_ktx2` shared library should be placed alongside the managed assembly
(typically under `runtimes/<rid>/native/`).

A separate `ris_ktx2_tests` executable is also produced, which runs the
Catch2-based C++ tests in [`RisKtx2.Native/tests`](./RisKtx2.Native/tests).

---

## Testing

- **Native (C++) tests** — Catch2, located in
  [`RisKtx2.Native/tests/`](./RisKtx2.Native/tests). Built automatically by
  the CMake target `ris_ktx2_tests`.
- **Managed (C#) tests** — NUnit, located in
  [`RisKtx2.Managed/RisKtx2/RisKtx2.Tests/`](./RisKtx2.Managed/RisKtx2/RisKtx2.Tests).
  Run them with:

  ```bash
  dotnet test RisKtx2.Managed/RisKtx2/RisKtx2.slnx
  ```

---

## Used by / intended for

- The **Ris Game Engine** — used as the primary texture container loader.
- The **Ris tooling** suite (sprite/asset pipelines, content cookers, …).
- Any third-party .NET project that needs KTX2 + Basis Universal support —
  the public API has no engine dependencies and is safe to use standalone.

---

## License

See [LICENSE](./LICENSE) for license information.

This project builds on top of the excellent
[KTX-Software](https://github.com/KhronosGroup/KTX-Software) library by the
Khronos Group and [`stb_image`](https://github.com/nothings/stb) by Sean Barrett,
each of which are distributed under their own licenses.
