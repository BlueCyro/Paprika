using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Paprika;

namespace PaprikaBenchmarks;

[SimpleJob(RunStrategy.Throughput, 25)]
// [SimpleJob(RunStrategy.Throughput)]
public class RenderBenchmark
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    BenchmarkOutput benchmarkOutput;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    // public const string IMAGE_OUTPUT_FOLDER = "C:/Users/Cyro/Documents/Coding Stuff/Software Renderer/Paprika/Paprika.Benchmarks/OutputImage";


    public int Width = 1280;
    public int Height = 1024;

    [Params(8, 16, 32, 64, 128, 256)]
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
        
        benchmarkOutput.MainCamera.Position = new(0.51144695f, 2.4718034f, 8.403356f);
        benchmarkOutput.MainCamera.Rotation = new(-0.019815441f, -0.9137283f, 0.40335205f, -0.044888653f);
        benchmarkOutput.MainCamera.FOV = 60f;
        GeometryHolder<TriangleWide>.UploadGeometry(uploader.WideUploaded.Span, uploader.WideUploaded.Length, Alignment);
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
