using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RisKtx2
{
    public class StbImageLoader
    {
        public StbImageLoader()
        {
            Native.NativeResolver.Setup();
        }
        public byte[] LoadImage(string filePath, out int width, out int height, out int channels)
        {
            IntPtr dataPtr = Native.StbImage.ris_stbi_load(filePath, out width, out height, out channels, 0);
            if (dataPtr == IntPtr.Zero)
            {
                throw new Exception($"Failed to load image: {filePath}");
            }
            try
            {
                int dataSize = width * height * channels;
                byte[] data = new byte[dataSize];
                Marshal.Copy(dataPtr, data, 0, dataSize);
                return data;
            }
            finally
            {
                Native.StbImage.ris_stbi_image_free(dataPtr);
            }
        }
    }
}
