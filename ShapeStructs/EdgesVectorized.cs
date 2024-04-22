using System.Numerics;
using System.Runtime.CompilerServices;


namespace Paprika;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct EdgesVectorized(in Vector2 p1, in Vector2 p2, in Vector2 p3)
{
    public static readonly Vector<float> Zero = new();
    public static readonly Vector<float> One = new(1f);
    public static readonly Vector<float> Row = new(Enumerable.Range(0, Vector<float>.Count).Select(i => (float)i).Reverse().ToArray());



    public readonly Vector<float> A1 = new(p2.Y - p3.Y);
    public readonly Vector<float> B1 = new(p3.X - p2.X);
    public readonly Vector<float> C1 = new(p2.X * p3.Y - p3.X * p2.Y);



    public readonly Vector<float> A2 = new(p3.Y - p1.Y);
    public readonly Vector<float> B2 = new(p1.X - p3.X);
    public readonly Vector<float> C2 = new(p3.X * p1.Y - p1.X * p3.Y);



    public readonly Vector<float> A3 = new(p1.Y - p2.Y);
    public readonly Vector<float> B3 = new(p2.X - p1.X);
    public readonly Vector<float> C3 = new(p1.X * p2.Y - p2.X * p1.Y);



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void IsInside(in int startX, in int startY, out BigVec3 eN, out Vector<float> magnitude)
    {
        var startXV = (One * startX) - Row;
        var startYV = One * startY;


        eN.X = A1 * startXV + B1 * startYV + C1;
        eN.Y = A2 * startXV + B2 * startYV + C2;
        eN.Z = A3 * startXV + B3 * startYV + C3;

        magnitude = eN.X + eN.Y + eN.Z;
        eN /= magnitude;
    }
}
