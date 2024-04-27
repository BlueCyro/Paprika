using System.Numerics;
using System.Diagnostics;
using SharpGLTF.Schema2;
using SharpGLTF.Geometry;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;


namespace Paprika;
using static VectorHelpers;



public class Program
{
    public const float FOV = 60f;
    private static readonly Stopwatch startTimer = new();
    public static double Time => startTimer.Elapsed.TotalSeconds;
    public static float TimeFloat => (float)Time;
    public static double avg;
    public static Triangle[] Uploaded = [];
    public static Stopwatch Timer = new();
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
        startTimer.Start();
        Console.WriteLine($"Using vector size of {Vector<byte>.Count * 8} bits for rasterization");
        var pusher = new PixelPusher("Paprika Renderer", 1280, 1024);

        if (args.Length == 0 || !Directory.Exists(Path.GetDirectoryName(args[0])))
        {
            Console.WriteLine("Please specify a directory");
            return;
        }

        var model = ModelRoot.Load($"{args[0]}");
        List<Triangle> triList = [];

        var triangles = model.LogicalMeshes.SelectMany(m => {
            var meshTris = m.EvaluateTriangles().Select<(IVertexBuilder A, IVertexBuilder B, IVertexBuilder C, Material mat), Triangle>(triangle => {
                Matrix4x4 worldMat = model.LogicalNodes[m.LogicalIndex].WorldMatrix;
                worldMat.Translation += new Vector3(0f, 0f, 0f);
                // List<Vector4> transformed = [];

                IVertexBuilder[] points = [triangle.A, triangle.B, triangle.C];
                Vertex[] vertices = new Vertex[3];


                // for (int i = 0; i < points.Length; i++)
                // {
                //     var data = points[i].GetGeometry();
                //     // var normal = data.TryGetNormal(out Vector3 aNorm) ? aNorm : new(0.5f, 0.5f, 1f);
                //     // var uv = points[i].GetMaterial().GetTexCoord(0);

                //     // vertices[i] = new(Vector3.Transform(data.GetPosition(), worldMat), Vector3.TransformNormal(normal, worldMat), default, uv);
                // }

                
                Triangle tri = new(
                    triangle.A.GetGeometry().GetPosition(),
                    triangle.B.GetGeometry().GetPosition(),
                    triangle.C.GetGeometry().GetPosition());



                tri.Transform(worldMat);
                
                // Console.WriteLine(CNormal);
                return tri;
            }); 
            return meshTris;
        });


        Uploaded = triangles.ToArray();

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