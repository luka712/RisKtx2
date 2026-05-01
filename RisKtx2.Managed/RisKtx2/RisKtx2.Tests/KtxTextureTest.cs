
using RisKtx2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace RisGameFramework.SpriteToolkit.Tests
{
    /// <summary>
    /// The KTX texture tests.
    /// </summary>
    [TestFixture]
    internal class KtxTextureTest
    {
        private const string TEXT_PNG = "Data/test.png";
        private const string TEST_KTX_BASIS_UASTC = "Data/test_basis_uastc.ktx2";

        /// <summary>
        /// Test try load unexisting KTX texture.
        /// </summary>
        [Test]
        public void Test_Try_Load_Unexisting_KTX_Texture()
        {
            Assert.Throws<FileNotFoundException>(() =>
            {
                var texture = new Ktx2Texture("nonexistent_file.ktx");
            });
        }

        /// <summary>
        /// Test try load existing KTX texture.
        /// </summary>
        [Test]
        public void Test_Try_Load_Existing_KTX_Texture()
        {
            try
            {
                using var texture = new Ktx2Texture(TEST_KTX_BASIS_UASTC);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Loading a valid KTX texture should not throw an exception. Exception message: {ex.Message}");
            }
        }

        /// <summary>
        /// Test try load existing KTX texture.
        /// </summary>
        [Test]
        public void Test_Confirm_KTX_Texture_Dimensions()
        {
            try
            {
                using var texture = new Ktx2Texture(TEST_KTX_BASIS_UASTC);
                Assert.That(texture.Width, Is.GreaterThan(0), "Texture width should be greater than 0.");
                Assert.That(texture.Height, Is.GreaterThan(0), "Texture height should be greater than 0.");

            }
            catch (Exception ex)
            {
                Assert.Fail($"Loading a valid KTX texture should not throw an exception. Exception message: {ex.Message}");
            }
        }

        private Ktx2Texture CreateAndFillTexture()
        {
            StbImageLoader loader = new();
            var bytes = loader.Load(TEXT_PNG, out int width, out int height, out int channels);

            var createInfo = new KtxTextureCreateInfo
            {
                BaseWidth = (uint)width,
                BaseHeight = (uint)height,
                VkFormat = VkFormat.R8G8B8A8_UNORM
            };

            var texture = new Ktx2Texture(createInfo, KtxTextureCreateStorage.ALLOC_STORAGE);
            texture.SetImageFromMemory(0, 0, 0, bytes, (uint) bytes.Length);
            return texture;
        }

        /// <summary>
        /// Test create and fill KTX texture.
        /// </summary>
        [Test]
        public void Test_Create_And_Fill_Texture()
        {
            using var texture = CreateAndFillTexture();
            var textureData = texture.GetTextureData();
            Assert.That(IntPtr.Zero, Is.Not.Zero);
        }

        /// <summary>
        /// Test create and fill KTX texture, then try to write to file.
        /// </summary>
        [Test]
        public void Test_Create_And_Fill_Texture_Write_To_File()
        {
            using var texture = CreateAndFillTexture();
            texture.WriteToNamedFile("Data/test_output.ktx2");
            Assert.That(File.Exists("Data/test_output.ktx2"), "Output KTX file should exist after writing.");
            var fileInfo = new FileInfo("Data/test_output.ktx2");
            Assert.That(fileInfo.Length, Is.GreaterThan(0), "Output KTX file should have a size greater than 0.");
        }

        /// <summary>
        /// Test create and fill KTX texture, then try to get image offset.
        /// </summary>
        [Test]
        public void Test_GetImageOffset()
        {
            using var texture = CreateAndFillTexture();
            var offset = texture.GetImageOffset(0, 0, 0);
            Assert.That(offset, Is.GreaterThanOrEqualTo(0), "Image offset should be greater than or equal to 0.");
        }

        /// <summary>
        /// Test create and fill KTX texture, then try to get image size.
        /// </summary>
        [Test]
        public void Test_Basis_Encode()
        {

        }
    }
}
