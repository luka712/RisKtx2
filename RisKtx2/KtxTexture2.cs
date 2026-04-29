using System.Runtime.InteropServices;
using RisKtx2.Interop;

namespace RisKtx2;

/// <summary>
/// Managed wrapper around a KTX2 texture loaded via RisKtx2Native.
/// Implements <see cref="IDisposable"/>; always dispose when finished to free native memory.
/// </summary>
public sealed class KtxTexture2 : IDisposable
{
    private IntPtr _handle;
    private bool   _disposed;

    // ------------------------------------------------------------------
    // Construction
    // ------------------------------------------------------------------

    private KtxTexture2(IntPtr handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Load a KTX2 texture from a file on disk.
    /// </summary>
    /// <param name="filePath">Path to the <c>.ktx2</c> file.</param>
    /// <returns>A new <see cref="KtxTexture2"/> instance.</returns>
    /// <exception cref="KtxException">Thrown if the native call fails.</exception>
    public static KtxTexture2 FromFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var result = Native.risKtx2TextureCreateFromFile(filePath, out IntPtr handle);
        ThrowIfFailed(result, nameof(Native.risKtx2TextureCreateFromFile));
        return new KtxTexture2(handle);
    }

    /// <summary>
    /// Load a KTX2 texture from a byte array in memory.
    /// </summary>
    /// <param name="data">The KTX2 file data.</param>
    /// <returns>A new <see cref="KtxTexture2"/> instance.</returns>
    /// <exception cref="KtxException">Thrown if the native call fails.</exception>
    public static KtxTexture2 FromMemory(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0)
            throw new ArgumentException("Data must not be empty.", nameof(data));

        unsafe
        {
            fixed (byte* pData = data)
            {
                var result = Native.risKtx2TextureCreateFromMemory(
                    pData, (nuint)data.Length, out IntPtr handle);
                ThrowIfFailed(result, nameof(Native.risKtx2TextureCreateFromMemory));
                return new KtxTexture2(handle);
            }
        }
    }

    /// <summary>
    /// Load a KTX2 texture from a <see cref="ReadOnlySpan{T}"/> of bytes.
    /// </summary>
    /// <param name="data">The KTX2 file data.</param>
    /// <returns>A new <see cref="KtxTexture2"/> instance.</returns>
    /// <exception cref="KtxException">Thrown if the native call fails.</exception>
    public static KtxTexture2 FromMemory(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            throw new ArgumentException("Data must not be empty.", nameof(data));

        unsafe
        {
            fixed (byte* pData = data)
            {
                var result = Native.risKtx2TextureCreateFromMemory(
                    pData, (nuint)data.Length, out IntPtr handle);
                ThrowIfFailed(result, nameof(Native.risKtx2TextureCreateFromMemory));
                return new KtxTexture2(handle);
            }
        }
    }

    // ------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------

    /// <summary>Base (mip 0) width in texels.</summary>
    public uint BaseWidth  => Native.risKtx2TextureGetBaseWidth(_handle);

    /// <summary>Base (mip 0) height in texels.</summary>
    public uint BaseHeight => Native.risKtx2TextureGetBaseHeight(_handle);

    /// <summary>Base (mip 0) depth in texels (1 for 2-D textures).</summary>
    public uint BaseDepth  => Native.risKtx2TextureGetBaseDepth(_handle);

    /// <summary>Number of mip levels.</summary>
    public uint NumLevels  => Native.risKtx2TextureGetNumLevels(_handle);

    /// <summary>Number of array layers.</summary>
    public uint NumLayers  => Native.risKtx2TextureGetNumLayers(_handle);

    /// <summary>Number of faces (6 for cube maps, 1 otherwise).</summary>
    public uint NumFaces   => Native.risKtx2TextureGetNumFaces(_handle);

    /// <summary>
    /// <see langword="true"/> if the texture uses a supercompressed (basis/BasisU) encoding
    /// and must be transcoded before use.
    /// </summary>
    public bool NeedsTranscoding => Native.risKtx2TextureNeedsTranscoding(_handle) != 0;

    /// <summary>
    /// Vulkan format identifier (<c>VkFormat</c>), or <c>0</c> (VK_FORMAT_UNDEFINED) if not set.
    /// </summary>
    public uint VkFormat => Native.risKtx2TextureGetVkFormat(_handle);

    // ------------------------------------------------------------------
    // Operations
    // ------------------------------------------------------------------

    /// <summary>
    /// Transcode a basis-compressed texture to a GPU-ready format.
    /// Must be called before accessing image data on a texture where
    /// <see cref="NeedsTranscoding"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="format">Target transcode format.</param>
    /// <exception cref="KtxException">Thrown if transcoding fails.</exception>
    public void TranscodeBasis(TranscodeFormat format)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var result = Native.risKtx2TextureTranscodeBasis(
            _handle, (Native.RisKtx2TranscodeFormat)format);
        ThrowIfFailed(result, nameof(Native.risKtx2TextureTranscodeBasis));
    }

    /// <summary>
    /// Get the raw image data for a specific mip level, array layer and face/depth slice
    /// as a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="level">Mip level (0 = base level).</param>
    /// <param name="layer">Array layer index.</param>
    /// <param name="faceSlice">Face index (for cube maps) or depth slice index.</param>
    /// <returns>A span over the native image data for the requested level/layer/face.</returns>
    /// <exception cref="KtxException">Thrown if the data cannot be retrieved.</exception>
    public unsafe ReadOnlySpan<byte> GetImageData(uint level = 0, uint layer = 0, uint faceSlice = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var result = Native.risKtx2TextureGetImageData(
            _handle, level, layer, faceSlice, out byte* pData, out nuint size);
        ThrowIfFailed(result, nameof(Native.risKtx2TextureGetImageData));

        return new ReadOnlySpan<byte>(pData, (int)size);
    }

    // ------------------------------------------------------------------
    // IDisposable
    // ------------------------------------------------------------------

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_handle != IntPtr.Zero)
        {
            Native.risKtx2TextureDestroy(_handle);
            _handle = IntPtr.Zero;
        }
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static void ThrowIfFailed(Native.RisKtx2Result result, string operation)
    {
        if (result == Native.RisKtx2Result.Success)
            return;

        string message = result switch
        {
            Native.RisKtx2Result.ErrorInvalidValue           => "Invalid value.",
            Native.RisKtx2Result.ErrorInvalidOperation       => "Invalid operation.",
            Native.RisKtx2Result.ErrorOutOfMemory            => "Out of memory.",
            Native.RisKtx2Result.ErrorFileNotFound           => "File not found or could not be opened.",
            Native.RisKtx2Result.ErrorFileDataError          => "File data error.",
            Native.RisKtx2Result.ErrorUnsupportedTextureType => "Unsupported texture type.",
            Native.RisKtx2Result.ErrorUnsupportedFeature     => "Unsupported feature.",
            Native.RisKtx2Result.ErrorLibraryNotLinked       => "Required library is not linked.",
            _                                                => "Unknown error."
        };

        throw new KtxException($"{operation} failed: {message}", (int)result);
    }
}
