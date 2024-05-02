using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using BepuUtilities;


namespace Paprika;


public partial class PixelPusher
{
    public static readonly Vector<int> FullByte = new(255);
    public static readonly Vector<int> zeroInt = new();
    public static readonly int AllBits = unchecked((int)uint.MaxValue);
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
    public unsafe void DrawPinedaTriangleSIMD(
        ref Triangle tri,
        in int col,
        DumbBuffer<int> bufStart,
        DumbBuffer<float> zBufStart,
        in Vector4 bbox,
        in Vector3 oldZ)
    {
        Vector<int> wideCol = new(col);
        EdgesVectorized edges = new(tri.A, tri.B, tri.C);

        int minX = (int)bbox.X;
        int minY = (int)bbox.Y;
        int maxX = (int)bbox.Z;
        int maxY = (int)bbox.W;
        int frameWidth = FrameBufferSize.Width;
        // int bboxWidth = maxX - minX;
        int x = 0;
        
        // Span<int> colStash = stackalloc int[bboxWidth + 1];
        // Span<float> zStash = stackalloc float[bboxWidth + 1];
        // int* colStart = (int*)Unsafe.AsPointer(ref colStash[0]);
        // float* zStart = (float*)Unsafe.AsPointer(ref zStash[0]);
        // int i = 0;

        // FillRect(bbox);
        for (int y = maxY; y > minY; y--)
        {
            int row = y * frameWidth;
            // Unsafe.CopyBlock(colStart, bufStart + (minX + minY * frameWidth), (uint)bboxWidth * sizeof(int));
            // Unsafe.CopyBlock(zStart, zBufStart + (minX + minY * frameWidth), (uint)bboxWidth * sizeof(float));

            // for (int i = bboxWidth; i > 0; i -= Vector<int>.Count)
            // i = 0;
            for (x = maxX; x > minX; x -= Vector<int>.Count)
            {
                int vecStart = x - Vector<int>.Count + row;
                int vecFloatStart = x - Vector<float>.Count + row;
                
                int* colStart = bufStart + vecStart;
                float* zStart = zBufStart + vecFloatStart;



                Vector<int> bufVec = Vector.Load(colStart);
                Vector<float> bufVecZ = Vector.Load(zStart);


                edges.IsInside(x, y, out Vector3Wide eN);

                RasterHelpers.InterpolateBarycentric(oldZ.X, oldZ.Y, oldZ.Z, eN, out Vector<float> bigDepth);
                Vector<int> depthMask = Vector.LessThanOrEqual(bigDepth, bufVecZ);


                Vector<int> mask =
                    Vector.GreaterThanOrEqual(eN.X, Vector<float>.Zero) &
                    Vector.GreaterThanOrEqual(eN.Y, Vector<float>.Zero) &
                    Vector.GreaterThanOrEqual(eN.Z, Vector<float>.Zero);

                Vector<int> colResult = Vector.ConditionalSelect(mask & depthMask, wideCol, bufVec);
                Vector<float> zResult = Vector.ConditionalSelect(mask & depthMask, bigDepth, bufVecZ);
                
                *(Vector<int>*)colStart = colResult;
                *(Vector<float>*)zStart = zResult;
                // Vector.StoreUnsafe(maskedCol, ref *colOffset);
                // Vector.StoreUnsafe(maskedZ, ref *zOffset);

                // i++;
            }

            // Unsafe.CopyBlock(bufStart + (minX + minY * frameWidth), colStart, (uint)bboxWidth * sizeof(int));
            // Unsafe.CopyBlock(zBufStart + (minX + minY * frameWidth), colStart, (uint)bboxWidth * sizeof(float));
            // Console.WriteLine($"Rounded: {maxX - x}");
            // Console.WriteLine($"Width: {Math.Ceiling(FrameBufferSize.WidthSingle / Vector<int>.Count) * Vector<int>.Count}");
            // rowSlice.CopyTo(rowBuf);
        }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void DrawPinedaTriangleBundle(ref TriangleWide triBundle, in Span<int> bufSlice, in Span<float> zBufSlicem, in Vector3Wide oldZ)
    {

        QuickColor col = new(0, 255, 0, 255);
        QuickColor boundsCol = new(255, 127, 0, 255);
        Vector<int> wideCol = new(boundsCol.RGBA);
        TriangleWide.GetTotalBounds(triBundle, out Vector3 bboxMin, out Vector3 bboxMax);
        EdgesBundled edges = new(triBundle);


        int minX = Math.Clamp((int)bboxMin.X, 0, FrameBufferSize.Width - 1);
        int minY = Math.Clamp((int)bboxMin.Y, 0, FrameBufferSize.Height - 1);
        int maxX = Math.Clamp((int)bboxMax.X, 0, FrameBufferSize.Width - 1);
        int maxY = Math.Clamp((int)bboxMax.Y, 0, FrameBufferSize.Height - 1);
        

        DrawBounds(new(minX, minY, 0), new(maxX, maxY, 0), col.RGBA);
        int width = FrameBufferSize.Width;

        for (int x = maxX - 1; x > minX; x--)
        {
            for (int y = maxY - 1; y > minY; y--)
            {
                edges.IsInside(x, y, out Vector3Wide eN, out _);


                int vecStart = x + y * width;
                // int vecStart = x - Vector<int>.Count + y * width;
                // int vecFloatStart = x - Vector<float>.Count + y * width;
                // ref int first = ref bufSlice[vecStart];
                // ref float firstZ = ref zBufSlice[vecFloatStart];

                // Vector<int> bufVec = Vector.LoadUnsafe(ref first);
                // Vector<float> bufVecZ = Vector.LoadUnsafe(ref firstZ);

                // VectorHelpers.InterpolateBarycentric(Red, Green, Blue, eN, out BigVec3 bigCol);
                // VectorHelpers.InterpolateBarycentric(tri.uv1, tri.uv2, tri.uv3, eN, out BigVec2 bigUV);
                // RasterHelpers.InterpolateBarycentric(oldZ.X, oldZ.Y, oldZ.Z, eN, out Vector<float> bigDepth);


                // bigUV *= 255f;
                // bigUV.ConvertToInt32(out var RWide, out var GWide);
                // Vector<int> wideInterp = FullByte << 24 | zeroInt << 16 | GWide << 8 | RWide;


                // var depthMask = Vector.LessThanOrEqual(bigDepth, bufVecZ);


                var mask =
                    Vector.GreaterThanOrEqualAny(eN.X, Vector<float>.Zero) &&
                    Vector.GreaterThanOrEqualAny(eN.Y, Vector<float>.Zero) &&
                    Vector.GreaterThanOrEqualAny(eN.Z, Vector<float>.Zero);


                // Vector<int> maskedCol = Vector.ConditionalSelect(mask, wideCol, bufVec);
                // Vector<float> maskedZ = Vector.ConditionalSelect(mask & depthMask, bigDepth, bufVecZ);
                // maskedCol.StoreUnsafe(ref first);
                // maskedZ.StoreUnsafe(ref firstZ);
                bufSlice[vecStart] = mask ? col.RGBA : 0;
                // FillRect(new(x - Vector<int>.Count, y - 1), new(x, y), x / Vector<int>.Count % 2 == 0 ? new QuickColor(128, 0, 0, 255).RGBA : new QuickColor(0, 128, 0, 255).RGBA);
            }

            // rowSlice.CopyTo(rowBuf);
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
