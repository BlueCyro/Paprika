using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Numerics;
using System.Diagnostics;
using SharpGLTF.Schema2;


namespace Paprika;


using static VectorHelpers;


public class Program
{
    public const float FOV = 60f;
    static readonly DateTime Start = DateTime.Now;
    public static double Time => (DateTime.Now - Start).TotalSeconds;
    public static float TimeFloat => (float)Time;


    public static readonly int defaultCol = new QuickColor(255, 0, 0, 255).RGBA;
    public static readonly int firstCol = new QuickColor(0, 255, 0, 255).RGBA;

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
        // var image = SixLabors.ImageSharp.Image.Load<Rgba32>("./Porch.png");
        Console.WriteLine($"Using vector size of {Vector<byte>.Count * 8} bits for rasterization");
        var pusher = new PixelPusher("Paprika Renderer", Vector64.Create(1280, 1024));

        var model = ModelRoot.Load("./Models/USA_Pizza.glb");

        var triangles = model.LogicalMeshes.SelectMany(m => {
            var meshTris = m.EvaluateTriangles().SelectMany(triangle => {
                Matrix4x4 worldMat = model.LogicalNodes[m.LogicalIndex].WorldMatrix;
                worldMat.Translation = new(1f, -2f, 5f);
                List<Vector4> transformed = [];

                // Console.WriteLine(triangle.A.GetGeometry().GetPosition());
                // Console.WriteLine(triangle.B.GetGeometry().GetPosition());
                // Console.WriteLine(triangle.C.GetGeometry().GetPosition());
                transformed.AddRange([
                    new(Vector3.Transform(triangle.A.GetGeometry().GetPosition(), worldMat), 1f),
                    new(Vector3.Transform(triangle.B.GetGeometry().GetPosition(), worldMat), 1f),
                    new(Vector3.Transform(triangle.C.GetGeometry().GetPosition(), worldMat), 1f),
                    new(0f, 0f, 0f, 1f)
                ]);
                return transformed;
            }); 
            return meshTris;
        });


        Triangle[] triArray = MemoryMarshal.Cast<Vector4, Triangle>(triangles.ToArray().AsSpan()).ToArray();
        
 
        var identity = Matrix4x4.Identity;
        identity *= 150f;


        Stopwatch timer = new();
        pusher.OnUpdate += (s, delta) => {
            timer.Start();
            Quaternion rot = Quaternion.CreateFromYawPitchRoll(TimeFloat * 45f * DEG2RAD, 30f * DEG2RAD, 0f);
            var localtr = Matrix4x4.Transform(identity, rot);
            localtr.Translation = pusher.SizeVec2.AsVector128().AsVector3() / 2f + new Vector3(0f, 0f, 2000f) /* + new Vector3(0f, 0f, -10f + (MathF.Sin(TimeFloat * MathF.PI) * 0.5f + 0.5f) * 100f)*/;



            int j = 0;

            Span<int> pixelBuf = pusher.PixelBuffer.AsSpan();
            Span<float> zBuf = pusher.ZBuffer.AsSpan();


            foreach (var tri in triArray)
            {
                Triangle curTri = tri;
                curTri.Transform(localtr);
                var dot = Vector3.Dot(curTri.Normal, Vector3.UnitZ);
                byte byteDot = (byte)(MathF.Pow((dot * 0.5f) + 0.5f, 1f) * 255);


                if (dot >= 0f)
                // if (j == 256)
                {
                    QuickColor col = new()
                    {
                        RGBA = firstCol
                    };
                    col *= dot;
                    // pusher.DrawPinedaTriangle(tri, defaultCol);
                    // pusher.DrawBounds(tri, defaultCol);
                    var white = (new QuickColor(255, 255, 255, 255) * dot).RGBA;
                    pusher.DrawPinedaTriangleSIMD(curTri, white, pixelBuf, zBuf);
                    // pusher.DrawTriangle(tri, new QuickColor(255, 128, 0, 255).RGBA);
                }
                j++;
            }

            timer.Stop();

            Console.WriteLine(timer.Elapsed.TotalMilliseconds);
            timer.Reset();
        };

        pusher.StartRender();
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