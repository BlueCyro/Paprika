using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using BepuUtilities;


namespace Paprika;

public static class VectorHelpers
{
    public const float RAD2DEG = 180f / MathF.PI;
    public const float DEG2RAD = MathF.PI / 180f;
    public static readonly Vector<float> OneHalf = Vector<float>.One / 2f;
    public static readonly Vector<float> OneThird = new(1f / 3f);


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




    // public static QuickColor Lerp(in this QuickColor A, in QuickColor B, in float lerp)
    // {
    //     return A * (1 - lerp) + B * lerp;
    // }



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



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Min<T>(in this Vector<T> target, out T min) where T: struct, INumber<T>
    {
        min = target[0];
        
        for (int i = 0; i < Vector<T>.Count; i++)
        {
            min = T.Min(min, target[i]);
        }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Max<T>(in this Vector<T> target, out T max) where T: struct, INumber<T>
    {
        max = target[0];
        
        for (int i = 0; i < Vector<T>.Count; i++)
        {
            max = T.Max(max, target[i]);
        }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref Vector2Wide AsVector2(in this Vector3Wide target)
    {
        return ref Unsafe.As<Vector3Wide, Vector2Wide>(ref Unsafe.AsRef(in target));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref Vector2Wide AsVector2(in this Vector4Wide target)
    {
        return ref Unsafe.As<Vector4Wide, Vector2Wide>(ref Unsafe.AsRef(in target));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref Vector3Wide AsVector3(in this Vector4Wide target)
    {
        return ref Unsafe.As<Vector4Wide, Vector3Wide>(ref Unsafe.AsRef(in target));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref Vector3Wide AsVector3(in this Vector2Wide target)
    {
        return ref Unsafe.As<Vector2Wide, Vector3Wide>(ref Unsafe.AsRef(in target));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref Vector4Wide AsVector4(in this Vector2Wide target)
    {
        return ref Unsafe.As<Vector2Wide, Vector4Wide>(ref Unsafe.AsRef(in target));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref Vector4Wide AsVector4(in this Vector3Wide target)
    {
        return ref Unsafe.As<Vector3Wide, Vector4Wide>(ref Unsafe.AsRef(in target));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Wide Min(in Vector2Wide left, in Vector2Wide right)
    {
        Unsafe.SkipInit(out Vector2Wide result);
        result.X = Vector.Min(left.X, right.X);
        result.Y = Vector.Min(left.Y, right.Y);
        return result;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Min(in Vector2Wide left, in Vector2Wide right, out Vector2Wide result)
    {
        result.X = Vector.Min(left.X, right.X);
        result.Y = Vector.Min(left.Y, right.Y);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Wide Max(in Vector2Wide left, in Vector2Wide right)
    {
        Unsafe.SkipInit(out Vector2Wide result);
        result.X = Vector.Max(left.X, right.X);
        result.Y = Vector.Max(left.Y, right.Y);
        return result;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Max(in Vector2Wide left, in Vector2Wide right, out Vector2Wide result)
    {
        result.X = Vector.Max(left.X, right.X);
        result.Y = Vector.Max(left.Y, right.Y);
    }


    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ceiling(in this Vector4Wide target, out Vector4Wide result)
    {
        result.X = Vector.Ceiling(target.X);
        result.Y = Vector.Ceiling(target.Y);
        result.Z = Vector.Ceiling(target.Z);
        result.W = Vector.Ceiling(target.W);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Floor(in this Vector4Wide target, out Vector4Wide result)
    {
        result.X = Vector.Floor(target.X);
        result.Y = Vector.Floor(target.Y);
        result.Z = Vector.Floor(target.Z);
        result.W = Vector.Floor(target.W);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AlignBounds(in this Vector4Wide target, out Vector4Wide result)
    {
        result.X = Vector.Floor(target.X);
        result.Y = Vector.Floor(target.Y);
        result.Z = Vector.Ceiling(target.Z);
        result.W = Vector.Ceiling(target.W);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AlignShrinkBounds(in this Vector4Wide target, out Vector4Wide result)
    {
        result.X = Vector.Ceiling(target.X);
        result.Y = Vector.Ceiling(target.Y);
        result.Z = Vector.Floor(target.Z);
        result.W = Vector.Floor(target.W);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Round(in this Vector4Wide target, out Vector4Wide result)
    {
        result.X = Vector.Floor(target.X + OneHalf);
        result.Y = Vector.Floor(target.Y + OneHalf);
        result.Z = Vector.Floor(target.Z + OneHalf);
        result.W = Vector.Floor(target.W + OneHalf);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AlignUpperBounds(in this Vector4Wide target, out Vector4Wide result)
    {
        result.X = target.X;
        result.Y = target.Y;
        result.Z = Vector.Ceiling(target.Z);
        result.W = Vector.Ceiling(target.W);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AlignWidth(in this Vector4Wide target, out Vector4Wide result)
    {
        result.X = Vector.Floor(target.X);
        result.Y = target.Y;
        result.Z = Vector.Ceiling(target.Z);
        result.W = target.W;
    }
}
