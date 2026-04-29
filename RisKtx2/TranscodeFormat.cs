namespace RisKtx2;

/// <summary>
/// Transcode target formats for basis-compressed KTX2 textures.
/// </summary>
public enum TranscodeFormat
{
    /// <summary>BC1 – RGB, no alpha (DXT1).</summary>
    Bc1Rgb   = 0,
    /// <summary>BC3 – RGBA (DXT5).</summary>
    Bc3Rgba  = 1,
    /// <summary>BC4 – single channel (R).</summary>
    Bc4R     = 2,
    /// <summary>BC5 – two channels (RG).</summary>
    Bc5Rg    = 3,
    /// <summary>BC7 – RGBA, high quality.</summary>
    Bc7Rgba  = 6,
    /// <summary>ASTC 4×4 block compressed.</summary>
    Astc4x4  = 10,
    /// <summary>Uncompressed 32-bit RGBA.</summary>
    Rgba32   = 13,
    /// <summary>Uncompressed 16-bit RGB565.</summary>
    Rgb565   = 14,
    /// <summary>Uncompressed 16-bit BGR565.</summary>
    Bgr565   = 15,
    /// <summary>Uncompressed 16-bit RGBA4444.</summary>
    Rgba4444 = 16
}
