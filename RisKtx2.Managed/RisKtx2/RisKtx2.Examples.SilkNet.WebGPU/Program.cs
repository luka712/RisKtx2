using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.Dawn;
using Silk.NET.Windowing;

namespace RisKtx2.Examples.SilkNet;

/// <summary>
/// WebGPU example demonstrating KTX2 texture loading and rendering.
/// Shows how to load, transcode, and upload compressed textures to the GPU.
/// </summary>
public unsafe class Program
{
    #region Constants

    private const int WINDOW_WIDTH = 800;
    private const int WINDOW_HEIGHT = 600;
    private const string WINDOW_TITLE = "RisKtx2 WebGPU Example";
    private const string TEST_TEXTURE_PATH = "test.ktx2";

    #endregion

    #region Fields

    // Window and WebGPU core
    private IWindow _window = null!;
    private WebGPU _wgpu = null!;

    // WebGPU objects
    private Instance* _instance;
    private Surface* _surface;
    private Adapter* _adapter;
    private Device* _device;
    private Queue* _queue;
    private SwapChain* _swapChain;

    // Rendering resources
    private RenderPipeline* _pipeline;
    private BindGroup* _bindGroup;
    private Silk.NET.WebGPU.Texture* _texture;
    private TextureView* _textureView;
    private Sampler* _sampler;
    private Silk.NET.WebGPU.Buffer* _vertexBuffer;
    private Silk.NET.WebGPU.Buffer* _indexBuffer;

    private uint _indexCount;

    private bool _bcFormatsSupported;
    private bool _astcFormatsSupported;
    private bool _etc2FormatsSupported;

    #endregion

    #region Entry Point

    public static void Main(string[] args)
    {
        var program = new Program();
        program.Run();
    }

    public void Run()
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(WINDOW_WIDTH, WINDOW_HEIGHT),
            Title = WINDOW_TITLE,
            API = GraphicsAPI.None, // Using WebGPU manually
            VSync = true
        };

        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Closing += OnClosing;

        _window.Run();
    }

    #endregion

    #region Initialization

    private void OnLoad()
    {
        // Initialize WebGPU API
        _wgpu = WebGPU.GetApi();

        // Create WebGPU instance
        var instanceDescriptor = new InstanceDescriptor();
        _instance = _wgpu.CreateInstance(&instanceDescriptor);

        if (_instance == null)
        {
            throw new Exception("Failed to create WebGPU instance");
        }

        // Create window surface for rendering
        _surface = _window.CreateWebGPUSurface(_wgpu, _instance);

        // Request GPU adapter
        _adapter = RequestAdapter();

        // Request logical device with required features
        _device = RequestDevice();

        // Get command queue
        _queue = _wgpu.DeviceGetQueue(_device);

        // Configure surface for presentation
        ConfigureSurface();

        // Load and prepare resources
        LoadTexture(TEST_TEXTURE_PATH);
        CreatePipeline();
        CreateGeometry();
    }

    /// <summary>
    /// Requests a GPU adapter from the WebGPU instance.
    /// Prefers high-performance adapters compatible with our surface.
    /// </summary>
    private Adapter* RequestAdapter()
    {
        Adapter* adapter = null;
        var options = new RequestAdapterOptions
        {
            CompatibleSurface = _surface,
            PowerPreference = PowerPreference.HighPerformance,
        };

        _wgpu.InstanceRequestAdapter(_instance, &options, new PfnRequestAdapterCallback((status, adapterHandle, message, _) =>
        {
            if (status == RequestAdapterStatus.Success)
            {
                adapter = adapterHandle;
            }
            else
            {
                throw new Exception($"Failed to request adapter: {SilkMarshal.PtrToString((nint)message)}");
            }
        }), null);

        if (adapter == null)
        {
            throw new Exception("Failed to get adapter");
        }

        return adapter;
    }

    /// <summary>
    /// Requests a logical device from the adapter.
    /// Requires BC texture compression support (common on desktop GPUs).
    /// </summary>
    private Device* RequestDevice()
    {
        Device* device = null;
        var deviceDescriptor = new DeviceDescriptor();

        int featuresCount = 0;

        // Check if adapter supports required BC texture compression feature
        if (_wgpu.AdapterHasFeature(_adapter, FeatureName.TextureCompressionBC))
        {
            _bcFormatsSupported = true;
            featuresCount++;
        }

        if (_wgpu.AdapterHasFeature(_adapter, FeatureName.TextureCompressionAstc))
        {
            _astcFormatsSupported = true;
            featuresCount++;
        }

        if (_wgpu.AdapterHasFeature(_adapter, FeatureName.TextureCompressionEtc2))
        {
            _etc2FormatsSupported = true;
            featuresCount++;
        }

        // Request BC compression feature
        var features = stackalloc FeatureName[featuresCount];
        var index = 0;
        if (_bcFormatsSupported)
        {
            features[index++] = FeatureName.TextureCompressionBC;
        }
        if (_astcFormatsSupported)
        {
            features[index++] = FeatureName.TextureCompressionAstc;
        }
        if (_etc2FormatsSupported)
        {
            features[index++] = FeatureName.TextureCompressionEtc2;
        }

        deviceDescriptor.RequiredFeatures = features;
        deviceDescriptor.RequiredFeatureCount = (nuint) featuresCount;

        _wgpu.AdapterRequestDevice(_adapter, &deviceDescriptor, new PfnRequestDeviceCallback((status, deviceHandle, message, _) =>
        {
            if (status == RequestDeviceStatus.Success)
            {
                device = deviceHandle;
            }
            else
            {
                throw new Exception($"Failed to request device: {SilkMarshal.PtrToString((nint)message)}");
            }
        }), null);

        if (device == null)
        {
            throw new Exception("Failed to get device");
        }

        return device;
    }

    /// <summary>
    /// Configures the surface for presentation.
    /// Sets up swap chain with preferred format and VSync.
    /// </summary>
    private void ConfigureSurface()
    {
        var surfaceFormat = _wgpu.SurfaceGetPreferredFormat(_surface, _adapter);
        var surfaceConfiguration = new SurfaceConfiguration
        {
            Usage = TextureUsage.RenderAttachment,
            Format = surfaceFormat,
            Width = (uint)_window.Size.X,
            Height = (uint)_window.Size.Y,
            PresentMode = PresentMode.Fifo, // VSync enabled
            Device = _device
        };

        _wgpu.SurfaceConfigure(_surface, in surfaceConfiguration);
    }

    #endregion

    #region Texture Loading

    /// <summary>
    /// Loads a KTX2 texture file, transcodes if needed, and uploads to GPU.
    /// Handles multiple mip levels and compressed formats.
    /// </summary>
    private void LoadTexture(string path)
    {
        using var ktx2Texture = new Ktx2Texture(path);

        // Transcode Basis Universal to BC7 if needed
        // BC7 provides high quality and is widely supported on desktop
        if (ktx2Texture.NeedsTranscoding)
        {
            if (_bcFormatsSupported)
            {
                ktx2Texture.TranscodeBasis(KtxTranscodeFormat.BC7_RGBA);
            }
            else if (_astcFormatsSupported)
            {
                ktx2Texture.TranscodeBasis(KtxTranscodeFormat.ASTC_4X4_RGBA);
            }
            else if (_etc2FormatsSupported)
            {
                ktx2Texture.TranscodeBasis(KtxTranscodeFormat.ETC2_RGBA);
            }
            else
            {
                throw new Exception("No supported compressed texture format available for transcoding");
            }
        }

        var formatInfo = ktx2Texture.GetTextureFormatInfo(KtxTranscodeFormat.BC7_RGBA);

        // Create GPU texture descriptor
        var textureDescriptor = new TextureDescriptor
        {
            Size = new Extent3D
            {
                Width = ktx2Texture.Width,
                Height = ktx2Texture.Height,
                DepthOrArrayLayers = 1
            },
            MipLevelCount = ktx2Texture.NumLevels,
            SampleCount = 1,
            Dimension = TextureDimension.Dimension2D,
            Format = MapVkFormatToWebGPU(ktx2Texture.VkFormat),
            Usage = TextureUsage.TextureBinding | TextureUsage.CopyDst
        };

        _texture = _wgpu.DeviceCreateTexture(_device, &textureDescriptor);

        // Upload each mip level
        // For compressed formats, dimensions must be aligned to block size (4x4 for BC7)
        for (uint level = 0; level < ktx2Texture.NumLevels; level++)
        {
            // Get texture data for this mip level
            var data = ktx2Texture.GetTextureData(ktx2Texture.GetImageOffset(level, 0, 0));
            var size = ktx2Texture.GetImageSize(level);

            // Calculate mip level dimensions
            uint logicalWidth = Math.Max(1, ktx2Texture.Width >> (int)level);
            uint logicalHeight = Math.Max(1, ktx2Texture.Height >> (int)level);

            // Round up to nearest 4x4 block for compressed formats
            uint physicalWidth = (logicalWidth + formatInfo.BlockWidth - 1) & ~(formatInfo.BlockWidth - 1);
            uint physicalHeight = (logicalHeight + formatInfo.BlockHeight - 1) & ~(formatInfo.BlockHeight - 1);

            // BC7 format: 16 bytes per 4x4 block
            uint bytesPerRow = physicalWidth * 4;

            // Define destination in texture
            var imageCopyTexture = new ImageCopyTexture
            {
                Texture = _texture,
                MipLevel = level,
                Origin = new Origin3D { X = 0, Y = 0, Z = 0 },
                Aspect = TextureAspect.All
            };

            // Define source data layout
            var textureDataLayout = new TextureDataLayout
            {
                Offset = 0,
                BytesPerRow = bytesPerRow,
                RowsPerImage = physicalHeight, // Must be block-aligned
            };

            // Define copy extent (must be block-aligned)
            var extent = new Extent3D
            {
                Width = physicalWidth,
                Height = physicalHeight,
                DepthOrArrayLayers = 1
            };

            // Upload texture data to GPU
            _wgpu.QueueWriteTexture(_queue, &imageCopyTexture, (void*)data, (nuint)size, &textureDataLayout, &extent);
        }

        // Create texture view for shader access
        var viewDescriptor = new TextureViewDescriptor
        {
            Format = textureDescriptor.Format,
            Dimension = TextureViewDimension.Dimension2D,
            BaseMipLevel = 0,
            MipLevelCount = ktx2Texture.NumLevels,
            BaseArrayLayer = 0,
            ArrayLayerCount = 1,
            Aspect = TextureAspect.All
        };

        _textureView = _wgpu.TextureCreateView(_texture, &viewDescriptor);

        // Create sampler with linear filtering and mipmapping
        var samplerDescriptor = new SamplerDescriptor
        {
            AddressModeU = AddressMode.ClampToEdge,
            AddressModeV = AddressMode.ClampToEdge,
            AddressModeW = AddressMode.ClampToEdge,
            MagFilter = FilterMode.Linear,
            MinFilter = FilterMode.Linear,
            MipmapFilter = MipmapFilterMode.Linear,
            MaxAnisotropy = 1
        };

        _sampler = _wgpu.DeviceCreateSampler(_device, &samplerDescriptor);
    }

    #endregion

    #region Pipeline Creation

    /// <summary>
    /// Creates the render pipeline with shaders, vertex layout, and bind groups.
    /// </summary>
    private void CreatePipeline()
    {
        // WGSL shader code for textured quad rendering
        const string shaderCode = @"
struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) texCoord: vec2<f32>,
}

@vertex
fn vs_main(@location(0) position: vec2<f32>, @location(1) texCoord: vec2<f32>) -> VertexOutput {
    var output: VertexOutput;
    output.position = vec4<f32>(position, 0.0, 1.0);
    output.texCoord = texCoord;
    return output;
}

@group(0) @binding(0) var texSampler: sampler;
@group(0) @binding(1) var tex: texture_2d<f32>;

@fragment
fn fs_main(input: VertexOutput) -> @location(0) vec4<f32> {
    return textureSample(tex, texSampler, input.texCoord);
}
";

        // Create shader module from WGSL code
        var shaderCodePtr = SilkMarshal.StringToPtr(shaderCode);
        var shaderModuleDescriptor = new ShaderModuleWGSLDescriptor
        {
            Chain = new ChainedStruct { SType = SType.ShaderModuleWgsldescriptor },
            Code = (byte*)shaderCodePtr
        };

        var shaderModule = _wgpu.DeviceCreateShaderModule(_device, new ShaderModuleDescriptor
        {
            NextInChain = (ChainedStruct*)&shaderModuleDescriptor
        });

        SilkMarshal.Free(shaderCodePtr);

        // Create bind group layout for sampler and texture
        var bindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[2];

        // Binding 0: Sampler
        bindGroupLayoutEntries[0] = new BindGroupLayoutEntry
        {
            Binding = 0,
            Visibility = ShaderStage.Fragment,
            Sampler = new SamplerBindingLayout { Type = SamplerBindingType.Filtering }
        };

        // Binding 1: Texture
        bindGroupLayoutEntries[1] = new BindGroupLayoutEntry
        {
            Binding = 1,
            Visibility = ShaderStage.Fragment,
            Texture = new TextureBindingLayout
            {
                SampleType = TextureSampleType.Float,
                ViewDimension = TextureViewDimension.Dimension2D
            }
        };

        var bindGroupLayoutDescriptor = new BindGroupLayoutDescriptor
        {
            EntryCount = 2,
            Entries = bindGroupLayoutEntries
        };

        var bindGroupLayout = _wgpu.DeviceCreateBindGroupLayout(_device, in bindGroupLayoutDescriptor);

        // Create pipeline layout
        var pipelineLayoutDescriptor = new PipelineLayoutDescriptor
        {
            BindGroupLayoutCount = 1,
            BindGroupLayouts = &bindGroupLayout
        };

        var pipelineLayout = _wgpu.DeviceCreatePipelineLayout(_device, in pipelineLayoutDescriptor);

        // Define vertex attributes: position (vec2) + texCoord (vec2)
        var vertexAttributes = stackalloc VertexAttribute[2];
        vertexAttributes[0] = new VertexAttribute
        {
            Format = VertexFormat.Float32x2,
            Offset = 0,
            ShaderLocation = 0 // position
        };
        vertexAttributes[1] = new VertexAttribute
        {
            Format = VertexFormat.Float32x2,
            Offset = 8, // 2 floats * 4 bytes
            ShaderLocation = 1 // texCoord
        };

        var vertexBufferLayout = new VertexBufferLayout
        {
            ArrayStride = 16, // 4 floats * 4 bytes
            StepMode = VertexStepMode.Vertex,
            AttributeCount = 2,
            Attributes = vertexAttributes
        };

        // Get surface format for render target
        var swapChainFormat = _wgpu.SurfaceGetPreferredFormat(_surface, _adapter);

        // Marshal shader entry point names
        var vsEntryPtr = SilkMarshal.StringToPtr("vs_main");
        var fsEntryPtr = SilkMarshal.StringToPtr("fs_main");

        // Define blend state
        var blendState = stackalloc BlendState[1];
        blendState[0] = new BlendState
        {
            Color = new BlendComponent
            {
                SrcFactor = BlendFactor.SrcAlpha,
                DstFactor = BlendFactor.OneMinusSrcAlpha,
                Operation = BlendOperation.Add
            },
            Alpha = new BlendComponent
            {
                SrcFactor = BlendFactor.One,
                DstFactor = BlendFactor.Zero,
                Operation = BlendOperation.Add
            }
        };

        // Define color target state
        var colorTargetState = stackalloc ColorTargetState[1];
        colorTargetState[0] = new ColorTargetState
        {
            Format = swapChainFormat,
            WriteMask = ColorWriteMask.All,
            Blend = blendState
        };

        // Define fragment state
        var fragmentState = stackalloc FragmentState[1];
        fragmentState[0] = new FragmentState
        {
            Module = shaderModule,
            EntryPoint = (byte*)fsEntryPtr,
            TargetCount = 1,
            Targets = colorTargetState
        };

        // Create render pipeline
        var pipelineDescriptor = new RenderPipelineDescriptor
        {
            Layout = pipelineLayout,
            Vertex = new VertexState
            {
                Module = shaderModule,
                EntryPoint = (byte*)vsEntryPtr,
                BufferCount = 1,
                Buffers = &vertexBufferLayout
            },
            Primitive = new PrimitiveState
            {
                Topology = PrimitiveTopology.TriangleList,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.None
            },
            Multisample = new MultisampleState
            {
                Count = 1,
                Mask = ~0u
            },
            Fragment = fragmentState
        };

        _pipeline = _wgpu.DeviceCreateRenderPipeline(_device, &pipelineDescriptor);

        SilkMarshal.Free(vsEntryPtr);
        SilkMarshal.Free(fsEntryPtr);

        // Create bind group with actual sampler and texture view
        var bindGroupEntries = stackalloc BindGroupEntry[2];
        bindGroupEntries[0] = new BindGroupEntry
        {
            Binding = 0,
            Sampler = _sampler
        };
        bindGroupEntries[1] = new BindGroupEntry
        {
            Binding = 1,
            TextureView = _textureView
        };

        var bindGroupDescriptor = new BindGroupDescriptor
        {
            Layout = bindGroupLayout,
            EntryCount = 2,
            Entries = bindGroupEntries
        };

        _bindGroup = _wgpu.DeviceCreateBindGroup(_device, bindGroupDescriptor);
    }

    #endregion

    #region Geometry Creation

    /// <summary>
    /// Creates vertex and index buffers for a textured quad.
    /// Quad is centered at origin with size 1x1 in NDC space.
    /// </summary>
    private void CreateGeometry()
    {
        // Quad vertices: position (xy) + texCoord (uv)
        float[] vertices =
        [
            -0.5f, -0.5f,   0f, 1f,
             0.5f, -0.5f,   1f, 1f,
             0.5f,  0.5f,   1f, 0f,
            -0.5f,  0.5f,   0f, 0f
        ];

        // Two triangles forming a quad
        ushort[] indices = [0, 1, 2, 2, 3, 0];
        _indexCount = (uint)indices.Length;

        // Create and upload vertex buffer
        fixed (float* verticesPtr = vertices)
        {
            var vertexBufferDescriptor = new BufferDescriptor
            {
                Size = (ulong)(vertices.Length * sizeof(float)),
                Usage = BufferUsage.Vertex | BufferUsage.CopyDst,
                MappedAtCreation = false
            };

            _vertexBuffer = _wgpu.DeviceCreateBuffer(_device, &vertexBufferDescriptor);
            _wgpu.QueueWriteBuffer(_queue, _vertexBuffer, 0, verticesPtr, (nuint)vertexBufferDescriptor.Size);
        }

        // Create and upload index buffer
        fixed (ushort* indicesPtr = indices)
        {
            var indexBufferDescriptor = new BufferDescriptor
            {
                Size = (ulong)(indices.Length * sizeof(ushort)),
                Usage = BufferUsage.Index | BufferUsage.CopyDst,
                MappedAtCreation = false
            };

            _indexBuffer = _wgpu.DeviceCreateBuffer(_device, &indexBufferDescriptor);
            _wgpu.QueueWriteBuffer(_queue, _indexBuffer, 0, indicesPtr, (nuint)indexBufferDescriptor.Size);
        }
    }

    #endregion

    #region Rendering

    /// <summary>
    /// Renders a frame with the textured quad.
    /// Called every frame by the window.
    /// </summary>
    private void OnRender(double deltaTime)
    {
        // Get current frame's texture from swap chain
        SurfaceTexture nextTexture = new();
        _wgpu.SurfaceGetCurrentTexture(_surface, ref nextTexture);

        // Create texture view for rendering
        var nextTextureView = _wgpu.TextureCreateView(nextTexture.Texture, null);

        // Create command encoder for recording GPU commands
        var commandEncoder = _wgpu.DeviceCreateCommandEncoder(_device, null);

        // Begin render pass with clear color
        var colorAttachment = new RenderPassColorAttachment
        {
            View = nextTextureView,
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            ClearValue = new Color { R = 0.95, G = 0.5, B = 0.6, A = 1.0 }
        };

        var renderPassDescriptor = new RenderPassDescriptor
        {
            ColorAttachmentCount = 1,
            ColorAttachments = &colorAttachment
        };

        var renderPass = _wgpu.CommandEncoderBeginRenderPass(commandEncoder, &renderPassDescriptor);

        // Set pipeline and resources
        _wgpu.RenderPassEncoderSetPipeline(renderPass, _pipeline);
        _wgpu.RenderPassEncoderSetBindGroup(renderPass, 0, _bindGroup, 0, null);
        _wgpu.RenderPassEncoderSetVertexBuffer(renderPass, 0, _vertexBuffer, 0, ulong.MaxValue);
        _wgpu.RenderPassEncoderSetIndexBuffer(renderPass, _indexBuffer, IndexFormat.Uint16, 0, ulong.MaxValue);

        // Draw indexed quad (6 indices, 2 triangles)
        _wgpu.RenderPassEncoderDrawIndexed(renderPass, _indexCount, 1, 0, 0, 0);

        _wgpu.RenderPassEncoderEnd(renderPass);

        // Finish encoding and submit to GPU
        var commandBuffer = _wgpu.CommandEncoderFinish(commandEncoder, null);
        _wgpu.QueueSubmit(_queue, 1, &commandBuffer);

        // Present the rendered frame
        _wgpu.SurfacePresent(_surface);
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Releases all WebGPU resources.
    /// Called when the window is closing.
    /// </summary>
    private void OnClosing()
    {
        // Release resources in reverse order of creation
        if (_vertexBuffer != null) _wgpu.BufferRelease(_vertexBuffer);
        if (_indexBuffer != null) _wgpu.BufferRelease(_indexBuffer);
        if (_bindGroup != null) _wgpu.BindGroupRelease(_bindGroup);
        if (_sampler != null) _wgpu.SamplerRelease(_sampler);
        if (_textureView != null) _wgpu.TextureViewRelease(_textureView);
        if (_texture != null) _wgpu.TextureRelease(_texture);
        if (_pipeline != null) _wgpu.RenderPipelineRelease(_pipeline);
        if (_queue != null) _wgpu.QueueRelease(_queue);
        if (_device != null) _wgpu.DeviceRelease(_device);
        if (_adapter != null) _wgpu.AdapterRelease(_adapter);
        if (_surface != null) _wgpu.SurfaceRelease(_surface);
        if (_instance != null) _wgpu.InstanceRelease(_instance);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Maps Vulkan texture formats to WebGPU texture formats.
    /// </summary>
    private static TextureFormat MapVkFormatToWebGPU(VkFormat format)
    {
        return format switch
        {
            VkFormat.R8G8B8A8_UNORM => TextureFormat.Rgba8Unorm,
            VkFormat.R8G8B8A8_SRGB => TextureFormat.Rgba8UnormSrgb,
            VkFormat.BC7_UNORM_BLOCK => TextureFormat.BC7RgbaUnorm,
            VkFormat.BC7_SRGB_BLOCK => TextureFormat.BC7RgbaUnormSrgb,
            VkFormat.BC3_UNORM_BLOCK => TextureFormat.BC3RgbaUnorm,
            VkFormat.ASTC_4x4_UNORM_BLOCK => TextureFormat.Astc4x4Unorm,
            VkFormat.ETC2_R8G8B8A8_UNORM_BLOCK => TextureFormat.Etc2Rgba8Unorm,
            _ => throw new NotSupportedException($"Unsupported VkFormat: {format}")
        };
    }

    /// <summary>
    /// Aligns a value to the specified alignment (power of 2).
    /// Used for aligning texture dimensions to compressed format block sizes.
    /// </summary>
    private static uint AlignTo(uint value, uint alignment)
    {
        return (value + alignment - 1) & ~(alignment - 1);
    }

    #endregion
}