# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-04-30

### Added
- Initial release of **RisKtx2** — C# bindings for the
  [KTX-Software](https://github.com/KhronosGroup/KTX-Software) library.
- `Ktx2Texture` managed wrapper with support for:
  - Loading `.ktx2` files from disk.
  - Creating textures from `KtxTextureCreateInfo` and populating them via
    `SetImageFromMemory`.
  - Writing textures to disk via `WriteToNamedFile`.
  - Basis Universal compression (`CompressBasis` + `KtxBasisParams`).
  - Transcoding Basis-compressed textures (`TranscodeBasis`).
  - Query helpers (width, height, mip levels, row pitch, element size,
    Vulkan format, supercompression scheme, image size/offset, raw data
    pointer).
- `StbImageLoader` helper for loading PNG/JPG/etc. as raw RGBA bytes.
- Native `ris_ktx2` shared library (C++20, built via CMake + vcpkg) wrapping
  KTX-Software and `stb_image` behind a flat C ABI.
- Pre-built Windows x64 native binaries shipped under
  `RisKtx2.Managed/RisKtx2/RisKtx2/runtimes/win-x64/native/`.
