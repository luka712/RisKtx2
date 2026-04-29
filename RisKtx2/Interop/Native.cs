using System.Runtime.InteropServices;

namespace RisKtx2.Interop;

/// <summary>
/// P/Invoke declarations for the RisKtx2Native shared library.
/// All identifiers mirror the C header <c>RisKtx2Native.h</c>.
/// </summary>
internal static class Native
{
    /// <summary>Name of the native shared library (without extension or "lib" prefix).</summary>
    private const string LibName = "RisKtx2Native";

    // ------------------------------------------------------------------
    // Opaque handle – mapped as an IntPtr in managed code
    // ------------------------------------------------------------------

    /// <summary>Error codes returned by RisKtx2Native functions.</summary>
    internal enum RisKtx2Result : int
    {
        Success                     = 0,
        ErrorInvalidValue           = 1,
        ErrorInvalidOperation       = 2,
        ErrorOutOfMemory            = 3,
        ErrorFileNotFound           = 4,
        ErrorFileDataError          = 5,
        ErrorUnsupportedTextureType = 6,
        ErrorUnsupportedFeature     = 7,
        ErrorLibraryNotLinked       = 8,
        ErrorUnknown                = 9
    }

    /// <summary>Transcode target formats for basis-compressed KTX2 textures.</summary>
    internal enum RisKtx2TranscodeFormat : int
    {
        Bc1Rgb  = 0,
        Bc3Rgba = 1,
        Bc4R    = 2,
        Bc5Rg   = 3,
        Bc7Rgba = 6,
        Astc4x4 = 10,
        Rgba32  = 13,
        Rgb565  = 14,
        Bgr565  = 15,
        Rgba4444 = 16
    }

    // ------------------------------------------------------------------
    // Texture lifecycle
    // ------------------------------------------------------------------

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               CharSet = CharSet.Ansi, ExactSpelling = true)]
    internal static extern RisKtx2Result risKtx2TextureCreateFromFile(
        string          filePath,
        out IntPtr      pTexture);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern unsafe RisKtx2Result risKtx2TextureCreateFromMemory(
        byte*       pData,
        nuint       dataSize,
        out IntPtr  pTexture);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern void risKtx2TextureDestroy(IntPtr texture);

    // ------------------------------------------------------------------
    // Transcoding
    // ------------------------------------------------------------------

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern RisKtx2Result risKtx2TextureTranscodeBasis(
        IntPtr                 texture,
        RisKtx2TranscodeFormat format);

    // ------------------------------------------------------------------
    // Image data access
    // ------------------------------------------------------------------

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern unsafe RisKtx2Result risKtx2TextureGetImageData(
        IntPtr         texture,
        uint           level,
        uint           layer,
        uint           faceSlice,
        out byte*      pData,
        out nuint      pSize);

    // ------------------------------------------------------------------
    // Property accessors
    // ------------------------------------------------------------------

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern uint risKtx2TextureGetBaseWidth(IntPtr texture);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern uint risKtx2TextureGetBaseHeight(IntPtr texture);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern uint risKtx2TextureGetBaseDepth(IntPtr texture);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern uint risKtx2TextureGetNumLevels(IntPtr texture);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern uint risKtx2TextureGetNumLayers(IntPtr texture);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern uint risKtx2TextureGetNumFaces(IntPtr texture);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern int risKtx2TextureNeedsTranscoding(IntPtr texture);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl,
               ExactSpelling = true)]
    internal static extern uint risKtx2TextureGetVkFormat(IntPtr texture);
}
