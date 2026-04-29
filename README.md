# RisKtx2

C# bindings for the [KTX2](https://github.khronos.org/KTX-File-Format-Specification/) GPU texture container format.

## Projects

| Project | Language | Purpose |
|---|---|---|
| **RisKtx2Native** | C++ / CMake | C-compatible shared library that wraps [KTX-Software](https://github.com/KhronosGroup/KTX-Software). Internal use only. |
| **RisKtx2** | C# / .NET 8 | Managed P/Invoke bindings published as a NuGet package. |

## RisKtx2Native (C++)

A CMake project that builds a shared library (`RisKtx2Native.dll` / `libRisKtx2Native.so` / `libRisKtx2Native.dylib`).

### Build

```bash
cd RisKtx2Native
cmake -B build -DCMAKE_BUILD_TYPE=Release
cmake --build build --config Release
```

The build fetches [KTX-Software v4.3.2](https://github.com/KhronosGroup/KTX-Software/releases/tag/v4.3.2) automatically via CMake `FetchContent`.

### C API overview

```c
// Load from file
RisKtx2Texture tex;
RisKtx2Result  res = risKtx2TextureCreateFromFile("texture.ktx2", &tex);

// Transcode if needed
if (risKtx2TextureNeedsTranscoding(tex))
    risKtx2TextureTranscodeBasis(tex, RISKTX2_TRANSCODE_FORMAT_BC7_RGBA);

// Get image data
const uint8_t* pData;
size_t         dataSize;
risKtx2TextureGetImageData(tex, 0, 0, 0, &pData, &dataSize);

// Destroy
risKtx2TextureDestroy(tex);
```

## RisKtx2 (C#)

A .NET 8 class library that wraps `RisKtx2Native` via P/Invoke and is published to NuGet.

### Usage

```csharp
using RisKtx2;

// Load from file
using var texture = KtxTexture2.FromFile("texture.ktx2");

// Transcode basis-compressed textures before uploading to the GPU
if (texture.NeedsTranscoding)
    texture.TranscodeBasis(TranscodeFormat.Bc7Rgba);

Console.WriteLine($"Size: {texture.BaseWidth}x{texture.BaseHeight}");
Console.WriteLine($"Mip levels: {texture.NumLevels}");

// Get raw pixel data for mip 0
ReadOnlySpan<byte> data = texture.GetImageData(level: 0, layer: 0, faceSlice: 0);
```

## License

MIT
