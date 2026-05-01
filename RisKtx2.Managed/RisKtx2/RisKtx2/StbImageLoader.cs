using System.Runtime.InteropServices;

namespace RisKtx2
{
    /// <summary>
    /// The StbImageLoader class provides functionality to load images using the stb_image library and convert them into a suitable format.
    /// It handles loading image data from files, extracting pixel data, and performing necessary swizzling to match the desired format. 
    /// This class abstracts away the complexities of image loading and format conversion,
    /// allowing developers to easily integrate image assets into their applications.
    /// </summary>
    public class StbImageLoader
    {
        private readonly VkFormat[] _supportedFormats = [ VkFormat.R8G8B8A8_UNORM, VkFormat.B8G8R8A8_UNORM ];

        public StbImageLoader()
        {
            Native.NativeResolver.Setup();
        }

        /// <summary>
        /// Loads an image from the specified file path using stb_image and returns the raw pixel data as a byte array, 
        /// along with the image's width, height, and number of channels.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="channels">Number of channels.</param>
        /// <param name="desiredChannels">The desired number of channels (0 to keep original).</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] Load(string filePath, out int width, out int height, out int channels, int desiredChannels = 0, VkFormat desiredFormat = VkFormat.R8G8B8A8_UNORM)
        {
            if(!_supportedFormats.Contains(desiredFormat))
            {
                throw new NotSupportedException($"Desired format {desiredFormat} is not supported. Supported formats are: {string.Join(", ", _supportedFormats)}");
            }

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
                data = Swizzle(data, width, height, channels, desiredFormat);
                return data;
            }
            finally
            {
                Native.StbImage.ris_stbi_image_free(dataPtr);
            }
        }

        private byte[] Swizzle(byte[] data, int width, int height, int channels, VkFormat desiredFormat)
        {
            if (desiredFormat == VkFormat.R8G8B8A8_UNORM && channels == 4)
            {
                return data; // No swizzling needed
            }

            if (channels == 4)
            {
                // Add alpha channel
                byte[] swizzledData = new byte[width * height * 4];

                int channelIndex0 = 0;
                int channelIndex1 = 1;
                int channelIndex2 = 2;
                int channelIndex3 = 3;

                if (desiredFormat == VkFormat.B8G8R8A8_UNORM)
                {
                    channelIndex0 = 2; // R
                    channelIndex1 = 1; // G
                    channelIndex2 = 0; // B
                    channelIndex3 = 3; // A
                }
                else
                {
                    throw new NotImplementedException($"Swizzling from 4 channels to {desiredFormat} is not implemented.");
                }

                for (int i = 0; i < width * height; i++)
                {
                    swizzledData[i * 4 + channelIndex0] = data[i * 4 + 0]; // R
                    swizzledData[i * 4 + channelIndex1] = data[i * 4 + 1]; // G
                    swizzledData[i * 4 + channelIndex2] = data[i * 4 + 2]; // B
                    swizzledData[i * 4 + channelIndex3] = data[i * 4 + 3]; // B
                }
                return swizzledData;
            }

            throw new NotImplementedException();
        }
    }
}
