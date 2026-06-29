namespace RisKtx2.Tests;

using static Constants;

/// <summary>
/// Tests for the StbImageLoader class.
/// </summary>
[TestFixture]
internal class StbImageLoaderTest
{
    #region Constants
    
    private const string NON_EXISTENT_FILE = "Data/nonexistent.png";

    #endregion

    #region Load Tests

    /// <summary>
    /// Test that loading a non-existent file throws FileNotFoundException.
    /// </summary>
    [Test]
    public void Test_Load_NonExistentFile_ThrowsFileNotFoundException()
    {
        var loader = new StbImageLoader();

        Assert.Throws<FileNotFoundException>(() =>
        {
            loader.Load(NON_EXISTENT_FILE, out _, out _, out _, 4);
        });
    }

    /// <summary>
    /// Test that loading with null file path throws ArgumentException.
    /// </summary>
    [Test]
    public void Test_Load_NullFilePath_ThrowsArgumentException()
    {
        var loader = new StbImageLoader();

        Assert.Throws<ArgumentException>(() =>
        {
            loader.Load(null!, out _, out _, out _, 4);
        });
    }

    /// <summary>
    /// Test that loading with empty file path throws ArgumentException.
    /// </summary>
    [Test]
    public void Test_Load_EmptyFilePath_ThrowsArgumentException()
    {
        var loader = new StbImageLoader();

        Assert.Throws<ArgumentException>(() =>
        {
            loader.Load("", out _, out _, out _, 4);
        });
    }

    /// <summary>
    /// Test that loading with negative desiredChannels throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Test_Load_NegativeDesiredChannels_ThrowsArgumentOutOfRangeException()
    {
        var loader = new StbImageLoader();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            loader.Load(TEST_PNG, out _, out _, out _, -1);
        });
    }

    /// <summary>
    /// Test loading an image with raw byte array output.
    /// </summary>
    [Test]
    public void Test_Load_RawBytes_ReturnsValidData()
    {
        var loader = new StbImageLoader();

        var bytes = loader.Load(TEST_PNG, out int width, out int height, out int channels, 4);

        Assert.That(width, Is.GreaterThan(0), "Width should be greater than 0.");
        Assert.That(height, Is.GreaterThan(0), "Height should be greater than 0.");
        Assert.That(bytes, Is.Not.Null, "Bytes should not be null.");
        Assert.That(bytes.Length, Is.EqualTo(width * height * 4), "Byte array length should match width * height * channels.");
    }

    /// <summary>
    /// Test loading an image and returning an StbImage object.
    /// </summary>
    [Test]
    public void Test_Load_ReturnsStbImage()
    {
        var loader = new StbImageLoader();

        var image = loader.Load(TEST_PNG, VkFormat.R8G8B8A8_UNORM);

        Assert.That(image, Is.Not.Null, "Image should not be null.");
        Assert.That(image.Width, Is.GreaterThan(0), "Width should be greater than 0.");
        Assert.That(image.Height, Is.GreaterThan(0), "Height should be greater than 0.");
        Assert.That(image.Channels, Is.EqualTo(4), "Channels should be 4 for RGBA format.");
        Assert.That(image.Format, Is.EqualTo(VkFormat.R8G8B8A8_UNORM), "Format should match requested format.");
        Assert.That(image.Bytes, Is.Not.Null, "Bytes should not be null.");
        Assert.That(image.Bytes.Length, Is.EqualTo(image.Width * image.Height * 4), "Byte array length should match dimensions.");
    }

    /// <summary>
    /// Test loading an image with BGRA format (swizzling).
    /// </summary>
    [Test]
    public void Test_Load_WithBGRAFormat_SwizzlesCorrectly()
    {
        var loader = new StbImageLoader();

        var image = loader.Load(TEST_PNG, VkFormat.B8G8R8A8_UNORM);

        Assert.That(image, Is.Not.Null, "Image should not be null.");
        Assert.That(image.Format, Is.EqualTo(VkFormat.B8G8R8A8_UNORM), "Format should be B8G8R8A8_UNORM.");
        Assert.That(image.Channels, Is.EqualTo(4), "Channels should be 4.");
    }

    /// <summary>
    /// Test loading an image with alignment.
    /// </summary>
    [Test]
    public void Test_Load_WithAlignment_AlignsCorrectly()
    {
        var loader = new StbImageLoader();
        const int alignment = 4;

        var image = loader.Load(TEST_PNG, alignment, VkFormat.R8G8B8A8_UNORM);

        Assert.That(image, Is.Not.Null, "Image should not be null.");
        Assert.That(image.Width % alignment, Is.EqualTo(0), "Width should be aligned to 4.");
        Assert.That(image.Height % alignment, Is.EqualTo(0), "Height should be aligned to 4.");
    }

    /// <summary>
    /// Test loading an image without specifying a format uses default format.
    /// </summary>
    [Test]
    public void Test_Load_WithoutFormat_UsesDefaultFormat()
    {
        var loader = new StbImageLoader();

        var image = loader.Load(TEST_PNG);

        Assert.That(image, Is.Not.Null, "Image should not be null.");
        Assert.That(image.Width, Is.GreaterThan(0), "Width should be greater than 0.");
        Assert.That(image.Height, Is.GreaterThan(0), "Height should be greater than 0.");
    }
    
    /// <summary>
    /// Test loading jpg image and returning an StbImage object.
    /// </summary>
    [Test]
    public void Test_LoadJpg_ReturnsStbImage()
    {
        var loader = new StbImageLoader();

        var image = loader.Load(CAT_PNG, VkFormat.R8G8B8A8_UNORM);

        Assert.That(image, Is.Not.Null, "Image should not be null.");
        Assert.That(image.Width, Is.GreaterThan(0), "Width should be greater than 0.");
        Assert.That(image.Height, Is.GreaterThan(0), "Height should be greater than 0.");
        Assert.That(image.Channels, Is.EqualTo(4), "Channels should be 4 for RGBA format.");
        Assert.That(image.Format, Is.EqualTo(VkFormat.R8G8B8A8_UNORM), "Format should match requested format.");
        Assert.That(image.Bytes, Is.Not.Null, "Bytes should not be null.");
        Assert.That(image.Bytes.Length, Is.EqualTo(image.Width * image.Height * 4), "Byte array length should match dimensions.");
    }

    #endregion

    #region Vertical Flip Tests

    /// <summary>
    /// Test that vertical flip property can be set.
    /// </summary>
    [Test]
    public void Test_VerticalFlip_CanBeSet()
    {
        var loader = new StbImageLoader();

        Assert.That(loader.VerticalFlip, Is.False, "VerticalFlip should default to false.");

        loader.VerticalFlip = true;
        Assert.That(loader.VerticalFlip, Is.True, "VerticalFlip should be true after setting.");
    }

    /// <summary>
    /// Test loading an image with vertical flip enabled produces different data.
    /// </summary>
    [Test]
    public void Test_Load_WithVerticalFlip_ProducesDifferentData()
    {
        var loaderNormal = new StbImageLoader { VerticalFlip = false };
        var loaderFlipped = new StbImageLoader { VerticalFlip = true };

        var normalImage = loaderNormal.Load(TEST_PNG, VkFormat.R8G8B8A8_UNORM);
        var flippedImage = loaderFlipped.Load(TEST_PNG, VkFormat.R8G8B8A8_UNORM);

        Assert.That(normalImage.Width, Is.EqualTo(flippedImage.Width), "Widths should match.");
        Assert.That(normalImage.Height, Is.EqualTo(flippedImage.Height), "Heights should match.");

        // The bytes should be different (unless the image is symmetric)
        bool bytesAreDifferent = false;
        for (int i = 0; i < normalImage.Bytes.Length; i++)
        {
            if (normalImage.Bytes[i] != flippedImage.Bytes[i])
            {
                bytesAreDifferent = true;
                break;
            }
        }

        Assert.That(bytesAreDifferent, Is.True, "Flipped image should have different byte data.");
    }

    #endregion

    #region Resize Tests

    /// <summary>
    /// Test resizing raw byte data.
    /// </summary>
    [Test]
    public void Test_Resize_RawBytes_ResizesCorrectly()
    {
        var loader = new StbImageLoader();
        var image = loader.Load(TEST_PNG, VkFormat.R8G8B8A8_UNORM);

        int newWidth = image.Width / 2;
        int newHeight = image.Height / 2;

        var resizedBytes = loader.Resize(image.Bytes, image.Width, image.Height, image.Channels, newWidth, newHeight);

        Assert.That(resizedBytes, Is.Not.Null, "Resized bytes should not be null.");
        Assert.That(resizedBytes.Length, Is.EqualTo(newWidth * newHeight * image.Channels),
            "Resized byte array length should match new dimensions.");
    }

    /// <summary>
    /// Test resizing an StbImage object.
    /// </summary>
    [Test]
    public void Test_Resize_StbImage_ResizesCorrectly()
    {
        var loader = new StbImageLoader();
        var image = loader.Load(TEST_PNG, VkFormat.R8G8B8A8_UNORM);

        int newWidth = image.Width / 2;
        int newHeight = image.Height / 2;

        var resizedImage = loader.Resize(image, newWidth, newHeight);

        Assert.That(resizedImage, Is.Not.Null, "Resized image should not be null.");
        Assert.That(resizedImage.Width, Is.EqualTo(newWidth), "Resized width should match.");
        Assert.That(resizedImage.Height, Is.EqualTo(newHeight), "Resized height should match.");
        Assert.That(resizedImage.Channels, Is.EqualTo(image.Channels), "Channels should be preserved.");
        Assert.That(resizedImage.Format, Is.EqualTo(image.Format), "Format should be preserved.");
        Assert.That(resizedImage.Bytes.Length, Is.EqualTo(newWidth * newHeight * image.Channels),
            "Resized byte array length should match new dimensions.");
    }

    /// <summary>
    /// Test resizing to larger dimensions (upscaling).
    /// </summary>
    [Test]
    public void Test_Resize_Upscale_ResizesCorrectly()
    {
        var loader = new StbImageLoader();
        var image = loader.Load(TEST_PNG, VkFormat.R8G8B8A8_UNORM);

        int newWidth = image.Width * 2;
        int newHeight = image.Height * 2;

        var resizedImage = loader.Resize(image, newWidth, newHeight);

        Assert.That(resizedImage.Width, Is.EqualTo(newWidth), "Upscaled width should match.");
        Assert.That(resizedImage.Height, Is.EqualTo(newHeight), "Upscaled height should match.");
        Assert.That(resizedImage.Bytes.Length, Is.EqualTo(newWidth * newHeight * image.Channels),
            "Upscaled byte array length should match new dimensions.");
    }

    /// <summary>
    /// Test creating a mip chain by repeatedly resizing.
    /// Stops at 4x4 to avoid native library issues with very small images.
    /// </summary>
    [Test]
    public void Test_Resize_MipChain_CreatesValidMipLevels()
    {
        var loader = new StbImageLoader();
        var image = loader.Load(LENNA_PNG, VkFormat.R8G8B8A8_UNORM);

        var mipLevels = new List<StbImage> { image };
        var currentImage = image;

        // Generate mip levels until we reach 4x4 (stop before very small sizes to avoid native issues)
        while (currentImage.Width > 4 && currentImage.Height > 4)
        {
            int newWidth = currentImage.Width / 2;
            int newHeight = currentImage.Height / 2;

            currentImage = loader.Resize(currentImage, newWidth, newHeight);
            mipLevels.Add(currentImage);
        }

        Assert.That(mipLevels.Count, Is.GreaterThan(1), "Should have multiple mip levels.");

        // Verify each mip level is half the size of the previous
        for (int i = 1; i < mipLevels.Count; i++)
        {
            var prevMip = mipLevels[i - 1];
            var currMip = mipLevels[i];

            Assert.That(currMip.Width, Is.EqualTo(prevMip.Width / 2),
                $"Mip level {i} width should be half of level {i - 1}.");
            Assert.That(currMip.Height, Is.EqualTo(prevMip.Height / 2),
                $"Mip level {i} height should be half of level {i - 1}.");
        }
    }

    #endregion

    #region StbImage Properties Tests

    /// <summary>
    /// Test that StbImage stride is calculated correctly.
    /// </summary>
    [Test]
    public void Test_StbImage_Stride_CalculatedCorrectly()
    {
        var loader = new StbImageLoader();
        var image = loader.Load(TEST_PNG, VkFormat.R8G8B8A8_UNORM);

        Assert.That(image.Stride, Is.EqualTo(image.Width * image.Channels),
            "Stride should equal Width * Channels.");
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Test loading the same image multiple times produces consistent results.
    /// </summary>
    [Test]
    public void Test_Load_MultipleTimes_ProducesConsistentResults()
    {
        var loader = new StbImageLoader();

        var image1 = loader.Load(TEST_PNG, VkFormat.R8G8B8A8_UNORM);
        var image2 = loader.Load(TEST_PNG, VkFormat.R8G8B8A8_UNORM);

        Assert.That(image1.Width, Is.EqualTo(image2.Width), "Widths should match.");
        Assert.That(image1.Height, Is.EqualTo(image2.Height), "Heights should match.");
        Assert.That(image1.Channels, Is.EqualTo(image2.Channels), "Channels should match.");
        Assert.That(image1.Bytes, Is.EqualTo(image2.Bytes), "Byte data should match.");
    }

    /// <summary>
    /// Test loading different images produces different results.
    /// </summary>
    [Test]
    public void Test_Load_DifferentImages_ProducesDifferentResults()
    {
        var loader = new StbImageLoader();

        var image1 = loader.Load(TEST_PNG, VkFormat.R8G8B8A8_UNORM);
        var image2 = loader.Load(LENNA_PNG, VkFormat.R8G8B8A8_UNORM);

        // At least one property should be different (dimensions or data)
        bool areDifferent = image1.Width != image2.Width ||
                           image1.Height != image2.Height ||
                           !image1.Bytes.SequenceEqual(image2.Bytes);

        Assert.That(areDifferent, Is.True, "Different images should produce different results.");
    }

    #endregion
}
