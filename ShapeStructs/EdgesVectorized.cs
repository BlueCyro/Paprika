using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using BepuUtilities;


namespace Paprika;


public readonly ref struct EdgesVectorized(in Vector3 a, in Vector3 b, in Vector3 c)
{
    public static readonly Vector<float> Zero = new();
    public static readonly Vector<float> One = new(1f);
    public static readonly Vector<float> Row = new(Enumerable.Range(0, Vector<float>.Count).Select(i => (float)i).Reverse().ToArray());



    public readonly Vector<float> A1 = new(b.Y - c.Y);
    public readonly Vector<float> B1 = new(c.X - b.X);
    public readonly Vector<float> C1 = new(b.X * c.Y - c.X * b.Y);



    public readonly Vector<float> A2 = new(c.Y - a.Y);
    public readonly Vector<float> B2 = new(a.X - c.X);
    public readonly Vector<float> C2 = new(c.X * a.Y - a.X * c.Y);



    public readonly Vector<float> A3 = new(a.Y - b.Y);
    public readonly Vector<float> B3 = new(b.X - a.X);
    public readonly Vector<float> C3 = new(a.X * b.Y - b.X * a.Y);



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void IsInside(in int startX, in int startY, out Vector3Wide eN, out Vector<float> magnitude)
    {
        var startXV = (One * startX) - Row;
        // var startYV = One * startY;


        eN.X = A1 * startXV + B1 * startY + C1;
        eN.Y = A2 * startXV + B2 * startY + C2;
        eN.Z = A3 * startXV + B3 * startY + C3;


        magnitude = eN.X + eN.Y + eN.Z;
        eN /= magnitude;
    }
}




// public readonly struct VectorInitializer<T> where T: IEquatable<Vector<T>>, IEquatable<Vector64<T>>, IEquatable<Vector128<T>>, IEquatable<Vector256<T>>, IEquatable<Vector512<T>>
// {
//     public static readonly T Value;

//     static VectorInitializer()
//     {
//         switch (typeof(T))
//         {
//             case var v64:
            
//                 break;
//         }
//     }
// }