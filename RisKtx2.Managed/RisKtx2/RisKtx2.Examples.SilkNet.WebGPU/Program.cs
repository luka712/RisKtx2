using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.Dawn;
using Silk.NET.Windowing;

namespace RisKtx2.Examples.SilkNet;

public unsafe class Program
{
    private const int WINDOW_WIDTH = 800;
    private const int WINDOW_HEIGHT = 600;
    private const string WINDOW_TITLE = "RisKtx2 WebGPU Example";
    private const string TEST_TEXTURE_PATH = "test.ktx2";

    private IWindow _window = null!;
    private WebGPU _wgpu = null!;
    private Instance* _instance;
    private Surface* _surface;
    private Adapter* _adapter;
    private Device* _device;
    private Queue* _queue;
    private SwapChain* _swapChain;

    private RenderPipeline* _pipeline;
    private BindGroup* _bindGroup;
    private Silk.NET.WebGPU.Texture* _texture;
    private TextureView* _textureView;
    private Sampler* _sampler;
    private Silk.NET.WebGPU.Buffer* _vertexBuffer;
    private Silk.NET.WebGPU.Buffer* _indexBuffer;

    private uint _indexCount;

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
            API = GraphicsAPI.None, // We'll use WebGPU manually
            VSync = true
        };

        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Closing += OnClosing;

        _window.Run();
    }

    private unsafe void OnLoad()
    {
        // Initialize WebGPU
        _wgpu = WebGPU.GetApi();

        // Create instance
        var instanceDescriptor = new InstanceDescriptor();
        _instance = _wgpu.CreateInstance(&instanceDescriptor);

        if (_instance == null)
        {
            throw new Exception("Failed to create WebGPU instance");
        }

        // Create surface
        _surface = _window.CreateWebGPUSurface(_wgpu, _instance);

        // Request adapter
        _adapter = RequestAdapter();

        // Request device
        _device = RequestDevice();

        // Get queue
        _queue = _wgpu.DeviceGetQueue(_device);

        // Configure swap chain
        ConfigureSurface();

        // Load texture and create GPU resources
        LoadTexture(TEST_TEXTURE_PATH);
        CreatePipeline();
        CreateGeometry();
    }

    private unsafe Adapter* RequestAdapter()
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

    private unsafe Device* RequestDevice()
    {
        Device* device = null;
        var deviceDescriptor = new DeviceDescriptor();
        var features = stackalloc FeatureName[1];
        features[0] = FeatureName.TextureCompressionBC;
        deviceDescriptor.RequiredFeatures = features;
        deviceDescriptor.RequiredFeatureCount = 1;

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

    private unsafe void ConfigureSurface()
    {
        var surfaceFormat = _wgpu.SurfaceGetPreferredFormat(_surface, _adapter);
        var surfaceConfiguration = new SurfaceConfiguration
        {
            Usage = TextureUsage.RenderAttachment,
            Format = surfaceFormat,
            Width = (uint)_window.Size.X,
            Height = (uint)_window.Size.Y,
            PresentMode = PresentMode.Fifo,
            Device = _device
        };

        _wgpu.SurfaceConfigure(_surface, in surfaceConfiguration);
    }

    private unsafe void LoadTexture(string path)
    {
        using var ktx2Texture = new Ktx2Texture(path);

        // Transcode to BC7 if needed (widely supported compressed format)
        if (ktx2Texture.NeedsTranscoding)
        {
            ktx2Texture.TranscodeBasis(KtxTranscodeFormat.KTX_TTF_BC7_RGBA);
        }

        // Create WebGPU texture
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

        // Upload texture data level by level, ensuring proper alignment for compressed formats
        for (uint level = 0; level < ktx2Texture.NumLevels; level++)
        {
            var data = ktx2Texture.GetTextureData(ktx2Texture.GetImageOffset(level, 0, 0));
            var size = ktx2Texture.GetImageSize(level);

            uint logicalWidth = Math.Max(1, ktx2Texture.Width >> (int)level);
            uint logicalHeight = Math.Max(1, ktx2Texture.Height >> (int)level);

            // 1. Round up to the nearest block (4)
            uint physicalWidth = (logicalWidth + 3) & ~3u;
            uint physicalHeight = (logicalHeight + 3) & ~3u;

            uint bytesPerRow = physicalWidth * 4; // 4 bytes per pixel for BC7 (adjust if using a different format)

            var imageCopyTexture = new ImageCopyTexture
            {
                Texture = _texture,
                MipLevel = level,
                Origin = new Origin3D { X = 0, Y = 0, Z = 0 },
                Aspect = TextureAspect.All
            };

            var textureDataLayout = new TextureDataLayout
            {
                Offset = 0,
                BytesPerRow = bytesPerRow,
                // 2. RowsPerImage must be the physical height (aligned to block size)
                RowsPerImage = physicalHeight,
            };

            var extent = new Extent3D
            {
                // 3. Use the physical (aligned) dimensions here
                Width = physicalWidth,
                Height = physicalHeight,
                DepthOrArrayLayers = 1
            };

            _wgpu.QueueWriteTexture(_queue, &imageCopyTexture, (void*)data, (nuint)size, &textureDataLayout, &extent);
        }

        // Create texture view
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

        // Create sampler
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

    private unsafe void CreatePipeline()
    {
        // Shader code
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

        // Create shader module
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

        // Create bind group layout
        var bindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[2];

        // Sampler
        bindGroupLayoutEntries[0] = new BindGroupLayoutEntry
        {
            Binding = 0,
            Visibility = ShaderStage.Fragment,
            Sampler = new SamplerBindingLayout { Type = SamplerBindingType.Filtering }
        };

        // Texture
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

        var pipelineLayoutDescriptor = new PipelineLayoutDescriptor
        {
            BindGroupLayoutCount = 1,
            BindGroupLayouts = &bindGroupLayout
        };

        // Create pipeline layout
        var pipelineLayout = _wgpu.DeviceCreatePipelineLayout(_device, in pipelineLayoutDescriptor);

        // Vertex attributes
        var vertexAttributes = stackalloc VertexAttribute[2];
        vertexAttributes[0] = new VertexAttribute
        {
            Format = VertexFormat.Float32x2,
            Offset = 0,
            ShaderLocation = 0
        };
        vertexAttributes[1] = new VertexAttribute
        {
            Format = VertexFormat.Float32x2,
            Offset = 8,
            ShaderLocation = 1
        };

        var vertexBufferLayout = new VertexBufferLayout
        {
            ArrayStride = 16,
            StepMode = VertexStepMode.Vertex,
            AttributeCount = 2,
            Attributes = vertexAttributes
        };

        // Create pipeline
        var swapChainFormat = _wgpu.SurfaceGetPreferredFormat(_surface, _adapter);

        var vsEntryPtr = SilkMarshal.StringToPtr("vs_main");
        var fsEntryPtr = SilkMarshal.StringToPtr("fs_main");

        var colorTargetState = stackalloc ColorTargetState[1];
        colorTargetState[0] = new ColorTargetState
        {
            Format = swapChainFormat,
            WriteMask = ColorWriteMask.All,
            Blend = null
        };

        var fragmentState = stackalloc FragmentState[1];
        fragmentState[0] = new FragmentState
        {
            Module = shaderModule,
            EntryPoint = (byte*)fsEntryPtr,
            TargetCount = 1,
            Targets = colorTargetState
        };

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

        // Create bind group
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

    private unsafe void CreateGeometry()
    {
        // Quad vertices (position + texCoord)
        float[] vertices =
        [
            -0.5f, -0.5f,   0f, 0f,
             0.5f, -0.5f,   1f, 0f,
             0.5f,  0.5f,   1f, 1f,
            -0.5f,  0.5f,   0f, 1f
        ];

        ushort[] indices = [0, 1, 2, 2, 3, 0];
        _indexCount = (uint)indices.Length;

        // Create vertex buffer
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

        // Create index buffer
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

    private unsafe void OnRender(double deltaTime)
    {
        //  SurfaceTexture* nextTexture = null;
        SurfaceTexture nextTexture = new();
        _wgpu.SurfaceGetCurrentTexture(_surface, ref nextTexture);

        var nextTextureView = _wgpu.TextureCreateView(nextTexture.Texture, null);

        var commandEncoder = _wgpu.DeviceCreateCommandEncoder(_device, null);

        var colorAttachment = new RenderPassColorAttachment
        {
            View = nextTextureView,
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            ClearValue = new Color { R = 0.1, G = 0.1, B = 0.15, A = 1.0 }
        };

        var renderPassDescriptor = new RenderPassDescriptor
        {
            ColorAttachmentCount = 1,
            ColorAttachments = &colorAttachment
        };

        var renderPass = _wgpu.CommandEncoderBeginRenderPass(commandEncoder, &renderPassDescriptor);

        _wgpu.RenderPassEncoderSetPipeline(renderPass, _pipeline);
        _wgpu.RenderPassEncoderSetBindGroup(renderPass, 0, _bindGroup, 0, null);
        _wgpu.RenderPassEncoderSetVertexBuffer(renderPass, 0, _vertexBuffer, 0, ulong.MaxValue);
        _wgpu.RenderPassEncoderSetIndexBuffer(renderPass, _indexBuffer, IndexFormat.Uint16, 0, ulong.MaxValue);
        _wgpu.RenderPassEncoderDrawIndexed(renderPass, _indexCount, 1, 0, 0, 0);

        _wgpu.RenderPassEncoderEnd(renderPass);

        var commandBuffer = _wgpu.CommandEncoderFinish(commandEncoder, null);
        _wgpu.QueueSubmit(_queue, 1, &commandBuffer);

        _wgpu.SurfacePresent(_surface);
    }

    private unsafe void OnClosing()
    {
        // Cleanup resources
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
    /// Aligns a value to the specified alignment.
    /// </summary>
    private static uint AlignTo(uint value, uint alignment)
    {
        return (value + alignment - 1) & ~(alignment - 1);
    }
}