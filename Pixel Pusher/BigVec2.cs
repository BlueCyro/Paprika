using System.Numerics;
using System.Runtime.CompilerServices;


namespace Paprika;

public struct BigVec2(in Vector<float> x, in Vector<float> y)
{
    public Vector<float> X = x;
    public Vector<float> Y = y;



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadElement(in int index, out Vector2 instance)
    {
        var offset = Unsafe.As<float, BigVec2>(ref Unsafe.Add(ref Unsafe.As<BigVec2, float>(ref this), index));
        
        instance.X = offset.X[0];
        instance.Y = offset.Y[0];
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ConvertToInt32(out Vector<int> xInt, out Vector<int> yInt)
    {
        xInt = Vector.ConvertToInt32(X);
        yInt = Vector.ConvertToInt32(Y);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigVec2 operator *(BigVec2 left, Vector<float> right)
    {
        return new(
            left.X * right,
            left.Y * right
        );
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigVec2 operator *(BigVec2 left, float right)
    {
        return new(
            left.X * right,
            left.Y * right);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigVec2 operator +(BigVec2 left, BigVec2 right)
    {
        return new(
            left.X + right.X,
            left.Y + right.Y);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigVec2 operator /(BigVec2 left, Vector<float> right)
    {
        return new(
            left.X / right,
            left.Y / right);
    }
}
