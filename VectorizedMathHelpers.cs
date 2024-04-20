using System.Numerics;
using System.Runtime.CompilerServices;


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



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetTriBounds(in Triangle tri, in Vector2 min, in Vector2 max, out Vector2 bboxMin, out Vector2 bboxMax)
    {
        bboxMin = Vector2.Clamp(Vector2.Min(Vector2.Min(tri.v1, tri.v2), tri.v3), min, max);
        bboxMax = Vector2.Clamp(Vector2.Max(Vector2.Max(tri.v1, tri.v2), tri.v3), min, max);
    }



    public static QuickColor Lerp(in this QuickColor A, in QuickColor B, in float lerp)
    {
        return A * (1 - lerp) + B * lerp;
    }



    // public static void InterpolateBarycentric(in )
}