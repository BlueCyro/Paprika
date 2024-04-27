using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using BepuUtilities;


namespace Paprika;


public partial class PixelPusher
{
    public static readonly Vector<int> FullByte = new(255);
    public static readonly Vector<int> zeroInt = new();
    public static readonly int allBits = unchecked((int)uint.MaxValue);
    public static readonly Vector3 Red = new(1f, 0f, 0f);
    public static readonly Vector3 Green = new(0f, 1f, 0f);
    public static readonly Vector3 Blue = new(0f, 0f, 1f);



    // public void DrawLine(in Vector2 from, in Vector2 to, in int col)
    // {
    //     var diff = to - from;
    //     var abs = Vector2.Abs(diff);
    //     float step = MathF.Max(abs.X, abs.Y);
        
    //     diff /= step;
    //     var start = from;

    //     for (int i = 0; i < step; i++)
    //     {
    //         SetPixel(col, start.X, start.Y);
    //         start += diff;
    //     }
    // }



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



    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public void DrawBounds(in Triangle tri, in int col)
    // {
    //     VectorHelpers.GetTriBounds(tri, Vector2.Zero, FrameBufferSize - Vector2.One, out var bboxMin, out var bboxMax);
    //     Bounds2D bounds = new(bboxMin, bboxMax);

    //     DrawLine(bounds.Min, new(bounds.Min.X, bounds.Max.Y), col);
    //     DrawLine(bounds.Min, new(bounds.Max.X, bounds.Min.Y), col);
    //     DrawLine(bounds.Max, new(bounds.Min.X, bounds.Max.Y), col);
    //     DrawLine(bounds.Max, new(bounds.Max.X, bounds.Min.Y), col);
    // }



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
    public unsafe void DrawPinedaTriangleSIMD(ref Triangle tri, in int col, in Span<int> bufSlice, in Span<float> zBufSlice)
    {
        tri.ZDivide(out Vector3 oldZ);

        
        Vector<int> wideCol = new(col);
        tri.GetBounds(out var bboxMin, out var bboxMax);
        // Bounds2D triBounds = new(bboxMin, bboxMax);
        EdgesVectorized edges = new(tri.A, tri.B, tri.C);


        int minX = Math.Clamp((int)bboxMin.X, 0, FrameBufferSize.Width - 1);
        int minY = Math.Clamp((int)bboxMin.Y, 0, FrameBufferSize.Height - 1);
        int maxX = Math.Clamp((int)bboxMax.X, 0, FrameBufferSize.Width - 1);
        int maxY = Math.Clamp((int)bboxMax.Y, 0, FrameBufferSize.Height - 1);
        

        int width = FrameBufferSize.Width;
        var zeroVec = EdgesVectorized.Zero;

        for (int x = maxX; x > minX; x -= Vector<int>.Count)
        {
            for (int y = maxY; y > minY; y--)
            {
                edges.IsInside(x, y, out Vector3Wide eN, out _);


                int vecStart = x - Vector<int>.Count + y * width;
                int vecFloatStart = x - Vector<float>.Count + y * width;
                ref int first = ref bufSlice[vecStart];
                ref float firstZ = ref zBufSlice[vecFloatStart];

                Vector<int> bufVec = Vector.LoadUnsafe(ref first);
                Vector<float> bufVecZ = Vector.LoadUnsafe(ref firstZ);

                // VectorHelpers.InterpolateBarycentric(Red, Green, Blue, eN, out BigVec3 bigCol);
                // VectorHelpers.InterpolateBarycentric(tri.uv1, tri.uv2, tri.uv3, eN, out BigVec2 bigUV);
                VectorHelpers.InterpolateBarycentric(oldZ.X, oldZ.Y, oldZ.Z, eN, out Vector<float> bigDepth);


                // bigUV *= 255f;
                // bigUV.ConvertToInt32(out var RWide, out var GWide);
                // Vector<int> wideInterp = FullByte << 24 | zeroInt << 16 | GWide << 8 | RWide;


                var depthMask = Vector.LessThanOrEqual(bigDepth, bufVecZ);


                var mask =
                    Vector.GreaterThanOrEqual(eN.X, zeroVec) &
                    Vector.GreaterThanOrEqual(eN.Y, zeroVec) &
                    Vector.GreaterThanOrEqual(eN.Z, zeroVec);


                Vector<int> maskedCol = Vector.ConditionalSelect(mask & depthMask, wideCol, bufVec);
                Vector<float> maskedZ = Vector.ConditionalSelect(mask & depthMask, bigDepth, bufVecZ);
                maskedCol.StoreUnsafe(ref first);
                maskedZ.StoreUnsafe(ref firstZ);

                // FillRect(new(x - Vector<int>.Count, y - 1), new(x, y), x / Vector<int>.Count % 2 == 0 ? new QuickColor(128, 0, 0, 255).RGBA : new QuickColor(0, 128, 0, 255).RGBA);

            }

            // rowSlice.CopyTo(rowBuf);
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
