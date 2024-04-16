using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;


namespace Paprika;


public partial class PixelPusher
{
    private const float SPARSE_SIZE = 8f;
    private static readonly Vector2 addend = new(SPARSE_SIZE, SPARSE_SIZE);
    private static readonly Vector2 boundsOffsets = new(Vector256<float>.Count, Vector256<float>.Count);



    public void DrawLine(in Vector2 from, in Vector2 to, in int col)
    {
        var diff = to - from;
        var abs = Vector2.Abs(diff);
        float step = MathF.Max(abs.X, abs.Y);
        
        diff /= step;
        var start = from;

        for (int i = 0; i < step; i++)
        {
            SetPixel(col, start.X, start.Y);
            start += diff;
        }
    }



    public static int SorterX(Vector2 a, Vector2 b)
    {
        return MathF.Sign(a.X - b.X);
    }



    public static int SorterY(Vector2 a, Vector2 b)
    {
        return MathF.Sign(b.Y - a.Y);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTriangle(in Triangle tri, in int col)
    {
        Vector2 max = SizeVec2 - Vector2.One;
        Vector2 v1Clamp = Vector2.Clamp(tri.v1, Vector2.Zero, max);
        Vector2 v2Clamp = Vector2.Clamp(tri.v2, Vector2.Zero, max);
        Vector2 v3Clamp = Vector2.Clamp(tri.v3, Vector2.Zero, max);
        DrawLine(v1Clamp, v2Clamp, col);
        DrawLine(v2Clamp, v3Clamp, col);
        DrawLine(v3Clamp, v1Clamp, col);
    }


    
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public void DrawFilledTriangle(in Triangle tri, in int col)
    // {
    //     Bounds2D triBounds = VectorHelpers.GetTriBounds(tri);   
    //     int minX = (int)triBounds.Min.X;
    //     int minY = (int)triBounds.Min.Y;
    //     int maxX = (int)triBounds.Max.X;
    //     int maxY = (int)triBounds.Max.Y;


    //     Lines triLines = new();
    //     bool stop = false;
    //     for (int y = minY; y <= maxY; y++)
    //     {
    //         for (int x = minX; x <= maxX; x++)
    //         {
                
    //             if (stop)
    //                 break;
    //             triLines.Update(new(x, y), tri.v1, tri.v2, tri.v3);
    //             // SetPixel(new QuickColor(0, 0, 255, 255).RGBA, x, y);

    //             while (triLines.UVW.X >= 0 && triLines.UVW.Y >= 0 && triLines.UVW.Z >= 0)
    //             // Console.WriteLine(tri.v1 * x + tri.v2 * y + tri.v3);
    //             // while (Vector128.GreaterThanOrEqualAll((tri.v1 * x + tri.v2 * y + tri.v3).AsVector128(), zero))
    //             {
    //                 x++;
    //                 stop = true;
    //                 SetPixel(col, x, y);
    //                 triLines.Update(new(x, y), tri.v1, tri.v2, tri.v3);
    //             }
    //         }
    //         stop = false;
    //     }
    // }



    public void DrawBounds(in Triangle tri, in int col)
    {
        Bounds2D bounds = VectorHelpers.GetTriBounds(tri);
        bounds.BoundsVec4 = Vector4.Clamp(bounds.BoundsVec4, Vector4.Zero, SizeVec4);
        DrawLine(bounds.Min, new(bounds.Min.X, bounds.Max.Y), col);
        DrawLine(bounds.Min, new(bounds.Max.X, bounds.Min.Y), col);
        DrawLine(bounds.Max, new(bounds.Min.X, bounds.Max.Y), col);
        DrawLine(bounds.Max, new(bounds.Max.X, bounds.Min.Y), col);
    }


    public void DrawTriangleSmart(in Triangle tri, in int col)
    {
        Span<Vector2> vX = stackalloc Vector2[3];
        Vector2 max = SizeVec2 - Vector2.One;
        Lines triLines = new(tri.v1, tri.v2, tri.v3);

        vX[0] = Vector2.Clamp(tri.v1, Vector2.Zero, max);
        vX[1] = Vector2.Clamp(tri.v2, Vector2.Zero, max);
        vX[2] = Vector2.Clamp(tri.v3, Vector2.Zero, max);
        vX.Sort(SorterY);
        bool set = true;
        // bool searching = false;
        int originalX = (int)vX[0].X;
        int x = originalX;
        int incrementer = 1;

        for (int y = (int)vX[0].Y; y > 0; y--)
        {
            Start:
            Vector3 e_n = tri.Bary(x, y, triLines);


            if (e_n.X >= 0f && e_n.Y >= 0f && e_n.Z >= 0f)
            {
                SetPixel(col, x, y);
                set = false;
                continue;
            }

            if (!set && x < vX[2].X && x > 0)
            {
                x += incrementer;
                goto Start;
            }
            incrementer *= -1;
        }
    }


    public unsafe void DrawPinedaTriangle(in Triangle tri, in int col)
    {
        Bounds2D triBounds = VectorHelpers.GetTriBounds(tri);
        triBounds.BoundsVec4 = Vector4.Clamp(triBounds.BoundsVec4, Vector4.Zero, SizeVec4 - Vector4.One);
        Lines triLines = new(tri.v1, tri.v2, tri.v3);

        Vector3 e_n;
        bool test;
        int minX = (int)triBounds.Min.X;
        int minY = (int)triBounds.Min.Y;
        int maxX = (int)triBounds.Max.X;
        int maxY = (int)triBounds.Max.Y;

        bool stop = false;
        int length = maxX - minX + 1;

        
        int[] pBuf = PixelBuffer;
        float[] zBuf = ZBuffer;

        for (int y = minY; y <= maxY; y++)
        {
            int start = minX + y * size[0];

            Span<int> row = pBuf.AsSpan().Slice(start, length);
            Span<float> depthRow = zBuf.AsSpan().Slice(start, length);
            int x = 0;
            while (x <= length)
            {
                e_n = tri.Bary(minX + x, y, triLines);
                test = e_n.X >= 0f && e_n.Y >= 0f && e_n.Z >= 0f && x < length;
                if (test)
                {
                    float depth = tri.p1.Z * e_n.X + tri.p2.Z * e_n.Y + tri.p3.Z * e_n.Z;

                    if (depthRow[x] < depth)
                    {
                        row[x] = col;
                        depthRow[x] = depth;
                    }
                    x++;
                    stop = true;
                    continue;
                }

                if (stop)
                    break;
                x++;
            }
            stop = false;
        }
    }



    public void DrawPinedaTriangleSIMD(in Triangle tri, in int col)
    {
        Bounds2D triBounds = VectorHelpers.GetTriBounds(tri);
        // if (Vector128.GreaterThanAll(triBounds.Bounds, SizeVec4.AsVector128()) ||
        //     Vector128.LessThanAll(triBounds.Bounds, Vector4.Zero.AsVector128()))
        //     return;
        
        triBounds.BoundsVec4 = Vector4.Clamp(triBounds.BoundsVec4, Vector4.Zero, SizeVec4 - Vector4.One);
        EdgesVectorized edges = new(tri.v1, tri.v2, tri.v3);

        // FillRect(triBounds.Min, triBounds.Max);
        int minX = (int)triBounds.Min.X;
        int minY = (int)triBounds.Min.Y;
        int maxX = (int)triBounds.Max.X;
        int maxY = (int)triBounds.Max.Y;


        // int j = 0;
        for (int y = maxY; y > minY; y--)
        {
            for (int x = maxX; x > minX; x -= Vector256<float>.Count)
            {
                edges.IsInside(x, y, out var e1, out var e2, out var e3);

                for (int i = 0; i < Vector256<float>.Count && x - i > 0; i++)
                {
                    if (e1[i] >= 0f && e2[i] >= 0f && e3[i] >= 0f)
                    {
                        SetPixel(col, x - i, y);
                    }
                }
            }
        }
    }



    public void FillRect(in Vector2 min, in Vector2 max, in int color)
    {
        for (int y = (int)max.Y; y > min.Y; y--)
        {
            for (int x = (int)max.X; x > min.X; x--)
            {
                SetPixel(color, x, y);
            }
        }
    }
}
