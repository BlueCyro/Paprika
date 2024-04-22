using System.Numerics;
using System.Runtime.CompilerServices;
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



    public void DrawBounds(in Triangle tri, in int col)
    {
        VectorHelpers.GetTriBounds(tri, Vector2.Zero, SizeVec2 - Vector2.One, out var bboxMin, out var bboxMax);
        Bounds2D bounds = new(bboxMin, bboxMax);

        DrawLine(bounds.Min, new(bounds.Min.X, bounds.Max.Y), col);
        DrawLine(bounds.Min, new(bounds.Max.X, bounds.Min.Y), col);
        DrawLine(bounds.Max, new(bounds.Min.X, bounds.Max.Y), col);
        DrawLine(bounds.Max, new(bounds.Max.X, bounds.Min.Y), col);
    }



    public unsafe void DrawPinedaTriangle(in Triangle tri, in int col) // Slow, but reliable
    {
        VectorHelpers.GetTriBounds(tri, Vector2.Zero, SizeVec2 - Vector2.One, out var bboxMin, out var bboxMax);
        Bounds2D triBounds = new(bboxMin, bboxMax);
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


    public static readonly Vector<int> fullByte = new(255);
    public static readonly Vector<int> zeroInt = new();
    public static readonly int allBits = new QuickColor(255, 255, 255, 255).RGBA;
    public static readonly Vector3 Red = new(1f, 0f, 0f);
    public static readonly Vector3 Green = new(0f, 1f, 0f);
    public static readonly Vector3 Blue = new(0f, 0f, 1f);


    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPinedaTriangleSIMD(in Triangle tri, in int col, in Span<int> bufSlice, in Span<float> zBufSlice)
    {
        Vector<int> wideCol = new(col);
        VectorHelpers.GetTriBounds(tri, Vector2.Zero, SizeVec2 - Vector2.One, out var bboxMin, out var bboxMax);
        Bounds2D triBounds = new(bboxMin, bboxMax);
        EdgesVectorized edges = new(tri.v1, tri.v2, tri.v3);


        int minX = (int)triBounds.Bounds[0];
        int minY = (int)triBounds.Bounds[1];
        int maxX = (int)triBounds.Bounds[2];
        int maxY = (int)triBounds.Bounds[3];
        

        int width = Size[0];
        var zeroVec = EdgesVectorized.Zero;
        // var oneVec = EdgesVectorized.One;
        // Vector<float> magnitude;
        BigVec3 eN;


        // Span<int> rowSlice = stackalloc int[width];
        // Vector<int> interpolatedColors = new();
        
        for (int y = maxY; y > minY; y--)
        {
            for (int x = maxX; x > minX; x -= Vector<int>.Count)
            {
                edges.IsInside(x, y, out eN, out _);


                int vecStart = Math.Max(x - Vector<int>.Count + y * width, 0);
                int vecFloatStart = Math.Max(x - Vector<float>.Count + y * width, 0);
                ref int first = ref bufSlice[vecStart];
                ref float firstZ = ref zBufSlice[vecFloatStart];

                Vector<int> bufVec = Unsafe.As<int, Vector<int>>(ref first);
                Vector<float> bufVecZ = Unsafe.As<float, Vector<float>>(ref firstZ);

                VectorHelpers.InterpolateBarycentric(Red, Green, Blue, eN, out BigVec3 bigCol);
                VectorHelpers.InterpolateBarycentric(tri.uv1, tri.uv2, tri.uv3, eN, out BigVec2 bigUV);
                VectorHelpers.InterpolateBarycentric(tri.p1.Z, tri.p2.Z, tri.p3.Z, eN, out Vector<float> bigDepth);


                bigUV *= 255f;
                bigUV.ConvertToInt32(out var RWide, out var GWide);
                Vector<int> wideInterp = fullByte << 24 | zeroInt << 16 | GWide << 8 | RWide;

                // bigCol *= 255f;
                // bigCol.ConvertToInt32(out var RWide, out var GWide, out var BWide);

                // Vector<int> wideInterp = fullByte << 24 | BWide << 16 | GWide << 8 | RWide;

                var depthMask = Vector.LessThanOrEqual(bigDepth, bufVecZ);

                var mask =
                    Vector.GreaterThanOrEqual(eN.X, zeroVec) &
                    Vector.GreaterThanOrEqual(eN.Y, zeroVec) &
                    Vector.GreaterThanOrEqual(eN.Z, zeroVec);


                // var unsafeCol = Unsafe.As<int, Vector<int>>(ref interpolatedColors);
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



public ref struct StackAllocator<T> where T: new()
{
    public T Instance;
}