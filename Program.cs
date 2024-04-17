using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.CompilerServices;
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


        Triangle testTri = new()
        {
            TriMatrix = new(
                0f,   640f,  0f, 1f,
                780f, 330f,  0f, 1f,
                330f, 128f, 0f, 1f,
                0f, 0f, 0f, 1f
            )
        };

        Stopwatch timer = new();
        pusher.OnUpdate += (s, delta) => {
            timer.Start();
            Array.Clear(pusher.PixelBuffer);
            Array.Clear(pusher.ZBuffer);


            // Quaternion rot = Quaternion.CreateFromYawPitchRoll(TimeFloat, TimeFloat * 1.3f, TimeFloat * 1.5f);
            // Quaternion rot = Quaternion.CreateFromYawPitchRoll(TimeFloat * 45f * DEG2RAD, MathF.Sin(TimeFloat) * 30f * DEG2RAD, 0f);
            Quaternion rot = Quaternion.CreateFromYawPitchRoll(TimeFloat * 45f * DEG2RAD, 30f * DEG2RAD, 0f);
            var localtr = Matrix4x4.Transform(identity, rot);
            localtr.Translation = pusher.SizeVec2.AsVector128().AsVector3() / 2f + new Vector3(0f, 0f, 2000f) /* + new Vector3(0f, 0f, -10f + (MathF.Sin(TimeFloat * MathF.PI) * 0.5f + 0.5f) * 100f)*/;



            int j = 0;
            foreach (var tri in triArray)
            {
                tri.Transform(localtr);
                var dot = Vector3.Dot(tri.Normal, Vector3.UnitZ);
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
                    pusher.DrawPinedaTriangleSIMD(tri, col.RGBA);
                    // pusher.DrawTriangle(tri, new QuickColor(255, 128, 0, 255).RGBA);
                }
                j++;
            }

            // Triangle tri = triArray[256];
            // tri.Transform(localtr);
            // pusher.DrawPinedaTriangleSIMD(tri, firstCol);
            // pusher.DrawBounds(tri, defaultCol);

            // testTri.Transform(localtr);
            // pusher.DrawFilledTriangle(testTri, firstCol);
            // pusher.DrawTriangle(testTri, defaultCol);
            // pusher.DrawBounds(testTri, defaultCol);
            // triArray.AsParallel().ForAll(tri => {
            //     tri.Transform(localtr);
            //     var dot = Vector3.Dot(tri.Normal, Vector3.UnitZ);
            //     QuickColor col = new()
            //     {
            //         RGBA = firstCol
            //     };
            //     col *= dot;
            //     // pusher.DrawPinedaTriangle(tri, defaultCol);
            //     // pusher.DrawBounds(tri, defaultCol);
            //     pusher.DrawPinedaTriangleSIMD(tri, col.RGBA);
            //     // pusher.DrawTriangle(tri, new QuickColor(255, 128, 0, 255).RGBA);
            // });


            pusher.PushPixels();
            timer.Stop();

            Console.WriteLine(timer.Elapsed.TotalMilliseconds);
            timer.Reset();
        };

        pusher.StartRender();
    }
}


[StructLayout(LayoutKind.Explicit)]
public struct Triangle
{
    [FieldOffset(0)]
    public readonly Vector2 v1;
    
    [FieldOffset(16)]
    public readonly Vector2 v2;

    [FieldOffset(32)]
    public readonly Vector2 v3;

    
    
    [FieldOffset(0)]
    public Vector3 p1;
    
    [FieldOffset(16)]
    public Vector3 p2;
    
    [FieldOffset(32)]
    public Vector3 p3;

    
    
    [FieldOffset(0)]
    public Matrix4x4 TriMatrix;

    [FieldOffset(0)]
    public Vector256<float> Vec256;

    public readonly Vector3 Normal => Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));
    public readonly Vector3 Center => (p1 + p2 + p3) / 3f;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Transform(in Matrix4x4 transform)
    {
        TriMatrix *= transform;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 Bary(in int x, in int y, in Lines lines)
    {
        float e1 = (x - v1.X) * lines.L0.Y - (y - v1.Y) * lines.L0.X;
        float e2 = (x - v2.X) * lines.L1.Y - (y - v2.Y) * lines.L1.X;
        float e3 = (x - v3.X) * lines.L2.Y - (y - v3.Y) * lines.L2.X;
        float area = e1 + e2 + e3;


        return new Vector3(e2, e3, e1) / area;
    }


    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 Bary(in float x, in float y, in Lines lines)
    {
        float e1 = (x - v1.X) * lines.L0.Y - (y - v1.Y) * lines.L0.X;
        float e2 = (x - v2.X) * lines.L1.Y - (y - v2.Y) * lines.L1.X;
        float e3 = (x - v3.X) * lines.L2.Y - (y - v3.Y) * lines.L2.X;
        float area = e1 + e2 + e3;


        return new Vector3(e2, e3, e1) / area;
    }
}


[StructLayout(LayoutKind.Explicit)]
public ref struct QuickVec3(in Vector3 target)
{
    [FieldOffset(0)]
    public Vector3 V3 = target;
    
    [FieldOffset(0)]
    public Vector128<float> V128;
}


[StructLayout(LayoutKind.Explicit)]
public unsafe ref struct TriangleBanger<T>(in T data) where T : struct
{
    [FieldOffset(0)]
    public T FragmentData = data;
}



[StructLayout(LayoutKind.Explicit)]
public ref struct Lines(in Vector2 p1, in Vector2 p2, in Vector2 p3)
{
    [FieldOffset(0)]
    public Vector2 L0 = p1 - p2;

    [FieldOffset(8)]
    public Vector2 L1 = p2 - p3;

    [FieldOffset(16)]
    public Vector2 L2 = p3 - p1;
}



// [StructLayout(LayoutKind.Explicit)]
public readonly ref struct Edges(in Vector2 p1, in Vector2 p2, in Vector2 p3)
{
    public readonly float A1 = p1.Y - p2.Y;
    public readonly float B1 = p2.X - p1.X;
    public readonly float C1 = p1.X * p2.Y - p2.X * p1.Y;


    public readonly float A2 = p2.Y - p3.Y;
    public readonly float B2 = p3.X - p2.X;
    public readonly float C2 = p2.X * p3.Y - p3.X * p2.Y;


    public readonly float A3 = p3.Y - p1.Y;
    public readonly float B3 = p1.X - p3.X;
    public readonly float C3 = p3.X * p1.Y - p1.X * p3.Y;



    public readonly void IsInside(int x, int y, out float e1, out float e2, out float e3)
    {
        e1 = A1 * x + B1 * y + C1;
        e2 = A2 * x + B2 * y + C2;
        e3 = A3 * x + B3 * y + C3;
    }
}



public readonly ref struct EdgesVectorized(in Vector2 p1, in Vector2 p2, in Vector2 p3)
{
    public static readonly Vector<float> Zero = new();
    public static readonly Vector<float> One = new(1f);
    public static readonly Vector<float> Row = new(Enumerable.Range(0, Vector<float>.Count).Select(i => (float)i).Reverse().ToArray());

    public readonly Vector<float> A1 = new(p1.Y - p2.Y);
    public readonly Vector<float> B1 = new(p2.X - p1.X);
    public readonly Vector<float> C1 = new(p1.X * p2.Y - p2.X * p1.Y);



    public readonly Vector<float> A2 = new(p2.Y - p3.Y);
    public readonly Vector<float> B2 = new(p3.X - p2.X);
    public readonly Vector<float> C2 = new(p2.X * p3.Y - p3.X * p2.Y);


    public readonly Vector<float> A3 = new(p3.Y - p1.Y);
    public readonly Vector<float> B3 = new(p1.X - p3.X);
    public readonly Vector<float> C3 = new(p3.X * p1.Y - p1.X * p3.Y);


    public readonly void IsInside(in int startX, in int startY, out Vector<float> e1, out Vector<float> e2, out Vector<float> e3)
    {
        var startXV = (One * startX) - Row;
        var startYV = One * startY;


        e1 = A1 * startXV + B1 * startYV + C1;
        e2 = A2 * startXV + B2 * startYV + C2;
        e3 = A3 * startXV + B3 * startYV + C3;
    }
}



[StructLayout(LayoutKind.Explicit)]
public ref struct Bounds2D
{
    [FieldOffset(0)]
    public Vector2 Min;
    [FieldOffset(8)]
    public Vector2 Max;

    [FieldOffset(0)]
    public Vector128<float> Bounds;

    [FieldOffset(0)]
    public Vector4 BoundsVec4;

    [FieldOffset(16)]
    public Vector2 Mid;
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