using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using Paprika;


namespace PaprikaBenchmarks;


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
