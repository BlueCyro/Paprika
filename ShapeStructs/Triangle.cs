using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using BepuUtilities;


namespace Paprika;


[StructLayout(LayoutKind.Explicit)]
public struct Triangle // Field offsets help significantly in aligning the memory for this struct
{
    [FieldOffset(0)]
    public Vector3 A;

    [FieldOffset(16)]
    public Vector3 B;

    [FieldOffset(32)]
    public Vector3 C;



    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        A = a;
        B = b;
        C = c;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void GetBounds(out Vector3 min, out Vector3 max)
    {
        min = Vector3.Min(Vector3.Min(A, B), C);
        max = Vector3.Max(Vector3.Max(A, B), C);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Transform(in Matrix4x4 transform)
    {
        A = Vector3.Transform(A, transform);
        B = Vector3.Transform(B, transform);
        C = Vector3.Transform(C, transform);
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


    public override readonly string ToString()
    {
        return $"<[[{A}]], [[{B}]], [[{C}]]>";
    }


    
    public readonly Vector3 Normal => Vector3.Normalize(Vector3.Cross(B - A, C - A));
    public readonly Vector3 Center => (A + B + C) / 3f;
}


[StructLayout(LayoutKind.Sequential, Pack = 32)]
public struct TriangleWide
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

        // Using min like this rather than using two out parameters yields a shorter method from the JIT
        VectorHelpers.Min(VectorHelpers.Min(bundle.A.AsVector2(), bundle.B.AsVector2()), bundle.C.AsVector2(), out boundsMin);

        VectorHelpers.Max(VectorHelpers.Max(bundle.A.AsVector2(), bundle.B.AsVector2()), bundle.C.AsVector2(), out boundsMax);
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
    public static void Transform(in TriangleWide bundle, in Matrix4x4Wide transform, out TriangleWide transformed)
    {
        Matrix4x4Wide.Transform(bundle.A, transform, out transformed.A);
        Matrix4x4Wide.Transform(bundle.B, transform, out transformed.B);
        Matrix4x4Wide.Transform(bundle.C, transform, out transformed.C);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Broadcast(in Triangle triangle, out TriangleWide bundle)
    {
        Vector3Wide.Broadcast(triangle.A, out bundle.A);
        Vector3Wide.Broadcast(triangle.B, out bundle.B);
        Vector3Wide.Broadcast(triangle.C, out bundle.C);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadSlot(ref TriangleWide bundle, in int index, out Triangle narrow)
    {
        ReadFirst(GatherScatter.GetOffsetInstance(ref bundle, index), out narrow);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteSlot(in Triangle narrow, in int index, ref TriangleWide wide)
    {
        Vector3Wide.WriteSlot(narrow.A, index, ref wide.A);
        Vector3Wide.WriteSlot(narrow.B, index, ref wide.B);
        Vector3Wide.WriteSlot(narrow.C, index, ref wide.C);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadFirst(in TriangleWide bundle, out Triangle narrow)
    {
        // The calls to Vector3Wide.ReadFirst appear to not be inlined, and I currently can't figure out why

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