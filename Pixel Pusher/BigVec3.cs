using System.Numerics;
using System.Runtime.CompilerServices;


namespace Paprika;

// public readonly ref struct Linear2DReader<T>(ref T[] array, in int start, in int width, in int height) where T: unmanaged
// {
//     public readonly int Width = width;
//     public readonly int Height = height;

//     readonly ref T[] arrayRef = ref array;


// }



public struct BigVec3(in Vector<float> x, in Vector<float> y, in Vector<float> z)
{
    public Vector<float> X = x;
    public Vector<float> Y = y;
    public Vector<float> Z = z;



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadElement(in int index, out Vector3 instance)
    {
        var offset = Unsafe.As<float, BigVec3>(ref Unsafe.Add(ref Unsafe.As<BigVec3, float>(ref this), index));
        
        instance.X = offset.X[0];
        instance.Y = offset.Y[0];
        instance.Z = offset.Z[0];
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ConvertToInt32(out Vector<int> xInt, out Vector<int> yInt, out Vector<int> zInt)
    {
        xInt = Vector.ConvertToInt32(X);
        yInt = Vector.ConvertToInt32(Y);
        zInt = Vector.ConvertToInt32(Z);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigVec3 operator *(BigVec3 left, Vector<float> right)
    {
        return left with
        {
            X = left.X * right,
            Y = left.Y * right,
            Z = left.Z * right
        };
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigVec3 operator *(BigVec3 left, float right)
    {
        return left with
        {
            X = left.X * right,
            Y = left.Y * right,
            Z = left.Z * right
        };
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigVec3 operator +(BigVec3 left, BigVec3 right)
    {
        return left with
        {
            X = left.X + right.X,
            Y = left.Y + right.Y,
            Z = left.Z * right.Z
        };
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigVec3 operator /(BigVec3 left, Vector<float> right)
    {
        return left with
        {
            X = left.X / right,
            Y = left.Y / right,
            Z = left.Z / right
        };
    }
}
