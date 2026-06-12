namespace RisKtx2.Tests
{
    /// <summary>
    /// The KTX texture tests.
    /// </summary>
    [TestFixture]
    internal class KtxTextureTest
    {
        #region Constants

        private const string TEXT_PNG = "Data/test.png";
        private const string TEST_KTX_BASIS_UASTC = "Data/test_basis_uastc.ktx2";
        private const string TEST_OUTPUT_KTX2 = "Data/test_output.ktx2";
        private const string TEST_COMPRESSED_KTX2 = "Data/test_compressed.ktx2";
        private const string TEST_COMPRESSED_WITH_MIPS_KTX2 = "Data/test_compressed_with_mips.ktx2";
        private const string TEST_ASTC_COMPRESSED_KTX2 = "Data/test_astc_compressed.ktx2";

        private const uint MIP_LEVEL_0 = 0;
        private const uint LAYER_0 = 0;
        private const uint FACE_SLICE_0 = 0;
        private const byte MAX_BASIS_QUALITY = 255;

        #endregion

        #region File Loading Tests

        /// <summary>
        /// Test loading a non-existent KTX texture file throws FileNotFoundException.
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
        /// Test loading an existing KTX texture file succeeds.
        /// </summary>
        [Test]
        public void Test_Try_Load_Existing_KTX_Texture()
        {
            using var texture = new Ktx2Texture(TEST_KTX_BASIS_UASTC);
            Assert.Pass("KTX texture loaded successfully.");
        }

        /// <summary>
        /// Test confirming that loaded KTX texture has valid dimensions.
        /// </summary>
        [Test]
        public void Test_Confirm_KTX_Texture_Dimensions()
        {
            using var texture = new Ktx2Texture(TEST_KTX_BASIS_UASTC);

            Assert.That(texture.Width, Is.GreaterThan(0), "Texture width should be greater than 0.");
            Assert.That(texture.Height, Is.GreaterThan(0), "Texture height should be greater than 0.");
        }

        #endregion

        #region Texture Creation Tests

        /// <summary>
        /// Test creating and filling a KTX texture from PNG data.
        /// </summary>
        [Test]
        public void Test_Create_And_Fill_Texture()
        {
            using var texture = CreateAndFillTexture();

            var textureData = texture.GetTextureData();
            Assert.That(textureData, Is.Not.EqualTo(IntPtr.Zero), "Texture data pointer should not be null.");
        }

        /// <summary>
        /// Test creating and filling a KTX texture, then writing it to a file.
        /// </summary>
        [Test]
        public void Test_Create_And_Fill_Texture_Write_To_File()
        {
            try
            {
                using var texture = CreateAndFillTexture();

                // Write texture to a file
                texture.WriteToNamedFile(TEST_OUTPUT_KTX2);

                // Verify a file was created
                Assert.That(File.Exists(TEST_OUTPUT_KTX2), "Output KTX file should exist after writing.");

                var fileInfo = new FileInfo(TEST_OUTPUT_KTX2);
                Assert.That(fileInfo.Length, Is.GreaterThan(0), "Output KTX file should have a size greater than 0.");
            }
            finally
            {
                // Cleanup: Delete test output file
                if (File.Exists(TEST_OUTPUT_KTX2))
                {
                    File.Delete(TEST_OUTPUT_KTX2);
                }
            }
        }

        /// <summary>
        /// Test getting the image offset for a specific mip level, layer, and face.
        /// </summary>
        [Test]
        public void Test_GetImageOffset()
        {
            using var texture = CreateAndFillTexture();

            var offset = texture.GetImageOffset(MIP_LEVEL_0, LAYER_0, FACE_SLICE_0);
            Assert.That(offset, Is.GreaterThanOrEqualTo(0), "Image offset should be greater than or equal to 0.");
        }

        #endregion

        #region Basis Compression Tests

        /// <summary>
        /// Test compressing a texture to Basis Universal format with maximum quality.
        /// </summary>
        [Test]
        public void Test_CompressBasis()
        {
            try
            {
                using var texture = CreateAndFillTexture();

                // Compress to Basis Universal format with maximum quality
                texture.CompressBasis(MAX_BASIS_QUALITY);

                // Write compressed texture to a file
                texture.WriteToNamedFile(TEST_COMPRESSED_KTX2);

                // Verify a file was created
                Assert.That(File.Exists(TEST_COMPRESSED_KTX2), "Compressed KTX file should exist after writing.");

                var fileInfo = new FileInfo(TEST_COMPRESSED_KTX2);
                Assert.That(fileInfo.Length, Is.GreaterThan(0),
                    "Compressed KTX file should have a size greater than 0.");
            }
            finally
            {
                // Cleanup: Delete test output file
                if (File.Exists(TEST_COMPRESSED_KTX2))
                {
                    File.Delete(TEST_COMPRESSED_KTX2);
                }
            }
        }

        /// <summary>
        /// Test compressing a texture with mip levels to Basis Universal UASTC format.
        /// </summary>
        [Test]
        public void Test_CompressBasisWithMipLevels()
        {
            // Create texture with mip levels
            using var texture = CreateTextureWithMipLevels(out int expectedMipLevels);

            // Compress to Basis Universal UASTC format
            texture.CompressBasis(new KtxBasisParams
            {
                Uastc = true,
                QualityLevel = MAX_BASIS_QUALITY,
            });

            // Write compressed texture with mips to a file
            texture.WriteToNamedFile(TEST_COMPRESSED_WITH_MIPS_KTX2);

            // Verify a file was created
            Assert.That(File.Exists(TEST_COMPRESSED_WITH_MIPS_KTX2),
                "Compressed KTX file with mips should exist after writing.");

            // Load the compressed texture and verify mip levels are preserved
            using var loadedTexture = new Ktx2Texture(TEST_COMPRESSED_WITH_MIPS_KTX2);
            Assert.That(loadedTexture.NumLevels, Is.EqualTo(expectedMipLevels),
                $"Loaded texture should have {expectedMipLevels} mip levels.");
        }

        #endregion
        
        #region ASTC Compression Tests
        
        /// <summary>
        /// Test compressing a texture to Astc format with maximum quality.
        /// </summary>
        [Test]
        public void Test_CompressAstc()
        {
            try
            {
                using var texture = CreateAndFillTexture();

                // Compress to Basis Universal format with maximum quality
                texture.CompressAstc();

                // Write compressed texture to a file
                texture.WriteToNamedFile(TEST_ASTC_COMPRESSED_KTX2);

                // Verify a file was created
                Assert.That(File.Exists(TEST_ASTC_COMPRESSED_KTX2), "Compressed KTX file should exist after writing.");

                var fileInfo = new FileInfo(TEST_ASTC_COMPRESSED_KTX2);
                Assert.That(fileInfo.Length, Is.GreaterThan(0),
                    "Compressed KTX file should have a size greater than 0.");
            }
            finally
            {
                // Cleanup: Delete test output file
                if (File.Exists(TEST_ASTC_COMPRESSED_KTX2))
                {
                    File.Delete(TEST_ASTC_COMPRESSED_KTX2);
                }
            }
        }
        
        /// <summary>
        /// Test compressing a texture to Astc format with maximum quality.
        /// </summary>
        [Test]
        public void Test_CompressAstcParams()
        {
            try
            {
                using var texture = CreateAndFillTexture();

                // Compress to Basis Universal format with maximum quality
                texture.CompressAstc(new KtxAstcParams()
                {
                    QualityLevel = KtxPackAstcQualityLevels.EXHAUSTIVE
                });

                // Write compressed texture to a file
                texture.WriteToNamedFile(TEST_ASTC_COMPRESSED_KTX2);

                // Verify a file was created
                Assert.That(File.Exists(TEST_ASTC_COMPRESSED_KTX2), "Compressed KTX file should exist after writing.");

                var fileInfo = new FileInfo(TEST_ASTC_COMPRESSED_KTX2);
                Assert.That(fileInfo.Length, Is.GreaterThan(0),
                    "Compressed KTX file should have a size greater than 0.");
            }
            finally
            {
                // Cleanup: Delete test output file
                if (File.Exists(TEST_ASTC_COMPRESSED_KTX2))
                {
                    File.Delete(TEST_ASTC_COMPRESSED_KTX2);
                }
            }
        }
        
        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a KTX texture from a PNG file and fills it with image data.
        /// </summary>
        /// <returns>A newly created and filled Ktx2Texture.</returns>
        private Ktx2Texture CreateAndFillTexture()
        {
            // Load image data from PNG file using STB
            StbImageLoader loader = new();
            var bytes = loader.Load(TEXT_PNG, out int width, out int height, out int channels, align: 4);

            // Create KTX texture with appropriate format
            var createInfo = new KtxTextureCreateInfo
            {
                BaseWidth = (uint)width,
                BaseHeight = (uint)height,
                VkFormat = VkFormat.R8G8B8A8_UNORM,
            };

            var texture = new Ktx2Texture(createInfo, KtxTextureCreateStorage.ALLOC_STORAGE);

            // Set image data for base mip level
            texture.SetImageFromMemory(MIP_LEVEL_0, LAYER_0, FACE_SLICE_0, bytes, (uint)bytes.Length);

            return texture;
        }

        /// <summary>
        /// Creates a KTX texture with multiple mip levels from a PNG file.
        /// </summary>
        /// <param name="mipLevelCount">Output parameter containing the number of mip levels created.</param>
        /// <returns>A newly created Ktx2Texture with mip levels.</returns>
        private Ktx2Texture CreateTextureWithMipLevels(out int mipLevelCount)
        {
            StbImageLoader loader = new();
            var level0Image = loader.Load(TEXT_PNG, align: 4);

            // Generate mip chain by repeatedly halving dimensions
            var width = level0Image.Width;
            var height = level0Image.Height;
            List<StbImage> mipLevels = [level0Image];

            while (width > 1 && height > 1)
            {
                width /= 2;
                height /= 2;

                var lastMip = mipLevels.Last();
                var currentMip = loader.Resize(lastMip, width, height);
                mipLevels.Add(currentMip);
            }

            mipLevelCount = mipLevels.Count;

            // Create KTX texture with mip levels
            var createInfo = new KtxTextureCreateInfo
            {
                BaseWidth = (uint)level0Image.Width,
                BaseHeight = (uint)level0Image.Height,
                VkFormat = VkFormat.R8G8B8A8_UNORM,
                NumLevels = (uint)mipLevels.Count,
            };

            var texture = new Ktx2Texture(createInfo, KtxTextureCreateStorage.ALLOC_STORAGE);

            // Set image data for each mip level
            for (int i = 0; i < mipLevels.Count; i++)
            {
                var image = mipLevels[i];
                texture.SetImageFromMemory((uint)i, LAYER_0, FACE_SLICE_0, image.Bytes, (uint)image.Bytes.Length);
            }

            return texture;
        }

        #endregion
    }
}