using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using BepuUtilities;


namespace Paprika;


public partial class PaprikaRenderer
{
    public static readonly Vector<int> FullByte = new(255);
    public static readonly Vector<int> zeroInt = new();
    public const int AllBits = unchecked((int)uint.MaxValue);
    public static readonly Vector3 Red = new(1f, 0f, 0f);
    public static readonly Vector3 Green = new(0f, 1f, 0f);
    public static readonly Vector3 Blue = new(0f, 0f, 1f);



    public void DrawLine(in Vector3 from, in Vector3 to, in int col)
    {
        var diff = to - from;
        var abs = Vector3.Abs(diff);
        float step = MathF.Max(abs.X, abs.Y);
        
        diff /= step;
        var start = from;

        for (int i = 0; i < step; i++)
        {
            SetPixel(col, start.X, start.Y);
            start += diff;
        }
    }



    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public void DrawTriangle(in Triangle tri, in int col)
    // {
    //     Vector2 max = FrameBufferSize - Vector2.One;
    //     Vector2 v1Clamp = Vector2.Clamp(tri.v1, Vector2.Zero, max);
    //     Vector2 v2Clamp = Vector2.Clamp(tri.v2, Vector2.Zero, max);
    //     Vector2 v3Clamp = Vector2.Clamp(tri.v3, Vector2.Zero, max);
    //     DrawLine(v1Clamp, v2Clamp, col);
    //     DrawLine(v2Clamp, v3Clamp, col);
    //     DrawLine(v3Clamp, v1Clamp, col);
    // }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawBounds(Vector3 min, Vector3 max, in int col)
    {
        DrawLine(min, new(min.X, max.Y, 0), col);
        DrawLine(min, new(max.X, min.Y, 0), col);
        DrawLine(max, new(min.X, max.Y, 0), col);
        DrawLine(max, new(max.X, min.Y, 0), col);
    }



    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public unsafe void DrawPinedaTriangle(in Triangle tri, in int col) // Slow, but reliable
    // {
    //     VectorHelpers.GetTriBounds(tri, Vector2.Zero, FrameBufferSize - Vector2.One, out var bboxMin, out var bboxMax);
    //     Bounds2D triBounds = new(bboxMin, bboxMax);
    //     Lines triLines = new(tri.v1, tri.v2, tri.v3);

    //     Vector3 e_n;
    //     bool test;
    //     int minX = (int)triBounds.Min.X;
    //     int minY = (int)triBounds.Min.Y;
    //     int maxX = (int)triBounds.Max.X;
    //     int maxY = (int)triBounds.Max.Y;

    //     bool stop = false;
    //     int length = maxX - minX + 1;

        
    //     int[] pBuf = PixelBuffer;
    //     float[] zBuf = ZBuffer;

    //     for (int y = minY; y <= maxY; y++)
    //     {
    //         int start = minX + y * FrameBufferSize.Width;

    //         Span<int> row = pBuf.AsSpan().Slice(start, length);
    //         Span<float> depthRow = zBuf.AsSpan().Slice(start, length);
    //         int x = 0;
    //         while (x <= length)
    //         {
    //             e_n = tri.Bary(minX + x, y, triLines);
    //             test = e_n.X >= 0f && e_n.Y >= 0f && e_n.Z >= 0f && x < length;
    //             if (test)
    //             {
    //                 float depth = tri.p1.Z * e_n.X + tri.p2.Z * e_n.Y + tri.p3.Z * e_n.Z;

    //                 if (depthRow[x] < depth)
    //                 {
    //                     row[x] = col;
    //                     depthRow[x] = depth;
    //                 }
    //                 x++;
    //                 stop = true;
    //                 continue;
    //             }

    //             if (stop)
    //                 break;
    //             x++;
    //         }
    //         stop = false;
    //     }
    // }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPinedaTriangleSIMD(
        in Triangle tri,
        in int col,
        DumbBuffer<int> bufStart,
        DumbBuffer<float> zBufStart,
        in Vector128<int> bbox,
        in Vector3 oldZ,
        ref EdgesVectorized edges)
    {
        Vector<int> wideCol = new(col);
        edges.UpdateEdges(tri.A, tri.B, tri.C);


        int frameWidth = FrameBufferSize.Width;
        for (int y = bbox[3]; y > bbox[1]; y--)
        {
            int row = y * frameWidth;
            for (int x = bbox[2]; x > bbox[0]; x -= Vector<int>.Count)
            {
                int vecStart = x - Vector<int>.Count + row;
                int vecFloatStart = x - Vector<float>.Count + row;

                ref int colStart = ref bufStart[vecStart];
                ref float zStart = ref zBufStart[vecFloatStart];


                Vector<int> bufVec = Vector.LoadUnsafe(ref colStart);
                Vector<float> bufVecZ = Vector.LoadUnsafe(ref zStart);


                edges.IsInside(x, y, out Vector3Wide eN);


                RasterHelpers.InterpolateBarycentric(oldZ.X, oldZ.Y, oldZ.Z, eN, out Vector<float> bigDepth);
                Vector<int> depthMask = Vector.LessThanOrEqual(bigDepth, bufVecZ);


                Vector<int> mask =
                    Vector.GreaterThanOrEqual(eN.X, Vector<float>.Zero) &
                    Vector.GreaterThanOrEqual(eN.Y, Vector<float>.Zero) &
                    Vector.GreaterThanOrEqual(eN.Z, Vector<float>.Zero);

                Vector<int> colResult = Vector.ConditionalSelect(mask & depthMask, wideCol, bufVec);
                Vector<float> zResult = Vector.ConditionalSelect(mask & depthMask, bigDepth, bufVecZ);

                colResult.StoreUnsafe(ref colStart);
                zResult.StoreUnsafe(ref zStart);
            }
        }
    }

    public void FillRect(in Vector4 bbox)
    {
        for (int y = (int)bbox.W; y > bbox.Y; y--)
        {
            for (int x = (int)bbox.Z - 1; x > bbox.X; x--)
            {
                SetPixel(x / Vector<int>.Count % 2 == 0 ? new QuickColor(128, 0, 0, 255).RGBA : new QuickColor(0, 128, 0, 255).RGBA, x, y);
            }
        }
    }
}



// This is just a blatant clone of Bepu's version of this.
// In theory, this should help the compiler inline better according to mister John Bepu (Ross)
public interface IForLoop<T>
{
    void LoopBody(T iteration);
}



// public struct TriBoundsRowIterator : IForLoop<int>
// {
//     int row;
//     TriBoundsColumnIterator columnIterator;



//     public void LoopBody(int iteration)
//     {

//     }



//     public void Reset()
//     {
        
//     }
// }



// public struct TriBoundsColumnIterator : IForLoop<int>
// {

//     public void LoopBody(int iteration)
//     {

//     }
// }