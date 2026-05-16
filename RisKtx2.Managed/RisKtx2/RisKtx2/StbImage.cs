using RisKtx2.Native;
using System.Runtime.InteropServices;
using System.Threading.Channels;


namespace RisKtx2
{

    public class StbImage 
    {
        internal StbImage(int width, int height, int channels, byte[] data, VkFormat format)
        {
            Width = width;
            Height = height;
            Channels = channels;
            Bytes = data;
            Format = format;
        }

        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// The number of color channels in the image (e.g., 3 for RGB, 4 for RGBA).
        /// </summary>
        public int Channels { get; }

        public byte[] Bytes { get; }

        /// <summary>
        /// The stride (number of bytes per row) of the image data.
        /// This is calculated as Width * Channels.
        /// </summary>
        public int Stride => Width * Channels;

        public VkFormat Format { get; }
    }
}
