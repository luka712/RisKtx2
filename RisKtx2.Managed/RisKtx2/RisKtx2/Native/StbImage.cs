using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RisKtx2.Native
{
    /// <summary>
    /// Provides P/Invoke signatures for interacting with the native Stb Image handling functions 
    /// in the Ris Texture Toolkit native library.
    /// </summary>
    internal static class StbImage
    {
        private const string DLL_NAME = "ris_ktx2";

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ris_stbi_load(
            string filename,
            out int width,
            out int height,
            out int channels_in_file,
            int desired_channels);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ris_stbi_image_free(IntPtr data);
    }
}
