using System.Numerics;
using System.Runtime.CompilerServices;
using BepuUtilities;


namespace Paprika;

public static class VectorHelpers
{
    public const float RAD2DEG = 180f / MathF.PI;
    public const float DEG2RAD = MathF.PI / 180f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Swap(in this Vector2 target)
    {
        return new Vector2(target.Y, target.X);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Floor(in this Vector2 target)
    {
        return new Vector2(MathF.Floor(target.X), MathF.Floor(target.Y));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Ceiling(in this Vector2 target)
    {
        return new Vector2(MathF.Ceiling(target.X), MathF.Ceiling(target.Y));
    }



    public static Vector2 Round(in this Vector2 target)
    {
        return new Vector2(MathF.Round(target.X), MathF.Round(target.Y));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sum(in this Vector2 target)
    {
        return target.X + target.Y;
    }



    public static float Difference(in this Vector2 target)
    {
        return target.X - target.Y;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sum(in this Vector3 target)
    {
        return target.X + target.Y + target.Z;
    }


    
    // public static unsafe Span<TTo> GetSpanOfType<TFrom, TTo>(this TFrom target)
    // where TFrom : unmanaged
    // where TTo : unmanaged
    // {
    //     Span<TFrom> span = stackalloc TFrom[1];
    //     return MemoryMarshal.Cast<TFrom, TTo>(span);
    // }



    public static Vector4 AsVector4(this Vector3 target)
    {
        return new(target, 1f);
    }




    public static QuickColor Lerp(in this QuickColor A, in QuickColor B, in float lerp)
    {
        return A * (1 - lerp) + B * lerp;
    }



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
    public static void InterpolateBarycentric(in float one, in float two, in float three, in Vector3Wide bary, out Vector<float> interpolated)
    {
        var oneMul = one * bary.X;
        var twoMul = two * bary.Y;
        var threeMul = three * bary.Z;
        interpolated = oneMul + twoMul + threeMul;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MulWide(in this Vector3 target, in Vector<float> multiplier, out Vector3Wide multiplied)
    {
        
        multiplied.X = new Vector<float>(target.X) * multiplier;
        multiplied.Y = new Vector<float>(target.Y) * multiplier;
        multiplied.Z = new Vector<float>(target.Z) * multiplier;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MulWide(in this Vector2 target, in Vector<float> multiplier, out Vector2Wide multiplied)
    {
        multiplied.X = new Vector<float>(target.X) * multiplier;
        multiplied.Y = new Vector<float>(target.Y) * multiplier;
    }
}