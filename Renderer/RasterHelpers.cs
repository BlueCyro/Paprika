using System.Numerics;
using System.Runtime.CompilerServices;
using BepuUtilities;


namespace Paprika;

public static class RasterHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InterpolateBarycentric(in Vector3 one, in Vector3 two, in Vector3 three, in Vector3Wide bary, out Vector3Wide interpolated)
    {
        one.MulWide(bary.X, out Vector3Wide oneMul);
        two.MulWide(bary.Y, out Vector3Wide twoMul);
        three.MulWide(bary.Z, out Vector3Wide threeMul);
        interpolated = oneMul + twoMul + threeMul;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InterpolateBarycentric(in Vector2 one, in Vector2 two, in Vector2 three, in Vector3Wide bary, out Vector2Wide interpolated)
    {
        one.MulWide(bary.X, out Vector2Wide oneMul);
        two.MulWide(bary.Y, out Vector2Wide twoMul);
        three.MulWide(bary.Z, out Vector2Wide threeMul);
        interpolated = oneMul + twoMul + threeMul;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InterpolateBarycentric(float one, float two, float three, in Vector3Wide bary, out Vector<float> interpolated)
    {
        var oneMul = one * bary.X;
        var twoMul = two * bary.Y;
        var threeMul = three * bary.Z;
        interpolated = oneMul + twoMul + threeMul;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InterpolateBarycentric(in Vector<float> one, in Vector<float> two, in Vector<float> three, in Vector3Wide bary, out Vector<float> interpolated)
    {
        var oneMul = one * bary.X;
        var twoMul = two * bary.Y;
        var threeMul = three * bary.Z;
        interpolated = oneMul + twoMul + threeMul;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InterpolateBarycentric(in Vector2Wide one, in Vector2Wide two, in Vector2Wide three, in Vector3Wide bary, out Vector2Wide interpolated)
    {
        var oneMul = one * bary.X;
        var twoMul = two * bary.Y;
        var threeMul = three * bary.Z;
        interpolated = oneMul + twoMul + threeMul;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InterpolateBarycentric(in Vector3Wide one, in Vector3Wide two, in Vector3Wide three, in Vector3Wide bary, out Vector3Wide interpolated)
    {
        var oneMul = one * bary.X;
        var twoMul = two * bary.Y;
        var threeMul = three * bary.Z;
        interpolated = oneMul + twoMul + threeMul;
    }
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static void InterpolateBarycentric(in Vector4Wide one, in Vector4Wide two, in Vector4Wide three, in Vector3Wide bary, out Vector4Wide interpolated)
    // {
    //     var oneMul = one * bary.X;
    //     var twoMul = two * bary.Y;
    //     var threeMul = three * bary.Z;
    //     interpolated = oneMul + twoMul + threeMul;
    // }
}