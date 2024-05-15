using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using BepuUtilities;


namespace Paprika;


[StructLayout(LayoutKind.Sequential, Size = 64)]
public struct Triangle : IGeometry  // Field offsets help significantly in aligning the memory for this struct
{                                   // These may be evil. Testing is pending...
    // [FieldOffset(0)]             // They were. FieldOffsets pessimize the JIT - worse codegen quality
    public Vector4 A;

    // [FieldOffset(16)]
    public Vector4 B;

    // [FieldOffset(32)]
    public Vector4 C;



    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        A = Unsafe.As<Vector3, Vector4>(ref a);
        B = Unsafe.As<Vector3, Vector4>(ref b);
        C = Unsafe.As<Vector3, Vector4>(ref c);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void GetBounds(out Vector4 min, out Vector4 max)
    {
        min = Vector4.Min(Vector4.Min(A, B), C);
        max = Vector4.Max(Vector4.Max(A, B), C);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Transform(in Matrix4x4 transform)
    {
        A = Vector4.Transform(A, transform);
        B = Vector4.Transform(B, transform);
        C = Vector4.Transform(C, transform);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZDivide(out Vector3 oldZ)
    {
        oldZ.X = A.Z;
        oldZ.Y = B.Z;
        oldZ.Z = C.Z;


        A /= MathF.Max(A.Z, 0f);
        B /= MathF.Max(B.Z, 0f);
        C /= MathF.Max(C.Z, 0f);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void DrawSIMD( // Draw using Pineda method
        ref int bufStart,
        ref float zBufStart,
        in Vector128<int> bbox,
        in Vector3 oldZ,
        in Size2D bufferResolution,
        int col)
    {
        Vector<int> wideCol = new(col);
        EdgesVectorized edges = new(A, B, C);
        // edges.UpdateEdges(tri.A, tri.B, tri.C);

        int minX = bbox[0];
        int minY = bbox[1];
        int maxX = bbox[2];
        int maxY = bbox[3];

        for (int y = maxY; y > minY; y--)
        {
            int row = y * bufferResolution.Width;
            for (int x = maxX; x > minX; x -= Vector<int>.Count)
            {
                int vecStart = x - Vector<int>.Count + row;
                // int vecFloatStart = x - Vector<float>.Count + row;

                ref int colStart = ref Unsafe.Add(ref bufStart, vecStart);
                ref float zStart = ref Unsafe.Add(ref zBufStart, vecStart);


                Vector<int> bufVec = Vector.LoadUnsafe(ref colStart);
                Vector<float> bufVecZ = Vector.LoadUnsafe(ref zStart);


                edges.IsInside(x, y, out Vector3Wide eN);


                RasterHelpers.InterpolateBarycentric(oldZ.X, oldZ.Y, oldZ.Z, in eN, out Vector<float> bigDepth);
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



    public override readonly string ToString()
    {
        return $"<[[{A}]], [[{B}]], [[{C}]]>";
    }


    // public readonly Vector3 Normal => Vector3.Normalize(Vector3.Cross(B - A, C - A));
    // public readonly Vector3 Center => *(Vector3*)&(A + B + C) / 3f;
}



// [StructLayout(LayoutKind.Sequential, Pack = 128)]
public struct TriangleWide : IGeometry
{
    public Vector3Wide A;
    public Vector3Wide B;
    public Vector3Wide C;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetBounds(in TriangleWide bundle, out Vector3Wide min, out Vector3Wide max)
    {
        // Using min like this rather than using two out parameters yields a shorter method from the JIT
        Vector3Wide.Min(Vector3Wide.Min(bundle.A, bundle.B), bundle.C, out min);

        Vector3Wide.Max(Vector3Wide.Max(bundle.A, bundle.B), bundle.C, out max);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Get2DBounds(in TriangleWide bundle, out Vector4Wide bounds)
    {
        Unsafe.SkipInit(out bounds);
        ref Vector2Wide boundsMin = ref bounds.AsVector2();
        ref Vector2Wide boundsMax = ref Unsafe.Add(ref bounds.AsVector2(), 1);

        // Using min/max like this rather than using two out parameters yields a shorter method from the JIT
        VectorHelpers.Min(VectorHelpers.Min(bundle.A.AsVector2(), bundle.B.AsVector2()), bundle.C.AsVector2(), out boundsMin);

        VectorHelpers.Max(VectorHelpers.Max(bundle.A.AsVector2(), bundle.B.AsVector2()), bundle.C.AsVector2(), out boundsMax);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Get2DBounds(in TriangleWide bundle, out Int4Wide bounds)
    {
        Unsafe.SkipInit(out Vector4Wide boundsSingle);
        ref Vector2Wide boundsMin = ref boundsSingle.AsVector2();
        ref Vector2Wide boundsMax = ref Unsafe.Add(ref boundsSingle.AsVector2(), 1);

        // Using min/max like this rather than using two out parameters yields a shorter method from the JIT
        VectorHelpers.Min(VectorHelpers.Min(bundle.A.AsVector2(), bundle.B.AsVector2()), bundle.C.AsVector2(), out boundsMin);

        VectorHelpers.Max(VectorHelpers.Max(bundle.A.AsVector2(), bundle.B.AsVector2()), bundle.C.AsVector2(), out boundsMax);

        boundsSingle.ConvertToInt32(out bounds);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int4Wide Get2DBounds(in TriangleWide bundle)
    {
        Unsafe.SkipInit(out Vector4Wide boundsSingle);
        ref Vector2Wide boundsMin = ref boundsSingle.AsVector2();
        ref Vector2Wide boundsMax = ref Unsafe.Add(ref boundsSingle.AsVector2(), 1);

        // Using min/max like this rather than using two out parameters yields a shorter method from the JIT
        VectorHelpers.Min(VectorHelpers.Min(bundle.A.AsVector2(), bundle.B.AsVector2()), bundle.C.AsVector2(), out boundsMin);

        VectorHelpers.Max(VectorHelpers.Max(bundle.A.AsVector2(), bundle.B.AsVector2()), bundle.C.AsVector2(), out boundsMax);

        boundsSingle.ConvertToInt32(out Int4Wide bounds);

        return bounds;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetTotalBounds(in TriangleWide bundle, out Vector3 min, out Vector3 max)
    {
        GetBounds(bundle, out Vector3Wide minWide, out Vector3Wide maxWide);

        minWide.X.Min(out min.X);
        minWide.Y.Min(out min.Y);
        minWide.Z.Min(out min.Z);


        maxWide.X.Max(out max.X);
        maxWide.Y.Max(out max.Y);
        maxWide.Z.Max(out max.Z);
        
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZDivide(in TriangleWide bundle, out Vector3Wide oldZ, out TriangleWide divided)
    {
        oldZ.X = bundle.A.Z;
        oldZ.Y = bundle.B.Z;
        oldZ.Z = bundle.C.Z;

        // The scale function here appears to yield a shorter method than the direct divisions from the JIT
        Vector3Wide.Scale(bundle.A, MathHelper.FastReciprocal(Vector.Max(bundle.A.Z, Vector<float>.Zero)), out divided.A);
        Vector3Wide.Scale(bundle.B, MathHelper.FastReciprocal(Vector.Max(bundle.B.Z, Vector<float>.Zero)), out divided.B);
        Vector3Wide.Scale(bundle.C, MathHelper.FastReciprocal(Vector.Max(bundle.C.Z, Vector<float>.Zero)), out divided.C);

        // divided.A = bundle.A / Vector.Max(bundle.A.Z, Vector<float>.Zero);
        // divided.B = bundle.B / Vector.Max(bundle.B.Z, Vector<float>.Zero);
        // divided.C = bundle.C / Vector.Max(bundle.C.Z, Vector<float>.Zero);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TriangleWide ZDivide(in TriangleWide bundle, out Vector3Wide oldZ)
    {
        oldZ.X = bundle.A.Z;
        oldZ.Y = bundle.B.Z;
        oldZ.Z = bundle.C.Z;

        Unsafe.SkipInit(out TriangleWide divided);

        // The scale function here appears to yield a shorter method than the direct divisions from the JIT
        Vector3Wide.Scale(bundle.A, MathHelper.FastReciprocal(Vector.Max(bundle.A.Z, Vector<float>.Zero)), out divided.A);
        Vector3Wide.Scale(bundle.B, MathHelper.FastReciprocal(Vector.Max(bundle.B.Z, Vector<float>.Zero)), out divided.B);
        Vector3Wide.Scale(bundle.C, MathHelper.FastReciprocal(Vector.Max(bundle.C.Z, Vector<float>.Zero)), out divided.C);

        // divided.A = bundle.A / Vector.Max(bundle.A.Z, Vector<float>.Zero);
        // divided.B = bundle.B / Vector.Max(bundle.B.Z, Vector<float>.Zero);
        // divided.C = bundle.C / Vector.Max(bundle.C.Z, Vector<float>.Zero);

        return divided;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Transform(in TriangleWide bundle, in Matrix4x4Wide transform, out TriangleWide transformed)
    {
        Matrix4x4Wide.Transform(bundle.A, transform, out transformed.A);
        Matrix4x4Wide.Transform(bundle.B, transform, out transformed.B);
        Matrix4x4Wide.Transform(bundle.C, transform, out transformed.C);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TriangleWide Transform(in TriangleWide bundle, in Matrix4x4Wide transform)
    {
        Unsafe.SkipInit(out TriangleWide transformed);
        
        Matrix4x4Wide.Transform(bundle.A, transform, out transformed.A);
        Matrix4x4Wide.Transform(bundle.B, transform, out transformed.B);
        Matrix4x4Wide.Transform(bundle.C, transform, out transformed.C);

        return transformed;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Broadcast(ref Triangle triangle, out TriangleWide bundle)
    {
        Vector3Wide.Broadcast(Unsafe.As<Vector4, Vector3>(ref triangle.A), out bundle.A);
        Vector3Wide.Broadcast(Unsafe.As<Vector4, Vector3>(ref triangle.B), out bundle.B);
        Vector3Wide.Broadcast(Unsafe.As<Vector4, Vector3>(ref triangle.C), out bundle.C);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadSlot(ref TriangleWide bundle, int index, out Triangle narrow)
    {
        ReadFirst(GatherScatter.GetOffsetInstance(ref bundle, index), out narrow);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteSlot(ref Triangle narrow, int index, ref TriangleWide wide)
    {
        Vector3Wide.WriteSlot(Unsafe.As<Vector4, Vector3>(ref narrow.A), index, ref wide.A);
        Vector3Wide.WriteSlot(Unsafe.As<Vector4, Vector3>(ref narrow.B), index, ref wide.B);
        Vector3Wide.WriteSlot(Unsafe.As<Vector4, Vector3>(ref narrow.C), index, ref wide.C);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadFirst(in TriangleWide bundle, out Triangle narrow)
    {
        // The calls to Vector3Wide.ReadFirst appear to not be inlined, and I currently can't figure out why
        Unsafe.SkipInit(out narrow);
        narrow.A.X = bundle.A.X[0];
        narrow.A.Y = bundle.A.Y[0];
        narrow.A.Z = bundle.A.Z[0];

        narrow.B.X = bundle.B.X[0];
        narrow.B.Y = bundle.B.Y[0];
        narrow.B.Z = bundle.B.Z[0];

        narrow.C.X = bundle.C.X[0];
        narrow.C.Y = bundle.C.Y[0];
        narrow.C.Z = bundle.C.Z[0];
        // Vector3Wide.ReadFirst(bundle.A, out narrow.A);
        // Vector3Wide.ReadFirst(bundle.B, out narrow.B);
        // Vector3Wide.ReadFirst(bundle.C, out narrow.C);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetCenter(in TriangleWide bundle, out Vector3Wide center)
    {
        Vector3Wide.Scale(bundle.A + bundle.B + bundle.C, VectorHelpers.OneThird, out center);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetNormal(in TriangleWide bundle, out Vector3Wide normal)
    {
        Vector3Wide.Cross(bundle.B - bundle.A, bundle.C - bundle.A, out normal);

        // Using FastReciprocalSquareRoot yields a very slightly shorter method and utilizes the proper
        // instruction set where applicable
        Vector3Wide.Scale(normal, MathHelper.FastReciprocalSquareRoot(normal.LengthSquared()), out normal);
    }


    
    public override readonly string ToString()
    {
        return $"<[[{A.X}]], [[{A.Y}]], [[{A.Z}]]>";
    }
}
