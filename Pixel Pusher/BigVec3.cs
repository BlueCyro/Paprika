using System.Numerics;
using System.Runtime.CompilerServices;


namespace Paprika;


// public struct BigVec3
// {
//     public Vector<float> X;
//     public Vector<float> Y;
//     public Vector<float> Z;



//     public BigVec3(in Vector<float> x, in Vector<float> y, in Vector<float> z)
//     {
//         X = x;
//         Y = y;
//         Z = z;
//     }



//     public BigVec3(in Vector<float> toWiden)
//     {
//         X = toWiden;
//         Y = toWiden;
//         Z = toWiden;
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public void ReadElement(in int index, out Vector3 instance)
//     {
//         var offset = Unsafe.As<float, BigVec3>(ref Unsafe.Add(ref Unsafe.As<BigVec3, float>(ref this), index));
        
//         instance.X = offset.X[0];
//         instance.Y = offset.Y[0];
//         instance.Z = offset.Z[0];
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public readonly void ConvertToInt32(out Vector<int> xInt, out Vector<int> yInt, out Vector<int> zInt)
//     {
//         xInt = Vector.ConvertToInt32(X);
//         yInt = Vector.ConvertToInt32(Y);
//         zInt = Vector.ConvertToInt32(Z);
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public void WidenFrom(in Vector3 vector)
//     {
//         X = new(vector.X);
//         Y = new(vector.Y);
//         Z = new(vector.Z);
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static BigVec3 operator *(BigVec3 left, Vector<float> right)
//     {
//         return left with
//         {
//             X = left.X * right,
//             Y = left.Y * right,
//             Z = left.Z * right
//         };
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static BigVec3 operator *(BigVec3 left, float right)
//     {
//         return left with
//         {
//             X = left.X * right,
//             Y = left.Y * right,
//             Z = left.Z * right
//         };
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static BigVec3 operator +(BigVec3 left, BigVec3 right)
//     {
//         return left with
//         {
//             X = left.X + right.X,
//             Y = left.Y + right.Y,
//             Z = left.Z + right.Z
//         };
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static BigVec3 operator -(BigVec3 left, BigVec3 right)
//     {
//         return left with
//         {
//             X = left.X - right.X,
//             Y = left.Y - right.Y,
//             Z = left.Z - right.Z
//         };
//     }


//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static BigVec3 operator /(BigVec3 left, Vector<float> right)
//     {
//         return left with
//         {
//             X = left.X / right,
//             Y = left.Y / right,
//             Z = left.Z / right
//         };
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static BigVec3 operator /(BigVec3 left, float right)
//     {
//         return left with
//         {
//             X = left.X / right,
//             Y = left.Y / right,
//             Z = left.Z / right
//         };
//     }



//     public readonly Vector<float> Magnitude
//     {
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]    
//         get => Vector.SquareRoot(SqrMagnitude);
//     }


//     public readonly Vector<float> SqrMagnitude
//     {
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         get => X * X + Y * Y + Z * Z;
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static void Min(in BigVec3 a, in BigVec3 b, out BigVec3 result)
//     {
//         result.X = Vector.Min(a.X, b.X);
//         result.Y = Vector.Min(a.Y, b.Y);
//         result.Z = Vector.Min(a.Z, b.Z);
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static void Max(in BigVec3 a, in BigVec3 b, out BigVec3 result)
//     {
//         result.X = Vector.Max(a.X, b.X);
//         result.Y = Vector.Max(a.Y, b.Y);
//         result.Z = Vector.Max(a.Z, b.Z);
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static void Normalize(in BigVec3 target, out BigVec3 normalized)
//     {
//         normalized = target / target.Magnitude;
//     }


    
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static void Cross(in BigVec3 a, in BigVec3 b, out BigVec3 cross)
//     {
//         cross.X = a.Y * b.Z - a.Z * b.Y;
//         cross.Y = a.Z * b.X - a.X * b.Z;
//         cross.Z = a.X * b.Y - a.Y * b.X;
//     }
// }
