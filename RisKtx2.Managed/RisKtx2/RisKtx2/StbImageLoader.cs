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
        private readonly VkFormat[] _supportedFormats = [VkFormat.R8G8B8A8_UNORM, VkFormat.B8G8R8A8_UNORM];

        public StbImageLoader()
        {
            Native.NativeResolver.Setup();
        }

   
        public byte[] Load(string filePath, out int width, out int height, out int channels, int desiredChannels = 0,
            VkFormat desiredFormat = VkFormat.R8G8B8A8_UNORM, int align = -1)
        {
            IntPtr dataPtr =
                Native.NativeStbImage.ris_stbi_load(filePath, out width, out height, out channels, desiredChannels);
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

                if (align > 0)
                {
                    data = Align(data, width, height, channels, align, out width, out height);
                }
                
                return data;
            }
            finally
            {
                Native.NativeStbImage.ris_stbi_image_free(dataPtr);
            }
        }


        public StbImage Load(string filePath, int desiredChannels = 0, VkFormat desiredFormat = VkFormat.R8G8B8A8_UNORM, int align = -1)
        {
            var bytes = Load(filePath, out int width, out int height, out int channels, desiredChannels, desiredFormat, align);
            return new StbImage(width, height, channels, bytes, desiredFormat);
        }

        public byte[] Resize(byte[] data, int inputWidth, int inputHeight, int channels, int outputWidth,
            int outputHeight)
        {
            var strideInBytes = inputWidth * channels;
            var outputStrideInBytes = outputHeight * channels;
            var outputBytes = new byte[outputWidth * outputHeight * channels];

            unsafe
            {
                fixed (byte* dataPtr = data)
                fixed (byte* outputPtr = outputBytes)
                {
                    int result = Native.NativeStbImage.ris_stbir_resize_uint8(
                        (nint)dataPtr, inputWidth, inputHeight, strideInBytes,
                        (nint)outputPtr, outputWidth, outputHeight, outputStrideInBytes,
                        channels);
                    if (result == 0)
                    {
                        throw new Exception("Failed to resize image.");
                    }

                    var dataSize = outputWidth * outputHeight * channels;
                    Marshal.Copy((nint)outputPtr, outputBytes, 0, dataSize);
                }
            }

            return outputBytes;
        }

        
        public byte[] Align(byte[] data, int inputWidth, int inputHeight, int channels, int alignment,
            out int outputWidth, out int outputHeight)
        {
            outputWidth = (inputWidth + alignment - 1) & ~(alignment - 1);
            outputHeight = (inputHeight + alignment - 1) / alignment * alignment;

            // Go through each row and align the data
            var outputBytes = new byte[outputWidth * outputHeight * channels];
            for (int y = 0; y < inputHeight; y++)
            {
                int rowSourceStartIndex = y * inputWidth * channels;
                int rowDestStartIndex = y * outputWidth * channels;
                Array.Copy(data, rowSourceStartIndex, outputBytes, rowDestStartIndex, inputWidth * channels);

                int copySize = outputWidth - inputWidth;
                int lastSourceIndex = rowSourceStartIndex + (inputWidth - 1) * channels;
                int destIndex = rowDestStartIndex + inputWidth * channels;
                for (int i = 0; i < copySize; i++)
                {
                    for (int b = 0; b < channels; b++)
                    {
                        outputBytes[destIndex + i + b] = data[lastSourceIndex + b];
                    }

                    destIndex += channels;
                }
            }

            // Repeat the last row to fill the rest with the output height
            for (int y = inputHeight; y < outputHeight; y++)
            {
                int lastFilledRowIndex = (inputHeight - 1) * outputWidth * channels;
                int destIndex = y * outputWidth * channels;
                Array.Copy(outputBytes, lastFilledRowIndex, outputBytes, destIndex, outputWidth * channels);
            }
            
            return outputBytes;
        }


        public StbImage Align(StbImage image, int alignment)
        {
            byte[] alignedBytes = Align(image.Bytes, image.Width, image.Height, image.Channels, alignment,
                out int outputWidth, out int outputHeight);
            return new StbImage(outputWidth, outputHeight, image.Channels, alignedBytes, image.Format);
        }

        public StbImage Resize(StbImage image, int outputWidth, int outputHeight)
        {
            byte[] resizedBytes = Resize(image.Bytes, image.Width, image.Height, image.Channels, outputWidth,
                outputHeight);
            return new StbImage(outputWidth, outputHeight, image.Channels, resizedBytes, image.Format);
        }

        internal static byte[] Swizzle(byte[] data, int width, int height, int channels, VkFormat desiredFormat)
        {
            if (desiredFormat == VkFormat.R8G8B8A8_UNORM && channels == 4)
            {
                return data; // No swizzling needed
            }

            if (channels == 4)
            {
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
                    throw new NotImplementedException(
                        $"Swizzling from 4 channels to {desiredFormat} is not implemented.");
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
            else if (channels == 3)
            {
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
                else if
                    (desiredFormat !=
                     VkFormat.R8G8B8A8_UNORM) // The default format is R8G8B8A8_UNORM, so if not in that throw.
                {
                    throw new NotImplementedException(
                        $"Swizzling from 3 channels to {desiredFormat} is not implemented.");
                }

                for (int i = 0; i < width * height; i++)
                {
                    swizzledData[i * 4 + channelIndex0] = data[i * 3 + 0]; // R
                    swizzledData[i * 4 + channelIndex1] = data[i * 3 + 1]; // G
                    swizzledData[i * 4 + channelIndex2] = data[i * 3 + 2]; // B
                    swizzledData[i * 4 + channelIndex3] = 255;
                }

                return swizzledData;
            }

            throw new NotImplementedException();
        }
    }
}