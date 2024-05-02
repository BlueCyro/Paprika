using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BepuUtilities;


namespace Paprika;


[StructLayout(LayoutKind.Sequential, Pack = 32)]
public readonly ref struct EdgesVectorized(in Vector3 a, in Vector3 b, in Vector3 c)
{
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
    public readonly void IsInside(in int startX, in int startY, out Vector3Wide eN)
    {
        // Unsafe.SkipInit(out eN);
        Vector<float> startXV = new Vector<float>(startX) - Row;
        Vector<float> startYV = new Vector<float>(startY);

        if (Fma.IsSupported && Vector<float>.Count == 8)
        {
            eN.X = Fma.MultiplyAdd(A1.AsVector256(), startXV.AsVector256(), Fma.MultiplyAdd(B1.AsVector256(), startYV.AsVector256(), C1.AsVector256())).AsVector();
            eN.Y = Fma.MultiplyAdd(A2.AsVector256(), startXV.AsVector256(), Fma.MultiplyAdd(B2.AsVector256(), startYV.AsVector256(), C2.AsVector256())).AsVector();
            eN.Z = Fma.MultiplyAdd(A3.AsVector256(), startXV.AsVector256(), Fma.MultiplyAdd(B3.AsVector256(), startYV.AsVector256(), C3.AsVector256())).AsVector();
        }
        else if (Fma.IsSupported && Vector<float>.Count == 4)
        {
            eN.X = Fma.MultiplyAdd(A1.AsVector128(), startXV.AsVector128(), Fma.MultiplyAdd(B1.AsVector128(), startYV.AsVector128(), C1.AsVector128())).AsVector();
            eN.Y = Fma.MultiplyAdd(A2.AsVector128(), startXV.AsVector128(), Fma.MultiplyAdd(B2.AsVector128(), startYV.AsVector128(), C2.AsVector128())).AsVector();
            eN.Z = Fma.MultiplyAdd(A3.AsVector128(), startXV.AsVector128(), Fma.MultiplyAdd(B3.AsVector128(), startYV.AsVector128(), C3.AsVector128())).AsVector();
        }
        else
        {
            eN.X = A1 * startXV + B1 * startY + C1;
            eN.Y = A2 * startXV + B2 * startY + C2;
            eN.Z = A3 * startXV + B3 * startY + C3;
        }


        Vector3Wide.Scale(eN, MathHelper.FastReciprocal(eN.X + eN.Y + eN.Z), out eN);
    }
}




public readonly ref struct EdgesBundled(in TriangleWide triangle)
{
    public static readonly Vector<float> Row = new(Enumerable.Range(0, Vector<float>.Count).Select(i => (float)i).Reverse().ToArray());



    public readonly Vector<float> A1 = triangle.B.Y - triangle.C.Y;
    public readonly Vector<float> B1 = triangle.C.X - triangle.B.X;
    public readonly Vector<float> C1 = triangle.B.X * triangle.C.Y - triangle.C.X * triangle.B.Y;



    public readonly Vector<float> A2 = triangle.C.Y - triangle.A.Y;
    public readonly Vector<float> B2 = triangle.A.X - triangle.C.X;
    public readonly Vector<float> C2 = triangle.C.X * triangle.A.Y - triangle.A.X * triangle.C.Y;



    public readonly Vector<float> A3 = triangle.A.Y - triangle.B.Y;
    public readonly Vector<float> B3 = triangle.B.X - triangle.A.X;
    public readonly Vector<float> C3 = triangle.A.X * triangle.B.Y - triangle.B.X * triangle.A.Y;



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void IsInside(in int startX, in int startY, out Vector3Wide eN, out Vector<float> magnitude)
    {
        //Vector<float> startXV = (Vector<float>.One * startX) - Row;
        // var startYV = One * startY;


        eN.X = A1 * startX + B1 * startY + C1;
        eN.Y = A2 * startX + B2 * startY + C2;
        eN.Z = A3 * startX + B3 * startY + C3;


        magnitude = eN.X + eN.Y + eN.Z;
        eN /= magnitude;
    }
}