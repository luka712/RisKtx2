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
        private const VkFormat DEFAULT_4_CHANNEL_FORMAT = VkFormat.R8G8B8A8_UNORM;
        private const VkFormat DEFAULT_3_CHANNEL_FORMAT = VkFormat.B8G8R8_UNORM;

        private static readonly Dictionary<VkFormat, int> VkFormatToChannels = new()
        {
            [VkFormat.R8G8B8A8_UNORM] = 4,
            [VkFormat.B8G8R8A8_UNORM] = 4,
            [VkFormat.R8G8B8_UNORM] = 3,
            [VkFormat.B8G8R8_UNORM] = 3,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="StbImageLoader"/> class.
        /// Sets up the native library resolver for stb_image functions.
        /// </summary>
        public StbImageLoader()
        {
            Native.NativeResolver.Setup();
        }

        /// <summary>
        /// If true, the image will be flipped vertically when loaded.
        /// </summary>
        public bool VerticalFlip { get; set; }

        /// <summary>
        /// Gets the default format for the given number of channels.
        /// </summary>
        /// <param name="channels">The number of channels.</param>
        /// <returns>The <see cref="VkFormat"/>.</returns>
        private static VkFormat GetDefaultFormat(int channels)
        {
            if (channels == 4)
            {
                return DEFAULT_4_CHANNEL_FORMAT;
            }

            return DEFAULT_3_CHANNEL_FORMAT;
        }

        /// <summary>
        /// The low-level call to load an image from a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="channels">Outputs # of image components in the image file.</param>
        /// <param name="desiredChannels">If non-zero, # of image components requested in the result.</param>
        /// <returns>The returned bytes.</returns>
        /// <exception cref="ArgumentException">If file path is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If desiredChannels is negative.</exception>
        /// <exception cref="FileNotFoundException">If the image file could not be loaded.</exception>
        public byte[] Load(string filePath, out int width, out int height, out int channels, int desiredChannels = 0)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (desiredChannels < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(desiredChannels), "Desired channels cannot be negative.");
            }

            Native.NativeStbImage.ris_stbi_set_flip_vertically_on_load(VerticalFlip);

            IntPtr dataPtr =
                Native.NativeStbImage.ris_stbi_load(filePath, out width, out height, out channels, desiredChannels);
            if (dataPtr == IntPtr.Zero)
            {
                throw new FileNotFoundException($"Failed to load image: {filePath}", filePath);
            }

            try
            {
                // Use actual channels from file if desiredChannels is 0
                int actualChannels = desiredChannels > 0 ? desiredChannels : channels;
                int dataSize = width * height * actualChannels;
                byte[] data = new byte[dataSize];
                Marshal.Copy(dataPtr, data, 0, dataSize);
                return data;
            }
            finally
            {
                Native.NativeStbImage.ris_stbi_image_free(dataPtr);
            }
        }

        /// <summary>
        /// The low-level call to load an image from a file which returns the image data in the desired format.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="desiredFormat">The desired format.</param>
        /// <returns>The loaded <see cref="StbImage"/>.</returns>
        /// <exception cref="ArgumentException">If file path is null or empty, or if the format is not supported.</exception>
        /// <exception cref="FileNotFoundException">If the image file could not be loaded.</exception>
        public StbImage Load(string filePath, VkFormat? desiredFormat = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            int desiredChannels = 0;

            if (desiredFormat.HasValue)
            {
                if (!VkFormatToChannels.TryGetValue(desiredFormat.Value, out desiredChannels))
                {
                    throw new ArgumentException($"Unsupported format: {desiredFormat.Value}. Supported formats are: {string.Join(", ", VkFormatToChannels.Keys)}", nameof(desiredFormat));
                }
            }

            var bytes = Load(filePath, out int width, out int height, out int channels, desiredChannels);

            // Use actual channels from file if desiredChannels is 0
            int actualChannels = desiredChannels > 0 ? desiredChannels : channels;

            if (desiredFormat.HasValue)
            {
                bytes = Swizzle(bytes, width, height, channels, desiredFormat.Value);
            }

            VkFormat format = desiredFormat ?? GetDefaultFormat(channels);

            return new StbImage(width, height, actualChannels, bytes, format);
        }

        /// <summary>
        /// The low-level call to load an image from a file that returns the image data in the desired format.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="align">The alignment. This is useful when working with format that need to be aligned, such as when trying to create basis formats. Must be a power of 2.</param>
        /// <param name="desiredFormat">The desired format.</param>
        /// <returns>The loaded <see cref="StbImage"/>.</returns>
        /// <exception cref="ArgumentException">If file path is null or empty, if alignment is invalid, or if the format is not supported.</exception>
        /// <exception cref="FileNotFoundException">If the image file could not be loaded.</exception>
        public StbImage Load(string filePath, int align, VkFormat? desiredFormat = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (align < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(align), "Alignment cannot be negative.");
            }

            if (align > 0 && (align & (align - 1)) != 0)
            {
                throw new ArgumentException("Alignment must be a power of 2.", nameof(align));
            }

            int desiredChannels = 0;

            if (desiredFormat.HasValue)
            {
                if (!VkFormatToChannels.TryGetValue(desiredFormat.Value, out desiredChannels))
                {
                    throw new ArgumentException($"Unsupported format: {desiredFormat.Value}. Supported formats are: {string.Join(", ", VkFormatToChannels.Keys)}", nameof(desiredFormat));
                }
            }

            var bytes = Load(filePath, out int width, out int height, out int channels, desiredChannels);

            // Use actual channels from file if desiredChannels is 0
            int actualChannels = desiredChannels > 0 ? desiredChannels : channels;

            if (desiredFormat.HasValue)
            {
                bytes = Swizzle(bytes, width, height, actualChannels, desiredFormat.Value);
            }

            if (align > 0)
            {
                bytes = Align(bytes, width, height, actualChannels, align, out width, out height);
            }

            VkFormat format = desiredFormat ?? GetDefaultFormat(channels);

            return new StbImage(width, height, actualChannels, bytes, format);
        }


        /// <summary>
        /// Resizes the image data to the specified output dimensions using stb_image_resize.
        /// </summary>
        /// <param name="data">The source image data as a byte array.</param>
        /// <param name="inputWidth">The width of the input image in pixels.</param>
        /// <param name="inputHeight">The height of the input image in pixels.</param>
        /// <param name="channels">The number of color channels in the image.</param>
        /// <param name="outputWidth">The desired width of the output image in pixels.</param>
        /// <param name="outputHeight">The desired height of the output image in pixels.</param>
        /// <returns>A new byte array containing the resized image data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when dimensions or channels are invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the resize operation fails.</exception>
        public byte[] Resize(byte[] data, int inputWidth, int inputHeight, int channels, int outputWidth,
            int outputHeight)
        {
            ArgumentNullException.ThrowIfNull(data);

            if (inputWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inputWidth), "Input width must be greater than 0.");
            }

            if (inputHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inputHeight), "Input height must be greater than 0.");
            }

            if (channels <= 0 || channels > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(channels), "Channels must be between 1 and 4.");
            }

            if (outputWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(outputWidth), "Output width must be greater than 0.");
            }

            if (outputHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(outputHeight), "Output height must be greater than 0.");
            }

            int expectedDataSize = inputWidth * inputHeight * channels;
            if (data.Length < expectedDataSize)
            {
                throw new ArgumentException($"Data array is too small. Expected at least {expectedDataSize} bytes, but got {data.Length}.", nameof(data));
            }

            var strideInBytes = inputWidth * channels;
            var outputStrideInBytes = outputWidth * channels;
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
                        throw new InvalidOperationException("Failed to resize image.");
                    }

                    var dataSize = outputWidth * outputHeight * channels;
                    Marshal.Copy((nint)outputPtr, outputBytes, 0, dataSize);
                }
            }

            return outputBytes;
        }

        /// <summary>
        /// Aligns the image data to the specified alignment.
        /// </summary>
        /// <param name="data">The data to align.</param>
        /// <param name="inputWidth">The input width.</param>
        /// <param name="inputHeight">The input height.</param>
        /// <param name="channels">The number of channels.</param>
        /// <param name="alignment">The desired alignment.</param>
        /// <param name="outputWidth">The aligned width.</param>
        /// <param name="outputHeight">The aligned height.</param>
        /// <returns>New aligned bytes.</returns>
        private static byte[] Align(
            byte[] data, int inputWidth, int inputHeight, int channels,
            int alignment,
            out int outputWidth, out int outputHeight)
        {
            outputWidth = (inputWidth + alignment - 1) & ~(alignment - 1);
            outputHeight = (inputHeight + alignment - 1) & ~(alignment - 1);

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

        /// <summary>
        /// Resizes an <see cref="StbImage"/> to the specified output dimensions.
        /// </summary>
        /// <param name="image">The source image to resize.</param>
        /// <param name="outputWidth">The desired width of the output image in pixels.</param>
        /// <param name="outputHeight">The desired height of the output image in pixels.</param>
        /// <returns>A new <see cref="StbImage"/> containing the resized image data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when image is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when output dimensions are invalid.</exception>
        public StbImage Resize(StbImage image, int outputWidth, int outputHeight)
        {
            ArgumentNullException.ThrowIfNull(image);

            if (outputWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(outputWidth), "Output width must be greater than 0.");
            }

            if (outputHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(outputHeight), "Output height must be greater than 0.");
            }

            byte[] resizedBytes = Resize(image.Bytes, image.Width, image.Height, image.Channels, outputWidth,
                outputHeight);
            return new StbImage(outputWidth, outputHeight, image.Channels, resizedBytes, image.Format);
        }

        /// <summary>
        /// Swizzles (reorders) the color channels of the image data to match the desired format.
        /// For example, converts RGBA to BGRA format. Also handles expanding 3-channel images to 4-channel.
        /// </summary>
        /// <param name="data">The source image data as a byte array.</param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="channels">The number of color channels in the source image (3 or 4).</param>
        /// <param name="desiredFormat">The target <see cref="VkFormat"/> to swizzle the data to.</param>
        /// <returns>A new byte array with swizzled color channel data.</returns>
        /// <exception cref="NotSupportedException">Thrown when the requested format conversion is not supported.</exception>
        private static byte[] Swizzle(byte[] data, int width, int height, int channels, VkFormat desiredFormat)
        {
            if (desiredFormat == VkFormat.R8G8B8A8_UNORM && channels == 4)
            {
                return data; // No swizzling needed
            }
            
            int desiredChannels = VkFormatToChannels[desiredFormat];
            byte[] swizzledData = new byte[width * height * desiredChannels];
            
            int channelIndex0 = 0;
            int channelIndex1 = 1;
            int channelIndex2 = 2;
            int channelIndex3 = 3;

            if (channels == 4)
            {
                if (desiredFormat == VkFormat.B8G8R8A8_UNORM)
                {
                    channelIndex0 = 2; // R
                    channelIndex1 = 1; // G
                    channelIndex2 = 0; // B
                    channelIndex3 = 3; // A
                }
                else if (desiredFormat == VkFormat.R8G8B8A8_UNORM)
                {
                    channelIndex0 = 0; // R
                    channelIndex1 = 1; // G
                    channelIndex2 = 2; // B
                    channelIndex3 = 3; // A
                }
                else
                {
                    throw new NotSupportedException(
                        $"Swizzling from 4 channels to {desiredFormat} is not supported.");
                }
            }
            else if (channels == 3)
            {
                if (desiredFormat == VkFormat.B8G8R8A8_UNORM)
                {
                    channelIndex0 = 2; // R
                    channelIndex1 = 1; // G
                    channelIndex2 = 0; // B
                    channelIndex3 = 3; // A
                }
                else if (desiredFormat == VkFormat.R8G8B8A8_UNORM)
                {
                    channelIndex0 = 0; // R
                    channelIndex1 = 1; // G
                    channelIndex2 = 2; // B
                    channelIndex3 = 3; // A
                }
                else if (desiredFormat == VkFormat.B8G8R8_UNORM)
                {
                    channelIndex0 = 2; // R
                    channelIndex1 = 1; // G
                    channelIndex2 = 0; // B
                    channelIndex3 = -1; // A not relevant, set to -1 to avoid issues later 
                }
                else if (desiredFormat == VkFormat.R8G8B8_UNORM)
                {
                    channelIndex0 = 0; // R
                    channelIndex1 = 1; // G
                    channelIndex2 = 2; // B
                    channelIndex3 = -1; // A not relevant, set to -1 to avoid issues later 
                }
                else
                {
                    throw new NotSupportedException(
                        $"Swizzling from 3 channels to {desiredFormat} is not supported.");
                }
            }
            
            for (int i = 0; i < width * height; i++)
            {
                swizzledData[i * desiredChannels + channelIndex0] = data[i * channels + 0]; // R
                swizzledData[i * desiredChannels + channelIndex1] = data[i * channels + 1]; // G
                swizzledData[i * desiredChannels + channelIndex2] = data[i * channels + 2]; // B

                if (channelIndex3 > -1 && channels == 3)
                {
                    swizzledData[i * desiredChannels + channelIndex3] = 255;
                }
                else if (channelIndex3 > -1 && channels == 4)
                {
                    swizzledData[i * desiredChannels + channelIndex3] = data[i * channels + 3]; // A;
                }
            }

            return swizzledData;
        }
    }
}
