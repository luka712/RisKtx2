using System.Runtime.InteropServices;

namespace RisKtx2.Native
{
    /// <summary>
    /// Provides P/Invoke signatures for interacting with the native Stb Image handling functions 
    /// in the Ris Texture Toolkit native library.
    /// </summary>
    internal static class NativeStbImage
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
        internal static extern int ris_stbir_resize_uint8(
            IntPtr input_pixels, int input_w, int input_h, int input_stride_in_bytes,
            IntPtr output_pixels, int output_w, int output_h, int output_stride_in_bytes,
            int num_channels);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ris_stbi_image_free(IntPtr data);
    }
}
