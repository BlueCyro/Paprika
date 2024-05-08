using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using Paprika;

// namespace Paprika.Benchmarks;



public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine($"Using vector size of {Vector<byte>.Count * 8} bits for rasterization");

        // if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
        // {
        //     Console.WriteLine($"Please choose a valid model");
        //     return;
        // }

        if (Avx.IsSupported)
            Console.WriteLine("Avx supported");
        
        if (Sse2.IsSupported)
            Console.WriteLine("Sse2 supported");

        if (Ssse3.IsSupported)
            Console.WriteLine("Ssse3 supported");

        if (Avx512F.IsSupported)
            Console.WriteLine("Avx512F supported");

        if (Fma.IsSupported && Vector<float>.Count == 8)
            Console.WriteLine("Taking fast x86 FMA path");


        Console.WriteLine($"Triangle byte width is: {Unsafe.SizeOf<Triangle>()}");
        Console.WriteLine($"Wide triangle width is: {Unsafe.SizeOf<TriangleWide>()}");
        unsafe
        {
            Console.WriteLine($"EdgesVectorized width is: {sizeof(EdgesVectorized)}");
        }
        
        // Model = Path.GetFullPath(args[0]);
        // Console.WriteLine(Model);
        BenchmarkRunner.Run<RenderBenchmark>();
    }
}



[SimpleJob(RunStrategy.Throughput, 25)]
// [SimpleJob(RunStrategy.Throughput)]
public class RenderBenchmark
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    BenchmarkOutput<PaprikaRenderer> benchmarkOutput;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    // public const string IMAGE_OUTPUT_FOLDER = "C:/Users/Cyro/Documents/Coding Stuff/Software Renderer/Paprika/Paprika.Benchmarks/OutputImage";


    public int Width = 1280;
    public int Height = 1024;

    // [Params(8, 16, 32, 64, 128, 256)]
    public int Alignment { get; set; } = 32;



    [GlobalSetup]
    public void Setup()
    {
        benchmarkOutput = new(Width, Height, Alignment);

        DumbUploader uploader = new();
        Console.WriteLine("Starting geometry uploader...");
        // Console.WriteLine($"Uploading {Program.Model}");
        uploader.Upload("C:/Users/Cyro/Documents/Coding Stuff/Software Renderer/Paprika/Model/tinobed.glb", Alignment);
        Console.WriteLine("Done!");
        
        benchmarkOutput.CurrentRenderer.MainCamera.Position = new(0.51144695f, 2.4718034f, 8.403356f);
        benchmarkOutput.CurrentRenderer.MainCamera.Rotation = new(-0.019815441f, -0.9137283f, 0.40335205f, -0.044888653f);
        benchmarkOutput.CurrentRenderer.DumpUploadGeometry(uploader.WideUploaded);
    }



    [Benchmark]
    public void Render() => benchmarkOutput.Update();



    // [GlobalCleanup]
    // public void Cleanup()
    // {
    //     Image<Rgba32> image = Image.LoadPixelData<Rgba32>(benchmarkOutput.PixelBufferBytes, Width, Height);

    //     // Console.WriteLine($"Working dir is: {Directory.GetCurrentDirectory()}");
    //     if (!Directory.Exists(IMAGE_OUTPUT_FOLDER))
    //         Directory.CreateDirectory(IMAGE_OUTPUT_FOLDER);
        
        
    //     string imageOutput = Path.Combine(IMAGE_OUTPUT_FOLDER, Path.GetFileNameWithoutExtension(ModelName) + ".png");
    //     Console.WriteLine($"Saving benchmarked image data to: {imageOutput}");
    //     FileStream output = new(imageOutput, FileMode.OpenOrCreate, FileAccess.Write);
    //     PngEncoder png = new()
    //     {
    //         ColorType = PngColorType.Rgb,
    //         TransparentColorMode = PngTransparentColorMode.Preserve
    //     };

    //     image.Save(output, png);
    // }
}



public class BenchmarkOutput<T> : IRenderOutput<T> where T: IRenderer
{
    public RenderBuffer<int> PixelBuffer => bufferSwap ? pixelBuffer2 : pixelBuffer1;
    public Span<byte> PixelBufferBytes => MemoryMarshal.Cast<int, byte>(PixelBuffer.Buffer.Span);
    public RenderBuffer<float> ZBuffer { get; private set; }
    public RenderBuffer<int> pixelBuffer1;
    public RenderBuffer<int> pixelBuffer2;
    
    
    public Size2D FrameBufferSize { get; private set; }
    public T CurrentRenderer { get; private set; }
    public IRenderer CurrentIRenderer => CurrentRenderer;


    private bool bufferSwap = false;



    public BenchmarkOutput(int width = 1280, int height = 1024, int alignment = 32)
    {
        Console.WriteLine($"Setting up bench for: {width}, {height}");
        pixelBuffer1 = new(width, height, alignment);
        pixelBuffer2 = new(width, height, alignment);
        ZBuffer = new(width, height, alignment);

        CurrentRenderer = (T?)Activator.CreateInstance(typeof(T), [ this ]) ?? throw new Exception($"Failed to initialize instance of {typeof(T)}!");
    }



    public void StartRender() => throw new NotImplementedException("Stub!");



    public void Update()
    {
        PixelBuffer.Buffer.Clear();
        ZBuffer.Buffer.Fill(float.MaxValue);

        CurrentRenderer.RenderFrame();
        bufferSwap = !bufferSwap;
    }
}