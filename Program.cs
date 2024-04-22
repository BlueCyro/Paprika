using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Numerics;
using System.Diagnostics;
using SharpGLTF.Schema2;
using SharpGLTF.Geometry;


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


                for (int i = 0; i < points.Length; i++)
                {
                    var data = points[i].GetGeometry();
                    var normal = data.TryGetNormal(out Vector3 aNorm) ? aNorm : new(0.5f, 0.5f, 1f);
                    var uv = points[i].GetMaterial().GetTexCoord(0);

                    vertices[i] = new(Vector3.Transform(data.GetPosition(), worldMat), Vector3.TransformNormal(normal, worldMat), default, uv);
                }

                
                Triangle tri = new(vertices);

                
                // Console.WriteLine(CNormal);
                return tri;
            }); 
            return meshTris;
        });


        Triangle[] triArray = triangles.ToArray();
        

        Stopwatch timer = new();
        double avg = 0;
        pusher.OnUpdate += (s, evArgs) => DoRender(s, evArgs, pusher, timer, triArray, ref avg);


        pusher.StartRender();
    }



    public static void DoRender(object? s, RenderEventArgs args, PixelPusher pusher, Stopwatch timer, Triangle[] triArray, ref double avg)
    {
        timer.Start();
        

        int j = 0;

        Span<int> pixelBuf = pusher.PixelBuffer.AsSpan();
        Span<float> zBuf = pusher.ZBuffer.AsSpan();

        Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(new(0f, 0f, 0f), new(0f, 0f, 1f), Vector3.UnitY);
        Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(60f * DEG2RAD, pusher.SizeVec2.X / pusher.SizeVec2.Y, 0.1f, 1000f);
        Matrix4x4 translation = Matrix4x4.CreateTranslation(new(0f, -0.5f, 3f));

        Quaternion rot = Quaternion.CreateFromYawPitchRoll(180f * DEG2RAD + MathF.Sin(TimeFloat) * 30f * DEG2RAD, 25f * DEG2RAD, 0f);
        
        foreach (var tri in triArray)
        {
            Matrix4x4 translated = Matrix4x4.Transform(tri.TriMatrix, rot) * translation;




            Triangle curTri = tri with
            {
                TriMatrix =
                    Matrix4x4.Transform(tri.TriMatrix, rot) *
                    translation
            };
            
            var dot = Vector3.Dot(curTri.Normal, Vector3.Normalize(new Vector3(0f, 0f, 0f) - curTri.Center));

            curTri.TriMatrix =
                curTri.TriMatrix *
                viewMatrix *
                projectionMatrix;
            
            

            // curTri.TriMatrix *= perspectiveDivide;
            // curTri.TriMatrix *= screenTransformMatrix;


            curTri.p1 /= curTri.m1N.W;
            curTri.p2 /= curTri.m2N.W;
            curTri.p3 /= curTri.m3N.W;



            curTri.v1 = new((curTri.v1.X + 1) * 0.5f * pusher.SizeVec2.X, (curTri.v1.Y + 1f) * 0.5f * pusher.SizeVec2.Y);
            curTri.v2 = new((curTri.v2.X + 1) * 0.5f * pusher.SizeVec2.X, (curTri.v2.Y + 1f) * 0.5f * pusher.SizeVec2.Y);
            curTri.v3 = new((curTri.v3.X + 1) * 0.5f * pusher.SizeVec2.X, (curTri.v3.Y + 1f) * 0.5f * pusher.SizeVec2.Y);



            if (dot >= 0f)
            // if (j == 256)
            {
                // pusher.DrawPinedaTriangle(tri, defaultCol);
                // pusher.DrawBounds(tri, defaultCol);
                var white = (new QuickColor(255, 255, 255, 255) * dot).RGBA;
                // int normal = new QuickColor(curTri.Normal / new Vector3(2f, 2f, 1f) + new Vector3(0.5f, 0.5f, 0f)).RGBA;
                pusher.DrawPinedaTriangleSIMD(curTri, white, pixelBuf, zBuf);
                // pusher.DrawBounds(curTri, firstCol);
                // pusher.DrawTriangle(curTri, new QuickColor(255, 128, 0, 255).RGBA);
            }
            j++;
        }

        timer.Stop();
        avg += timer.Elapsed.TotalMilliseconds;
        ulong frameCount = 20;
        if (args.Frame % frameCount == 0)
        {
            Console.WriteLine($"[Paprika] Took (avg): {avg / frameCount:F3}ms, Frame: {args.Frame}");
            avg = 0;
        }
        // Console.WriteLine(timer.Elapsed.TotalMilliseconds);
        timer.Reset();
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