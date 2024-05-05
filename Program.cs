using System.Numerics;
using System.Diagnostics;
using SharpGLTF.Schema2;
using SharpGLTF.Geometry;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;


namespace Paprika;


public class Program
{
    public const float FOV = 60f;
    private static readonly Stopwatch startTimer = new();
    public static double Time => startTimer.Elapsed.TotalSeconds;
    public static float TimeFloat => (float)Time;
    public static double avg;
    public static DumbBuffer<TriangleWide> WideUploaded;
    public static Stopwatch Timer = new();
    public static readonly int defaultCol = new QuickColor(255, 0, 0, 255).RGBA;
    public static readonly int firstCol = new QuickColor(0, 255, 0, 255).RGBA;
    public static List<TriangleWide> widebatches = [];
    public static PixelPusher pusher = new("Paprika Renderer", 1280, 1024);

    public static readonly int[] colors = [
        new QuickColor(255, 0, 0, 255).RGBA,
        new QuickColor(0, 255, 0, 255).RGBA,
        new QuickColor(0, 0, 255, 255).RGBA,
        new QuickColor(255, 255, 0, 255).RGBA,
        new QuickColor(0, 255, 255, 255).RGBA,
        new QuickColor(255, 0, 255, 255).RGBA,
    ];

    public static void Main(string[] args)
    {
        startTimer.Start();
        now = TimeFloat + 5f;
        Console.WriteLine($"Using vector size of {Vector<byte>.Count * 8} bits for rasterization");


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


        if (args.Length == 0 || !Directory.Exists(Path.GetDirectoryName(args[0])))
        {
            Console.WriteLine("Please specify a directory");
            return;
        }

        Upload(args[0]);


        pusher.MainCamera.Position = new(0.51144695f, 2.4718034f, 8.403356f);
        pusher.MainCamera.Rotation = new(-0.019815441f, -0.9137283f, 0.40335205f, -0.044888653f);
        pusher.StartRender();
    }


    public static void Upload(string path)
    {
        var model = ModelRoot.Load($"{path}");

        var triList = model.LogicalMeshes.SelectMany(m => {
            var meshTris = m.EvaluateTriangles().Select<(IVertexBuilder A, IVertexBuilder B, IVertexBuilder C, Material mat), Triangle>(triangle => {
                Matrix4x4 worldMat = model.LogicalNodes[m.LogicalIndex].WorldMatrix;
                worldMat.Translation += new Vector3(0f, 0f, 0f);

                
                Triangle tri = new(
                    triangle.A.GetGeometry().GetPosition(),
                    triangle.B.GetGeometry().GetPosition(),
                    triangle.C.GetGeometry().GetPosition());



                tri.Transform(worldMat);
                
                // Console.WriteLine(CNormal);
                return tri;
            }); 
            return meshTris;
        })
        .ToList();


        // foreach (Triangle tri in triList);
        for (int i = 0; i < triList.Count; i += Vector<float>.Count)
        {
            TriangleWide current = new();
            for (int j = 0; j < Vector<float>.Count; j++)
            {
                int realIndex = Math.Min(i + j, triList.Count - 1);
                Triangle curTri = triList[realIndex];
                TriangleWide.WriteSlot(curTri, j, ref current);
            }
            widebatches.Add(current);
        }


        WideUploaded = new(widebatches.Count);

        for (int i = 0; i < widebatches.Count; i++)
        {
            WideUploaded[i] = widebatches[i];
        }
    }
    static float now = 0f;
    public static void DEBUG_ReAllocateDumbBuffer()
    {
        if (now - TimeFloat <= 0f)
        {
            now = TimeFloat + 5f;


            // DumbBuffer<TriangleWide> wideBuf = new(widebatches.Count);
            // for (int i = 0; i < widebatches.Count; i++)
            // {
            //     wideBuf[i] = widebatches[i];
            // }

            // WideUploaded.Dispose();
            // WideUploaded = wideBuf;


            // int width = pusher.pixelBuffer1.Size.Width;
            // int height = pusher.pixelBuffer1.Size.Height;
            // pusher.pixelBuffer1.Dispose();
            // pusher.pixelBuffer1 = new(width, height);


            // width = pusher.pixelBuffer2.Size.Width;
            // height = pusher.pixelBuffer2.Size.Height;
            // pusher.pixelBuffer2.Dispose();
            // pusher.pixelBuffer2 = new(width, height);



            // width = pusher.ZBuffer.Size.Width;
            // height = pusher.ZBuffer.Size.Height;
            // pusher.ZBuffer.Dispose();
            // pusher.ZBuffer = new(width, height);
            Console.WriteLine("Re-Allocating pixel buffers!");
        }
    }
}



public static class ScreenQuad
{
    public static float[] Vertices = [ // Screen quad
        1.0f,  1.0f,  0.0f,    1f, 1f,
        1.0f, -1.0f,  0.0f,    1f, 0f,
       -1.0f, -1.0f,  0.0f,    0f, 0f,
       -1.0f,  1.0f,  0.0f,    0f, 1f
    ];



    public static uint[] Indices = [
        0u, 1u, 3u,
        1u, 2u, 3u
    ];
}


public static class TestCube
{
    public static float[] CubeData = {
        -1.0f,-1.0f,-1.0f, // triangle 1 : begin
        -1.0f,-1.0f, 1.0f,
        -1.0f, 1.0f, 1.0f, // triangle 1 : end
        1.0f, 1.0f,-1.0f, // triangle 2 : begin
        -1.0f,-1.0f,-1.0f,
        -1.0f, 1.0f,-1.0f, // triangle 2 : end
        1.0f,-1.0f, 1.0f,
        -1.0f,-1.0f,-1.0f,
        1.0f,-1.0f,-1.0f,
        1.0f, 1.0f,-1.0f,
        1.0f,-1.0f,-1.0f,
        -1.0f,-1.0f,-1.0f,
        -1.0f,-1.0f,-1.0f,
        -1.0f, 1.0f, 1.0f,
        -1.0f, 1.0f,-1.0f,
        1.0f,-1.0f, 1.0f,
        -1.0f,-1.0f, 1.0f,
        -1.0f,-1.0f,-1.0f,
        -1.0f, 1.0f, 1.0f,
        -1.0f,-1.0f, 1.0f,
        1.0f,-1.0f, 1.0f,
        1.0f, 1.0f, 1.0f,
        1.0f,-1.0f,-1.0f,
        1.0f, 1.0f,-1.0f,
        1.0f,-1.0f,-1.0f,
        1.0f, 1.0f, 1.0f,
        1.0f,-1.0f, 1.0f,
        1.0f, 1.0f, 1.0f,
        1.0f, 1.0f,-1.0f,
        -1.0f, 1.0f,-1.0f,
        1.0f, 1.0f, 1.0f,
        -1.0f, 1.0f,-1.0f,
        -1.0f, 1.0f, 1.0f,
        1.0f, 1.0f, 1.0f,
        -1.0f, 1.0f, 1.0f,
        1.0f,-1.0f, 1.0f
    };
}