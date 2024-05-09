using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using BepuUtilities;


namespace Paprika;


// [StructLayout(LayoutKind.Sequential, Size = 512)]
public struct Int4Wide
{
    public Vector<int> X;
    public Vector<int> Y;
    public Vector<int> Z;
    public Vector<int> W;




    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Broadcast(in Vector128<int> narrow, out Int4Wide bundle)
    {
        bundle.X = new(narrow[0]);
        bundle.Y = new(narrow[1]);
        bundle.Z = new(narrow[2]);
        bundle.W = new(narrow[3]);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Min(in Int4Wide left, in Int4Wide right, out Int4Wide min)
    {
        min.X = Vector.Min(left.X, right.X);
        min.Y = Vector.Min(left.Y, right.Y);
        min.Z = Vector.Min(left.Z, right.Z);
        min.W = Vector.Min(left.W, right.W);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Max(in Int4Wide left, in Int4Wide right, out Int4Wide max)
    {
        max.X = Vector.Max(left.X, right.X);
        max.Y = Vector.Max(left.Y, right.Y);
        max.Z = Vector.Max(left.Z, right.Z);
        max.W = Vector.Max(left.W, right.W);
    }



    public static void Clamp(in Int4Wide value, in Int4Wide min, in Int4Wide max, out Int4Wide clamped)
    {
        clamped.X = Vector.Max(min.X, Vector.Min(value.X, max.X));
        clamped.Y = Vector.Max(min.Y, Vector.Min(value.Y, max.Y));
        clamped.Z = Vector.Max(min.Z, Vector.Min(value.Z, max.Z));
        clamped.W = Vector.Max(min.W, Vector.Min(value.W, max.W));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadSlot(ref Int4Wide bundle, in int index, out Vector128<int> narrow)
    {
        ReadFirst(GatherScatter.GetOffsetInstance(ref bundle, index), out narrow);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadFirst(in Int4Wide bundle, out Vector128<int> narrow)
    {
        Unsafe.SkipInit(out narrow);
        ref int XYZW = ref Unsafe.As<Vector128<int>, int>(ref narrow);

        // Faster than instantiating a new Vector128<int> every time
        // The JIT generates the same ASM method as pointer casting
        XYZW = bundle.X[0];
        Unsafe.Add(ref XYZW, 1) = bundle.Y[0];
        Unsafe.Add(ref XYZW, 2) = bundle.Z[0];
        Unsafe.Add(ref XYZW, 3) = bundle.W[0];
    }
}