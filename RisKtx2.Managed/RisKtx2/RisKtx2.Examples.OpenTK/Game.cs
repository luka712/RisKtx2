using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace RisKtx2.Examples.OpenTK
{
    public class Game : GameWindow
    {
        private int _vao;
        private int _vbo;
        private int _shaderProgram;
        private int _texture;

        private bool _supportsBC7;
        private bool _supportsASTC;
        private bool _supportsS3TC;
        private bool _supportsETC2;

        public Game(NativeWindowSettings settings)
            : base(GameWindowSettings.Default, settings) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            SupportedTextureFormats();

            GL.ClearColor(0.3f, 0.6f, 0.95f, 1f);

            SetupQuad();
            _shaderProgram = CreateShader();
            _texture = LoadTexture("test.ktx2");
        }

        private void SupportedTextureFormats()
        {
            int numExt = GL.GetInteger(GetPName.NumExtensions);

            var extensions = new HashSet<string>();
            for (int i = 0; i < numExt; i++)
            {
                extensions.Add(GL.GetString(StringNameIndexed.Extensions, i));
            }

            _supportsASTC =
              extensions.Contains("GL_KHR_texture_compression_astc_ldr") ||
              extensions.Contains("GL_KHR_texture_compression_astc_hdr");

            _supportsBC7 =
                extensions.Contains("GL_ARB_texture_compression_bptc");

            _supportsS3TC =
                extensions.Contains("GL_EXT_texture_compression_s3tc") ||
                extensions.Contains("GL_S3_s3tc");

            _supportsETC2 =
                extensions.Contains("GL_ARB_ES3_compatibility") ||  // desktop GL 4.x
                extensions.Contains("GL_OES_compressed_ETC2_RGB8_texture");
        }

        private void SetupQuad()
        {
            float[] vertices =
            {
                // pos        // uv
                -0.5f, -0.5f, 0f, 1f,
                 0.5f, -0.5f, 1f, 1f,
                 0.5f,  0.5f, 1f, 0f,
                -0.5f,  0.5f, 0f, 0f
            };

            uint[] indices = [
             0, 1, 2,
            2, 3, 0];

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // position
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // uv
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        private int CreateShader()
        {
            string vertexShaderSource = @"
#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

void main()
{
    gl_Position = vec4(aPosition, 0.0, 1.0);
    vTexCoord = aTexCoord;
}
";

            string fragmentShaderSource = @"
#version 330 core
in vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D texture0;

void main()
{
    FragColor = texture(texture0, vTexCoord);
}
";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }

        private int LoadTexture(string path)
        {
            using var ktx2Texture = new Ktx2Texture(path, KtxTextureCreateFlags.TEXTURE_CREATE_LOAD_IMAGE_DATA_BIT);

            // Determine target format and transcode if needed
            InternalFormat internalFormat;
            TextureFormatInfo formatInfo;

            if (ktx2Texture.NeedsTranscoding)
            {
                if (_supportsBC7)
                {
                    ktx2Texture.TranscodeBasis(KtxTranscodeFormat.BC7_RGBA);
                    internalFormat = InternalFormat.CompressedRgbaBptcUnorm;
                    formatInfo = ktx2Texture.GetTextureFormatInfo(KtxTranscodeFormat.BC7_RGBA);
                }
                else if (_supportsASTC)
                {
                    ktx2Texture.TranscodeBasis(KtxTranscodeFormat.ASTC_4X4_RGBA);
                    internalFormat = InternalFormat.CompressedRgbaAstc4X4;
                    formatInfo = ktx2Texture.GetTextureFormatInfo(KtxTranscodeFormat.ASTC_4X4_RGBA);
                }
                else if (_supportsETC2)
                {
                    ktx2Texture.TranscodeBasis(KtxTranscodeFormat.ETC2_RGBA);
                    internalFormat = InternalFormat.CompressedRgba8Etc2Eac;
                    formatInfo = ktx2Texture.GetTextureFormatInfo(KtxTranscodeFormat.ETC2_RGBA);
                }
                else if (_supportsS3TC)
                {
                    // fallback for older desktop GPUs
                    ktx2Texture.TranscodeBasis(KtxTranscodeFormat.BC3_RGBA);
                    internalFormat = InternalFormat.CompressedRgbaS3tcDxt5Ext;
                    formatInfo = ktx2Texture.GetTextureFormatInfo(KtxTranscodeFormat.BC3_RGBA);
                }
                else
                {
                    // 🚨 CRITICAL fallback
                    ktx2Texture.TranscodeBasis(KtxTranscodeFormat.RGBA32);
                    internalFormat = InternalFormat.Rgba8;
                    formatInfo = ktx2Texture.GetTextureFormatInfo(KtxTranscodeFormat.RGBA32);
                }
            }
            else
            {
                // Texture is already in a GPU-friendly format, map VkFormat to OpenGL InternalFormat
                internalFormat = MapVkFormatToGLInternalFormat(ktx2Texture);
                formatInfo = ktx2Texture.GetTextureFormatInfo(ktx2Texture.VkFormat);
            }

            int handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);

            var width = ktx2Texture.Width;
            var height = ktx2Texture.Height;

            if (formatInfo.BlockWidth > 1 || formatInfo.BlockHeight > 1)
            {
                // For block-compressed formats, dimensions must be aligned to block size
                width = (width + formatInfo.BlockWidth - 1) / formatInfo.BlockWidth * formatInfo.BlockWidth;
                height = (height + formatInfo.BlockHeight - 1) / formatInfo.BlockHeight * formatInfo.BlockHeight;
            }

            // Upload all mip levels
            uint numLevels = ktx2Texture.NumLevels;
            for (uint level = 0; level < numLevels; level++)
            {
                var data = ktx2Texture.GetTextureData(ktx2Texture.GetImageOffset(level, 0, 0));
                var size = ktx2Texture.GetImageSize(level);

                // Calculate dimensions for this mip level
                width = Math.Max(1, ktx2Texture.Width >> (int)level);
                height = Math.Max(1, ktx2Texture.Height >> (int)level);

                GL.CompressedTexImage2D(
                    TextureTarget.Texture2D,
                    (int)level,
                    internalFormat,
                    (int)width,
                    (int)height,
                    0,
                    (int)size,
                    data);
            }

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                numLevels > 1 ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return handle;
        }

        private InternalFormat MapVkFormatToGLInternalFormat(Ktx2Texture texture)
        {
            // Map common VkFormats to OpenGL InternalFormat
            // You'll need to add a property to Ktx2Texture to expose VkFormat
            return texture.VkFormat switch
            {
                VkFormat.R8G8B8A8_UNORM => InternalFormat.Rgba8,
                VkFormat.R8G8B8A8_SRGB => InternalFormat.Srgb8Alpha8,
                VkFormat.BC7_UNORM_BLOCK => InternalFormat.CompressedRgbaBptcUnorm,
                VkFormat.BC7_SRGB_BLOCK => InternalFormat.CompressedSrgbAlphaBptcUnorm,
                VkFormat.BC3_UNORM_BLOCK => InternalFormat.CompressedRgbaS3tcDxt5Ext,
                VkFormat.ASTC_4x4_UNORM_BLOCK => InternalFormat.CompressedRgbaAstc4X4,
                VkFormat.ETC2_R8G8B8A8_UNORM_BLOCK => InternalFormat.CompressedRgba8Etc2Eac,
                _ => throw new NotSupportedException($"Unsupported VkFormat: {texture.VkFormat}")
            };
        }
    }
}
